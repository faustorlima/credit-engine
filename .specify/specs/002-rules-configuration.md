# Rules Configuration Specification

## 1. Purpose

This document defines the logical configuration contract that the Credit
Classification Engine's JSON policy documents must satisfy.

## 2. Configuration Structure

The policy package contains four JSON documents:

| Document | Root property | Rule group |
|---|---|---|
| `customerClusters.json` | `clusters` | Cluster rules |
| `jobTitleCategories.json` | `jobTitleCategories` | Job-title category rules |
| `monthlyIncome.json` | `monthlyIncome` | Income matrix |
| `penaltyRules.json` | `penaltyRules` | Penalty rules |

## 3. Cluster Rules

Cluster-selection behavior is defined by FR-002, Customer classification, in
`001-requirements.md`.

### ClusterRule

| Field | Type | Required | Description |
|---|---|---:|---|
| id | string | yes | Unique cluster identifier |
| name | string | yes | Display name. Must be non-empty after trimming leading and trailing spaces. |
| priority | integer | yes | Evaluation order |
| conditions | ClusterConditions | yes | Conditions that must all match |
| baseLimit | decimal | yes | Base credit limit |
| cap | decimal | yes | Maximum approved limit |
| approved | boolean | yes | Whether this cluster permits credit |

`approved: false` does not impose fixed values on `baseLimit`, `cap`, or the
income-matrix values associated with the cluster; those values remain
policy-configurable.

### ClusterConditions

| Field | Type | Required | Description |
|---|---|---:|---|
| minScore | integer? | no | Minimum score, inclusive |
| minAge | integer? | no | Minimum age, inclusive |
| maxAge | integer? | no | Maximum age, inclusive |
| hasMarketDebt | boolean? | no | Required value for the customer's `hasMarketDebt` field |
| excludedMarketDebtTypes | string[] | no | Market-debt types that must not be present in the customer's `marketDebtTypes` list |
| catchAll | boolean? | no | When present, must be `true`; identifies the cluster fallback. |

All specified conditions are combined using AND.
Missing conditions impose no restriction.

A cluster is the fallback if and only if its `conditions` object contains only
`catchAll: true`. `catchAll: false` is invalid. The fallback has the greatest
numeric `priority` value and is therefore evaluated last according to FR-002.

A non-fallback cluster must specify at least one condition other than
`catchAll`.

When specified, `minScore` must be between 0 and 1000, inclusive. `minAge` and
`maxAge` must be at least 18; when both are specified, `maxAge` must be greater
than or equal to `minAge`.

`excludedMarketDebtTypes` may be empty, which imposes no debt-type restriction.
When it contains values, each value must be a canonical market-debt type defined
by the input contract in `001-requirements.md`. An empty
`excludedMarketDebtTypes` list does not satisfy the non-fallback condition
requirement by itself. Its values must be unique.

## 4. Job Category Rules

| Field | Type | Required | Description |
|---|---|---:|---|
| name | string | yes | Unique canonical category identifier. Income-matrix entries reference the category using this value. |
| priority | integer | yes | Numeric value used for category ordering. |
| multiplier | decimal | yes | Category factor supplied to the approved-limit calculation. |
| keywords | string[] | yes | Job-title keywords. The list must contain at least one value unless the category is `OTHER`; every keyword must normalize to a non-empty value under FR-003. |

Within a category, normalized keyword values must be unique.

`OTHER` is the unique fallback category. It must have an empty `keywords` list
and the greatest numeric `priority` value.

The interpretation of `keywords` and category-selection behavior are defined by
FR-003, Job-title categorization, in `001-requirements.md`.

## 5. Income Matrix

The `monthlyIncome` configuration contains an array of `MonthlyIncomeEntry`
objects.

### MonthlyIncomeEntry

| Field | Type | Required | Description |
|---|---|---:|---|
| clusterId | string | yes | Identifier of the referenced cluster. |
| incomeValues | IncomeValue[] | yes | Income values for job-title categories in that cluster. |

### IncomeValue

| Field | Type | Required | Description |
|---|---|---:|---|
| category | string | yes | `name` of the referenced job-title category. |
| value | decimal | yes | Monthly income for the cluster/category pair. |

`clusterId` must reference a configured cluster, and `category` must reference a
configured job-title category. Matrix coverage requirements are defined by
FR-004, Monthly income estimation, in `001-requirements.md`.

## 6. Penalty Rules

The `penaltyRules` configuration contains an array of `PenaltyRule` objects.
The array may be empty. When no penalty rule is selected, including when the
array is empty, the default `penaltyFactor` is defined by FR-005,
Penalty selection, in `001-requirements.md`.

| Field | Type | Required | Description |
|---|---|---:|---|
| priority | integer | yes | Numeric value used for penalty-rule ordering. |
| ruleId | string | yes | Unique canonical penalty-rule identifier. |
| conditions | PenaltyConditions | yes | Domain-specific conditions that determine whether the rule applies. |
| penaltyFactor | decimal | yes | Factor supplied to the approved-limit calculation. |

### PenaltyConditions

| Field | Type | Required | Description |
|---|---|---:|---|
| marketDebtTypesAnyOf | string[] | yes | Non-empty list of canonical market-debt types. The condition applies when the customer's `marketDebtTypes` list contains at least one listed value. |

Each `marketDebtTypesAnyOf` value must be a canonical market-debt type defined
by the input contract in `001-requirements.md`. Penalty-rule selection behavior
is defined by FR-005, Penalty selection, in `001-requirements.md`. Values in
`marketDebtTypesAnyOf` must be unique.

## 7. Credit-Limit Input Sources

This section maps configuration fields to the conceptual inputs of the
approved-limit calculation. The calculation itself is defined by FR-006,
Credit-limit calculation, in `001-requirements.md`.

| Calculation input | Configuration source |
|---|---|
| baseLimit | `baseLimit` of the matched cluster rule. |
| cap | `cap` of the matched cluster rule. |
| jobTitleCategoryMultiplier | `multiplier` of the matched job-title category. |
| penaltyFactor | `penaltyFactor` of the selected penalty rule; its default when no rule is selected is defined by FR-005. |

The income matrix provides `monthlyIncome` for the analysis result; it is not
an input to the approved-limit calculation.

## 8. Configuration Consistency

The policy documents must be valid JSON and conform to the field contracts in
this document.

- An optional property may be omitted. When present, it must have the declared
  non-null type; JSON `null` is not permitted.
- Fields declared as `decimal` must be represented as JSON numbers, not JSON
  strings. Their decimal scale is unrestricted unless another constraint states
  otherwise.
- Fields declared as `integer` must be represented as JSON numbers with no
  fractional part, never as JSON strings.
- Fields declared as `boolean` must be represented as JSON booleans, never as
  JSON strings.
- Each object may contain only the properties declared by its corresponding
  contract in this document; additional properties are not permitted.
- Cluster `id`, category `name`, and penalty `ruleId` values are non-empty
  strings with no leading or trailing spaces. References to them use exact,
  case-sensitive matching.
- Cluster `id` values, category `name` values, and penalty `ruleId` values are
  unique within their respective rule groups.
- A `MonthlyIncomeEntry` references each configured cluster at most once.
- Within a `MonthlyIncomeEntry`, an `IncomeValue` references each job-title
  category at most once.
- Referential-integrity and condition-specific constraints are defined in the
  relevant rule-group sections above.

Runtime validation, startup behavior, and general numeric constraints for
policy values are defined in `001-requirements.md`. Field-specific condition
limits are defined in this document.
