using FilaDeCampo.Data;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services
    .AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions
    {
        ProgressBar = true,
        PositionClass = ToastPositions.TopRight,
        TimeOut = 4000,
        CloseButton = true
    });

builder.Services.AddDbContext<DbSolaresCampo>(options =>
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        options.UseNpgsql(databaseUrl + ";SSL Mode=Require;Trust Server Certificate=true");
    }
    else
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
    }
});

var portVar = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(portVar) && int.TryParse(portVar, out int port))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(port);
    });
}

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Congregacao/Login";
        options.AccessDeniedPath = "/Home/AcessoNegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

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

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.UseNToastNotify();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Congregacao}/{action=Login}/{id?}");

app.Run();
