using CreditEngine.Domain;

namespace CreditEngine.Application;

public interface IPolicyLoader
{
    Task<RulesPolicy> LoadAsync(string rulesDirectory, CancellationToken cancellationToken = default);
}
