# ADR 0011: API-Key Tenant Scope And Prospect Resume Hardening

**Date:** 2026-05-21  
**Status:** Accepted

## Context

Phase 8 required concrete tenant isolation and identity mechanics beyond Phase 7 integration plumbing. Prior to this slice, API routes were versioned and functional, but not consistently protected by tenant-scoped authentication. Prospect resume support also existed only as planned domain shape (`MagicLinkToken`, `MagicLinkExpiresAt`) without a minimal API contract.

## Decision

1. Add API-key authorization for `/api/v1/*` controller surfaces via `ApiKeyAuthorize` filter.
2. Resolve tenant by `X-Api-Key`, store authenticated tenant ID in request context, and enforce:
   - `401` for missing/invalid key
   - `403` for tenant scope mismatch
3. Add tenant policy/branding management API:
   - `GET/PUT /api/v1/tenants/{id}/policy`
4. Keep webhook callback management API in place:
   - `GET/PUT /api/v1/tenants/{id}/integration`
5. Add minimal prospect identity/resume API:
   - `POST /api/v1/prospect-sessions` (start + token issuance)
   - `GET /api/v1/prospect-sessions/resume?token=...` (resume with expiry enforcement)

## Alternatives Considered

1. Jump directly to OAuth and defer API-key auth.
   - Rejected for this phase because API-key auth achieves tenant isolation quickly and cleanly while preserving a path to OAuth later.

2. Keep tenant scoping implicit in request payload IDs.
   - Rejected because payload-based scoping is not a trustworthy authorization strategy.

3. Defer prospect resume until full email pipeline.
   - Rejected because tokenized start/resume contracts can be validated now; delivery transport can evolve independently.

## Consequences

1. API routes now enforce explicit tenant identity and scope boundaries.
2. Tenant policy and branding become manageable through versioned API contracts.
3. Prospect resume behavior is no longer purely conceptual; it is test-backed and API-accessible.
4. Phase 8 hardening is complete at the application-contract level, while Phase 9 remains optional UI modernization.
