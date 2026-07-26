namespace CreditEngine.Domain;

public static class RulesPolicyValidator
{
    public static void Validate(RulesPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        ValidateClusters(policy.Clusters);
        ValidateCategories(policy.JobTitleCategories);
        ValidateIncomeMatrix(policy.IncomeMatrix, policy.Clusters, policy.JobTitleCategories);
        ValidatePenaltyRules(policy.PenaltyRules);
    }

    private static void ValidateClusters(IReadOnlyList<ClusterRule> clusters)
    {
        EnsureNotEmpty(clusters, "At least one cluster rule is required.");
        EnsureNoNulls(clusters, "Cluster rules cannot contain null values.");
        ValidatePriorities(clusters.Select(cluster => cluster.Priority), "cluster rules");
        ValidateUniqueIdentifiers(clusters.Select(cluster => cluster.Id), "cluster id");

        foreach (var cluster in clusters)
        {
            ValidateIdentifier(cluster.Id, "cluster id");
            if (string.IsNullOrWhiteSpace(cluster.Name))
            {
                Invalid("Cluster name must not be empty.");
            }

            if (cluster.BaseLimit < 0 || cluster.Cap < 0 || cluster.Cap < cluster.BaseLimit)
            {
                Invalid("Cluster baseLimit and cap must be non-negative and cap must be at least baseLimit.");
            }

            ValidateClusterConditions(cluster.Conditions);
        }

        var fallbackClusters = clusters.Where(cluster => cluster.Conditions.IsFallback).ToArray();
        if (fallbackClusters.Length != 1)
        {
            Invalid("Exactly one cluster fallback with only catchAll: true is required.");
        }

        if (fallbackClusters[0].Priority != clusters.Max(cluster => cluster.Priority))
        {
            Invalid("The cluster fallback must have the greatest priority and be evaluated last.");
        }
    }

    private static void ValidateClusterConditions(ClusterConditions conditions)
    {
        if (conditions.CatchAll is false)
        {
            Invalid("catchAll, when present, must be true.");
        }

        if (conditions.CatchAll is true && !conditions.IsFallback)
        {
            Invalid("A catchAll cluster condition cannot contain other conditions.");
        }

        if (conditions.MinScore is < 0 or > 1000)
        {
            Invalid("Cluster minScore must be between 0 and 1000.");
        }

        if (conditions.MinAge is < 18 || conditions.MaxAge is < 18)
        {
            Invalid("Cluster minAge and maxAge must be at least 18.");
        }

        if (conditions.MinAge.HasValue && conditions.MaxAge.HasValue && conditions.MaxAge < conditions.MinAge)
        {
            Invalid("Cluster maxAge must be greater than or equal to minAge.");
        }

        ValidateCanonicalDebtTypes(conditions.ExcludedMarketDebtTypes, "excludedMarketDebtTypes");

        var hasSpecificCondition = conditions.MinScore.HasValue
            || conditions.MinAge.HasValue
            || conditions.MaxAge.HasValue
            || conditions.HasMarketDebt.HasValue
            || conditions.ExcludedMarketDebtTypes.Count > 0;

        if (!conditions.IsFallback && !hasSpecificCondition)
        {
            Invalid("A non-fallback cluster must contain at least one specific condition.");
        }
    }

    private static void ValidateCategories(IReadOnlyList<JobTitleCategory> categories)
    {
        EnsureNotEmpty(categories, "At least one job-title category is required.");
        EnsureNoNulls(categories, "Job-title categories cannot contain null values.");
        ValidatePriorities(categories.Select(category => category.Priority), "job-title categories");
        ValidateUniqueIdentifiers(categories.Select(category => category.Name), "job-title category name");

        foreach (var category in categories)
        {
            ValidateIdentifier(category.Name, "job-title category name");
            if (category.Multiplier <= 0)
            {
                Invalid("Job-title category multiplier must be greater than zero.");
            }

            if (string.Equals(category.Name, "OTHER", StringComparison.Ordinal))
            {
                if (category.Keywords.Count != 0)
                {
                    Invalid("The OTHER job-title category must have no keywords.");
                }
            }
            else
            {
                EnsureNotEmpty(category.Keywords, "A non-fallback job-title category requires at least one keyword.");
            }

            var normalizedKeywords = category.Keywords.Select(CreditAnalysisEngine.NormalizeKeyword).ToArray();
            if (normalizedKeywords.Any(string.IsNullOrEmpty))
            {
                Invalid("Job-title keywords must normalize to a non-empty value.");
            }

            if (normalizedKeywords.Distinct(StringComparer.Ordinal).Count() != normalizedKeywords.Length)
            {
                Invalid("Normalized job-title keywords must be unique within a category.");
            }
        }

        var otherCategories = categories.Where(category => string.Equals(category.Name, "OTHER", StringComparison.Ordinal)).ToArray();
        if (otherCategories.Length != 1)
        {
            Invalid("Exactly one OTHER job-title fallback category is required.");
        }

        if (otherCategories[0].Priority != categories.Max(category => category.Priority))
        {
            Invalid("The OTHER job-title category must have the greatest priority.");
        }
    }

    private static void ValidateIncomeMatrix(
        IncomeMatrix incomeMatrix,
        IReadOnlyList<ClusterRule> clusters,
        IReadOnlyList<JobTitleCategory> categories)
    {
        EnsureNotEmpty(incomeMatrix.Entries, "The monthly-income matrix cannot be empty.");
        EnsureNoNulls(incomeMatrix.Entries, "Monthly-income entries cannot contain null values.");
        ValidateUniqueIdentifiers(incomeMatrix.Entries.Select(entry => entry.ClusterId), "monthly-income cluster id");

        var clusterIds = clusters.Select(cluster => cluster.Id).ToHashSet(StringComparer.Ordinal);
        var categoryNames = categories.Select(category => category.Name).ToHashSet(StringComparer.Ordinal);
        if (!incomeMatrix.Entries.Select(entry => entry.ClusterId).ToHashSet(StringComparer.Ordinal).SetEquals(clusterIds))
        {
            Invalid("The monthly-income matrix must define one entry for every configured cluster.");
        }

        foreach (var entry in incomeMatrix.Entries)
        {
            ValidateIdentifier(entry.ClusterId, "monthly-income cluster id");
            EnsureNoNulls(entry.IncomeValues, "Income values cannot contain null values.");
            ValidateUniqueIdentifiers(entry.IncomeValues.Select(value => value.Category), "monthly-income category");

            if (!entry.IncomeValues.Select(value => value.Category).ToHashSet(StringComparer.Ordinal).SetEquals(categoryNames))
            {
                Invalid($"The monthly-income matrix entry for cluster '{entry.ClusterId}' must define every category exactly once.");
            }

            foreach (var incomeValue in entry.IncomeValues)
            {
                ValidateIdentifier(incomeValue.Category, "monthly-income category");
                if (incomeValue.Value < 0)
                {
                    Invalid("Monthly income must be non-negative.");
                }
            }
        }
    }

    private static void ValidatePenaltyRules(IReadOnlyList<PenaltyRule> penaltyRules)
    {
        EnsureNoNulls(penaltyRules, "Penalty rules cannot contain null values.");
        ValidatePriorities(penaltyRules.Select(rule => rule.Priority), "penalty rules");
        ValidateUniqueIdentifiers(penaltyRules.Select(rule => rule.RuleId), "penalty rule id");

        foreach (var rule in penaltyRules)
        {
            ValidateIdentifier(rule.RuleId, "penalty rule id");
            if (rule.PenaltyFactor is < 0 or > 1)
            {
                Invalid("Penalty factor must be between zero and one.");
            }

            EnsureNotEmpty(rule.Conditions.MarketDebtTypesAnyOf, "Penalty conditions require at least one market debt type.");
            ValidateCanonicalDebtTypes(rule.Conditions.MarketDebtTypesAnyOf, "marketDebtTypesAnyOf");
        }
    }

    private static void ValidatePriorities(IEnumerable<int> priorities, string groupName)
    {
        var values = priorities.ToArray();
        if (values.Any(value => value <= 0) || values.Distinct().Count() != values.Length)
        {
            Invalid($"Priorities for {groupName} must be positive and unique.");
        }
    }

    private static void ValidateUniqueIdentifiers(IEnumerable<string> identifiers, string identifierName)
    {
        var values = identifiers.ToArray();
        if (values.Distinct(StringComparer.Ordinal).Count() != values.Length)
        {
            Invalid($"{identifierName} values must be unique.");
        }
    }

    private static void ValidateIdentifier(string value, string identifierName)
    {
        if (string.IsNullOrWhiteSpace(value) || !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            Invalid($"{identifierName} must be non-empty and must not contain leading or trailing spaces.");
        }
    }

    private static void ValidateCanonicalDebtTypes(IReadOnlyList<string> debtTypes, string propertyName)
    {
        if (debtTypes.Any(debtType => !Customer.IsCanonicalMarketDebtType(debtType)))
        {
            Invalid($"{propertyName} must contain only canonical market debt types.");
        }

        if (debtTypes.Distinct(StringComparer.Ordinal).Count() != debtTypes.Count)
        {
            Invalid($"{propertyName} values must be unique.");
        }
    }

    private static void EnsureNotEmpty<T>(IReadOnlyCollection<T> values, string message)
    {
        if (values.Count == 0)
        {
            Invalid(message);
        }
    }

    private static void EnsureNoNulls<T>(IEnumerable<T> values, string message)
    {
        if (values.Any(value => value is null))
        {
            Invalid(message);
        }
    }

    private static void Invalid(string message) => throw new RulesPolicyValidationException(message);
}

public sealed class RulesPolicyValidationException : ArgumentException
{
    public RulesPolicyValidationException(string message)
        : base(message)
    {
    }
}
