# Pricing Test Suite Summary

## Current Status

The pricing comparison tests are part of the normal `dotnet test .\WindowConfigurator.sln` suite.

Current verification:

- `PricingComparisonMatrixTests` -> JavaScript-generated fixture compared against C# pricing service
- `PricingCrossValidationTests` -> hand-curated JavaScript vs C# regression cases
- Full suite -> 157 passing

Note: when overriding build output during local debugging, use an absolute repo-root `BaseOutputPath` (for example `-p:BaseOutputPath=<repo>\artifacts\testbin\`). Some pricing tests resolve `AppData` fixtures relative to runtime output structure, and relative output overrides can produce false `DirectoryNotFoundException` failures.

The earlier `$1812.33` expectation was resolved as a stale screenshot narrative, not an actual result from executing the legacy JavaScript engine against the same payload. The verified JavaScript result for that disputed large two-section case is `$1066.75`, and the C# service now matches it.

## JavaScript Comparison Matrix

The generated fixture is `pricing-comparison-fixture.json`. It was produced by `generate-pricing-comparison-fixture.js`, which executes the legacy browser pricing engine directly from:

- `wwwroot/js/windowStore/windowStore.measurement.js`
- `wwwroot/js/windowStore/pricing.js`
- `AppData/priceInfo.json`

The matrix contains:

| Category | Count |
|---|---:|
| Total generated cases | 80,911 |
| In-range cases | 49,461 |
| Past-final-breakpoint cases | 31,450 |

The in-range comparison test requires every in-range case to match JavaScript exactly. The past-final-breakpoint test intentionally confirms parity gaps still exist beyond supported pricing grids, so raw pricing-service differences remain visible. Phase 5 completion validation now rejects submitted dimensions that exceed the supported pricing grid before authoritative pricing runs.

The matrix also includes a submitted two-section regression case where brickmould pricing dimensions (`width`/`height`) differ from frame dimensions (`frameWidth`/`frameHeight`). This catches the `$447.75` client estimate versus `$467.72` server result that occurred when C# priced brickmould from frame dimensions instead of the top-level sizing used by the legacy JavaScript.

## Product Line Coverage

| Product line | Total | In range | Past breakpoint |
|---|---:|---:|---:|
| EnergySaver 2500 | 50,651 | 31,957 | 18,694 |
| Apex | 7,880 | 5,072 | 2,808 |
| Carriage | 22,380 | 12,432 | 9,948 |

## Section Count Coverage

| Section count | Cases |
|---|---:|
| 1 section | 42,350 |
| 2 sections | 6,161 |
| 3 sections | 32,400 |

## Feature Coverage

Across the generated matrix:

- Frame colors: White, Wicker
- Brickmould styles: None, 1 1/2 Inch, 1 1/2 Inch - Fin, 1 5/8 Inch, 2 Inch, 3 1/2 Inch
- Pane configurations: Dual, Dual - LowE/Argon, Triple, Triple - LowE/Argon
- Styles: Awning, Casement, Casement - Left, Double hung, Fixed Sash, Glider, Glider - Left, Picture, Single Hung, Single hung - Down
- Cranks: None, Regular, Folding, Encore, Roto, Encore/ADA, Encore/Folding, Roto/Folding, Roto/Regular
- Grilles: None, Ladder, Double Ladder, Rectangular, Perimeter, Double Perimeter, plus unpriced placeholders for product lines without grille breakpoint pricing
- SDL: None, Colonial, Craftsman, Heritage, plus unpriced placeholders for product lines without SDL breakpoint pricing

## Size Coverage

Single-section cases use ten profiles per style:

- min exact breakpoint
- first midpoint
- second exact breakpoint
- second/third midpoint
- middle exact breakpoint
- middle/next midpoint
- penultimate exact breakpoint
- max exact breakpoint
- width overflow
- width and height overflow

Multi-section cases use ten corresponding profile indexes (`p0` through `p9`) across all 2-style and 3-style tuples for each product line.

## Previous Matrix Construction Bug

The previous comparison issue was in fixture construction, not the C# pricing engine:

- some single-section cases omitted pane configuration
- some multi-section cases treated pane configuration as per-section even though legacy JavaScript prices it from the window-level selection
- some cases were marked in-range by style grid only, without checking grille, SDL, pane, or brickmould breakpoint grids

The current generator fixes those problems:

- pane configuration is included consistently
- multi-section cases use one window-level pane selection
- past-final-breakpoint detection checks style, grille, SDL, pane, and brickmould pricing grids

## Other Pricing Tests

`PricingFeatureCombinationTests.cs` contains feature smoke/regression coverage over common combinations at a standard size. Those tests assert pricing remains positive/non-negative across combinations, but they are not the JavaScript golden comparison; the matrix and cross-validation tests are the authoritative JS-vs-C# checks.
