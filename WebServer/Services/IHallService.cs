using WebServer.Model.Room;
using WebServer.Model.User;

namespace WebServer.Services
{
    public interface IHallService
    {
        IEnumerable<WaitRoomDTO> GetRooms(int page, bool onlyValid);
        WaitRoomDTO? GetRoom(int id);
        WaitRoomDTO CreateRoom(CreateRoomDTO dto, UserDTO host);
        bool JoinRoom(int roomId, UserDTO user, out bool isHost);
        IEnumerable<UserDTO> GetPlayers(int roomId);
        int KickPlayer(UserDTO user);
    }
}