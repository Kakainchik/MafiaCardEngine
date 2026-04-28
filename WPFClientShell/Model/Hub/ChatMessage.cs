namespace WPFClientShell.Model.Hub
{
    public record class ChatMessage
    {
        public string Who { get; init; }
        public string Message { get; init; }
        public TimeOnly DesiredTime { get; init; }

        public ChatMessage(string who, string message, TimeOnly desiredTime)
        {
            Who = who;
            Message = message;
            DesiredTime = desiredTime;
        }
    }
}