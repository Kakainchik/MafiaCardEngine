using GameLogic.Roles;
using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// The role of a security specialist. Belongs to <see cref="Team.CORPORATION">Corporation</see>.
    /// During overtime, he can block a single target. The target cancels all their actions.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Team(Team.CORPORATION)]
    public class SecuritySpecialistRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new InvestigateAbility(this, targets[0]);
        }
    }
}