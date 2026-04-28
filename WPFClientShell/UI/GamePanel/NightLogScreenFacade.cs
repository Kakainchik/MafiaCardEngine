using GameLogic.Attributes;
using System.Windows.Documents;
using WebServer.Shared.GameObjects.Night;
using WebServer.Shared.HubObjects;
using WPFClientShell.Extensions;
using WPFClientShell.Resources.GameStory;

namespace WPFClientShell.UI.GamePanel
{
    public class NightLogScreenFacade
    {
        private readonly IFlowStory flow;

        public NightLogScreenFacade(IFlowStory flow)
        {
            this.flow = flow;
        }

        public void HanleLog(NightActionLogContext context)
        {
            switch(context.Action)
            {
                case InfoType.EXECUTOR_CULTUS_LEADER_RECRUIT:
                {
                    if(context.Success)
                    {
                        flow.StoryRun(new Run()
                        {
                            Text = NightResources.ExecutorCultusLeaderAct
                        });
                    }
                    else
                    {
                        flow.StoryRun(new Run()
                        {
                            Text = NightResources.ExecutorCultusLeaderActFail
                        });
                    }
                    break;
                }
                case InfoType.EXECUTOR_GODFATHER_RECRUIT:
                {
                    if(context.Success)
                    {
                        flow.StoryRun(new Run()
                        {
                            Text = NightResources.ExecutorGodfatherAct
                        });
                    }
                    else
                    {
                        flow.StoryRun(new Run()
                        {
                            Text = NightResources.ExecutorGodfatherActFail
                        });
                    }
                    break;
                }
                case InfoType.EXECUTOR_DETECT_DANGEROUS:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.ExecutorDetectActDangerous,
                        Foreground = Team.MAFIA.GetColor()
                    });
                    break;
                }
                case InfoType.EXECUTOR_DETECT_PEACEFUL:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.ExecutorDetectActPeaceful,
                        Foreground = Team.TOWN.GetColor()
                    });
                    break;
                }
                case InfoType.EXECUTOR_INVESTIGATE when context is DetectiveLogContext dlc:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.ExecutorInvestigateAct
                    });
                    flow.StoryRun(new Run
                    {
                        Text = dlc.Target.MapRole().GetLocalizedName(),
                        Foreground = dlc.Target.MapRole().GetColor()
                    });
                    break;
                }
                case InfoType.EXECUTOR_KILL_IMMUNE:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.ExecutorKillImmune
                    });
                    break;
                }
                case InfoType.EXECUTOR_WITCH:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.ExecutorWitchAct
                    });
                    break;
                }
                case InfoType.ALL_DRIVER_INCEDENT:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.KillActDriverAccident
                    });
                    break;
                }
                case InfoType.ALL_RESSURECT:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.RessurectAct
                    });
                    break;
                }
                case InfoType.ALL_TERRORIST_BLOW:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TerroristAct
                    });
                    break;
                }
                case InfoType.OTHER_SUICIDE:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.KillActSuicide
                    });
                    break;
                }
                case InfoType.OTHER_MAFIA_KILL:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.MafiaActKill
                    });
                    break;
                }
                case InfoType.OTHER_SERIAL_KILLER_KILL:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.SerialKillerActKill
                    });
                    break;
                }
                case InfoType.OTHER_VIGILANTE_KILL:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.VigilanteActKill
                    });
                    break;
                }
                case InfoType.TARGET_CULTUS_LEADER_RECRUIT:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TargetCultusLeaderAct
                    });
                    break;
                }
                case InfoType.TARGET_GODFATHER_RECRUIT:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TargetGodfatherAct
                    });
                    break;
                }
                case InfoType.TARGET_DRIVER:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TargetDriverAct
                    });
                    break;
                }
                case InfoType.TARGET_ESCORT:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TargetEscortAct
                    });
                    break;
                }
                case InfoType.TARGET_ESCORT_MEET_ESCORT:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TargetEscortActOther
                    });
                    break;
                }
                case InfoType.TARGET_ESCORT_SELF:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TargetEscortActSelf
                    });
                    break;
                }
                case InfoType.TARGET_HEAL:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TargetHealAct
                    });
                    break;
                }
                case InfoType.TARGET_KILL:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TargetKillAct
                    });
                    break;
                }
                case InfoType.TARGET_SUICIDE:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TargetKillActSuicide
                    });
                    break;
                }
                case InfoType.TARGET_RESSURECT:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TargetRessurectAct
                    });
                    break;
                }
                case InfoType.TARGET_WITCH:
                {
                    flow.StoryRun(new Run
                    {
                        Text = NightResources.TargetWitchAct
                    });
                    break;
                }
            }

            flow.StoryNewLine();
        }
    }
}