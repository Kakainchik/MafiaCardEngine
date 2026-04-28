using WebServer.Model.Room;
using WebServer.Model.User;

namespace WebServer.Services
{
    public class HallService : IHallService
    {
        private const int PAGE_SIZE = 100;
        private const string DUBLICATE_LOBBY_ERROR = "Dublicate lobby tried to be created. Possibly host-user is already present in another lobby.";

        private readonly IWaitRoomRepository roomRepository;

        public HallService(IWaitRoomRepository roomyRepository)
        {
            this.roomRepository = roomyRepository;
        }

        public WaitRoomDTO CreateRoom(CreateRoomDTO dto, UserDTO host)
        {
            UserRoomDomain hostDomain = new UserRoomDomain(host.Id, host.Username)
            {
                IsReady = true
            };
            WaitRoomDomain domain = new WaitRoomDomain(hostDomain, dto.Title, dto.Description, dto.MaxSeats);
            bool success = roomRepository.AddRoom(domain);

            if(!success)
            {
                throw new BadHttpRequestException(DUBLICATE_LOBBY_ERROR, StatusCodes.Status403Forbidden);
            }

            return new WaitRoomDTO(domain.Id, host, domain.Title, domain.Description, domain.Fullness, domain.MaxSeats);
        }

        public IEnumerable<WaitRoomDTO> GetRooms(int page, bool onlyValid = true)
        {
            page = Math.Abs(page);
            int offset = PAGE_SIZE * (page - 1);
            Range listRange = offset..(offset + PAGE_SIZE);

            IEnumerable<WaitRoomDomain> domains;
            if(onlyValid)
            {
                domains = roomRepository.GetValidRooms(listRange);
            }
            else
            {
                domains = roomRepository.GetRooms(listRange);
            }

            return domains.Select(domain =>
            {
                UserDTO hostDto = new UserDTO(domain.Host.Id, domain.Host.Username);
                return new WaitRoomDTO(domain.Id, hostDto, domain.Title, domain.Description, domain.Fullness, domain.MaxSeats);
            });
        }

        public WaitRoomDTO? GetRoom(int id)
        {
            WaitRoomDomain? domain = roomRepository.GetRoom(id);
            if(domain is null)
            {
                return null;
            }

            UserDTO hostDto = new UserDTO(domain.Host.Id, domain.Host.Username);
            return new WaitRoomDTO(domain.Id, hostDto, domain.Title, domain.Description, domain.Fullness, domain.MaxSeats);
        }

        public IEnumerable<UserDTO> GetPlayers(int roomId)
        {
            IEnumerable<UserRoomDomain> domains = roomRepository.GetPlayers(roomId);
            return domains.Select(d => new UserDTO(d.Id, d.Username));
        }

        public bool JoinRoom(int roomId, UserDTO user, out bool isHost)
        {
            isHost = false;

            WaitRoomDomain? lobby = roomRepository.GetRoom(roomId);
            if(lobby is not null)
            {
                bool doPlayerExist = roomRepository.IsPlayerExist(user.Id);
                if(!doPlayerExist)
                {
                    isHost = lobby.Host == user;
                    if(!isHost && !roomRepository.IsHostPresent(roomId))
                    {
                        //Not allow to join if host is not present yet
                        return false;
                    }

                    UserRoomDomain userDomain = new UserRoomDomain(user.Id, user.Username)
                    {
                        IsReady = isHost
                    };
                    roomRepository.AddPlayerToRoom(roomId, userDomain);
                }
                return !doPlayerExist;
            }
            else return false;
        }

        /// <summary>
        /// Remove the player instance from a lobby it is affiliated.
        /// Note: This method always return a value and is successful.
        /// </summary>
        /// <param name="user">The player instance.</param>
        /// <returns>The lobby identification number. Returns 0 if no lobby found attached to the player.</returns>
        public int KickPlayer(UserDTO user)
        {
            int? roomId = roomRepository.GetRoomIdByPlayer(user.Id);
            if(roomId.HasValue)
            {
                roomRepository.KickPlayerFromRoom(roomId.Value, user.Id);
            }

            return roomId ?? 0;
        }
    }
}