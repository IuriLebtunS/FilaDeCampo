using FilaDeCampo.Data;
using FilaDeCampo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class CongregacaoController : Controller
{
    private readonly DbSolaresCampo _db;

    private const string MasterEmail = "Lebtuniuri@gmail.com";
    private const string MasterSenha = "Mortadela1";

    public CongregacaoController(DbSolaresCampo db)
    {
        _db = db;
    }

    // ===================== LOGIN CONGREGAÇÃO =====================

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        try
        {
            var vm = new LoginCongreVM
            {
                Congregacoes = await _db.Congregacoes
                    .Where(c => c.Ativa)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Nome
                    })
                    .ToListAsync()
            };

            return View(vm);
        }
        catch (Exception)
        {
            // Evita HTTP 500 caso DB não esteja configurado
            return Content("Erro ao carregar congregações. Verifique o banco de dados.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginCongreVM model)
    {
        try
        {
            model.Congregacoes = await _db.Congregacoes
                .Where(c => c.Ativa)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Nome
                })
                .ToListAsync();

            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.ChaveAcesso))
            {
                ModelState.AddModelError("", "Informe a chave de acesso.");
                return View(model);
            }

            var chave = model.ChaveAcesso.Trim().ToLower();

            var congregacao = await _db.Congregacoes.FirstOrDefaultAsync(c =>
                c.Id == model.CongregacaoId &&
                c.ChaveAcesso != null &&
                c.ChaveAcesso.Trim().ToLower() == chave &&
                c.Ativa);

            if (congregacao == null)
            {
                ModelState.AddModelError("", "Chave inválida para esta congregação.");
                return View(model);
            }

            // Proteção extra para Session
            HttpContext.Session?.SetInt32("CongregacaoId", congregacao.Id);
            HttpContext.Session?.SetString("CongregacaoNome", congregacao.Nome);
            HttpContext.Session?.SetString("Perfil", "Congregacao");

            return RedirectToAction("Index", "Escala");
        }
        catch (Exception)
        {
            return Content("Erro interno no login. Verifique configuração do sistema.");
        }
    }

    // ===================== LOGIN MASTER =====================

    [HttpGet]
    public IActionResult LoginMaster()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult LoginMaster(string usuario, string senha)
    {
        if (!string.IsNullOrWhiteSpace(usuario) &&
            usuario.Equals(MasterEmail, StringComparison.OrdinalIgnoreCase) &&
            senha == MasterSenha)
        {
            HttpContext.Session?.SetString("Perfil", "Master");
            HttpContext.Session?.SetString("NomeUsuario", "Iuri");

            return RedirectToAction("DashboardMaster");
        }

        ModelState.AddModelError("", "Usuário ou senha incorretos.");
        return View();
    }

    // ===================== DASHBOARD MASTER =====================

    [HttpGet]
    public IActionResult DashboardMaster()
    {
        if (HttpContext.Session?.GetString("Perfil") != "Master")
            return Forbid();

        ViewData["NomeUsuario"] =
            HttpContext.Session.GetString("NomeUsuario") ?? "Master";

        return View();
    }

    // ===================== LOGOUT =====================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session?.Clear();
        return RedirectToAction("Login", "Congregacao");
    }

    // ===================== CRIAR CONGREGAÇÃO =====================

    [HttpGet]
    public IActionResult Criar()
    {
        if (HttpContext.Session?.GetString("Perfil") != "Master")
            return Forbid();

        return View(new CriarCongreVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarCongreVM model)
    {
        try
        {
            if (HttpContext.Session?.GetString("Perfil") != "Master")
                return Forbid();

            if (!ModelState.IsValid)
                return View(model);

            if (await _db.Congregacoes.AnyAsync(c => c.Nome == model.Nome))
            {
                ModelState.AddModelError("", "Já existe uma congregação com este nome.");
                return View(model);
            }

            if (await _db.Congregacoes.AnyAsync(c => c.ChaveAcesso == model.ChaveAcesso))
            {
                ModelState.AddModelError("", "Esta chave de acesso já está em uso.");
                return View(model);
            }

            var congregacao = new FilaDeCampo.Models.Congregacao
            {
                Nome = model.Nome,
                ChaveAcesso = model.ChaveAcesso,
                Ativa = model.Ativa
            };

            _db.Congregacoes.Add(congregacao);
            await _db.SaveChangesAsync();

            TempData["Mensagem"] =
                $"Congregação '{model.Nome}' criada com sucesso!";

            return RedirectToAction("Login", "Congregacao");
        }
        catch (Exception)
        {
            return Content("Erro ao criar congregação. Verifique o banco.");
        }
    }
}
