# Skill: hubspot-org-config (Core Instructions)

This file is the single source of truth for the `hubspot-org-config` skill. Runtime-specific wrappers can reference this file.

---

## Purpose

Configure a HubSpot organization in a repeatable config-as-code workflow using the Python tool:

`tools/hubspot_org_config.py`

Current scope:
- Upsert CRM properties for HubSpot objects (for example `contacts`, `companies`, `deals`, custom objects).
- Provide a minimal contractor-initiated demo setup/runbook for WindowConfigurator.

---

## When To Invoke

Invoke this skill when the user asks to:
- Set up or standardize HubSpot org metadata
- Apply CRM property definitions from source control
- Dry-run org configuration changes before applying
- Prepare a bare-bones HubSpot demo flow for contractor-initiated quoting

---

## Preconditions

1. Work from the repository root: `WindowConfigurator/`
2. Set auth token in the environment:
   - PowerShell: `$env:HUBSPOT_ACCESS_TOKEN="..."` (HubSpot Private App access token)
3. Prepare a JSON config file following `tools/hubspot_org_config.example.json`

---

## Tool Usage

Dry run:

```powershell
python tools/hubspot_org_config.py --config tools/hubspot_org_config.example.json --dry-run
```

Apply changes:

```powershell
python tools/hubspot_org_config.py --config tools/hubspot_org_config.example.json --apply
```

Custom report output:

```powershell
python tools/hubspot_org_config.py --config my-hubspot-config.json --dry-run --report artifacts/hubspot/report.json
```

Render demo runbook from config (no API calls):

```powershell
python tools/hubspot_org_config.py --config tools/hubspot_org_config.example.json --render-demo-plan
```

---

## Expected Outputs

- Console summary of planned/applied actions
- JSON report file (default: `artifacts/hubspot-org-config-report.json`) containing:
  - `dryRun`
  - summary counts (`planned`, `ok`, `error`)
  - per-property action results
- Markdown demo runbook (default: `artifacts/hubspot-demo-plan.md`) when using `--render-demo-plan`

---

## Minimal HubSpot Configuration For Demo (No Website Flow)

Use this when the goal is to demo the application concept quickly without prospect/website initiation.

1. Create a HubSpot private app token with CRM scopes needed for your chosen integration path.
2. Ensure core properties exist (via `hubspot_org_config.py` config):
   - contact/deal fields for product line and quote session linkage
3. Define one contractor-initiated flow entry:
   - user starts in CRM context
   - create order/session stub
   - launch configurator URL
4. Keep outbound integration generic:
   - CRM calls WindowConfigurator APIs
   - WindowConfigurator emits webhook/event payloads
   - avoid CRM-specific coupling in server design

Important constraint for this demo phase:
- Prospect website-initiated flow is deferred.
- Focus only on contractor-initiated and multi-item/multi-order behavior.

---

## Demo Script: Contractor-Initiated Scenarios

### Scenario A: Multi-item lifecycle in one order
1. Create order/session and add item 1.
2. Configure item 1 and save.
3. Add item 2.
4. Edit item 1 and save again.
5. Edit item 2 and save again.

### Scenario B: Separate order with different product line
1. Create a new order/session.
2. Use a different product line than Scenario A.
3. Add item 1.
4. Configure/edit and save.

This validates:
- repeated edit/save cycles
- multiple items per order
- product-line isolation across separate orders

---

## Rules

- Always run `--dry-run` first unless the user explicitly asks to apply immediately.
- Never hardcode HubSpot credentials; use `HUBSPOT_ACCESS_TOKEN` (Private App access token).
- Keep config declarative. Do not add ad hoc one-off edits when they can be expressed in the config file.
- If apply returns errors, preserve the report artifact and summarize which properties failed and why.

---

## Extension Guidance

When adding new HubSpot config surfaces (for example pipelines, property groups, lifecycle stages):
- Add tests first under `tools/tests/`
- Reuse the same dry-run/apply pattern
- Include all new action results in the report format
- Update this skill doc and the example config file
