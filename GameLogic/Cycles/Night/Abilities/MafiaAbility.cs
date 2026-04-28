using GameLogic.Attributes;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Extensions;
using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    [AbilityPriority(APriority.B8)]
    public class MafiaAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public MafiaAbility(IExecutor executor, ITarget target)
        {
            Holder = executor;
            Targets = new[] { target };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            IEnumerable<MafiaAbility> mafiaOrder = order.OfType<MafiaAbility>();
            int count = mafiaOrder.Count();

            if(count == 1)
            {
                return;
            }

            //Just one mafia can kill per night
            //Randomize the choise
            int r = Random.Shared.Next(count);
            MafiaAbility desired = mafiaOrder.ElementAt(r);

            order = order.Except(mafiaOrder).ToList();
            order.PutByPriority(desired);
        }

        public void SendVisitor()
        {
            KillVisitor visitor = new KillVisitor(Holder);
            Targets[0].Visitors.Add(visitor);
            Holder.HasLeftHouse = true;
        }
    }
}