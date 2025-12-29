using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FilaDeCampo.Models;
using FilaDeCampo.Data;

namespace FilaDeCampo.Controllers;

public class DirigenteController : Controller
{
    private readonly DbSolaresCampo _dbSolares;

    public DirigenteController(DbSolaresCampo dbSolares)
    {
        _dbSolares = dbSolares;
    }

    public async Task<IActionResult> Index()
    {
        var dirigentes = await _dbSolares.Dirigentes
            .OrderBy(d => d.OrdemRodizio)
            .ToListAsync();

        ViewData["Dirigentes"] = dirigentes;
        return View();
    }

    public IActionResult Criar()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(Dirigente dirigente)
    {
        if (!ModelState.IsValid)
            return View(dirigente);

        int ultimo = await _dbSolares.Dirigentes
            .MaxAsync(d => (int?)d.OrdemRodizio) ?? 0;

        dirigente.OrdemRodizio = ultimo + 1;
        dirigente.Ativo = true;

        _dbSolares.Dirigentes.Add(dirigente);
        await _dbSolares.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var dirigente = await _dbSolares.Dirigentes.FindAsync(id);
        if (dirigente == null)
            return NotFound();

        return View(dirigente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Dirigente dirigente)
    {
        if (!ModelState.IsValid)
            return View(dirigente);

        _dbSolares.Update(dirigente);
        await _dbSolares.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}