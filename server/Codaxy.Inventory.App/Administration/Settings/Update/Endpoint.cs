using System.Text.Json;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Access;
using Codaxy.Inventory.App.Shared.ExpiryAlerts;
using Codaxy.Inventory.App.Shared.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Administration.Settings.Update;

/// <summary>
/// One group's settings, every one of them: saved all together or, with any problem, none — the
/// problems under their keys.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder settings) => settings.MapPut("/{group}", Handle);

    private static async Task<IResult> Handle(
        string group,
        [FromBody] Dictionary<string, JsonElement> values,
        InventoryContext context,
        ExpiryAlertRequests alerts,
        CancellationToken cancellationToken
    )
    {
        var definitions = SettingCatalogue.All.Where(d => d.Group == group).ToList();
        if (definitions.Count == 0)
            return Results.NotFound();

        Dictionary<string, string[]> problems = [];
        Dictionary<string, string> accepted = [];

        foreach (var key in values.Keys.Where(k => definitions.All(d => d.Key != k)))
            problems[key] = [$"Not a setting of {group}."];

        foreach (var definition in definitions)
        {
            if (!values.TryGetValue(definition.Key, out var value))
            {
                problems[definition.Key] = ["Required."];
                continue;
            }

            var (json, problem) = definition.Accept(value);
            if (problem is not null)
                problems[definition.Key] = [problem];
            else
                accepted[definition.Key] = json!;
        }

        if (
            accepted.TryGetValue(SignInSettings.AllowedDomains.Key, out var domainsJson)
            && await ShutOutAsync(context, domainsJson, cancellationToken) is { } shutOut
        )
            problems[SignInSettings.AllowedDomains.Key] = [shutOut];

        if (problems.Count > 0)
            return Results.ValidationProblem(problems);

        var rows = await context
            .Settings.Where(s => accepted.Keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, cancellationToken);

        foreach (var (key, json) in accepted)
        {
            if (rows.TryGetValue(key, out var row))
                row.Value = json;
            else
                context.Settings.Add(
                    new Setting
                    {
                        Id = Guid.CreateVersion7(),
                        Key = key,
                        Value = json,
                    }
                );
        }

        await context.SaveChangesAsync(cancellationToken);

        // Recipients just named hear at once of what is due, not at the next interval.
        if (group == ExpiryAlertSettings.Lead.Group)
            alerts.CheckNow();

        return Results.Ok(await List.Endpoint.ReadAsync(context, cancellationToken));
    }

    /// <summary>
    /// Why the domains would lock out an active System Admin, or null: without one, nobody could put
    /// the domains back but by SQL.
    /// </summary>
    private static async Task<string?> ShutOutAsync(
        InventoryContext context,
        string domainsJson,
        CancellationToken cancellationToken
    )
    {
        var domains = SignInSettings.AllowedDomains.Read(domainsJson).Value;
        var admins = await context
            .AppUsers.AsNoTracking()
            .Where(u => u.IsActive && u.Role.IsSystemAdmin)
            .Select(u => u.Email)
            .ToListAsync(cancellationToken);

        var outside = admins.Where(e => !SignInSettings.Allows(domains, e)).Order().ToList();

        return outside.Count == 0
            ? null
            : $"System Admin {string.Join(", ", outside)} could no longer sign in.";
    }
}
