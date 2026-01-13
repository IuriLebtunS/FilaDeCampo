using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class MasterOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var perfil = context.HttpContext.Session.GetString("Perfil");
        if (perfil != "Master")
        {
            context.Result = new ForbidResult();
        }
    }
}