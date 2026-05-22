import unittest

from tools.hubspot_org_config import apply_properties
from tools.hubspot_org_config import normalized_update_payload
from tools.hubspot_org_config import render_demo_plan
from tools.hubspot_org_config import validate_property_spec


class _FakeHubSpotClient:
    def __init__(self, existing=None):
        self.existing = existing or {}
        self.created = []
        self.updated = []

    def get_property(self, object_type, property_name):
        if (object_type, property_name) in self.existing:
            return 200, self.existing[(object_type, property_name)]
        return 404, None

    def create_property(self, object_type, payload):
        self.created.append((object_type, payload))
        return 201, {"status": "created"}

    def update_property(self, object_type, property_name, payload):
        self.updated.append((object_type, property_name, payload))
        return 200, {"status": "updated"}


class HubSpotOrgConfigTests(unittest.TestCase):
    def test_validate_property_spec_when_missing_required_fields_returns_errors(self):
        errors = validate_property_spec(
            {
                "objectType": "contacts",
                "name": "favorite_install_type",
            }
        )

        self.assertTrue(any("label" in e for e in errors))
        self.assertTrue(any("type" in e for e in errors))
        self.assertTrue(any("fieldType" in e for e in errors))
        self.assertTrue(any("groupName" in e for e in errors))

    def test_normalized_update_payload_only_includes_allowed_fields(self):
        payload = normalized_update_payload(
            {
                "label": "Favorite Install Type",
                "description": "How the customer prefers install",
                "type": "enumeration",
                "fieldType": "select",
                "groupName": "contactinformation",
                "displayOrder": 1,
                "name": "favorite_install_type",
                "objectType": "contacts",
                "ignored": "value",
            }
        )

        self.assertIn("label", payload)
        self.assertIn("description", payload)
        self.assertIn("type", payload)
        self.assertIn("fieldType", payload)
        self.assertIn("groupName", payload)
        self.assertIn("displayOrder", payload)
        self.assertNotIn("name", payload)
        self.assertNotIn("objectType", payload)
        self.assertNotIn("ignored", payload)

    def test_apply_properties_dry_run_plans_create_and_update_without_writes(self):
        client = _FakeHubSpotClient(existing={("contacts", "favorite_install_type"): {"name": "favorite_install_type"}})
        specs = [
            {
                "objectType": "contacts",
                "name": "favorite_install_type",
                "label": "Favorite Install Type",
                "type": "enumeration",
                "fieldType": "select",
                "groupName": "contactinformation",
            },
            {
                "objectType": "contacts",
                "name": "window_product_line",
                "label": "Window Product Line",
                "type": "enumeration",
                "fieldType": "select",
                "groupName": "contactinformation",
            },
        ]

        results = apply_properties(client, specs, dry_run=True, sleep_ms=0)

        self.assertEqual(2, len(results))
        self.assertEqual("update", results[0].action)
        self.assertEqual("planned", results[0].status)
        self.assertEqual("create", results[1].action)
        self.assertEqual("planned", results[1].status)
        self.assertEqual([], client.created)
        self.assertEqual([], client.updated)

    def test_render_demo_plan_renders_configured_checklist_and_scenarios(self):
        config = {
            "demoPlan": {
                "title": "Contractor Demo Flow",
                "hubspotChecklist": [
                    "Create private app token",
                    "Create required deal/contact properties",
                ],
                "scenarios": [
                    {
                        "name": "Order A Multi-Item",
                        "steps": [
                            "Create order and item 1",
                            "Save item 1",
                            "Add item 2",
                        ],
                    },
                    {
                        "name": "Order B Different Product Line",
                        "steps": [
                            "Create new order",
                            "Use different product line",
                        ],
                    },
                ],
            }
        }

        markdown = render_demo_plan(config)

        self.assertIn("# Contractor Demo Flow", markdown)
        self.assertIn("## HubSpot Setup Checklist", markdown)
        self.assertIn("- [ ] Create private app token", markdown)
        self.assertIn("## Scenario 1: Order A Multi-Item", markdown)
        self.assertIn("1. Create order and item 1", markdown)
        self.assertIn("## Scenario 2: Order B Different Product Line", markdown)
        self.assertIn("1. Create new order", markdown)


if __name__ == "__main__":
    unittest.main()
