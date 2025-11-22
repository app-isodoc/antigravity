using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<Isodoc.Web.Data.ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddIdentity<Isodoc.Web.Models.ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<Isodoc.Web.Data.ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<Isodoc.Web.Services.CustomUserClaimsPrincipalFactory>();

builder.Services.Configure<Isodoc.Web.Models.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<Isodoc.Web.Services.IEmailService, Isodoc.Web.Services.EmailService>();

builder.Services.AddScoped<Isodoc.Web.Services.ICurrentTenantService, Isodoc.Web.Services.CurrentTenantService>();

var app = builder.Build();

// Garantir que o banco de dados seja criado e inicializar dados
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("==== INICIANDO SETUP DO BANCO DE DADOS ====");
        
        var context = services.GetRequiredService<Isodoc.Web.Data.ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<Isodoc.Web.Models.ApplicationUser>>();
        
        // Criar banco de dados
        logger.LogInformation("Criando banco de dados...");
        context.Database.EnsureCreated();
        logger.LogInformation("Banco de dados criado/verificado com sucesso!");
        
        // Criar super admin se não existir
        var superAdminEmail = "consultoriaviso@gmail.com";
        logger.LogInformation($"Verificando se super admin '{superAdminEmail}' existe...");
        
        var existingSuperAdmin = userManager.FindByEmailAsync(superAdminEmail).GetAwaiter().GetResult();
        
        if (existingSuperAdmin == null)
        {
            logger.LogInformation("Super admin não encontrado. Criando novo usuário...");
            
            var superAdmin = new Isodoc.Web.Models.ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                EmailConfirmed = true,
                Nome = "Super Administrador",
                Role = Isodoc.Web.Models.UserRole.SuperAdmin,
                Ativo = true,
                PrimeiroAcesso = false,
                ClientId = null // Super admin não pertence a nenhum cliente específico
            };
            
            logger.LogInformation($"Tentando criar usuário com email: {superAdminEmail}");
            var result = userManager.CreateAsync(superAdmin, "QAZ12@#wsx").GetAwaiter().GetResult();
            
            if (result.Succeeded)
            {
                logger.LogInformation("✅ ✅ ✅ SUPER ADMIN CRIADO COM SUCESSO! ✅ ✅ ✅");
                logger.LogInformation($"Email: {superAdminEmail}");
                logger.LogInformation($"Senha: QAZ12@#wsx");
            }
            else
            {
                logger.LogError($"❌ ERRO AO CRIAR SUPER ADMIN!");
                foreach (var error in result.Errors)
                {
                    logger.LogError($"  - {error.Code}: {error.Description}");
                }
            }
        }
        else
        {
            logger.LogInformation($"ℹ️ Super admin '{superAdminEmail}' já existe no banco de dados.");
        }

        // Criar usuário DEMO se não existir
        var demoEmail = "admin@demo.com";
        logger.LogInformation($"Verificando se usuário demo '{demoEmail}' existe...");
        
        var existingDemoUser = userManager.FindByEmailAsync(demoEmail).GetAwaiter().GetResult();
        
        if (existingDemoUser == null)
        {
            logger.LogInformation("Usuário demo não encontrado. Criando...");
            
            var demoUser = new Isodoc.Web.Models.ApplicationUser
            {
                UserName = demoEmail,
                Email = demoEmail,
                EmailConfirmed = true,
                Nome = "Usuário Demo",
                Role = Isodoc.Web.Models.UserRole.Administrador,
                Ativo = true,
                PrimeiroAcesso = false,
                ClientId = null // Demo user sem cliente específico por enquanto, ou criar um cliente demo
            };
            
            var result = userManager.CreateAsync(demoUser, "Admin@123").GetAwaiter().GetResult();
            
            if (result.Succeeded)
            {
                logger.LogInformation("✅ ✅ ✅ USUÁRIO DEMO CRIADO COM SUCESSO! ✅ ✅ ✅");
                logger.LogInformation($"Email: {demoEmail}");
                logger.LogInformation($"Senha: Admin@123");
            }
            else
            {
                logger.LogError($"❌ ERRO AO CRIAR USUÁRIO DEMO!");
            }
        }
        
        logger.LogInformation("==== SETUP DO BANCO FINALIZADO ====");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ ❌ ❌ ERRO CRÍTICO NO SETUP DO BANCO DE DADOS!");
    }
}





if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var supportedCultures = new[] { "pt-BR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<Isodoc.Web.Middleware.TenantResolutionMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");

app.Run();
