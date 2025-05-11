using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SearchService.Services;

public class ClearAuthHeaderHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = null;     // wipe
        return base.SendAsync(request, cancellationToken);
    }
}
