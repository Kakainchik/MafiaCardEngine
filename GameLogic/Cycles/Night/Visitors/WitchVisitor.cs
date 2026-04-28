using GameLogic.Interfaces;
using GameLogic.Model;

namespace GameLogic.Cycles.Night.Visitors
{
    public class WitchVisitor : BaseVisitor
    {
        public WitchVisitor(IExecutor visitor) : base(visitor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            //No restrinction for a witch
            Success = true;
            return new ActionLog
            {
                Action = ActionType.WITCH_CONTROL,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}