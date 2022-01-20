using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Events.OnSigningOut = async context => { await context.HttpContext.RevokeUserRefreshTokenAsync(); };
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
 {
     options.Authority = builder.Configuration.GetValue<string>("Authentication:Authority");
    // to test token refresh, we use 'interactive.confidential.short' -> token life time is 75 seconds
    options.ClientId = builder.Configuration.GetValue<string>("Authentication:ClientId");
     options.ClientSecret = builder.Configuration.GetValue<string>("Authentication:ClientSecret");
     options.ResponseType = OpenIdConnectResponseType.Code;
     options.Scope.Add("api");
     options.Scope.Add("offline_access");
     options.SaveTokens = true;
     options.UsePkce = true;
     options.GetClaimsFromUserInfoEndpoint = true;
     options.TokenValidationParameters = new TokenValidationParameters
     {
         NameClaimType = JwtClaimTypes.Name,
         RoleClaimType = JwtClaimTypes.Role,
     };
 });

builder.Services.AddAccessTokenManagement();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
