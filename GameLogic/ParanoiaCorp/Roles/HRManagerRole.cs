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
    /// Role of the HR Manager. Belongs to <see cref="Team.CORPORATION">Corporation</see>.
    /// For every overtime, he hides incriminating information about one employee (protection).
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Team(Team.CORPORATION)]
    public class HRManagerRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new ProtectAbility(this, targets[0]);
        }
    }
}