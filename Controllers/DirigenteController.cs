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

    // ================= INDEX =================
    public async Task<IActionResult> Index()
    {
        // Pega a congregação logada
        int congregacaoId = HttpContext.Session.GetInt32("CongregacaoId")!.Value;

        // Lista só os dirigentes da congregação, ordenados pelo rodízio
        var dirigentes = await _dbSolares.Dirigentes
            .AsNoTracking()
            .Where(d => d.CongregacaoId == congregacaoId)
            .OrderBy(d => d.OrdemRodizio)
            .ToListAsync();

        return View(dirigentes);
    }

    // ================= CRIAR =================
    public IActionResult Criar() => View();

    [HttpPost]
    public async Task<IActionResult> Criar(Dirigente dirigente)
    {
        if (!ModelState.IsValid)
            return View(dirigente);

        // Pega a congregação logada
        int congregacaoId = HttpContext.Session.GetInt32("CongregacaoId")!.Value;

        // Define a congregação do dirigente
        dirigente.CongregacaoId = congregacaoId;

        // Calcula a próxima ordem de rodízio
        int ultimo = await _dbSolares.Dirigentes
            .AsNoTracking()
            .Where(d => d.CongregacaoId == congregacaoId)
            .MaxAsync(d => (int?)d.OrdemRodizio) ?? 0;

        dirigente.OrdemRodizio = ultimo + 1;
        dirigente.Ativo = true;

        _dbSolares.Dirigentes.Add(dirigente);
        await _dbSolares.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // ================= EDITAR =================
    public async Task<IActionResult> Editar(int id)
    {
        int congregacaoId = HttpContext.Session.GetInt32("CongregacaoId")!.Value;

        var dirigente = await _dbSolares.Dirigentes
            .Where(d => d.CongregacaoId == congregacaoId)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dirigente == null)
            return NotFound();

        return View(dirigente);
    }

    [HttpPost]
    public async Task<IActionResult> Editar(Dirigente dirigente)
    {
        if (!ModelState.IsValid)
            return View(dirigente);

        // Garante que só edita dirigentes da congregação logada
        int congregacaoId = HttpContext.Session.GetInt32("CongregacaoId")!.Value;
        dirigente.CongregacaoId = congregacaoId;

        _dbSolares.Update(dirigente);
        await _dbSolares.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
