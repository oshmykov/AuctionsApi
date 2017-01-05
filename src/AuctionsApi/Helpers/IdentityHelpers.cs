using System.Linq;
using System.Security.Claims;

namespace AuctionsApi.Helpers
{
    public static class IdentityHelpers
    {
        public static string GetSubject(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims == null)
            {
                return null;
            }

            var sub = user.Claims.FirstOrDefault(p => p.Type.Equals("sub"));
            if (sub == null)
            {
                return null;
            }

            return sub.Value;
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            if (user == null || user.Claims == null)
            {
                return null;
            }

            var sub = user.Claims.FirstOrDefault(p => p.Type.Equals("name"));
            if (sub == null)
            {
                return null;
            }

            return sub.Value;
        }
    }
}
