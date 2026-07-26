# Implementation Plan

## Goal

Build a maintainable, configuration-driven Credit Engine using Clean Architecture.
Each phase must produce a working increment that can be compiled and tested.
Tests are implemented with the increment they verify; no phase is reserved for
adding coverage retroactively.

---

# Phase 1 – Four-Project Foundation

## Goal

Establish the enforceable four-project solution structure and its test projects.

## Deliverables

- `CreditEngine.Api`, `CreditEngine.Application`, `CreditEngine.Domain`, and
  `CreditEngine.Infrastructure` projects
- Domain, Infrastructure, and API-integration test projects
- Project references that enforce the dependency direction in `003-architecture.md`
- `rules/` moved as content of `CreditEngine.Api`
- API composition-root baseline and OpenAPI enabled

## Definition of Done

- `dotnet build` succeeds
- `dotnet test` succeeds
- The API starts successfully and its OpenAPI endpoint is available

---

# Phase 2 – Domain Model

## Goal

Model the business domain independently of infrastructure.

## Deliverables

- `Customer` and pure Domain invariants
- `RulesPolicy`, `ClusterRule`, `JobTitleCategory`, `IncomeMatrix`, and
  `PenaltyRule`
- `RulesPolicyValidator`, `CreditAnalysisEngine`, and `CreditAnalysis`
- Required Domain unit tests for cluster assignment and boundaries, job-title
  category matching and priority, income lookup, penalties, cap enforcement,
  conservative rounding, and fallback denial

## Definition of Done

- Domain contains no ASP.NET dependencies
- Domain contains no JSON or FluentValidation dependencies
- Domain compiles independently
- All required Domain unit tests pass

---

# Phase 3 – Application Use Case

## Goal

Orchestrate classification using the pure Domain engine and immutable policy.

## Deliverables

- `IPolicyLoader` Application port
- `ClassifyCustomer` use case
- Read-only `RulesPolicy` dependency and `CreditAnalysis` result flow
- Application use-case tests

## Definition of Done

- Application depends only on Domain
- `ClassifyCustomer` does not read JSON, interpret HTTP, or implement rules
- Application use-case tests pass

---

# Phase 4 – Infrastructure Policy Loading

## Goal

Load the four policy documents and map them to the logical policy model.

## Deliverables

- JSON DTOs for all four policy documents
- `JsonPolicyLoader` implementation of `IPolicyLoader`
- JSON format validation and JSON-to-`RulesPolicy` mapping
- Infrastructure contract tests for valid documents and invalid syntax, types,
  nullability, unknown properties, references, duplicate entries, and invalid
  fallback or condition structures

## Definition of Done

- Infrastructure depends on Application and Domain but never on API
- Valid policy documents load into `RulesPolicy`
- Invalid documents are rejected by the contract tests

---

# Phase 5 – REST API

## Goal

Expose the engine through HTTP.

## Deliverables

- POST /customers/classify
- Startup composition that loads, validates, and registers the immutable
  `RulesPolicy` snapshot
- FluentValidation, normalization, and ProblemDetails mapping
- OpenAPI documentation
- Required API integration tests, including the six cases in
  `tests/fixtures/expected-output.json`

## Definition of Done

- Invalid policy prevents API startup
- The API returns the expected output contract for all six fixture cases
- Invalid and missing request fields return the required ProblemDetails response
- The complete test suite runs with `dotnet test`

---

# Phase 6 – Operational Documentation

## Goal

Document how to build, test, run, and maintain the implemented solution without
duplicating the normative specifications.

## Deliverables

- README with prerequisites, build, test (`dotnet test`), and run instructions
- Documentation of the API-owned `rules/` location and how policy changes are
  validated at startup
- A concise implemented-architecture overview that links to
  `001-requirements.md`, `002-rules-configuration.md`, and
  `003-architecture.md` rather than restating them
- Updated AI Journey entries for material implementation decisions

## Definition of Done

- A new developer can build, test, and run the solution by following the README
- Documentation identifies the policy-document location and the applicable
  specifications
- Documentation reflects the implemented project structure and API contract

---

# Phase 7 – Final Review

## Goal

Verify that the repository satisfies the approved specifications and is ready
for evaluation.

## Checklist

- `dotnet build` succeeds
- `dotnet test` succeeds from the repository root
- The six cases in `expected-output.json` produce their exact HTTP responses
- Invalid policy prevents API startup; invalid and missing request fields return
  the specified ProblemDetails response
- OpenAPI describes the implemented endpoint and validation responses
- Project references still enforce the dependency direction in
  `003-architecture.md`
- Formatting and documentation are reviewed; no obsolete implementation paths
  remain
