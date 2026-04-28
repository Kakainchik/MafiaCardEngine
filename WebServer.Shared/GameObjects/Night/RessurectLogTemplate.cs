using GameLogic.Cycles.Night;
using GameLogic.Model;
using GameLogic.Roles;
using WebServer.Shared.HubObjects;

namespace WebServer.Shared.GameObjects.Night
{
    internal class RessurectLogTemplate : LogTemplate
    {
        public RessurectLogTemplate(IEnumerable<ActionLog> dedicatedLogs, int roomId)
            : base(dedicatedLogs, roomId)
        {

        }

        public override IEnumerable<NightActionLogContext> ConvertLog()
        {
            Func<ActionLog, bool> comparer = (ActionLog l) =>
            {
                switch(l.Executor)
                {
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
                    case ActionType.RESSURECT:
                    {
                        foreach(NightActionLogContext ressurect in HandleRessurect(l))
                        {
                            yield return ressurect;
                        }
                        break;
                    }
                }
            }
        }

        private IEnumerable<NightActionLogContext> HandleRessurect(ActionLog log)
        {
            if(!log.Success)
            {
                yield break;
            }

            NightActionLogContext allContext = new NightActionLogContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Action = InfoType.ALL_RESSURECT,
                Success = log.Success
            };
            yield return allContext;

            NightActionLogContext tarContext = new NightActionLogContext
            {
                Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, log.Target.Owner!.Id),
                Action = InfoType.TARGET_RESSURECT,
                Success = log.Success
            };
            yield return tarContext;
        }
    }
}