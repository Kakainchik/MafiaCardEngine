using GameLogic.Interfaces;
using GameLogic.Model;

namespace GameLogic.Cycles.Night.Visitors
{
    public class InvestigateVisitor : BaseVisitor
    {
        public InvestigateVisitor(IExecutor visitor) : base(visitor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            //No restrictions
            Success = true;
            return new ActionLog
            {
                Action = ActionType.INVESTIGATE,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}