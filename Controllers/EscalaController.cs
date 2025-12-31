using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using FilaDeCampo.Models;
using FilaDeCampo.Data;
using FilaDeCampo.ViewModels;
using X.PagedList.Extensions;

namespace FilaDeCampo.Controllers;

public class EscalaController : Controller
{
    private readonly DbSolaresCampo _dbSolares;

    public EscalaController(DbSolaresCampo dbsolares)
    {
        _dbSolares = dbsolares;
    }


    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 10;

        var mesesQuery = _dbSolares.Escalas
            .AsNoTracking()
            .GroupBy(e => new { e.Data.Year, e.Data.Month })
            .Select(g => new EscalaMesVM
            {
                Ano = g.Key.Year,
                Mes = g.Key.Month
            })
            .OrderByDescending(x => x.Ano)
            .ThenByDescending(x => x.Mes)
            .ToListAsync();

        var mesesList = await mesesQuery;
        var mesesPaged = mesesList.ToPagedList(page, pageSize);

        return View(mesesPaged);
    }

    public async Task<IActionResult> Detalhes(int mes, int ano)
    {
        var escalas = await _dbSolares.Escalas
            .AsNoTracking()
            .Include(e => e.Dirigente)
            .Where(e => e.Data.Month == mes && e.Data.Year == ano)
            .OrderBy(e => e.Data)
            .Select(e => new EscalaDiaVM
            {
                Data = e.Data,
                Dirigente = e.Dirigente.Nome,
                DirigenteId = e.Dirigente.Id
            })
            .ToListAsync();

        if (!escalas.Any())
            return NotFound();

        var vm = new EscalaDetalheVM
        {
            Mes = mes,
            Ano = ano,
            Sabados = escalas
        };

        return View(vm);
    }

    public IActionResult Criar()
    {
        ViewData["MesAtual"] = DateTime.Now.Month;
        ViewData["AnoAtual"] = DateTime.Now.Year;
        ViewData["QtdMeses"] = 1;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(int mes, int ano, int quantidadeMeses)
    {
        if (quantidadeMeses < 1)
            quantidadeMeses = 1;

        if (quantidadeMeses > 3)
            quantidadeMeses = 3;

        var dirigentes = await _dbSolares.Dirigentes
            .Where(d => d.Ativo)
            .OrderBy(d => d.OrdemRodizio)
            .AsNoTracking()
            .ToListAsync();

        if (!dirigentes.Any())
            return RedirectToAction(nameof(Index));

        int dirigenteIndex = 0;

        for (int i = 0; i < quantidadeMeses; i++)
        {
            int mesAtual = mes + i;
            int anoAtual = ano;

            if (mesAtual > 12)
            {
                mesAtual -= 12;
                anoAtual++;
            }

            var datasExistentes = await _dbSolares.Escalas
                .Where(e => e.Data.Month == mesAtual && e.Data.Year == anoAtual)
                .Select(e => e.Data)
                .ToListAsync();

            var data = new DateTime(anoAtual, mesAtual, 1);

            while (data.Month == mesAtual)
            {
                if (data.DayOfWeek == DayOfWeek.Saturday &&
                    !datasExistentes.Contains(data))
                {
                    var dirigente = dirigentes[dirigenteIndex % dirigentes.Count];

                    _dbSolares.Escalas.Add(new EscalaDeSabado
                    {
                        Data = data,
                        DirigenteId = dirigente.Id
                    });

                    dirigenteIndex++;
                }

                data = data.AddDays(1);
            }
        }

        await _dbSolares.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Editar(int id)
    {
        var escala = await _dbSolares.Escalas
            .Include(e => e.Dirigente)
            .FirstOrDefaultAsync(e => e.Id == id);

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
    public async Task<IActionResult> Editar(int id, int dirigenteId)
    {
        var escala = await _dbSolares.Escalas.FindAsync(id);
        if (escala == null)
            return NotFound();

        escala.DirigenteId = dirigenteId;

        await _dbSolares.SaveChangesAsync();

        return RedirectToAction(nameof(Detalhes), new
        {
            mes = escala.Data.Month,
            ano = escala.Data.Year
        });
    }

}
