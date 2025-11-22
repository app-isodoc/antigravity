namespace Isodoc.Web.Services;

public interface ICurrentTenantService
{
    Guid? ClientId { get; }
    string? ClientStatus { get; }
    bool IsSuperAdmin { get; }
    void SetTenant(Guid clientId);
    void SetTenantWithStatus(Guid clientId, string status);
    void SetSuperAdmin();
}

public class CurrentTenantService : ICurrentTenantService
{
    public Guid? ClientId { get; private set; }
    public string? ClientStatus { get; private set; }
    public bool IsSuperAdmin { get; private set; }

    public void SetTenant(Guid clientId)
    {
        ClientId = clientId;
        IsSuperAdmin = false;
    }

    public void SetTenantWithStatus(Guid clientId, string status)
    {
        ClientId = clientId;
        ClientStatus = status;
        IsSuperAdmin = false;
    }

    public void SetSuperAdmin()
    {
        IsSuperAdmin = true;
        ClientId = null;
    }
}
