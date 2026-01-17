using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Teams.Domain.Teams;

public static class TeamErrors
{
    public static Error NameRequired => Error.Validation("Team.NameRequired", "Team name is required.");

    public static Error NotFound(Guid id) => Error.NotFound("Team.NotFound", $"Team with ID {id} was not found.");

    public static Error SquadMemberAlreadyExists => Error.Conflict(
        "Team.SquadMemberAlreadyExists",
        "Squad member already exists on this team.");

    public static Error SquadMemberNotFound => Error.NotFound(
        "Team.SquadMemberNotFound",
        "Squad member not found on this team.");
}
