using GameLogic.Roles;
using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;
using ChatScope = GameLogic.ParanoiaCorp.Attributes.ChatScope;
using ChatScopeAttribute = GameLogic.ParanoiaCorp.Attributes.ChatScopeAttribute;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// Shadow Director role. Belongs to <see cref="Team.SYNDICATE">Syndicate</see>.
    /// During overtime, he can select a target for recruitment into the Syndicate <see cref="InternRole">Intern</see> and <see cref="SecuritySpecialistRole">Security Specialist</see>.
    /// Cannot be killed at night.
    /// Is Unique.
    /// </summary>
    [Unique]
    [Executor(ExecutorType.TARGET)]
    [ChatScope(ChatScope.SYNDICATE, CanWrite = true)]
    [Team(Team.SYNDICATE)]
    public class ShadowDirectorRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility = 1, params ITarget[] targets)
        {
            return new GameLogic.ParanoiaCorp.Model.Abilities.RecruiterAbility(this, targets[0]);
        }
    }
}