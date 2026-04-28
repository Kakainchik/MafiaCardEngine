using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
namespace GameLogic.Roles
{
    /// <summary>
    /// Role of the Doctor. Belongs to <see cref="Team.TOWN">Town</see>.
    /// Each night can protect one person from death during that night.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.SUPPORT)]
    [Team(Team.TOWN)]
    public class DoctorRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new DoctorAbility(this, targets[0]);
        }
    }
}