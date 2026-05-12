using System.Security.Claims;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Services.Authentication
{
    /// <summary>
    /// Service interface for Geonorge authentication operations
    /// </summary>
    public interface IGeonorgeAuthenticationService
    {
        /// <summary>
        /// Check if user has specific role
        /// </summary>
        bool HasRole(ClaimsPrincipal user, string role);

        /// <summary>
        /// Check if user belongs to specific organization
        /// </summary>
        bool BelongsToOrganization(ClaimsPrincipal user, string organizationName);

        /// <summary>
        /// Get user's organization name
        /// </summary>
        string GetOrganizationName(ClaimsPrincipal user);

        /// <summary>
        /// Get user's organization number
        /// </summary>
        string GetOrganizationNumber(ClaimsPrincipal user);

        /// <summary>
        /// Check if user can edit metadata
        /// </summary>
        bool CanEditMetadata(ClaimsPrincipal user);

        /// <summary>
        /// Check if user can administer system
        /// </summary>
        bool CanAdminister(ClaimsPrincipal user);

        /// <summary>
        /// Get user's display name
        /// </summary>
        string GetDisplayName(ClaimsPrincipal user);

        /// <summary>
        /// Get user's email
        /// </summary>
        string GetEmail(ClaimsPrincipal user);

        /// <summary>
        /// Check if user is authenticated for Geonorge
        /// </summary>
        bool IsGeonorgeAuthenticated(ClaimsPrincipal user);
    }
}