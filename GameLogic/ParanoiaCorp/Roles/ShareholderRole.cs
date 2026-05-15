using GameLogic.Roles;
using GameLogic.Attributes;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;
using ChatScope = GameLogic.ParanoiaCorp.Attributes.ChatScope;
using ChatScopeAttribute = GameLogic.ParanoiaCorp.Attributes.ChatScopeAttribute;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// Role of a Shareholder. Belongs to <see cref="Team.CORPORATION">Corporation</see>.
    /// Can communicate with other Shareholders at night.
    /// </summary>
    [Executor(ExecutorType.NONE)]
    [ChatScope(ChatScope.SHAREHOLDER, CanWrite = true)]
    [Team(Team.CORPORATION)]
    public class ShareholderRole : Role
    {

    }
}