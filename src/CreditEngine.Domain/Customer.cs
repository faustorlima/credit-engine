using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace CreditEngine.Domain;

public sealed class Customer
{
    private static readonly HashSet<string> ValidMarketDebtTypes =
    [
        "credit_card",
        "personal_loan",
        "mortgage",
        "credit_default",
        "loan_default"
    ];

    public Customer(
        Guid id,
        string name,
        int age,
        int score,
        bool hasMarketDebt,
        IEnumerable<string> marketDebtTypes,
        CustomerLocation location,
        string jobTitle)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Customer id cannot be empty.", nameof(id));
        }

        if (age < 18)
        {
            throw new ArgumentOutOfRangeException(nameof(age), "Customer age must be at least 18.");
        }

        if (score is < 0 or > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(score), "Customer score must be between 0 and 1000.");
        }

        var debtTypes = (marketDebtTypes ?? throw new ArgumentNullException(nameof(marketDebtTypes))).ToArray();
        if (debtTypes.Any(type => !ValidMarketDebtTypes.Contains(type)))
        {
            throw new ArgumentException("Customer market debt types must use canonical values.", nameof(marketDebtTypes));
        }

        if (debtTypes.Distinct(StringComparer.Ordinal).Count() != debtTypes.Length)
        {
            throw new ArgumentException("Customer market debt types must be normalized without duplicates.", nameof(marketDebtTypes));
        }

        if (hasMarketDebt != (debtTypes.Length > 0))
        {
            throw new ArgumentException("hasMarketDebt must be consistent with marketDebtTypes.", nameof(marketDebtTypes));
        }

        Id = id;
        Name = ValidateAndTrimText(name, nameof(name));
        Age = age;
        Score = score;
        HasMarketDebt = hasMarketDebt;
        MarketDebtTypes = new ReadOnlyCollection<string>(debtTypes);
        Location = location ?? throw new ArgumentNullException(nameof(location));
        JobTitle = ValidateAndTrimText(jobTitle, nameof(jobTitle));
    }

    public Guid Id { get; }

    public string Name { get; }

    public int Age { get; }

    public int Score { get; }

    public bool HasMarketDebt { get; }

    public IReadOnlyList<string> MarketDebtTypes { get; }

    public CustomerLocation Location { get; }

    public string JobTitle { get; }

    internal static bool IsCanonicalMarketDebtType(string value) => ValidMarketDebtTypes.Contains(value);

    private static string ValidateAndTrimText(string value, string parameterName)
    {
        var trimmed = value?.Trim() ?? throw new ArgumentNullException(parameterName);
        if (trimmed.Length < 2)
        {
            throw new ArgumentException("Value must contain at least two characters after trimming.", parameterName);
        }

        return trimmed;
    }
}

public sealed class CustomerLocation
{
    private static readonly IReadOnlyDictionary<string, string[]> StatesByRegion =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Norte"] = ["AC", "AP", "AM", "PA", "RO", "RR", "TO"],
            ["Nordeste"] = ["AL", "BA", "CE", "MA", "PB", "PE", "PI", "RN", "SE"],
            ["Centro-Oeste"] = ["DF", "GO", "MT", "MS"],
            ["Sudeste"] = ["ES", "MG", "RJ", "SP"],
            ["Sul"] = ["PR", "RS", "SC"]
        };

    public CustomerLocation(string city, string state, string region)
    {
        City = ValidateAndTrimCity(city);
        State = (state ?? throw new ArgumentNullException(nameof(state))).Trim().ToUpperInvariant();
        Region = ResolveRegion(region);

        if (!StatesByRegion[Region].Contains(State, StringComparer.Ordinal))
        {
            throw new ArgumentException("State must be a valid Brazilian UF that belongs to the supplied region.", nameof(state));
        }
    }

    public string City { get; }

    public string State { get; }

    public string Region { get; }

    private static string ValidateAndTrimCity(string city)
    {
        var trimmed = city?.Trim() ?? throw new ArgumentNullException(nameof(city));
        if (trimmed.Length < 2)
        {
            throw new ArgumentException("City must contain at least two characters after trimming.", nameof(city));
        }

        return trimmed;
    }

    private static string ResolveRegion(string region)
    {
        var normalized = new string((region ?? throw new ArgumentNullException(nameof(region)))
            .Normalize(NormalizationForm.FormD)
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());

        return normalized switch
        {
            "NORTE" => "Norte",
            "NORDESTE" => "Nordeste",
            "CENTROOESTE" => "Centro-Oeste",
            "SUDESTE" => "Sudeste",
            "SUL" => "Sul",
            _ => throw new ArgumentException("Region must be a valid Brazilian region.", nameof(region))
        };
    }
}
