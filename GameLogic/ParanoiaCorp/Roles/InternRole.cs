using GameLogic.Roles;
using GameLogic.Attributes;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// Intern role. Belongs to <see cref="Team.CORPORATION">Corporation</see>.
    /// No abilities. Recruits into <see cref="MasterOfCompromisingRole">Master of Compromising</see>.
    /// </summary>
    [Executor(ExecutorType.NONE)]
    [Team(Team.CORPORATION)]
    public class InternRole : Role
    {

    }
}