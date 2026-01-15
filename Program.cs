using FilaDeCampo.Data;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using Microsoft.AspNetCore.HttpOverrides;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DbSolaresCampo>(options =>
    options.UseNpgsql(
        Environment.GetEnvironmentVariable("DATABASE_URL")
        + ";SSL Mode=Require;Trust Server Certificate=true"
    )
);

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
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();        // Session ANTES dos controllers
app.UseAuthorization();

// NToastNotify
app.UseNToastNotify();

// ================================
// Rotas MVC
// ================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Congregacao}/{action=Login}/{id?}");

app.Run();
