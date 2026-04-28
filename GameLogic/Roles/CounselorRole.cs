using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// The role of an advisor. Belongs to <see cref="Team.MAFIA">Mafia</see>.
    /// Each night can study one person, as a result of which he can learn his role.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.INVESTIGATE)]
    [ChatScope(ChatScope.MAFIA, CanWrite = true)]
    [Team(Team.MAFIA)]
    public class CounselorRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new InvestigateAbility(this, targets[0]);
        }
    }
}