using HealthChecks.NpgSql;
using PerfumeStore.Web.Data;
using PerfumeStore.Web.Data.Repositories;
using PerfumeStore.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<ICommerceRepository, CommerceRepository>();

builder.Services.AddScoped<LandingPageService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<AdminService>();

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
app.UseResponseCaching();

app.MapHealthChecks("/healthz");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");

app.Run();
