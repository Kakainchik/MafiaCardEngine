using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;
using ChatScope = GameLogic.ParanoiaCorp.Attributes.ChatScope;
using ChatScopeAttribute = GameLogic.ParanoiaCorp.Attributes.ChatScopeAttribute;
using GameLogic.Roles;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// The role of a spy. Belongs to <see cref="Team.SYNDICATE">Syndicate</see>.
    /// Each overtime can study one person, as a result of which he can learn their role at the end of the overtime.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [ChatScope(ChatScope.SYNDICATE, CanWrite = true)]
    [Team(Team.SYNDICATE)]
    public class SpyRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new InvestigateAbility(this, targets[0]);
        }
    }
}