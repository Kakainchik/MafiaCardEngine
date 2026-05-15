namespace WebServer.Shared.HubObjects
{
    public abstract record class Context
    {
        public ContextPresenter Presenter { get; set; }

        public readonly record struct ContextPresenter
        {
            public int RoomId { get; init; }
            public ulong Sender { get; init; }
            public ulong Receiver { get; init; }
            public ulong Except { get; init; }
        }
    }
}