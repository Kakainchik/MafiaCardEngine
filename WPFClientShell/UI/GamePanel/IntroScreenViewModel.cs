using System.Windows;
using System.Windows.Documents;
using WebServer.Shared.HubObjects;
using WPFClientShell.Extensions;
using WPFClientShell.Model.Hub;
using WPFClientShell.Resources.GameStory;

namespace WPFClientShell.UI
{
    public class IntroScreenViewModel : ScreenViewModel
    {
        public IntroScreenViewModel(LobbyDomain lobbyDomain, PlayerEntity ownPlayer)
            : base(lobbyDomain, ownPlayer)
        {
            
        }

        public override Task HandleContext(Context context)
        {
            switch(context)
            {
                case IntroStepContext isc:
                {
                    ShowStepMessage(isc.Step);
                    break;
                }
            }

            return Task.CompletedTask;
        }

        private void ShowStepMessage(IntroStepContext.IntroStep step)
        {
            switch(step)
            {
                case IntroStepContext.IntroStep.START:
                {
                    StoryRun(new Run(IntroResources.Start));
                    StoryNewLine();
                    StoryNewLine();
                    break;
                }
                case IntroStepContext.IntroStep.MIDDLE:
                {
                    string text = string.Format(IntroResources.Middle);
                    StoryRun(new Run(text));
                    StoryNewLine();
                    StoryNewLine();
                    break;
                }
                case IntroStepContext.IntroStep.END:
                {
                    StoryRun(new Run(IntroResources.End));
                    break;
                }
                case IntroStepContext.IntroStep.TIP:
                {
                    StoryClear();
                    StoryRun(new Run(IntroResources.RoleIs));
                    StoryRun(new Run()
                    {
                        Text = ownPlayer.Role.GetLocalizedName(),
                        Foreground = ownPlayer.Role.GetColor(),
                        FontWeight = FontWeights.Medium
                    });
                    StoryNewLine();
                    StoryRun(new Run()
                    {
                        Text = ownPlayer.Role.GetLocilizedDescription(),
                        Foreground = ownPlayer.Role.GetColor()
                    });
                    StoryNewLine();
                    StoryRun(new Run()
                    {
                        Text = ownPlayer.Role.GetLocilizedBelonging(),
                        Foreground = ownPlayer.Role.GetColor(),
                        TextDecorations = TextDecorations.Underline
                    });
                    StoryNewLine();
                    StoryRun(new Run(IntroResources.Tip));
                    break;
                }
            }
        }
    }
}