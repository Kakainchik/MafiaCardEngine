using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// The role of the driver. Belongs to <see cref="Team.TOWN">Town</see>.
    /// Each night can swap two people, causing the actions directed at them at night to swap between them.
    /// </summary>
    [Executor(ExecutorType.TARGET_TARGET)]
    [Category(RoleCategory.SUPPORT)]
    [Team(Team.TOWN)]
    public class DriverRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new DriverAbility(this, targets[0], targets[1]);
        }
    }
}