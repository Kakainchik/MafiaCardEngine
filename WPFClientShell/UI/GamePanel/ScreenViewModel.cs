using System.Windows.Documents;
using System.Windows.Threading;
using WebServer.Shared.HubObjects;
using WPFClientShell.Core;
using WPFClientShell.Model.Hub;
using WPFClientShell.UI.GamePanel;

namespace WPFClientShell.UI
{
    public abstract class ScreenViewModel : BaseViewModel, IFlowStory
    {
        private readonly TimeSpan OneSecondSpan = TimeSpan.FromSeconds(1);

        protected DispatcherTimer timer;
        protected TimeSpan remainedTime;

        protected readonly LobbyDomain lobbyDomain;
        protected readonly PlayerEntity ownPlayer;

        protected Paragraph StoryParagraph => (Paragraph)StoryLog.Blocks.FirstBlock;

        public TimeSpan RemainedTime
        {
            get => remainedTime;
            set
            {
                remainedTime = value;
                OnPropertyChanged(nameof(RemainedTime));
            }
        }

        public bool IsAlive => ownPlayer.IsAlive;

        public FlowDocument StoryLog { get; set; }

        public ScreenViewModel(LobbyDomain lobbyDomain, PlayerEntity ownPlayer)
        {
            this.lobbyDomain = lobbyDomain;
            this.ownPlayer = ownPlayer;

            timer = new DispatcherTimer(OneSecondSpan,
                DispatcherPriority.DataBind,
                OnTimerTick,
                App.Current.Dispatcher);

            StoryLog = new FlowDocument(new Paragraph());
        }

        public abstract Task HandleContext(Context context);

        public virtual void StoryNewLine()
        {
            StoryParagraph.Inlines.Add(new LineBreak());
        }

        public virtual void StoryRun(Run line)
        {
            StoryParagraph.Inlines.Add(line);
        }

        public virtual void StoryClear()
        {
            StoryParagraph.Inlines.Clear();
        }

        public void StartTimer(short seconds)
        {
            RemainedTime = TimeSpan.FromSeconds(seconds);
            timer.Start();
        }

        public void StopTimer()
        {
            RemainedTime = TimeSpan.Zero;
            timer.Stop();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            RemainedTime -= OneSecondSpan;
            if(RemainedTime <= TimeSpan.Zero)
            {
                timer.Stop();
            }
        }
    }
}