using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
using GameLogic.ParanoiaCorp.Model.Visitors;
using GameLogic.Roles;

namespace GameLogic.ParanoiaCorp.Model.Abilities
{
    [AbilityPriority(APriority.B16)]
    public class ProtectAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public ProtectAbility(IExecutor executor, ITarget target)
        {
            Holder = executor;
            Targets = new[] { target };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            //Protector cannot protect himself
            if(Targets[0] == Holder)
            {
                order.Remove(this);
            }

            //Just one protector can protect per target
            //Remove another protectors' actions at the target
            IEnumerable<ProtectAbility> protectors = order.OfType<ProtectAbility>()
                .Where(d => d.Targets[0] == Targets[0]);

            ProtectAbility first = protectors.First();

            protectors = protectors.Where(d => d != first);
            order = order.Except(protectors)
                .ToList();
        }

        public void SendVisitor()
        {
            ProtectVisitor visitor = new ProtectVisitor(Holder);
            Targets[0].Visitors.Add(visitor);
            Holder.HasLeftHouse = true;
        }
    }
}