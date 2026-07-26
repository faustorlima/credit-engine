using System.Text.Json.Serialization;
using CreditEngine.Api;
using CreditEngine.Application;
using CreditEngine.Domain;
using CreditEngine.Infrastructure;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IValidator<ClassifyCustomerRequest>, ClassifyCustomerRequestValidator>();

var rulesDirectory = builder.Configuration["RulesDirectory"] ?? Path.Combine(builder.Environment.ContentRootPath, "rules");
var policyLoader = new JsonPolicyLoader();
var policy = policyLoader.LoadAsync(rulesDirectory).GetAwaiter().GetResult();
RulesPolicyValidator.Validate(policy);
builder.Services.AddSingleton(policy);
builder.Services.AddSingleton<CreditAnalysisEngine>();
builder.Services.AddSingleton<ClassifyCustomer>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/customers/classify", async (ClassifyCustomerRequest request, IValidator<ClassifyCustomerRequest> validator, ClassifyCustomer useCase, CancellationToken cancellationToken) =>
{
    var validation = await validator.ValidateAsync(request, cancellationToken);
    if (!validation.IsValid)
    {
        return Results.ValidationProblem(validation.ToDictionary());
    }

    var customer = new Customer(
        Guid.Parse(request.Id!), request.Name!, request.Age!.Value, request.Score!.Value,
        request.HasMarketDebt!.Value, request.MarketDebtTypes!.Distinct(StringComparer.Ordinal).ToArray(),
        new CustomerLocation(request.Location!.City!, request.Location.State!, request.Location.Region!), request.JobTitle!);
    var analysis = useCase.Execute(customer);

    return Results.Ok(new ClassifyCustomerResponse(customer, analysis));
})
.WithName("ClassifyCustomer")
.WithOpenApi();

app.Run();

public partial class Program;
