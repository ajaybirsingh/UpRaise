using UpRaise.Constants;
using System.Security.Claims;
using System.Security.Principal;

namespace UpRaise.Extensions
{
    public static class IdentityExtensions
    {
        public static int? GetUserId(this IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            Claim claim = claimsIdentity?.FindFirst(CustomClaimTypes.UserId);

            if (claim != null)
            {
                int userId = 0;
                if (int.TryParse(claim.Value, out userId))
                    return userId;
            }

            return null;
        }

    }
}
