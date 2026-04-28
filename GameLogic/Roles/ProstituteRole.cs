using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// Role of a prostitute. Temporarily belongs to <see cref="Team.TOWN">Town</see>.
    /// Can block another person's actions every night.
    /// Recruited as a <see cref="WhoreRole">Whore</see>.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.SUPPORT)]
    [Team(Team.TOWN)]
    public class ProstituteRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new ProstituteAbility(this, targets[0]);
        }
    }
}