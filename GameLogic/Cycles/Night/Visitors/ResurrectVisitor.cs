using GameLogic.Extensions;
using GameLogic.Interfaces;
using GameLogic.Model;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Visitors
{
    public class ResurrectVisitor : BaseVisitor
    {
        public ResurrectVisitor(ITarget visitor) : base(visitor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            if(target.IsAlive)
            {
                //Cannot ressurect alive target
                Success = false;
            }
            else if(target.GetTeam() == Attributes.Team.UNDEAD)
            {
                //Cannot ressurect undead
                Success = false;
            }
            else
            {
                target.Owner?.ChangeRole(new ZombieRole());
                target.Owner?.SetDeathReason(null);
                Success = true;
            }

            return new ActionLog
            {
                Action = ActionType.RESSURECT,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}