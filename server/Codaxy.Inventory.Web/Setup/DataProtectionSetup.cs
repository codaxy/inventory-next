using Microsoft.AspNetCore.DataProtection;

namespace Codaxy.Inventory.Web.Setup;

public static class DataProtectionSetup
{
    /// <summary>
    /// The session cookie is protected by the data protection key ring, so a ring that does not
    /// outlive the container signs every user out on restart and stops two instances reading each
    /// other's cookies. Persisted whenever a path is configured; the application name is fixed so the
    /// discriminator does not change with the content root.
    /// </summary>
    public static IServiceCollection AddInventoryDataProtection(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var keyRingPath = configuration["DataProtection:KeyRingPath"];
        var dataProtection = services.AddDataProtection().SetApplicationName("Inventory");

        if (!string.IsNullOrWhiteSpace(keyRingPath))
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));

        return services;
    }
}
