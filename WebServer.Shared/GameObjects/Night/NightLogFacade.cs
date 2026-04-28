using GameLogic.Cycles.Night;
using WebServer.Shared.HubObjects;

namespace WebServer.Shared.GameObjects.Night
{
    public class NightLogFacade
    {
        private const int IMIDIATE_INTERVAL = 0;
        private const int BLOCK_INTERVAL = 1;
        private const int KILL_INTERVAL = 3;
        private const int RESSURECT_INTERVAL = 2;

        private readonly IReadOnlyCollection<ActionLog> logs;
        private readonly int roomId;

        public NightLogFacade(IReadOnlyCollection<ActionLog> logs, int roomId)
        {
            this.logs = logs;
            this.roomId = roomId;
        }

        public Queue<Tuple<NightActionLogContext, short>> ZipContext()
        {
            Queue<Tuple<NightActionLogContext, short>> queue = new Queue<Tuple<NightActionLogContext, short>>();

            InitialNotifyLogTemplate initial = new InitialNotifyLogTemplate(logs, roomId);
            foreach(NightActionLogContext i in initial.ConvertLog())
            {
                queue.Enqueue(new Tuple<NightActionLogContext, short>(i, IMIDIATE_INTERVAL));
            }

            EscortLogTemplate escort = new EscortLogTemplate(logs, roomId);
            foreach(NightActionLogContext i in escort.ConvertLog())
            {
                queue.Enqueue(new Tuple<NightActionLogContext, short>(i, BLOCK_INTERVAL));
            }

            KillerLogTemplate killer = new KillerLogTemplate(logs, roomId);
            foreach(NightActionLogContext i in killer.ConvertLog())
            {
                queue.Enqueue(new Tuple<NightActionLogContext, short>(i, KILL_INTERVAL));
            }

            BlowLogTemplate blow = new BlowLogTemplate(logs, roomId);
            foreach(NightActionLogContext i in blow.ConvertLog())
            {
                queue.Enqueue(new Tuple<NightActionLogContext, short>(i, KILL_INTERVAL));
            }

            RessurectLogTemplate ressurect = new RessurectLogTemplate(logs, roomId);
            foreach(NightActionLogContext i in ressurect.ConvertLog())
            {
                queue.Enqueue(new Tuple<NightActionLogContext, short>(i, RESSURECT_INTERVAL));
            }

            return queue;
        }
    }

    public enum InfoType
    {
        EXECUTOR_CULTUS_LEADER_RECRUIT,
        EXECUTOR_GODFATHER_RECRUIT,
        EXECUTOR_DETECT_DANGEROUS,
        EXECUTOR_DETECT_PEACEFUL,
        EXECUTOR_INVESTIGATE,
        EXECUTOR_KILL_IMMUNE,
        EXECUTOR_WITCH,

        ALL_DRIVER_INCEDENT,
        ALL_RESSURECT,
        ALL_TERRORIST_BLOW,

        OTHER_SUICIDE,
        OTHER_MAFIA_KILL,
        OTHER_SERIAL_KILLER_KILL,
        OTHER_VIGILANTE_KILL,

        TARGET_CULTUS_LEADER_RECRUIT,
        TARGET_GODFATHER_RECRUIT,
        TARGET_DRIVER,
        TARGET_ESCORT,
        TARGET_ESCORT_MEET_ESCORT,
        TARGET_ESCORT_SELF,
        TARGET_HEAL,
        TARGET_KILL,
        TARGET_SUICIDE,
        TARGET_RESSURECT,
        TARGET_WITCH
    }
}