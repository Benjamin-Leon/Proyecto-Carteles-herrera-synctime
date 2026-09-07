using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LetrerosHerreraSYNCTIMEapi.Tests;

public sealed class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    private readonly IConfiguration testConfiguration = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .Build();

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    // Estas pruebas ejercen la API completa: pipeline, JWT, EF Core y base real.
    [Fact]
    public async Task OpenApi_ReturnsSuccess()
    {
        // OpenAPI debe existir y anunciar que la API acepta JWT Bearer.
        var response = await client.GetAsync("/openapi/v1.json");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Bearer", body);
    }

    [Fact]
    public async Task Products_ArePublic()
    {
        var response = await client.GetAsync("/api/v1/productos/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Clients_RequireAuthentication()
    {
        var response = await client.GetAsync("/api/v1/clientes/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@losherrera.com",
            password = "password-invalido"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_AndAuthenticatedEndpoint_Work()
    {
        // Esta prueba recorre login, generacion de token y uso de una ruta protegida.
        var email = testConfiguration["TestCredentials:Email"];
        var password = testConfiguration["TestCredentials:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var loginJson = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginJson.RootElement.GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var meResponse = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
    }
}