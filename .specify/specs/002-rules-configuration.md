# Rules Configuration Specification

## 1. Purpose

This document defines the configuration contract used by the
Credit Classification Engine.

The configuration is the source of truth for business rules and
is loaded by the Infrastructure layer.

## 2. Configuration Structure

The configuration contains four rule groups:

- Clusters
- Job Categories
- Income Matrix
- Penalties

## 3. Cluster Rules

Each cluster rule defines:

- Identifier
- Priority
- Conditions
- Base limit
- Limit cap
- Approval status

Rules are evaluated in ascending priority order.

The first matching rule determines the customer's cluster.

### ClusterRule

| Field | Type | Required | Description |
|---|---|---:|---|
| id | string | yes | Unique cluster identifier |
| name | string | yes | Display name |
| priority | integer | yes | Evaluation order |
| conditions | ClusterConditions | yes | Conditions that must all match |
| baseLimit | decimal | yes | Base credit limit |
| cap | decimal | yes | Maximum approved limit |
| approved | boolean | yes | Whether this cluster permits credit |

### ClusterConditions

| Field | Type | Required | Description |
|---|---|---:|---|
| minScore | integer? | no | Minimum score, inclusive |
| minAge | integer? | no | Minimum age, inclusive |
| maxAge | integer? | no | Maximum age, inclusive |
| requireMarketDebt | boolean? | no | Required value for has_market_debt |
| excludedDebtTypes | string[] | no | Debt types that must not be present |

All specified conditions are combined using AND.
Missing conditions impose no restriction.

## 4. Job Category Rules

Each category defines:

- Identifier
- Priority
- Matching patterns
- Credit multiplier

Matching is case-insensitive.

The first matching category determines the result.


Aqui ....

## 5. Income Matrix

The income matrix maps:

Cluster × Job Category → Monthly Income

## 6. Penalty Rules

Each penalty defines:

- Identifier
- Condition
- Penalty factor

Multiple applicable penalties may be combined according to
the business rules.

## 7. Configuration Validation

Configuration must be validated during application startup.

The application must fail fast when configuration is invalid.

Examples:

- Missing required rules
- Invalid numeric values
- Invalid references
- Missing fallback rules
- Invalid priorities