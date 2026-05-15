using GameLogic.Roles;
using GameLogic.Attributes;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;
using ChatScope = GameLogic.ParanoiaCorp.Attributes.ChatScope;
using ChatScopeAttribute = GameLogic.ParanoiaCorp.Attributes.ChatScopeAttribute;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// Role of an Evangelist. Belongs to <see cref="Team.STARTUP">Startup</see>.
    /// During overtime, he may communicate with other Startup Evangelists.
    /// He doesn't know who is Startup Founder and cannot communicate with them directly.
    /// </summary>
    [Executor(ExecutorType.NONE)]
    [ChatScope(ChatScope.STARTUP, CanWrite = true)]
    [Team(Team.STARTUP)]
    public class EvangelistRole : Role
    {

    }
}