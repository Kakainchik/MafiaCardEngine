using GameLogic.Attributes;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    [AbilityPriority(APriority.B64)]
    public class TerroristAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public TerroristAbility(IExecutor holder, ITarget target)
        {
            Holder = holder;
            Targets = new[] { target };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            //No condition
        }

        public void SendVisitor()
        {
            TerroristVisitor visitor = new TerroristVisitor(Holder);
            Targets[0].Visitors.Add(visitor);

            if(Targets[0] == Holder)
            {
                Holder.HasLeftHouse = false;
            }
            else
            {
                Holder.HasLeftHouse = true;
            }
        }
    }
}