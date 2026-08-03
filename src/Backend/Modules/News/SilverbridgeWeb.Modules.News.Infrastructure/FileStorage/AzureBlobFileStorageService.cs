using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Storage;

namespace SilverbridgeWeb.Modules.News.Infrastructure.FileStorage;

internal sealed class AzureBlobFileStorageService(
    BlobServiceClient blobServiceClient,
    IOptions<FileStorageOptions> fileStorageOptions) : IFileStorageService
{
    private static int _publicAccessEnsured;

    private readonly BlobContainerClient _blobContainerClient =
        blobServiceClient.GetBlobContainerClient(fileStorageOptions.Value.ContainerName);

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        // Public read access on individual blobs is required so the WebUI can render uploaded media
        // directly via the returned blob URL (no auth header/SAS token). Without this, the container
        // defaults to private and anonymous GETs from the browser return 403.
        Response<BlobContainerInfo>? createResponse =
            await _blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        if (createResponse is null && System.Threading.Interlocked.Exchange(ref _publicAccessEnsured, 1) == 0)
        {
            // Container already existed (e.g. created before this fix, or by a prior run without the
            // public access level set) - explicitly (re)apply the access policy so existing containers
            // are corrected too. This is done once per process to avoid a network call on every upload.
            await _blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
        }

        string blobName = $"{Ulid.NewUlid().ToString().ToLowerInvariant()}{Path.GetExtension(fileName)}";

        BlobClient blobClient = _blobContainerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            },
            cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(Uri blobUri, CancellationToken cancellationToken)
    {
        string blobName = Path.GetFileName(blobUri.LocalPath);

        if (string.IsNullOrWhiteSpace(blobName))
        {
            return;
        }

        BlobClient blobClient = _blobContainerClient.GetBlobClient(Uri.UnescapeDataString(blobName));

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string blobUrl, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(blobUrl, UriKind.Absolute, out Uri? uri))
        {
            return;
        }

        await DeleteAsync(uri, cancellationToken);
    }
}
