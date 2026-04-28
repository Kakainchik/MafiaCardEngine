using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// Standard Mafia role. Belongs to <see cref="Team.MAFIA">Mafia</see>.
    /// Every night must decide who to kill.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.KILLING)]
    [ChatScope(ChatScope.MAFIA, CanWrite = true)]
    [Team(Team.MAFIA)]
    public class MafiaRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new MafiaAbility(this, targets[0]);
        }
    }
}