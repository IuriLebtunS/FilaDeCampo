using Microsoft.EntityFrameworkCore;
using FilaDeCampo.Extensions;
using Microsoft.AspNetCore.Mvc;
using FilaDeCampo.Models;
using FilaDeCampo.Data;
using Microsoft.AspNetCore.Authorization;

namespace FilaDeCampo.Controllers;

[Authorize(Roles = "Congregacao")]
public class DirigenteController : Controller
{
    private readonly DbSolaresCampo _db;

    public DirigenteController(DbSolaresCampo db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        int congregacaoId = User.GetCongregacaoId();

        var dirigentes = await _db.Dirigentes
            .AsNoTracking()
            .Where(d => d.CongregacaoId == congregacaoId)
            .OrderBy(d => d.OrdemRodizio)
            .ToListAsync();

        return View(dirigentes);
    }

    public IActionResult Criar() => View();

    [HttpPost]
    public async Task<IActionResult> Criar(Dirigente dirigente)
    {
        if (!ModelState.IsValid)
            return View(dirigente);

        int congregacaoId = User.GetCongregacaoId();

        dirigente.CongregacaoId = congregacaoId;

        int ultimo = await _db.Dirigentes
            .Where(d => d.CongregacaoId == congregacaoId)
            .MaxAsync(d => (int?)d.OrdemRodizio) ?? 0;

        dirigente.OrdemRodizio = ultimo + 1;
        dirigente.Ativo = true;

        _db.Dirigentes.Add(dirigente);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        int congregacaoId = User.GetCongregacaoId();

        var dirigente = await _db.Dirigentes
            .FirstOrDefaultAsync(d =>
                d.Id == id &&
                d.CongregacaoId == congregacaoId);

        if (dirigente == null)
            return NotFound();

        return View(dirigente);
    }

    [HttpPost]
    public async Task<IActionResult> Editar(Dirigente dirigente)
    {
        if (!ModelState.IsValid)
            return View(dirigente);

        int congregacaoId = User.GetCongregacaoId();

        dirigente.CongregacaoId = congregacaoId;

        _db.Dirigentes.Update(dirigente);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
