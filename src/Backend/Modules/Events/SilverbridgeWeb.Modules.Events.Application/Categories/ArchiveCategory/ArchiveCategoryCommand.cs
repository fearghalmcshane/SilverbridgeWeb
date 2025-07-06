using SilverbridgeWeb.Modules.Events.Application.Abstractions.Messaging;

namespace SilverbridgeWeb.Modules.Events.Application.Categories.ArchiveCategory;

public sealed record ArchiveCategoryCommand(Guid CategoryId) : ICommand;
