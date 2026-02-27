using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;

namespace Kartverket.Metadatakatalog.Services.Authentication
{
    /// <summary>
    /// Implementation of Geonorge authentication service
    /// Provides authentication and authorization utilities
    /// </summary>
    public class GeonorgeAuthenticationService : IGeonorgeAuthenticationService
    {
        private readonly ILogger<GeonorgeAuthenticationService> _logger;

        public GeonorgeAuthenticationService(ILogger<GeonorgeAuthenticationService> logger)
        {
            _logger = logger;
        }

        public bool HasRole(ClaimsPrincipal user, string role)
        {
            if (user?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(role))
                return false;

            return user.HasClaim(ClaimTypes.Role, role) || 
                   user.HasClaim("role", role) ||
                   user.HasClaim("roles", role);
        }

        public bool BelongsToOrganization(ClaimsPrincipal user, string organizationName)
        {
            if (user?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(organizationName))
                return false;

            var userOrg = GetOrganizationName(user);
            return string.Equals(userOrg, organizationName, System.StringComparison.OrdinalIgnoreCase);
        }

        public string GetOrganizationName(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return user.FindFirst("OrganizationName")?.Value ??
                   user.FindFirst("organization")?.Value ??
                   user.FindFirst("company")?.Value;
        }

        public string GetOrganizationNumber(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return user.FindFirst("OrganizationOrgnr")?.Value ??
                   user.FindFirst("organization_number")?.Value ??
                   user.FindFirst("orgNr")?.Value;
        }

        public bool CanEditMetadata(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            return HasRole(user, "Administrator") ||
                   HasRole(user, "Editor") ||
                   user.HasClaim("permission", "metadata.edit") ||
                   user.HasClaim("scope", "metadata.write");
        }

        public bool CanAdminister(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            return HasRole(user, "Administrator") ||
                   HasRole(user, "Admin") ||
                   user.HasClaim("permission", "admin") ||
                   user.HasClaim("scope", "admin");
        }

        public string GetDisplayName(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return user.FindFirst("Name")?.Value ??
                   user.FindFirst(ClaimTypes.Name)?.Value ??
                   user.FindFirst("displayName")?.Value ??
                   user.FindFirst("name")?.Value ??
                   user.FindFirst("preferred_username")?.Value ??
                   GetEmail(user);
        }

        public string GetEmail(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return user.FindFirst("Email")?.Value ??
                   user.FindFirst(ClaimTypes.Email)?.Value ??
                   user.FindFirst("email")?.Value ??
                   user.FindFirst("mail")?.Value;
        }

        public bool IsGeonorgeAuthenticated(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            // Check if user has Geonorge system claim
            var systemClaim = user.FindFirst("system")?.Value;
            if (systemClaim == "geonorge")
                return true;

            // Check if user has Geonorge-related claims
            var hasGeonorgeClaims = user.Claims.Any(c => 
                c.Type.Contains("geonorge", System.StringComparison.OrdinalIgnoreCase) ||
                c.Value.Contains("geonorge", System.StringComparison.OrdinalIgnoreCase));

            return hasGeonorgeClaims;
        }
    }
}