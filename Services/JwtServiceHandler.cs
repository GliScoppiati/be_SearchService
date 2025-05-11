using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SearchService.Services;

public class JwtServiceHandler : DelegatingHandler
{
    private readonly IConfiguration _cfg;
    private string? _cached;
    private DateTime _expiresUtc;

    public JwtServiceHandler(IConfiguration cfg)
    {
        _cfg = cfg;
        Console.WriteLine("🛠️ JwtServiceHandler istanziato!");
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage req, CancellationToken ct)
    {
        if (_cached is null || DateTime.UtcNow >= _expiresUtc)
            GenerateToken();

        Console.WriteLine("🔐 Generated JWT: " + _cached);

        req.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _cached);

        return base.SendAsync(req, ct);
    }

    private void GenerateToken()
    {
        var key   = new SymmetricSecurityKey(
                      Encoding.ASCII.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddHours(12);

        var jwt = new JwtSecurityToken(
            issuer:    _cfg["Jwt:Issuer"],
            audience:  _cfg["Jwt:Audience"],
            claims:    new[] { new Claim(ClaimTypes.Role, "Service") },
            notBefore: DateTime.UtcNow,
            expires:   expires,
            signingCredentials: creds);

        _cached      = new JwtSecurityTokenHandler().WriteToken(jwt);
        _expiresUtc  = expires;
    }
}
