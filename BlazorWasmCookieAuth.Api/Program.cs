using BlazorWasmCookieAuth.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>()
//                .AddSwaggerGen();
//builder.Services.AddHttpClient();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://demo.identityserver.io/connect/authorize"),
                TokenUrl = new Uri("https://demo.identityserver.io/connect/token"),
                Scopes = {
                                    { "openid", "openid" },
                                    { "profile", "profile" },
                                    { "email", "email" },
                                    { "api", "api" },
                                    { "offline_access", "offline_access" },
                            },
            },
        },
    });
    c.OperationFilter<SwaggerSecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = builder.Configuration["JWT:Authority"];
                    //options.Audience = builder.Configuration["Jwt:Audience"];
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.TokenValidationParameters.ValidateIssuer = false;
                    options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                    //options.TokenValidationParameters = new()
                    //{
                    //    ValidTypes = new[] { "at+jwt" }
                    //};
                });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId(app.Configuration.GetValue<string>("JWT:Swagger:ClientId"));
        c.OAuthClientSecret(app.Configuration.GetValue<string>("JWT:Swagger:ClientSecret"));
        c.OAuthScopes("openid", "profile", "email", "api", "offline_access");
        //c.OAuthScopes(app.Configuration.GetValue<string>("JWT:Swagger:Scopes","openid"));
        //c.OAuthScopeSeparator(app.Configuration.GetValue<string>("JWT:Swagger:ScopeSeperator", "+"));
        c.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

//[Authorize(Policy = "ApiScope")]
app.MapGet("/weatherforecast",() =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithTags("Weather");

app.MapGet("/weatherforecast/protected",() => 
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
    .RequireAuthorization("ApiScope")
    .WithName("GetProtectedWeatherForecast")
    .WithTags("Weather");
    

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}