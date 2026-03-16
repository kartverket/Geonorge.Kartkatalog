using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Extensions;
using Kartverket.Metadatakatalog.Middleware;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.Article;
using Kartverket.Metadatakatalog.Service.Search;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using Kartverket.Metadatakatalog.Adapters;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using System.Security.Claims;
using System.Net.Http;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Kartverket.Metadatakatalog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                // Add Response Compression (replaces custom CompressFilter)
                services.AddResponseCompression();

                // Add Memory Cache
                services.AddMemoryCache();

                // Add Razor Pages first (primary framework for this project)
                services.AddRazorPages();

                // Add API controllers for REST endpoints
                services.AddControllers();

                // Enable Geonorge Authentication (includes OpenIdConnect)
                services.AddGeonorgeAuthentication(Configuration);

                // Add simplified authorization policies
                services.AddAuthorization();

                // ADD MISSING HTTPCLIENTFACTORY
                services.AddHttpClient();

                // Configure HttpClient with certificate bypass for development
                if (Configuration["AppSettings:EnvironmentName"] == "dev")
                {
                    services.ConfigureHttpClientDefaults(builder =>
                    {
                        builder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                        });
                    });
                }
                
                // Register custom HttpClientFactory adapter for Kartverket.Geonorge.Utilities.Organization.IHttpClientFactory
                services.AddSingleton<Kartverket.Geonorge.Utilities.Organization.IHttpClientFactory, HttpClientFactoryAdapter>();

                // Register configuration service for easy access
                services.AddScoped<Kartverket.Metadatakatalog.Helpers.IConfigurationService, Kartverket.Metadatakatalog.Helpers.ConfigurationService>();

                // Register all Kartverket services (migrated from DependencyConfig)
                try 
                {
                    services.AddKartverketServices(Configuration);
                    System.Diagnostics.Debug.WriteLine("✅ AddKartverketServices completed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ AddKartverketServices failed: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    // Continue without these services for now to isolate the issue
                }

                // Register RegisterFetcher service
                try 
                {
                    services.AddScoped<RegisterFetcher>();
                    System.Diagnostics.Debug.WriteLine("✅ RegisterFetcher registered");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ RegisterFetcher registration failed: {ex.Message}");
                }

                // Configure Antiforgery (replaces AntiForgeryConfig.UniqueClaimTypeIdentifier)
                services.Configure<AntiforgeryOptions>(options =>
                {
                    // You can set the HeaderName to specify the header used for the antiforgery token.
                    options.HeaderName = "X-CSRF-TOKEN";
                    // Other options can be configured as needed.
                });

                // Configure localization
                services.Configure<RequestLocalizationOptions>(options =>
                {
                    var supportedCultures = new[]
                    {
                new CultureInfo("nb-NO"),
                new CultureInfo("en-US")
            };

                    options.DefaultRequestCulture = new RequestCulture("nb-NO");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;
                });

                // Add HttpClient for PlaceResolver
                services.AddHttpClient<Kartverket.Metadatakatalog.Service.PlaceResolver>();

                // Add CORS services
                services.AddCors();

                // Add Swagger/OpenAPI services with simplified configuration
                services.AddEndpointsApiExplorer();
                
                services.AddSwaggerGen(c =>
                {
                    // Simple OpenAPI document configuration
                    c.SwaggerDoc("v1", new()
                    {
                        Title = "Kartverket Metadata Catalog API",
                        Version = "v1",
                        Description = "API for managing metadata in the Kartverket metadata catalog"
                    });

                    // Only include controllers with [ApiController] attribute to avoid conflicts
                    c.DocInclusionPredicate((name, api) => 
                    {
                        return api.ActionDescriptor.EndpointMetadata
                            .Any(x => x is Microsoft.AspNetCore.Mvc.ApiControllerAttribute);
                    });

                    // Simple conflict resolution
                    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.FirstOrDefault());
                    
                    System.Diagnostics.Debug.WriteLine("✅ Swagger generation configured");
                });

                System.Diagnostics.Debug.WriteLine("✅ ConfigureServices completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ConfigureServices failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to see the actual error
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 Starting Configure method...");

                // Add global exception handling (replaces Application_Error)
                app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    
                    // Enable Swagger in development with enhanced error handling
                    try
                    {
                        app.UseSwagger(c => 
                        {
                            c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                            {
                                System.Diagnostics.Debug.WriteLine($"✅ Swagger document generated successfully for {httpReq.Path}");
                            });
                        });
                        
                        app.UseSwaggerUI(c =>
                        {
                            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kartverket Metadata Catalog API v1");
                            c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
                            c.DisplayRequestDuration();
                            c.EnableDeepLinking();
                        });
                        System.Diagnostics.Debug.WriteLine("✅ Swagger configured successfully");
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't crash the application
                        System.Diagnostics.Debug.WriteLine($"❌ Failed to configure Swagger: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Swagger stack trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios.
                    app.UseHsts();
                    
                    // Enable Swagger in production with error handling
                    try
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI(c =>
                        {
                            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kartverket Metadata Catalog API v1");
                            c.RoutePrefix = "swagger";
                        });
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't crash the application
                        System.Diagnostics.Debug.WriteLine($"❌ Failed to configure Swagger in production: {ex}");
                    }
                }

                app.UseHttpsRedirection();
                
                // Add Response Compression (replaces custom CompressFilter)
                app.UseResponseCompression();
                
                app.UseStaticFiles();

                app.UseRouting();

                // Add CORS middleware (must be between UseRouting and UseEndpoints)
                app.UseCors();

                // Add localization middleware
                app.UseRequestLocalization();

                // Add authentication and authorization BEFORE culture middleware for auth paths
                app.UseAuthentication();
                app.UseAuthorization();

                // Add whitespace compression middleware (after authentication to prevent interference)
                app.UseMiddleware<Kartverket.Metadatakatalog.Middleware.WhitespaceCompressionMiddleware>();

                // Add custom middleware for culture handling (after authentication to prevent interference)
                app.UseMiddleware<CultureMiddleware>();

                // Add custom middleware for return URL validation (after authentication)
                app.UseMiddleware<ReturnUrlValidationMiddleware>();

                app.UseEndpoints(endpoints =>
                {
                    // Map Razor Pages first (primary routing for this project)
                    endpoints.MapRazorPages();
                    
                    // Add root route to redirect to Download controller
                    endpoints.MapControllerRoute(
                        name: "root",
                        pattern: "",
                        defaults: new { controller = "Download", action = "Index" });
                    
                    // Add specific route for Norwegian "nedlasting" to map to Download controller
                    endpoints.MapControllerRoute(
                        name: "nedlasting",
                        pattern: "nedlasting/{action=Index}/{id?}",
                        defaults: new { controller = "Download" });
                    
                    // Add general controller route for MVC controllers like SearchController
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Download}/{action=Index}/{id?}");
                    
                    // Map API controllers for REST endpoints
                    endpoints.MapControllers();
                });

                System.Diagnostics.Debug.WriteLine("✅ Configure method completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Configure method failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Configure stack trace: {ex.StackTrace}");
                throw; // Re-throw to see the actual error
            }
        }
    }
}



