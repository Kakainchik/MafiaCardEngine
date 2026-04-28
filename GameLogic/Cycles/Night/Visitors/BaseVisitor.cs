using GameLogic.Interfaces;

namespace GameLogic.Cycles.Night.Visitors
{
    public abstract class BaseVisitor : IVisitor
    {
        public ITarget Visitor { get; }
        public bool Success { get; protected set; }

        protected BaseVisitor(ITarget visitor)
        {
            Visitor = visitor;
        }

        public abstract ActionLog VisitTarget(ITarget target);
    }
}