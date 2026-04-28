using GameLogic.Interfaces;
using GameLogic.Model;

namespace GameLogic.Cycles.Night.Visitors
{
    public class TerroristVisitor : BaseVisitor
    {
        public TerroristVisitor(IExecutor visitor) : base(visitor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            //Always success
            //Can kill godfather at night

            //Kill all visitors of the target
            IEnumerable<ITarget> guests = target.Visitors.Select(v => v.Visitor);
            foreach(ITarget guest in guests)
            {
                if(guest.IsAlive)
                {
                    guest.IsAlive = false;
                    guest.Owner?.SetDeathReason(Visitor);
                }
            }

            //If target is at home - kill target as well
            if(!target.HasLeftHouse)
            {
                target.IsAlive = false;
                target.Owner?.SetDeathReason(Visitor);
            }

            Success = true;
            return new ActionLog
            {
                Action = ActionType.TERRORIST_BLOW,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }
    }
}