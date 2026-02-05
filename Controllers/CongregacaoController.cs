using System.Security.Claims;
using FilaDeCampo.Data;
using FilaDeCampo.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

public class CongregacaoController : Controller
{
    private readonly DbSolaresCampo _db;
    private readonly IToastNotification _notification;
    private const string MasterEmail = "Lebtuniuri@gmail.com";
    private const string MasterSenha = "Mortadela1";

    public CongregacaoController(DbSolaresCampo db, IToastNotification notification)
    {
        _db = db;
        _notification = notification;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login()
    {
        // Limpa sessão e cookie ao entrar no login
        await HttpContext.SignOutAsync();
        HttpContext.Session.Clear();

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

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginCongreVM model)
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

        var congregacao = await _db.Congregacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Id == model.CongregacaoId &&
                c.Ativa &&
                c.ChaveAcesso.ToLower() == model.ChaveAcesso.Trim().ToLower());

        if (congregacao == null)
        {
            ModelState.AddModelError("", "Chave inválida para esta congregação.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new("CongregacaoId", congregacao.Id.ToString()),
            new Claim(ClaimTypes.Name, congregacao.Nome),
            new Claim(ClaimTypes.Role, "Congregacao")
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        return RedirectToAction("Index", "Escala");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult LoginMaster() => View();

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> LoginMaster(string usuario, string senha)
    {
        if (usuario.Equals(MasterEmail, StringComparison.OrdinalIgnoreCase)
            && senha == MasterSenha)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Iuri"),
                new Claim(ClaimTypes.Role, "Master")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return RedirectToAction("DashboardMaster");
        }

        ModelState.AddModelError("", "Usuário ou senha incorretos.");
        return View();
    }

    [Authorize(Roles = "Master")]
    public IActionResult DashboardMaster()
    {
        return View();
    }


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Criar()
    {
        if (HttpContext.Session.GetString("Perfil") != "Master")
            return Forbid();

        return View(new CriarCongreVM());
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarCongreVM model)
    {
        if (HttpContext.Session.GetString("Perfil") != "Master")
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

        _notification.AddSuccessToastMessage($"Congregação '{model.Nome}' criada com sucesso!");


        return RedirectToAction("Login", "Congregacao");
    }
}
