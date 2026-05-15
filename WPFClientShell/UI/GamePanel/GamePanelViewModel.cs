using GameLogic.Attributes;
using Swordfish.NET.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WebServer.Shared.Extensions;
using WebServer.Shared.GameObjects;
using WebServer.Shared.HubObjects;
using WPFClientShell.Core;
using WPFClientShell.Extensions;
using WPFClientShell.Model.Hub;

namespace WPFClientShell.UI
{
    public class GamePanelViewModel : BaseViewModel, IAsyncNavigationService
    {
        private readonly LobbyDomain lobbyDomain;

        private ConcurrentObservableDictionary<ulong, PlayerEntity> allPlayers;
        private PlayerEntity ownPlayer;
        private ScreenViewModel screen;
        private StageType stageType;
        private Visibility aliveChatVisibility;
        private Visibility deadChatVisibility;
        private bool isAliveChatEnabled;
        private bool isDeadChatEnabled;
        private string aliveChatMessage = string.Empty;
        private string deadChatMessage = string.Empty;
        private int dayNumber = 0;

        public ConcurrentObservableDictionary<ulong, PlayerEntity> AllPlayers
        {
            get => allPlayers;
            set
            {
                allPlayers = value;
                OnPropertyChanged(nameof(AllPlayers));
            }
        }

        public PlayerEntity OwnPlayer
        {
            get => ownPlayer;
            set
            {
                ownPlayer = value;
                OnPropertyChanged(nameof(OwnPlayer));
            }
        }

        public ScreenViewModel Screen
        {
            get => screen;
            set
            {
                screen = value;
                OnPropertyChanged(nameof(Screen));
            }
        }

        public Visibility AliveChatVisibility
        {
            get => aliveChatVisibility;
            set
            {
                aliveChatVisibility = value;
                OnPropertyChanged(nameof(AliveChatVisibility));
            }
        }

        public Visibility DeadChatVisibility
        {
            get => deadChatVisibility;
            set
            {
                deadChatVisibility = value;
                OnPropertyChanged(nameof(DeadChatVisibility));
            }
        }

        public bool IsAliveChatEnabled
        {
            get => isAliveChatEnabled;
            set
            {
                isAliveChatEnabled = value;
                OnPropertyChanged(nameof(IsAliveChatEnabled));
            }
        }

        public bool IsDeadChatEnabled
        {
            get => isDeadChatEnabled;
            set
            {
                isDeadChatEnabled = value;
                OnPropertyChanged(nameof(IsDeadChatEnabled));
            }
        }

        public string AliveChatMessage
        {
            get => aliveChatMessage;
            set
            {
                aliveChatMessage = value;
                OnPropertyChanged(nameof(AliveChatMessage));
            }
        }

        public string DeadChatMessage
        {
            get => deadChatMessage;
            set
            {
                deadChatMessage = value;
                OnPropertyChanged(nameof(DeadChatMessage));
            }
        }

        public ObservableCollection<ChatMessage> AliveChatLines { get; set; }
        public ObservableCollection<ChatMessage> DeadChatLines { get; set; }

        public ICommand DropAliveMessageCommand { get; set; }
        public ICommand DropDeadMessageCommand { get; set; }

        public GamePanelViewModel(LobbyDomain lobbyDomain)
        {
            this.lobbyDomain = lobbyDomain;
            allPlayers = new ConcurrentObservableDictionary<ulong, PlayerEntity>();
            ownPlayer = new PlayerEntity(default, string.Empty, default, default, default);
            screen = new IntroScreenViewModel(lobbyDomain, ownPlayer);
            stageType = StageType.INTRO;
            aliveChatVisibility = Visibility.Collapsed;
            deadChatVisibility = Visibility.Collapsed;

            AliveChatLines = new ObservableCollection<ChatMessage>();
            DeadChatLines = new ObservableCollection<ChatMessage>();

            DropAliveMessageCommand = new RelayCommand(OnDropAliveMessage);
            DropDeadMessageCommand = new RelayCommand(OnDropDeadMessage);
        }

        public async Task InitializeAsync()
        {
            ServerRequestContext request = new ServerRequestContext
            {
                RequestFor = ServerRequestType.FOR_PLAYERS_DATA
            };
            InitialPlayerDataContext ownResponse = await lobbyDomain.SendServerRequestAsync<InitialPlayerDataContext>(request);

            AllPlayers.Clear();
            foreach(InitialPlayerDataContext.PlayerInstance p in ownResponse.AllPlayers)
            {
                AllPlayers[p.Id] = new PlayerEntity(p.Id,
                        p.Nickname,
                        p.NColor.ConvertToColor(),
                        isAlive: true,
                        role: RoleVisual.CITIZEN);
            }

            OwnPlayer = AllPlayers[ownResponse.Presenter.Receiver];
            OwnPlayer.Role = ownResponse.OwnRole.MapRole();

            PlayerLoadedContext loaded = new PlayerLoadedContext();
            await lobbyDomain.SendContextAsync(loaded);

            lobbyDomain.ContextReceived += LobbyDomain_ContextReceived;
            lobbyDomain.ChatContextReceived += LobbyDomain_ChatContextReceived;
        }

        private async void OnDropAliveMessage(object? obj)
        {
            if(!string.IsNullOrWhiteSpace(AliveChatMessage))
            {
                ChatScope scope;
                if(stageType == StageType.DAY)
                {
                    //During the day cycle put all messages to the general scope
                    scope = ChatScope.GENERAL_ALIVE;
                }
                else
                {
                    ChatScopeAttribute[] attrs = OwnPlayer.Role.GetChatScopes();
                    ChatScopeAttribute? writeScope = attrs.FirstOrDefault(a => a.CanWrite
                        && a.Scope != ChatScope.DEAD);
                    if(writeScope is null)
                    {
                        return;
                    }

                    scope = writeScope.Scope;
                }
                

                ScopedChatContext scc = new ScopedChatContext
                {
                    Who = ownPlayer.Nickname,
                    Message = AliveChatMessage,
                    Scope = scope
                };

                await lobbyDomain.SendChatContextAsync(scc);

                ChatMessage msg = new ChatMessage(scc.Who, scc.Message, TimeOnly.FromDateTime(DateTime.Now));
                DeadChatLines.Add(msg);
            }
        }

        private async void OnDropDeadMessage(object? obj)
        {
            if(!string.IsNullOrWhiteSpace(DeadChatMessage))
            {
                ScopedChatContext scc = new ScopedChatContext
                {
                    Who = ownPlayer.Nickname,
                    Message = DeadChatMessage,
                    Scope = ChatScope.DEAD
                };

                await lobbyDomain.SendChatContextAsync(scc);

                ChatMessage msg = new ChatMessage(scc.Who, scc.Message, TimeOnly.FromDateTime(DateTime.Now));
                DeadChatLines.Add(msg);
            }
        }

        private void ChangeCycle(StageType cycle)
        {
            stageType = cycle;
            AliveChatLines.Clear();
            DeadChatLines.Clear();

            switch(cycle)
            {
                case StageType.LOADING:
                {
                    break;
                }
                case StageType.INTRO:
                {
                    Screen = new IntroScreenViewModel(lobbyDomain, OwnPlayer);
                    break;
                }
                case StageType.DAY:
                {
                    ActivateDayChats();
                    Screen = new DayScreenViewModel(lobbyDomain, OwnPlayer, allPlayers.AsReadOnly(), dayNumber++);
                    break;
                }
                case StageType.LYNCH:
                {
                    Screen = new LynchScreenViewModel(lobbyDomain, OwnPlayer);
                    AliveChatVisibility = Visibility.Collapsed;
                    DeadChatVisibility = Visibility.Collapsed;
                    break;
                }
                case StageType.NIGHT:
                {
                    Screen = new NightScreenViewModel(lobbyDomain, OwnPlayer, allPlayers);
                    ActivateNightChats();
                    break;
                }
                case StageType.MORNING:
                {
                    Screen = new MorningScreenViewModel(lobbyDomain, OwnPlayer, allPlayers.AsReadOnly());
                    AliveChatVisibility = Visibility.Collapsed;
                    DeadChatVisibility = Visibility.Collapsed;
                    break;
                }
                case StageType.WIN:
                {
                    break;
                }
            }
        }

        private void ActivateDayChats()
        {
            //During a day the alive chat is always visible
            AliveChatVisibility = Visibility.Visible;

            if(!OwnPlayer.IsAlive)
            {
                //Dead player sees both chats
                DeadChatVisibility = Visibility.Visible;
                IsAliveChatEnabled = false;
                IsDeadChatEnabled = true;
                return;
            }

            IsAliveChatEnabled = true;

            ChatScopeAttribute[] scopes = OwnPlayer.Role.GetChatScopes();
            if(scopes.Any(s => s.Scope == ChatScope.DEAD))
            {
                //Alive player with this ability can see the chat but write
                DeadChatVisibility = Visibility.Visible;
                IsDeadChatEnabled = false;
            }
            else
            {
                //The rest roles have only day chat
                DeadChatVisibility = Visibility.Collapsed;
            }
        }

        private void ActivateNightChats()
        {
            if(!OwnPlayer.IsAlive)
            {
                //Dead player sees only secondary chat
                AliveChatVisibility = Visibility.Collapsed;
                DeadChatVisibility = Visibility.Visible;
                IsDeadChatEnabled = true;
                return;
            }

            //Default roles do not see any chat
            AliveChatVisibility = Visibility.Collapsed;
            DeadChatVisibility = Visibility.Collapsed;

            ChatScopeAttribute[] scopes = OwnPlayer.Role.GetChatScopes();
            foreach(ChatScopeAttribute s in scopes)
            {
                if(s.Scope == ChatScope.DEAD)
                {
                    DeadChatVisibility = Visibility.Visible;
                    IsDeadChatEnabled = s.CanWrite;
                }
                else
                {
                    AliveChatVisibility = Visibility.Visible;
                    IsAliveChatEnabled = s.CanWrite;
                }
            }
        }

        private async void LobbyDomain_ContextReceived(Context context)
        {
            switch(context)
            {
                case CycleStateContext csc:
                {
                    //Update own data every cycle
                    OwnPlayer.IsAlive = csc.IsAlive;
                    OwnPlayer.Role = csc.Role.MapRole();
                    ChangeCycle(csc.Cycle);
                    break;
                }
                case TimerContext tc:
                {
                    if(tc.ToStart)
                    {
                        Screen.StartTimer(tc.Seconds);
                    }
                    else
                    {
                        Screen.StopTimer();
                    }
                    break;
                }
                default:
                {
                    await Screen.HandleContext(context);
                    break;
                }
            }
        }

        private void LobbyDomain_ChatContextReceived(ChatContext context)
        {
            ChatMessage msg = new ChatMessage(context.Who, context.Message, TimeOnly.FromDateTime(DateTime.Now));
            ChatScopeAttribute[] scopes = OwnPlayer.Role.GetChatScopes();

            if(context is ScopedChatContext scc)
            {
                if(scc.Scope == ChatScope.GENERAL_ALIVE)
                {
                    AliveChatLines.Add(msg);
                }
                else if(scc.Scope == ChatScope.DEAD)
                {
                    DeadChatLines.Add(msg);
                }
                else if(scopes.Any(s => s.Scope == scc.Scope))
                {
                    AliveChatLines.Add(msg);
                }
            }
        }
    }
}