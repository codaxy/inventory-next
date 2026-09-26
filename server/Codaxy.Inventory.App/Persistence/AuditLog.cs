#nullable disable

using System;

namespace Codaxy.Inventory.App.Persistence;

public class AuditLog
{
    public Guid Id { get; set; }
    public string Table { get; set; }
    public Guid EntityId { get; set; }
    public string Email { get; set; }
    public string ActionType { get; set; }
    public Guid? TransactionId { get; set; }
    public string NewValuesJson { get; set; }
    public string OldValuesJson { get; set; }
    public DateTimeOffset TimeCreated { get; set; }
}
