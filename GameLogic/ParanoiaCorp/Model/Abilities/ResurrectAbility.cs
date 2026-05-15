using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Extensions;
using GameLogic.Interfaces;
using GameLogic.ParanoiaCorp.Model.Visitors;
using GameLogic.Roles;

namespace GameLogic.ParanoiaCorp.Model.Abilities
{
    [AbilityPriority(APriority.B128)]
    public class ResurrectAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public ResurrectAbility(IExecutor holder, ITarget target)
        {
            Holder = holder;
            Targets = new[] { target };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            //Only one ressurect per night
            IEnumerable<ResurrectAbility> resurrectAbilities = order.OfType<ResurrectAbility>();

            int r = Random.Shared.Next(resurrectAbilities.Count());
            ResurrectAbility desired = resurrectAbilities.ElementAt(r);

            order = order.Except(resurrectAbilities).ToList();
            order.PutByPriority(desired);
        }

        public void SendVisitor()
        {
            ResurrectVisitor visitor = new ResurrectVisitor(Holder);
            Targets[0].Visitors.Add(visitor);
            Holder.HasLeftHouse = true;
        }
    }
}