using GameLogic.Roles;
using GameLogic.Attributes;
using GameLogic.Interfaces;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;
using GameLogic.ParanoiaCorp.Model.Abilities;
using GameLogic.Cycles.Night.Abilities;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// Political Strategist role. Independent and belongs to <see cref="Team.SINGLES">None</see>.
    /// Every overtime, can make someone perform their role action on a target
    /// of her choosing, even if they missed a turn.
    /// </summary>
    [Executor(ExecutorType.TARGET_ANYTARGET)]
    [Team(Team.SINGLES)]
    public class PoliticalStrategistRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new ControlAbility(this, targets[0], targets[1]);
        }
    }
}