using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// Vigilante role. Belongs to <see cref="Team.TOWN">Town</see>.
    /// Can kill someone every night until runs out of ammo.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Category(RoleCategory.KILLING)]
    [Team(Team.TOWN)]
    public class VigilanteRole : Role, IExecutor
    {
        public int Items { get; set; }

        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            //Check bullets amount
            if(Items == 0)
            {
                //In case of someone forces to invoke ability
                return new IdleAbility(this);
            }
            else
            {
                Items--;
                return new VigilanteAbility(this, targets[0]);
            }
        }
    }
}