using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Kartverket.Metadatakatalog.Services.Authentication
{
    /// <summary>
    /// Claims transformation service for Geonorge authentication
    /// Handles role mapping and organization information
    /// </summary>
    public class GeonorgeClaimsTransformation : IClaimsTransformation
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeonorgeClaimsTransformation> _logger;

        public GeonorgeClaimsTransformation(IConfiguration configuration, ILogger<GeonorgeClaimsTransformation> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.IsAuthenticated != true)
                return principal;

            var identity = (ClaimsIdentity)principal.Identity;
            
            try
            {
                // Transform and enrich claims
                await AddOrganizationClaims(identity);
                await AddRoleClaims(identity);
                await AddGeonorgeSpecificClaims(identity);
                await NormalizeClaims(identity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming claims for user {UserName}", principal.Identity.Name);
            }

            return principal;
        }

        /// <summary>
        /// Add organization-related claims
        /// </summary>
        private async Task AddOrganizationClaims(ClaimsIdentity identity)
        {
            var organizationClaim = identity.FindFirst("organization") ?? identity.FindFirst("OrganizationName");
            if (organizationClaim != null && !identity.HasClaim("OrganizationName", organizationClaim.Value))
            {
                identity.AddClaim(new Claim("OrganizationName", organizationClaim.Value));
            }

            var orgNumberClaim = identity.FindFirst("organization_number") ?? identity.FindFirst("OrganizationOrgnr");
            if (orgNumberClaim != null && !identity.HasClaim("OrganizationOrgnr", orgNumberClaim.Value))
            {
                identity.AddClaim(new Claim("OrganizationOrgnr", orgNumberClaim.Value));
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Add and normalize role claims
        /// </summary>
        private async Task AddRoleClaims(ClaimsIdentity identity)
        {
            // Get roles from various claim types
            var roleClaims = identity.FindAll(ClaimTypes.Role)
                .Union(identity.FindAll("role"))
                .Union(identity.FindAll("roles"))
                .Union(identity.FindAll("groups"))
                .ToList();

            // Normalize role names
            foreach (var roleClaim in roleClaims)
            {
                var normalizedRole = NormalizeRole(roleClaim.Value);
                if (!string.IsNullOrEmpty(normalizedRole) && 
                    !identity.HasClaim(ClaimTypes.Role, normalizedRole))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, normalizedRole));
                }
            }

            // Add default user role if no roles present
            if (!identity.HasClaim(ClaimTypes.Role, "User") && !roleClaims.Any())
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Add Geonorge-specific claims
        /// </summary>
        private async Task AddGeonorgeSpecificClaims(ClaimsIdentity identity)
        {
            // Add system identifier
            if (!identity.HasClaim("system", "geonorge"))
            {
                identity.AddClaim(new Claim("system", "geonorge"));
            }

            // Add application identifier
            if (!identity.HasClaim("application", "kartkatalog"))
            {
                identity.AddClaim(new Claim("application", "kartkatalog"));
            }

            // Add timestamp for audit purposes
            if (identity.FindFirst("auth_time") == null)
            {
                identity.AddClaim(new Claim("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Normalize standard claims
        /// </summary>
        private async Task NormalizeClaims(ClaimsIdentity identity)
        {
            // Normalize name claims
            var nameClaim = identity.FindFirst(ClaimTypes.Name) ?? 
                           identity.FindFirst("name") ?? 
                           identity.FindFirst("preferred_username") ?? 
                           identity.FindFirst("email");
            
            if (nameClaim != null && !identity.HasClaim("Name", nameClaim.Value))
            {
                identity.AddClaim(new Claim("Name", nameClaim.Value));
            }

            // Normalize email claims
            var emailClaim = identity.FindFirst(ClaimTypes.Email) ?? identity.FindFirst("email");
            if (emailClaim != null && !identity.HasClaim("Email", emailClaim.Value))
            {
                identity.AddClaim(new Claim("Email", emailClaim.Value));
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Normalize role names to standard values
        /// </summary>
        private string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return null;

            var normalizedRole = role.Trim().ToLowerInvariant();

            return normalizedRole switch
            {
                "administrator" or "admin" or "sysadmin" => "Administrator",
                "editor" or "redaktør" or "metadata-editor" => "Editor",
                "user" or "bruker" or "standard-user" => "User",
                "readonly" or "read-only" or "leser" => "ReadOnly",
                "guest" or "gjest" => "Guest",
                _ => char.ToUpperInvariant(role[0]) + role.Substring(1).ToLowerInvariant()
            };
        }
    }
}