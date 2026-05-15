using GameLogic.Cycles.Night;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;
using GameLogic.Model;
using GameLogic.ParanoiaCorp.Attributes;
using GameLogic.ParanoiaCorp.Roles;
using System.Reflection;

namespace GameLogic.ParanoiaCorp.Model.Visitors
{
    public class ResurrectVisitor : BaseVisitor
    {
        public ResurrectVisitor(ITarget visitor) : base(visitor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            Team GetTeam()
            {
                TeamAttribute? attr = target.GetType().GetCustomAttribute<TeamAttribute>();
                return attr?.ItsTeam
                    ?? throw new InvalidOperationException($"The {nameof(TeamAttribute)} is not set for [{target}].");
            }

            if(target.IsAlive)
            {
                //Cannot ressurect alive target
                Success = false;
            }
            else if(GetTeam() == Team.OUTSOURCE)
            {
                //Cannot ressurect outsource
                Success = false;
            }
            else
            {
                target.Owner?.ChangeRole(new OutsourcerRole());
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