using WebServer.Model.Game;
using WebServer.Model.Room;
using WebServer.Shared.GameObjects;
using WebServer.Shared.HubObjects;

namespace WebServer.Hubs
{
    public class ContextManager : IContextManager
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

        public ContextManager(IWaitRoomRepository roomRepository, IGameRepository gameRepository)
        {
            this.roomRepository = roomRepository;
            this.gameRepository = gameRepository;
        }

        public async Task ProcessContext(Context context)
        {
            EnsureCommandNotNull();

            switch(context)
            {
                case WaitingRoomReadyChangeContext wrrcc:
                {
                    roomRepository.SetReadiness(context.Presenter.RoomId, wrrcc.UserId, wrrcc.IsReady);
                    WaitingRoomReadyChangeContext result = wrrcc with
                    {
                        Presenter = LobbyHubConstants.ServerPresenter(context.Presenter.RoomId)
                    };
                    await SendAllCommand!.ExecuteAsync(result);
                    break;
                }
                case WaitingRoomRoleChangeContext wrrcc:
                {
                    roomRepository.SetRole(context.Presenter.RoomId, wrrcc.Role, wrrcc.Amount);
                    WaitingRoomRoleChangeContext result = wrrcc with
                    {
                        Presenter = LobbyHubConstants.ServerPresenter(context.Presenter.RoomId)
                    };
                    await SendOtherCommand!.ExecuteAsync(result);
                    break;
                }
                case WaitingRoomSeatsChangeContext wrscc:
                {
                    roomRepository.SetMaxSeats(context.Presenter.RoomId, wrscc.MaxSeats);
                    var result = wrscc with
                    {
                        Presenter = LobbyHubConstants.ServerPresenter(context.Presenter.RoomId)
                    };
                    await SendOtherCommand!.ExecuteAsync(result);
                    break;
                }
                case KickPlayerContext kpc:
                {
                    KickPlayerContext result = kpc with
                    {
                        Presenter = new Context.ContextPresenter
                        {
                            RoomId = context.Presenter.RoomId,
                            Receiver = kpc.UserToKickId
                        }
                    };

                    //Send the context only to kicked user
                    await SendUserCommand!.ExecuteAsync(result, result.Presenter.Receiver);
                    break;
                }
                case PlayerLoadedContext plc:
                {
                    //Player loaded the game data
                    GameHolder game = gameRepository.GetGame(plc.Presenter.RoomId);
                    game.Players.Single(p => p.Id == plc.Presenter.Sender).StartedGame = true;

                    gameRepository.Update(plc.Presenter.RoomId, game);

                    IEnumerable<string> nicknames = game.Players.Where(p => !p.StartedGame)
                        .Select(p => p.Nickname);

                    if(nicknames.Any())
                    {
                        PlayersLoadingContext response = new PlayersLoadingContext
                        {
                            Nicknames = nicknames.ToArray(),
                            Presenter = LobbyHubConstants.ServerPresenter(context.Presenter.RoomId)
                        };
                        await SendAllCommand!.ExecuteAsync(response);
                    }
                    else
                    {
                        await game.RunGameAsync();
                    }
                    
                    break;
                }
                case SendVoteContext svc:
                {
                    //Player send their vote
                    GameHolder game = gameRepository.GetGame(svc.Presenter.RoomId);
                    
                    if(game.Stage != StageType.DAY || game.Day is null)
                    {
                        return;
                    }

                    if(!svc.TargetId.HasValue)
                    {
                        //Someone clicked unvote
                        await game.Day.CancelVoteAsync(svc.Presenter.Sender);
                    }
                    else if(svc.TargetId.Value == 0L)
                    {
                        //Someone clicked on non-lynch object
                        await game.Day.VoteForNonLynchAsync(svc.Presenter.Sender);
                    }
                    else
                    {
                        //Someone clicked on player
                        await game.Day.VoteFromToAsync(svc.Presenter.Sender, svc.TargetId.Value);
                    }
                    break;
                }
                case SendLastMessageContext slmc:
                {
                    //Retreive the last message during the lynch
                    GameHolder game = gameRepository.GetGame(slmc.Presenter.RoomId);
                    game.Lynch?.ConfirmLastMessage(slmc.Message);

                    break;
                }
                case NightActionConfirmation nac:
                {
                    //Retreive the action during the night
                    GameHolder game = gameRepository.GetGame(nac.Presenter.RoomId);
                    game.Night?.ConfirmFlag(nac.Presenter.Sender, nac.AbilityFlag, nac.TargetIds);
                    break;
                }
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