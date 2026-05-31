using GameLogic.Cycles.Night;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;
using GameLogic.Model;
using GameLogic.ParanoiaCorp.Roles;
using GameLogic.Roles;

namespace GameLogic.ParanoiaCorp.Model.Visitors
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
                case ShadowDirectorRole:
                {
                    SyndicateRecruit(target);
                    break;
                }
                case StartupFounderRole:
                {
                    StartupRecruit(target);
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

        private void SyndicateRecruit(ITarget target)
        {
            if(target is InternRole)
            {
                if(target.IsAlive)
                {
                    target.Owner?.ChangeRole(new MasterOfCompromisingRole());
                }
                Success = true;
            }
            else if(target is SystemAdministratorRole)
            {
                if(target.IsAlive)
                {
                    target.Owner?.ChangeRole(new HackerRole());
                }
                Success = true;
            }
            else
            {
                Success = false;
            }
        }

        private void StartupRecruit(ITarget target)
        {
            if(target is EvangelistRole
                || target is ShadowDirectorRole
                || target is StartupFounderRole
                || target is AlumniManagerRole)
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