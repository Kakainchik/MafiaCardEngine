using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// The role of a zombie. Belongs to <see cref="Team.UNDEAD">Undead</see>.
    /// Every night decides who to resurrect from the dead.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.SUPPORT)]
    [ChatScope(ChatScope.UNDEAD, CanWrite = true)]
    [Team(Team.UNDEAD)]
    public class ZombieRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new ZombieAbility(this, targets[0]);
        }
    }
}