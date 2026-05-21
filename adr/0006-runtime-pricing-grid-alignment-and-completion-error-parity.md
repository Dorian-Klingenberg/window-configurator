# ADR 0006: Runtime Pricing Grid Alignment And Completion Error Parity

## Context

Phase 5 introduced server-authoritative completion validation and pricing, but two operational gaps remained:

1. `priceInfo.json` breakpoints did not always reach catalog-supported maximum dimensions from `*ItemTemplate.json`, which caused edge behavior past final pricing breakpoints.
2. The browser completion UX was not using the same effective pricing data path as the server and was not surfacing structured completion validation errors from the API.

This created real user-visible mismatches where client estimate and server authoritative price diverged sharply, especially in brickmould-heavy edge cases.

## Decision

1. Introduce runtime pricing-grid alignment:
   - At app startup, load `priceInfo.json` and align each pricing grid to catalog maximum dimensions from `AppData/*ItemTemplate.json`.
   - Use the aligned in-memory pricing object for both `IPricingService` and `ICompletionValidationService`.
   - Return the same aligned in-memory object from `OrderItem/PriceInfo` so browser estimation uses the identical effective grid shape.

2. Preserve server-authoritative completion while improving client parity:
   - On successful completion, set UI displayed price to returned `authoritativePrice`.
   - On completion failure, parse and display API `validationErrors` instead of generic transport text.

3. Enforce mixed-product-line tenant policy at completion:
   - If tenant `MixedProductLinesAllowed` is false, reject completion when the session would contain multiple product lines.

## Alternatives Considered

1. Reject all near-edge dimensions instead of extending pricing grids.
   - Rejected because those dimensions are still catalog-valid and should remain quoteable.

2. Edit and persist expanded `priceInfo.json` statically.
   - Rejected for now to avoid hard-forking vendor/source pricing data and to keep alignment deterministic at runtime from catalog templates.

3. Keep browser on raw `priceInfo.json` and trust only server result.
   - Rejected because it preserves avoidable estimate/authoritative drift and harms user confidence.

## Consequences

1. Catalog-valid maximum dimensions now remain inside aligned pricing grids for server and browser flows.
2. Completion validation retains server authority while giving users actionable error messages.
3. Browser price display reconciles to authoritative price on submit success.
4. Mixed product-line policy is now enforced at completion boundary, not only implied by catalog/tenant setup.
5. Additional tests were added to verify:
   - pricing-grid alignment to template maxima,
   - mixed-product-line rejection path,
   - section-size compatibility rejection path.
