using IdentityServer4.Services;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using IdentityServerWithAspNetIdentity.Models;
using System.Security.Claims;
using IdentityModel;

namespace IdentityServerWithAspNetIdentity.Services
{
    public class CustomProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory;

        public CustomProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
        {
            this.userManager = userManager;
            this.claimsFactory = claimsFactory;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.FindFirst("sub").Value;
            var user = await userManager.FindByIdAsync(sub);
            var principal = await claimsFactory.CreateAsync(user);

            var claims = principal.Claims.ToList();
            claims.Add(new Claim(JwtClaimTypes.GivenName, user.UserName));
            
            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.FindFirst("sub").Value;
            var user = await userManager.FindByIdAsync(sub);

            context.IsActive = user != null;
        }
    }
}
