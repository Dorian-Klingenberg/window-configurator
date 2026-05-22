# ADR 0013 — API Key Rotation/Revocation Lifecycle

## Context

After Phase 8, each tenant had exactly one API key stored as a plain string on `TenantEntity`. There was no way to invalidate a key without a direct database edit. In a production multi-tenant system this is a liability: a leaked or compromised key cannot be revoked, and there is no mechanism to enforce key rotation policies.

## Decision

Add two nullable lifecycle fields to `TenantEntity`:

- `ApiKeyExpiresAtUtc` — when set, the key is rejected after this UTC timestamp
- `ApiKeyRevokedAt` — when set, the key is permanently rejected regardless of expiry

Update `ApiKeyAuthorizeFilter` to enforce both checks after the key is found in the database. The rejection order is: missing key → invalid key → revoked → expired → grant access.

Add two management endpoints scoped to the authenticated tenant:

- `POST /api/v1/tenants/{id}/api-key/rotate` — generates a new `Guid.ToString("N")` key, replaces the current one, clears any existing revocation and expiry, and optionally sets a new `ExpiresAtUtc`. The raw key value is returned only in this response.
- `DELETE /api/v1/tenants/{id}/api-key` — sets `ApiKeyRevokedAt` on the current key. Idempotent: a second revoke call is a no-op.

## Why a single key per tenant (no key history)

Storing a history of previous keys would require a separate `TenantApiKey` entity and a one-to-many relationship. For the current product stage, that adds complexity without clear benefit: rotation atomically replaces the key, and the caller is responsible for propagating the new key to their CRM. A key history table would be needed only if we needed to audit past key usage — a future concern.

## Why `Guid.ToString("N")` for key generation

A 32-character hexadecimal string from `Guid.NewGuid()` provides 128 bits of randomness, is URL-safe, and is trivially comparable. For a higher-security context, a cryptographically random byte array encoded as base64 would be preferable. This can be upgraded later without changing the field type.

## Why expiry is nullable (not required)

Many internal or long-lived integrations do not need key expiry. Requiring an expiry would break the developer experience for dev/test environments. Nullable expiry means "this key never expires" and is the correct default for most tenants.

## Consequences

- A leaked key can now be revoked without a database intervention.
- Tenants can enforce rotation policies by setting `ExpiresAtUtc`.
- Rotation clears revocation status, so a revoked key can be "un-revoked" via rotate — intentional: the old key value is replaced, not restored.
- The raw key value is only exposed at rotation time. Existing tests that seed tenants directly with known keys still work (key lifecycle fields are nullable and default to null).
