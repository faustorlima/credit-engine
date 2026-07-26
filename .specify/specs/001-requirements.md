# Credit Engine Requirements

## 1. Overview

The Credit Engine is a stateless REST API that classifies a customer into a
risk cluster, categorizes the customer's job title, estimates monthly income,
and calculates an approved credit limit.

Business policy is configuration-driven.

---

## 2. Input Contract

### Customer

| Field | Type | Required | Validation and behavior |
|---|---|---:|---|
| `id` | string | yes | Must be a valid UUID. The API does not verify uniqueness across requests. |
| `name` | string | yes | After trimming leading and trailing spaces, must contain at least 2 characters. The trimmed value is returned. |
| `age` | integer | yes | Must be at least 18. There is no global maximum age; cluster rules may impose age limits. |
| `score` | integer | yes | Must be between 0 and 1000, inclusive. |
| `hasMarketDebt` | boolean | yes | Indicates whether the customer has market debt. |
| `marketDebtTypes` | string[] | yes | Must be consistent with `hasMarketDebt`; see [Market debt types](#market-debt-types). |
| `location` | Location | yes | Customer location. |
| `jobTitle` | string | yes | After trimming leading and trailing spaces, must contain at least 2 characters. The trimmed value is returned. |

### Location

| Field | Type | Required | Validation and behavior |
|---|---|---:|---|
| `city` | string | yes | After trimming leading and trailing spaces, must contain at least 2 characters. No city database validation is performed. The trimmed value is returned. |
| `state` | string | yes | Must be a valid Brazilian UF, case-insensitively. The response returns the canonical uppercase UF. |
| `region` | string | yes | Must be a valid Brazilian region and correspond to `state`, case-insensitively. The response returns the canonical display value. |

Valid regions and corresponding UFs are:

| Region | UFs |
|---|---|
| `Norte` | AC, AP, AM, PA, RO, RR, TO |
| `Nordeste` | AL, BA, CE, MA, PB, PE, PI, RN, SE |
| `Centro-Oeste` | DF, GO, MT, MS |
| `Sudeste` | ES, MG, RJ, SP |
| `Sul` | PR, RS, SC |

`Centro-Oeste` is returned in that canonical form. Inputs such as `centro oeste`
and `Centro Oeste` are accepted.

### Market debt types

The accepted canonical values are:

- `credit_card`
- `personal_loan`
- `mortgage`
- `credit_default`
- `loan_default`

The values are case-sensitive and must use the canonical spelling above.

- When `hasMarketDebt` is `true`, `marketDebtTypes` must contain at least one
  valid value.
- When `hasMarketDebt` is `false`, `marketDebtTypes` must be empty.
- Duplicate values are removed before rule evaluation. The response returns the
  normalized list, preserving the first-occurrence order.

---

## 3. Functional Requirements

### FR-001 Input validation and normalization

The system shall validate and normalize a customer before executing business
rules.

Acceptance criteria:

- Invalid requests return `400 Bad Request` using `application/problem+json`
  ProblemDetails with field-level errors.
- When an invalid value belongs to a finite set, the validation error shall
  include the accepted values whenever possible. For other constraints, it
  shall describe the required format or range.
- No business rule is evaluated for an invalid request.
- The normalizations defined in the input contract are reflected in the
  returned customer.

### FR-002 Customer classification

The system shall classify a valid customer into exactly one risk cluster.

Acceptance criteria:

- Cluster rules are evaluated in ascending numeric `priority` order.
- The first matching cluster wins.
- Exactly one cluster fallback is required. It is identified by
  `conditions.catchAll: true`.
- The fallback has the greatest numeric `priority` value and is evaluated last;
  it applies when no preceding cluster matches.

### FR-003 Job-title categorization

The system shall assign a job-title category to the customer.

Acceptance criteria:

- Categories are evaluated in ascending numeric `priority` order; the first
  matching category wins.
- A keyword matches when its normalized form is contained in the normalized
  `jobTitle`. Normalization ignores case, accents, spaces, and hyphens.
- `OTHER` is the required fallback category when no configured keyword matches.

### FR-004 Monthly income estimation

The system shall estimate monthly income from the matched cluster and job-title
category using the configured income matrix.

Acceptance criteria:

- The matrix must define an income for every configured cluster/category pair,
  including the fallback job-title category and the fallback cluster.
- The initial policy defines monthly income as zero for every pair involving the
  fallback cluster.

### FR-005 Penalty selection

The system shall select at most one penalty rule for a customer.

Acceptance criteria:

- Penalty rules are evaluated in ascending numeric `priority` order.
- The first matching penalty rule is applied; all later matching rules are
  ignored.
- When no penalty rule matches, `penaltyFactor` is `1.0` and no penalty changes
  the calculation.

### FR-006 Credit-limit calculation

The system shall calculate the approved limit using the matched cluster,
job-title category, and selected penalty.

```text
approvedLimit = round_to_nearest_100(
  min(baseLimit × jobTitleCategoryMultiplier × penaltyFactor, cap)
)
```

Acceptance criteria:

- The cap is applied before rounding.
- The result is rounded to the nearest multiple of 100.
- When exactly halfway between two multiples of 100, the result is rounded down
  to the lower multiple.
- The initial fallback-cluster policy produces an unapproved decision with zero
  income and zero approved limit.

### FR-007 Classification API

The API shall expose `POST /customers/classify`.

For a valid customer, it returns `200 OK` with the normalized customer enriched
by `creditAnalysisResult`:

```json
{
  "creditAnalysisResult": {
    "cluster": "the complete matched cluster configuration object",
    "jobTitleCategory": "the complete matched job-title category configuration object",
    "monthlyIncome": 0,
    "penaltyRule": null,
    "approvedLimit": 0
  }
}
```

`cluster` contains the `approved` status. `penaltyRule` contains the complete
matched penalty-rule configuration object, or `null` when no penalty applies.

---

## 4. Rule-Configuration Requirements

Configuration is validated before the application starts. Invalid configuration
prevents startup.

- Priorities are positive integers and unique within each group of cluster,
  job-title-category, or penalty rules. Gaps are allowed.
- A cluster fallback is required; it must use `conditions.catchAll: true` and
  have the greatest numeric `priority` value, so it is evaluated last.
- An `OTHER` job-title-category fallback is required.
- Penalty rules do not require a fallback.
- `baseLimit`, `cap`, and `monthlyIncome` are non-negative.
- `cap` is greater than or equal to `baseLimit`.
- `jobTitleCategoryMultiplier` is greater than zero.
- `penaltyFactor` is between zero and one, inclusive.

---

## 5. Non-Functional Requirements

- Stateless request processing
- Configuration-driven business rules
- Testable behavior
- OpenAPI documentation

---

## 6. Constraints

- .NET 8
- REST and JSON
- No persistence, cache, or database
- No authentication
- Rules are loaded at startup and configuration is immutable for the running
  application

---

## 7. Out of Scope

- Persistence and database storage
- Authentication and authorization
- Caching
- Administrative rule-management UI
- Rule hot reload
