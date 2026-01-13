using FilaDeCampo.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ================= PORTA (Railway)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ================= MVC
builder.Services.AddControllersWithViews();

// ================= DB
builder.Services.AddDbContext<DbSolaresCampo>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

// ================= SESSION
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

// ================= ROTAS
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Congregacao}/{action=Login}/{id?}");

// ================= TESTE
app.MapGet("/health", () => "OK");

app.Run();
