using Microsoft.AspNetCore.Mvc.Rendering;
using FilaDeCampo.ViewModels.Escala;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;
using System.Globalization;
using FilaDeCampo.Models;
using FilaDeCampo.Data;
using ClosedXML.Excel;

namespace FilaDeCampo.Controllers;

public class EscalaController : Controller
{
    private readonly DbSolaresCampo _dbSolares;

    public EscalaController(DbSolaresCampo dbSolares)
    {
        _dbSolares = dbSolares;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 10;

        int congregacaoId = HttpContext.Session.GetInt32("CongregacaoId")!.Value;

        var mesesQuery = _dbSolares.Escalas
            .AsNoTracking()
            .Where(e => e.Dirigente.CongregacaoId == congregacaoId)
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
        int congregacaoId = HttpContext.Session.GetInt32("CongregacaoId")!.Value;

        var escalas = await _dbSolares.Escalas
            .AsNoTracking()
            .Include(e => e.Dirigente)
            .Where(e => e.Data.Month == mes && e.Data.Year == ano
                        && e.Dirigente.CongregacaoId == congregacaoId)
            .OrderBy(e => e.Data)
            .Select(e => new EscalaDiaVM
            {
                Id = e.Id,
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
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Criar(int mes, int ano, int quantidadeMeses)
    {
        // ================== Validações ==================
        if (mes < 1 || mes > 12)
            ModelState.AddModelError("mes", "Mês inválido.");

        if (ano < 2000 || ano > 2100)
            ModelState.AddModelError("ano", "Ano inválido.");

        if (quantidadeMeses < 1 || quantidadeMeses > 3)
            ModelState.AddModelError("quantidadeMeses", "A quantidade de meses deve ser entre 1 e 3.");

        int congregacaoId = HttpContext.Session.GetInt32("CongregacaoId") ?? 0;
        if (congregacaoId == 0)
            ModelState.AddModelError("congregacao", "Congregação não encontrada na sessão.");

        var dirigentes = await _dbSolares.Dirigentes
            .Where(d => d.Ativo && d.CongregacaoId == congregacaoId)
            .OrderBy(d => d.OrdemRodizio)
            .AsNoTracking()
            .ToListAsync();

        if (!dirigentes.Any())
            ModelState.AddModelError("dirigentes", "Não há dirigentes ativos cadastrados.");

        // Se houver algum erro, retorna para a view
        if (!ModelState.IsValid)
        {
            ViewData["MesAtual"] = mes;
            ViewData["AnoAtual"] = ano;
            ViewData["QtdMeses"] = quantidadeMeses;
            return View();
        }

        // ================== Criação das escalas ==================
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

            // Datas já cadastradas (apenas a data, sem hora)
            var datasExistentes = await _dbSolares.Escalas
                .Where(e => e.Data.Year == anoAtual &&
                            e.Data.Month == mesAtual &&
                            e.CongregacaoId == congregacaoId)
                .Select(e => e.Data.Date)
                .ToListAsync();

            var data = new DateTime(anoAtual, mesAtual, 1);

            while (data.Month == mesAtual)
            {
                // Só sábado e ainda não cadastrado
                if (data.DayOfWeek == DayOfWeek.Saturday &&
                    !datasExistentes.Contains(data.Date))
                {
                    var dirigente = dirigentes[dirigenteIndex % dirigentes.Count];

                    _dbSolares.Escalas.Add(new EscalaDeSabado
                    {
                        Data = data,
                        DirigenteId = dirigente.Id,
                        CongregacaoId = congregacaoId
                    });

                    dirigenteIndex++;
                }

                data = data.AddDays(1);
            }
        }

        await _dbSolares.SaveChangesAsync();

        TempData["Success"] = "Escalas criadas com sucesso!";
        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Editar(int id)
    {
        int congregacaoId = HttpContext.Session.GetInt32("CongregacaoId")!.Value;

        var escala = await _dbSolares.Escalas
            .Include(e => e.Dirigente)
            .Where(e => e.Dirigente.CongregacaoId == congregacaoId)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (escala == null)
            return NotFound();

        ViewData["Dirigentes"] = new SelectList(
            await _dbSolares.Dirigentes
                .Where(d => d.Ativo && d.CongregacaoId == congregacaoId)
                .OrderBy(d => d.Nome)
                .ToListAsync(),
            "Id",
            "Nome",
            escala.DirigenteId
        );

        var vm = new EditarEscalaVM
        {
            EscalaId = escala.Id,
            DirigenteId = escala.DirigenteId
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarEscalaVM model)
    {
        int congregacaoId = HttpContext.Session.GetInt32("CongregacaoId")!.Value;

        var escala = await _dbSolares.Escalas
            .Include(e => e.Dirigente)
            .Where(e => e.Dirigente.CongregacaoId == congregacaoId)
            .FirstOrDefaultAsync(e => e.Id == model.EscalaId);

        if (escala == null)
            return NotFound();

        escala.DirigenteId = model.DirigenteId;
        await _dbSolares.SaveChangesAsync();

        return RedirectToAction(nameof(Detalhes), new
        {
            mes = escala.Data.Month,
            ano = escala.Data.Year
        });
    }

    // ================= EXPORTAR =================
    public async Task<IActionResult> ExportarExcelPeriodo(int mesInicial, int anoInicial, int quantidadeMeses)
    {
        if (quantidadeMeses < 1) quantidadeMeses = 1;
        if (quantidadeMeses > 3) quantidadeMeses = 3;

        int congregacaoId = HttpContext.Session.GetInt32("CongregacaoId")!.Value;

        var dataInicio = new DateTime(anoInicial, mesInicial, 1);
        var dataFim = dataInicio.AddMonths(quantidadeMeses).AddDays(-1);

        var escalas = await _dbSolares.Escalas
            .AsNoTracking()
            .Include(e => e.Dirigente)
            .Where(e => e.Data >= dataInicio && e.Data <= dataFim
                        && e.Dirigente.CongregacaoId == congregacaoId)
            .OrderBy(e => e.Data)
            .ToListAsync();

        if (!escalas.Any())
            return NotFound("Nenhuma escala encontrada.");

        using var workbook = new XLWorkbook();
        var grupos = escalas
            .GroupBy(e => new { e.Data.Year, e.Data.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month);

        foreach (var grupo in grupos)
        {
            var nomeAba = $"{grupo.Key.Month:D2}-{grupo.Key.Year}";
            var ws = workbook.Worksheets.Add(nomeAba);

            ws.Cell(1, 1).Value = "Data";
            ws.Cell(1, 2).Value = "Dia da Semana";
            ws.Cell(1, 3).Value = "Dirigente";

            ws.Range("A1:C1").Style.Font.Bold = true;
            ws.Range("A1:C1").Style.Fill.BackgroundColor = XLColor.LightGray;

            int linha = 2;

            foreach (var escala in grupo)
            {
                ws.Cell(linha, 1).Value = escala.Data.ToString("dd/MM/yyyy");
                ws.Cell(linha, 2).Value =
                    CultureInfo.GetCultureInfo("pt-BR")
                        .DateTimeFormat.GetDayName(escala.Data.DayOfWeek);
                ws.Cell(linha, 3).Value = escala.Dirigente.Nome;
                linha++;
            }

            ws.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var nomeArquivo = $"Escala_{dataInicio:MM-yyyy}_a_{dataFim:MM-yyyy}.xlsx";

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            nomeArquivo
        );
    }
}
