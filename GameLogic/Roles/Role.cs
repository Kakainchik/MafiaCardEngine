using GameLogic.Cycles.Night;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// Generic class for other roles. Contains primary properties.
    /// <b>All children of this class must have empty constructor.</b>
    /// </summary>
    public abstract class Role : ITarget
    {
        public bool IsAlive { get; set; }
        public bool HasLeftHouse { get; set; }
        public IRoleOwner? Owner { get; set; }
        public ICollection<IVisitor> Visitors { get; }

        public Role()
        {
            IsAlive = true;
            HasLeftHouse = false;
            Visitors = new List<IVisitor>();
        }

        public virtual IEnumerable<ActionLog> AcceptVisitors()
        {
            foreach(IVisitor visitor in Visitors)
            {
                yield return visitor.VisitTarget(this);
            }
        }

        public override string ToString()
        {
            return $"[{Owner?.Id}]: {this.GetType().Name}";
        }
    }
}