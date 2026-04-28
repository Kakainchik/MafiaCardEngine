using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// The role of a surgeon. Belongs to <see cref="Team.MAFIA">Mafia</see>.
    /// Every night can protect a person from death during the given night.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.SUPPORT)]
    [ChatScope(ChatScope.MAFIA, CanWrite = true)]
    [Team(Team.MAFIA)]
    public class SurgeonRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new DoctorAbility(this, targets[0]);
        }
    }
}