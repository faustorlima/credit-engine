# Architecture

## 1. Overview

This solution implements a configuration-driven Credit Classification Engine using
Clean Architecture.

The primary architectural goal is to isolate business rules from application code.
Classification, job categories, penalties and income tables are represented as
configuration, allowing business changes without modifying the domain logic.

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
                | Controllers          |
                | Validation           |
                +----------+-----------+
                           |
                           v
                +----------------------+
                |    Application       |
                | Use Cases            |
                | Orchestration        |
                +----------+-----------+
                           |
                           v
                +----------------------+
                |      Domain          |
                | Entities             |
                | Rule Engine          |
                | Services             |
                +----------+-----------+
                           |
                           v
                +----------------------+
                | Infrastructure       |
                | JSON Rules           |
                | Configuration Loader |
                +----------------------+
```

The Domain layer has no dependency on ASP.NET Core,
JSON serialization or configuration frameworks.

---

## 4. Rule Engine

Business rules are externalized into JSON configuration files.

The engine loads:

- Cluster definitions
- Job categories
- Income matrix
- Penalty rules

at application startup.

The application evaluates rules in priority order.

```
Customer

↓

Cluster Rules

↓

Job Category Rules

↓

Income Lookup

↓

Penalty Rules

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

without changing the domain layer.

---

## 6. Domain Model

Core concepts:

Customer

Cluster

ClusterRule

JobCategory

JobCategoryRule

PenaltyRule

CreditDecision

MonthlyIncome

---

## 7. Dependency Direction

API
↓

Application
↓

Domain

Infrastructure implements the abstractions required by the Domain/Application.

No business logic exists in Controllers.

---

## 8. Validation

Input validation is performed before entering the domain.

Invalid requests never execute business rules.

---

## 9. Testing Strategy

Unit Tests

- Rule evaluation
- Cluster assignment
- Job matching
- Income lookup
- Penalty calculation
- Credit calculation

Integration Tests

- Complete API flow
- Invalid payloads
- Expected output validation

---

## 10. Design Decisions

### Why Clean Architecture?

Business rules are the core of the system.

The chosen architecture isolates them from infrastructure concerns.

---

### Why JSON?

The challenge explicitly requests a data-driven solution.

Representing rules as JSON allows adding or modifying
business rules without changing domain code.

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

## 11. Future Improvements

- Database-backed rule provider
- Rule versioning
- Administrative UI
- Audit trail
- OpenTelemetry
- Metrics
- Rule hot reload