using GameLogic;
using GameLogic.Roles;
using WebServer.Model.Game;
using WebServer.Model.Room;
using WebServer.Shared.Extensions;
using WebServer.Shared.GameObjects;
using WebServer.Shared.HubObjects;

namespace WebServer.Hubs
{
    public class RequestManager
    {
        private readonly IWaitRoomRepository roomRepository;
        private readonly IGameRepository gameRepository;

        private IHubCommand? sendOtherCommand;
        private IHubCommand? sendAllCommand;
        private IHubIdCommand? sendUserCommand;

        public IHubCommand? SendOtherCommand
        {
            private get => sendOtherCommand;
            set => sendOtherCommand = value;
        }

        public IHubCommand? SendAllCommand
        {
            private get => sendAllCommand;
            set => sendAllCommand = value;
        }

        public IHubIdCommand? SendUserCommand
        {
            private get => sendUserCommand;
            set => sendUserCommand = value;
        }

        public RequestManager(IWaitRoomRepository roomRepository,
            IGameRepository gameRepository)
        {
            this.roomRepository = roomRepository;
            this.gameRepository = gameRepository;
        }

        public async Task<Context> ProcessContext(ServerRequestContext context)
        {
            EnsureCommandNotNull();

            switch(context.RequestFor)
            {
                case ServerRequestType.FOR_WAITROOM_DATA:
                {
                    WaitRoomDomain? room = roomRepository.GetRoom(context.Presenter.RoomId);
                    if(room is null)
                    {
                        throw new OperationCanceledException($"Room [{context.Presenter.RoomId}] not found.");
                    }

                    IEnumerable<WaitingRoomContext.WaitingRoomUser> players = room.Players.Values.Select(u =>
                        new WaitingRoomContext.WaitingRoomUser
                        {
                            Id = u.Id,
                            Username = u.Username,
                            IsReady = u.IsReady
                        });
                    IReadOnlyDictionary<RoleSignature, int> roles = room.Roles.AsReadOnly();

                    WaitingRoomContext response = new WaitingRoomContext
                    {
                        Presenter = LobbyHubConstants.ServerPresenter(context.Presenter.RoomId) with
                        {
                            Receiver = context.Presenter.Sender
                        },
                        Players = players.ToArray(),
                        MaxSeats = room.MaxSeats,
                        Roles = roles
                    };
                    return response;
                }
                case ServerRequestType.FOR_INITIALIZE_GAME:
                {
                    if(roomRepository.GetHost(context.Presenter.RoomId).Id != context.Presenter.Sender)
                    {
                        throw new OperationCanceledException("Only host is allowed to initialize game.");
                    }

                    WaitRoomDomain? room = roomRepository.GetRoom(context.Presenter.RoomId);
                    if(room is null)
                    {
                        throw new OperationCanceledException($"Room [{context.Presenter.RoomId}] not found.");
                    }

                    int rolesAmount = room.Roles.Sum(kvp => kvp.Value);
                    if(room.Players.Count != rolesAmount)
                    {
                        return new GameReadyContext
                        {
                            Presenter = LobbyHubConstants.ServerPresenter(context.Presenter.RoomId) with
                            {
                                Receiver = context.Presenter.Sender
                            },
                            Success = false
                        };
                    }
                    
                    IList<Role> unorderedRoles = new List<Role>();
                    foreach(KeyValuePair<RoleSignature, int> kvp in room.Roles)
                    {
                        for(int i = 0; i < kvp.Value; i++)
                        {
                            unorderedRoles.Add(kvp.Key.MakeGameRole());
                        }
                    }

                    //Randomize role order
                    Role[] shuffledRoles = unorderedRoles.ToArray();
                    Random.Shared.Shuffle(shuffledRoles);

                    unorderedRoles.Clear();

                    long[] ids = room.Players.Keys.ToArray();
                    Player[] players = new Player[rolesAmount];
                    PlayerDomain[] domains = new PlayerDomain[rolesAmount];
                    RGB[] rgbs = ColorBank.GetRandomColors(rolesAmount);

                    for(int i = 0; i < players.Length; i++)
                    {
                        players[i] = new Player(ids[i], shuffledRoles[i]);

                        string nickname = room.Players[ids[i]].Username;
                        domains[i] = new PlayerDomain(nickname, rgbs[i], players[i]);
                    }

                    GameHolder holder = new GameHolder(sendAllCommand!, sendUserCommand!, domains, context.Presenter.RoomId);

                    gameRepository.AddGame(context.Presenter.RoomId, holder);
                    roomRepository.DeleteRoom(context.Presenter.RoomId);

                    GameReadyContext readyCon = new GameReadyContext
                    {
                        Presenter = LobbyHubConstants.ServerPresenter(context.Presenter.RoomId) with
                        {
                            Receiver = context.Presenter.Sender
                        },
                        Success = true
                    };
                    await SendOtherCommand!.ExecuteAsync(readyCon);

                    return readyCon;
                }
                case ServerRequestType.FOR_PLAYERS_DATA:
                {
                    GameHolder game = gameRepository.GetGame(context.Presenter.RoomId);
                    PlayerDomain requester = game.Players.Single(p => p.Id == context.Presenter.Sender);
                    InitialPlayerDataContext.PlayerInstance[] allPlayers = game.Players.Select<PlayerDomain, InitialPlayerDataContext.PlayerInstance>(p =>
                    {
                        return new InitialPlayerDataContext.PlayerInstance
                        {
                            Id = p.Id,
                            Nickname = p.Nickname,
                            NColor = p.NColor
                        };
                    }).ToArray();

                    return new InitialPlayerDataContext
                    {
                        Presenter = LobbyHubConstants.ServerPresenter(context.Presenter.RoomId) with
                        {
                            Receiver = context.Presenter.Sender
                        },
                        OwnRole = requester.GameRepresentation.Role.IntoSignature(),
                        AllPlayers = allPlayers
                    };
                }
                default:
                    return new EmptyContext();
            }
        }

        private void EnsureCommandNotNull()
        {
            if(SendOtherCommand is null)
            {
                throw new NullReferenceException(nameof(SendOtherCommand));
            }
            else if(SendAllCommand is null)
            {
                throw new NullReferenceException(nameof(SendAllCommand));
            }
            else if(SendUserCommand is null)
            {
                throw new NullReferenceException(nameof(SendUserCommand));
            }
        }
    }
}