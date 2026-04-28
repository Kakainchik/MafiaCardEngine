using GameLogic.Cycles.Night;
using GameLogic.Roles;
using WebServer.Shared.HubObjects;

namespace WebServer.Shared.GameObjects.Night
{
    public class EscortLogTemplate : LogTemplate
    {
        public EscortLogTemplate(IEnumerable<ActionLog> dedicatedLogs, int roomId)
            : base(dedicatedLogs, roomId)
        {

        }

        public override IEnumerable<NightActionLogContext> ConvertLog()
        {
            Func<ActionLog, bool> comparer = (ActionLog l) =>
            {
                switch(l.Executor)
                {
                    case ProstituteRole:
                    case WhoreRole:
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
                if(l.Executor == l.Target)
                {
                    yield return new NightActionLogContext
                    {
                        Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Executor.Owner!.Id),
                        Action = InfoType.TARGET_ESCORT_SELF,
                        Success = l.Success
                    };
                }
                else if(l.Target is WhoreRole || l.Target is ProstituteRole)
                {
                    yield return new NightActionLogContext
                    {
                        Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Target.Owner!.Id),
                        Action = InfoType.TARGET_ESCORT_MEET_ESCORT,
                        Success = l.Success
                    };
                }
                else
                {
                    yield return new NightActionLogContext
                    {
                        Presenter = LobbyHubConstants.ServerToUserPresenter(roomId, l.Target.Owner!.Id),
                        Action = InfoType.TARGET_ESCORT,
                        Success = l.Success
                    };
                }
            }
        }
    }
}