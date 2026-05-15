using GameLogic.Interfaces;
using GameLogic.Model;
using GameLogic.ParanoiaCorp.Roles;

namespace GameLogic.Cycles.Night.Visitors
{
    public class BlockerVisitor : BaseVisitor
    {
        public BlockerVisitor(IExecutor visitor) : base(visitor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            //No restriction
            Success = true;

            if(Visitor is HackerRole hacker)
            {
                hacker.PreviousTarget = target;
                hacker.DidActionPreviousTurn = true;
            }

            return new ActionLog
            {
                Action = ActionType.BLOCK,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}