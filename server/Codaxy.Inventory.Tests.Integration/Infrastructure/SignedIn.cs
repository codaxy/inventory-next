using System.Net.Http.Json;
using Codaxy.Inventory.App.Administration.Roles;
using Codaxy.Inventory.App.Administration.Users;
using Codaxy.Inventory.App.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration.Infrastructure;

public static class SignedIn
{
    /// <summary>
    /// A client holding a session, signed in the way a person is: by a one-time code, as a user
    /// holding <paramref name="role"/> — Admin unless a test needs another.
    /// </summary>
    public static async Task<HttpClient> ClientAsync(
        this InventoryApplication app,
        string email = "reader@codaxy.com",
        string role = BuiltInRoles.Admin
    )
    {
        await app.UserAsync(email, role);
        return await app.SignInAsync(email);
    }

    /// <summary>A client that has signed in as <paramref name="email"/>, whatever it is to the application.</summary>
    public static async Task<HttpClient> SignInAsync(this InventoryApplication app, string email)
    {
        var client = app.Client();
        var verified = await app.VerifyAsync(client, email);
        verified.EnsureSuccessStatusCode();

        return client;
    }

    /// <summary>Requests a code for <paramref name="email"/> and answers it; the verification's response.</summary>
    public static async Task<HttpResponseMessage> VerifyAsync(
        this InventoryApplication app,
        HttpClient client,
        string email
    )
    {
        await client.PostAsJsonAsync("/api/auth/one-time-code/request", new { email });

        return await client.PostAsJsonAsync(
            "/api/auth/one-time-code/verify",
            new { email, code = app.Emails.LastCodeFor(email) }
        );
    }

    public static HttpClient Client(this InventoryApplication app) =>
        app.CreateClient(
            new()
            {
                BaseAddress = app.ClientOptions.BaseAddress,
                AllowAutoRedirect = false,
                HandleCookies = true,
            }
        );

    /// <summary>The user <paramref name="email"/>, active and holding <paramref name="role"/>, made so.</summary>
    public static async Task<AppUser> UserAsync(
        this InventoryApplication app,
        string email,
        string role = BuiltInRoles.Admin,
        bool active = true
    )
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var roleId = await context.Roles.Where(r => r.Name == role).Select(r => r.Id).SingleAsync();
        email = AppUser.Normalize(email);

        var user = await context.AppUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            user = new AppUser
            {
                Id = Guid.CreateVersion7(),
                Email = email,
                Name = email.Split('@')[0],
                CreatedAt = DateTimeOffset.UtcNow,
            };
            context.AppUsers.Add(user);
        }

        user.RoleId = roleId;
        user.IsActive = active;
        await context.SaveChangesAsync();

        // A test's setup, not something the application did: left in, it would be in every audit log
        // a test reads.
        await context
            .AuditLogs.Where(a =>
                a.EntityId == user.Id && a.Email == AuditLogInterceptor.SystemUser
            )
            .ExecuteDeleteAsync();

        return user;
    }
}
