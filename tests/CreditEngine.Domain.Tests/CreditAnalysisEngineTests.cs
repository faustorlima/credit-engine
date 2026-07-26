using CreditEngine.Domain;

namespace CreditEngine.Domain.Tests;

public sealed class CreditAnalysisEngineTests
{
    private readonly CreditAnalysisEngine _engine = new();

    [Theory]
    [InlineData(700, 25, false, "Engineer", "CLUSTER_A")]
    [InlineData(500, 18, false, "Engineer", "CLUSTER_B")]
    [InlineData(300, 18, true, "Engineer", "CLUSTER_C")]
    [InlineData(299, 18, false, "Engineer", "CLUSTER_D")]
    public void Analyze_assigns_the_first_matching_cluster_at_boundaries(
        int score,
        int age,
        bool hasMarketDebt,
        string jobTitle,
        string expectedClusterId)
    {
        var customer = CreateCustomer(score, age, hasMarketDebt, jobTitle);

        var result = _engine.Analyze(customer, TestPolicy.Create());

        Assert.Equal(expectedClusterId, result.Cluster.Id);
    }

    [Fact]
    public void Analyze_assigns_the_fallback_cluster_when_no_specific_cluster_matches()
    {
        var result = _engine.Analyze(CreateCustomer(score: 100, age: 18, hasMarketDebt: false, "Baker"), TestPolicy.Create());

        Assert.Equal("CLUSTER_D", result.Cluster.Id);
        Assert.False(result.Cluster.Approved);
        Assert.Equal(0m, result.MonthlyIncome);
        Assert.Equal(0m, result.ApprovedLimit);
    }

    [Theory]
    [InlineData("Vice-President", "EXECUTIVE")]
    [InlineData("SÊNIOR engineer", "SENIOR_PROFESSIONAL")]
    [InlineData("software developer", "MID_PROFESSIONAL")]
    [InlineData("Junior Analyst", "MID_PROFESSIONAL")]
    [InlineData("Baker", "OTHER")]
    public void Analyze_selects_job_category_case_and_accent_insensitively_by_priority(
        string jobTitle,
        string expectedCategory)
    {
        var result = _engine.Analyze(CreateCustomer(score: 800, age: 30, hasMarketDebt: false, jobTitle), TestPolicy.Create());

        Assert.Equal(expectedCategory, result.JobTitleCategory.Name);
    }

    [Fact]
    public void Analyze_selects_the_first_matching_penalty_by_priority()
    {
        var customer = CreateCustomer(
            score: 600,
            age: 30,
            hasMarketDebt: true,
            jobTitle: "Engineer",
            marketDebtTypes: ["credit_default"]);

        var result = _engine.Analyze(customer, TestPolicy.Create());

        Assert.Equal("DEFAULT_DEBT_PENALTY", result.PenaltyRule?.RuleId);
        Assert.Equal(2_500m, result.ApprovedLimit);
    }

    [Theory]
    [InlineData("CLUSTER_A", "EXECUTIVE", 30_000)]
    [InlineData("CLUSTER_A", "SENIOR_PROFESSIONAL", 20_000)]
    [InlineData("CLUSTER_A", "MID_PROFESSIONAL", 12_000)]
    [InlineData("CLUSTER_A", "JUNIOR_PROFESSIONAL", 8_000)]
    [InlineData("CLUSTER_A", "OTHER", 10_000)]
    [InlineData("CLUSTER_B", "EXECUTIVE", 20_000)]
    [InlineData("CLUSTER_B", "SENIOR_PROFESSIONAL", 15_000)]
    [InlineData("CLUSTER_B", "MID_PROFESSIONAL", 8_000)]
    [InlineData("CLUSTER_B", "JUNIOR_PROFESSIONAL", 5_000)]
    [InlineData("CLUSTER_B", "OTHER", 6_500)]
    [InlineData("CLUSTER_C", "EXECUTIVE", 10_000)]
    [InlineData("CLUSTER_C", "SENIOR_PROFESSIONAL", 7_000)]
    [InlineData("CLUSTER_C", "MID_PROFESSIONAL", 5_000)]
    [InlineData("CLUSTER_C", "JUNIOR_PROFESSIONAL", 3_000)]
    [InlineData("CLUSTER_C", "OTHER", 4_000)]
    [InlineData("CLUSTER_D", "EXECUTIVE", 0)]
    [InlineData("CLUSTER_D", "SENIOR_PROFESSIONAL", 0)]
    [InlineData("CLUSTER_D", "MID_PROFESSIONAL", 0)]
    [InlineData("CLUSTER_D", "JUNIOR_PROFESSIONAL", 0)]
    [InlineData("CLUSTER_D", "OTHER", 0)]
    public void Analyze_looks_up_monthly_income_for_every_cluster_and_category_pair(
        string clusterId,
        string category,
        decimal expectedIncome)
    {
        var (score, hasMarketDebt) = clusterId switch
        {
            "CLUSTER_A" => (800, false),
            "CLUSTER_B" => (600, false),
            "CLUSTER_C" => (400, true),
            _ => (200, false)
        };

        var jobTitle = category switch
        {
            "EXECUTIVE" => "CEO",
            "SENIOR_PROFESSIONAL" => "Manager",
            "MID_PROFESSIONAL" => "Engineer",
            "JUNIOR_PROFESSIONAL" => "Intern",
            _ => "Baker"
        };

        var result = _engine.Analyze(CreateCustomer(score, 30, hasMarketDebt, jobTitle, hasMarketDebt ? ["mortgage"] : []), TestPolicy.Create());

        Assert.Equal(clusterId, result.Cluster.Id);
        Assert.Equal(category, result.JobTitleCategory.Name);
        Assert.Equal(expectedIncome, result.MonthlyIncome);
    }

    [Fact]
    public void Analyze_applies_base_formula_and_rounds_halfway_values_down()
    {
        var policy = TestPolicy.Create(clusterABaseLimit: 1_050m, clusterACap: 2_000m);

        var result = _engine.Analyze(CreateCustomer(800, 30, false, "Engineer"), policy);

        Assert.Equal(1_000m, result.ApprovedLimit);
    }

    [Fact]
    public void Analyze_applies_the_cap_before_rounding()
    {
        var policy = TestPolicy.Create(clusterABaseLimit: 60_000m, clusterACap: 99_950m);

        var result = _engine.Analyze(CreateCustomer(800, 30, false, "CEO"), policy);

        Assert.Equal(99_900m, result.ApprovedLimit);
    }

    [Fact]
    public void Customer_enforces_age_score_and_market_debt_invariants()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateCustomer(score: 800, age: 17, hasMarketDebt: false, "Engineer"));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateCustomer(score: 1_001, age: 30, hasMarketDebt: false, "Engineer"));
        Assert.Throws<ArgumentException>(() => CreateCustomer(score: 800, age: 30, hasMarketDebt: false, "Engineer", ["mortgage"]));
        Assert.Throws<ArgumentException>(() => CreateCustomer(score: 800, age: 30, hasMarketDebt: true, "Engineer", []));
    }

    [Fact]
    public void RulesPolicyValidator_rejects_a_fallback_that_is_not_last()
    {
        var policy = TestPolicy.Create(fallbackPriority: 3, clusterCPriority: 4);

        var exception = Assert.Throws<RulesPolicyValidationException>(() => RulesPolicyValidator.Validate(policy));

        Assert.Contains("greatest priority", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static Customer CreateCustomer(
        int score,
        int age,
        bool hasMarketDebt,
        string jobTitle,
        IReadOnlyList<string>? marketDebtTypes = null)
    {
        return new Customer(
            Guid.NewGuid(),
            "Ada Lovelace",
            age,
            score,
            hasMarketDebt,
            marketDebtTypes ?? (hasMarketDebt ? ["mortgage"] : []),
            new CustomerLocation("Sao Paulo", "SP", "Sudeste"),
            jobTitle);
    }

    private static class TestPolicy
    {
        public static RulesPolicy Create(
            decimal clusterABaseLimit = 50_000m,
            decimal clusterACap = 100_000m,
            int fallbackPriority = 4,
            int clusterCPriority = 3)
        {
            var categories = new[]
            {
                new JobTitleCategory("EXECUTIVE", 1, 2m, ["CEO", "Vice President", "VP", "Director"]),
                new JobTitleCategory("SENIOR_PROFESSIONAL", 2, 1.5m, ["Senior", "Manager"]),
                new JobTitleCategory("MID_PROFESSIONAL", 3, 1m, ["Engineer", "Analyst", "Developer"]),
                new JobTitleCategory("JUNIOR_PROFESSIONAL", 4, .7m, ["Junior", "Intern"]),
                new JobTitleCategory("OTHER", 5, .8m, [])
            };

            var clusters = new[]
            {
                new ClusterRule("CLUSTER_A", "Diamond", 1, new ClusterConditions(700, 25, 60, false), clusterABaseLimit, clusterACap, true),
                new ClusterRule("CLUSTER_B", "Gold", 2, new ClusterConditions(500, 18, 65, null, ["credit_default", "loan_default"]), 20_000m, 40_000m, true),
                new ClusterRule("CLUSTER_C", "Silver", clusterCPriority, new ClusterConditions(minScore: 300), 5_000m, 10_000m, true),
                new ClusterRule("CLUSTER_D", "Bronze", fallbackPriority, new ClusterConditions(catchAll: true), 0m, 0m, false)
            };

            var incomes = new Dictionary<string, decimal[]>
            {
                ["CLUSTER_A"] = [30_000m, 20_000m, 12_000m, 8_000m, 10_000m],
                ["CLUSTER_B"] = [20_000m, 15_000m, 8_000m, 5_000m, 6_500m],
                ["CLUSTER_C"] = [10_000m, 7_000m, 5_000m, 3_000m, 4_000m],
                ["CLUSTER_D"] = [0m, 0m, 0m, 0m, 0m]
            };

            var matrix = new IncomeMatrix(clusters.Select(cluster =>
                new MonthlyIncomeEntry(
                    cluster.Id,
                    categories.Select((category, index) => new IncomeValue(category.Name, incomes[cluster.Id][index])).ToArray())));

            var penalties = new[]
            {
                new PenaltyRule(1, "DEFAULT_DEBT_PENALTY", new PenaltyConditions(["credit_default", "loan_default"]), .5m),
                new PenaltyRule(2, "SECONDARY_DEBT_PENALTY", new PenaltyConditions(["credit_default"]), .25m)
            };

            return new RulesPolicy(clusters, categories, matrix, penalties);
        }
    }
}
