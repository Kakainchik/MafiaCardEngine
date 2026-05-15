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
    /// Raider role. Independent and belongs to <see cref="Team.SINGLES">None</see>.
    /// During overtime, either block or dismiss the target. The result is determined randomly.
    /// The result of the action is determined randomly.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Team(Team.SINGLES)]
    public class RaiderRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            int r = Random.Shared.Next(2);
            if(r == 0)
            {
                return new BlockAbility(this, targets[0]);
            }
            else
            {
                return new FireAbility(this, targets[0]);
            }
            
        }
    }
}