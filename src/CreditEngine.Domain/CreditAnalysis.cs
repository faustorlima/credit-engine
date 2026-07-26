namespace CreditEngine.Domain;

public sealed class CreditAnalysis
{
    public CreditAnalysis(
        ClusterRule cluster,
        JobTitleCategory jobTitleCategory,
        decimal monthlyIncome,
        PenaltyRule? penaltyRule,
        decimal approvedLimit)
    {
        Cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
        JobTitleCategory = jobTitleCategory ?? throw new ArgumentNullException(nameof(jobTitleCategory));
        MonthlyIncome = monthlyIncome;
        PenaltyRule = penaltyRule;
        ApprovedLimit = approvedLimit;
    }

    public ClusterRule Cluster { get; }

    public JobTitleCategory JobTitleCategory { get; }

    public decimal MonthlyIncome { get; }

    public PenaltyRule? PenaltyRule { get; }

    public decimal ApprovedLimit { get; }
}
