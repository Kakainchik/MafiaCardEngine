using GameLogic.Roles;
using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;
using ChatScope = GameLogic.ParanoiaCorp.Attributes.ChatScope;
using ChatScopeAttribute = GameLogic.ParanoiaCorp.Attributes.ChatScopeAttribute;
using GameLogic.ParanoiaCorp.Model.Abilities;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// Belongs to <see cref="Team.SYNDICATE">Syndicate</see>.
    /// At every overtime, he can select a single target for compromising material.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [ChatScope(ChatScope.SYNDICATE, CanWrite = true)]
    [Team(Team.SYNDICATE)]
    public class MasterOfCompromisingRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new CompromiseAbility(this, targets[0]);
        }
    }
}