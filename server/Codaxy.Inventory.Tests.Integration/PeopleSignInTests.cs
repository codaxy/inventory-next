using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Company.People;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Me = Codaxy.Inventory.Web.Auth.Session.Me.Endpoint;

namespace Codaxy.Inventory.Tests.Integration;

public class PeopleSignInApplication : InventoryApplication
{
    /// <summary>A person, made with the email given, and whether they may sign in.</summary>
    public async Task<Guid> PersonAsync(string name, string email, bool canSignIn = true)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var person = new Person
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Email = email,
            CanSignIn = canSignIn,
        };
        context.Persons.Add(person);
        await context.SaveChangesAsync();
        return person.Id;
    }

    public async Task CanSignInAsync(Guid person, bool canSignIn)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        await context
            .Persons.Where(p => p.Id == person)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.CanSignIn, canSignIn));
    }

    public async Task PeopleMaySignInAsync(bool open)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        await Seed.SettingAsync(context, SignInSettings.People, open);
    }
}

/// <summary>Employees signing in as the person they are. Each test sets the switch it needs.</summary>
public class PeopleSignInTests(PeopleSignInApplication app) : IClassFixture<PeopleSignInApplication>
{
    [Fact]
    public async Task With_employees_let_in_a_person_signs_in_by_their_email_whatever_its_case()
    {
        await app.PeopleMaySignInAsync(true);
        await app.PersonAsync("Ana Employee", " Ana.Employee@codaxy.com ");

        var client = await app.SignInAsync("ana.employee@codaxy.com");
        var me = await client.GetFromJsonAsync<Me.Response>("/api/auth/me");

        Assert.Equal(
            (Me.PersonKind, "Ana.Employee@codaxy.com", "Ana Employee", (string?)null),
            (me!.Kind, me.Email, me.Name, me.Role)
        );
        Assert.Empty(me.Permissions);
    }

    [Fact]
    public async Task With_employees_kept_out_a_person_is_not_let_in()
    {
        await app.PeopleMaySignInAsync(false);
        await app.PersonAsync("Ben Employee", "ben.employee@codaxy.com");

        var verified = await app.VerifyAsync(app.Client(), "ben.employee@codaxy.com");

        Assert.Equal(HttpStatusCode.Forbidden, verified.StatusCode);
    }

    [Fact]
    public async Task A_person_turned_off_is_not_let_in_while_employees_are()
    {
        await app.PeopleMaySignInAsync(true);
        await app.PersonAsync("Cora Leaver", "cora.leaver@codaxy.com", canSignIn: false);

        var verified = await app.VerifyAsync(app.Client(), "cora.leaver@codaxy.com");

        Assert.Equal(HttpStatusCode.Forbidden, verified.StatusCode);
    }

    [Fact]
    public async Task A_person_s_session_holds_no_permission()
    {
        await app.PeopleMaySignInAsync(true);
        await app.PersonAsync("Dan Employee", "dan.employee@codaxy.com");
        var client = await app.SignInAsync("dan.employee@codaxy.com");

        var people = await client.GetAsync("/api/company/people");
        var settings = await client.GetAsync("/api/administration/settings");

        Assert.Equal(
            (HttpStatusCode.Forbidden, HttpStatusCode.Forbidden),
            (people.StatusCode, settings.StatusCode)
        );
    }

    [Fact]
    public async Task Turning_the_person_off_ends_their_session_on_the_next_request()
    {
        await app.PeopleMaySignInAsync(true);
        var person = await app.PersonAsync("Eva Employee", "eva.employee@codaxy.com");
        var client = await app.SignInAsync("eva.employee@codaxy.com");

        await app.CanSignInAsync(person, false);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/auth/me")).StatusCode
        );
    }

    [Fact]
    public async Task Keeping_employees_out_ends_their_sessions_on_the_next_request()
    {
        await app.PeopleMaySignInAsync(true);
        await app.PersonAsync("Finn Employee", "finn.employee@codaxy.com");
        var client = await app.SignInAsync("finn.employee@codaxy.com");

        await app.PeopleMaySignInAsync(false);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.GetAsync("/api/auth/me")).StatusCode
        );
    }

    [Fact]
    public async Task Someone_both_a_user_and_a_person_signs_in_as_the_user()
    {
        await app.PeopleMaySignInAsync(true);
        await app.PersonAsync("Gia Both", "gia.both@codaxy.com");
        var client = await app.ClientAsync("gia.both@codaxy.com");

        var me = await client.GetFromJsonAsync<Me.Response>("/api/auth/me");

        Assert.Equal(Me.UserKind, me!.Kind);
        Assert.NotEmpty(me.Permissions);
    }

    [Fact]
    public async Task A_person_is_saved_with_whether_they_may_sign_in_and_shown_it_beside_the_switch()
    {
        await app.PeopleMaySignInAsync(false);
        var client = await app.ClientAsync("people.editor@codaxy.com");

        var created = await (
            await client.PostAsJsonAsync(
                "/api/company/people",
                new
                {
                    name = "Hal Placeholder",
                    email = "hal.placeholder@codaxy.com",
                    canSignIn = false,
                }
            )
        ).Content.ReadFromJsonAsync<PersonDetail>();
        var defaulted = await (
            await client.PostAsJsonAsync(
                "/api/company/people",
                new { name = "Ida Newcomer", email = "ida.newcomer@codaxy.com" }
            )
        ).Content.ReadFromJsonAsync<PersonDetail>();

        Assert.Equal((false, false), (created!.CanSignIn, created.SignInOpen));
        Assert.True(defaulted!.CanSignIn);
    }

    [Fact]
    public async Task A_person_s_form_is_told_whether_employees_may_sign_in()
    {
        var client = await app.ClientAsync("people.reader@codaxy.com");

        await app.PeopleMaySignInAsync(true);
        var open = await client.GetFromJsonAsync<App.Company.People.SignIn.Endpoint.Response>(
            "/api/company/people/sign-in"
        );
        await app.PeopleMaySignInAsync(false);
        var closed = await client.GetFromJsonAsync<App.Company.People.SignIn.Endpoint.Response>(
            "/api/company/people/sign-in"
        );

        Assert.Equal((true, false), (open!.Open, closed!.Open));
    }

    [Fact]
    public async Task The_database_holds_one_person_per_email_whatever_its_case_or_spaces()
    {
        await app.PersonAsync("Jan First", "jan.same@codaxy.com");

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            app.PersonAsync("Jan Second", "  JAN.Same@codaxy.com ")
        );
    }

    [Fact]
    public async Task The_audit_log_leaves_out_the_email_key_the_database_computes()
    {
        var id = await app.PersonAsync("Kim Audited", "kim.audited@codaxy.com");

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var logged = await context.AuditLogs.SingleAsync(a => a.EntityId == id);

        Assert.Contains("\"Email\"", logged.NewValuesJson);
        Assert.DoesNotContain("EmailKey", logged.NewValuesJson);
    }
}
