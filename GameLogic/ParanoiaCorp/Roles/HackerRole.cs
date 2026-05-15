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
    /// Role of a hacker. Belongs to <see cref="Team.SYNDICATE">Syndicate</see>.
    /// Can block another person's actions every overtime.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [ChatScope(ChatScope.SYNDICATE, CanWrite = true)]
    [Team(Team.SYNDICATE)]
    public class HackerRole : Role, IExecutor
    {
        public bool DidActionPreviousTurn { get; set; }
        public ITarget? PreviousTarget { get; set; }

        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            if(PreviousTarget is not null && targets[0] == PreviousTarget)
            {
                return new IdleAbility(this);
            }
            else
            {
                return new BlockAbility(this, targets[0]);
            }
        }
    }
}