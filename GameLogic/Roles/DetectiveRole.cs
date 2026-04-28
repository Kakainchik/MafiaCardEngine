using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// The role of a detective. Belongs to <see cref="Team.TOWN">Town</see>.
    /// Every night can study one person, as a result of which will learn one's role completely, will learn it immediately.
    /// Sees the godfather as a mafioso.
    /// Sees the cult leader as a cultist.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.INVESTIGATE)]
    [Team(Team.TOWN)]
    public class DetectiveRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new InvestigateAbility(this, targets[0]);
        }
    }
}