using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Kartverket.Metadatakatalog.Authentication;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Kartverket.Metadatakatalog.Extensions
{
    /// <summary>
    /// ASP.NET Core authentication configuration for Geonorge
    /// Migrated from GeonorgeAuthenticationModule (Autofac/.NET Framework)
    /// </summary>
    public static class AuthenticationExtensions
    {
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

            // Example: Add default claims if not present
            if (!principal.HasClaim(ClaimTypes.Role, "User"))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
            }

            // Add system identifier for Geonorge
            if (!principal.HasClaim("system", "geonorge"))
            {
                identity.AddClaim(new Claim("system", "geonorge"));
            }

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
                    var authorizationData = JsonSerializer.Deserialize<BaatAuthorizationResponse>(jsonContent, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });

                    if (authorizationData != null)
                    {
                        // Add BAAT user identifier if not already present
                        if (!string.IsNullOrEmpty(authorizationData.User) && 
                            !identity.HasClaim("baat_user", authorizationData.User))
                        {
                            identity.AddClaim(new Claim("baat_user", authorizationData.User));
                        }

                        // Add email if not already present and different from existing
                        if (!string.IsNullOrEmpty(authorizationData.Email) && 
                            !identity.HasClaim(ClaimTypes.Email, authorizationData.Email))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Email, authorizationData.Email));
                        }

                        // Add full name if not already present
                        if (!string.IsNullOrEmpty(authorizationData.Name) && 
                            !identity.HasClaim(ClaimTypes.Name, authorizationData.Name))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Name, authorizationData.Name));
                        }

                        // Add authorization period claims
                        if (authorizationData.AuthorizedFrom > 0)
                        {
                            identity.AddClaim(new Claim("baat_authorized_from", authorizationData.AuthorizedFrom.ToString()));
                        }

                        if (authorizationData.AuthorizedUntil > 0)
                        {
                            identity.AddClaim(new Claim("baat_authorized_until", authorizationData.AuthorizedUntil.ToString()));
                        }

                        // Check if user authorization is currently valid
                        var currentDate = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                        var isAuthorized = authorizationData.AuthorizedFrom <= currentDate && 
                                         authorizationData.AuthorizedUntil >= currentDate;
                        
                        identity.AddClaim(new Claim("baat_authorized", isAuthorized.ToString().ToLower()));

                        // Add organization information if available
                        if (authorizationData.Organization != null)
                        {
                            if (!string.IsNullOrEmpty(authorizationData.Organization.Name))
                            {
                                identity.AddClaim(new Claim("organization", authorizationData.Organization.Name));
                            }

                            if (!string.IsNullOrEmpty(authorizationData.Organization.Orgnr))
                            {
                                identity.AddClaim(new Claim("organization_number", authorizationData.Organization.Orgnr));
                            }

                            if (!string.IsNullOrEmpty(authorizationData.Organization.ContactName))
                            {
                                identity.AddClaim(new Claim("organization_contact_name", authorizationData.Organization.ContactName));
                            }

                            if (!string.IsNullOrEmpty(authorizationData.Organization.ContactEmail))
                            {
                                identity.AddClaim(new Claim("organization_contact_email", authorizationData.Organization.ContactEmail));
                            }

                            if (!string.IsNullOrEmpty(authorizationData.Organization.ContactPhone))
                            {
                                identity.AddClaim(new Claim("organization_contact_phone", authorizationData.Organization.ContactPhone));
                            }
                        }

                        // Add role based on authorization status and organization
                        if (isAuthorized)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, "AuthorizedUser"));
                            
                            // Add organization-specific roles
                            if (authorizationData.Organization?.Name == "Kartverket")
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, "KartverketUser"));
                            }
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

        /// <summary>
        /// Response model for BAAT Authorization API
        /// </summary>
        private class BaatAuthorizationResponse
        {
            public string User { get; set; }
            public string Email { get; set; }
            public int AuthorizedUntil { get; set; }
            public int AuthorizedFrom { get; set; }
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
            public string ContactName { get; set; }
            public string ContactEmail { get; set; }
            public string ContactPhone { get; set; }
        }
    }
}