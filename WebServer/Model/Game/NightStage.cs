using GameLogic;
using WebServer.Hubs;
using GameLogic.Cycles.Night;
using Timer = System.Timers.Timer;
using System.Timers;
using WebServer.Shared.HubObjects;
using GameLogic.Interfaces;
using WebServer.Shared.GameObjects.Night;

namespace WebServer.Model.Game
{
    public class NightStage
    {
        private const short STEP_INTERVAL = 6;
        private const short PICK_ACTION_TIME = 60;
        private const short WAIT_FOR_ACTIONS_TIME = 6;

        private readonly Timer timer;
        private readonly NightCycle night;
        private readonly IReadOnlyList<Player> players;
        private readonly IHubCommand sendAllCommand;
        private readonly IHubIdCommand sendUserCommand;
        private readonly int roomId;

        private ISet<ulong> executorsSet;

        public NightStage(NightCycle night,
            IReadOnlyList<Player> players,
            IHubCommand sendAllCommand,
            IHubIdCommand sendUserCommand,
            int roomId)
        {
            this.night = night;
            this.players = players;
            this.sendAllCommand = sendAllCommand;
            this.sendUserCommand = sendUserCommand;
            this.roomId = roomId;

            executorsSet = new HashSet<ulong>();

            TimeSpan span = TimeSpan.FromSeconds(STEP_INTERVAL);
            timer = new Timer(span);
        }

        public event EventHandler? NightEnded;

        public void Run()
        {
            timer.Elapsed += StartReminder;
            timer.Start();
        }

        public void ConfirmFlag(ulong executorId, int ability = 1, params ulong[] targetIds)
        {
            IRoleOwner executor = players.Single(p => p.Id == executorId);

            if(executorsSet.Contains(executorId))
            {
                //Once an action added it is saved
                return;
            }
            else
            {
                executorsSet.Add(executorId);
            }

            IRoleOwner[] targets = new IRoleOwner[targetIds.Length];
            for(int i = 0; i < targets.Length; i++)
            {
                targets[i] = players.Single(p => p.Id == targetIds[i]);
            }

            night.ConfirmAction(executor, ability, targets);
        }

        private void StartReminder(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= StartReminder;

            NightStepContext context = new NightStepContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Step = NightStepContext.NightStep.START_REMINDER
            };
            sendAllCommand.Execute(context);

            timer.Elapsed += AllowSelection;
            timer.Start();
        }

        private void AllowSelection(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= AllowSelection;

            NightStepContext context = new NightStepContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Step = NightStepContext.NightStep.ALLOW_SELECTION
            };
            sendAllCommand.Execute(context);

            TimerContext tc = new TimerContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Seconds = PICK_ACTION_TIME,
                ToStart = true
            };
            sendAllCommand.Execute(tc);

            timer.Interval = TimeSpan.FromSeconds(PICK_ACTION_TIME).TotalMilliseconds;
            timer.Elapsed += DisallowSelection;
            timer.Start();
        }

        private void DisallowSelection(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= DisallowSelection;

            NightStepContext context = new NightStepContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Step = NightStepContext.NightStep.DISALLOW_SELECTION
            };
            sendAllCommand.Execute(context);
            
            TimerContext tc = new TimerContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Seconds = 0,
                ToStart = false
            };
            sendAllCommand.Execute(tc);

            timer.Interval = TimeSpan.FromSeconds(WAIT_FOR_ACTIONS_TIME).TotalMilliseconds;
            timer.Elapsed += SendLogs;
            timer.Start();
        }

        private void SendLogs(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= SendLogs;

            IReadOnlyCollection<ActionLog> logs = night.ExecuteActions();
            NightLogFacade facade = new NightLogFacade(logs, roomId);
            Queue<Tuple<NightActionLogContext, short>> contextQueue = facade.ZipContext();
            
            while(contextQueue.Count > 0)
            {
                Tuple<NightActionLogContext, short> tuple = contextQueue.Dequeue();

                TimeSpan span = TimeSpan.FromSeconds(tuple.Item2);
                Thread.Sleep(span);

                NightActionLogContext log = tuple.Item1;

                if(log.Presenter.Receiver != 0L)
                {
                    //Send to exact player
                    sendUserCommand.Execute(log, log.Presenter.Receiver);
                }
                else if(log.Presenter.Except != 0L)
                {
                    //Send all except one
                    for(int i = 0; i < players.Count;  i++)
                    {
                        if(players[i].Id == log.Presenter.Except)
                        {
                            continue;
                        }

                        sendUserCommand.Execute(log, players[i].Id);
                    }
                }
                else
                {
                    //Send all
                    sendAllCommand.Execute(log);
                }
            }

            timer.Elapsed += End;
            timer.Start();
        }

        private void End(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= End;

            NightStepContext context = new NightStepContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Step = NightStepContext.NightStep.END
            };
            sendAllCommand.Execute(context);

            NightEnded?.Invoke(this, EventArgs.Empty);
        }
    }
}