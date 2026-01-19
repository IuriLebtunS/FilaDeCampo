using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using FilaDeCampo.Data;

public class ValidateCongregacaoFilter : IAsyncAuthorizationFilter
{
    private readonly DbSolaresCampo _context;

    public ValidateCongregacaoFilter(DbSolaresCampo context)
    {
        _context = context;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new RedirectToActionResult(
                "Login", "Congregacao", null
            );
            return;
        }

        var claim = user.FindFirst("CongregacaoId");

        if (claim == null || !int.TryParse(claim.Value, out int congregacaoId))
        {
            await context.HttpContext.SignOutAsync();
            context.Result = new RedirectToActionResult(
                "Login", "Congregacao", null
            );
            return;
        }

        bool existe = await _context.Congregacoes
            .AsNoTracking()
            .AnyAsync(c => c.Id == congregacaoId);

        if (!existe)
        {
            await context.HttpContext.SignOutAsync();
            context.Result = new RedirectToActionResult(
                "Login", "Congregacao", null
            );
        }
    }
}
