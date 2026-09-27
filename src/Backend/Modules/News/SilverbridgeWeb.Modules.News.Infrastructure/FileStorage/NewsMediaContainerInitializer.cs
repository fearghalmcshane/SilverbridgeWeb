using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SilverbridgeWeb.Modules.News.Infrastructure.FileStorage;

/// <summary>
/// Grants anonymous blob-level read on the media container so browsers can render media by URL.
/// </summary>
/// <remarks>
/// In Azure this is provisioned declaratively in the AppHost, and the app identity deliberately lacks
/// the container-ACL rights this would need, so a failure here is expected and non-fatal. It exists for
/// the Azurite emulator, which creates containers private and offers no provisioning hook.
/// </remarks>
internal sealed class NewsMediaContainerInitializer(
    BlobContainerClient blobContainerClient,
    ILogger<NewsMediaContainerInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

            BlobContainerProperties properties =
                await blobContainerClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            if (properties.PublicAccess != PublicAccessType.Blob)
            {
                await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
            }
        }
        catch (RequestFailedException exception)
        {
            logger.LogWarning(
                exception,
                "Could not ensure anonymous read access on news media container {ContainerName}. " +
                "Media will fail to render if the container is not already publicly readable.",
                blobContainerClient.Name);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
