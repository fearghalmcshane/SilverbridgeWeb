using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.GetTeams;

public sealed record GetTeamsQuery : IQuery<IReadOnlyCollection<TeamResponse>>;
