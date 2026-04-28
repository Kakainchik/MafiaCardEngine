using GameLogic.Interfaces;
using GameLogic.Model;

namespace GameLogic.Cycles.Night.Visitors
{
    public class BlockerVisitor : BaseVisitor
    {
        public BlockerVisitor(IExecutor visitor) : base(visitor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            //No restrictions
            Success = true;
            return new ActionLog
            {
                Action = ActionType.ESCORT_BLOCK,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}