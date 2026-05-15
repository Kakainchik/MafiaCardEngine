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
    /// Anticrisis Manager role. Belongs to <see cref="Team.CORPORATION">Corporation</see>.
    /// Can compromise someone every overtime until runs out of ammo.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Team(Team.CORPORATION)]
    public class AnticrisisManagerRole : Role, IExecutor
    {
        public int Items { get; set; }

        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            //Check compromise tokens amount
            if(Items == 0)
            {
                //In case of someone forces to invoke ability
                return new IdleAbility(this);
            }
            else
            {
                Items--;
                return new TokenFireAbility(this, targets[0]);
            }
        }
    }
}