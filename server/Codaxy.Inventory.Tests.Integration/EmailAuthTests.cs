using Codaxy.Inventory.Web.Email;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>A relay that takes mail only from an account, as Gmail does.</summary>
public class EmailAuthTests : IAsyncLifetime
{
    private readonly IContainer mailpit = new ContainerBuilder("axllent/mailpit:latest")
        .WithEnvironment("MP_SMTP_AUTH", "inventory:secret")
        .WithEnvironment("MP_SMTP_AUTH_ALLOW_INSECURE", "true")
        .WithPortBinding(1025, true)
        .WithPortBinding(8025, true)
        .WithWaitStrategy(
            Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r => r.ForPort(8025).ForPath("/readyz"))
        )
        .Build();

    public Task InitializeAsync() => mailpit.StartAsync();

    public Task DisposeAsync() => mailpit.DisposeAsync().AsTask();

    private SmtpEmailSender Sender(string userName, string password) =>
        new(
            Options.Create(
                new SmtpOptions
                {
                    Host = mailpit.Hostname,
                    Port = mailpit.GetMappedPublicPort(1025),
                    From = "noreply@codaxy.com",
                    FromName = "Codaxy Inventory",
                    UserName = userName,
                    Password = password,
                }
            )
        );

    [Fact]
    public async Task Signs_in_with_the_account_and_sends_under_the_sender_s_name()
    {
        await Sender("inventory", "secret")
            .SendAsync(["someone@codaxy.com"], "Expiry alert: 1 warranty ending", "Text");

        using var http = new HttpClient
        {
            BaseAddress = new Uri($"http://{mailpit.Hostname}:{mailpit.GetMappedPublicPort(8025)}"),
        };
        var messages = await http.GetStringAsync("/api/v1/messages");

        Assert.Contains("\"Name\":\"Codaxy Inventory\"", messages);
        Assert.Contains("Expiry alert: 1 warranty ending", messages);
    }

    [Fact]
    public async Task Without_an_account_the_relay_refuses_it() =>
        await Assert.ThrowsAnyAsync<Exception>(() =>
            Sender("", "").SendAsync(["someone@codaxy.com"], "Refused", "Text")
        );

    [Fact]
    public async Task A_wrong_password_is_refused() =>
        await Assert.ThrowsAsync<AuthenticationException>(() =>
            Sender("inventory", "wrong").SendAsync(["someone@codaxy.com"], "Refused", "Text")
        );
}
