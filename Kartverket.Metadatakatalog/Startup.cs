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
            // Add Response Compression (replaces custom CompressFilter)
            services.AddResponseCompression();

            // Add Memory Cache
            services.AddMemoryCache();

            services.AddGeonorgeAuthentication(Configuration);

            // Add services to the container.
            services.AddControllersWithViews();

            // Add API controllers
            services.AddControllers();

            // Add Razor Pages
            services.AddRazorPages(options =>
            {
                // Configure Razor Pages authorization
                options.Conventions.AuthorizeFolder("/Admin", "Admin");
                options.Conventions.AuthorizeFolder("/Editor", "Editor");
            });

            // ADD MISSING HTTPCLIENTFACTORY
            services.AddHttpClient();
            
            // Register custom HttpClientFactory adapter for Kartverket.Geonorge.Utilities.Organization.IHttpClientFactory
            services.AddSingleton<Kartverket.Geonorge.Utilities.Organization.IHttpClientFactory, HttpClientFactoryAdapter>();

            // Register configuration service for easy access
            services.AddScoped<Kartverket.Metadatakatalog.Helpers.IConfigurationService, Kartverket.Metadatakatalog.Helpers.ConfigurationService>();

            // Register all Kartverket services (migrated from DependencyConfig)
            services.AddKartverketServices(Configuration);

            // Register RegisterFetcher service
            services.AddScoped<RegisterFetcher>();

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Add global exception handling (replaces Application_Error)
            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios.
                app.UseHsts();
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
                // Add a specific route for Norwegian "nedlasting" to map to Download controller
                endpoints.MapControllerRoute(
                    name: "nedlasting",
                    pattern: "nedlasting/{action=Index}/{id?}",
                    defaults: new { controller = "Download" });
                    
                // Add a specific route for the root to Download
                endpoints.MapControllerRoute(
                    name: "root",
                    pattern: "",
                    defaults: new { controller = "Download", action = "Index" });
                    
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Download}/{action=Index}/{id?}");
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}



