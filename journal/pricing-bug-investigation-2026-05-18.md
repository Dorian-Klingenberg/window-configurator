# Pricing Bug Investigation - 2026-05-18

## Resolution

This investigation is now resolved.

- The `$1812.33` value was **not** the result of running the exact payload in the legacy JavaScript engine. Executing `pricing.js` directly against the same measurements and `priceInfo.json` returns **`$1066.75`**.
- The C# port was underpricing the same case at **`$544.10`** for two reasons:
  - it priced brickmould from `osmWidth` / `osmHeight`, while the legacy JavaScript prices brickmould from frame `width` / `height`
  - it used a clamped interpolated brickmould PPI, while the legacy JavaScript's zero-width branch reuses the breakpoint PPI as a slope, causing brickmould pricing to keep growing once width exceeds the final breakpoint

The fix was to:

1. carry both frame and outside measurements in `WindowPricingInput`
2. build pricing input from `frameWidth` / `frameHeight` plus `osmWidth` / `osmHeight`
3. match the legacy JavaScript brickmould calculation exactly

After the fix:

- C# authoritative price for the disputed configuration: **`$1066.75`**
- Full test suite: **140 passing, 0 failing**

## Matrix Follow-Up

After resolving the disputed two-section case, the comparison work was expanded into a generated matrix that runs tens of thousands of combinations through the legacy JavaScript engine and compares them to the C# port.

That broader audit showed:

- all **49,460** genuinely in-range cases matched exactly
- **31,450** cases crossed at least one selected component's final breakpoint
- **21,046** of those overflow cases diverged from the C# result

This confirmed that the remaining pricing difference is not an ordinary interpolation bug inside the supported grid. The unresolved gap is the legacy extrapolation behavior once a selected style, grille, SDL, pane configuration, or brickmould option runs past its final width and/or height breakpoint.

## Problem Statement

User reported a pricing discrepancy:
- **JavaScript client estimate:** $1812.33
- **C# server calculation:** $350.48 (from screenshot)
- **C# test calculation:** $544.10 (using exact payload dimensions)

The $350.48 vs $544.10 discrepancy suggests the screenshot used different section dimensions than the JSON payload provided.

## Root Cause Analysis

### Data Verified Correct
✅ Frame color: White at $0.28/inch → $67.41  
✅ Section 1 (Picture, 27.375" × 44.0625"): $157.92  
✅ Section 2 (Picture, 48.4375" × 44.0625"): $188.10  
✅ Brickmould (2 Inch): $130.66  
✅ Adjustment (0%): $0  
✅ Markup (0%): $0  
**C# Subtotal: $544.10**

### Data Found to Be Zero
❌ Pane Configuration (Dual): $0.00 (all pricePerInch values in priceInfo.json are 0)

### Hypothesis

The $1812.33 estimate shown in the JavaScript UI and the $544.10 calculated by C# suggest one of:

1. **Payload inconsistency:** Section 2 width shows as 48.4375" (`whole: 48, numerator: 7, denominator: 16`) in the JSON payload but the `widthDescription` field says `"34 5/8"`. If the UI calculated using 34.625" but the server used 48.4375", that explains part of the delta.

2. **Stale/incorrect priceInfo.json:** The "Dual" pane configuration has all zeros for `pricePerInch`. This appears intentional (Dual is the base configuration with no upcharge). The $1268.23 delta cannot be explained by pane config pricing alone.

3. **Missing pricing component:** There may be another component in the JavaScript pricing engine that C# is not implementing. Candidates:
   - Jamb extension (JS line 183, but returns 0 per line 494-496)
   - Hidden adjustments or product-line-specific rules not captured in priceInfo.json
   - Client-side price overrides or cached values

## Interpolation Logic Verified

The C# `PriceInterpolator` matches the JavaScript `getPriceForSection` logic:
- ✅ Finds bounding width breakpoints
- ✅ Interpolates height at both width bounds
- ✅ Interpolates final PPI between the two height-interpolated values
- ✅ Returns `perimeter × PPI`

## Test Case Added

Created `PricingCrossValidationTests.LargeWindow_TwoSections_2InchBrickmould_MatchesJavaScript()` that:
- Uses the exact dimensions from the user's payload
- Expects $1812.33 (JavaScript result)
- Currently fails with $544.10 (C# result)

This test will pass once the root cause is identified and fixed.

## Next Steps

1. **Verify the actual payload sent to the server** — confirm it matches the JSON provided
2. **Check priceInfo.json history** — was "Dual" pane config ever non-zero?
3. **Inspect JavaScript console output** — add logging to see component-by-component breakdown
4. **Compare section dimensions** — resolve the width discrepancy (48.4375" vs 34.625") for section 2
5. **Check for client-side price caching** — the UI might be showing a stale estimate

## Files Changed

- `WindowConfigurator.Tests/Pricing/PricingCrossValidationTests.cs` (new)
- `WindowConfigurator.Tests/Pricing/PricingDebugTests.cs` (new)

Both tests currently fail as expected, capturing the bug for future regression testing.
