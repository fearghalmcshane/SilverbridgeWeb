using SilverbridgeWeb.Common.Application.Messaging;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.GetTeam;

public sealed record GetTeamQuery(Guid Id) : IQuery<TeamDetailResponse>;
