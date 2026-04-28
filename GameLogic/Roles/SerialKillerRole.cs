using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// Serial killer role. Independent and belongs to <see cref="Team.SERIAL_KILLER">None</see>.
    /// Can kill anyone every night.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.KILLING)]
    [Team(Team.SERIAL_KILLER)]
    public class SerialKillerRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new SerialKillerAbility(this, targets[0]);
        }
    }
}