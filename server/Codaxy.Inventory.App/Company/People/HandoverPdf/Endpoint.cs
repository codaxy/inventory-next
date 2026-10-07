using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Export;
using Codaxy.Inventory.App.Shared.Printing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Codaxy.Inventory.App.Company.People.HandoverPdf;

/// <summary>
/// The handover sheet as a PDF: the sheet's own page, printed by the server as the caller sees it,
/// its date the caller's (<c>tz</c>). Absent where the server has no browser to print with.
/// </summary>
public static class Endpoint
{
    public static void Map(RouteGroupBuilder people) =>
        people.MapGet("/{id:guid}/handover.pdf", Handle);

    private static async Task<IResult> Handle(
        Guid id,
        [FromQuery(Name = ExportZone.ZoneParameter)] string? tz,
        HttpContext http,
        InventoryContext context,
        [FromServices] IPagePrinter? printer,
        TimeProvider time,
        CancellationToken cancellationToken
    )
    {
        if (printer is null)
            return Results.NotFound();

        if (ExportZone.Read(tz, null, time.GetUtcNow().Year, out var zone) is { } refused)
            return refused;

        var name = await context
            .Persons.Where(p => p.Id == id)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken);
        if (name is null)
            return Results.NotFound();

        try
        {
            var pdf = await printer.PrintAsync(
                http,
                $"/company/people/{id}/handover",
                zone.Time,
                cancellationToken
            );
            return Results.File(pdf, "application/pdf", $"Handover sheet - {name.Trim()}.pdf");
        }
        catch (PagePrintException)
        {
            return Results.Problem(
                title: "The handover sheet could not be printed.",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
