using Microsoft.Identity.Web;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Logging;
using CatCam.Common.Services.Secrets;
using CatCam.Common.Services.Storage;
using CatCam.Common.Models;
using CatCam.Common.Services.EntityProvider;
using Microsoft.AspNetCore.Authorization;
using CatCam.Common.Services.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using CatCam.Common.Services.IOTHub;

namespace CatCam.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureCommonServices(IServiceCollection services)
        {
            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Adds Microsoft Identity platform (AAD v2.0) support to protect this Api
            services.AddMicrosoftIdentityWebApiAuthentication(Configuration);
            services.AddAuthorization(options =>
            {
                // For initial version, all controllers will use the fallback policy to require an auth'ed user
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

                // options.AddPolicy("BadgeEntry", policy =>
                //         policy.RequireAssertion(context => context.User.HasClaim(c =>
                //             (c.Type == "BadgeId" || c.Type == "TemporaryBadgeId")
                //             && c.Issuer == "https://microsoftsecurity")));
            });

            // or if you want to explicitly set the values
            // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //     .AddMicrosoftIdentityWebApi(
            //     options =>
            //     {
            //         options.Audience = "https://login.microsoftonline.com/mattcosturosgmail.onmicrosoft.com";
            //         options.TokenValidationParameters.ValidAudiences = new string[] { "6d640e1e-5d45-43fb-abac-133b4943280c", "api://catcam-api/api.access" };
            //     },
            //     options =>
            //     {
            //         options.TenantId = "5cf954a5-057f-4af4-94da-71b8bfe8e39b";
            //         options.Instance = "https://login.microsoftonline.com";
            //         options.Domain = "mattcosturosgmail.onmicrosoft.com";
            //         options.ClientId = "6d640e1e-5d45-43fb-abac-133b4943280c";
            //     });

            //// Comment the lines of code above and uncomment the following section if you would like to limit calls to this API to just a set of client apps
            //// The following is an example of extended token validation
            /**
            * The example below can be used do extended token validation and check for additional claims, such as:
            * -check if the caller's tenant is in the allowed tenants list via the 'tid' claim (for multi-tenant applications)
            *  -check if the caller's account is homed or guest via the 'acct' optional claim
                * -check if the caller belongs to right roles or groups via the 'roles' or 'groups' claim, respectively
                *
                * For more information, visit: https://docs.microsoft.com/azure/active-directory/develop/access-tokens#validate-the-user-has-permission-to-access-this-data
            **/

            /**
             * Also look up Policy-based authorization in ASP.NET Core(https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies)
             */

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //  .AddMicrosoftIdentityWebApi(options =>
            //      {
            //          Configuration.Bind("AzureAd", options);
            //          options.Events = new JwtBearerEvents();
            //          options.Events.OnTokenValidated = async context =>
            //          {
            //              string[] allowedClientApps =
            //              {
            //                  /* list of client ids to allow */
            //              };
            //              string clientappId = context?.Principal?.Claims
            //                  .FirstOrDefault(x => x.Type == "azp" || x.Type == "appid")?.Value;

            //              if (!allowedClientApps.Contains(clientappId))
            //              {
            //                  throw new UnauthorizedAccessException("The client app is not permitted to access this API");
            //              }

            //              await Task.CompletedTask;
            //          };

            //      }, options =>
            //      {
            //          Configuration.Bind("AzureAd", options);
            //      });


            // The following flag can be used to get more descriptive errors in development environments
            // Enable diagnostic logging to help with troubleshooting.  For more details, see https://aka.ms/IdentityModel/PII.
            // You might not want to keep this following flag on for production
            IdentityModelEventSource.ShowPII = true;

            services.AddMvc(options =>
            {
                // options.Filters.Add<CustomExceptionFilter>();
            });

            services.AddControllers();

            services.AddEndpointsApiExplorer();

            var scheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                In = ParameterLocation.Header,
                Name = "Authorization?",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                },

                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri("https://login.microsoftonline.com/5cf954a5-057f-4af4-94da-71b8bfe8e39b/oauth2/v2.0/authorize"),
                        TokenUrl = new Uri("https://login.microsoftonline.com/5cf954a5-057f-4af4-94da-71b8bfe8e39b/oauth2/v2.0/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "api://catcam-api/api.access", "access web api as a user"}
                        }
                    }
                }
            };

            services.AddSwaggerGen(config =>
            {
                config.AddSecurityDefinition(scheme.Reference.Id, scheme);
                config.AddSecurityRequirement( new OpenApiSecurityRequirement
                {
                    { scheme, Array.Empty<string>()}
                });
            });
            services.AddHttpClient();

            services.AddSingleton<IEntityProvider, EntityProviderTableStorage>();
            services.AddSingleton<IClipBrowser, ClipBrowserAzureBlobStorage>();
            services.AddSingleton<IIOTHubService, IOTHubService>();
        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
            services.AddSingleton<ISecretsManager, UserSecretsSecretManager>();
            // services.AddSingleton<IAuthorizationHandler, AlwaysAllowAuthHandler>();
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            ConfigureCommonServices(services);
            services.AddSingleton<ISecretsManager, EnvVarSecretManager>();
            // Auth handlers after app roles are defined and worked out
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
                // PII hiding in log files is enabled by default for GDPR concerns.
                // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.
                // Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.EnableTryItOutByDefault();
                    options.OAuthUsePkce();
                    options.OAuthClientId("a6fe10f9-9e44-4865-8ee8-ccacca8715f1");
                    options.OAuthScopes(new string[] { "api://catcam-api/api.access" });
                });
            }
            else
            {
                app.UseHsts();
                // dev certs don't work on fedora. need to do manual setup to use https locally
                app.UseHttpsRedirection();
            }


            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }
    }
}