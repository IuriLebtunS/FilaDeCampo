using FilaDeCampo.Data;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// ================================
// Services
// ================================
builder.Services.AddControllersWithViews();

// ================================
// Database
// ================================
builder.Services.AddDbContext<DbSolaresCampo>(options =>
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        // PRODUÇÃO (Railway)
        options.UseNpgsql(
            databaseUrl + ";SSL Mode=Require;Trust Server Certificate=true"
        );
    }
    else
    {
        // DESENVOLVIMENTO (local)
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("Default")
        );
    }
});

var portVar = Environment.GetEnvironmentVariable("PORT");

if (portVar is { Length: > 0 } && int.TryParse(portVar, out int port))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(port);
    });
}

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

// NToastNotify
app.UseNToastNotify();

// ================================
// MVC Routes
// ================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Congregacao}/{action=Login}/{id?}");

app.Run();
