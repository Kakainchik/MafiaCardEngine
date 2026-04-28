using GameLogic.Attributes;
using Swordfish.NET.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using WebServer.Shared.Extensions;
using WebServer.Shared.HubObjects;
using WPFClientShell.Core;
using WPFClientShell.Extensions;
using WPFClientShell.Model.Hub;
using WPFClientShell.Resources.GameStory;
using WPFClientShell.UI.GamePanel;

namespace WPFClientShell.UI
{
    public class NightScreenViewModel : ScreenViewModel
    {
        private readonly NightLogScreenFacade nightFacade;

        private bool isNightBegan;
        private ExecutorType executorType;
        private int? targetNumberSelected;

        public bool IsNightBegan
        {
            get => isNightBegan;
            set
            {
                isNightBegan = value;
                OnPropertyChanged(nameof(IsNightBegan));
            }
        }

        public ObservableCollection<NightTargetEntity> OwnTargets { get; set; }
        public ConcurrentObservableDictionary<long, PlayerEntity> PossibleTargets { get; set; }

        public ICommand ClickTargetCommand { get; set; }
        public ICommand ClearTargetCommand { get; set; }
        public ICommand ActivateFlagCommand { get; set; }

        public NightScreenViewModel(LobbyDomain lobbyDomain,
            PlayerEntity ownPlayer,
            IDictionary<long, PlayerEntity> possibleTargets)
            : base(lobbyDomain, ownPlayer)
        {
            nightFacade = new NightLogScreenFacade(this);

            OwnTargets = new ObservableCollection<NightTargetEntity>();
            PossibleTargets = new ConcurrentObservableDictionary<long, PlayerEntity>();

            foreach(KeyValuePair<long, PlayerEntity> p in possibleTargets)
            {
                PossibleTargets[p.Key] = p.Value;
            }

            ExecutorAttribute attr = ownPlayer.Role.GetExecutorAttribute();
            executorType = attr.EType;

            if(ownPlayer.IsAlive)
            {
                switch(executorType)
                {
                    case ExecutorType.NONE:
                    {
                        //Nothing to do, we cannot pick a target
                        break;
                    }
                    case ExecutorType.TARGET:
                    {
                        //We can pick just a target
                        OwnTargets.Add(new NightTargetEntity(1));
                        break;
                    }
                    case ExecutorType.TARGET_TARGET:
                    {
                        //We can pick two targets
                        OwnTargets.Add(new NightTargetEntity(1));
                        OwnTargets.Add(new NightTargetEntity(2));
                        break;
                    }
                    case ExecutorType.EXECUTOR_TARGER:
                    {
                        //We can pick two targets
                        OwnTargets.Add(new NightTargetEntity(1));
                        OwnTargets.Add(new NightTargetEntity(2));
                        break;
                    }
                }
            }

            ClickTargetCommand = new RelayCommand(OnClickTarget);
            ClearTargetCommand = new RelayCommand(OnClearTarget);
            ActivateFlagCommand = new RelayCommand(OnActivateFlag);
        }

        public override async Task HandleContext(Context context)
        {
            switch(context)
            {
                case NightPlayerDataContext npdc:
                {
                    //Update public players info for this day
                    for(int i = 0; i < npdc.NightPlayers.Length; i++)
                    {
                        PossibleTargets[npdc.NightPlayers[i].Id].IsAlive = npdc.NightPlayers[i].IsAlive;
                    }
                    break;
                }
                case NightStepContext nsc:
                {
                    await ProcessStep(nsc.Step);
                    break;
                }
                case NightItemsContext nic:
                {
                    StoryNewLine();
                    StoryRun(new Run()
                    {
                        Text = string.Format(NightResources.VigilanteRemainingBullets, nic.Amount),
                        FontStyle = FontStyles.Italic
                    });
                    break;
                }
                case NightActionLogContext nalc:
                {
                    nightFacade.HanleLog(nalc);
                    break;
                }
            }
        }

        private async Task ProcessStep(NightStepContext.NightStep step)
        {
            switch(step)
            {
                case NightStepContext.NightStep.START_REMINDER:
                {
                    StoryRun(new Run()
                    {
                        Text = NightResources.NightStart
                    });
                    StoryNewLine();
                    StoryRun(new Run()
                    {
                        Text = NightResources.RoleRemind
                    });
                    StoryRun(new Run()
                    {
                        Text = ownPlayer.Role.GetLocalizedName(),
                        Foreground = ownPlayer.Role.GetColor()
                    });
                    StoryNewLine();
                    StoryRun(new Run()
                    {
                        Text = ownPlayer.Role.GetLocilizedAbility(),
                        Foreground = ownPlayer.Role.GetColor()
                    });
                    break;
                }
                case NightStepContext.NightStep.ALLOW_SELECTION:
                {
                    IsNightBegan = true;

                    StoryNewLine();
                    StoryRun(new Run()
                    {
                        Text = NightResources.StartAction
                    });
                    break;
                }
                case NightStepContext.NightStep.DISALLOW_SELECTION:
                {
                    IsNightBegan = false;

                    IEnumerable<long?> targetIds = OwnTargets.Select(t => t.TargetId);

                    if(targetIds.Any(i => !i.HasValue))
                    {
                        //Do not send the context if we have not picked targets correctly
                        break;
                    }

                    NightActionConfirmation confirmation = new NightActionConfirmation
                    {
                        AbilityFlag = 1,
                        TargetIds = targetIds.Cast<long>().ToArray<long>()
                    };
                    await lobbyDomain.SendContextAsync(confirmation);
                    break;
                }
                case NightStepContext.NightStep.END:
                {
                    StoryNewLine();
                    StoryRun(new Run()
                    {
                        Text = NightResources.NightEnd
                    });
                    break;
                }
            }
        }

        private void OnClickTarget(object? obj)
        {
            if(targetNumberSelected.HasValue)
            {
                PlayerEntity target = (PlayerEntity)obj!;

                if(!target.IsAlive)
                {
                    //Cannot pick dead players
                    return;
                }

                if(targetNumberSelected.Value == 1)
                {
                    if(target.Id != ownPlayer.Id)
                    {
                        //Cannot pick ourself as a primary target
                        OwnTargets[targetNumberSelected.Value - 1].SetTarget(target);
                        targetNumberSelected = null;
                    }
                }
                else
                {
                    //Pick non-primary target
                    switch(executorType)
                    {
                        case ExecutorType.TARGET_TARGET:
                        {
                            if(target.Id != ownPlayer.Id)
                            {
                                //Cannot pick ourself as a non-primary target
                                OwnTargets[targetNumberSelected.Value - 1].SetTarget(target);
                                targetNumberSelected = null;
                            }
                            break;
                        }
                        default:
                        {
                            OwnTargets[targetNumberSelected.Value - 1].SetTarget(target);
                            targetNumberSelected = null;
                            break;
                        }
                    }
                }
            }
        }

        private void OnClearTarget(object? obj)
        {
            NightTargetEntity target = (NightTargetEntity)obj!;

            if(target.TargetId.HasValue)
            {
                target.ClearTarget();
            }

            targetNumberSelected = null;
        }

        private void OnActivateFlag(object? obj)
        {
            int number = (int)obj!;
            
            targetNumberSelected = number;
        }
    }
}