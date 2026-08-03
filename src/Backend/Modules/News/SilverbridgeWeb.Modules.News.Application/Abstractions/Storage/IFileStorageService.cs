namespace SilverbridgeWeb.Modules.News.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken);

    Task DeleteAsync(Uri blobUri, CancellationToken cancellationToken);

    Task DeleteAsync(string blobUrl, CancellationToken cancellationToken);
}
