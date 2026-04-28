using GameLogic.Cycles.Night;
using GameLogic.Cycles.Night.Visitors;

namespace GameLogic.Interfaces
{
    public interface ITarget
    {
        bool IsAlive { get; set; }
        bool HasLeftHouse { get; set; }
        IRoleOwner? Owner { get; set; }
        ICollection<IVisitor> Visitors { get; }

        IEnumerable<ActionLog> AcceptVisitors();
    }
}