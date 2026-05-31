using GameLogic.Cycles;
using GameLogic.Cycles.Night;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Extensions;
using GameLogic.Interfaces;
using GameLogic.Roles;
using System.Collections.ObjectModel;

namespace GameLogic.ParanoiaCorp.Cycles
{
    public class OvertimeCycle : ICycle
    {
        private bool actionsComfirmed = false;
        private GameEngine engine;
        private List<IAbility> abilities = new List<IAbility>();
        private EndGameRoundHistory endGameRoundHistory;
        private Queue<EndGameOvernightHistory> endGameOvernightHistory;

        protected IDictionary<Player, bool> alivePlayers;

        public OvertimeCycle(GameEngine engine)
        {
            this.engine = engine;
            alivePlayers = engine.AlivePlayers.ToDictionary(p => p, p => false);
            endGameRoundHistory = this.engine.History.Peek();
            endGameOvernightHistory = new Queue<EndGameOvernightHistory>();
            endGameRoundHistory.OvertimeActions = endGameOvernightHistory;
        }

        public bool CanFinish()
        {
            //The cycle can only be finished when all alive players have confirmed their actions.
            return alivePlayers.Values.All(ready => ready);
        }

        public ICycle NextCycle()
        {
            IReadOnlyCollection<ActionLog> logs = ExecuteActions();
            return new MorningCycle(engine, logs);
        }

        public void ConfirmAction(IRoleOwner executor, params IRoleOwner[] targets)
        {
            //There is only one ability
            ConfirmAction(executor, 1, targets);
        }

        public void ConfirmAction(IRoleOwner executor, int selectAbility, params IRoleOwner[] targets)
        {
            //Actions can only be confirmed once, and the order of actions is determined by the first time they are confirmed.
            //So if actions has been comfirmed, no more action can be added.
            if(actionsComfirmed) return;

            if(executor.Role is not IExecutor)
            {
                throw new InvalidCastException($"The role [{executor.Role}] is not an executor.");
            }

            if(!executor.Role.IsAlive || targets.Any(t => !t.Role.IsAlive))
            {
                throw new ArgumentException("Fired players are forbidden to have actions.");
            }

            Role[] targetRoles = targets.Select(x => x.Role).ToArray();
            IAbility ability = ((IExecutor)executor.Role).GetAbility(selectAbility, targetRoles);
            abilities.Add(ability);

            EndGameOvernightHistory overnightRecord = new EndGameOvernightHistory()
            {
                Executor = executor.Id,
                ExecutorRoleType = executor.Role.GetType(),
                Targets = targets.Select(t => t.Id).ToArray()
            };
            endGameOvernightHistory.Enqueue(overnightRecord);
        }

        public void SetPlayerReady(Player player)
        {
            if(alivePlayers.ContainsKey(player))
            {
                alivePlayers[player] = true;
            }
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
            IEnumerable<Role> aliveRoles = engine.AlivePlayers.Select(a => a.Role);
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
            for(int i = 0; i < engine.AlivePlayers.Count; i++)
            {
                foreach(ActionLog log in engine.AlivePlayers[i].Role.AcceptVisitors())
                {
                    logs.Add(log);
                }
            }

            //All actions has done, record and clear visitors
            for(int i = 0; i < engine.Players.Length; i++)
            {
                engine.Players[i].Role.Visitors.Clear();
            }

            return new ReadOnlyCollection<ActionLog>(logs);
        }
    }
}