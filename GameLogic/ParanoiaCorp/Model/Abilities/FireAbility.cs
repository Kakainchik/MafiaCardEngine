using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
using GameLogic.ParanoiaCorp.Model.Visitors;
using GameLogic.Roles;

namespace GameLogic.ParanoiaCorp.Model.Abilities
{
    [AbilityPriority(APriority.B8)]
    public class FireAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public FireAbility(IExecutor holder, ITarget target)
        {
            Holder = holder;
            Targets = new[] { target };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            //No pre-condition
        }

        public void SendVisitor()
        {
            FireVisitor visitor = new FireVisitor(Holder);
            Targets[0].Visitors.Add(visitor);
            Holder.HasLeftHouse = true;
        }
    }
}