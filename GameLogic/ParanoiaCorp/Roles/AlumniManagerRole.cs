using GameLogic.Roles;
using GameLogic.Attributes;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;
using ChatScope = GameLogic.ParanoiaCorp.Attributes.ChatScope;
using ChatScopeAttribute = GameLogic.ParanoiaCorp.Attributes.ChatScopeAttribute;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// The role of an Alumni Manager. Belongs to <see cref="Team.OUTSOURCE">Outsource</see>.
    /// At overtime, can speak to the fired employees.
    /// Is Unique.
    /// </summary>
    [Unique]
    [Executor(ExecutorType.NONE)]
    [ChatScope(ChatScope.OUTSOURCE, CanWrite = true)]
    [ChatScope(ChatScope.FIRED, CanWrite = true)]
    [Team(Team.OUTSOURCE)]
    public class AlumniManagerRole : Role
    {

    }
}