using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// Cult leader role. Belongs to <see cref="Team.CULTUS">Cult</see>.
    /// Can recruit someone into the cult every night, can't talk to cult members but can hear them.
    /// Can't recruit the godfather.
    /// Unique.
    /// </summary>
    [Unique]
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.GOVERMENT)]
    [ChatScope(ChatScope.CULTUS, CanWrite = false)]
    [Team(Team.CULTUS)]
    public class CultusLeaderRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new RecruiterAbility(this, targets[0]);
        }
    }
}