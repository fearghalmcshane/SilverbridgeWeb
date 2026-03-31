using System.Net;
using Microsoft.Extensions.Options;

namespace SilverbridgeWeb.Modules.Foireann.Infrastructure;

internal sealed class FoireannAuthDelegatingHandler(IOptions<FoireannOptions> options)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        FoireannOptions foireannOptions = options.Value;

        request.Headers.Authorization = new("Bearer", foireannOptions.PrimaryApiKey);

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized
            && !string.IsNullOrEmpty(foireannOptions.SecondaryApiKey))
        {
            using HttpRequestMessage retry = await CloneRequestAsync(request);
            retry.Headers.Authorization = new("Bearer", foireannOptions.SecondaryApiKey);
            response.Dispose();
            response = await base.SendAsync(retry, cancellationToken);
        }

        return response;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        if (request.Content is not null)
        {
            byte[] content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);

            foreach (System.Collections.Generic.KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (System.Collections.Generic.KeyValuePair<string, IEnumerable<string>> header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
