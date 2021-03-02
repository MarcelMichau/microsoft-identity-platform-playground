using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;

namespace WebApiAuthPlayground
{
    internal static class AppRoles
    {
        public const string ApiFullAccess = "Api.ReadWrite.All";
        public const string ReadWeather = "Weather.Read";
        public const string ReadLotsOfWeather = "Weather.Lots.Read";

        public static string[] All => typeof(AppRoles)
            .GetFields()
            .Where(f => f.Name != nameof(All))
            .Select(f => f.GetValue(null) as string)
            .ToArray();
    }

    internal static class DelegatedPermissions
    {
        public const string ReadBasicWeather = "Weather.Read.Basic";
        public const string ReadLotsOfWeather = "Weather.Read.Lots";
        public const string ReadSpecialWeather = "Weather.Read.Special";

        public static string[] All => typeof(DelegatedPermissions)
            .GetFields()
            .Where(f => f.Name != nameof(All))
            .Select(f => f.GetValue(null) as string)
            .ToArray();
    }

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
            services.AddMicrosoftIdentityWebApiAuthentication(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddDownstreamWebApi("SummaryApi", Configuration.GetSection("SummaryApi"))
                        .AddInMemoryTokenCaches();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAnyRole",
                    policy => policy.RequireRole(AppRoles.All));
            });

            services.AddHttpClient();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<AuthorizeOperationFilter>();

                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApiAuthPlayground", Version = "v1" });

                var azureAdTenantId = Configuration.GetValue<string>("AzureAd:TenantId");

                var configurationUrl =
                    new Uri(
                        $"https://login.microsoftonline.com/{azureAdTenantId}/v2.0/.well-known/openid-configuration");
                var authorizationUrl =
                    new Uri(
                        $"https://login.microsoftonline.com/{azureAdTenantId}/oauth2/v2.0/authorize");
                var tokenUrl =
                    new Uri(
                        $"https://login.microsoftonline.com/{azureAdTenantId}/oauth2/v2.0/token");

                c.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
                {
                    Description = "Azure AD Authentication",
                    OpenIdConnectUrl = configurationUrl,
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = authorizationUrl,
                            TokenUrl = tokenUrl,
                            Scopes = DelegatedPermissions.All.ToDictionary(p => $"api://{Configuration.GetValue<string>("AzureAd:ClientId")}/{p}")
                        }
                    }
                });
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000")
                            .AllowAnyHeader();
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApiAuthPlayground v1");
                    c.OAuthClientId(Configuration.GetValue<string>("AzureAd:ClientId"));
                    c.OAuthUsePkce();
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
