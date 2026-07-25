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
