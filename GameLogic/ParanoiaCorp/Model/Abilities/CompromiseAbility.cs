using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Extensions;
using GameLogic.Interfaces;
using GameLogic.ParanoiaCorp.Model.Visitors;
using GameLogic.Roles;

namespace GameLogic.ParanoiaCorp.Model.Abilities
{
    [AbilityPriority(APriority.B8)]
    public class CompromiseAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public CompromiseAbility(IExecutor executor, ITarget target)
        {
            Holder = executor;
            Targets = new[] { target };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            IEnumerable<CompromiseAbility> compromatorsOrder = order.OfType<CompromiseAbility>();
            int count = compromatorsOrder.Count();

            if(count == 1)
            {
                return;
            }

            //Just one compromator can kill per night
            //Randomize the choise
            int r = Random.Shared.Next(count);
            CompromiseAbility desired = compromatorsOrder.ElementAt(r);

            order = order.Except(compromatorsOrder).ToList();
            order.PutByPriority(desired);
        }

        public void SendVisitor()
        {
            FireVisitor visitor = new FireVisitor(Holder);
            Targets[0].Visitors.Add(visitor);
            Holder.HasLeftHouse = true;
        }
    }
}