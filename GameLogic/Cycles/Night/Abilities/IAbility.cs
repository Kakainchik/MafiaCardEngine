using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    public interface IAbility
    {
        ITarget[] Targets { get; }
        IExecutor Holder { get; }

        void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles);
        void SendVisitor();
    }
}