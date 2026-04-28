using GameLogic.Cycles.Night.Abilities;
using GameLogic.Extensions;
using GameLogic.Interfaces;
using GameLogic.Roles;
using System.Collections.ObjectModel;

namespace GameLogic.Cycles.Night
{
    public class NightCycle
    {
        private bool actionsComfirmed = false;
        private List<IAbility> abilities = new List<IAbility>();

        protected IList<Player> alivePlayers;

        public NightCycle(IList<Player> alivePlayers)
        {
            this.alivePlayers = alivePlayers;
        }

        public void ConfirmAction(IRoleOwner executor, params IRoleOwner[] targets)
        {
            //There is only one ability
            ConfirmAction(executor, 1, targets);
        }

        public void ConfirmAction(IRoleOwner executor, int selectAbility, params IRoleOwner[] targets)
        {
            if(actionsComfirmed) return;

            if(executor.Role is not IExecutor)
            {
                throw new InvalidCastException($"The role [{executor.Role}] is not an executor.");
            }

            if(!executor.Role.IsAlive || targets.Any(t => !t.Role.IsAlive))
            {
                throw new ArgumentException("Dead players are forbidden to have actions.");
            }

            Role[] targetRoles = targets.Select(x => x.Role).ToArray();
            IAbility ability = ((IExecutor)executor.Role).GetAbility(selectAbility, targetRoles);
            abilities.Add(ability);
        }

        public IReadOnlyCollection<ActionLog> ExecuteActions()
        {
            if(actionsComfirmed)
            {
                return ReadOnlyCollection<ActionLog>.Empty;
            }

            actionsComfirmed = true;
            abilities.Sort(AbilityComparer.Instance);

            //Run conditions
            IEnumerable<Role> aliveRoles = alivePlayers.Select(a => a.Role);
            for(int i = 0; i < abilities.Count; i++)
            {
                abilities[i].Condition(ref abilities, aliveRoles);
            }

            //Conditions been set, send visitors
            for(int i = 0; i < abilities.Count; i++)
            {
                abilities[i].SendVisitor();
            }

            //Visitor been set, accept them
            IList<ActionLog> logs = new List<ActionLog>();
            for(int i = 0; i < alivePlayers.Count; i++)
            {
                foreach(ActionLog log in alivePlayers[i].Role.AcceptVisitors())
                {
                    logs.Add(log);
                }
            }

            //All actions has done, clear visitors
            for(int i = 0; i < alivePlayers.Count; i++)
            {
                alivePlayers[i].Role.Visitors.Clear();
            }

            return new ReadOnlyCollection<ActionLog>(logs);
        }
    }
}