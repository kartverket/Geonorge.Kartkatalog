using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Kartverket.Metadatakatalog.App_Start;

namespace Kartverket.Metadatakatalog.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;

        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IConfiguration configuration)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                
                if (authHeader.Scheme != "Basic")
                    return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Scheme"));

                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                
                if (credentials.Length != 2)
                    return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header Format"));

                var username = credentials[0];
                var password = credentials[1];

                // Validate credentials
                if (IsValidUser(username, password))
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, username),
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.Role, AuthConfig.DatasetProviderRole)
                    };

                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    Logger.LogInformation("Basic authentication successful for user: {Username}", username);
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }

                Logger.LogWarning("Basic authentication failed for user: {Username}", username);
                return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during basic authentication");
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }
        }

        private bool IsValidUser(string username, string password)
        {
            var validUsername = _configuration["Authentication:BasicAuth:Username"];
            var validPassword = _configuration["Authentication:BasicAuth:Password"];

            return !string.IsNullOrEmpty(validUsername) && 
                   !string.IsNullOrEmpty(validPassword) &&
                   username == validUsername && 
                   password == validPassword;
        }
    }
}