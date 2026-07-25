# Implementation Plan

## Goal

Build a maintainable, configuration-driven Credit Engine using Clean Architecture.
Each phase must produce a working increment that can be compiled and tested.

---

# Phase 1 – Foundation

## Goal

Establish the project structure.

## Deliverables

- ASP.NET Core Web API
- Clean Architecture project structure
- Dependency Injection configured
- Build pipeline
- OpenAPI enabled
- Basic health check

## Definition of Done

- Solution builds successfully
- Application starts successfully
- OpenAPI endpoint is available

---

# Phase 2 – Domain Model

## Goal

Model the business domain independently of infrastructure.

## Deliverables

- Customer
- Cluster
- JobCategory
- CreditDecision
- Rule models
- Domain interfaces

## Definition of Done

- Domain contains no ASP.NET dependencies
- Domain compiles independently
- Initial unit tests pass

---

# Phase 3 – Configuration-Driven Rule Engine

## Goal

Implement the core business engine.

## Deliverables

- JsonRuleProvider
- RuleLoader
- ClusterEvaluator
- JobCategoryEvaluator
- IncomeLookup
- PenaltyEvaluator
- CreditLimitCalculator

## Definition of Done

- Rules are loaded from JSON
- Cluster evaluation follows priority order
- Job matching works
- Credit limit calculation is complete
- Unit tests cover all business rules

---

# Phase 4 – Application Layer

## Goal

Orchestrate the business workflow.

## Deliverables

- ClassificationService
- Request/Response models
- Mapping

## Definition of Done

The application service executes:

Customer
→ Cluster
→ Job Category
→ Income
→ Penalty
→ Credit Limit

---

# Phase 5 – REST API

## Goal

Expose the engine through HTTP.

## Deliverables

- POST /customers/classify
- Validation
- ProblemDetails
- OpenAPI documentation

## Definition of Done

The API returns the expected output contract.

---

# Phase 6 – Testing

## Goal

Validate business correctness.

## Deliverables

### Unit Tests

- Cluster assignment
- Boundary conditions
- Job matching
- Income lookup
- Penalties
- Credit calculation

### Integration Tests

- Successful request
- Invalid payload
- Sample customers
- Error responses

## Definition of Done

All required tests pass.

---

# Phase 7 – Documentation

## Goal

Document architecture and design decisions.

## Deliverables

- README
- Architecture
- AI Journey

## Definition of Done

A new developer can understand the project without reading the source code.

---

# Phase 8 – Final Review

## Goal

Prepare the repository for evaluation.

## Checklist

- Build passes
- Tests pass
- Formatting applied
- No dead code
- No TODOs
- Documentation reviewed