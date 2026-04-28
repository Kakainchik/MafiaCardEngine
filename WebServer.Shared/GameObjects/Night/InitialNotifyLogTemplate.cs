using GameLogic.Attributes;
using GameLogic.Cycles.Night;
using GameLogic.Model;
using GameLogic.Roles;
using WebServer.Shared.Extensions;
using WebServer.Shared.HubObjects;

namespace WebServer.Shared.GameObjects.Night
{
    public class InitialNotifyLogTemplate : LogTemplate
    {
        public InitialNotifyLogTemplate(IEnumerable<ActionLog> dedicatedLogs, int roomId)
            : base(dedicatedLogs, roomId)
        {

        }

        public override IEnumerable<NightActionLogContext> ConvertLog()
        {
            Func<ActionLog, bool> comparer = (ActionLog l) =>
            {
                switch(l.Executor)
                {
                    case WitchRole:
                    case DriverRole:
                    case DetectiveRole:
                    case CounselorRole:
                    case PolicemanRole:
                    case GodfatherRole:
                    case CultusLeaderRole:
                    case ZombieRole:
                    case CursedRole:
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
                    case ActionType.WITCH_CONTROL:
                    {
                        foreach(NightActionLogContext witch in HandleWitch(l))
                        {
                            yield return witch;
                        }
                        break;
                    }
                    case ActionType.DRIVER_SWAP:
                    {
                        foreach(NightActionLogContext driver in HandleDriver(l))
                        {
                            yield return driver;
                        }
                        break;
                    }
                    case ActionType.INVESTIGATE:
                    {
                        foreach(NightActionLogContext investigate in HandleInvestigate(l))
                        {
                            yield return investigate;
                        }
                        break;
                    }
                    case ActionType.RECRUIT:
                    {
                        foreach(NightActionLogContext recruit in HandleRecruit(l))
                        {
                            yield return recruit;
                        }
                        break;
                    }
                }
            }
        }

        private IEnumerable<NightActionLogContext> HandleWitch(ActionLog l)
        {
            NightActionLogContext exContext = new NightActionLogContext
            {
                Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Executor.Owner!.Id),
                Action = InfoType.EXECUTOR_WITCH,
                Success = l.Success
            };
            yield return exContext;

            NightActionLogContext tarContext = new NightActionLogContext
            {
                Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Target.Owner!.Id),
                Action = InfoType.TARGET_WITCH,
                Success = l.Success
            };
            yield return tarContext;
        }

        private IEnumerable<NightActionLogContext> HandleDriver(ActionLog l)
        {
            NightActionLogContext tarContext = new NightActionLogContext
            {
                Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Target.Owner!.Id),
                Action = InfoType.TARGET_DRIVER,
                Success = l.Success
            };
            yield return tarContext;
        }

        private IEnumerable<NightActionLogContext> HandleInvestigate(ActionLog l)
        {
            if(l.Executor is PolicemanRole)
            {
                InfoType infoType = l.Target.GetTeam() == Team.TOWN
                    ? InfoType.EXECUTOR_DETECT_PEACEFUL : InfoType.EXECUTOR_DETECT_DANGEROUS;

                NightActionLogContext tarContext = new NightActionLogContext
                {
                    Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Executor.Owner!.Id),
                    Action = infoType,
                    Success = l.Success
                };
                yield return tarContext;
            }
            else
            {
                NightActionLogContext tarContext = new DetectiveLogContext
                {
                    Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Executor.Owner!.Id),
                    Action = InfoType.EXECUTOR_INVESTIGATE,
                    Success = l.Success,
                    Target = l.Target.IntoSignature()
                };
                yield return tarContext;
            }
        }

        private IEnumerable<NightActionLogContext> HandleRecruit(ActionLog log)
        {
            switch(log.Executor)
            {
                case GodfatherRole:
                {
                    NightActionLogContext exContext = new NightActionLogContext
                    {
                        Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, log.Executor.Owner!.Id),
                        Action = InfoType.EXECUTOR_GODFATHER_RECRUIT,
                        Success = log.Success
                    };
                    yield return exContext;

                    NightActionLogContext tarContext = new NightActionLogContext
                    {
                        Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, log.Target.Owner!.Id),
                        Action = InfoType.TARGET_GODFATHER_RECRUIT,
                        Success = log.Success
                    };
                    yield return exContext;

                    break;
                }
                case CultusLeaderRole:
                {
                    NightActionLogContext exContext = new NightActionLogContext
                    {
                        Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, log.Executor.Owner!.Id),
                        Action = InfoType.EXECUTOR_CULTUS_LEADER_RECRUIT,
                        Success = log.Success
                    };
                    yield return exContext;

                    if(log.Success)
                    {
                        NightActionLogContext tarContext = new NightActionLogContext
                        {
                            Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, log.Target.Owner!.Id),
                            Action = InfoType.TARGET_CULTUS_LEADER_RECRUIT,
                            Success = log.Success
                        };
                        yield return tarContext;
                    }
                    break;
                }
            }
        }
    }
}