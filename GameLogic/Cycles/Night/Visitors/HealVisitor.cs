using GameLogic.Interfaces;
using GameLogic.Model;

namespace GameLogic.Cycles.Night.Visitors
{
    public class HealVisitor : BaseVisitor
    {
        public HealVisitor(IExecutor executor) : base(executor)
        {
            
        }

        public override ActionLog VisitTarget(ITarget target)
        {
            int killers = target.Visitors.OfType<KillVisitor>()
                .Count(v => v.Success);

            if(killers == 0)
            {
                //Healing with no kill executed is pointless
                Success = false;
            }
            else if(killers == 1)
            {
                //Heal only if one killer visited the target
                target.IsAlive = true;
                Success = true;
            }
            else
            {
                //Healing with many killers is successful
                //but not saving the life
                Success = true;
            }

            return new ActionLog
            {
                Action = ActionType.HEAL,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}