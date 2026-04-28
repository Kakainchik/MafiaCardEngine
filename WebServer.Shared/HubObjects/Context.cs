namespace WebServer.Shared.HubObjects
{
    public abstract record class Context
    {
        public ContextPresenter Presenter { get; set; }

        public readonly record struct ContextPresenter
        {
            public int RoomId { get; init; }
            public long Sender { get; init; }
            public long Receiver { get; init; }
            public long Except { get; init; }
        }
    }
}