# Credit Engine

Credit Engine is a .NET 8 REST API that classifies a customer and calculates an approved credit limit from an immutable, configuration-driven policy.

The normative behavior and policy contract are defined in:

- [Requirements](.specify/specs/001-requirements.md)
- [Rules configuration](.specify/specs/002-rules-configuration.md)
- [Architecture](.specify/specs/003-architecture.md)

## Prerequisites

- .NET 8 SDK

## Build and test

Run these commands from the repository root:

```powershell
dotnet build CreditEngine.slnx
dotnet test CreditEngine.slnx
```

## Run the API

```powershell
dotnet run --project src/CreditEngine.Api/CreditEngine.Api.csproj
```

When the API is running, the OpenAPI document is available at `/swagger/v1/swagger.json` and Swagger UI at `/swagger`.

## Policy files

The API owns the deployed policy files in [`src/CreditEngine.Api/rules`](src/CreditEngine.Api/rules):

- `customerClusters.json`
- `jobTitleCategories.json`
- `monthlyIncome.json`
- `penaltyRules.json`

They are copied to the API build and publish outputs. At startup, Infrastructure reads the documents, validates their JSON shape, maps them to the logical policy model, and Domain validates policy invariants. Invalid policy prevents the API from starting. Policy is then registered as a read-only snapshot; requests never read policy files.

## Implemented architecture

The solution follows the four-project dependency direction described in the [architecture](.specify/specs/003-architecture.md):

```text
CreditEngine.Api -> CreditEngine.Application + CreditEngine.Infrastructure
CreditEngine.Application -> CreditEngine.Domain
CreditEngine.Infrastructure -> CreditEngine.Application + CreditEngine.Domain
CreditEngine.Domain -> no outer-layer dependencies
```

`CreditEngine.Api` owns HTTP validation, request/response mapping, OpenAPI, and composition. `CreditEngine.Application` owns the classification use case and the policy-loading port. `CreditEngine.Infrastructure` owns JSON loading and DTOs. `CreditEngine.Domain` owns policy models, validation, and rule evaluation.
