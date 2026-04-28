namespace WebServer.Shared.HubObjects
{
    public static class LobbyHubConstants
    {
        public const string JOIN_LOBBY_METHOD = "Join";
        public const string LEAVE_LOBBY_METHOD = "Leave";
        public const string SEND_CONTEXT_METHOD = "SendContext";
        public const string RECEIVE_CONTEXT_METHOD = "ReceiveContext";
        public const string SEND_CHAT_CONTEXT_METHOD = "SendChatContext";
        public const string RECEIVE_CHAT_CONTEXT_METHOD = "ReceiveChatContext";
        public const string SEND_SERVER_REQUEST_METHOD = "SendServerRequest";

        public static string MapGroupName(int roomId) => $"{roomId}_lobby";

        public static Context.ContextPresenter ServerPresenter(int roomId)
        {
            return new Context.ContextPresenter()
            {
                RoomId = roomId,
                Sender = 0L
            };
        }

        public static Context.ContextPresenter ServerToUserPresenter(int roomId, long user)
        {
            return new Context.ContextPresenter()
            {
                RoomId = roomId,
                Sender = 0L,
                Receiver = user
            };
        }

        public static Context.ContextPresenter ServerExceptPresenter(int roomId, long user)
        {
            return new Context.ContextPresenter()
            {
                RoomId = roomId,
                Sender = 0L,
                Except = user
            };
        }
    }
}