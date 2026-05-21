# Pricing Test Suite Summary

## Overview

Created comprehensive pricing tests covering **59 feature combination permutations** using a standard 36" × 48" window size. These tests validate that the C# pricing engine calculates prices consistently across all feature combinations.

## Test Results

**Total Tests:** 62  
**Passing:** 61 ✅  
**Failing:** 1 ❌ (known bug: JavaScript $1812.33 vs C# $544.10)

## Test Coverage

### Single-Section Feature Variations

1. **Frame Colors** (4 tests)
   - White, Wicker
   - Combined with Picture/Fixed Sash styles

2. **Operational Styles** (5 tests)
   - Picture (no crank)
   - Awning (with crank)
   - Casement (with crank)
   - Casement - Left (with crank)
   - Fixed Sash (no crank)

3. **Brickmould Styles** (4 tests)
   - None: $222.14
   - 1 1/2 Inch: $310.43
   - 2 Inch: $310.43
   - 1 1/2 Inch - Fin: $284.13

4. **Pane Configurations** (4 tests)
   - Dual: $222.14 (base)
   - Dual - LowE/Argon: $258.47 (+$36.33)
   - Triple: $313.56 (+$91.42)
   - Triple - LowE/Argon: $336.65 (+$114.51)

5. **Grille Patterns** (6 tests)
   - None: $222.14 (base)
   - Ladder: $233.03 (+$10.89)
   - Double Ladder: $240.76 (+$18.62)
   - Rectangular: $257.09 (+$34.95)
   - Perimeter: $249.63 (+$27.49)
   - Double Perimeter: $277.14 (+$54.99)

6. **SDL Patterns** (6 tests)
   - All patterns return $222.14 (SDL pricing appears to be $0 in current data)

7. **Crank Types** (4 tests)
   - All crank types (Regular, Folding, Encore, Roto) return same price: $410.57
   - Crank pricing appears to be flat per-section, not per-type

### Multi-Section Combinations

8. **Two-Section Style Combos** (5 tests)
   - Picture + Picture: $343.28
   - Picture + Fixed Sash: $415.88
   - Casement + Casement: $710.73
   - Casement-Left + Casement: $710.73
   - Awning + Picture: $576.42

9. **Three+ Sections** (2 tests)
   - Three sections (Picture/Casement/Picture): $721.79
   - Four sections (all Picture, fully loaded): $893.21

10. **Mixed Features** (3 tests)
    - Different pane configs per section: $373.30
    - Different grille patterns per section: $350.83
    - Casement + Picture with premium features: $696.74

### Feature Combination Matrix

11. **Common Real-World Combos** (8 tests)
    - Tests all combinations of:
      - Frame colors: White, Wicker
      - Brickmould: None, 2 Inch
      - Pane: Dual, Triple - LowE/Argon

12. **Grille/SDL Combinations** (5 tests)
    - Casement with various grille/SDL permutations
    - Awning with grille patterns

### Edge Cases

13. **Minimal Configuration**
    - White, no brickmould, Picture, Dual: $222.14

14. **Fully Loaded Configuration**
    - Wicker, 2" brickmould, Casement, Triple LowE/Argon, Encore crank, Double Perimeter grille, Rectangular SDL: $687.19

## Key Findings

### Pricing Patterns Observed

1. **Additive Components:**
   - Frame color: ~$19-$68 depending on perimeter and color
   - Sections: Varies by style (Picture cheapest, Casement/Awning more expensive)
   - Brickmould: +$88-$62 depending on style
   - Pane configs: +$0 (Dual) to +$114 (Triple LowE/Argon)
   - Grilles: +$0 (None) to +$55 (Double Perimeter)

2. **Flat Pricing (No Variation):**
   - Crank types: All cost the same per section
   - SDL patterns: Currently $0 in all cases

3. **Perimeter-Based:**
   - Frame color, grilles, brickmould, and pane configs all scale with perimeter
   - Style pricing includes base + perimeter calculation

### Data Quality Issues

1. **SDL Pricing:** All SDL patterns return $0 — suggests missing pricing data
2. **Crank Pricing:** No differentiation between crank types — may be intentional or missing data
3. **Dual Pane Config:** Base pane config has $0 PPI (intentional — it's the baseline)

## Test Files

- `PricingFeatureCombinationTests.cs` — 59 feature combination tests
- `PricingCrossValidationTests.cs` — 2 JavaScript vs C# validation tests
- `PricingDebugTests.cs` — 1 detailed breakdown diagnostic test

## Usage

These tests serve as:
1. **Regression protection** — Ensures pricing logic changes don't break existing calculations
2. **Feature coverage** — Documents which feature combinations are supported
3. **Cross-validation baseline** — Once the JS/C# discrepancy is resolved, these provide reference prices for all combinations
4. **Performance baseline** — 62 tests run in <2 seconds, suitable for CI/CD

## Next Steps

1. **Resolve the $1812.33 vs $544.10 bug** — Root cause still unknown
2. **Add JavaScript-generated expected values** — Run actual JS pricing engine to get authoritative prices for each test case
3. **Investigate SDL pricing** — Confirm whether $0 is intentional or missing data
4. **Verify crank pricing** — Confirm whether all crank types should cost the same
5. **Add size variation tests** — Currently all tests use 36" × 48"; add tests for small/large/edge dimensions
