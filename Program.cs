using FilaDeCampo.Data;
using Microsoft.EntityFrameworkCore;
using NToastNotify; // <- Import necessário

var builder = WebApplication.CreateBuilder(args);

// MVC + NToastNotify
builder.Services.AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions
    {
        ProgressBar = true,
        PositionClass = ToastPositions.TopRight,
        PreventDuplicates = true,
        CloseButton = true
    });

// DbContext
builder.Services.AddDbContext<DbSolaresCampo>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Middlewares
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();       // <- Session antes dos controllers
app.UseAuthorization();

// **Middleware do NToastNotify**
app.UseNToastNotify();

// Rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Congregacao}/{action=Login}/{id?}");

app.Run();
