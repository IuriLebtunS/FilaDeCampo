using System.Diagnostics;
using FilaDeCampo.Data;
using FilaDeCampo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FilaDeCampo.Controllers;

public class HomeController : Controller
{
    private readonly DbSolaresCampo _dbSolares;

    public HomeController(DbSolaresCampo dbSolares)
    {
        _dbSolares = dbSolares;
    }

    public async Task<IActionResult> Index()
    {
        int mesAtual = DateTime.Now.Month;
        int anoAtual = DateTime.Now.Year;

        var escalas = await _dbSolares.Escalas
            .Include(e => e.Dirigente)
            .Where(e => e.Data.Month == mesAtual && e.Data.Year == anoAtual)
            .OrderBy(e => e.Data)
            .ToListAsync();

        ViewData["Mes"] = mesAtual;
        ViewData["Ano"] = anoAtual;
        ViewData["Escalas"] = escalas;

        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
