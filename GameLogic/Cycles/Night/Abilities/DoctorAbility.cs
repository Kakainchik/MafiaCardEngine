using GameLogic.Attributes;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    [AbilityPriority(APriority.B8)]
    public class DoctorAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public DoctorAbility(IExecutor executor, ITarget target)
        {
            Holder = executor;
            Targets = new[] { target };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            //Doctor cannot heal himself
            if(Targets[0] == Holder)
            {
                order.Remove(this);
            }

            //Just one doctor can heal per target
            //Remove another doctors' actions at the target
            IEnumerable<DoctorAbility> doctors = order.OfType<DoctorAbility>()
                .Where(d => d.Targets[0] == Targets[0]);

            DoctorAbility first = doctors.First();

            doctors = doctors.Where(d => d != first);
            order = order.Except(doctors)
                .ToList();
        }

        public void SendVisitor()
        {
            HealVisitor visitor = new HealVisitor(Holder);
            Targets[0].Visitors.Add(visitor);
            Holder.HasLeftHouse = true;
        }
    }
}