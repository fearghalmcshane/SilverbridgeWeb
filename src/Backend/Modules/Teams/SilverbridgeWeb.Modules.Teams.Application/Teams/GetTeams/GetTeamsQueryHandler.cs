using System.Data.Common;
using Dapper;
using SilverbridgeWeb.Common.Application.Data;
using SilverbridgeWeb.Common.Application.Messaging;
using SilverbridgeWeb.Common.Domain;

namespace SilverbridgeWeb.Modules.Teams.Application.Teams.GetTeams;

internal sealed class GetTeamsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetTeamsQuery, IReadOnlyCollection<TeamResponse>>
{
    public async Task<Result<IReadOnlyCollection<TeamResponse>>> Handle(
        GetTeamsQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 t.id AS {nameof(TeamResponse.Id)},
                 t.name AS {nameof(TeamResponse.Name)},
                 t.age_group AS {nameof(TeamResponse.AgeGroup)},
                 t.sport_type AS {nameof(TeamResponse.SportType)},
                 t.coach_name AS {nameof(TeamResponse.CoachName)},
                 t.is_active AS {nameof(TeamResponse.IsActive)},
                 COALESCE(sm_count.count, 0) AS {nameof(TeamResponse.SquadMemberCount)}
             FROM teams.teams t
             LEFT JOIN (
                 SELECT team_id, COUNT(*) AS count
                 FROM teams.squad_members
                 GROUP BY team_id
             ) sm_count ON t.id = sm_count.team_id
             ORDER BY t.age_group, t.name
             """;

        List<TeamResponse> teams = (await connection.QueryAsync<TeamResponse>(sql, request)).AsList();

        return teams;
    }
}
