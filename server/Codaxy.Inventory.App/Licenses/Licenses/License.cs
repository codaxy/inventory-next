#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codaxy.Inventory.App.Persistence;
using Codaxy.Inventory.App.Shared.Assets;
using Codaxy.Inventory.App.Shared.Volumes;

namespace Codaxy.Inventory.App.Licenses.Licenses;

public class License : IIdentifiableReadOnly<Guid>
{
    public Guid Id => AssetId;
    public Guid AssetId { get; set; }
    public Guid? LicenseTypeId { get; set; }
    public Guid? LicenseExpirationModelId { get; set; }
    public Guid? LicenseModelId { get; set; }
    public string ManagementConsoleUrl { get; set; }
    public string RegistrationNumber { get; set; }
    public DateOnly? SubscriptionExpirationDate { get; set; }
    public decimal? SubscriptionFee { get; set; }
    public bool? AutoRenew { get; set; }
    public string KeyIdentifier { get; set; }
    public Guid? PeriodId { get; set; }
    public Guid? CurrencyId { get; set; }
    public ICollection<Volume> Volumes { get; set; }

    public Asset Asset { get; set; }
    public LicenseType LicenseType { get; set; }
    public LicenseExpirationModel LicenseExpirationModel { get; set; }
    public LicenseModel LicenseModel { get; set; }
    public Period Period { get; set; }
    public Currency Currency { get; set; }
}
