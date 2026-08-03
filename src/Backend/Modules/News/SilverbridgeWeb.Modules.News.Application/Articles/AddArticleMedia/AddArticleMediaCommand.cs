using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.News.Application.Articles.AddArticleMedia;

public sealed class AddArticleMediaCommand : ICommand<Guid>
{
    public Guid ArticleId { get; init; }

    public string BlobUrl { get; init; }

    public string MediaType { get; init; }

    public string? AltText { get; init; }

    public int DisplayOrder { get; init; }
}
