namespace MyCRM.Auth.Domain.Enums;

public enum TenantPlanStatus
{
    Active              = 0,
    Paused              = 1,
    PendingCancellation = 2,
    Expired             = 3,
    Cancelled           = 4,
    Upgraded            = 5,
}
