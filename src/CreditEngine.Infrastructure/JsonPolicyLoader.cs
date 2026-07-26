using System.Text.Json;
using System.Text.Json.Serialization;
using CreditEngine.Application;
using CreditEngine.Domain;

namespace CreditEngine.Infrastructure;

public sealed class JsonPolicyLoader : IPolicyLoader
{
    private const string ClustersFileName = "customerClusters.json";
    private const string CategoriesFileName = "jobTitleCategories.json";
    private const string IncomeFileName = "monthlyIncome.json";
    private const string PenaltiesFileName = "penaltyRules.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        NumberHandling = JsonNumberHandling.Strict
    };

    public async Task<RulesPolicy> LoadAsync(string rulesDirectory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rulesDirectory))
        {
            throw new ArgumentException("The rules directory is required.", nameof(rulesDirectory));
        }

        var clusters = await ReadDocumentAsync<ClusterDocumentDto>(rulesDirectory, ClustersFileName, cancellationToken);
        var categories = await ReadDocumentAsync<JobTitleCategoryDocumentDto>(rulesDirectory, CategoriesFileName, cancellationToken);
        var income = await ReadDocumentAsync<MonthlyIncomeDocumentDto>(rulesDirectory, IncomeFileName, cancellationToken);
        var penalties = await ReadDocumentAsync<PenaltyRuleDocumentDto>(rulesDirectory, PenaltiesFileName, cancellationToken);

        return Map(clusters, categories, income, penalties);
    }

    private static async Task<TDocument> ReadDocumentAsync<TDocument>(
        string rulesDirectory,
        string fileName,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(rulesDirectory, fileName);
        var json = await File.ReadAllTextAsync(path, cancellationToken);

        try
        {
            using var document = JsonDocument.Parse(json);
            RejectNullValues(document.RootElement, fileName);
            return JsonSerializer.Deserialize<TDocument>(document.RootElement.GetRawText(), SerializerOptions)
                ?? throw new PolicyDocumentFormatException(fileName, "The document root cannot be null.");
        }
        catch (JsonException exception)
        {
            throw new PolicyDocumentFormatException(fileName, "The document is not valid according to the JSON policy schema.", exception);
        }
    }

    private static RulesPolicy Map(
        ClusterDocumentDto clusterDocument,
        JobTitleCategoryDocumentDto categoryDocument,
        MonthlyIncomeDocumentDto incomeDocument,
        PenaltyRuleDocumentDto penaltyDocument)
    {
        var clusters = RequiredList(clusterDocument.Clusters, "customerClusters.json", "clusters")
            .Select(cluster => new ClusterRule(
                Required(cluster.Id, "customerClusters.json", "clusters[].id"),
                Required(cluster.Name, "customerClusters.json", "clusters[].name"),
                cluster.Priority,
                new ClusterConditions(
                    cluster.Conditions?.MinScore,
                    cluster.Conditions?.MinAge,
                    cluster.Conditions?.MaxAge,
                    cluster.Conditions?.HasMarketDebt,
                    cluster.Conditions?.ExcludedMarketDebtTypes,
                    cluster.Conditions?.CatchAll),
                cluster.BaseLimit,
                cluster.Cap,
                cluster.Approved))
            .ToArray();

        var categories = RequiredList(categoryDocument.JobTitleCategories, "jobTitleCategories.json", "jobTitleCategories")
            .Select(category => new JobTitleCategory(
                Required(category.Name, "jobTitleCategories.json", "jobTitleCategories[].name"),
                category.Priority,
                category.Multiplier,
                RequiredList(category.Keywords, "jobTitleCategories.json", "jobTitleCategories[].keywords")))
            .ToArray();

        var incomeEntries = RequiredList(incomeDocument.MonthlyIncome, "monthlyIncome.json", "monthlyIncome")
            .Select(entry => new MonthlyIncomeEntry(
                Required(entry.ClusterId, "monthlyIncome.json", "monthlyIncome[].clusterId"),
                RequiredList(entry.IncomeValues, "monthlyIncome.json", "monthlyIncome[].incomeValues")
                    .Select(value => new IncomeValue(
                        Required(value.Category, "monthlyIncome.json", "monthlyIncome[].incomeValues[].category"),
                        value.Value))))
            .ToArray();

        var penalties = RequiredList(penaltyDocument.PenaltyRules, "penaltyRules.json", "penaltyRules")
            .Select(rule => new PenaltyRule(
                rule.Priority,
                Required(rule.RuleId, "penaltyRules.json", "penaltyRules[].ruleId"),
                new PenaltyConditions(RequiredList(rule.Conditions?.MarketDebtTypesAnyOf, "penaltyRules.json", "penaltyRules[].conditions.marketDebtTypesAnyOf")),
                rule.PenaltyFactor))
            .ToArray();

        return new RulesPolicy(clusters, categories, new IncomeMatrix(incomeEntries), penalties);
    }

    private static string Required(string? value, string fileName, string propertyName) =>
        value ?? throw new PolicyDocumentFormatException(fileName, $"Required property '{propertyName}' cannot be null.");

    private static IReadOnlyList<T> RequiredList<T>(IEnumerable<T>? values, string fileName, string propertyName) =>
        values?.ToArray() ?? throw new PolicyDocumentFormatException(fileName, $"Required property '{propertyName}' cannot be null.");

    private static void RejectNullValues(JsonElement element, string fileName)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Null:
                throw new PolicyDocumentFormatException(fileName, "JSON null values are not permitted.");
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    RejectNullValues(item, fileName);
                }

                break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    RejectNullValues(property.Value, fileName);
                }

                break;
        }
    }
}

public sealed class PolicyDocumentFormatException : Exception
{
    public PolicyDocumentFormatException(string fileName, string message, Exception? innerException = null)
        : base($"Invalid policy document '{fileName}': {message}", innerException)
    {
        FileName = fileName;
    }

    public string FileName { get; }
}
