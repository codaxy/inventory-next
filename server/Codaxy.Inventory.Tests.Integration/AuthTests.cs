using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.Auth;
using Codaxy.Inventory.Tests.Integration.Infrastructure;

namespace Codaxy.Inventory.Tests.Integration;

public class AuthTests(InventoryApplication app) : IClassFixture<InventoryApplication>
{
    private HttpClient Client() =>
        app.CreateClient(new() { AllowAutoRedirect = false, HandleCookies = true });

    [Fact]
    public async Task Options_reports_which_providers_are_configured()
    {
        var options = await Client()
            .GetFromJsonAsync<AuthEndpoints.AuthOptionsResponse>("/api/auth/options");

        Assert.NotNull(options);
        Assert.True(options.OneTimeCode);

        // No credentials are configured in a test run, so the provider is not offered at all.
        Assert.False(options.Google);
    }

    [Fact]
    public async Task Google_is_not_reachable_when_it_is_not_configured()
    {
        var response = await Client().GetAsync("/auth/google/start");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Me_is_unauthorized_before_signing_in()
    {
        var response = await Client().GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_code_signs_the_caller_in()
    {
        var client = Client();
        const string email = "someone@codaxy.com";

        var requested = await client.PostAsJsonAsync(
            "/api/auth/one-time-code/request",
            new { email }
        );
        Assert.Equal(HttpStatusCode.NoContent, requested.StatusCode);

        var code = app.Emails.LastCodeFor(email);
        Assert.NotNull(code);

        var verified = await client.PostAsJsonAsync(
            "/api/auth/one-time-code/verify",
            new { email, code }
        );
        Assert.Equal(HttpStatusCode.NoContent, verified.StatusCode);

        var me = await client.GetFromJsonAsync<AuthEndpoints.MeResponse>("/api/auth/me");
        Assert.Equal(email, me!.Email);
    }

    [Fact]
    public async Task Signing_out_ends_the_session()
    {
        var client = Client();
        const string email = "signs-out@codaxy.com";

        await client.PostAsJsonAsync("/api/auth/one-time-code/request", new { email });
        await client.PostAsJsonAsync(
            "/api/auth/one-time-code/verify",
            new { email, code = app.Emails.LastCodeFor(email) }
        );

        var signedOut = await client.PostAsync("/api/auth/sign-out", null);
        Assert.Equal(HttpStatusCode.NoContent, signedOut.StatusCode);

        var me = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Fact]
    public async Task An_address_outside_the_domain_is_told_so_and_sent_nothing()
    {
        var client = Client();
        const string email = "someone@example.com";

        var response = await client.PostAsJsonAsync(
            "/api/auth/one-time-code/request",
            new { email }
        );

        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // Told that the domain is refused, never which one would be accepted.
        Assert.Contains("not allowed", body);
        Assert.DoesNotContain("codaxy.com", body);
        Assert.Null(app.Emails.LastCodeFor(email));
    }

    [Fact]
    public async Task A_wrong_code_is_refused()
    {
        var client = Client();
        const string email = "wrong-code@codaxy.com";

        await client.PostAsJsonAsync("/api/auth/one-time-code/request", new { email });

        var response = await client.PostAsJsonAsync(
            "/api/auth/one-time-code/verify",
            new { email, code = "000000" }
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("not-an-email", "123456")]
    [InlineData("someone@codaxy.com", "12345")]
    [InlineData("someone@codaxy.com", "abcdef")]
    public async Task A_malformed_verification_is_rejected(string email, string code)
    {
        var response = await Client()
            .PostAsJsonAsync("/api/auth/one-time-code/verify", new { email, code });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task The_shell_is_served_for_a_client_route()
    {
        var response = await Client().GetAsync("/sign-in");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_checks_answer_without_signing_in(string path)
    {
        var response = await Client().GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
