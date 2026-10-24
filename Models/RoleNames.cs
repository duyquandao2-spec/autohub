namespace AutoHub.Models;

public static class RoleNames
{
    public const string Customer = "Customer";
    public const string Sales = "Sales";
    public const string ContentEditor = "ContentEditor";
    public const string Manager = "Manager";
    public const string Administrator = "Administrator";
    public const string SuperAdmin = "SuperAdmin";

    public static readonly string[] All =
    {
        Customer, Sales, ContentEditor, Manager, Administrator, SuperAdmin
    };

    // Roles that can manage vehicles fully
    public static readonly string[] VehicleWriters = { Administrator, SuperAdmin };
    // Roles that can manage appointments/consultations
    public static readonly string[] AppointmentManagers = { Sales, Manager, Administrator, SuperAdmin };
    // Roles that can manage users
    public static readonly string[] UserManagers = { Administrator, SuperAdmin };
    // Roles that can manage content
    public static readonly string[] ContentManagers = { ContentEditor, Manager, Administrator, SuperAdmin };
    // Roles that can view audit logs
    public static readonly string[] AuditViewers = { Manager, Administrator, SuperAdmin };
    // Roles that can manage roles
    public static readonly string[] RoleManagers = { SuperAdmin };
    // Roles that can change system settings
    public static readonly string[] SystemConfigurators = { SuperAdmin };
}

public static class Policies
{
    public const string VehicleWrite = "VehicleWrite";
    public const string VehicleDelete = "VehicleDelete";
    public const string UserManagement = "UserManagement";
    public const string AppointmentManagement = "AppointmentManagement";
    public const string ContentManagement = "ContentManagement";
    public const string SecurityAdministration = "SecurityAdministration";
    public const string SystemConfiguration = "SystemConfiguration";
    public const string AuditView = "AuditView";
    public const string AdminArea = "AdminArea";
}
