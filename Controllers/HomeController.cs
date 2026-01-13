using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private const string MasterEmail = "Lebtuniuri@gmail.com";
    private const string MasterSenha = "Mortadela1";

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
            HttpContext.Session.SetString("Perfil", "Master");
            HttpContext.Session.SetString("NomeUsuario", "Iuri");
            return RedirectToAction("DashboardMaster");
        }

        ModelState.AddModelError("", "Usuário ou senha incorretos.");
        return View();
    }

    [HttpGet]
    public IActionResult DashboardMaster()
    {
        if (HttpContext.Session.GetString("Perfil") != "Master")
            return Forbid();

        ViewBag.NomeUsuario = HttpContext.Session.GetString("NomeUsuario");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Congregacao");
    }
}