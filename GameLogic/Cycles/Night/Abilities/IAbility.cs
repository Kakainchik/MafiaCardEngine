using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    public interface IAbility
    {
        ITarget[] Targets { get; }
        IExecutor Holder { get; }

        /// <summary>
        /// A method that moderate the order of the abilities.
        /// It is called before the execution of the abilities, and it can change the order of the abilities based on the alive roles.
        /// </summary>
        /// <param name="order">The list of abilities in the order of execution</param>
        /// <param name="aliveRoles">The list of alive roles</param>
        void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles);
        void SendVisitor();
    }
}