using FilaDeCampo.Data;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ================================
// Services
// ================================
builder.Services
    .AddControllersWithViews()
    .AddNToastNotifyToastr();

// ================================
// Database
// ================================
builder.Services.AddDbContext<DbSolaresCampo>(options =>
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        // Produção (Railway)
        options.UseNpgsql(
            databaseUrl + ";SSL Mode=Require;Trust Server Certificate=true"
        );
    }
    else
    {
        // Desenvolvimento
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("Default")
        );
    }
});

// ================================
// PORT (Railway / PaaS)
// ================================
var portVar = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(portVar) && int.TryParse(portVar, out int port))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(port);
    });
}

// ================================
// Authentication
// ================================
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Congregacao/Login";
        options.AccessDeniedPath = "/Home/AcessoNegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// ================================
// Session
// ================================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ================================
// Build
// ================================
var app = builder.Build();

// ================================
// Middlewares
// ================================
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

// ⚠️ Se estiver atrás de proxy HTTPS (Railway), pode manter
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// 🔑 ORDEM CORRETA
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Toasts
app.UseNToastNotify();

// ================================
// MVC Routes
// ================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Congregacao}/{action=Login}/{id?}");

app.Run();
