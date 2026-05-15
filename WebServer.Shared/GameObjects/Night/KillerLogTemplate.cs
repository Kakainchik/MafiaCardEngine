using GameLogic.Cycles.Night;
using GameLogic.Model;
using GameLogic.Roles;
using WebServer.Shared.Extensions;
using WebServer.Shared.HubObjects;

namespace WebServer.Shared.GameObjects.Night
{
    public class KillerLogTemplate : LogTemplate
    {
        public KillerLogTemplate(IEnumerable<ActionLog> dedicatedLogs, int roomId)
            : base(dedicatedLogs, roomId)
        {

        }

        public override IEnumerable<NightActionLogContext> ConvertLog()
        {
            Func<ActionLog, bool> comparer = (ActionLog l) =>
            {
                switch(l.Executor)
                {
                    case MafiaRole:
                    case SerialKillerRole:
                    case VigilanteRole:
                    case DoctorRole:
                    case SurgeonRole:
                    case DriverRole:
                    {
                        return true;
                    }
                    default:
                    {
                        return false;
                    }
                }
            };
            IEnumerable<ActionLog> desiredRoles = dedicatedLogs.Where(comparer);
            
            foreach(ActionLog l in desiredRoles)
            {
                if(l.Executor.Owner is null || l.Target.Owner is null)
                {
                    continue;
                }

                switch(l.Action)
                {
                    case ActionType.KILL:
                    {
                        if(!l.Success)
                        {
                            NightActionLogContext exContext = new NightActionLogContext
                            {
                                Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Executor.Owner.Id),
                                Action = InfoType.EXECUTOR_KILL_IMMUNE,
                                Success = l.Success
                            };
                            yield return exContext;

                            break;
                        }

                        if(l.Target == l.Executor)
                        {
                            //Figure out why the killer killed themself
                            bool driverActed = desiredRoles.Any(driverLog => driverLog.Action == ActionType.SWAP
                                && driverLog.Target == l.Executor);

                            if(driverActed)
                            {
                                //Driver rided over the target
                                NightActionLogContext allContext = new NightActionLogContext
                                {
                                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                                    Action = InfoType.ALL_DRIVER_INCEDENT,
                                    Success = l.Success
                                };
                                yield return allContext;
                            }
                            else
                            {
                                //It is just a suicide, probably influenced by a witch
                                NightActionLogContext otherContext = new NightActionLogContext
                                {
                                    Presenter = LobbyHubConstants.ServerExceptPresenter(roomId, l.Target.Owner.Id),
                                    Action = InfoType.OTHER_SUICIDE,
                                    Success = l.Success
                                };
                                yield return otherContext;

                                NightActionLogContext tarContext = new NightActionLogContext
                                {
                                    Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Target.Owner.Id),
                                    Action = InfoType.TARGET_SUICIDE,
                                    Success = l.Success
                                };
                                yield return tarContext;
                            }
                        }
                        else
                        {
                            yield return FindKillerForOther(l);

                            NightActionLogContext tarContext = new NightActionLogContext
                            {
                                Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Target.Owner.Id),
                                Action = InfoType.TARGET_KILL,
                                Success = l.Success
                            };
                            yield return tarContext;
                        }

                        //Find healer
                        bool hasHealed = desiredRoles.Any(healerLog => healerLog.Action == ActionType.PROTECT
                            && healerLog.Target == l.Target
                            && healerLog.Success);

                        if(hasHealed)
                        {
                            NightActionLogContext tarContext = new NightActionLogContext
                            {
                                Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Target.Owner.Id),
                                Action = InfoType.TARGET_HEAL,
                                Success = l.Success
                            };
                            yield return tarContext;
                        }

                        break;
                    }
                }
            }
        }

        private NightActionLogContext FindKillerForOther(ActionLog log)
        {
            InfoType infoType;
            switch(log.Executor)
            {
                case MafiaRole:
                {
                    infoType = InfoType.OTHER_MAFIA_KILL;
                    break;
                }
                case SerialKillerRole:
                {
                    infoType = InfoType.OTHER_SERIAL_KILLER_KILL;
                    break;
                }
                case VigilanteRole:
                {
                    infoType = InfoType.OTHER_VIGILANTE_KILL;
                    break;
                }
                default:
                {
                    infoType = InfoType.OTHER_SUICIDE;
                    break;
                }
            }

            NightActionLogContext otherContext = new NightActionLogContext
            {
                Presenter = LobbyHubConstants.ServerExceptPresenter(roomId, log.Target.Owner!.Id),
                Action = infoType,
                Success = log.Success
            };
            return otherContext;
        }
    }
}