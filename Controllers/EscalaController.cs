using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FilaDeCampo.Models;
using FilaDeCampo.Data;


namespace FilaDeCampo.Controllers;

public class EscalaController : Controller
{
    private readonly DbSolaresCampo _context;

    public EscalaController(DbSolaresCampo context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? mes, int? ano)
    {
        int mesAtual = mes ?? DateTime.Now.Month;
        int anoAtual = ano ?? DateTime.Now.Year;

        var escalas = await _context.Escalas
            .Include(e => e.Dirigente)
            .Where(e => e.Data.Month == mesAtual && e.Data.Year == anoAtual)
            .OrderBy(e => e.Data)
            .ToListAsync();

        ViewData["Mes"] = mesAtual;
        ViewData["Ano"] = anoAtual;
        ViewData["Escalas"] = escalas;

        return View();
    }

      public async Task<IActionResult> Detalhes(int id)
    {
        var escala = await _context.Escalas
            .Include(e => e.Dirigente)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (escala == null)
            return NotFound();

        return View(escala);
    }

    // CREATE (GET)
    public async Task<IActionResult> Criar()
    {
        ViewData["Dirigentes"] = await _context.Dirigentes
            .Where(d => d.Ativo)
            .OrderBy(d => d.Nome)
            .ToListAsync();

        return View();
    }

    // CREATE (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(EscalaDeSabado escala)
    {
        if (ModelState.IsValid)
        {
            _context.Add(escala);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["Dirigentes"] = await _context.Dirigentes.ToListAsync();
        return View(escala);
    }
    public async Task<IActionResult> Editar(int id)
    {
        var escala = await _context.Escalas.FindAsync(id);
        if (escala == null)
            return NotFound();

        ViewData["Dirigentes"] = await _context.Dirigentes
            .Where(d => d.Ativo)
            .OrderBy(d => d.Nome)
            .ToListAsync();

        return View(escala);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EscalaDeSabado escala)
    {
        if (id != escala.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(escala);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["Dirigentes"] = await _context.Dirigentes.ToListAsync();
        return View(escala);
    }
}
