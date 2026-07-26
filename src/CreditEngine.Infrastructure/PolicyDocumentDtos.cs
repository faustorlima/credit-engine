using System.Text.Json.Serialization;

namespace CreditEngine.Infrastructure;

internal sealed class ClusterDocumentDto
{
    [JsonPropertyName("clusters")]
    public required List<ClusterRuleDto> Clusters { get; init; }
}

internal sealed class ClusterRuleDto
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("priority")]
    public required int Priority { get; init; }

    [JsonPropertyName("conditions")]
    public required ClusterConditionsDto Conditions { get; init; }

    [JsonPropertyName("baseLimit")]
    public required decimal BaseLimit { get; init; }

    [JsonPropertyName("cap")]
    public required decimal Cap { get; init; }

    [JsonPropertyName("approved")]
    public required bool Approved { get; init; }
}

internal sealed class ClusterConditionsDto
{
    [JsonPropertyName("minScore")]
    public int? MinScore { get; init; }

    [JsonPropertyName("minAge")]
    public int? MinAge { get; init; }

    [JsonPropertyName("maxAge")]
    public int? MaxAge { get; init; }

    [JsonPropertyName("hasMarketDebt")]
    public bool? HasMarketDebt { get; init; }

    [JsonPropertyName("excludedMarketDebtTypes")]
    public List<string>? ExcludedMarketDebtTypes { get; init; }

    [JsonPropertyName("catchAll")]
    public bool? CatchAll { get; init; }
}

internal sealed class JobTitleCategoryDocumentDto
{
    [JsonPropertyName("jobTitleCategories")]
    public required List<JobTitleCategoryDto> JobTitleCategories { get; init; }
}

internal sealed class JobTitleCategoryDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("priority")]
    public required int Priority { get; init; }

    [JsonPropertyName("multiplier")]
    public required decimal Multiplier { get; init; }

    [JsonPropertyName("keywords")]
    public required List<string> Keywords { get; init; }
}

internal sealed class MonthlyIncomeDocumentDto
{
    [JsonPropertyName("monthlyIncome")]
    public required List<MonthlyIncomeEntryDto> MonthlyIncome { get; init; }
}

internal sealed class MonthlyIncomeEntryDto
{
    [JsonPropertyName("clusterId")]
    public required string ClusterId { get; init; }

    [JsonPropertyName("incomeValues")]
    public required List<IncomeValueDto> IncomeValues { get; init; }
}

internal sealed class IncomeValueDto
{
    [JsonPropertyName("category")]
    public required string Category { get; init; }

    [JsonPropertyName("value")]
    public required decimal Value { get; init; }
}

internal sealed class PenaltyRuleDocumentDto
{
    [JsonPropertyName("penaltyRules")]
    public required List<PenaltyRuleDto> PenaltyRules { get; init; }
}

internal sealed class PenaltyRuleDto
{
    [JsonPropertyName("priority")]
    public required int Priority { get; init; }

    [JsonPropertyName("ruleId")]
    public required string RuleId { get; init; }

    [JsonPropertyName("conditions")]
    public required PenaltyConditionsDto Conditions { get; init; }

    [JsonPropertyName("penaltyFactor")]
    public required decimal PenaltyFactor { get; init; }
}

internal sealed class PenaltyConditionsDto
{
    [JsonPropertyName("marketDebtTypesAnyOf")]
    public required List<string> MarketDebtTypesAnyOf { get; init; }
}
