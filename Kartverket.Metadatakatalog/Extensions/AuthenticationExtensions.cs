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

namespace Kartverket.Metadatakatalog.Extensions
{
    /// <summary>
    /// ASP.NET Core authentication configuration for Geonorge
    /// Migrated from GeonorgeAuthenticationModule (Autofac/.NET Framework)
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Add Geonorge authentication services (JWT + Cookies + OpenID Connect)
        /// </summary>
        public static IServiceCollection AddGeonorgeAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure authentication schemes
            var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            // Add Cookie Authentication (for web sessions)
            authBuilder.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.Name = "GeonorgeAuth";
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;

                // Configure cookie domain for production
                var cookieDomain = configuration["Culture:CookieDomain"];
                if (!string.IsNullOrEmpty(cookieDomain) && cookieDomain != "localhost")
                {
                    options.Cookie.Domain = cookieDomain;
                }
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
            var oidcSettings = configuration.GetSection("Authentication:OpenIdConnect");
            if (oidcSettings.Exists())
            {
                authBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = oidcSettings["Authority"];
                    options.ClientId = oidcSettings["ClientId"];
                    options.ClientSecret = oidcSettings["ClientSecret"];
                    options.ResponseType = "code";
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    // Add scopes for Norwegian government systems
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("geonorge");

                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            await EnrichClaims(context.Principal, configuration);
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

                // API policy - require specific API claims
                options.AddPolicy("ApiAccess", policy =>
                    policy.RequireClaim("scope", "api.geonorge"));

                // Organization policy - require organization membership
                options.AddPolicy("OrganizationMember", policy =>
                    policy.RequireClaim("organization"));

                // Metadata editor policy
                options.AddPolicy("MetadataEditor", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim("role", "Editor") ||
                        context.User.HasClaim("role", "Administrator") ||
                        context.User.HasClaim("permission", "metadata.edit")));
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

            await Task.CompletedTask;
        }
    }
}