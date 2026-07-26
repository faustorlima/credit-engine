using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CreditEngine.Api.IntegrationTests;

public sealed class ClassificationEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Post_classify_returns_every_expected_fixture_response()
    {
        using var fixture = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "fixtures", "expected-output.json")));
        var client = factory.CreateClient();

        foreach (var testCase in fixture.RootElement.GetProperty("cases").EnumerateArray())
        {
            using var response = await client.PostAsJsonAsync("/customers/classify", testCase.GetProperty("request"));
            var responseJson = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            var expectedJson = JsonNode.Parse(testCase.GetProperty("expectedResponse").GetRawText());

            Assert.Equal(testCase.GetProperty("expectedStatus").GetInt32(), (int)response.StatusCode);
            Assert.True(JsonNode.DeepEquals(expectedJson, responseJson), $"{testCase.GetProperty("name").GetString()}\nExpected: {expectedJson}\nActual: {responseJson}");
        }
    }

    [Fact]
    public async Task Post_classify_with_invalid_input_returns_problem_details()
    {
        using var response = await factory.CreateClient().PostAsJsonAsync("/customers/classify", new { });
        var problem = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.StartsWith("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        Assert.NotNull(problem["errors"]);
    }

    [Fact]
    public async Task OpenApi_document_is_available()
    {
        using var response = await factory.CreateClient().GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
