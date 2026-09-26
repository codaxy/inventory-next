using Codaxy.Inventory.Web.Auth;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Options;

namespace Codaxy.Inventory.Tests.Integration;

/// <summary>
/// Against the same relay the development stack runs, because the part worth proving is that a
/// message actually leaves — a mocked client proves only that the code calls itself.
/// </summary>
public class EmailTests : IAsyncLifetime
{
    private readonly IContainer mailpit = new ContainerBuilder("axllent/mailpit:latest")
        .WithPortBinding(1025, true)
        .WithPortBinding(8025, true)
        .WithWaitStrategy(
            Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r => r.ForPort(8025).ForPath("/readyz"))
        )
        .Build();

    public Task InitializeAsync() => mailpit.StartAsync();

    public Task DisposeAsync() => mailpit.DisposeAsync().AsTask();

    [Fact]
    public async Task Sends_a_message_the_relay_accepts()
    {
        var sender = new SmtpEmailSender(
            Options.Create(
                new SmtpOptions
                {
                    Host = mailpit.Hostname,
                    Port = mailpit.GetMappedPublicPort(1025),
                    From = "inventory@codaxy.com",
                }
            )
        );

        await sender.SendAsync("someone@codaxy.com", "Your code", "Your sign-in code is 123456.");

        using var http = new HttpClient
        {
            BaseAddress = new Uri($"http://{mailpit.Hostname}:{mailpit.GetMappedPublicPort(8025)}"),
        };

        var messages = await http.GetStringAsync("/api/v1/messages");

        Assert.Contains("someone@codaxy.com", messages);
        Assert.Contains("Your code", messages);
    }
}
