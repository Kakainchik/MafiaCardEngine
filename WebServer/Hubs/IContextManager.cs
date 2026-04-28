using WebServer.Shared.HubObjects;

namespace WebServer.Hubs
{
    public interface IContextManager
    {
        IHubCommand? SendOtherCommand { set; }
        IHubCommand? SendAllCommand { set; }
        IHubIdCommand? SendUserCommand { set; }

        Task ProcessContext(Context context);
    }
}