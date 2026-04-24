using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Adapters;
using Kartverket.Metadatakatalog.Extensions;
using Kartverket.Metadatakatalog.Middleware;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.Article;
using Kartverket.Metadatakatalog.Service.Search;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;

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
                services.AddControllers()
                    .AddJsonOptions(opts =>
                    {
                        // Preserve exact property names (no camel-casing)
                        opts.JsonSerializerOptions.PropertyNamingPolicy = null;
                        // Skip null properties during serialization
                        opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                        // Optional: also skip default values (empty strings, 0, false, etc.)
                        // opts.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault;
                        // optional: control other options
                        // opts.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
                    });

                // Enable Geonorge Authentication (includes OpenIdConnect)
                services.AddGeonorgeAuthentication(Configuration);

                // Add simplified authorization policies
                services.AddAuthorization();

                // ADD MISSING HTTPCLIENTFACTORY
                services.AddHttpClient();

                // 🔧 MAJOR PERFORMANCE FIX: Configure HttpClient for .NET 10 to match .NET Framework 4.8 performance
                services.ConfigureHttpClientDefaults(builder =>
                {
                    builder.ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        var handler = new HttpClientHandler();
                        
                        // 🔧 DNS PERFORMANCE: Reduce DNS lookup time (was automatic in .NET Framework 4.8)
                        // Force connection reuse and reduce DNS refresh time
                        
                        // 🔧 CONNECTION PERFORMANCE: Configure connection limits (ServicePointManager equivalent)
                        handler.MaxConnectionsPerServer = 100; // Default was 2, increase for Solr performance
                        
                        // 🔧 SSL/TLS PERFORMANCE: Use system defaults which are more optimized
                        handler.SslProtocols = System.Security.Authentication.SslProtocols.None; // Let system choose
                        
                        // 🔧 COMPRESSION: Enable automatic decompression for better performance
                        handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
                        
                        if (Configuration["AppSettings:EnvironmentName"] == "dev")
                        {
                            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                        }
                        
                        return handler;
                    });
                    
                    // 🔧 TIMEOUT PERFORMANCE: Configure timeouts to match .NET Framework behavior
                    builder.ConfigureHttpClient(client =>
                    {
                        client.Timeout = TimeSpan.FromSeconds(30); // .NET Framework default was 100 seconds, but 30 is better for Solr
                        
                        // 🔧 KEEP-ALIVE: Ensure connections stay alive (critical for Solr performance)
                        client.DefaultRequestHeaders.Connection.Add("keep-alive");
                        
                        // 🔧 USER-AGENT: Set a consistent user agent
                        client.DefaultRequestHeaders.Add("User-Agent", "Kartverket-Metadatakatalog/1.0");
                    });
                });
                
                // 🔧 SOCKET PERFORMANCE: Configure socket settings at the application level
                // This will be called in the Configure method where 'this' context is available
                
                // 🔧 CRITICAL PERFORMANCE FIX: Add named HttpClient for GeoNorge API
                // This addresses the potential bottleneck in _geoNorge.GetRecordByUuid() calls
                services.AddHttpClient("GeoNorge", client =>
                {
                    var geoNetworkUrl = Configuration["GeoNetworkUrl"] ?? "https://kartkatalog.geonorge.no/geonetwork";
                    client.BaseAddress = new Uri(geoNetworkUrl);
                    client.Timeout = TimeSpan.FromSeconds(15); // Shorter timeout for metadata API calls
                    client.DefaultRequestHeaders.Add("Accept", "application/xml, text/xml");
                    client.DefaultRequestHeaders.Connection.Add("keep-alive");
                    client.DefaultRequestHeaders.Add("User-Agent", "Kartverket-Metadatakatalog-GeoNorge/1.0");
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler()
                    {
                        MaxConnectionsPerServer = 20, // Dedicated connections for GeoNorge
                        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
                        UseCookies = false, // Disable cookies for API calls
                        UseDefaultCredentials = false
                    };
                });

                // 🔧 CRITICAL PERFORMANCE FIX: Add named HttpClient for Solr
                services.AddHttpClient("Solr", client =>
                {
                    var solrUrl = Configuration["SolrServerUrl"] ?? "http://localhost:8983";
                    client.BaseAddress = new Uri(solrUrl);
                    client.Timeout = TimeSpan.FromSeconds(10); // Short timeout for Solr operations
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Connection.Add("keep-alive");
                    client.DefaultRequestHeaders.Add("User-Agent", "Kartverket-Metadatakatalog-Solr/1.0");
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler()
                    {
                        MaxConnectionsPerServer = 50, // Many connections for multiple Solr cores
                        AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
                        UseCookies = false,
                        UseDefaultCredentials = false
                    };
                });
                
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
                    // Singleton: the constructor performs ~25 synchronous HTTP calls against
                    // the Kartverket register service to populate code lists, and caches them
                    // in per-instance dictionaries. Scoping this service threw the cache away
                    // on every request, which was the dominant per-request cost (~1 s).
                    services.AddSingleton<RegisterFetcher>();
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
                services.AddCors(options => options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

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
                    
                    // Include XML comments for better documentation
                    var xmlFile = System.IO.Path.Combine(AppContext.BaseDirectory, "App_Data", "XmlDocument.xml");
                    if (System.IO.File.Exists(xmlFile))
                    {
                        c.IncludeXmlComments(xmlFile);
                    }
                    
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

                // 🔧 CRITICAL PERFORMANCE FIX: Configure system-level network settings early
                ConfigureSocketPerformance();

                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
                    ForwardLimit = int.Parse(Configuration["HeaderForwardLimit"]!), // Default is 1
                    KnownProxies = { IPAddress.Loopback, IPAddress.Parse("127.0.0.6") },
                    KnownIPNetworks = {
                        new System.Net.IPNetwork(IPAddress.Parse("10.0.0.0"), 8),
                    }
                });

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

                //app.UseHttpsRedirection();

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

        /// <summary>
        /// Configure socket-level performance settings to address .NET 10 vs .NET Framework 4.8 performance differences
        /// </summary>
        private void ConfigureSocketPerformance()
        {
            // 🔧 CRITICAL PERFORMANCE FIX: Configure system-level network settings
            // These settings address the most common performance regressions when migrating from .NET Framework 4.8
            
            // 🔧 DNS PERFORMANCE: Reduce DNS lookup delays
            ServicePointManager.DnsRefreshTimeout = 60000; // 1 minute (was default in .NET Framework)
            ServicePointManager.EnableDnsRoundRobin = false; // Disable for consistent performance
            
            // 🔧 CONNECTION PERFORMANCE: Increase connection limits (critical for Solr)
            ServicePointManager.DefaultConnectionLimit = 100; // Default was 2, now 100 for multiple Solr cores
            ServicePointManager.MaxServicePointIdleTime = 30000; // 30 seconds keep-alive
            
            // 🔧 HTTP PERFORMANCE: Configure HTTP/TCP behavior
            ServicePointManager.UseNagleAlgorithm = false; // Disable Nagle for low-latency scenarios like Solr
            ServicePointManager.Expect100Continue = false; // Disable 100-Continue for better performance
            
            // 🔧 TLS/SSL PERFORMANCE: Use optimal security protocols
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            
            // 🔧 SOCKET PERFORMANCE: Configure TCP socket behavior
            // Note: Some of these are global settings that affect all HTTP traffic in the application
            
            System.Diagnostics.Debug.WriteLine("🔧 Applied .NET 10 performance optimizations for HTTP/Solr connections");
            System.Diagnostics.Debug.WriteLine($"🔧 DNS Refresh Timeout: {ServicePointManager.DnsRefreshTimeout}ms");
            System.Diagnostics.Debug.WriteLine($"🔧 Default Connection Limit: {ServicePointManager.DefaultConnectionLimit}");
            System.Diagnostics.Debug.WriteLine($"🔧 Max Idle Time: {ServicePointManager.MaxServicePointIdleTime}ms");
        }
    }
}



