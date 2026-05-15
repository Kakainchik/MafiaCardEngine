using GameLogic.Cycles.Night;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;
using GameLogic.Model;

namespace GameLogic.ParanoiaCorp.Model.Visitors
{
    public class ControlVisitor : BaseVisitor
    {
        public ControlVisitor(IExecutor visitor) : base(visitor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            //No restrinction for a controling role
            Success = true;
            return new ActionLog
            {
                Action = ActionType.CONTROL,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}