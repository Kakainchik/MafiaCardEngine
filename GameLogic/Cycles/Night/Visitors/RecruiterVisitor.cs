using GameLogic.Interfaces;
using GameLogic.Model;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Visitors
{
    public class RecruiterVisitor : BaseVisitor
    {
        public RecruiterVisitor(IExecutor visitor) : base(visitor)
        {

        }

        public override ActionLog VisitTarget(ITarget target)
        {
            switch(Visitor)
            {
                case GodfatherRole:
                {
                    GodfatherRecruit(target);
                    break;
                }
                case CultusLeaderRole:
                {
                    CultusRecruit(target);
                    break;
                }
            }

            return new ActionLog
            {
                Action = ActionType.RECRUIT,
                Executor = this.Visitor,
                Target = target,
                Success = this.Success
            };
        }

        private void GodfatherRecruit(ITarget target)
        {
            if(target is RecruitRole)
            {
                if(target.IsAlive)
                {
                    target.Owner?.ChangeRole(new RecruitRole());
                }
                Success = true;
            }
            else if(target is ProstituteRole)
            {
                if(target.IsAlive)
                {
                    target.Owner?.ChangeRole(new ProstituteRole());
                }
                Success = true;
            }
            else
            {
                Success = false;
            }
        }

        private void CultusRecruit(ITarget target)
        {
            if(target is CultistRole
                || target is GodfatherRole
                || target is CultusLeaderRole)
            {
                Success = false;
            }
            else
            {
                if(target.IsAlive)
                {
                    target.Owner?.ChangeRole(new CultistRole());
                }
                Success = true;
            }
        }
    }
}