using GameLogic.Attributes;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    [AbilityPriority(APriority.B2)]
    public class DriverAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public DriverAbility(IExecutor holder, ITarget target1, ITarget target2)
        {
            Holder = holder;
            Targets = new[] { target1, target2 };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            //Find all actions where their target is driver primary one
            List<IAbility> executors1to2 = order.FindAll(a =>
            {
                return a != this && a.Targets.Any(t => t == Targets[0]);
            });
            //Find all actions where their target is driver secondary one
            List<IAbility> executors2to1 = order.FindAll(a =>
            {
                return a != this && a.Targets.Any(t => t == Targets[1]);
            });

            //Swap 1 target onto 2
            executors1to2.ForEach(a =>
            {
                for(int i = 0; i < a.Targets.Length; i++)
                {
                    if(a.Targets[i] == Targets[0])
                    {
                        a.Targets[i] = Targets[1];
                    }
                }
            });

            //Swap 2 target onto 1
            executors2to1.ForEach(a =>
            {
                for(int i = 0; i < a.Targets.Length; i++)
                {
                    if(a.Targets[i] == Targets[1])
                    {
                        a.Targets[i] = Targets[0];
                    }
                }
            });
        }

        public void SendVisitor()
        {
            DriverVisitor visitor = new DriverVisitor(Holder);
            Targets[0].Visitors.Add(visitor);
            Targets[1].Visitors.Add(visitor);
            Holder.HasLeftHouse = true;
        }
    }
}