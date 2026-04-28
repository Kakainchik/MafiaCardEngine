using GameLogic.Attributes;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    [AbilityPriority(APriority.B4)]
    public class ProstituteAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public ProstituteAbility(IExecutor holder, ITarget target)
        {
            Holder = holder;
            Targets = new[] { target };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            bool weBlocked = order.Exists(a => a is ProstituteAbility && a.Targets[0] == Holder);
            if(weBlocked)
            {
                //If we got blocked by other prostitute, we are not prioritized
                order.Remove(this);
                return;
            }

            //Block target so it won't execute abilities
            order.RemoveAll(a => a.Holder == Targets[0]);
        }

        public void SendVisitor()
        {
            BlockerVisitor visitor = new BlockerVisitor(Holder);
            Targets[0].Visitors.Add(visitor);
            Holder.HasLeftHouse = true;
        }
    }
}