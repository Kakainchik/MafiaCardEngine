using GameLogic.Cycles.Night;
using GameLogic.Model;
using GameLogic.Roles;
using WebServer.Shared.HubObjects;

namespace WebServer.Shared.GameObjects.Night
{
    public class BlowLogTemplate : LogTemplate
    {
        public BlowLogTemplate(IEnumerable<ActionLog> dedicatedLogs, int roomId)
            : base(dedicatedLogs, roomId)
        {

        }

        public override IEnumerable<NightActionLogContext> ConvertLog()
        {
            Func<ActionLog, bool> comparer = (ActionLog l) =>
            {
                switch(l.Executor)
                {
                    case TerroristRole:
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
                    case ActionType.TERRORIST_BLOW:
                    {
                        NightActionLogContext exContext = new NightActionLogContext
                        {
                            Presenter = LobbyHubConstants.ServerPresenter(roomId),
                            Action = InfoType.ALL_TERRORIST_BLOW,
                            Success = l.Success
                        };
                        yield return exContext;

                        break;
                    }
                }
            }
        }
    }
}