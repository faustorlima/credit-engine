using CreditEngine.Domain;
using CreditEngine.Infrastructure;

namespace CreditEngine.Infrastructure.Tests;

public sealed class JsonPolicyLoaderTests
{
    [Fact]
    public async Task LoadAsync_maps_valid_policy_documents_to_the_logical_domain_model()
    {
        using var rules = new TemporaryRulesDirectory();
        rules.WriteValidDocuments();

        var policy = await new JsonPolicyLoader().LoadAsync(rules.Path);

        Assert.Single(policy.Clusters);
        Assert.Equal("CLUSTER_D", policy.Clusters[0].Id);
        Assert.Single(policy.JobTitleCategories);
        Assert.Equal("OTHER", policy.JobTitleCategories[0].Name);
        Assert.Equal(0m, policy.IncomeMatrix.GetMonthlyIncome("CLUSTER_D", "OTHER"));
        Assert.Empty(policy.PenaltyRules);
        RulesPolicyValidator.Validate(policy);
    }

    [Fact]
    public async Task LoadAsync_rejects_invalid_json_syntax()
    {
        using var rules = new TemporaryRulesDirectory();
        rules.WriteValidDocuments(clusters: "{ \"clusters\": [ }");

        await Assert.ThrowsAsync<PolicyDocumentFormatException>(() => new JsonPolicyLoader().LoadAsync(rules.Path));
    }

    [Fact]
    public async Task LoadAsync_rejects_values_with_an_incorrect_json_type()
    {
        using var rules = new TemporaryRulesDirectory();
        rules.WriteValidDocuments(clusters: Clusters.Replace("\"priority\": 1", "\"priority\": \"1\"", StringComparison.Ordinal));

        await Assert.ThrowsAsync<PolicyDocumentFormatException>(() => new JsonPolicyLoader().LoadAsync(rules.Path));
    }

    [Fact]
    public async Task LoadAsync_rejects_null_values()
    {
        using var rules = new TemporaryRulesDirectory();
        rules.WriteValidDocuments(categories: Categories.Replace("\"name\": \"OTHER\"", "\"name\": null", StringComparison.Ordinal));

        await Assert.ThrowsAsync<PolicyDocumentFormatException>(() => new JsonPolicyLoader().LoadAsync(rules.Path));
    }

    [Fact]
    public async Task LoadAsync_rejects_unknown_properties()
    {
        using var rules = new TemporaryRulesDirectory();
        rules.WriteValidDocuments(penalties: Penalties.Replace("[]", "[], \"unexpected\": true", StringComparison.Ordinal));

        await Assert.ThrowsAsync<PolicyDocumentFormatException>(() => new JsonPolicyLoader().LoadAsync(rules.Path));
    }

    [Theory]
    [InlineData("duplicate-cluster")]
    [InlineData("unknown-income-reference")]
    [InlineData("invalid-fallback")]
    [InlineData("invalid-condition-structure")]
    public async Task Mapped_policy_is_rejected_when_the_logical_contract_is_invalid(string scenario)
    {
        using var rules = new TemporaryRulesDirectory();
        rules.WriteValidDocuments(
            clusters: scenario switch
            {
                "duplicate-cluster" => DuplicateClusters,
                "invalid-fallback" => InvalidFallback,
                "invalid-condition-structure" => InvalidConditionStructure,
                _ => Clusters
            },
            income: scenario == "unknown-income-reference" ? UnknownIncomeReference : Income);

        await Assert.ThrowsAsync<RulesPolicyValidationException>(async () =>
        {
            var policy = await new JsonPolicyLoader().LoadAsync(rules.Path);
            RulesPolicyValidator.Validate(policy);
        });
    }

    private sealed class TemporaryRulesDirectory : IDisposable
    {
        public TemporaryRulesDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"credit-engine-rules-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void WriteValidDocuments(
            string? clusters = null,
            string? categories = null,
            string? income = null,
            string? penalties = null)
        {
            File.WriteAllText(System.IO.Path.Combine(Path, "customerClusters.json"), clusters ?? Clusters);
            File.WriteAllText(System.IO.Path.Combine(Path, "jobTitleCategories.json"), categories ?? Categories);
            File.WriteAllText(System.IO.Path.Combine(Path, "monthlyIncome.json"), income ?? Income);
            File.WriteAllText(System.IO.Path.Combine(Path, "penaltyRules.json"), penalties ?? Penalties);
        }

        public void Dispose()
        {
            Directory.Delete(Path, recursive: true);
        }
    }

    private const string Clusters = """
        { "clusters": [{ "priority": 1, "id": "CLUSTER_D", "name": "Bronze", "conditions": { "catchAll": true }, "baseLimit": 0, "cap": 0, "approved": false }] }
        """;

    private const string Categories = """
        { "jobTitleCategories": [{ "priority": 1, "name": "OTHER", "multiplier": 1, "keywords": [] }] }
        """;

    private const string Income = """
        { "monthlyIncome": [{ "clusterId": "CLUSTER_D", "incomeValues": [{ "category": "OTHER", "value": 0 }] }] }
        """;

    private const string Penalties = """
        { "penaltyRules": [] }
        """;

    private const string DuplicateClusters = """
        { "clusters": [
          { "priority": 1, "id": "CLUSTER_D", "name": "Bronze", "conditions": { "catchAll": true }, "baseLimit": 0, "cap": 0, "approved": false },
          { "priority": 2, "id": "CLUSTER_D", "name": "Bronze 2", "conditions": { "minScore": 500 }, "baseLimit": 1, "cap": 1, "approved": true }
        ] }
        """;

    private const string UnknownIncomeReference = """
        { "monthlyIncome": [{ "clusterId": "UNKNOWN", "incomeValues": [{ "category": "OTHER", "value": 0 }] }] }
        """;

    private const string InvalidFallback = """
        { "clusters": [{ "priority": 1, "id": "CLUSTER_D", "name": "Bronze", "conditions": { "catchAll": false }, "baseLimit": 0, "cap": 0, "approved": false }] }
        """;

    private const string InvalidConditionStructure = """
        { "clusters": [{ "priority": 1, "id": "CLUSTER_D", "name": "Bronze", "conditions": { "catchAll": true, "minScore": 0 }, "baseLimit": 0, "cap": 0, "approved": false }] }
        """;
}
