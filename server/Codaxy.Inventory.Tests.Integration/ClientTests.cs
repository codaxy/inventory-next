using System.Net;
using System.Net.Http.Json;
using Codaxy.Inventory.App.Company.Clients;
using Codaxy.Inventory.App.Company.People;
using Codaxy.Inventory.App.Company.Projects;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Paging;
using Codaxy.Inventory.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codaxy.Inventory.Tests.Integration;

using Item = App.Company.Clients.List.Endpoint.Item;

/// <summary>Acme has two projects led by Olga; Idle Inc. has none.</summary>
public class ClientApplication : InventoryApplication
{
    public static readonly Guid Acme = Guid.CreateVersion7();
    public static readonly Guid Idle = Guid.CreateVersion7();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InventoryContext>();
        var olga = new Person
        {
            Id = Guid.CreateVersion7(),
            Name = "Olga Owner",
            Email = "olga@codaxy.com",
        };
        context.Persons.Add(olga);
        context.Clients.AddRange(
            new Client { Id = Acme, Name = "Acme" },
            new Client { Id = Idle, Name = "Idle Inc." }
        );
        context.Projects.AddRange(
            new Project
            {
                Id = Guid.CreateVersion7(),
                Name = "Rocket",
                ClientId = Acme,
                ProjectOwnerId = olga.Id,
            },
            new Project
            {
                Id = Guid.CreateVersion7(),
                Name = "Anvil",
                ClientId = Acme,
                ProjectOwnerId = olga.Id,
            }
        );
        await context.SaveChangesAsync();
    }

    public async Task<T> InScopeAsync<T>(Func<InventoryContext, Task<T>> read)
    {
        using var scope = Services.CreateScope();
        return await read(scope.ServiceProvider.GetRequiredService<InventoryContext>());
    }
}

public class ClientTests(ClientApplication app) : IClassFixture<ClientApplication>
{
    private const string Url = "/api/company/clients";

    private async Task<HttpClient> Client() => await app.ClientAsync("editor@codaxy.com");

    private static async Task<string[]> ErrorsOf(HttpResponseMessage response, string field)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!.Errors[
            field
        ];
    }

    private async Task<Page<Item>> ListAsync(string query) =>
        (await (await Client()).GetFromJsonAsync<Page<Item>>($"{Url}/?{query}"))!;

    [Fact]
    public async Task Refuses_a_caller_without_a_session() =>
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await app.CreateClient().GetAsync($"{Url}/")).StatusCode
        );

    [Fact]
    public async Task A_client_is_created_edited_and_deleted_with_its_audit_rows()
    {
        var client = await Client();

        var created = await client.PostAsJsonAsync(Url, new { name = "  Globex " });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var globex = (await created.Content.ReadFromJsonAsync<ClientDetail>())!;
        Assert.Equal(("Globex", 0), (globex.Name, globex.ProjectCount));

        var edited = await client.PutAsJsonAsync(
            $"{Url}/{globex.Id}",
            new { name = "Globex Corp" }
        );
        Assert.Equal("Globex Corp", (await edited.Content.ReadFromJsonAsync<ClientDetail>())!.Name);

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"{Url}/{globex.Id}")).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync($"{Url}/{globex.Id}")).StatusCode
        );
        Assert.Equal(
            ["Create", "Update", "Delete"],
            await app.InScopeAsync(c =>
                c.AuditLogs.Where(a => a.EntityId == globex.Id)
                    .OrderBy(a => a.TimeCreated)
                    .Select(a => a.ActionType)
                    .ToListAsync()
            )
        );
    }

    [Fact]
    public async Task A_name_is_required_unique_whatever_its_case_and_kept_by_its_own()
    {
        var client = await Client();

        Assert.NotEmpty(
            await ErrorsOf(await client.PostAsJsonAsync(Url, new { name = " " }), "name")
        );
        Assert.Equal(
            ["A client with this name already exists."],
            await ErrorsOf(await client.PostAsJsonAsync(Url, new { name = " ACME" }), "name")
        );
        Assert.NotEmpty(
            await ErrorsOf(
                await client.PostAsJsonAsync(Url, new { name = new string('n', 201) }),
                "name"
            )
        );
        Assert.Equal(
            HttpStatusCode.OK,
            (
                await client.PutAsJsonAsync(
                    $"{Url}/{ClientApplication.Idle}",
                    new { name = "IDLE INC." }
                )
            ).StatusCode
        );
        await client.PutAsJsonAsync($"{Url}/{ClientApplication.Idle}", new { name = "Idle Inc." });
    }

    [Fact]
    public async Task A_client_with_projects_is_not_deleted_and_keeps_them()
    {
        var response = await (await Client()).DeleteAsync($"{Url}/{ClientApplication.Acme}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(
            "2 projects are of this client, so it cannot be deleted.",
            (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title
        );
        Assert.Equal(
            2,
            await app.InScopeAsync(c =>
                c.Projects.CountAsync(p => p.ClientId == ClientApplication.Acme)
            )
        );
    }

    [Fact]
    public async Task The_detail_carries_its_projects_by_name_with_their_owners()
    {
        var acme = (
            await (await Client()).GetFromJsonAsync<ClientDetail>($"{Url}/{ClientApplication.Acme}")
        )!;

        Assert.Equal(2, acme.ProjectCount);
        Assert.Equal(["Anvil", "Rocket"], acme.Projects.Select(p => p.Name));
        Assert.All(acme.Projects, p => Assert.Equal("Olga Owner", p.Owner));
    }

    [Fact]
    public async Task An_unknown_client_is_not_found()
    {
        var client = await Client();
        var id = Guid.CreateVersion7();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Url}/{id}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.PutAsJsonAsync($"{Url}/{id}", new { name = "X" })).StatusCode
        );
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{Url}/{id}")).StatusCode);
    }

    [Fact]
    public async Task Lists_with_their_project_count_searching_sorting_and_paging()
    {
        var acme = Assert.Single((await ListAsync("q=acm")).Items);
        Assert.Equal(2, acme.Projects);

        var byProjects = (await ListAsync("sort=-projects&pageSize=100")).Items;
        Assert.Equal(
            byProjects.OrderByDescending(c => c.Projects).Select(c => c.Projects),
            byProjects.Select(c => c.Projects)
        );
        Assert.Single((await ListAsync("pageSize=1")).Items);
        Assert.Empty((await ListAsync("page=999")).Items);
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await (await Client()).GetAsync($"{Url}/?sort=size")).StatusCode
        );
    }
}
