using Isodoc.Web.Models;
using Isodoc.Web.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Isodoc.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentTenantService _currentTenantService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentTenantService currentTenantService)
        : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<NonConformity> NonConformities { get; set; }
    public DbSet<Indicator> Indicators { get; set; }
    public DbSet<IndicatorMeasurement> IndicatorMeasurements { get; set; }
    public DbSet<AuditProgram> AuditPrograms { get; set; }
    public DbSet<UserModulePermission> UserModulePermissions { get; set; }
    public DbSet<SystemModule> SystemModules { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Client>().HasIndex(c => c.NomeFantasia);
        builder.Entity<Client>().HasIndex(c => c.CNPJ).IsUnique();
        builder.Entity<Client>().HasIndex(c => c.UrlPersonalizada).IsUnique();

        // Aplicar filtro global de tenant
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetGlobalQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { builder });
            }
        }
    }

    private void SetGlobalQueryFilter<T>(ModelBuilder builder) where T : class, IMustHaveTenant
    {
        builder.Entity<T>().HasQueryFilter(e => _currentTenantService.IsSuperAdmin || e.ClientId == _currentTenantService.ClientId);
    }
}
