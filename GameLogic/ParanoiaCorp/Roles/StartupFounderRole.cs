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
    /// Startup founder role. Belongs to <see cref="Team.STARTUP">Startup</see>.
    /// Can recruit someone into the startup every overtime, can't talk to startup members but can hear them.
    /// Can't recruit the <see cref="ShadowDirectorRole"/>.
    /// Is Unique.
    /// </summary>
    [Unique]
    [Executor(ExecutorType.TARGET)]
    [ChatScope(ChatScope.STARTUP, CanWrite = false)]
    [Team(Team.STARTUP)]
    public class StartupFounderRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new GameLogic.ParanoiaCorp.Model.Abilities.RecruiterAbility(this, targets[0]);
        }
    }
}