using GameLogic.Attributes;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Extensions;
using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    [AbilityPriority(APriority.B128)]
    public class ZombieAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public ZombieAbility(IExecutor holder, ITarget target)
        {
            Holder = holder;
            Targets = new[] { target };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            //Only one ressurect per night
            IEnumerable<ZombieAbility> zombieAbilities = order.OfType<ZombieAbility>();

            int r = Random.Shared.Next(zombieAbilities.Count());
            ZombieAbility desired = zombieAbilities.ElementAt(r);

            order = order.Except(zombieAbilities).ToList();
            order.PutByPriority(desired);
        }

        public void SendVisitor()
        {
            RessurectVisitor visitor = new RessurectVisitor(Holder);
            Targets[0].Visitors.Add(visitor);
            Holder.HasLeftHouse = true;
        }
    }
}