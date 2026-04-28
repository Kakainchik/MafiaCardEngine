using GameLogic.Interfaces;
using GameLogic.Model;

namespace GameLogic.Cycles.Night.Visitors
{
    public class DriverVisitor : BaseVisitor
    {
        public DriverVisitor(IExecutor visitor) : base(visitor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            //No restrictions
            Success = true;
            return new ActionLog
            {
                Action = ActionType.DRIVER_SWAP,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}