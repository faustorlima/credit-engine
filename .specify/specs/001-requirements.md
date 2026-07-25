# Credit Engine Requirements

## 1. Overview

Build a REST API that classifies customers into a risk cluster,
estimates monthly income and calculates approved credit limits.

---

## 2. Functional Requirements

### FR-001 Customer Classification

The system shall classify a customer into exactly one risk cluster.

Acceptance Criteria

- Clusters are evaluated by priority.
- First matching cluster wins.
- Cluster D is the fallback.

---

### FR-002 Job Category

The system shall classify the customer's job title.

Acceptance Criteria

- Case insensitive
- First keyword wins
- OTHER when no match

---

### FR-003 Monthly Income

The system shall estimate the customer's monthly income based on:

- Cluster
- Job Category

---

### FR-004 Credit Limit

The system shall calculate the approved limit.

Acceptance Criteria

- Apply multiplier
- Apply penalties
- Respect cap
- Round to nearest 100

---

### FR-005 API

POST /customers/classify

Returns the enriched customer.

---

## 3. Non Functional Requirements

- Stateless
- Configuration-driven rules
- Testable
- Clean Architecture
- OpenAPI

---

## 4. Constraints

- .NET 8
- REST
- JSON
- No persistence

---

## 5. Assumptions

Rules loaded at startup.

Configuration is immutable.

No authentication.

---

## 6. Out of Scope

Persistence

Authentication

Caching

Database

## OPen Questions

### OQ-001

What should happen if:

has_market_debt = false

but

market_debt_types contains loan_default?

Decision

The implementation will trust has_market_debt as the authoritative field.

Reason

The challenge does not specify conflict resolution.

### OQ-002

Should keyword matching ignore accents?

Decision

No.

Reason

The challenge only specifies case-insensitive matching.