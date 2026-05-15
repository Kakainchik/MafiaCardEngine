using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// Witch role. Independent and belongs to <see cref="Team.WITCH">None</see>.
    /// Every night, can make someone perform their role action on a target
    /// of her choosing, even if they missed a turn.
    /// </summary>
    [Executor(ExecutorType.TARGET_ANYTARGET)]
    [Category(RoleCategory.GOVERMENT)]
    [Team(Team.WITCH)]
    public class WitchRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new WitchAbility(this, targets[0], targets[1]);
        }
    }
}