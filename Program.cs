using FilaDeCampo.Data;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using Microsoft.AspNetCore.HttpOverrides;


var builder = WebApplication.CreateBuilder(args);

// ================================
// MVC + NToastNotify
// ================================
builder.Services.AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions
    {
        ProgressBar = true,
        PositionClass = ToastPositions.TopRight,
        PreventDuplicates = true,
        CloseButton = true
    });

// ================================
// DbContext
// ================================
builder.Services.AddDbContext<DbSolaresCampo>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
        ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ));

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
