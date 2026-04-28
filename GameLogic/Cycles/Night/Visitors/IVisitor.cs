using GameLogic.Interfaces;

namespace GameLogic.Cycles.Night.Visitors
{
    public interface IVisitor
    {
        ITarget Visitor { get; }
        bool Success { get; }

        ActionLog VisitTarget(ITarget target);
    }
}