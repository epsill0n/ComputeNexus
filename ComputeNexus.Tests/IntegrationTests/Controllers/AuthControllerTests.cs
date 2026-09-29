using ComputeNexus.Tests.IntegrationTests.Fixtures;
using ComputeNexus.Web.Request;

namespace ComputeNexus.Tests.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactoryFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        // arrange
        var request = new RegisterRequest("test@example.com", "Password123!");

        // act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsStringAsync();
        result.Should().Contain("User registered successfully");
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // arrange - register first user
        var request = new RegisterRequest("existing@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // act - try to register same email again
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // arrange - register user first
        var registerRequest = new RegisterRequest("login@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // act - login
        var loginRequest = new LoginRequest("login@example.com", "Password123!");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // arrange - Register user first
        var registerRequest = new RegisterRequest("invalid@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // act - Login with wrong password
        var loginRequest = new LoginRequest("invalid@example.com", "WrongPassword!");
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}