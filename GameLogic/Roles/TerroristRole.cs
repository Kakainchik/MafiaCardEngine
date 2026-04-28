using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// Terrorist role. Independent and belongs to <see cref="Team.TERRORIST">None</see>.
    /// Can commit a suicide bombing in someone's house at night,
    /// thereby killing himself and everyone who was in that house that night.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.GOVERMENT)]//REMARK
    [Team(Team.TERRORIST)]
    public class TerroristRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new TerroristAbility(this, targets[0]);
        }
    }
}