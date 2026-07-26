## Generating Specs

### 1 - Prompt: requirements.md

**Tool:**
ChatGPT

**What I asked:**
Following the SDD approach, generate the `requirements.md` file based on the attached PDF document.

**What happened:**
Generated an initial version containing six functional requirements.

**Final solution:**
First version of `001-requirements.md` *(will be reviewed and refined).*

---

### 2 - Prompt: architecture.md

**Tool:**
ChatGPT

**What I asked:**
Following the SDD approach, generate the content for the `architecture.md` file, assuming Clean Architecture as the architectural style and a configuration-driven approach where business rules are stored in JSON files.

**What happened:**
Generated the initial version of `002-architecture.md`.

**Final solution:**
First version of `002-architecture.md` *(will be reviewed and refined).*

---

### 3 - Prompt: implementation-plan.md

**Tool:**
ChatGPT

**What I asked:**
Following the SDD approach, what would be an appropriate implementation plan for this project?

**What happened:**
Generated the initial implementation plan, organized into incremental phases with clear objectives and deliverables.

**Final solution:**
First version of `003-implementation-plan.md` *(will be reviewed and refined).*

---

### 4 - Prompt: Customer Clusters json generation

**Tool:**
ChatGPT

**What I asked:**
Generate the full JSON for the "1. Customer Clusters" table in the image, strictly following the structure and key names from the example JSON I have uploaded.

**What happened:**
ChatGPT generated a full JSON output containing the entire "1. Customer Clusters" table data.

---

### 5 - Prompt: Job Title Categories json generation

**Tool:**
ChatGPT

**What I asked:**
Generate the full JSON for the "2. Job Title Categories" table in the image, strictly following the structure and key names from the example JSON I have uploaded.

**What happened:**
ChatGPT generated a full JSON output containing the entire "2. Job Title Categories" table data.

---

### 6 - Prompt: Monthly Income json generation

**Tool:**
ChatGPT

**What I asked:**
Generate the full JSON for the "3. Monthly Income" table in the image, strictly following the structure and key names from the example JSON I have uploaded.

**What happened:**
ChatGPT generated a full JSON output containing the entire "3. Monthly Income" table data.

## 007 — Initial specification review

**Tool:** Codex — GPT-5

**What I asked:**
> Read the four specification documents as the sole source of requirements, critically analyze `001-requirements.md`, identify ambiguities and inconsistencies without changing files or implementing code, and resolve open product decisions iteratively before updating the specification.

**What happened:**
Codex read the documents, identified 16 points requiring clarification, and presented the first one for an iterative review. No implementation or specification changes were made.

**Final solution:**
The specification review would proceed one point at a time, with decisions consolidated only after all open points had been reviewed and approved.

## 008 — Initial policy and limit formula

**Tool:** Codex — GPT-5

**What I asked:**
> Use `customer_clusters.json`, `job_title_categories.json`, `monthly_income.json`, and `penalty_rules.json` as the structure and initial values for customer classification, job categorization, income estimation, and approved-limit calculation. Use `approved_limit = round_to_nearest_100(min(base_limit × job_multiplier × penalty_factor, cluster_cap))`.

**What happened:**
Codex reviewed the four files and identified that they provide the initial policy data. It also identified a naming and placement inconsistency for the Cluster A market-debt condition.

**Final solution:**
The four JSON files were adopted as the initial policy source. The approved-limit formula applies the cap before rounding to the nearest 100.

## 009 — Cluster A condition and JSON naming

**Tool:** Codex — GPT-5

**What I asked:**
> Move the Cluster A debt rule to `conditions.hasMarketDebt: false` and change the JSON attribute names to camel case.

**What happened:**
Codex identified that the existing top-level `hasMarkedDebt` property did not align with the customer debt field or the conditions structure.

**Final solution:**
The Cluster A rule was defined as `conditions.hasMarketDebt: false`. The keys in the four policy JSON files were converted to camel case.

## 010 — Market-debt input validation

**Tool:** Codex — GPT-5

**What I asked:**
> When market debt is true, require at least one valid debt type and remove duplicates. When market debt is false, do not allow items in the debt-type list. Adopt camel case for the input contract.

**What happened:**
Codex asked for confirmation because `hasMarketData` could be confused with the debt concept already defined in the requirements.

**Final solution:**
The input fields were defined as `hasMarketDebt` and `marketDebtTypes`. When `hasMarketDebt` is true, at least one valid type is required and duplicates are removed. When it is false, `marketDebtTypes` must be empty. The penalty rule reference was aligned to `marketDebtTypes`.

## 011 — Job-category priority

**Tool:** Codex — GPT-5

**What I asked:**
> Define which job category wins when a job title matches keywords from more than one category.

**What happened:**
Codex presented alternatives based on category priority, keyword position in the title, or declaration order in the configuration.

**Final solution:**
The category with the lowest numeric priority wins. Categories are evaluated in ascending `priority` order, and `OTHER` remains the fallback.

## 012 — Job-title keyword matching

**Tool:** Codex — GPT-5

**What I asked:**
> Define how a job-title keyword should match a customer's `jobTitle`, including the handling of multi-word keywords.

**What happened:**
Codex identified that the initial keyword list includes both single words and the multi-word keyword `Vice President`.

**Final solution:**
A keyword matches when its normalized form is found in the normalized `jobTitle`. Matching is case-insensitive and ignores accents, spaces, and hyphens. This replaces the earlier decision not to ignore accents.

## 013 — Penalty-rule priority

**Tool:** Codex — GPT-5

**What I asked:**
> Define how to select a penalty when more than one penalty rule matches a customer.

**What happened:**
Codex presented alternatives for rejecting overlapping penalties, combining them, or selecting one by priority. It then asked how numeric priority represents precedence.

**Final solution:**
Only one penalty is applied. Among matching penalty rules, the rule with the lowest numeric `priority` wins.

## 014 — Credit-analysis response contract

**Tool:** Codex — GPT-5

**What I asked:**
> Return the customer entity with a `creditAnalysisResult` containing the matched cluster, matched job-title category, inferred monthly income, applied penalty rule when any, and calculated approved limit.

**What happened:**
Codex identified that the full matched configuration objects would become part of the public response contract and asked how to represent the absence of an applied penalty.

**Final solution:**
The response returns the customer plus `creditAnalysisResult`. `cluster` and `jobTitleCategory` contain the complete matched configuration objects; `monthlyIncome` and `approvedLimit` contain calculated values; and `penaltyRule` is `null` when no penalty applies.

## 015 — Invalid-request response

**Tool:** Codex — GPT-5

**What I asked:**
> Define the API response for malformed or invalid customer-classification requests.

**What happened:**
Codex presented alternatives using a single 400 response, separating 400 and 422 responses, or returning a generic error.

**Final solution:**
Any invalid request returns `400 Bad Request` as structured `application/problem+json` ProblemDetails with field-level errors. Credit rules are not executed for invalid input.

## 016 — Customer identifier format

**Tool:** Codex — GPT-5

**What I asked:**
> Define how the unique customer identifier should be validated in a stateless system with no persistence.

**What happened:**
Codex explained that cross-request uniqueness cannot be verified without persistence and presented opaque-ID and UUID alternatives.

**Final solution:**
The `id` field is required and must be a valid UUID. Any other format returns `400 Bad Request`; uniqueness across requests is not verified.

## 017 — Location validation and normalization

**Tool:** Codex — GPT-5

**What I asked:**
> Define how `location.state` and `location.region` should be validated, whether city should be validated, and how location values should be formatted in the response.

**What happened:**
Codex presented minimal validation, full geographic validation, and removal of location. It then asked how case differences should be handled.

**Final solution:**
`state` must be a valid Brazilian UF and must correspond to the supplied Brazilian region. `city` is not validated against a geographic database. Location input ignores case, and the response normalizes the UF to uppercase and the region to its canonical display form, including `Centro-Oeste`.

## 018 — Rule-configuration priority validation

**Tool:** Codex — GPT-5

**What I asked:**
> Define how priorities and fallback rules should be validated in the rule configuration.

**What happened:**
Codex presented strict unique priorities with gaps allowed, sequential priorities, and file-order tie-breaking.

**Final solution:**
Priorities are positive and unique within each rule group, evaluated in ascending order, and may have gaps. `CLUSTER_D` is the only cluster fallback, has no conditions, and has the largest priority value. `OTHER` is the only job-category fallback. Penalty rules have no fallback requirement. Invalid configuration prevents application startup.

## 019 — Explicit cluster fallback

**Tool:** Codex — GPT-5

**What I asked:**
> Standardize the Cluster D fallback using `conditions.catchAll: true` and add this attribute to the customer-clusters JSON file.

**What happened:**
Codex identified that the previous fallback convention relied on the absence of conditions, which made the intent implicit.

**Final solution:**
`CLUSTER_D` is explicitly marked with `conditions.catchAll: true`. This condition identifies the cluster fallback and supersedes the earlier no-conditions fallback convention.

## 020 — Conservative credit-limit rounding

**Tool:** Codex — GPT-5

**What I asked:**
> Define how to round an approved limit that is exactly halfway between two multiples of 100.

**What happened:**
Codex presented rounding up, rounding down, and banker’s-rounding alternatives.

**Final solution:**
When the calculated limit is exactly halfway between two multiples of 100, it is rounded down to the lower multiple to keep the credit policy conservative.

## 021 — Basic customer-field validation

**Tool:** Codex — GPT-5

**What I asked:**
> Define validation rules for required customer text fields and the valid age range.

**What happened:**
Codex presented alternatives for non-empty text, adult-only eligibility, and closed age ranges. The user defined the minimum lengths and minimum age, while city validation and whitespace treatment remained to be clarified.

**Final solution:**
`name` and `jobTitle` require at least two characters. `age` must be at least 18, with no global maximum age because cluster rules define their own age limits.

## 022 — Text-field trimming and city validation

**Tool:** Codex — GPT-5

**What I asked:**
> Confirm whether text-length validation ignores leading and trailing spaces and define the corresponding rule for city.

**What happened:**
Codex explained that trimming avoids accepting values that meet the minimum length only through surrounding spaces.

**Final solution:**
`name`, `jobTitle`, and `location.city` are trimmed before validation and must contain at least two characters after trimming.

## 023 — Normalized debt types in the response

**Tool:** Codex — GPT-5

**What I asked:**
> Define whether the response should return the original or normalized `marketDebtTypes` list after duplicate removal.

**What happened:**
Codex presented returning the normalized list or preserving the original payload list.

**Final solution:**
The response returns the normalized `marketDebtTypes` list without duplicates, preserving the order of each value’s first occurrence.

## 024 — Canonical debt-type values and validation guidance

**Tool:** Codex — GPT-5

**What I asked:**
> Define whether `marketDebtTypes` accepts case variations and ensure 400 responses inform valid values whenever possible.

**What happened:**
Codex presented strict canonical matching and case-insensitive normalization alternatives.

**Final solution:**
`marketDebtTypes` accepts only the canonical identifiers defined by the contract. For invalid enumerated values, 400 responses include the valid values whenever possible; for non-enumerated constraints, they describe the expected format or range.

## 025 — Successful classification response

**Tool:** Codex — GPT-5

**What I asked:**
> Define the HTTP status returned by a successful synchronous customer classification.

**What happened:**
Codex compared 200, 201, and 202 according to the stateless and synchronous API behavior.

**Final solution:**
A successful `POST /customers/classify` response returns `200 OK` with the enriched customer and its credit-analysis result.

## 026 — Default penalty factor

**Tool:** Codex — GPT-5

**What I asked:**
> Define the penalty factor used in the credit-limit formula when no penalty rule matches.

**What happened:**
Codex presented a neutral factor, a zero factor, and a separately configured default-factor alternative.

**Final solution:**
When no penalty rule matches, `penaltyFactor` is `1.0`, so no penalty changes the approved-limit calculation.

## 027 — Numeric configuration constraints

**Tool:** Codex — GPT-5

**What I asked:**
> Define the accepted numeric ranges for monetary values, caps, multipliers, and penalty factors in the rule configuration.

**What happened:**
Codex presented semantic per-field constraints, non-negative-only validation, and strictly positive validation.

**Final solution:**
`baseLimit`, `cap`, and `monthlyIncome` are non-negative; `cap` is at least `baseLimit`; `jobMultiplier` is greater than zero; and `penaltyFactor` is between zero and one inclusive. Invalid configuration prevents application startup.

## 028 — Normalized text fields in the response

**Tool:** Codex — GPT-5

**What I asked:**
> Define whether validated text fields should be returned with their original formatting or with surrounding spaces removed.

**What happened:**
Codex compared returning canonical trimmed values with preserving the original payload text.

**Final solution:**
`name`, `jobTitle`, and `location.city` are returned with leading and trailing spaces removed.

## 029 — Consolidated product requirements

**Tool:** Codex — GPT-5

**What I asked:**
> After reviewing and approving the consolidated decisions, update `001-requirements.md` without implementing code.

**What happened:**
Codex consolidated the approved product behavior, input validation, calculation, response contract, and rule-configuration requirements into the requirements specification. No code, architecture, implementation-plan, or business-rule specification file was changed.

**Final solution:**
`001-requirements.md` was updated to reflect the approved decisions from the specification review.

## 030 — Fallback cluster identification

**Tool:** Codex — GPT-5

**What I asked:**
> Decide whether the requirements should identify the cluster fallback by its ID or by the `conditions.catchAll` attribute.

**What happened:**
Codex explained that an ID-based reference couples the requirements to one concrete policy entry, even though fallback selection is determined by configuration.

**Final solution:**
The fallback cluster is identified exclusively by `conditions.catchAll: true`. References to `CLUSTER_D` in the requirements were replaced with the generic fallback-cluster concept while retaining the initial policy's zero-income and zero-limit outcome.

## 031 — Job-title category multiplier naming

**Tool:** Codex — GPT-5

**What I asked:**
> Resolve the naming difference between the JSON category field `multiplier` and the multiplier used in the approved-limit formula.

**What happened:**
Codex proposed retaining the compact field name in the category JSON while using a descriptive conceptual name in the formula.

**Final solution:**
The JSON field remains `multiplier`. The requirements refer to its calculation input as `jobTitleCategoryMultiplier`, which is the multiplier of the matched job-title category.

## 032 — Cluster-condition contract alignment

**Tool:** Codex — GPT-5

**What I asked:**
> Align the cluster-condition contract in `002-rules-configuration.md` with the JSON field names and define the semantics of the fallback condition.

**What happened:**
Codex compared the cluster JSON with the existing condition table and identified outdated names and the absence of an explicit `catchAll` contract.

**Final solution:**
`002-rules-configuration.md` now defines `hasMarketDebt`, `excludedMarketDebtTypes`, and `catchAll`. A `catchAll` condition is the only condition on the unique cluster fallback, which has the greatest priority.

## 033 — Fallback evaluation order wording

**Tool:** Codex — GPT-5

**What I asked:**
> Clarify the wording that determines the fallback cluster's priority and evaluation order.

**What happened:**
Codex explained that a `catchAll` rule matches every customer and therefore must be evaluated after all specific cluster rules.

**Final solution:**
The requirements and configuration contract now state that the fallback has the greatest numeric `priority` value and is evaluated last.

## 034 — Job-title category identifier

**Tool:** Codex — GPT-5

**What I asked:**
> Define whether a job-title category's `name` is its technical identifier and how the income matrix references it.

**What happened:**
Codex showed that category names such as `EXECUTIVE` are the values used by the income matrix's `category` field.

**Final solution:**
`name` is the unique canonical identifier of a job-title category, and income-matrix entries reference the category through that value.

## 035 — Job-title category fallback

**Tool:** Codex — GPT-5

**What I asked:**
> Define how the `OTHER` job-title category is represented and evaluated as the fallback.

**What happened:**
Codex compared the `OTHER` entry in the category JSON with the category-selection requirements.

**Final solution:**
`OTHER` is the unique fallback category. Its `keywords` list is empty, it has the greatest numeric `priority` value, and it is evaluated last.

## 036 — Configuration-contract boundary

**Tool:** Codex — GPT-5

**What I asked:**
> Keep `002-rules-configuration.md` limited to information additional to `001-requirements.md`, without duplicating job-title matching behavior.

**What happened:**
Codex separated the category data contract from the business behavior already defined by FR-003.

**Final solution:**
`002-rules-configuration.md` defines category fields and fallback structure, while referring to FR-003 for keyword interpretation and category selection. Duplicated matching rules were removed.

## 037 — Job-title category data contract

**Tool:** Codex — GPT-5

**What I asked:**
> Complete the job-title category data contract and require keywords for every non-fallback category.

**What happened:**
Codex replaced the incomplete category section with a field-level contract matching the current JSON structure.

**Final solution:**
Each job-title category defines `name`, `priority`, `multiplier`, and `keywords`. `keywords` is required and non-empty except for the `OTHER` fallback category.

## 038 — Income-matrix data contract

**Tool:** Codex — GPT-5

**What I asked:**
> Document the nested income-matrix JSON structure and its references without duplicating the coverage requirement in `001-requirements.md`.

**What happened:**
Codex mapped the `monthlyIncome` JSON into matrix-entry and income-value records, then separated structural references from the FR-004 coverage behavior.

**Final solution:**
The configuration contract defines `MonthlyIncomeEntry` (`clusterId`, `incomeValues`) and `IncomeValue` (`category`, `value`). Their references are validated against configured clusters and categories; FR-004 remains the source for matrix completeness.

## 039 — Domain-specific penalty condition

**Tool:** Codex — GPT-5

**What I asked:**
> Replace the generic penalty predicate with a domain-specific market-debt condition that aligns with the architecture.

**What happened:**
Codex identified that `targetField` and `operator` formed a generic mini-language despite the architecture's domain-specific-schema decision.

**Final solution:**
Penalty rules now use `conditions.marketDebtTypesAnyOf`. The configuration contract defines its structure and valid values, while FR-005 remains responsible for selecting one applicable penalty rule.

## 040 — Cluster-selection contract boundary

**Tool:** Codex — GPT-5

**What I asked:**
> Remove cluster-selection behavior duplicated from the requirements and retain only the configuration contract in `002-rules-configuration.md`.

**What happened:**
Codex identified that priority evaluation and first-match selection were already fully defined by FR-002.

**Final solution:**
The cluster configuration contract now refers to FR-002 for cluster selection and no longer repeats the runtime evaluation behavior.

## 041 — Rules-configuration purpose boundary

**Tool:** Codex — GPT-5

**What I asked:**
> Remove the infrastructure-loading concern from the purpose of `002-rules-configuration.md` and limit it to the policy contract.

**What happened:**
Codex separated the configuration contract from the architecture's responsibility for loading configuration.

**Final solution:**
`002-rules-configuration.md` now defines the logical contract that JSON policy documents must satisfy; infrastructure loading remains an architectural concern.

## 042 — Configuration consistency boundary

**Tool:** Codex — GPT-5

**What I asked:**
> Replace repeated configuration-validation behavior with structural consistency rules that belong to the configuration contract.

**What happened:**
Codex separated runtime and numeric validation from the referential and uniqueness checks required to make the policy documents coherent.

**Final solution:**
The final section of `002-rules-configuration.md` is now Configuration Consistency. It defines JSON conformance, unique identifiers, and matrix-entry uniqueness; runtime, startup, and numeric constraints remain in `001-requirements.md`.

## 043 — Policy-document convention

**Tool:** Codex — GPT-5

**What I asked:**
> Make the four JSON policy documents and their root properties explicit in the configuration contract.

**What happened:**
Codex replaced the generic rule-group list with the actual policy-document convention.

**Final solution:**
The configuration contract identifies the four JSON documents and their root properties: `customerClusters.json`/`clusters`, `jobTitleCategories.json`/`jobTitleCategories`, `monthlyIncome.json`/`monthlyIncome`, and `penaltyRules.json`/`penaltyRules`.

## 044 — Credit-limit input sources

**Tool:** Codex — GPT-5

**What I asked:**
> Make explicit which configuration fields provide the inputs to the approved-limit calculation without repeating its formula.

**What happened:**
Codex traced each formula input to its cluster, job-title-category, or penalty-rule source and distinguished income lookup from limit calculation.

**Final solution:**
The configuration contract maps `baseLimit`, `cap`, `jobTitleCategoryMultiplier`, and `penaltyFactor` to their sources. It also states that the income matrix supplies `monthlyIncome` but is not an approved-limit input.

## 045 — Explicit non-fallback cluster conditions

**Tool:** Codex — GPT-5

**What I asked:**
> Prevent a cluster without conditions from acting as an implicit fallback.

**What happened:**
Codex identified that an unconstrained non-fallback cluster would match every customer and shadow later rules.

**Final solution:**
Every non-fallback cluster must define at least one condition other than `catchAll`.

## 046 — Cluster-condition value consistency

**Tool:** Codex — GPT-5

**What I asked:**
> Define valid ranges for score and age cluster conditions, and decide whether an empty excluded-debt-type list is valid.

**What happened:**
Codex proposed input-domain ranges and non-empty exclusions. The user accepted the ranges and clarified that an empty exclusion list is meaningful.

**Final solution:**
Cluster score and age conditions are constrained to the valid input domain, with coherent age bounds. `excludedMarketDebtTypes` may be empty, meaning debt type does not restrict that cluster condition.

## 047 — Explicit fallback-cluster definition

**Tool:** Codex — GPT-5

**What I asked:**
> Make explicit that the cluster fallback is determined by `catchAll: true`, while respecting priority-based evaluation.

**What happened:**
Codex separated fallback identification in the configuration contract from the ascending-priority selection behavior in FR-002.

**Final solution:**
A cluster is the fallback if and only if its conditions contain only `catchAll: true`. Its greatest numeric priority value causes it to be evaluated after all preceding cluster rules.

## 048 — Empty penalty-rule configuration

**Tool:** Codex — GPT-5

**What I asked:**
> Decide whether the penalty-rule list may be empty and clarify the default factor when no rule applies.

**What happened:**
Codex verified that FR-005 already defines the default `penaltyFactor` as `1.0` when no penalty rule matches.

**Final solution:**
`penaltyRules` may be empty. In that case no rule is selected and FR-005 supplies the default `penaltyFactor` of `1.0`.

## 049 — Non-approved cluster values

**Tool:** Codex — GPT-5

**What I asked:**
> Decide whether every non-approved cluster must have zero limit and income values.

**What happened:**
Codex identified that the current zero values belong to the initial fallback policy and asked whether they should become a general invariant.

**Final solution:**
`approved: false` does not impose fixed `baseLimit`, `cap`, or income values. Those values remain configurable policy data.

## 050 — Closed policy-document contract

**Tool:** Codex — GPT-5

**What I asked:**
> Decide whether the policy JSON documents may contain properties not defined by the configuration contract.

**What happened:**
Codex explained that accepting unknown properties could silently ignore misspelled or unsupported configuration.

**Final solution:**
The policy-document contract is closed: objects may contain only properties declared by their corresponding `002-rules-configuration.md` contract.

## 051 — Optional policy properties

**Tool:** Codex — GPT-5

**What I asked:**
> Define whether optional configuration properties may use JSON `null`.

**What happened:**
Codex distinguished omission from nullability to remove ambiguity from optional field types.

**Final solution:**
Optional properties may be omitted. If present, they must contain a non-null value of the declared type; JSON `null` is invalid.

## 052 — Canonical policy identifiers

**Tool:** Codex — GPT-5

**What I asked:**
> Define validity and reference matching for cluster, category, and penalty identifiers.

**What happened:**
Codex identified that configuration references require an exact canonical representation to prevent ambiguous joins.

**Final solution:**
Cluster `id`, category `name`, and penalty `ruleId` are non-empty strings. References use exact, case-sensitive matching.

## 053 — Valid job-title keywords

**Tool:** Codex — GPT-5

**What I asked:**
> Prevent job-title keywords that normalize to an empty value from matching every title.

**What happened:**
Codex connected the category data contract to the existing FR-003 normalization rule and identified empty normalized keywords as a universal-match risk.

**Final solution:**
Every configured job-title keyword must normalize to a non-empty value under FR-003.

## 054 — Unique normalized job-title keywords

**Tool:** Codex — GPT-5

**What I asked:**
> Prevent duplicate job-title keywords that become identical after normalization within a category.

**What happened:**
Codex identified that normalized duplicates do not change category matching but make the configuration redundant.

**Final solution:**
Within each job-title category, normalized keyword values must be unique.

## 055 — Unique configured debt types

**Tool:** Codex — GPT-5

**What I asked:**
> Prevent repeated debt-type values in cluster exclusions and penalty conditions.

**What happened:**
Codex identified that both fields represent sets, so repeated canonical debt types do not change policy behavior.

**Final solution:**
`excludedMarketDebtTypes` and `marketDebtTypesAnyOf` require unique values.

## 056 — Explicit fallback marker retained

**Tool:** Codex — GPT-5

**What I asked:**
> Evaluate replacing the explicit fallback marker with an empty conditions object, then decide the final representation.

**What happened:**
Codex compared a shorter empty-object convention with the explicit intent conveyed by `catchAll: true`.

**Final solution:**
The explicit `catchAll: true` marker is retained. It is optional for normal clusters, mandatory for the fallback cluster, and `catchAll: false` is invalid.

## 057 — Single cluster-rule field definition

**Tool:** Codex — GPT-5

**What I asked:**
> Remove the redundant cluster-rule field summary and keep one authoritative definition.

**What happened:**
Codex identified that the summary duplicated the field table immediately below it.

**Final solution:**
The redundant summary was removed; the `ClusterRule` table is the sole definition of cluster fields.

## 058 — Cluster display-name validity

**Tool:** Codex — GPT-5

**What I asked:**
> Define whether a cluster display name may be empty.

**What happened:**
Codex identified that the display name is returned as part of the matched cluster configuration and should therefore be meaningful.

**Final solution:**
A cluster `name` must be non-empty after trimming leading and trailing spaces.

## 059 — Canonical identifier whitespace

**Tool:** Codex — GPT-5

**What I asked:**
> Decide whether technical policy identifiers may contain leading or trailing spaces.

**What happened:**
Codex distinguished display-name trimming from the exact representation needed by configuration references.

**Final solution:**
Cluster `id`, category `name`, and penalty `ruleId` must not contain leading or trailing spaces.

## 060 — Numeric-constraint boundary

**Tool:** Codex — GPT-5

**What I asked:**
> Clarify the boundary between numeric constraints in the requirements and field-specific limits in the configuration contract.

**What happened:**
Codex identified that the configuration contract now defines score and age condition ranges while the requirements define general policy-value constraints.

**Final solution:**
`001-requirements.md` remains the source for general numeric policy constraints; `002-rules-configuration.md` defines field-specific condition limits.

## 061 — Decimal JSON representation

**Tool:** Codex — GPT-5

**What I asked:**
> Define how monetary values and factors declared as decimal must be represented in the policy JSON documents.

**What happened:**
Codex identified that the contract named decimal types but did not distinguish JSON numbers from numeric strings or state decimal-scale policy.

**Final solution:**
Decimal fields must use JSON numbers, never strings. Their decimal scale is unrestricted unless another constraint applies.

## 062 — Integer JSON representation

**Tool:** Codex — GPT-5

**What I asked:**
> Define how fields declared as integer must be represented in policy JSON documents.

**What happened:**
Codex applied the same native-type principle used for decimals to priorities and cluster condition values.

**Final solution:**
Integer fields must use JSON numbers without a fractional part and must not be represented as strings.

## 063 — Boolean JSON representation

**Tool:** Codex — GPT-5

**What I asked:**
> Define how boolean policy fields must be represented in JSON.

**What happened:**
Codex applied the native-type convention to `approved`, `hasMarketDebt`, and `catchAll`.

**Final solution:**
Boolean fields must use JSON booleans rather than strings; `catchAll`, when present, remains restricted to `true`.
