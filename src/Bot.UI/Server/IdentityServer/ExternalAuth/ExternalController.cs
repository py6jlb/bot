using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Server.IdentityServer.Attributes;
using Server.IdentityServer.DataAccess.Entities;
using Server.IdentityServer.Extensions;
using Shared;

namespace Server.IdentityServer.ExternalAuth
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class ExternalController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;
        private readonly ILogger<ExternalController> _logger;
        
        public ExternalController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IEventService events,
            ILogger<ExternalController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _events = events;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Challenge(string scheme, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
                throw new Exception("invalid return URL");

            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    {"returnUrl", returnUrl},
                    {"scheme", scheme},
                }
            };

            return Challenge(props, scheme);
        }

        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
                throw new Exception("External authentication error");

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                _logger.LogDebug("External claims: {@claims}", externalClaims);
            }

            var (user, provider, providerUserId, claims) = await FindUserFromExternalProviderAsync(result);
            if (user == null)
            {
                user = await AutoProvisionUserAsync(provider, providerUserId, claims);
            }
            else
            {
                user = await AutoUpdateUserAsync(provider, providerUserId, claims);
            }

            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);

            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            additionalLocalClaims.AddRange(principal.Claims);
            var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.Id;

            var isuser = new IdentityServerUser(user.Id)
            {
                DisplayName = name,
                IdentityProvider = provider,
                AdditionalClaims = additionalLocalClaims
            };

            await HttpContext.SignInAsync(isuser, localSignInProps);

            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, name, true,
                context?.Client.ClientId));

            if (context != null)
            {
                if (context.IsNativeClient())
                {
                    return this.LoadingPage("Redirect", returnUrl);
                }
            }

            return Redirect(returnUrl);
        }

        private async Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)> FindUserFromExternalProviderAsync(AuthenticateResult result)
        {
            var externalUser = result.Principal;
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ?? throw new Exception("Unknown userid");
            
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);
            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;
            var user = await _userManager.FindByLoginAsync(provider, providerUserId);
            return (user, provider, providerUserId, claims);
        }

        private async Task<ApplicationUser> AutoProvisionUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            var (filtered, scopeData, name, surname, ogrn) = GetClaims(claims);

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = name,
                DisplayName = filtered.FirstOrDefault(x => x.Type == "displayname")?.Value,
                UserData = scopeData,
                Surname = surname
            };
            
            var identityResult = await _userManager.CreateAsync(user);
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

            if (filtered.Any())
            {
                identityResult = await _userManager.AddClaimsAsync(user, filtered);
                if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
            }
            
            identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

            var roles = filtered.Where(x => x.Type == JwtClaimTypes.Role).ToArray();
            if (!roles.Any()) return user;
            
            foreach (var role in roles)
            {
                var roleExists = await _roleManager.RoleExistsAsync(role.Value);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(role.Value));
                }

                await _userManager.AddToRoleAsync(user, role.Value);
            }

            return user;
        }

        private async Task<ApplicationUser> AutoUpdateUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            var user = await _userManager.FindByLoginAsync(provider, providerUserId);

            var (filtered, scopeData, firstName, surname, ogrn) = GetClaims(claims);
            var roles = filtered.Where(x => x.Type == JwtClaimTypes.Role).ToArray();

            user.UserData = scopeData;
            user.Name = firstName;
            user.Surname = surname;
            user.DisplayName = filtered.FirstOrDefault(x => x.Type == "displayname")?.Value;

            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

            var userRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, userRoles);

            if (roles.Any())
            {
                foreach (var role in roles)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(role.Value);
                    if (!roleExists)
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role.Value));
                    }

                    await _userManager.AddToRoleAsync(user, role.Value);
                }
            }

            if (!filtered.Any()) return user;
            
            var userClaims = await _userManager.GetClaimsAsync(user);
            await _userManager.RemoveClaimsAsync(user, userClaims);
            identityResult = await _userManager.AddClaimsAsync(user, filtered);
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
            
            return user;
        }

        private (List<Claim> filtered, string scopeData, string name,  string surname, string ogrn) GetClaims(IEnumerable<Claim> claims)
        {
            var claimArray = claims as Claim[] ?? claims.ToArray();
            var (filtered, scopeData, name, lastName, ogrn) = 
                new Tuple<List<Claim>, string, string, string, string>(new List<Claim>(), null, null, null, null);

            foreach (var claim in claimArray)
            {
                switch (claim.Type)
                {
                    case JwtClaimTypes.Name:
                    case ClaimTypes.Name:
                    {
                        name = claimArray.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                               claimArray.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                    
                        if (name != null)
                            filtered.Add(new Claim(JwtClaimTypes.Name, name));
                        break;
                    }
                    case JwtClaimTypes.GivenName:
                    case ClaimTypes.GivenName:
                    {
                        var givenName = claimArray.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                                        claimArray.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
                        if (givenName != null)
                            filtered.Add(new Claim(JwtClaimTypes.GivenName, givenName));
                        break;
                    }
                    case "LastName":
                    case JwtClaimTypes.FamilyName:
                    case ClaimTypes.Surname:
                    {
                        lastName = claimArray.FirstOrDefault(x => x.Type == "LastName")?.Value ??
                                   claimArray.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                                   claimArray.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
                        if (lastName != null)
                            filtered.Add(new Claim(JwtClaimTypes.FamilyName, lastName));
                        break;
                    }
                    case "PatronymicName":
                    {
                        var patronymic = claimArray.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
                        if (patronymic != null)
                            filtered.Add(new Claim(CustomClaimTypes.Patronymic, patronymic));
                        break;
                    }
                    case "Email":
                    case JwtClaimTypes.Email:
                    case ClaimTypes.Email:
                    {
                        var email = claimArray.FirstOrDefault(x => x.Type == "Email")?.Value ??
                                    claimArray.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
                                    claimArray.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                        if (email != null)
                            filtered.Add(new Claim(JwtClaimTypes.Email, email));
                        break;
                    }
                    case ClaimTypes.UserData:
                    {
                        scopeData = claimArray.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
                        if (scopeData != null)
                        {
                            filtered.Add(new Claim(CustomClaimTypes.UserData, scopeData));
                            var userData = JObject.Parse(scopeData);
                            var org = userData["org"] ?? null;
                            ogrn = org?["ogrn"].ToObject<string>();
                            if (!string.IsNullOrWhiteSpace(ogrn))
                            {
                                filtered.Add(new Claim(CustomClaimTypes.Ogrn, ogrn));
                            }
                        }
                        break;
                    }
                    case ClaimTypes.Role:
                    {
                        filtered.Add(new Claim(JwtClaimTypes.Role, claim.Value));
                        break;
                    }
                    case "http://schemas.xmlsoap.org/claims/distinguishedname":
                    {
                        filtered.Add(new Claim(CustomClaimTypes.DistinguishedName, claim.Value));
                        break;
                    }
                    case "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider":
                    {
                        filtered.Add(new Claim(CustomClaimTypes.IdentityProvider, claim.Value));
                        break; 
                    }
                    case "http://schemas.xmlsoap.org/claims/displayname":
                    {
                        filtered.Add(new Claim(CustomClaimTypes.DisplayName, claim.Value));
                        break;
                    }
                    case ClaimTypes.Upn:
                    {
                        filtered.Add(new Claim(CustomClaimTypes.Login, claim.Value));
                        break;
                    }
                    default:
                    {
                        filtered.Add(claim);
                        break;
                    }
                }
            }

            return (filtered, scopeData, name, lastName, ogrn);
        }

        // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
        // this will be different for WS-Fed, SAML2p or other protocols
        private void ProcessLoginCallback(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[] {new AuthenticationToken {Name = "id_token", Value = idToken}});
            }
        }
    }
}