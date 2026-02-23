using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace SilverbridgeWeb.WebUI.Authentication;

/// <summary>
/// Maps Keycloak realm roles from the "realm_access.roles" claim into individual
/// role claims so ASP.NET Core authorization (RequireRole, AuthorizeView Policy) works.
/// Keycloak sends realm roles as a single claim with a JSON array value.
/// </summary>
internal sealed class KeycloakRolesClaimsTransformation : IClaimsTransformation
{
    private const string RealmAccessRolesClaimType = "realm_access.roles";
    private const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return Task.FromResult(principal);
        }

        var realmRoles = principal.FindAll(RealmAccessRolesClaimType).ToList();
        if (realmRoles.Count == 0)
        {
            return Task.FromResult(principal);
        }

        foreach (Claim? claim in realmRoles)
        {
            if (string.IsNullOrEmpty(claim.Value))
            {
                continue;
            }

            // Keycloak can send: (a) one claim per role, or (b) one claim with JSON array
            if (claim.Value.StartsWith('['))
            {
                try
                {
                    string[]? roles = JsonSerializer.Deserialize<string[]>(claim.Value);
                    if (roles != null)
                    {
                        foreach (string role in roles)
                        {
                            if (!string.IsNullOrEmpty(role) && !identity.HasClaim(RoleClaimType, role))
                            {
                                identity.AddClaim(new Claim(RoleClaimType, role));
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Single role as plain value
                    if (!identity.HasClaim(RoleClaimType, claim.Value))
                    {
                        identity.AddClaim(new Claim(RoleClaimType, claim.Value));
                    }
                }
            }
            else
            {
                if (!identity.HasClaim(RoleClaimType, claim.Value))
                {
                    identity.AddClaim(new Claim(RoleClaimType, claim.Value));
                }
            }
        }

        return Task.FromResult(principal);
    }
}
