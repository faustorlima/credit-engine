# Architecture

## 1. Overview

This solution implements a configuration-driven Credit Classification Engine using
Clean Architecture.

The primary architectural goal is to isolate policy evaluation from application
code. Classification, job categories, penalties and income tables are
represented as configuration, allowing changes to policy values and entries
that conform to `RulesPolicy` without modifying the domain logic.

---

## 2. Architectural Goals

- Separation of concerns
- Data-driven business rules
- High testability
- Extensibility
- Stateless request processing
- Independent domain model

---

## 3. Architecture Style

The solution follows Clean Architecture.

```
                +----------------------+
                |      API Layer       |
                | Endpoints            |
                | HTTP Validation      |
                | Composition Root     |
                | rules/ content       |
                +----------+-----------+
                           |
                           v
                +----------------------+
                |    Application       |
                | ClassifyCustomer     |
                | RulesPolicy snapshot |
                +----------+-----------+
                           |
                           v
                +----------------------+
                |      Domain          |
                | CreditAnalysisEngine |
                | RulesPolicy          |
                | RulesPolicyValidator |
                +----------+-----------+
                           ↑
                           | implements abstractions
                +----------------------+
                | Infrastructure       |
                | JsonPolicyLoader     |
                | JSON-to-policy mapper |
                +----------------------+
```

The Domain layer has no dependency on ASP.NET Core,
JSON serialization or configuration frameworks.

### Project Structure

The solution is composed of four .NET projects:

| Project | Responsibility |
|---|---|
| `CreditEngine.Api` | HTTP host, endpoints, FluentValidation integration, response mapping, OpenAPI publication, composition root, and `rules/` content. |
| `CreditEngine.Application` | `ClassifyCustomer`, startup orchestration contracts, and the `IPolicyLoader` port. |
| `CreditEngine.Domain` | Pure policy models, `CreditAnalysisEngine`, `RulesPolicyValidator`, and Domain invariants. |
| `CreditEngine.Infrastructure` | `JsonPolicyLoader`, JSON document DTOs, and JSON-to-`RulesPolicy` mapping. |

### Application Responsibility

`ClassifyCustomer` is the Application use case. It receives a normalized
`Customer`, invokes `CreditAnalysisEngine` with the immutable `RulesPolicy`,
and returns `CreditAnalysis`.

The use case orchestrates the flow but does not read JSON, interpret HTTP, or
implement classification rules.

---

## 4. Rule Engine

Business rules are externalized into JSON configuration files.

At application startup, the API composition root supplies the `rules/` content
location to an `IPolicyLoader` Application port. Infrastructure implements that
port through `JsonPolicyLoader`, which loads, deserializes, validates the JSON
document format, and maps the policy documents to a `RulesPolicy` domain model.

The Application startup flow invokes the Domain `RulesPolicyValidator` for the
logical invariants defined by the policy contract. Only a valid `RulesPolicy`
can become the running snapshot.

The resulting `RulesPolicy` is an immutable snapshot registered during
application composition. Application use cases and the Rule Engine receive this
snapshot as a read-only dependency; no provider or file is accessed during a
request.

The Rule Engine has no knowledge of JSON or the underlying configuration source. It evaluates the provided rules deterministically according to their configured priority.

The classification workflow follows:

```
Customer

↓

Cluster Evaluation

↓

Job Category Evaluation

↓

Income Lookup

↓

Penalty Evaluation

↓

Credit Limit Calculation

↓

Result
```

---

## 5. Configuration Strategy

Rules are stored in JSON because:

- readable by business analysts
- easy to version
- simple to test
- independent from source code
- no recompilation required

Future implementations could replace the JSON provider with:

- Database
- Feature Flags
- Remote Configuration
- Rule Service

without changing the Domain layer, provided that they map to the same
`RulesPolicy` contract. Expanding that contract, such as by adding a new
condition type, requires evolution of the Domain model and Rule Engine.

### Configuration Boundary

The JSON policy documents are content deployed with the API layer under
`rules/`. The API layer owns their physical location and supplies it during
application composition; it does not parse or interpret business rules.

The Infrastructure layer is responsible for loading, deserializing, validating
the JSON document format, and mapping those documents to the `RulesPolicy`
logical model through the `IPolicyLoader` port.

JSON document DTOs belong exclusively to Infrastructure. Application, Domain,
and API operate on logical models and never depend on JSON DTO types.

The Domain layer operates on strongly typed rule definitions and has no
knowledge of the underlying configuration format. `RulesPolicyValidator`
validates the logical policy invariants after mapping.

The API/Application startup flow coordinates both validation stages.
Invalid configuration prevents the application from starting.

---

## 6. Domain Model

Core concepts:

Customer

RulesPolicy

RulesPolicyValidator

ClusterRule

JobTitleCategory

IncomeMatrix

PenaltyRule

CreditAnalysis

CreditAnalysisEngine

---

## 7. Dependency Direction

Allowed compile-time dependencies are:

| Layer | May depend on |
|---|---|
| API | Application and Infrastructure, only to compose the running application. |
| Application | Domain. |
| Infrastructure | Application, to implement `IPolicyLoader`, and Domain, to construct `RulesPolicy`. |
| Domain | No outer layer. |

No business logic exists in API endpoints.

---

## 8. API Boundary

The API layer owns HTTP request/response mapping and the input validation and
normalization required by FR-001. Application use cases return a
`CreditAnalysis` domain result.

The API layer maps `CreditAnalysis` to the `creditAnalysisResult` response
contract defined by FR-007, including the matched logical cluster, job-title
category, and penalty rule when present. JSON deserialization DTOs used by
Infrastructure are never exposed in HTTP responses.

The API layer publishes OpenAPI documentation for the endpoint, request and
response contracts, and validation `ProblemDetails` responses.

---

## 9. Validation

### API Request Validation

The API layer uses FluentValidation to validate and normalize the HTTP request
before invoking `ClassifyCustomer`. Invalid requests are mapped to the
`400 application/problem+json` contract in FR-001, and do not execute business
rules.

### Domain Invariants

The Domain independently enforces the core `Customer` invariants required for
rule evaluation, including age, score, and consistency between `hasMarketDebt`
and `marketDebtTypes`. It does not depend on FluentValidation.

`RulesPolicyValidator` independently enforces the logical policy invariants
after JSON documents have been mapped to `RulesPolicy`.

---

## 10. Testing Strategy

Both unit and integration tests are required. The complete suite must run with
a single `dotnet test` command.

### Required Domain Unit Tests

- Cluster assignment for every configured cluster, including boundary conditions
  such as a score exactly at a threshold
- Job-title category matching, including case-insensitivity and priority order
- Credit-limit calculation: base formula, penalty application, cap enforcement,
  and `round_to_nearest_100`
- Monthly-income lookup for every configured cluster/job-title-category pair
- Fallback-cluster denial, with zero approved limit in the initial policy

### Required API Integration Tests

- `POST /customers/classify` with a valid request returns the required output
  contract
- `POST /customers/classify` with invalid or missing fields returns the required
  error response
- All six sample customers defined in `expected-output.json` produce their
  exact expected outputs

`expected-output.json` and its six sample customers are mandatory integration
test fixtures and must be provided before implementation.

### Supporting Infrastructure Contract Tests

- Valid policy-document loading and mapping
- Invalid JSON syntax, types, nullability, and unknown properties
- Invalid references, duplicate identifiers or matrix entries, and invalid
  fallback or condition structures

### Supporting Application Startup Tests

- Startup failure for an invalid policy

---

## 11. Design Decisions

### Why Clean Architecture?

Business rules are the core of the system.

The chosen architecture isolates them from infrastructure concerns.

---

### Why JSON?

The challenge explicitly requests a data-driven solution.

Representing the policy as JSON allows changing policy values and configured
entries that conform to `RulesPolicy` without changing Domain code.

---

### Why not hardcode if/else?

Embedding business rules into imperative code would tightly couple
business policy to implementation.

The Rule Engine evaluates configuration rather than code.

---

### Why a custom Rule Engine?

The rule set is relatively small and deterministic.

A lightweight implementation is easier to understand,
maintain and test than introducing an external rules framework.

---

### Why a Domain-Specific Rule Schema?

The rule configuration uses a domain-specific schema rather than a generic
expression-based rule language.

The current business rules operate on a small and well-defined set of customer
attributes. Modeling these conditions explicitly keeps the configuration
strongly typed, readable, easy to validate, and straightforward to test.

A generic rule language based on fields, operators, and arbitrary expressions
would provide greater flexibility, but would introduce additional complexity
in parsing, type validation, error handling, and rule execution that is not
justified by the current requirements.

The design favors the simplest model that satisfies the configuration-driven
requirement while preserving the ability to evolve the rule model as new
business requirements emerge.

---

## 12. Future Improvements

- Database-backed rule provider
- Rule versioning
- Administrative UI
- Audit trail
- OpenTelemetry
- Metrics
- Rule hot reload
