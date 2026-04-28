using GameLogic.Attributes;
using GameLogic.Cycles.Night.Visitors;
using GameLogic.Extensions;
using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    [AbilityPriority(APriority.B1)]
    public class WitchAbility : IAbility
    {
        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public WitchAbility(IExecutor holder, ITarget exTarget, ITarget target)
        {
            Holder = holder;
            Targets = new[] { exTarget, target };
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            //Change targets for all who witches control
            //Find if such an executor exists in the order
            IAbility? exAbility = order.Find(a => a.Holder == Targets[0]);

            //Check if the exTarget is able to have any ability, otherwise do nothing
            if(Targets[0] is IExecutor executor)
            {
                if(executor.GetExecutorType() == ExecutorType.TARGET)
                {
                    if(exAbility is not null)
                    {
                        exAbility.Targets[0] = Targets[1];
                    }
                    else
                    {
                        //No ability found in order, create new one
                        int selectAbility = Random.Shared.Next(1, executor.GetNumberAbilities());
                        exAbility = executor.GetAbility(selectAbility, Targets[1]);
                        APriority exPriority = exAbility.GetAbilityPriority();

                        order.PutByPriority(exAbility);
                    }
                }
            }
        }

        public void SendVisitor()
        {
            WitchVisitor visitor = new WitchVisitor(Holder);
            Targets[0].Visitors.Add(visitor);

            //Stay at home
            Holder.HasLeftHouse = false;
        }
    }
}