using GameLogic.Roles;
using GameLogic.Attributes;
using GameLogic.Interfaces;
using GameLogic.Cycles.Night.Abilities;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;
using ChatScope = GameLogic.ParanoiaCorp.Attributes.ChatScope;
using ChatScopeAttribute = GameLogic.ParanoiaCorp.Attributes.ChatScopeAttribute;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.ParanoiaCorp.Model.Abilities;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// The role of an Outsourcer. Belongs to <see cref="Team.OUTSOURCE">Outsource</see>.
    /// Every overtime decides who to offer an outcource contract.
    /// If the selected target got a compromtation the same shift, they become an <see cref="OutsourcerRole">Outsourcer</see>.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [ChatScope(ChatScope.OUTSOURCE, CanWrite = true)]
    [Team(Team.OUTSOURCE)]
    public class OutsourcerRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new ResurrectAbility(this, targets[0]);
        }
    }
}