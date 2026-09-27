using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SilverbridgeWeb.Modules.News.Application.Abstractions.Storage;

namespace SilverbridgeWeb.Modules.News.Infrastructure.FileStorage;

internal sealed class AzureBlobFileStorageService(
    BlobContainerClient blobContainerClient) : IFileStorageService
{
    private readonly BlobContainerClient _blobContainerClient = blobContainerClient;

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
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
