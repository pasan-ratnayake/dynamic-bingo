using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace DynamicBingo.WebApi.Authentication;

public class SimpleAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public SimpleAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? token = null;
        
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            token = authHeader.Substring("Bearer ".Length).Trim();
        }
        
        if (string.IsNullOrEmpty(token))
        {
            var queryToken = Request.Query["access_token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(queryToken))
            {
                token = Uri.UnescapeDataString(queryToken);
            }
        }

        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        
        try
        {
            var tokenData = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = tokenData.Split(':');
            
            if (parts.Length >= 2 && Guid.TryParse(parts[0], out var userId))
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, parts[1])
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
        catch
        {
        }

        return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
    }
}
