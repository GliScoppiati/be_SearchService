using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SearchService.Repositories;
using SearchService.Clients;
using System;
using System.Threading.Tasks;

namespace SearchService.Controllers
{
    [ApiController]
    [Authorize(Policy = "AdminOrService")]
    [Route("cocktails/reload")]
    public class ReloadController : ControllerBase
    {
        private readonly CocktailRepository _repo;
        private readonly CocktailServiceClient _client;
        private readonly ILogger<ReloadController> _logger;

        public ReloadController(
            CocktailRepository repo,
            CocktailServiceClient client,
            ILogger<ReloadController> logger)
        {
            _repo = repo;
            _client = client;
            _logger = logger;
        }

        [Authorize(Policy = "ServiceOnly")]
        [HttpPost("now")]
        public async Task<IActionResult> ReloadNow()
        {
            _logger.LogInformation("[SearchService] 🚨 Chiamata ricevuta su /cocktails/reload/now");

            foreach (var c in User.Claims)
            {
                _logger.LogDebug(
                    "[SearchService] 🔍 CLAIM: {ClaimType} = {ClaimValue}",
                    c.Type,
                    c.Value
                );
            }

            if (Request.Headers.ContainsKey("Authorization"))
            {
                _logger.LogInformation("[SearchService] 📥 Authorization header presente");
            }

            try
            {
                await _repo.ReloadAsync(_client, force: true);
                var reloadedAt = DateTime.UtcNow;
                _logger.LogInformation(
                    "[SearchService] ✅ Reload eseguito con successo a {ReloadedAt}",
                    reloadedAt
                );

                return Ok(new
                {
                    message = "Reload executed successfully.",
                    forced = true,
                    reloadedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[SearchService] ❌ Errore durante il reload"
                );
                throw;
            }
        }
    }
}
