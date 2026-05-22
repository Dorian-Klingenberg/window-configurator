#!/usr/bin/env python3
"""
HubSpot Org Config Tool

Applies a declarative subset of HubSpot CRM configuration from JSON:
- custom property upsert for standard/custom objects

Usage:
  python tools/hubspot_org_config.py --config tools/hubspot_org_config.example.json --dry-run
  python tools/hubspot_org_config.py --config my-config.json --apply
  python tools/hubspot_org_config.py --config tools/hubspot_org_config.example.json --render-demo-plan

Auth:
  set HUBSPOT_ACCESS_TOKEN=<private-app-access-token>
"""

from __future__ import annotations

import argparse
import json
import os
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Dict, List, Tuple


API_BASE = "https://api.hubapi.com"


@dataclass
class ActionResult:
    object_type: str
    property_name: str
    action: str
    status: str
    message: str


class HubSpotClient:
    def __init__(self, access_token: str, timeout_seconds: int = 30) -> None:
        self._access_token = access_token
        self._timeout_seconds = timeout_seconds

    def get_property(self, object_type: str, property_name: str) -> Tuple[int, Dict[str, Any] | None]:
        path = f"/crm/v3/properties/{urllib.parse.quote(object_type)}/{urllib.parse.quote(property_name)}"
        return self._request("GET", path, None)

    def create_property(self, object_type: str, payload: Dict[str, Any]) -> Tuple[int, Dict[str, Any] | None]:
        path = f"/crm/v3/properties/{urllib.parse.quote(object_type)}"
        return self._request("POST", path, payload)

    def update_property(self, object_type: str, property_name: str, payload: Dict[str, Any]) -> Tuple[int, Dict[str, Any] | None]:
        path = f"/crm/v3/properties/{urllib.parse.quote(object_type)}/{urllib.parse.quote(property_name)}"
        return self._request("PATCH", path, payload)

    def _request(self, method: str, path: str, body: Dict[str, Any] | None) -> Tuple[int, Dict[str, Any] | None]:
        url = f"{API_BASE}{path}"
        payload = None if body is None else json.dumps(body).encode("utf-8")
        req = urllib.request.Request(url=url, data=payload, method=method)
        req.add_header("Authorization", f"Bearer {self._access_token}")
        req.add_header("Content-Type", "application/json")

        try:
            with urllib.request.urlopen(req, timeout=self._timeout_seconds) as response:
                raw = response.read().decode("utf-8").strip()
                parsed = json.loads(raw) if raw else None
                return response.status, parsed
        except urllib.error.HTTPError as ex:
            raw = ex.read().decode("utf-8").strip()
            parsed = json.loads(raw) if raw else None
            return ex.code, parsed


def load_config(path: Path) -> Dict[str, Any]:
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


def validate_property_spec(spec: Dict[str, Any]) -> List[str]:
    required = ["objectType", "name", "label", "type", "fieldType", "groupName"]
    missing = [k for k in required if not spec.get(k)]
    return [f"Missing required field '{k}' in property spec." for k in missing]


def normalized_update_payload(spec: Dict[str, Any]) -> Dict[str, Any]:
    allowed_keys = {
        "label",
        "description",
        "type",
        "fieldType",
        "groupName",
        "options",
        "hidden",
        "formField",
        "displayOrder",
        "hasUniqueValue",
    }
    return {k: v for k, v in spec.items() if k in allowed_keys}


def apply_properties(
    client: HubSpotClient,
    property_specs: List[Dict[str, Any]],
    dry_run: bool,
    sleep_ms: int,
) -> List[ActionResult]:
    results: List[ActionResult] = []

    for spec in property_specs:
        errors = validate_property_spec(spec)
        object_type = str(spec.get("objectType", "")).strip()
        name = str(spec.get("name", "")).strip()

        if errors:
            results.append(
                ActionResult(object_type, name, "validate", "error", "; ".join(errors))
            )
            continue

        status, current = client.get_property(object_type, name)
        exists = status == 200 and current is not None
        payload = normalized_update_payload(spec)

        if dry_run:
            action = "update" if exists else "create"
            results.append(
                ActionResult(object_type, name, action, "planned", "Dry run only.")
            )
            continue

        if exists:
            status_code, response = client.update_property(object_type, name, payload)
            if 200 <= status_code < 300:
                results.append(ActionResult(object_type, name, "update", "ok", "Updated."))
            else:
                message = extract_error(response) or f"HubSpot update failed ({status_code})."
                results.append(ActionResult(object_type, name, "update", "error", message))
        else:
            create_payload = dict(payload)
            create_payload["name"] = name
            status_code, response = client.create_property(object_type, create_payload)
            if 200 <= status_code < 300:
                results.append(ActionResult(object_type, name, "create", "ok", "Created."))
            else:
                message = extract_error(response) or f"HubSpot create failed ({status_code})."
                results.append(ActionResult(object_type, name, "create", "error", message))

        if sleep_ms > 0:
            time.sleep(sleep_ms / 1000.0)

    return results


def extract_error(response_json: Dict[str, Any] | None) -> str | None:
    if not response_json:
        return None
    # HubSpot commonly returns message and/or errors[]
    message = response_json.get("message")
    if isinstance(message, str) and message.strip():
        return message.strip()
    errors = response_json.get("errors")
    if isinstance(errors, list) and errors:
        first = errors[0]
        if isinstance(first, dict):
            msg = first.get("message")
            if isinstance(msg, str) and msg.strip():
                return msg.strip()
    return None


def write_report(path: Path, results: List[ActionResult], dry_run: bool) -> None:
    payload = {
        "dryRun": dry_run,
        "summary": {
            "planned": sum(1 for r in results if r.status == "planned"),
            "ok": sum(1 for r in results if r.status == "ok"),
            "error": sum(1 for r in results if r.status == "error"),
        },
        "results": [r.__dict__ for r in results],
    }
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2), encoding="utf-8")


def render_demo_plan(config: Dict[str, Any]) -> str:
    demo_plan = config.get("demoPlan", {})
    title = str(demo_plan.get("title", "HubSpot Contractor Demo Plan")).strip()
    checklist = demo_plan.get("hubspotChecklist", [])
    scenarios = demo_plan.get("scenarios", [])

    lines: List[str] = []
    lines.append(f"# {title}")
    lines.append("")
    lines.append("## HubSpot Setup Checklist")
    if isinstance(checklist, list) and checklist:
        for item in checklist:
            lines.append(f"- [ ] {item}")
    else:
        lines.append("- [ ] Add checklist items under demoPlan.hubspotChecklist in the config.")
    lines.append("")

    if isinstance(scenarios, list) and scenarios:
        for index, scenario in enumerate(scenarios, start=1):
            name = str(scenario.get("name", f"Scenario {index}")).strip()
            steps = scenario.get("steps", [])
            lines.append(f"## Scenario {index}: {name}")
            if isinstance(steps, list) and steps:
                for step_index, step in enumerate(steps, start=1):
                    lines.append(f"{step_index}. {step}")
            else:
                lines.append("1. Add steps under this scenario in demoPlan.scenarios[].steps.")
            lines.append("")
    else:
        lines.append("## Scenarios")
        lines.append("1. Add scenarios under demoPlan.scenarios in the config.")
        lines.append("")

    return "\n".join(lines).rstrip() + "\n"


def parse_args(argv: List[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Apply HubSpot org config from JSON.")
    parser.add_argument("--config", required=True, help="Path to JSON config file.")
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--dry-run", action="store_true", help="Plan only; do not modify HubSpot.")
    mode.add_argument("--apply", action="store_true", help="Apply config to HubSpot.")
    mode.add_argument(
        "--render-demo-plan",
        action="store_true",
        help="Render markdown contractor demo runbook from config; no HubSpot API calls.",
    )
    parser.add_argument(
        "--report",
        default="artifacts/hubspot-org-config-report.json",
        help="Output JSON report path.",
    )
    parser.add_argument(
        "--demo-plan-output",
        default="artifacts/hubspot-demo-plan.md",
        help="Output markdown path for --render-demo-plan mode.",
    )
    parser.add_argument(
        "--sleep-ms",
        type=int,
        default=50,
        help="Sleep between API calls (milliseconds).",
    )
    return parser.parse_args(argv)


def main(argv: List[str]) -> int:
    args = parse_args(argv)
    config_path = Path(args.config)
    if not config_path.exists():
        print(f"Config file not found: {config_path}", file=sys.stderr)
        return 2

    config = load_config(config_path)
    property_specs = config.get("properties", [])
    if not isinstance(property_specs, list):
        print("'properties' must be a list.", file=sys.stderr)
        return 2

    if args.render_demo_plan:
        output_path = Path(args.demo_plan_output)
        output_path.parent.mkdir(parents=True, exist_ok=True)
        output_path.write_text(render_demo_plan(config), encoding="utf-8")
        print(f"Demo plan written to: {output_path}")
        return 0

    token = os.getenv("HUBSPOT_ACCESS_TOKEN", "").strip()
    if not token:
        print(
            "Missing HUBSPOT_ACCESS_TOKEN environment variable (use a HubSpot private app access token).",
            file=sys.stderr,
        )
        return 2

    client = HubSpotClient(token)
    results = apply_properties(client, property_specs, dry_run=args.dry_run, sleep_ms=args.sleep_ms)
    write_report(Path(args.report), results, dry_run=args.dry_run)

    errors = [r for r in results if r.status == "error"]
    for r in results:
        print(f"[{r.status.upper():7}] {r.object_type}.{r.property_name} -> {r.action}: {r.message}")

    if errors:
        print(f"\nCompleted with {len(errors)} error(s). See report: {args.report}")
        return 1

    print(f"\nCompleted successfully. See report: {args.report}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
