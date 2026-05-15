using GameLogic.Roles;
using GameLogic.Attributes;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// General Director role. Belongs to <see cref="Team.CORPORATION">Corporation</see>.
    /// Cannot be targeted. Has no overtime authority. Makes sole decisions on dismissals during the day.
    /// A mandatory, unique, open role.
    /// </summary>
    [Unique]
    [Executor(ExecutorType.NONE)]
    [Team(Team.CORPORATION)]
    public class GeneralDirectorRole : Role
    {
        public int VetoCount { get; set; } = 2;
    }
}