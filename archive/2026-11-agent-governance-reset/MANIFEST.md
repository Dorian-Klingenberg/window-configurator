# Legacy Instruction Snapshot Manifest

This manifest records every legacy instruction file preserved in
`legacy-agent-instructions/`. The path is the file's former active path and the
SHA-256 value was calculated immediately after archival on 2026-11-05.

| Former path | Bytes | SHA-256 |
|---|---:|---|
| `.cursorrules` | 1510 | `3F6368DB32C7E4F59E23BE470C3E22B205D3A333900447BAD2DBD579FE02B5BC` |
| `AGENTS.md` | 14610 | `5D0B76770815154CE55EDAE7A1478DD3E048F60C8E0A741FF9AA46737A4CA122` |
| `CLAUDE.md` | 2064 | `13DCC713C9544B52ADCCBC67F5A62177400253327AB793DD26AE6CE8C9C7CD3C` |
| `GEMINI.md` | 2063 | `ECE1571579515FCF169FF408BC7EDCDEB8070FAF1F56845C5115FC376F4A8C19` |
| `.claude/settings.json` | 188 | `7BA6A3FD1F392E4F104AA4EB673CC193390D04092FF483ACF2E559C4F8AF8446` |
| `.claude/commands/create-lesson.md` | 515 | `1B71111E8339D3ACB1176A3840B1DD1C21126859C848E53AFF1515A65146335F` |
| `.claude/commands/write-journal-entry.md` | 481 | `F1671152C705A20465820CB7993F402B4240A1B3F790E7642D8DB8AD1B3C12B1` |
| `.github/copilot-instructions.md` | 2416 | `BF0AB38A570635BCB9922FBFDCE4164643277669D2A3DA632A580D2E4032E916` |
| `.github/CreateLesson.agent.md` | 324 | `B95D200AE085A18586BE6C378E9EA005FF695234A5ADF52E568EFC87F72CD86C` |
| `skills/create-lesson-core.md` | 6808 | `F801124397EAE3AB48B235C89F2AA2DA3B61EAA10C108EF4BADADF9CA013978A` |
| `skills/hubspot-org-config-core.md` | 4201 | `31CA8420CA7BF8B3476E737287F82ED79DDC5BEC950BFBD205B427FEA8824372` |
| `skills/vscode-debug-launch-core.md` | 3549 | `A95C35E6F8A55B3DFCDA4905ED87FFA87A4ABF75D3E9F0980CE429BB059BFF80` |
| `skills/write-journal-entry.md` | 4479 | `E8531F2B586612AF26554A3240C29D20F04D96C043F8541B56A137C07AA23086` |

The separate `legacy-governance/` directory contains full historical copies of
the retired branch policy and handoffs. Those are project-history records, not
agent instruction sets, so they are intentionally outside this manifest.
