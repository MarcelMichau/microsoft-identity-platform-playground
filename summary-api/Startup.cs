using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace SummaryApi;

internal static class DelegatedPermissions
{
    public const string ReadSummary = "Summary.Read";

    public static string[] All => typeof(DelegatedPermissions)
        .GetFields()
        .Where(f => f.Name != nameof(All))
        .Select(f => f.GetValue(null) as string)
        .ToArray();
}

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));

        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.OperationFilter<AuthorizeOperationFilter>();

            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SummaryApi", Version = "v1" });

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
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "summary_api v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}