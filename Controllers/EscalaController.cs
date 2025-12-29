using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FilaDeCampo.Models;
using FilaDeCampo.Data;


namespace FilaDeCampo.Controllers;

public class EscalaController : Controller
{
    private readonly DbSolaresCampo _dbSolares;

    public EscalaController(DbSolaresCampo dbsolares)
    {
        _dbSolares = dbsolares;
    }

    public async Task<IActionResult> Index(int? mes, int? ano)
    {
        int mesAtual = mes ?? DateTime.Now.Month;
        int anoAtual = ano ?? DateTime.Now.Year;

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

    public async Task<IActionResult> Detalhes(int id)
    {
        var escala = await _dbSolares.Escalas
            .Include(e => e.Dirigente)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (escala == null)
            return NotFound();

        return View(escala);
    }


    public IActionResult Criar()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(int mes, int ano)
    {
        var dirigentes = await _dbSolares.Dirigentes
            .Where(d => d.Ativo)
            .OrderBy(d => d.OrdemRodizio)
            .ToListAsync();

        if (!dirigentes.Any())
            return RedirectToAction(nameof(Index));

        var data = new DateTime(ano, mes, 1);
        var sabados = new List<DateTime>();

        while (data.Month == mes)
        {
            if (data.DayOfWeek == DayOfWeek.Saturday)
                sabados.Add(data);

            data = data.AddDays(1);
        }

        int index = 0;

        foreach (var sabado in sabados)
        {
            var dirigente = dirigentes[index % dirigentes.Count];

            _dbSolares.Escalas.Add(new EscalaDeSabado
            {
                Data = sabado,
                DirigenteId = dirigente.Id
            });

            index++;
        }

        await _dbSolares.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { mes, ano });
    }
    
    public async Task<IActionResult> Editar(int id)
    {
        var escala = await _dbSolares.Escalas.FindAsync(id);
        if (escala == null)
            return NotFound();

        ViewData["Dirigentes"] = await _dbSolares.Dirigentes
            .Where(d => d.Ativo)
            .OrderBy(d => d.Nome)
            .ToListAsync();

        return View(escala);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EscalaDeSabado escala)
    {
        if (ModelState.IsValid)
        {
            _dbSolares.Update(escala);
            await _dbSolares.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["Dirigentes"] = await _dbSolares.Dirigentes.ToListAsync();
        return View(escala);
    }
}
