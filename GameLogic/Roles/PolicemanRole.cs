using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// The role of a policeman. Belongs to <see cref="Team.TOWN">Town</see>.
    /// Every night can check one person and find out whether they belong to
    /// any <see cref="Team">side</see>, finds out the next night.
    /// Sees the Godfather as the Town.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.INVESTIGATE)]
    [Team(Team.TOWN)]
    public class PolicemanRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new InvestigateAbility(this, targets[0]);
        }
    }
}