using Swordfish.NET.Collections;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using WebServer.Shared.HubObjects;
using WPFClientShell.Core;
using WPFClientShell.Model.Hub;
using WPFClientShell.Resources.GameStory;

namespace WPFClientShell.UI
{
    public class DayScreenViewModel : ScreenViewModel
    {
        private readonly int dayNumber;

        private ConcurrentObservableDictionary<ulong, PlayerVotableDecorator> allPlayers;
        private bool isBallotActive;
        private int votesForNonLynch;

        public ConcurrentObservableDictionary<ulong, PlayerVotableDecorator> AllPlayers
        {
            get => allPlayers;
            set
            {
                allPlayers = value;
                OnPropertyChanged(nameof(AllPlayers));
            }
        }

        public bool IsBallotActive
        {
            get => isBallotActive;
            set
            {
                isBallotActive = value;
                OnPropertyChanged(nameof(IsBallotActive));
            }
        }

        public int VotesForNonLynch
        {
            get => votesForNonLynch;
            set
            {
                votesForNonLynch = value;
                OnPropertyChanged(nameof(VotesForNonLynch));
            }
        }

        public bool IsOwnAlive => ownPlayer.IsAlive;

        public ICommand VoteClickCommand { get; set; }

        public DayScreenViewModel(LobbyDomain lobbyDomain,
            PlayerEntity ownPlayer,
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers,
            int dayNumber)
            : base(lobbyDomain, ownPlayer)
        {
            this.dayNumber = dayNumber;
            this.allPlayers = new ConcurrentObservableDictionary<ulong, PlayerVotableDecorator>();

            foreach(KeyValuePair<ulong, PlayerEntity> p in allPlayers)
            {
                this.allPlayers[p.Key] = new PlayerVotableDecorator(p.Value.Id,
                    p.Value.Nickname,
                    p.Value.NColor,
                    p.Value.IsAlive,
                    p.Value.Role);
            }

            VoteClickCommand = new RelayCommand(OnVoteClick);
        }

        public override Task HandleContext(Context context)
        {
            switch(context)
            {
                case DayPlayerDataContext dpdc:
                {
                    //Update public players info for this day
                    for(int i = 0; i < dpdc.DayPlayers.Length; i++)
                    {
                        AllPlayers[dpdc.DayPlayers[i].Id].IsAlive = dpdc.DayPlayers[i].IsAlive;
                    }
                    break;
                }
                case DayStepContext dsc:
                {
                    ProcessStep(dsc.Step);
                    break;
                }
                case ReceiveVoteContext rvc:
                {
                    PlayerVotableDecorator voter = allPlayers[rvc.Voter];
                    if(rvc.CurrentTarget is null)
                    {
                        //Clear voter's target
                        voter.VoteForId = null;
                        voter.VoteFor = null;
                        voter.TargetColor = null;
                    }
                    else if(rvc.CurrentTarget.TargetId == 0L)
                    {
                        //Set Non-Lynch as voted
                        voter.VoteForId = 0L;
                        voter.VoteFor = null;
                        voter.TargetColor = null;

                        VotesForNonLynch = rvc.CurrentTarget.VotesNumber;
                    }
                    else
                    {
                        PlayerVotableDecorator current = allPlayers[rvc.CurrentTarget.TargetId];
                        current.OwnVotes = rvc.CurrentTarget.VotesNumber;

                        //Set voter's target
                        voter.VoteForId = rvc.CurrentTarget.TargetId;
                        voter.VoteFor = current.Nickname;
                        voter.TargetColor = current.NColor;
                    }

                    if(rvc.PreviousTarget is not null)
                    {
                        //Set votes number for previous target
                        PlayerVotableDecorator previous = allPlayers[rvc.PreviousTarget.TargetId];
                        previous.OwnVotes = rvc.PreviousTarget.VotesNumber;
                    }

                    break;
                }
                case WarningVoteContext wvc:
                {
                    //Clear panel
                    StoryClear();

                    if(!wvc.VotedId.HasValue)
                    {
                        break;
                    }
                    else if(wvc.VotedId.Value == 0L)
                    {
                        //Non-lynch been voted
                        StoryRun(new Run(DayResources.AttentionVotingForNonLynch));
                    }
                    else
                    {
                        //Find the voted player nickname
                        PlayerVotableDecorator decorator = AllPlayers[wvc.VotedId.Value];
                        StoryRun(new Run(DayResources.AttentionVotingFor));
                        StoryRun(new Run(decorator.Nickname)
                        {
                            Foreground = new SolidColorBrush(decorator.NColor)
                        });
                    }
                    break;
                }
            }

            return Task.CompletedTask;
        }

        private async void OnVoteClick(object? obj)
        {
            ulong? votableId = (ulong?)obj!;

            if(votableId == ownPlayer.Id)
            {
                //Cannot vote for ourself
                return;
            }

            if(votableId != 0L && !AllPlayers[votableId.Value].IsAlive)
            {
                //Cannot vote for dead players
                return;
            }

            ulong? previousId = AllPlayers[ownPlayer.Id].VoteForId;
            if(votableId == previousId)
            {
                //Click on the voted player by us - unvote
                votableId = null;
            }

            SendVoteContext context = new SendVoteContext
            {
                TargetId = votableId
            };

            await lobbyDomain.SendContextAsync(context);
        }

        private void ProcessStep(DayStepContext.DayStep step)
        {
            switch(step)
            {
                case DayStepContext.DayStep.START_DAY:
                {
                    StoryRun(new Run()
                    {
                        Text = string.Format(DayResources.DayNumber, dayNumber)
                    });
                    StoryNewLine();
                    StoryRun(new Run(DayResources.MeetingStarted));
                    break;
                }
                case DayStepContext.DayStep.START_BALLOT:
                {
                    //Clear panel
                    StoryClear();
                    StoryRun(new Run(DayResources.BallotStarted));
                    break;
                }
                case DayStepContext.DayStep.END_BALLOT:
                {
                    //Clear panel
                    StoryClear();
                    StoryRun(new Run(DayResources.BallotEnded));
                    break;
                }
                case DayStepContext.DayStep.FIRST_DAY_CASE:
                {
                    StoryNewLine();
                    StoryRun(new Run(DayResources.FirstDayNoLynch));
                    break;
                }
            }
        }
    }
}