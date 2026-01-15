using FilaDeCampo.Data;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

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
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")
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
// 🚀 CONFIGURAÇÃO OBRIGATÓRIA PARA RAILWAY
// ================================
var portVar = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrEmpty(portVar) && int.TryParse(portVar, out int port))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(port);
    });
}

// ================================
// Build
// ================================
var app = builder.Build();

// ================================
// Middlewares
// ================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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
