using WebServer.Shared.HubObjects;

namespace WebServer.Hubs
{
    public interface IHubCommand
    {
        void Execute(Context con);
        Task ExecuteAsync(Context con);
    }

    public interface IHubIdCommand
    {
        void Execute(Context con, params ulong[] userIds);
        Task ExecuteAsync(Context con, params ulong[] userIds);
    }
}