using GameLogic.Cycles.Night;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;
using GameLogic.Model;
using GameLogic.ParanoiaCorp.Roles;

namespace GameLogic.ParanoiaCorp.Model.Visitors
{
    public class FireVisitor : BaseVisitor
    {
        public FireVisitor(IExecutor executor) : base(executor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            if(target is ShadowDirectorRole)
            {
                //Shadow Director cannot get killed at the night
                Success = false;
            }
            else
            {
                target.IsAlive = false;
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