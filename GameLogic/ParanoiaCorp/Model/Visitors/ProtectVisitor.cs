using GameLogic.Cycles.Night;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;
using GameLogic.Model;

namespace GameLogic.ParanoiaCorp.Model.Visitors
{
    public class ProtectVisitor : BaseVisitor
    {
        public ProtectVisitor(IExecutor executor) : base(executor)
        {
            
        }

        public override ActionLog VisitTarget(ITarget target)
        {
            int killers = target.Visitors.OfType<FireVisitor>()
                .Count(v => v.Success);

            if(killers == 0)
            {
                //Protecting with no kill executed is pointless
                Success = false;
            }
            else if(killers == 1)
            {
                //Protect only if one killer visited the target
                target.IsAlive = true;
                Success = true;
            }
            else
            {
                //Protecting with many killers is successful
                //but not saving the player
                Success = true;
            }

            return new ActionLog
            {
                Action = ActionType.PROTECT,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}