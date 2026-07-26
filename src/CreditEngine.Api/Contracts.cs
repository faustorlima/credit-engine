using CreditEngine.Domain;
using FluentValidation;
using FluentValidation.Results;
using System.Text.Json.Serialization;

namespace CreditEngine.Api;

public sealed class ClassifyCustomerRequest
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public int? Age { get; init; }
    public int? Score { get; init; }
    public bool? HasMarketDebt { get; init; }
    public List<string>? MarketDebtTypes { get; init; }
    public LocationRequest? Location { get; init; }
    public string? JobTitle { get; init; }
}

public sealed class LocationRequest { public string? City { get; init; } public string? State { get; init; } public string? Region { get; init; } }

public sealed class ClassifyCustomerResponse
{
    public ClassifyCustomerResponse(Customer customer, CreditAnalysis analysis)
    {
        Id = customer.Id; Name = customer.Name; Age = customer.Age; Score = customer.Score; HasMarketDebt = customer.HasMarketDebt;
        MarketDebtTypes = customer.MarketDebtTypes; Location = customer.Location; JobTitle = customer.JobTitle; CreditAnalysisResult = new CreditAnalysisResponse(analysis);
    }
    public Guid Id { get; } public string Name { get; } public int Age { get; } public int Score { get; } public bool HasMarketDebt { get; }
    public IReadOnlyList<string> MarketDebtTypes { get; } public CustomerLocation Location { get; } public string JobTitle { get; } public CreditAnalysisResponse CreditAnalysisResult { get; }
}

public sealed class CreditAnalysisResponse
{
    public CreditAnalysisResponse(CreditAnalysis analysis) { Cluster = new ClusterResponse(analysis.Cluster); JobTitleCategory = new CategoryResponse(analysis.JobTitleCategory); MonthlyIncome = analysis.MonthlyIncome; PenaltyRule = analysis.PenaltyRule is null ? null : new PenaltyResponse(analysis.PenaltyRule); ApprovedLimit = analysis.ApprovedLimit; }
    public ClusterResponse Cluster { get; } public CategoryResponse JobTitleCategory { get; } public decimal MonthlyIncome { get; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)] public PenaltyResponse? PenaltyRule { get; }
    public decimal ApprovedLimit { get; }
}
public sealed class ClusterResponse { public ClusterResponse(ClusterRule rule) { Priority=rule.Priority; Id=rule.Id; Name=rule.Name; Conditions=ConditionsMap(rule.Conditions); BaseLimit=rule.BaseLimit; Cap=rule.Cap; Approved=rule.Approved; } public int Priority{get;} public string Id{get;} public string Name{get;} public Dictionary<string,object> Conditions{get;} public decimal BaseLimit{get;} public decimal Cap{get;} public bool Approved{get;} private static Dictionary<string,object> ConditionsMap(ClusterConditions c) { var d=new Dictionary<string,object>(); if(c.MinScore is not null)d["minScore"]=c.MinScore.Value; if(c.MinAge is not null)d["minAge"]=c.MinAge.Value; if(c.MaxAge is not null)d["maxAge"]=c.MaxAge.Value; if(c.HasMarketDebt is not null)d["hasMarketDebt"]=c.HasMarketDebt.Value; if(c.ExcludedMarketDebtTypes.Count>0)d["excludedMarketDebtTypes"]=c.ExcludedMarketDebtTypes; if(c.CatchAll is not null)d["catchAll"]=c.CatchAll.Value; return d; } }
public sealed class CategoryResponse { public CategoryResponse(JobTitleCategory value){Priority=value.Priority;Name=value.Name;Multiplier=value.Multiplier;Keywords=value.Keywords;} public int Priority{get;} public string Name{get;} public decimal Multiplier{get;} public IReadOnlyList<string> Keywords{get;} }
public sealed class PenaltyResponse { public PenaltyResponse(PenaltyRule value){Priority=value.Priority;RuleId=value.RuleId;Conditions=new Dictionary<string,object>{{"marketDebtTypesAnyOf",value.Conditions.MarketDebtTypesAnyOf}};PenaltyFactor=value.PenaltyFactor;} public int Priority{get;} public string RuleId{get;} public Dictionary<string,object> Conditions{get;} public decimal PenaltyFactor{get;} }

public sealed class ClassifyCustomerRequestValidator : AbstractValidator<ClassifyCustomerRequest>
{
    private static readonly HashSet<string> DebtTypes = ["credit_card", "personal_loan", "mortgage", "credit_default", "loan_default"];
    private static readonly Dictionary<string, string> RegionByState = new(StringComparer.Ordinal)
    {
        ["AC"]="Norte",["AP"]="Norte",["AM"]="Norte",["PA"]="Norte",["RO"]="Norte",["RR"]="Norte",["TO"]="Norte",
        ["AL"]="Nordeste",["BA"]="Nordeste",["CE"]="Nordeste",["MA"]="Nordeste",["PB"]="Nordeste",["PE"]="Nordeste",["PI"]="Nordeste",["RN"]="Nordeste",["SE"]="Nordeste",
        ["DF"]="Centro-Oeste",["GO"]="Centro-Oeste",["MT"]="Centro-Oeste",["MS"]="Centro-Oeste",["ES"]="Sudeste",["MG"]="Sudeste",["RJ"]="Sudeste",["SP"]="Sudeste",["PR"]="Sul",["RS"]="Sul",["SC"]="Sul"
    };
    public ClassifyCustomerRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty().Must(value => Guid.TryParse(value, out _)).WithMessage("id must be a valid UUID.");
        RuleFor(x => x.Name).Must(ValidText).WithMessage("name must contain at least 2 characters after trimming.");
        RuleFor(x => x.JobTitle).Must(ValidText).WithMessage("jobTitle must contain at least 2 characters after trimming.");
        RuleFor(x => x.Age).NotNull().GreaterThanOrEqualTo(18);
        RuleFor(x => x.Score).NotNull().InclusiveBetween(0, 1000);
        RuleFor(x => x.HasMarketDebt).NotNull();
        RuleFor(x => x.MarketDebtTypes).NotNull().Must((request, values) => values is not null && (request.HasMarketDebt == (values.Count > 0)) && values.All(DebtTypes.Contains)).WithMessage("marketDebtTypes must use canonical values and be consistent with hasMarketDebt.");
        RuleFor(x => x.Location).NotNull();
        When(x => x.Location is not null, () => {
            RuleFor(x => x.Location!.City).Must(ValidText).WithMessage("location.city must contain at least 2 characters after trimming.");
            RuleFor(x => x.Location!.State).Must(state => state is not null && RegionByState.ContainsKey(state.Trim().ToUpperInvariant())).WithMessage("location.state must be a valid Brazilian UF.");
            RuleFor(x => x.Location).Must(location => location is not null && location.State is not null && location.Region is not null && RegionByState.TryGetValue(location.State.Trim().ToUpperInvariant(), out var region) && NormalizeRegion(location.Region) == region).WithMessage("location.region must correspond to location.state.");
        });
    }
    private static bool ValidText(string? value) => value?.Trim().Length >= 2;
    private static string NormalizeRegion(string value) => value.Trim().Replace(" ", "").Replace("-", "", StringComparison.Ordinal).ToUpperInvariant() switch { "NORTE"=>"Norte", "NORDESTE"=>"Nordeste", "CENTROOESTE"=>"Centro-Oeste", "SUDESTE"=>"Sudeste", "SUL"=>"Sul", _=>string.Empty };
}

internal static class ValidationResultExtensions { public static Dictionary<string, string[]> ToDictionary(this ValidationResult result) => result.Errors.GroupBy(error => error.PropertyName).ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()); }
