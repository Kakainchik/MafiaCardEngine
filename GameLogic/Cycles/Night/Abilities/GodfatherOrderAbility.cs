using GameLogic.Attributes;
using GameLogic.Extensions;
using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Night.Abilities
{
    [AbilityPriority(APriority.B1)]
    public class GodfatherOrderAbility : IAbility
    {
        private readonly OrderTarget orderTarget;

        public ITarget[] Targets { get; }
        public IExecutor Holder { get; }

        public GodfatherOrderAbility(IExecutor holder, ITarget target, OrderTarget order)
        {
            Holder = holder;
            Targets = new[] { target };
            orderTarget = order;
        }

        public void Condition(ref List<IAbility> order, IEnumerable<Role> aliveRoles)
        {
            switch(orderTarget)
            {
                case OrderTarget.MAFIA:
                {
                    IEnumerable<MafiaAbility> mafiaAbilities = order.OfType<MafiaAbility>();

                    //Change target for current abilities
                    foreach(MafiaAbility m in mafiaAbilities)
                    {
                        m.Targets[0] = Targets[0];
                    }

                    //Create the action for other mafiosi
                    IEnumerable<MafiaRole> holders = mafiaAbilities.Select(a => a.Holder)
                        .Cast<MafiaRole>();
                    IEnumerable<MafiaRole> mafiaRoles = aliveRoles.OfType<MafiaRole>();
                    mafiaRoles = mafiaRoles.Except(holders);
                    foreach(MafiaRole role in mafiaRoles)
                    {
                        order.PutByPriority(role.GetAbility(1, Targets[0]));
                    }
                    break;
                }
                case OrderTarget.COUNSELOR:
                {
                    IEnumerable<InvestigateAbility> counselorAbilities = order.OfType<InvestigateAbility>();

                    //Change target for current abilities
                    foreach(InvestigateAbility m in counselorAbilities)
                    {
                        m.Targets[0] = Targets[0];
                    }

                    //Create the action for other counselors
                    IEnumerable<CounselorRole> holders = counselorAbilities.Select(a => a.Holder)
                        .Cast<CounselorRole>();
                    IEnumerable<CounselorRole> counselorRoles = aliveRoles.OfType<CounselorRole>();
                    counselorRoles = counselorRoles.Except(holders);
                    foreach(CounselorRole role in counselorRoles)
                    {
                        order.PutByPriority(role.GetAbility(1, Targets[0]));
                    }
                    break;
                }
            }
        }

        public void SendVisitor()
        {
            //No visitor needed to send
            //Stay at home
            Holder.HasLeftHouse = false;
        }

        public enum OrderTarget
        {
            MAFIA,
            COUNSELOR
        }
    }
}