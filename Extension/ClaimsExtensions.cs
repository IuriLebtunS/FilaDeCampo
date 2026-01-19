using System.Security.Claims;

namespace FilaDeCampo.Extensions
{
    public static class ClaimsExtensions
    {
        public static int GetCongregacaoId(this ClaimsPrincipal user)
        {
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("Usuário não autenticado.");

            var claim = user.FindFirst("CongregacaoId");

            if (claim == null)
                throw new UnauthorizedAccessException("Claim CongregacaoId não encontrada.");

            return int.Parse(claim.Value);
        }
    }
}
