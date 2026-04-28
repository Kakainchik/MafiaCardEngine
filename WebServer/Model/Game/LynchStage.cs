using GameLogic.Cycles.Lynch;
using WebServer.Hubs;
using WebServer.Shared.HubObjects;
using Timer = System.Timers.Timer;

namespace WebServer.Model.Game
{
    public class LynchStage
    {
        private const short STEP_INTERVAL = 5;
        private const short LAST_MESSAGE_WAIT_TIME = 15;

        private readonly Timer timer;
        private readonly LynchCycle lynch;
        private readonly IHubCommand sendAllCommand;
        private readonly IHubIdCommand sendUserCommand;
        private readonly int roomId;

        private string lastMessage;

        public LynchStage(LynchCycle lynch, IHubCommand sendAllCommand, IHubIdCommand sendUserCommand, int roomId)
        {
            this.lynch = lynch;
            this.sendAllCommand = sendAllCommand;
            this.sendUserCommand = sendUserCommand;
            this.roomId = roomId;
            lastMessage = string.Empty;

            TimeSpan span = new TimeSpan(STEP_INTERVAL);
            timer = new Timer(span);
        }

        public event EventHandler? LynchEnded;

        public void Run()
        {
            timer.Elapsed += Question;
            timer.Start();
        }

        public void ConfirmLastMessage(string message)
        {
            //Wait for any message from the executed
            lastMessage = message;
        }

        private void Question(object? sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= Question;

            LynchStepContext lsc = new LynchStepContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Step = LynchStepContext.LynchStep.QUESTION
            };
            sendAllCommand.Execute(lsc);

            timer.Elapsed += LastMessage;
            timer.Start();
        }

        private void LastMessage(object? sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= LastMessage;

            LynchStepContext context = new LynchStepContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId) with
                {
                    Receiver = lynch.Elected.Id
                },
                Step = LynchStepContext.LynchStep.LAST_MESSAGE
            };

            //Send request to retreive the last message
            sendUserCommand.Execute(context, lynch.Elected.Id);

            timer.Interval = TimeSpan.FromSeconds(LAST_MESSAGE_WAIT_TIME).TotalMicroseconds;
            timer.Elapsed += PrepareExecute;
            timer.Start();
        }

        private void PrepareExecute(object? sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= PrepareExecute;
            timer.Interval = TimeSpan.FromSeconds(STEP_INTERVAL).TotalMicroseconds;

            LynchStepContext context = new LynchStepContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Step = LynchStepContext.LynchStep.PREPARE_EXECUTE
            };
            sendAllCommand.Execute(context);

            timer.Elapsed += Execute;
            timer.Start();
        }

        private void Execute(object? sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= Execute;

            lynch.Lynch(lastMessage);

            LynchStepContext context = new LynchStepContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Step = LynchStepContext.LynchStep.EXECUTE
            };
            sendAllCommand.Execute(context);

            timer.Elapsed += ShowRole;
            timer.Start();
        }

        private void ShowRole(object? sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= ShowRole;

            LynchStepContext context = new LynchStepContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Step = LynchStepContext.LynchStep.SHOW_ROLE
            };
            sendAllCommand.Execute(context);

            timer.Elapsed += End;
            timer.Start();
        }

        private void End(object? sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= End;

            LynchStepContext context = new LynchStepContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Step = LynchStepContext.LynchStep.END
            };
            sendAllCommand.Execute(context);

            LynchEnded?.Invoke(this, EventArgs.Empty);
        }
    }
}