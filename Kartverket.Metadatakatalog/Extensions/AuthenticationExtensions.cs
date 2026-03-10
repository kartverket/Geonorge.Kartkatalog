using Geonorge.AuthLib.Common;
using Kartverket.Metadatakatalog.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Extensions
{
    /// <summary>
    /// ASP.NET Core authentication configuration for Geonorge
    /// Migrated from GeonorgeAuthenticationModule (Autofac/.NET Framework)
    /// </summary>
    public static class AuthenticationExtensions
    {
        private const string GeonorgeRoleNamePrefix = "nd.";
        public const string ClaimIdentifierRole = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        /// <summary>
        /// Add Geonorge authentication services (JWT + Cookies + OpenID Connect + Basic Auth)
        /// </summary>
        public static IServiceCollection AddGeonorgeAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Check if OpenIdConnect will be registered
            var oidcSettings = configuration.GetSection("Authentication:OpenIdConnect");
            bool oidcAvailable = oidcSettings.Exists() && !oidcSettings["Authority"]?.Contains("your-tenant-id") == true;

            // Configure authentication schemes
            var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // Use OpenIdConnect if available, otherwise fall back to Cookies
                options.DefaultChallengeScheme = oidcAvailable ? 
                    OpenIdConnectDefaults.AuthenticationScheme : 
                    CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            // Add Basic Authentication for API endpoints
            authBuilder.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", options => { });

            // Add Cookie Authentication (for web sessions)
            authBuilder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Search/SignIn";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.Name = "GeonorgeAuth";
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;

                // Remove domain configuration temporarily for testing
                // This ensures cookies work properly in development
            });

            // Add JWT Bearer Authentication (for API endpoints)
            authBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var jwtSettings = configuration.GetSection("Authentication:Jwt");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        await EnrichClaims(context.Principal, configuration);
                    }
                };
            });

            // Add OpenID Connect (for external authentication - Azure AD, etc.)
            // Only add if properly configured (not using placeholder values)
            if (oidcAvailable)
            {
                authBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = oidcSettings["Authority"];
                    options.ClientId = oidcSettings["ClientId"];
                    options.ClientSecret = oidcSettings["ClientSecret"];
                    options.MetadataAddress = oidcSettings["MetadataAddress"];
                    
                    // Use authorization code flow (modern, secure)
                    options.ResponseType = "code";
                    options.ResponseMode = "query";
                    
                    // Enable PKCE for security
                    options.UsePkce = true;
                    
                    // Configure callback paths to match routing
                    options.CallbackPath = "/signin-oidc";
                    options.SignedOutCallbackPath = "/signout-callback-oidc";
                    
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    // Add required scopes for OpenID Connect
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");

                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = context =>
                        {
                            System.Diagnostics.Debug.WriteLine($"OpenIdConnect: Redirecting to identity provider: {context.ProtocolMessage.IssuerAddress}");
                            return Task.CompletedTask;
                        },
                        OnAuthorizationCodeReceived = context =>
                        {
                            System.Diagnostics.Debug.WriteLine("OpenIdConnect: Authorization code received");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            System.Diagnostics.Debug.WriteLine("OpenIdConnect: Token validated successfully");
                            System.Diagnostics.Debug.WriteLine($"OpenIdConnect: User identity name: {context.Principal.Identity.Name}");
                            System.Diagnostics.Debug.WriteLine($"OpenIdConnect: Claims count: {context.Principal.Claims.Count()}");
                            System.Diagnostics.Debug.WriteLine($"OpenIdConnect: Authentication type: {context.Principal.Identity.AuthenticationType}");
                            System.Diagnostics.Debug.WriteLine($"OpenIdConnect: DefaultSignInScheme: {context.Scheme.Name}");
                            
                            // Log first few claims
                            foreach (var claim in context.Principal.Claims.Take(5))
                            {
                                System.Diagnostics.Debug.WriteLine($"OpenIdConnect: Claim {claim.Type}: {claim.Value}");
                            }
                            
                            await EnrichClaims(context.Principal, configuration);
                            System.Diagnostics.Debug.WriteLine("OpenIdConnect: About to complete sign-in");
                        },
                        OnAuthenticationFailed = context =>
                        {
                            System.Diagnostics.Debug.WriteLine($"OpenIdConnect: Authentication failed: {context.Exception?.Message}");
                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context =>
                        {
                            System.Diagnostics.Debug.WriteLine($"OpenIdConnect: Remote authentication failure: {context.Failure?.Message}");
                            return Task.CompletedTask;
                        }
                    };
                });
            }

            return services;
        }

        /// <summary>
        /// Add authorization policies for Geonorge
        /// </summary>
        public static IServiceCollection AddGeonorgeAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Default policy - require authenticated users
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                // Admin policy - require admin role
                options.AddPolicy("Admin", policy =>
                    policy.RequireRole("Administrator", "Admin"));

                // Editor policy - require editor role
                options.AddPolicy("Editor", policy =>
                    policy.RequireRole("Editor", "Administrator", "Admin"));

                // API policy - require specific API claims or basic auth
                options.AddPolicy("ApiAccess", policy =>
                {
                    policy.AuthenticationSchemes.Add("BasicAuthentication");
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });

                // API Dataset Provider policy - for metadata API endpoints
                options.AddPolicy("ApiDatasetProvider", policy =>
                {
                    policy.AuthenticationSchemes.Add("BasicAuthentication");
                    policy.RequireRole("DatasetProvider", "Administrator", "Admin");
                });

                // Organization policy - require organization membership
                options.AddPolicy("OrganizationMember", policy =>
                    policy.RequireClaim("organization"));

                // Metadata editor policy
                options.AddPolicy("MetadataEditor", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim("role", "Editor") ||
                        context.User.HasClaim("role", "Administrator") ||
                        context.User.HasClaim("permission", "metadata.edit") ||
                        (context.User.HasClaim("baat_authorized", "true") && 
                         context.User.HasClaim(ClaimTypes.Role, "AuthorizedUser"))));

                // BAAT authorized user policy
                options.AddPolicy("BaatAuthorized", policy =>
                    policy.RequireClaim("baat_authorized", "true"));

                // Kartverket employee policy
                options.AddPolicy("KartverketEmployee", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim("organization", "Kartverket") &&
                        context.User.HasClaim("baat_authorized", "true")));
            });

            return services;
        }

        /// <summary>
        /// Enrich user claims with Geonorge-specific information
        /// </summary>
        private static async Task EnrichClaims(ClaimsPrincipal principal, IConfiguration configuration)
        {
            if (principal?.Identity?.IsAuthenticated != true)
                return;

            var identity = (ClaimsIdentity)principal.Identity;

            // Add custom claims transformation logic here
            // This would typically:
            // 1. Look up user roles from database
            // 2. Add organization information
            // 3. Add permissions based on user context
            // 4. Add Norwegian-specific claims

            // Get additional claims from BAAT Authorization API
            await EnrichClaimsFromBaatApi(identity, configuration);
        }

        /// <summary>
        /// Enrich claims by calling BAAT Authorization API with basic authentication
        /// </summary>
        private static async Task EnrichClaimsFromBaatApi(ClaimsIdentity identity, IConfiguration configuration)
        {
            try
            {
                // Get BAAT API configuration
                var baatApiUrl = configuration["Authentication:OpenIdConnect:BaatAuthzApiUrl"];
                var baatCredentials = configuration["Authentication:OpenIdConnect:BaatAuthzApiCredentials"];

                if (string.IsNullOrEmpty(baatApiUrl) || string.IsNullOrEmpty(baatCredentials))
                {
                    return; // Configuration not available, skip BAAT enrichment
                }

                // Get user identifier from existing claims
                var userIdentifier = identity.FindFirst("preferred_username")?.Value;
                if (string.IsNullOrEmpty(userIdentifier))
                {
                    return; // No user identifier found
                }

                // Create HTTP client and configure basic authentication
                using var httpClient = new HttpClient();
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(baatCredentials));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                httpClient.Timeout = TimeSpan.FromSeconds(10); // Set reasonable timeout

                // Construct API URL for user authorization (adjust URL structure as needed)
                var requestUrl = $"{baatApiUrl.TrimEnd('/')}/authzinfo/{userIdentifier}";

                // Make the API call
                var response = await httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var authorizationData = System.Text.Json.JsonSerializer.Deserialize<BaatAuthorizationResponse>(jsonContent, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });

                    if (authorizationData != null)
                    {
                       
                        if (!string.IsNullOrEmpty(authorizationData.Name) &&
                            !identity.HasClaim("Name", authorizationData.Name))
                        {
                            identity.AddClaim(new Claim("Name", authorizationData.Name));
                        }

                        if (!identity.HasClaim("AuthorizedFrom", authorizationData.Authorized_From.ToString()))
                        {
                            identity.AddClaim(new Claim("AuthorizedFrom", authorizationData.Authorized_From.ToString()));
                        }

                        if (!identity.HasClaim("AuthorizedUntil", authorizationData.Authorized_Until.ToString()))
                        {
                            identity.AddClaim(new Claim("AuthorizedUntil", authorizationData.Authorized_Until.ToString()));
                        }

                        if (!string.IsNullOrEmpty(authorizationData?.Organization?.Name) &&
                            !identity.HasClaim("OrganizationName", authorizationData.Organization.Name))
                        {
                            identity.AddClaim(new Claim("OrganizationName", authorizationData.Organization.Name));
                        }

                        if (!string.IsNullOrEmpty(authorizationData?.Organization?.Orgnr) &&
                            !identity.HasClaim("OrganizationOrgnr", authorizationData.Organization.Orgnr))
                        {
                            identity.AddClaim(new Claim("OrganizationOrgnr", authorizationData.Organization.Orgnr));
                        }

                        if (!string.IsNullOrEmpty(authorizationData?.Organization?.Contact_Name) &&
                            !identity.HasClaim("OrganizationContactName", authorizationData.Organization.Contact_Name))
                        {
                            identity.AddClaim(new Claim("OrganizationContactName", authorizationData.Organization.Contact_Name));
                        }

                        if (!string.IsNullOrEmpty(authorizationData?.Organization?.Contact_Email) &&
                            !identity.HasClaim("OrganizationContactEmail", authorizationData.Organization.Contact_Email))
                        {
                            identity.AddClaim(new Claim("OrganizationContactEmail", authorizationData.Organization.Contact_Email));
                        }

                        if (!string.IsNullOrEmpty(authorizationData?.Organization?.Contact_Phone) &&
                            !identity.HasClaim("OrganizationContactPhone", authorizationData.Organization.Contact_Phone))
                        {
                            identity.AddClaim(new Claim("OrganizationContactPhone", authorizationData.Organization.Contact_Phone));
                        }


                        var url = $"{baatApiUrl.TrimEnd('/')}/authzlist/{userIdentifier}";

                        var res = await httpClient.GetAsync(url);
                            
                        var json = await res.Content.ReadAsStringAsync();


                        if (json.Contains("\"services\": false"))
                            json = json.Replace("\"services\": false", "\"services\": \"\"");

                        var responseRoles = JsonConvert.DeserializeObject<BaatAuthzUserRolesResponse>(json);

                        if (responseRoles.Services != null)
                        {
                            responseRoles.Services
                                .Where(role => role.StartsWith(GeonorgeRoleNamePrefix))
                                .ToList()
                                .ForEach(role => identity.AddClaim(new Claim(ClaimIdentifierRole, role)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail authentication
                // In a production environment, use proper logging
                System.Diagnostics.Debug.WriteLine($"Failed to enrich claims from BAAT API: {ex.Message}");
            }
        }

        public class BaatAuthzUserRolesResponse
        {
            public static readonly BaatAuthzUserRolesResponse Empty = new BaatAuthzUserRolesResponse();

            [JsonProperty("services")]
            public List<string> Services = new List<string>();
        }

/// <summary>
/// Response model for BAAT Authorization API
/// </summary>
private class BaatAuthorizationResponse
        {
            public string User { get; set; }
            public string Email { get; set; }
            public int Authorized_Until { get; set; }
            public int Authorized_From { get; set; }
            public BaatOrganization Organization { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// Organization model for BAAT Authorization API
        /// </summary>
        private class BaatOrganization
        {
            public string Name { get; set; }
            public string Orgnr { get; set; }
            public string Contact_Name { get; set; }
            public string Contact_Email { get; set; }
            public string Contact_Phone { get; set; }
        }
    }
}