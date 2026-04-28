using GameLogic.Cycles.Morning;
using GameLogic.Interfaces;
using System.Timers;
using WebServer.Hubs;
using WebServer.Shared.Extensions;
using WebServer.Shared.GameObjects;
using WebServer.Shared.HubObjects;
using Timer = System.Timers.Timer;

namespace WebServer.Model.Game
{
    public class MorningStage
    {
        private const short STEP_INTERVAL = 5;

        private readonly Timer timer;
        private readonly MorningCycle morning;
        private readonly IHubCommand sendAllCommand;
        private readonly int roomId;

        public MorningStage(MorningCycle morning, IHubCommand sendAllCommand, int roomId)
        {
            this.morning = morning;
            this.sendAllCommand = sendAllCommand;
            this.roomId = roomId;

            TimeSpan span = new TimeSpan(STEP_INTERVAL);
            timer = new Timer(span);
        }

        public event EventHandler? MorningEnded;

        public void Run()
        {
            timer.Elapsed += SendVictim;
            timer.Start();
        }

        private void SendVictim(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= SendVictim;

            ISet<DeathReason> notes = morning.NoteDeaths();
            foreach(DeathReason note in notes)
            {
                bool isSuicide = note.Reason == note.Reason;
                MorningVictimContext context = new MorningVictimContext
                {
                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                    VictimId = note.Dead.Id,
                    VictimRole = note.Dead.Role.IntoSignature(),
                    Reason = isSuicide ? MorningVictimContext.DeathReason.SUICIDE : FindReason(note.Reason),
                    LastWill = note.Dead.LastWill
                };

                sendAllCommand.Execute(context);

                TimeSpan span = TimeSpan.FromSeconds(STEP_INTERVAL);
                Thread.Sleep(span);
            }

            timer.Elapsed += End;
            timer.Start();
        }

        private void End(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= End;

            MorningEnded?.Invoke(this, EventArgs.Empty);
        }

        private MorningVictimContext.DeathReason FindReason(IRoleOwner reason)
        {
            switch(reason.Role.IntoSignature())
            {
                case RoleSignature.MAFIA:
                {
                    return MorningVictimContext.DeathReason.MAFIA;
                }
                case RoleSignature.SERIAL_KILLER:
                {
                    return MorningVictimContext.DeathReason.SERIAL_KILLER;
                }
                case RoleSignature.VIGILANTE:
                {
                    return MorningVictimContext.DeathReason.VIGILANTE;
                }
                case RoleSignature.DRIVER:
                {
                    return MorningVictimContext.DeathReason.DRIVER;
                }
                case RoleSignature.TERRORIST:
                {
                    return MorningVictimContext.DeathReason.TERRORIST;
                }
                default:
                {
                    return MorningVictimContext.DeathReason.SUICIDE;
                }
            }
        }
    }
}