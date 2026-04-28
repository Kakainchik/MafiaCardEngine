using System.Timers;
using WebServer.Hubs;
using WebServer.Shared.HubObjects;
using Timer = System.Timers.Timer;

namespace WebServer.Model.Game
{
    public class IntroStage
    {
        private const int STEP_INTERVAL = 6;

        private readonly Timer timer;
        private readonly int roomId;
        private readonly IHubCommand sendAllCommand;

        public IntroStage(int roomId, IHubCommand sendAllCommand)
        {
            this.roomId = roomId;
            this.sendAllCommand = sendAllCommand;
            TimeSpan span = TimeSpan.FromSeconds(STEP_INTERVAL);
            timer = new Timer(span);
        }

        public event EventHandler? IntroEnded;

        public void Run()
        {
            timer.Start();
            timer.Elapsed += Start;
        }

        private void Start(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= Start;

            IntroStepContext context = new IntroStepContext
            {
                Step = IntroStepContext.IntroStep.START,
                Presenter = LobbyHubConstants.ServerPresenter(roomId)
            };
            sendAllCommand.Execute(context);

            timer.Elapsed += Middle;
            timer.Start();
        }

        private void Middle(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= Middle;

            IntroStepContext context = new IntroStepContext
            {
                Step = IntroStepContext.IntroStep.MIDDLE,
                Presenter = LobbyHubConstants.ServerPresenter(roomId)
            };
            sendAllCommand.Execute(context);

            timer.Elapsed += End;
            timer.Start();
        }

        private void End(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= End;

            IntroStepContext context = new IntroStepContext
            {
                Step = IntroStepContext.IntroStep.END,
                Presenter = LobbyHubConstants.ServerPresenter(roomId)
            };
            sendAllCommand.Execute(context);

            timer.Elapsed += Tip;
            timer.Start();
        }

        private void Tip(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= Tip;

            IntroStepContext context = new IntroStepContext
            {
                Step = IntroStepContext.IntroStep.TIP,
                Presenter = LobbyHubConstants.ServerPresenter(roomId)
            };
            sendAllCommand.Execute(context);

            timer.Elapsed += FinishIntro;
            timer.Start();
        }

        private void FinishIntro(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= FinishIntro;
            timer.Dispose();

            IntroEnded?.Invoke(this, EventArgs.Empty);
        }
    }
}