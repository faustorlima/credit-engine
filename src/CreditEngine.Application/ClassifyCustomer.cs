using CreditEngine.Domain;

namespace CreditEngine.Application;

public sealed class ClassifyCustomer
{
    private readonly CreditAnalysisEngine _creditAnalysisEngine;
    private readonly RulesPolicy _rulesPolicy;

    public ClassifyCustomer(CreditAnalysisEngine creditAnalysisEngine, RulesPolicy rulesPolicy)
    {
        _creditAnalysisEngine = creditAnalysisEngine ?? throw new ArgumentNullException(nameof(creditAnalysisEngine));
        _rulesPolicy = rulesPolicy ?? throw new ArgumentNullException(nameof(rulesPolicy));
    }

    public CreditAnalysis Execute(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        return _creditAnalysisEngine.Analyze(customer, _rulesPolicy);
    }
}
