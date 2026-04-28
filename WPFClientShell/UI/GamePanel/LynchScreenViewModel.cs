using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using WebServer.Shared.HubObjects;
using WPFClientShell.Extensions;
using WPFClientShell.Model.Hub;
using WPFClientShell.Resources.GameStory;

namespace WPFClientShell.UI
{
    public class LynchScreenViewModel : ScreenViewModel
    {
        private bool isMessageBoxVisible;
        private string? lastMessageText;
        private PlayerEntity? electedPlayer;

        public bool IsMessageBoxVisible
        {
            get => isMessageBoxVisible;
            set
            {
                isMessageBoxVisible = value;
                OnPropertyChanged(nameof(IsMessageBoxVisible));
            }
        }

        public string? LastMessageText
        {
            get => lastMessageText;
            set
            {
                lastMessageText = value;
                OnPropertyChanged(nameof(LastMessageText));
            }
        }

        public PlayerEntity? ElectedPlayer
        {
            get => electedPlayer;
            set
            {
                electedPlayer = value;
                OnPropertyChanged(nameof(ElectedPlayer));
            }
        }

        public LynchScreenViewModel(LobbyDomain lobbyDomain, PlayerEntity ownPlayer)
            : base(lobbyDomain, ownPlayer)
        {

        }

        public override async Task HandleContext(Context context)
        {
            switch(context)
            {
                case LynchPlayerContext lpc:
                {
                    ElectedPlayer = new PlayerEntity(lpc.PlayerId,
                        lpc.Nickname,
                        lpc.NColor.ConvertToColor(),
                        isAlive: true,
                        lpc.Role.MapRole());
                    break;
                }
                case LynchStepContext lsc:
                {
                    await ProcessStep(lsc.Step);
                    break;
                }
                case ReceiveLastMessageContext rlmc:
                {
                    //Clear panel
                    StoryClear();
                    StoryRun(new Run()
                    {
                        Text = electedPlayer!.Nickname,
                        Foreground = new SolidColorBrush(electedPlayer.NColor)
                    });
                    StoryRun(new Run()
                    {
                        Text = "> "
                    });
                    StoryRun(new Run()
                    {
                        Text = rlmc.Message,
                        FontStyle = FontStyles.Italic
                    });
                    break;
                }
            }
        }

        private async Task ProcessStep(LynchStepContext.LynchStep step)
        {
            switch(step)
            {
                case LynchStepContext.LynchStep.QUESTION:
                {
                    StoryRun(new Run(LynchResources.AnyMessage));
                    if(ElectedPlayer!.Id == ownPlayer.Id)
                    {
                        //If we got elected
                        IsMessageBoxVisible = true;
                    }
                    break;
                }
                case LynchStepContext.LynchStep.LAST_MESSAGE:
                {
                    IsMessageBoxVisible = false;

                    if(!string.IsNullOrEmpty(LastMessageText))
                    {
                        SendLastMessageContext msg = new SendLastMessageContext
                        {
                            Message = LastMessageText
                        };

                        //Run and fire
                        await lobbyDomain.SendContextAsync(msg);
                    }
                    break;
                }
                case LynchStepContext.LynchStep.PREPARE_EXECUTE:
                {
                    StoryNewLine();
                    StoryRun(new Run()
                    {
                        Text = LynchResources.LynchExecuted
                    });
                    break;
                }
                case LynchStepContext.LynchStep.EXECUTE:
                {
                    //Play sound
                    ElectedPlayer!.IsAlive = false;
                    break;
                }
                case LynchStepContext.LynchStep.SHOW_ROLE:
                {
                    StoryClear();
                    StoryRun(new Run()
                    {
                        Text = LynchResources.RoleWas
                    });
                    StoryRun(new Run()
                    {
                        Text = ElectedPlayer!.Role.GetLocalizedName(),
                        Foreground = ElectedPlayer.Role.GetColor()
                    });
                    break;
                }
                case LynchStepContext.LynchStep.END:
                {
                    StoryNewLine();
                    StoryRun(new Run()
                    {
                        Text = LynchResources.MeetingEnded
                    });
                    break;
                }
            }
        }
    }
}