using GameLogic.Roles;
using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;
using GameLogic.ParanoiaCorp.Model.Abilities;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// The role of a Scrum Master. Belongs to <see cref="Team.CORPORATION">Corporation</see>.
    /// Each overtime can swap two people, causing the actions directed at them at night to swap between them.
    /// </summary>
    [Executor(ExecutorType.TARGET_TARGET)]
    [Category(RoleCategory.SUPPORT)]
    [Team(Team.CORPORATION)]
    public class ScrumMasterRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new SwapAbility(this, targets[0], targets[1]);
        }
    }
}