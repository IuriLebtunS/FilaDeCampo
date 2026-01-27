using FilaDeCampo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.Cookies;
using NToastNotify;
using Npgsql;

string ConvertDatabaseUrlToConnectionString(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    return new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    }.ToString();
}

var builder = WebApplication.CreateBuilder(args);

// ================================
// MVC + Toast
// ================================
builder.Services
    .AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions
    {
        ProgressBar = true,
        PositionClass = ToastPositions.TopRight,
        TimeOut = 4000,
        CloseButton = true
    });

// ================================
// Database (Railway / Local)
// ================================
builder.Services.AddDbContext<DbSolaresCampo>(options =>
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        // PRODUÇÃO → Railway
        options.UseNpgsql(ConvertDatabaseUrlToConnectionString(databaseUrl));
    }
    else
    {
        // LOCAL → PostgreSQL
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection")
        );
    }
});
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
// Session (opcional, legado)
// ================================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<ValidateCongregacaoFilter>();

// ================================
// Build app
// ================================
var app = builder.Build();

// ================================
// Middlewares
// ================================
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();        // pode remover futuramente
app.UseAuthentication();
app.UseAuthorization();
app.UseNToastNotify();

// ================================
// EF Core Migrations (seguro)
// ================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<DbSolaresCampo>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration error: {ex.Message}");
    }
}

// ================================
// Routes
// ================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Congregacao}/{action=Login}/{id?}");

app.Run();
