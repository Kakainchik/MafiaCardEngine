using GameLogic.Attributes;
using System.Windows;
using System.Windows.Documents;
using WebServer.Shared.HubObjects;
using WPFClientShell.Extensions;
using WPFClientShell.Model.Hub;
using WPFClientShell.Resources.GameStory;

namespace WPFClientShell.UI
{
    public class MorningScreenViewModel : ScreenViewModel
    {
        private readonly IReadOnlyDictionary<ulong, PlayerEntity> allPlayers;

        private PlayerVictimDecorator? victim;

        public PlayerVictimDecorator? Victim
        {
            get => victim;
            set
            {
                victim = value;
                OnPropertyChanged(nameof(victim));

                if(victim is null)
                {
                    VictimVisibility = Visibility.Collapsed;
                }
                else
                {
                    VictimVisibility = Visibility.Visible;
                }
                OnPropertyChanged(nameof(VictimVisibility));
            }
        }

        public Visibility VictimVisibility { get; set; }

        public MorningScreenViewModel(LobbyDomain lobbyDomain,
            PlayerEntity ownPlayer,
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers)
            : base(lobbyDomain, ownPlayer)
        {
            this.allPlayers = allPlayers;
        }

        public override Task HandleContext(Context context)
        {
            switch(context)
            {
                case MorningBreakContext mbc:
                {
                    ShowBreak(mbc.Deaths);
                    StoryNewLine();
                    ShowRemainedTeam(mbc);
                    break;
                }
                case MorningVictimContext mvc:
                {
                    PlayerEntity entity = allPlayers[mvc.VictimId];

                    Victim = new PlayerVictimDecorator(entity.Id,
                        entity.Nickname,
                        entity.NColor,
                        entity.IsAlive,
                        mvc.VictimRole.MapRole(),
                        mvc.LastWill);

                    ShowReasonText(mvc.Reason);

                    if(!string.IsNullOrEmpty(mvc.LastWill))
                    {
                        ShowLastWill(mvc.LastWill);
                    }
                    
                    break;
                }
            }

            return Task.CompletedTask;
        }

        private void ShowBreak(int deaths)
        {
            if(deaths == 0)
            {
                //No message
                return;
            }
            else if(deaths == 1)
            {
                StoryRun(new Run()
                {
                    Text = MorningResources.DayBreak1
                });
            }
            else if(deaths >= 2 && deaths <= 3)
            {
                StoryRun(new Run()
                {
                    Text = MorningResources.DayBreak2_3
                });
            }
            else if(deaths >= 4 && deaths <= 5)
            {
                StoryRun(new Run()
                {
                    Text = MorningResources.DayBreak4_5
                });
            }
            else if(deaths >= 6 && deaths <= 7)
            {
                StoryRun(new Run()
                {
                    Text = MorningResources.DayBreak6_7
                });
            }
            else if(deaths >= 8 && deaths <= 9)
            {
                StoryRun(new Run()
                {
                    Text = MorningResources.DayBreak8_9
                });
            }
            else if(deaths >= 10 && deaths <= 11)
            {
                StoryRun(new Run()
                {
                    Text = MorningResources.DayBreak10_11
                });
            }
            else if(deaths >= 12 && deaths <= 13)
            {
                StoryRun(new Run()
                {
                    Text = MorningResources.DayBreak12_13
                });
            }
            else
            {
                StoryRun(new Run()
                {
                    Text = MorningResources.DayBreak14_MORE
                });
            }
        }

        private void ShowRemainedTeam(MorningBreakContext con)
        {
            StoryRun(new Run()
            {
                Text = MorningResources.Parity
            });
            StoryRun(new Run()
            {
                Text = $"{con.Town} ",
                Foreground = Team.TOWN.GetColor()
            });
            StoryRun(new Run()
            {
                Text = $"{con.Mafia} ",
                Foreground = Team.MAFIA.GetColor()
            });
            StoryRun(new Run()
            {
                Text = $"{con.Cultus} ",
                Foreground = Team.CULTUS.GetColor()
            });
            StoryRun(new Run()
            {
                Text = $"{con.Undead} ",
                Foreground = Team.UNDEAD.GetColor()
            });
            StoryRun(new Run()
            {
                Text = $"{con.Neutral} ",
                Foreground = Team.SERIAL_KILLER.GetColor()
            });
        }

        private void ShowReasonText(MorningVictimContext.DeathReason reason)
        {
            StoryClear();
            string text = string.Empty;

            try
            {
                string[] texts = Array.Empty<string>();

                switch(reason)
                {
                    case MorningVictimContext.DeathReason.MAFIA:
                    {
                        texts = MorningResources.MafiaDeath.Split(Environment.NewLine);
                        break;
                    }
                    case MorningVictimContext.DeathReason.SERIAL_KILLER:
                    {
                        texts = MorningResources.SerialKillerDeath.Split(Environment.NewLine);
                        break;
                    }
                    case MorningVictimContext.DeathReason.VIGILANTE:
                    {
                        texts = MorningResources.VigilanteDeath.Split(Environment.NewLine);
                        break;
                    }
                    case MorningVictimContext.DeathReason.DRIVER:
                    {
                        texts = MorningResources.DriverDeath.Split(Environment.NewLine);
                        break;
                    }
                    case MorningVictimContext.DeathReason.TERRORIST:
                    {
                        texts = MorningResources.TerroristDeath.Split(Environment.NewLine);
                        break;
                    }
                    case MorningVictimContext.DeathReason.SUICIDE:
                    {
                        texts = MorningResources.SuicideDeath.Split(Environment.NewLine);
                        break;
                    }
                }

                text = texts[Random.Shared.Next(texts.Length)];
            }
            catch(IndexOutOfRangeException)
            {
                text = string.Empty;
            }
            finally
            {
                StoryRun(new Run()
                {
                    Text = text
                });
            }
        }

        private void ShowLastWill(string will)
        {
            if(!string.IsNullOrWhiteSpace(will))
            {
                StoryNewLine();
                StoryRun(new Run()
                {
                    Text = MorningResources.LastWill,
                    FontStyle = FontStyles.Italic
                });
                StoryRun(new Run()
                {
                    Text = will
                });
            }
        }
    }
}