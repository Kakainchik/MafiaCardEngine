using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// Role of a whore. Belongs to <see cref="Team.MAFIA">Mafia</see>.
    /// Can block another person's actions every night.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.SUPPORT)]
    [ChatScope(ChatScope.MAFIA, CanWrite = true)]
    [Team(Team.MAFIA)]
    public class WhoreRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new ProstituteAbility(this, targets[0]);
        }
    }
}