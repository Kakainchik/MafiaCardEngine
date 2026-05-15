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
    /// Role of a System Administrator. Belongs to <see cref="Team.CORPORATION">Corporation</see>.
    /// Can block another person's actions every overtime.
    /// Recruited as a <see cref="HackerRole">Hacker</see>.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Team(Team.CORPORATION)]
    public class SystemAdministratorRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new BlockAbility(this, targets[0]);
        }
    }
}