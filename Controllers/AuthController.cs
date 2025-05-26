using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly APS _aps;
    private readonly string _frontendUrl;

    public AuthController(APS aps, IConfiguration configuration)
    {
        _aps = aps;
        _frontendUrl = configuration["FrontendUrl"];
    }

    public static async Task<Tokens> PrepareTokens(
        HttpRequest request,
        HttpResponse response,
        APS aps
    )
    {
        if (!request.Cookies.ContainsKey("internal_token"))
        {
            Log.Warning("PrepareTokens: No internal token found");
            return null;
        }
        var tokens = new Tokens
        {
            PublicToken = request.Cookies["public_token"],
            InternalToken = request.Cookies["internal_token"],
            RefreshToken = request.Cookies["refresh_token"],
            ExpiresAt = DateTime.Parse(request.Cookies["expires_at"]),
        };
        if (tokens.ExpiresAt < DateTime.Now.ToUniversalTime())
        {
            Log.Information("Tokens expired, refreshing...");
            tokens = await aps.RefreshTokens(tokens);
            response.Cookies.Append("public_token", tokens.PublicToken);
            response.Cookies.Append("internal_token", tokens.InternalToken);
            response.Cookies.Append("refresh_token", tokens.RefreshToken);
            response.Cookies.Append("expires_at", tokens.ExpiresAt.ToString());
        }
        return tokens;
    }

    [HttpGet("login")]
    public ActionResult Login()
    {
        var redirectUri = _aps.GetAuthorizationURL();
        Log.Information("Redirecting to login URL: {Url}", redirectUri);
        return Redirect(redirectUri);
    }

    [HttpGet("logout")]
    public ActionResult Logout()
    {
        Log.Information("User logging out");
        Response.Cookies.Delete("public_token");
        Response.Cookies.Delete("internal_token");
        Response.Cookies.Delete("refresh_token");
        Response.Cookies.Delete("expires_at");
        return Redirect(_frontendUrl);
    }

    [HttpGet("callback")]
    public async Task<ActionResult> Callback(string code)
    {
        try
        {
            Log.Information("Received callback with code");
            var tokens = await _aps.GenerateTokens(code);
            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
            };
            Response.Cookies.Append("public_token", tokens.PublicToken, cookieOptions);
            Response.Cookies.Append("internal_token", tokens.InternalToken, cookieOptions);
            Response.Cookies.Append("refresh_token", tokens.RefreshToken, cookieOptions);
            Response.Cookies.Append("expires_at", tokens.ExpiresAt.ToString(), cookieOptions);
            return Redirect($"{_frontendUrl}/hubs");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in callback: {Message}", ex.Message);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("profile")]
    public async Task<ActionResult> GetProfile()
    {
        try
        {
            var tokens = await PrepareTokens(Request, Response, _aps);
            if (tokens == null)
            {
                Log.Warning("GetProfile: No tokens found");
                return Unauthorized();
            }
            var profile = await _aps.GetUserProfile(tokens);
            Log.Information("Retrieved profile for user: {Name}", profile.Name);
            return Ok(new { name = profile.Name, email = profile.Email });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting profile: {Message}", ex.Message);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpGet("token")]
    public async Task<ActionResult> GetPublicToken()
    {
        try
        {
            var tokens = await PrepareTokens(Request, Response, _aps);
            if (tokens == null)
            {
                Log.Warning("GetPublicToken: No tokens found");
                return Unauthorized();
            }
            var expiresIn = Math.Floor(
                (tokens.ExpiresAt - DateTime.Now.ToUniversalTime()).TotalSeconds
            );
            return Ok(new { access_token = tokens.PublicToken, expires_in = expiresIn });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting public token: {Message}", ex.Message);
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}
