using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Server.IdentityServer.DataAccess.Entities;

namespace Server.IdentityServer.Services
{
    public class IdentityProfileService: IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public IdentityProfileService(
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, 
            UserManager<ApplicationUser> userManager)
        {
            _claimsFactory = claimsFactory;
            _userManager = userManager;
        }
        
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            if (user == null)
                throw new NullReferenceException("user not be null");
            var claims = new List<Claim> {new Claim(JwtClaimTypes.Subject, user.Id)};
            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims.Where(x=>x.Type != "user_data"));
            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}