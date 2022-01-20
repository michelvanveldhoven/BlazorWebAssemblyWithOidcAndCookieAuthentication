using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BlazorWasmCookieAuth.Api
{
    public class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigurationSection _oidcoptions;
        private readonly string _endpoint;

        public ConfigureSwaggerGenOptions(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _oidcoptions = config.GetSection("JWT");
            _endpoint = _oidcoptions.GetValue<string>("Authority");
        }

        public void Configure(SwaggerGenOptions options)
        {
            //swagger-ui to show a padlock 
            options.OperationFilter<SwaggerSecurityRequirementsOperationFilter>();

            options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
            {
                Description = "Duende IdentityServer",
                Type = SecuritySchemeType.OpenIdConnect,

                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{_endpoint}authorize"), //  AuthorizeEndpoint,
                        TokenUrl = new Uri($"{_endpoint}connect"),  //TokenEndpoint,
                        Scopes = {
                                { "openid", "openid" },
                                { "profile", "profile" },
                                { "email", "email" },
                                { "api", "api" },
                                { "offline_access", "offline_access" },
                        }
                    }
                },

            }); ;
        }

        private Uri AuthorizeEndpoint => new Uri(discoveryDocument.AuthorizeEndpoint);

        private Uri TokenEndpoint => new Uri(discoveryDocument.TokenEndpoint);

        private DiscoveryDocumentResponse discoveryDocument => GetDiscoveryDocument().GetAwaiter().GetResult();
        
        private async Task<DiscoveryDocumentResponse> GetDiscoveryDocument()
        {
            var r = await _httpClientFactory
                .CreateClient()
                .GetDiscoveryDocumentAsync(_oidcoptions.GetValue<string>("Authority"));
            return r;
                
        }
    }
}
