using CreditEngine.Application;
using CreditEngine.Domain;

namespace CreditEngine.Application.Tests;

public sealed class ClassifyCustomerTests
{
    [Fact]
    public void Execute_orchestrates_the_domain_engine_with_the_registered_policy_snapshot()
    {
        var policy = CreatePolicy();
        var useCase = new ClassifyCustomer(new CreditAnalysisEngine(), policy);

        var analysis = useCase.Execute(CreateCustomer("Chief Technology Officer"));

        Assert.Equal("CLUSTER_A", analysis.Cluster.Id);
        Assert.Equal("EXECUTIVE", analysis.JobTitleCategory.Name);
        Assert.Equal(15_000m, analysis.MonthlyIncome);
        Assert.Equal(2_400m, analysis.ApprovedLimit);
    }

    [Fact]
    public async Task IPolicyLoader_returns_the_policy_snapshot_for_the_supplied_rules_directory()
    {
        var policy = CreatePolicy();
        var loader = new StubPolicyLoader(policy);

        var loadedPolicy = await loader.LoadAsync("rules", CancellationToken.None);

        Assert.Same(policy, loadedPolicy);
        Assert.Equal("rules", loader.RequestedRulesDirectory);
    }

    private static Customer CreateCustomer(string jobTitle) =>
        new(
            Guid.NewGuid(),
            "Ada Lovelace",
            30,
            800,
            false,
            [],
            new CustomerLocation("Sao Paulo", "SP", "Sudeste"),
            jobTitle);

    private static RulesPolicy CreatePolicy()
    {
        var cluster = new ClusterRule(
            "CLUSTER_A",
            "Diamond",
            1,
            new ClusterConditions(catchAll: true),
            1_200m,
            5_000m,
            true);
        var executive = new JobTitleCategory("EXECUTIVE", 1, 2m, ["Chief"]);
        var other = new JobTitleCategory("OTHER", 2, .8m, []);
        var policy = new RulesPolicy(
            [cluster],
            [executive, other],
            new IncomeMatrix(
            [
                new MonthlyIncomeEntry(
                    cluster.Id,
                    [
                        new IncomeValue(executive.Name, 15_000m),
                        new IncomeValue(other.Name, 8_000m)
                    ])
            ]),
            []);

        RulesPolicyValidator.Validate(policy);
        return policy;
    }

    private sealed class StubPolicyLoader(RulesPolicy policy) : IPolicyLoader
    {
        public string? RequestedRulesDirectory { get; private set; }

        public Task<RulesPolicy> LoadAsync(string rulesDirectory, CancellationToken cancellationToken = default)
        {
            RequestedRulesDirectory = rulesDirectory;
            return Task.FromResult(policy);
        }
    }
}
