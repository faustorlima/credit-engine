## What worked well

- Building the system phase by phase kept the dependency direction enforceable: pure Domain logic was completed before the Application port, JSON adapter, and HTTP host.
- Keeping JSON DTOs inside Infrastructure and registering a validated `RulesPolicy` snapshot at startup prevented configuration and serialization concerns from leaking into request processing.

## What didn't work

- The initial NuGet restore for the API validation and integration-test dependencies was blocked by the default network sandbox and required explicit approval for NuGet access.

## What I'd do differently

- Add a dedicated integration test that boots the API with an intentionally invalid rules directory, so startup-failure behavior is verified directly as well as implemented.
