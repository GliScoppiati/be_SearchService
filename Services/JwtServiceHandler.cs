using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace SearchService.Services
{
    public class JwtServiceHandler : DelegatingHandler
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<JwtServiceHandler> _logger;
        private string? _cachedToken;
        private DateTime _expiresUtc;

        public JwtServiceHandler(
            IConfiguration cfg,
            ILogger<JwtServiceHandler> logger)
        {
            _cfg    = cfg;
            _logger = logger;

            _logger.LogDebug("[SearchService] 🛠️ JwtServiceHandler instantiated.");
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage req, CancellationToken ct)
        {
            if (_cachedToken is null || DateTime.UtcNow >= _expiresUtc)
            {
                GenerateToken();
                _logger.LogInformation(
                    "[SearchService] 🔐 Generated new JWT, expires at {ExpiresUtc}.",
                    _expiresUtc
                );
            }
            else
            {
                _logger.LogDebug(
                    "[SearchService] 🔐 Reusing cached JWT, expires at {ExpiresUtc}.",
                    _expiresUtc
                );
            }

            req.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _cachedToken);

            return base.SendAsync(req, ct);
        }

        private void GenerateToken()
        {
            var key = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now     = DateTime.UtcNow;
            _expiresUtc = now.AddHours(12);

            var jwt = new JwtSecurityToken(
                issuer:    _cfg["Jwt:Issuer"],
                audience:  _cfg["Jwt:Audience"],
                claims:    new[] { new Claim(ClaimTypes.Role, "Service") },
                notBefore: now,
                expires:   _expiresUtc,
                signingCredentials: creds);

            _cachedToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
