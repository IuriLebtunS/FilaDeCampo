using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FilaDeCampo.Data;
using FilaDeCampo.Models;
using FilaDeCampo.Extensions;
using FilaDeCampo.ViewModels.TecnicoAudioVideo;

namespace FilaDeCampo.Controllers;

[Authorize]
public class TecnicoAudioVideoController : Controller
{
    private readonly DbSolaresCampo _db;

    public TecnicoAudioVideoController(DbSolaresCampo db)
    {
        _db = db;
    }

    // ================= INDEX =================
    public async Task<IActionResult> Index()
    {
        int congregacaoId = User.GetCongregacaoId();

        var tecnicos = await _db.TecnicosAudioVideo
            .Where(t => t.CongregacaoId == congregacaoId)
            .OrderBy(t => t.OrdemRodizio)
            .ToListAsync();

        return View(tecnicos);
    }

    // ================= CRIAR =================
    public IActionResult Criar()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarTecnicoAudioVideoVM viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        int congregacaoId = User.GetCongregacaoId();

        int ultimo = await _db.TecnicosAudioVideo
            .Where(t => t.CongregacaoId == congregacaoId)
            .MaxAsync(t => (int?)t.OrdemRodizio) ?? 0;

        var tecnico = new TecnicoAudioVideo
        {
            Nome = viewModel.Nome,
            FuncaoPermitida = viewModel.FuncaoPermitida,
            Ativo = true,
            OrdemRodizio = ultimo + 1,
            CongregacaoId = congregacaoId
        };

        _db.TecnicosAudioVideo.Add(tecnico);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // ================= EDITAR =================
    public async Task<IActionResult> Editar(int id)
    {
        int congregacaoId = User.GetCongregacaoId();

        var tecnico = await _db.TecnicosAudioVideo
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.CongregacaoId == congregacaoId);

        if (tecnico == null)
            return NotFound();

        var vm = new EditarTecnicoAudioVideoVM
        {
            Id = tecnico.Id,
            Nome = tecnico.Nome,
            FuncaoPermitida = tecnico.FuncaoPermitida,
            Ativo = tecnico.Ativo
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarTecnicoAudioVideoVM viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        int congregacaoId = User.GetCongregacaoId();

        var tecnico = await _db.TecnicosAudioVideo
            .FirstOrDefaultAsync(t =>
                t.Id == viewModel.Id &&
                t.CongregacaoId == congregacaoId);

        if (tecnico == null)
            return NotFound();

        tecnico.Nome = viewModel.Nome;
        tecnico.FuncaoPermitida = viewModel.FuncaoPermitida;
        tecnico.Ativo = viewModel.Ativo;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
