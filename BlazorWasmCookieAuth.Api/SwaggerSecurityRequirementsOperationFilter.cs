using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BlazorWasmCookieAuth.Api
{
    public class SwaggerSecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //if (!context.ApiDescription.TryGetMethodInfo(out var methodInfo))
            //{
            //    return;
            //}

            //var authRequired = methodInfo.GetCustomAttributes(true)
            //    .Union(methodInfo?.DeclaringType?.GetCustomAttributes(true))
            //    .OfType<AuthorizeAttribute>()
            //    .Any();

            if (operation.Security == null)
                operation.Security = new List<OpenApiSecurityRequirement>();

            if (true)
            {
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

                var oAuthScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" },
                };

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [oAuthScheme] = new[] { "api" },
                    },
                };
            }
        }
    }
}
