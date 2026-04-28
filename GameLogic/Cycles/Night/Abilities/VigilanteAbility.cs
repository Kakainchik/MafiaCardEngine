using GameLogic.Attributes;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    [AbilityPriority(APriority.B8)]
    public class VigilanteAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public VigilanteAbility(IExecutor holder, ITarget target)
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
            KillVisitor visitor = new KillVisitor(Holder);
            Targets[0].Visitors.Add(visitor);
            Holder.HasLeftHouse = true;
        }
    }
}