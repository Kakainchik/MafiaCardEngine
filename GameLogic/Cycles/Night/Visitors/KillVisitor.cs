using GameLogic.Interfaces;
using GameLogic.Model;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Visitors
{
    public class KillVisitor : BaseVisitor
    {
        public KillVisitor(IExecutor executor) : base(executor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            if(target is GodfatherRole)
            {
                //Godfather cannot get killed at the night
                Success = false;
            }
            else
            {
                target.IsAlive = false;
                target.Owner?.SetDeathReason(Visitor);
                Success = true;
            }

            return new ActionLog
            {
                Action = ActionType.KILL,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}