# Credit Engine — Key Interactions

This file records the interactions that materially shaped the specification, architecture, implementation, and verification of Credit Engine.

## 1. Initial requirements

**Tool:** ChatGPT

**What I asked:**
> Following the SDD approach, generate the requirements file from the Credit Engine challenge.

**What happened:**
The initial functional requirements were drafted and became the basis for the later specification review.

**Final solution:**
The requirements specification was established as the product source of truth.

## 2. Initial architecture

**Tool:** ChatGPT

**What I asked:**
> Define an SDD architecture using Clean Architecture and JSON-based business rules.

**What happened:**
The initial architecture separated business policy from application code.

**Final solution:**
Clean Architecture and configuration-driven rules were adopted as the core design approach.

## 3. Incremental implementation plan

**Tool:** ChatGPT

**What I asked:**
> Create an implementation plan for the project.

**What happened:**
The work was organized into incremental phases with deliverables and quality gates.

**Final solution:**
The plan became the basis for the seven implementation phases.

## 4. Specification review

**Tool:** Codex - GPT-5

**What I asked:**
> Read the specification documents as the only source of requirements, identify ambiguities, and resolve product decisions before implementation.

**What happened:**
The first reading exposed unresolved assumptions in the policy, validation, response, and configuration contracts. Instead of implementing them as guesses, the review addressed the decisions one at a time and incorporated only the approved outcomes into the specifications.

**Final solution:**
The specifications were refined before implementation began.

## 5. Policy model and credit-limit formula

**Tool:** Codex - GPT-5

**What I asked:**
> Use the four policy JSON files as the initial source for classification, job categorization, income, penalties, and approved-limit calculation.

**What happened:**
The initial data needed a clear boundary from executable rules and a precise calculation order. The policy values were retained as configuration, while the formula was clarified to apply the cap before conservative rounding.

**Final solution:**
The approved limit is calculated from the matched cluster, category multiplier, penalty factor, cap, and conservative rounding.

## 6. Customer validation and normalization

**Tool:** Codex - GPT-5

**What I asked:**
> Define validation and normalization for identifiers, text fields, debt types, and Brazilian locations.

**What happened:**
The initial contract did not distinguish invalid values from values that could be normalized. The review separated rejection of invalid identifiers, debt types, and locations from trimming text and removing duplicate debt types before rule evaluation.

**Final solution:**
Invalid requests return field-level ProblemDetails, while valid requests use normalized customer data.

## 7. Cluster selection and fallback

**Tool:** Codex - GPT-5

**What I asked:**
> Define cluster priority, fallback representation, and evaluation order.

**What happened:**
The initial convention treated a cluster with empty conditions as the fallback. That was ambiguous because it could also describe an incomplete rule, so the representation was revised to make the fallback explicit while preserving priority-based evaluation.

**Final solution:**
Cluster rules are evaluated by ascending priority; exactly one last-priority fallback uses `conditions.catchAll: true`.

## 8. Job-title categorization

**Tool:** Codex - GPT-5

**What I asked:**
> Define job-title keyword matching, priority, and fallback behavior.

**What happened:**
The initial keyword list included overlapping and multi-word values, so a simple literal comparison would have produced inconsistent results. The matching normalization and priority rule were defined after considering case, accents, spaces, hyphens, and competing categories.

**Final solution:**
Keywords match case-insensitively while ignoring accents, spaces, and hyphens; the lowest-priority-number match wins and `OTHER` is the fallback.

## 9. Penalties and conservative rounding

**Tool:** Codex - GPT-5

**What I asked:**
> Define penalty selection and rounding behavior for approved limits.

**What happened:**
The policy could match more than one penalty and the formula did not define an exact half-way rounding outcome. The alternatives were reviewed and a deterministic, conservative rule was selected for both cases.

**Final solution:**
Only the first matching penalty applies, no penalty uses factor `1.0`, and exact half-way values round down to the lower hundred.

## 10. HTTP response contract

**Tool:** Codex - GPT-5

**What I asked:**
> Define successful and invalid classification API responses.

**What happened:**
The response needed to expose the matched logical policy data without leaking JSON DTOs, and invalid requests needed a single predictable HTTP contract. The API and Domain boundaries were clarified before defining the payload and ProblemDetails behavior.

**Final solution:**
`POST /customers/classify` returns `200 OK` with the normalized customer and `creditAnalysisResult`; invalid input returns `400 application/problem+json`.

## 11. Consolidated requirements

**Tool:** Codex - GPT-5

**What I asked:**
> Consolidate the approved product decisions into the requirements specification without implementing code.

**What happened:**
The individual decisions were integrated into a single coherent specification.

**Final solution:**
`001-requirements.md` became the consolidated product contract.

## 12. Closed policy-document contract

**Tool:** Codex - GPT-5

**What I asked:**
> Define the logical contract and validation rules for the policy JSON documents.

**What happened:**
Accepting loosely shaped JSON could silently ignore misspellings, invalid numeric representations, or broken references. The contract was tightened iteratively around strict types, no unknown properties or nulls, canonical identifiers, priorities, references, and matrix coverage.

**Final solution:**
`002-rules-configuration.md` defines a closed, validated policy-document contract.

## 13. API-owned policy and immutable snapshot

**Tool:** Codex - GPT-5

**What I asked:**
> Define policy-file ownership, startup loading, immutability, and two-stage validation.

**What happened:**
Reading policy files during each request would have coupled request processing to storage and made the active policy mutable. The design moved file ownership to API deployment, JSON parsing to Infrastructure, and logical validation to Domain during startup.

**Final solution:**
API owns `rules/`; Infrastructure loads and maps them; Domain validates invariants; requests consume an immutable `RulesPolicy` snapshot.

## 14. Layer responsibilities and dependencies

**Tool:** Codex - GPT-5

**What I asked:**
> Define the Domain, Application, Infrastructure, and API boundaries and their allowed compile-time dependencies.

**What happened:**
The initial layer description was conceptual and did not prevent accidental references. It was converted into separate projects with explicit compile-time dependency rules so JSON and HTTP concerns could not leak into Domain.

**Final solution:**
Application depends on Domain; Infrastructure depends on Application and Domain; API composes Application and Infrastructure; Domain has no outer-layer dependency.

## 15. Test strategy and expected outputs

**Tool:** Codex - GPT-5

**What I asked:**
> Define mandatory unit and integration coverage and create the six expected classification outputs.

**What happened:**
The original test strategy named required coverage but lacked the six expected HTTP outputs needed to verify the full contract. The fixture was created and the risks were mapped to Domain, Infrastructure, Application, and API tests.

**Final solution:**
The repository includes fixture-driven API integration tests and supporting unit and contract tests runnable through `dotnet test`.

## 16. Implementation plan refinement

**Tool:** Codex - GPT-5

**What I asked:**
> Align implementation phases with the approved architecture, incremental tests, operational documentation, and final quality gates.

**What happened:**
The plan was reorganized around a four-project foundation, Domain first, then Application, Infrastructure, API, documentation, and review.

**Final solution:**
`004-implementation-plan.md` defines the approved incremental delivery sequence.

## 17. Four-project foundation implementation

**Tool:** Codex - GPT-5

**What I asked:**
> Create the four projects under `src/` and enforce the architecture through solution references.

**What happened:**
The existing single-project starting point could not enforce the intended architecture. The solution was reorganized into production and test projects, rules were moved into API-owned content, and OpenAPI was enabled as a composition-root baseline.

**Final solution:**
The solution has enforceable API, Application, Domain, and Infrastructure project boundaries.

## 18. Domain implementation

**Tool:** Codex - GPT-5

**What I asked:**
> Execute the Domain implementation phase.

**What happened:**
Pure policy models, customer invariants, the validator, rule engine, and comprehensive Domain tests were implemented.

**Final solution:**
Domain contains the business rules without ASP.NET, JSON, FluentValidation, or external package dependencies.

## 19. Application implementation

**Tool:** Codex - GPT-5

**What I asked:**
> Execute the Application implementation phase.

**What happened:**
The policy-loading port and classification use case were implemented and tested.

**Final solution:**
Application orchestrates the Domain engine using an injected read-only policy snapshot.

## 20. Infrastructure implementation

**Tool:** Codex - GPT-5

**What I asked:**
> Execute the Infrastructure policy-loading phase.

**What happened:**
The implementation separated strict JSON-format checks and private DTOs from the Domain logical validator and its policy invariants. This avoided making Infrastructure responsible for HTTP concerns or for the business meaning of a valid policy.

**Final solution:**
Infrastructure implements `IPolicyLoader` without depending on API.

## 21. REST API implementation

**Tool:** Codex - GPT-5

**What I asked:**
> Execute the REST API phase.

**What happened:**
The first package restore for FluentValidation and ASP.NET Core integration testing was blocked by the default network sandbox, so explicit NuGet approval was required. The API initially returned Domain objects directly, which exposed implementation-only condition members and omitted a required `null` penalty field; the response mapping was then adjusted against the six fixtures.

**Final solution:**
The API validates policy at startup and returns the specified output for all six fixture cases.

## 22. Operational documentation

**Tool:** Codex - GPT-5

**What I asked:**
> Execute the operational documentation phase.

**What happened:**
Build, test, run, policy-file, and architecture documentation was added, along with implementation learnings.

**Final solution:**
A new developer can use the README to build, test, run, and maintain the solution.

## 23. Final review

**Tool:** Codex - GPT-5

**What I asked:**
> Execute the final review phase.

**What happened:**
Build, tests, fixtures, ProblemDetails, OpenAPI, dependency direction, and repository hygiene were reviewed.

**Final solution:**
The quality gates pass, with automated coverage for the implemented architecture and API behavior.

## 24. Prompt-history curation

**Tool:** Codex - GPT-5

**What I asked:**
> Review the prompt history, keep only interactions with material impact, and renumber the remaining entries sequentially.

**What happened:**
The detailed interaction log was condensed into the milestones that shaped the product and implementation.

**Final solution:**
This file is now a curated, sequential record rather than a full prompt log.
