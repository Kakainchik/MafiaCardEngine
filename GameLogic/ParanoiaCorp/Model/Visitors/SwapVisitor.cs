using GameLogic.Cycles.Night;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;
using GameLogic.Model;

namespace GameLogic.ParanoiaCorp.Model.Visitors
{
    public class SwapVisitor : BaseVisitor
    {
        public SwapVisitor(IExecutor visitor) : base(visitor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            //No restrictions
            Success = true;
            return new ActionLog
            {
                Action = ActionType.SWAP,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}