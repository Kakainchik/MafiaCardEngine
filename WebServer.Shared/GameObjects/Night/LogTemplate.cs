using GameLogic.Cycles.Night;
using WebServer.Shared.HubObjects;

namespace WebServer.Shared.GameObjects.Night
{
    public abstract class LogTemplate
    {
        public readonly IEnumerable<ActionLog> dedicatedLogs;
        public readonly int roomId;

        public LogTemplate(IEnumerable<ActionLog> dedicatedLogs, int roomId)
        {
            this.dedicatedLogs = dedicatedLogs;
            this.roomId = roomId;
        }

        public abstract IEnumerable<NightActionLogContext> ConvertLog();
    }
}