namespace Codaxy.Inventory.App;

public static class InventoryApi
{
    /// <summary>
    /// The menu's endpoints, under <c>/api</c>, every one behind a session: the menu's sections in
    /// its order, and the Dashboard, which is in none; each maps its own items. Then what serves
    /// no one item: the printed documents' settings.
    /// </summary>
    public static void MapInventoryApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").RequireAuthorization();

        Dashboard.Endpoints.Map(api);
        Company.Endpoints.Map(api);
        ElectronicDevices.Endpoints.Map(api);
        Licenses.Endpoints.Map(api);
        Furnitures.Endpoints.Map(api);
        Informations.Endpoints.Map(api);
        Infrastructure.Endpoints.Map(api);
        Administration.Endpoints.Map(api);
        Shared.Documents.Settings.Endpoint.Map(api);
    }
}
