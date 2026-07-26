using System.Globalization;
using System.Text;

namespace CreditEngine.Domain;

public sealed class CreditAnalysisEngine
{
    public CreditAnalysis Analyze(Customer customer, RulesPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(policy);

        var cluster = policy.Clusters
            .OrderBy(rule => rule.Priority)
            .FirstOrDefault(rule => Matches(rule.Conditions, customer))
            ?? throw new InvalidOperationException("No cluster rule matched the customer.");

        var jobTitleCategory = policy.JobTitleCategories
            .OrderBy(category => category.Priority)
            .FirstOrDefault(category => category.Keywords.Any(keyword => JobTitleMatches(keyword, customer.JobTitle)))
            ?? policy.JobTitleCategories.SingleOrDefault(category => string.Equals(category.Name, "OTHER", StringComparison.Ordinal))
            ?? throw new InvalidOperationException("No job-title category matched the customer.");

        var penaltyRule = policy.PenaltyRules
            .OrderBy(rule => rule.Priority)
            .FirstOrDefault(rule => rule.Conditions.MarketDebtTypesAnyOf
                .Any(debtType => customer.MarketDebtTypes.Contains(debtType, StringComparer.Ordinal)));

        var monthlyIncome = policy.IncomeMatrix.GetMonthlyIncome(cluster.Id, jobTitleCategory.Name);
        var penaltyFactor = penaltyRule?.PenaltyFactor ?? 1m;
        var approvedLimit = CalculateApprovedLimit(cluster.BaseLimit, jobTitleCategory.Multiplier, penaltyFactor, cluster.Cap);

        return new CreditAnalysis(cluster, jobTitleCategory, monthlyIncome, penaltyRule, approvedLimit);
    }

    private static bool Matches(ClusterConditions conditions, Customer customer)
    {
        if (conditions.IsFallback)
        {
            return true;
        }

        return (!conditions.MinScore.HasValue || customer.Score >= conditions.MinScore.Value)
            && (!conditions.MinAge.HasValue || customer.Age >= conditions.MinAge.Value)
            && (!conditions.MaxAge.HasValue || customer.Age <= conditions.MaxAge.Value)
            && (!conditions.HasMarketDebt.HasValue || customer.HasMarketDebt == conditions.HasMarketDebt.Value)
            && !conditions.ExcludedMarketDebtTypes.Any(debtType => customer.MarketDebtTypes.Contains(debtType, StringComparer.Ordinal));
    }

    private static bool JobTitleMatches(string keyword, string jobTitle) =>
        NormalizeKeyword(jobTitle).Contains(NormalizeKeyword(keyword), StringComparison.Ordinal);

    private static decimal CalculateApprovedLimit(decimal baseLimit, decimal multiplier, decimal penaltyFactor, decimal cap)
    {
        var cappedAmount = Math.Min(baseLimit * multiplier * penaltyFactor, cap);
        var lowerMultiple = Math.Floor(cappedAmount / 100m) * 100m;
        var remainder = cappedAmount - lowerMultiple;

        return remainder > 50m ? lowerMultiple + 100m : lowerMultiple;
    }

    internal static string NormalizeKeyword(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new string(value
            .Normalize(NormalizationForm.FormD)
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .Where(character => !char.IsWhiteSpace(character) && character != '-')
            .Select(char.ToUpperInvariant)
            .ToArray());
    }
}
