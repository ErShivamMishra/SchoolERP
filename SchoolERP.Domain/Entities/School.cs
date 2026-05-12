using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Domain.Entities;

public sealed class School : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime SubscriptionStartDateUtc { get; set; }
    public DateTime SubscriptionEndDateUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public SchoolStatus Status { get; set; } = SchoolStatus.Active;
    public int MaxStaffLimit { get; set; }
    public bool IsCampusEnabled { get; set; }

    [NotMapped]
    public DateTime SubscriptionStartDate
    {
        get => SubscriptionStartDateUtc;
        set => SubscriptionStartDateUtc = value;
    }

    [NotMapped]
    public DateTime SubscriptionEndDate
    {
        get => SubscriptionEndDateUtc;
        set => SubscriptionEndDateUtc = value;
    }

    [NotMapped]
    public DateTime CreatedAt => CreatedAtUtc;

    public ICollection<Campus> Campuses { get; set; } = new List<Campus>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<SchoolSubscription> Subscriptions { get; set; } = new List<SchoolSubscription>();
}
