using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
using GameLogic.ParanoiaCorp.Model.Visitors;
using GameLogic.Roles;

namespace GameLogic.ParanoiaCorp.Model.Abilities
{
    [AbilityPriority(APriority.B128)]
    public class RecruiterAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public RecruiterAbility(IExecutor holder, ITarget target)
        {
            Holder = holder;
            Targets = new[] { target };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            //No-condition
        }

        public void SendVisitor()
        {
            RecruiterVisitor visitor = new RecruiterVisitor(Holder);
            Targets[0].Visitors.Add(visitor);
            Holder.HasLeftHouse = true;
        }
    }
}