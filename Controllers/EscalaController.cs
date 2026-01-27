using Microsoft.AspNetCore.Mvc.Rendering;
using FilaDeCampo.ViewModels.Escala;
using Microsoft.EntityFrameworkCore;
using FilaDeCampo.Extensions;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;
using System.Globalization;
using FilaDeCampo.Models;
using FilaDeCampo.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;


namespace FilaDeCampo.Controllers;

[Authorize]
public class EscalaController : Controller
{
    private readonly DbSolaresCampo _dbSolares;


    public EscalaController(DbSolaresCampo dbSolares)
    {
        _dbSolares = dbSolares;
    }

    // ================= INDEX =================
    [Authorize]
    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 10;
        int congregacaoId = User.GetCongregacaoId();

        var meses = await _dbSolares.Escalas
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

        return View(meses.ToPagedList(page, pageSize));
    }

    // ================= DETALHES =================
    public async Task<IActionResult> Detalhes(int mes, int ano)
    {
        int congregacaoId = User.GetCongregacaoId();

        var escalas = await _dbSolares.Escalas
            .AsNoTracking()
            .Include(e => e.Dirigente)
            .Where(e =>
                e.Data.Month == mes &&
                e.Data.Year == ano &&
                e.Dirigente.CongregacaoId == congregacaoId)
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

        return View(new EscalaDetalheVM
        {
            Mes = mes,
            Ano = ano,
            Sabados = escalas
        });
    }

    // ================= CRIAR =================
    public IActionResult Criar()
    {
        ViewData["MesAtual"] = DateTime.Now.Month;
        ViewData["AnoAtual"] = DateTime.Now.Year;
        ViewData["QtdMeses"] = 1;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Criar(int mes, int ano, int quantidadeMeses)
    {
        int congregacaoId = User.GetCongregacaoId();

        if (mes is < 1 or > 12)
            ModelState.AddModelError(nameof(mes), "Mês inválido.");

        if (ano is < 2000 or > 2100)
            ModelState.AddModelError(nameof(ano), "Ano inválido.");

        if (quantidadeMeses is < 1 or > 3)
            ModelState.AddModelError(nameof(quantidadeMeses), "A quantidade deve ser entre 1 e 3.");

        var dirigentes = await _dbSolares.Dirigentes
            .AsNoTracking()
            .Where(d => d.Ativo && d.CongregacaoId == congregacaoId)
            .OrderBy(d => d.OrdemRodizio)
            .ToListAsync();

        if (!dirigentes.Any())
            ModelState.AddModelError("dirigentes", "Não há dirigentes ativos cadastrados.");

        if (!ModelState.IsValid)
        {
            ViewData["MesAtual"] = mes;
            ViewData["AnoAtual"] = ano;
            ViewData["QtdMeses"] = quantidadeMeses;
            return View();
        }

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
                .Where(e =>
                    e.Data.Year == anoAtual &&
                    e.Data.Month == mesAtual &&
                    e.CongregacaoId == congregacaoId)
                .Select(e => e.Data.Date)
                .ToListAsync();

            for (var data = new DateTime(anoAtual, mesAtual, 1);
                 data.Month == mesAtual;
                 data = data.AddDays(1))
            {
                if (data.DayOfWeek != DayOfWeek.Saturday ||
                    datasExistentes.Contains(data.Date))
                    continue;

                var dirigente = dirigentes[dirigenteIndex % dirigentes.Count];

                _dbSolares.Escalas.Add(new EscalaDeSabado
                {
                    Data = DateTime.SpecifyKind(data, DateTimeKind.Utc),
                    DirigenteId = dirigente.Id,
                    CongregacaoId = dirigente.CongregacaoId // 🔥 NÃO use o User aqui
                });

                dirigenteIndex++;
            }
        }

        try
        {
            await _dbSolares.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        TempData["Success"] = "Escalas criadas com sucesso!";
        return RedirectToAction(nameof(Index));
    }

    // ================= EDITAR =================
    public async Task<IActionResult> Editar(int id)
    {
        int congregacaoId = User.GetCongregacaoId();

        var escala = await _dbSolares.Escalas
            .Include(e => e.Dirigente)
            .FirstOrDefaultAsync(e =>
                e.Id == id &&
                e.Dirigente.CongregacaoId == congregacaoId);

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

        return View(new EditarEscalaVM
        {
            EscalaId = escala.Id,
            DirigenteId = escala.DirigenteId
        });
    }

    [HttpPost]
    public async Task<IActionResult> Editar(EditarEscalaVM model)
    {
        int congregacaoId = User.GetCongregacaoId();

        var escala = await _dbSolares.Escalas
            .Include(e => e.Dirigente)
            .FirstOrDefaultAsync(e =>
                e.Id == model.EscalaId &&
                e.Dirigente.CongregacaoId == congregacaoId);

        if (escala == null)
            return NotFound();

        escala.DirigenteId = model.DirigenteId;
        await _dbSolares.SaveChangesAsync();

        return RedirectToAction(nameof(Detalhes),
            new { mes = escala.Data.Month, ano = escala.Data.Year });
    }

    // ================= EXPORTAR =================
    public async Task<IActionResult> ExportarExcelPeriodo(int mesInicial, int anoInicial, int quantidadeMeses)
    {
        quantidadeMeses = Math.Clamp(quantidadeMeses, 1, 3);
        int congregacaoId = User.GetCongregacaoId();

        var dataInicio = new DateTime(anoInicial, mesInicial, 1, 0, 0, 0, DateTimeKind.Utc);
        var dataFim = dataInicio.AddMonths(quantidadeMeses).AddDays(-1);
        
        var escalas = await _dbSolares.Escalas
            .AsNoTracking()
            .Include(e => e.Dirigente)
            .Where(e =>
                e.Data >= dataInicio &&
                e.Data <= dataFim &&
                e.Dirigente.CongregacaoId == congregacaoId)
            .OrderBy(e => e.Data)
            .ToListAsync();

        if (!escalas.Any())
            return NotFound("Nenhuma escala encontrada.");

        using var workbook = new XLWorkbook();

        foreach (var grupo in escalas.GroupBy(e => new { e.Data.Year, e.Data.Month }))
        {
            var ws = workbook.Worksheets.Add($"{grupo.Key.Month:D2}-{grupo.Key.Year}");

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
                ws.Cell(linha++, 3).Value = escala.Dirigente.Nome;
            }

            ws.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Escala_{dataInicio:MM-yyyy}_a_{dataFim:MM-yyyy}.xlsx"
        );
    }
}
