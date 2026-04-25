using HealthChecks.NpgSql;
using PerfumeStore.Web.Data;
using PerfumeStore.Web.Data.Repositories;
using PerfumeStore.Web.Infrastructure;
using PerfumeStore.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<ICommerceRepository, CommerceRepository>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Perfumier.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(2);
});

builder.Services.AddScoped<LandingPageService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<CustomerService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                var loginPath = context.Request.Path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase)
                    ? "/Account/Login?type=admin"
                    : "/Account/Login?type=customer";

                context.Response.Redirect(loginPath);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                var returnUrl = string.IsNullOrWhiteSpace(context.Request.Path.Value)
                    ? string.Empty
                    : QueryString.Create("returnUrl", context.Request.Path.Value!).Value;

                context.Response.Redirect($"/Account/AccessDenied{returnUrl}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
    options.AddPolicy("StaffPortal", policy => policy.RequireRole(AppRoles.StaffRoles));
});


builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddResponseCaching();

var connectionString = builder.Configuration.GetConnectionString("PerfumeStore");
var healthChecks = builder.Services.AddHealthChecks();
if (!string.IsNullOrEmpty(connectionString))
{
    healthChecks.AddNpgSql(connectionString);
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Landing/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.UseResponseCaching();

app.MapHealthChecks("/healthz");
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{*action}",
    defaults: new { controller = "Admin" }
).RequireAuthorization("StaffPortal");

app.Run();
