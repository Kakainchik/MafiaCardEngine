using GameLogic.Attributes;
using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    [AbilityPriority(APriority.B128)]
    public sealed class IdleAbility : IAbility
    {
        public ITarget[] Targets { get; } = Array.Empty<ITarget>();

        public IExecutor Holder { get; }

        public IdleAbility(IExecutor holder)
        {
            Holder = holder;
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {

        }

        public void SendVisitor()
        {
            //Idle, stay at home
            Holder.HasLeftHouse = false;
        }
    }
}