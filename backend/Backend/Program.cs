using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Register Mongo service
builder.Services.AddSingleton<MongoDbService>();

// Cookie + Google OAuth
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = config["Google:ClientId"];
    options.ClientSecret = config["Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
})
// Add JWT Bearer
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["JWT:Secret"]))
    };
});

builder.Services.AddAuthorization();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Google sign-in route
app.MapGet("/signin-google", async context =>
{
    await context.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/signin-google-callback" });
});

// Google callback → Save user + return token
app.MapGet("/signin-google-callback", async context =>
{
    var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    var claims = result.Principal?.Identities?.FirstOrDefault()?.Claims;
    var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

    var mongo = context.RequestServices.GetRequiredService<MongoDbService>();
    await mongo.GetOrCreateUserAsync(email, name);

    var jwt = JwtHelper.GenerateJwt(email, config["JWT:Secret"]);
    context.Response.Redirect($"/login-success?token={jwt}");
});

// Sample protected route
app.MapGet("/api/profile", [Microsoft.AspNetCore.Authorization.Authorize] (ClaimsPrincipal user) =>
{
    var email = user.FindFirst(ClaimTypes.Email)?.Value;
    return Results.Ok(new { message = "You're authenticated!", email });
});

app.Run();
