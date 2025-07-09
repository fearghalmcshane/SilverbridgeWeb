using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Events.Application.Categories.CreateCategory;

public sealed record CreateCategoryCommand(string Name) : ICommand<Guid>;
