namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// Primary classification of a Company within the platform.
/// A company may evolve to hold multiple roles (e.g. a school that is also a supplier),
/// but this enum captures the dominant business relationship.
/// Role-specific data (Supplier, Partner, School, etc.) lives in dedicated satellite
/// entities that reference Company by CompanyId.
/// </summary>
public enum CompanyType
{
    Supplier          = 1,
    Partner           = 2,
    School            = 3,
    CorporateCustomer = 4,
    BillingAccount    = 5,
    ServiceProvider   = 6,
    Sponsor           = 7,
    Other             = 8
}
