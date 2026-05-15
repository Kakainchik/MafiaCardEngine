using GameLogic.Roles;
using GameLogic.Attributes;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// Role of a clerk. Belongs to <see cref="Team.CORPORATION">Corporation</see>.
    /// No overtime authority.
    /// </summary>
    [Executor(ExecutorType.NONE)]
    [Team(Team.CORPORATION)]
    public class ClerkRole : Role
    {

    }
}