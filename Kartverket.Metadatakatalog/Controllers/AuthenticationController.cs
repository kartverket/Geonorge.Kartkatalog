using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handle OpenID Connect callback
        /// This action is called by the identity provider after successful authentication
        /// </summary>
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Callback()
        {
            _logger.LogInformation("Authentication callback received");
            
            // The authentication middleware will handle the callback automatically
            // This action just provides a route target for explicit routing
            
            // Check if user is authenticated after callback
            if (User.Identity.IsAuthenticated)
            {
                _logger.LogInformation("User authenticated successfully: {Name}", User.Identity.Name);
                
                // Redirect to the return URL or default location
                var returnUrl = Request.Query["ReturnUrl"].ToString();
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    _logger.LogInformation("Redirecting to return URL: {ReturnUrl}", returnUrl);
                    return Redirect(returnUrl);
                }
                
                _logger.LogInformation("Redirecting to default location: /nedlasting");
                return Redirect("/nedlasting");
            }
            else
            {
                _logger.LogWarning("Authentication callback received but user not authenticated");
                return Redirect("/");
            }
        }
    }
}