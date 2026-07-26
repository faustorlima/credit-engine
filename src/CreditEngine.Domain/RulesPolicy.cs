using System.Collections.ObjectModel;

namespace CreditEngine.Domain;

public sealed class RulesPolicy
{
    public RulesPolicy(
        IEnumerable<ClusterRule> clusters,
        IEnumerable<JobTitleCategory> jobTitleCategories,
        IncomeMatrix incomeMatrix,
        IEnumerable<PenaltyRule> penaltyRules)
    {
        Clusters = Snapshot(clusters, nameof(clusters));
        JobTitleCategories = Snapshot(jobTitleCategories, nameof(jobTitleCategories));
        IncomeMatrix = incomeMatrix ?? throw new ArgumentNullException(nameof(incomeMatrix));
        PenaltyRules = Snapshot(penaltyRules, nameof(penaltyRules));
    }

    public IReadOnlyList<ClusterRule> Clusters { get; }

    public IReadOnlyList<JobTitleCategory> JobTitleCategories { get; }

    public IncomeMatrix IncomeMatrix { get; }

    public IReadOnlyList<PenaltyRule> PenaltyRules { get; }

    internal static IReadOnlyList<T> Snapshot<T>(IEnumerable<T> values, string parameterName) =>
        new ReadOnlyCollection<T>((values ?? throw new ArgumentNullException(parameterName)).ToArray());
}

public sealed class ClusterRule
{
    public ClusterRule(
        string id,
        string name,
        int priority,
        ClusterConditions conditions,
        decimal baseLimit,
        decimal cap,
        bool approved)
    {
        Id = id;
        Name = name;
        Priority = priority;
        Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
        BaseLimit = baseLimit;
        Cap = cap;
        Approved = approved;
    }

    public string Id { get; }

    public string Name { get; }

    public int Priority { get; }

    public ClusterConditions Conditions { get; }

    public decimal BaseLimit { get; }

    public decimal Cap { get; }

    public bool Approved { get; }
}

public sealed class ClusterConditions
{
    public ClusterConditions(
        int? minScore = null,
        int? minAge = null,
        int? maxAge = null,
        bool? hasMarketDebt = null,
        IEnumerable<string>? excludedMarketDebtTypes = null,
        bool? catchAll = null)
    {
        MinScore = minScore;
        MinAge = minAge;
        MaxAge = maxAge;
        HasMarketDebt = hasMarketDebt;
        ExcludedMarketDebtTypes = RulesPolicy.Snapshot(excludedMarketDebtTypes ?? [], nameof(excludedMarketDebtTypes));
        CatchAll = catchAll;
    }

    public int? MinScore { get; }

    public int? MinAge { get; }

    public int? MaxAge { get; }

    public bool? HasMarketDebt { get; }

    public IReadOnlyList<string> ExcludedMarketDebtTypes { get; }

    public bool? CatchAll { get; }

    public bool IsFallback => CatchAll is true
        && MinScore is null
        && MinAge is null
        && MaxAge is null
        && HasMarketDebt is null
        && ExcludedMarketDebtTypes.Count == 0;
}

public sealed class JobTitleCategory
{
    public JobTitleCategory(string name, int priority, decimal multiplier, IEnumerable<string> keywords)
    {
        Name = name;
        Priority = priority;
        Multiplier = multiplier;
        Keywords = RulesPolicy.Snapshot(keywords, nameof(keywords));
    }

    public string Name { get; }

    public int Priority { get; }

    public decimal Multiplier { get; }

    public IReadOnlyList<string> Keywords { get; }
}

public sealed class IncomeMatrix
{
    public IncomeMatrix(IEnumerable<MonthlyIncomeEntry> entries)
    {
        Entries = RulesPolicy.Snapshot(entries, nameof(entries));
    }

    public IReadOnlyList<MonthlyIncomeEntry> Entries { get; }

    public decimal GetMonthlyIncome(string clusterId, string category)
    {
        var entry = Entries.SingleOrDefault(value => string.Equals(value.ClusterId, clusterId, StringComparison.Ordinal));
        if (entry is null)
        {
            throw new InvalidOperationException($"Income matrix does not contain cluster '{clusterId}'.");
        }

        var income = entry.IncomeValues.SingleOrDefault(value => string.Equals(value.Category, category, StringComparison.Ordinal));
        return income?.Value ?? throw new InvalidOperationException($"Income matrix does not contain category '{category}' for cluster '{clusterId}'.");
    }
}

public sealed class MonthlyIncomeEntry
{
    public MonthlyIncomeEntry(string clusterId, IEnumerable<IncomeValue> incomeValues)
    {
        ClusterId = clusterId;
        IncomeValues = RulesPolicy.Snapshot(incomeValues, nameof(incomeValues));
    }

    public string ClusterId { get; }

    public IReadOnlyList<IncomeValue> IncomeValues { get; }
}

public sealed class IncomeValue
{
    public IncomeValue(string category, decimal value)
    {
        Category = category;
        Value = value;
    }

    public string Category { get; }

    public decimal Value { get; }
}

public sealed class PenaltyRule
{
    public PenaltyRule(int priority, string ruleId, PenaltyConditions conditions, decimal penaltyFactor)
    {
        Priority = priority;
        RuleId = ruleId;
        Conditions = conditions ?? throw new ArgumentNullException(nameof(conditions));
        PenaltyFactor = penaltyFactor;
    }

    public int Priority { get; }

    public string RuleId { get; }

    public PenaltyConditions Conditions { get; }

    public decimal PenaltyFactor { get; }
}

public sealed class PenaltyConditions
{
    public PenaltyConditions(IEnumerable<string> marketDebtTypesAnyOf)
    {
        MarketDebtTypesAnyOf = RulesPolicy.Snapshot(marketDebtTypesAnyOf, nameof(marketDebtTypesAnyOf));
    }

    public IReadOnlyList<string> MarketDebtTypesAnyOf { get; }
}
