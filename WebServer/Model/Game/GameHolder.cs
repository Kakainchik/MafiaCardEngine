using GameLogic.Attributes;
using WebServer.Hubs;
using WebServer.Shared.Extensions;
using WebServer.Shared.GameObjects;
using WebServer.Shared.HubObjects;

namespace WebServer.Model.Game
{
    public class GameHolder
    {
        private readonly IHubCommand sendAllCommand;
        private readonly IHubIdCommand sendUserCommand;
        private readonly int roomId;

        private IntroStage intro;
        private StageType stage;

        public DayStage? Day { get; private set; }
        public LynchStage? Lynch { get; private set; }
        public NightStage? Night { get; private set; }
        public MorningStage? Morning { get; private set; }
        public GameLogic.Game GameInstance { get; }
        public IReadOnlyList<PlayerDomain> Players { get; }
        
        public StageType Stage
        {
            get
            {
                if(GameInstance.DayCycle is not null)
                {
                    return StageType.DAY;
                }
                else if(GameInstance.LynchCycle is not null)
                {
                    return StageType.LYNCH;
                }
                else if(GameInstance.NightCycle is not null)
                {
                    return StageType.NIGHT;
                }
                else if(GameInstance.MorningCycle is not null)
                {
                    return StageType.MORNING;
                }
                else
                {
                    return stage;
                }
            }
            private set => stage = value;
        }

        public GameHolder(IHubCommand sendAllCommand, IHubIdCommand sendUserCommand, PlayerDomain[] players, int roomId)
        {
            this.sendAllCommand = sendAllCommand;
            this.sendUserCommand = sendUserCommand;
            this.roomId = roomId;

            Stage = StageType.LOADING;
            GameInstance = new GameLogic.Game(players.Select(d => d.GameRepresentation).ToArray());
            Players = players;

            intro = new IntroStage(roomId, sendAllCommand);

            intro.IntroEnded += Intro_IntroEnded;
        }

        public async Task RunGameAsync()
        {
            Stage = StageType.INTRO;
            await SendCycleInfoToAllAsync();

            intro.Run();
        }

        private void SendCycleInfoToAll()
        {
            CycleStateContext context = new CycleStateContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Cycle = Stage,
                IsAlive = default,
                Role = default
            };

            //Send personal info to all
            for(int i = 0; i < Players.Count; i++)
            {
                context = context with
                {
                    Presenter = context.Presenter with
                    {
                        Receiver = Players[i].Id
                    },
                    IsAlive = Players[i].GameRepresentation.IsAlive,
                    Role = Players[i].GameRepresentation.Role.IntoSignature()
                };
                sendUserCommand.Execute(context, Players[i].Id);
            }
        }

        private async Task SendCycleInfoToAllAsync()
        {
            CycleStateContext context = new CycleStateContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Cycle = Stage,
                IsAlive = default,
                Role = default
            };

            //Send personal info to all
            Task[] userTasks = new Task[Players.Count];
            for(int i = 0; i < Players.Count; i++)
            {
                context = context with
                {
                    Presenter = context.Presenter with
                    {
                        Receiver = Players[i].Id
                    },
                    IsAlive = Players[i].GameRepresentation.IsAlive,
                    Role = Players[i].GameRepresentation.Role.IntoSignature()
                };
                userTasks[i] = sendUserCommand.ExecuteAsync(context, Players[i].Id);
            }

            await Task.WhenAll(userTasks);
        }

        private void MakeNewTurn()
        {
            GameInstance.NextTurn();

            if(GameInstance.DayCycle is not null)
            {
                Day = new DayStage(GameInstance.DayCycle, GameInstance.Players, sendAllCommand, roomId);
                Day.DayEnded += Day_DayEnded;

                SendCycleInfoToAll();

                DayPlayerDataContext dpdc = new DayPlayerDataContext
                {
                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                    DayPlayers = Players.Select(p =>
                    {
                        return new DayPlayerDataContext.DayPlayerInstance
                        {
                            Id = p.Id,
                            IsAlive = p.GameRepresentation.IsAlive
                        };
                    }).ToArray()
                };
                sendAllCommand.Execute(dpdc);

                Day.Run();
            }
            else if(GameInstance.LynchCycle is not null)
            {
                Lynch = new LynchStage(GameInstance.LynchCycle, sendAllCommand, sendUserCommand, roomId);
                Lynch.LynchEnded += Lynch_LynchEnded;

                SendCycleInfoToAll();

                PlayerDomain elected = Players.Single(p => p.Id == GameInstance.LynchCycle.Elected.Id);
                LynchPlayerContext lpc = new LynchPlayerContext
                {
                    PlayerId = elected.Id,
                    Nickname = elected.Nickname,
                    NColor = elected.NColor,
                    Role = elected.GameRepresentation.Role.IntoSignature()
                };
                sendAllCommand.Execute(lpc);

                Lynch.Run();
            }
            else if(GameInstance.NightCycle is not null)
            {
                Night = new NightStage(GameInstance.NightCycle, GameInstance.Players, sendAllCommand, sendUserCommand, roomId);
                Night.NightEnded += Night_NightEnded;

                SendCycleInfoToAll();

                NightPlayerDataContext npdc = new NightPlayerDataContext
                {
                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                    NightPlayers = Players.Select(p =>
                    {
                        return new NightPlayerDataContext.NightPlayerInstance
                        {
                            Id = p.Id,
                            IsAlive = p.GameRepresentation.IsAlive
                        };
                    }).ToArray()
                };
                sendAllCommand.Execute(npdc);

                Night.Run();
            }
            else if(GameInstance.MorningCycle is not null)
            {
                Morning = new MorningStage(GameInstance.MorningCycle, sendAllCommand, roomId);
                Morning.MorningEnded += Morning_MorningEnded;

                SendCycleInfoToAll();

                NightPlayerDataContext npdc = new NightPlayerDataContext
                {
                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                    NightPlayers = Players.Select(p =>
                    {
                        return new NightPlayerDataContext.NightPlayerInstance
                        {
                            Id = p.Id,
                            IsAlive = p.GameRepresentation.IsAlive
                        };
                    }).ToArray()
                };
                sendAllCommand.Execute(npdc);

                Morning.Run();
            }
            else
            {
                return;
            }
        }

        private void Intro_IntroEnded(object? sender, EventArgs e)
        {
            intro.IntroEnded -= Intro_IntroEnded;

            GameInstance.GameEnded += GameInstance_GameEnded;

            //Start first day
            GameInstance.Run();
            MakeNewTurn();
        }

        private void Day_DayEnded(object? sender, EventArgs e)
        {
            DayStage stage = (DayStage)sender!;
            stage.DayEnded -= Day_DayEnded;

            MakeNewTurn();
        }

        private void Lynch_LynchEnded(object? sender, EventArgs e)
        {
            LynchStage stage = (LynchStage)sender!;
            stage.LynchEnded -= Lynch_LynchEnded;

            MakeNewTurn();
        }

        private void Night_NightEnded(object? sender, EventArgs e)
        {
            NightStage stage = (NightStage)sender!;
            stage.NightEnded -= Night_NightEnded;

            MakeNewTurn();
        }

        private void Morning_MorningEnded(object? sender, EventArgs e)
        {
            MorningStage stage = (MorningStage)sender!;
            stage.MorningEnded -= Morning_MorningEnded;

            MakeNewTurn();
        }

        private void GameInstance_GameEnded(object? sender, Team? e)
        {
            GameInstance.GameEnded -= GameInstance_GameEnded;

            Stage = StageType.WIN;
            SendCycleInfoToAll();

            var endPlayers = new EndGameContext.EndGamePlayerState[Players.Count];
            for(int i = 0; i < endPlayers.Length; i++)
            {
                PlayerDomain player = Players[i];

                endPlayers[i] = new()
                {
                    Id = player.Id,
                    Nickname = player.Nickname,
                    NColor = player.NColor,
                    Role = player.GameRepresentation.Role.IntoSignature(),
                    IsAlive = player.GameRepresentation.IsAlive
                };
            }

            var history = new EndGameContext.EndGameHistory[GameInstance.Day];

            EndGameContext context = new EndGameContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Winner = e,
                Players = endPlayers,
                History = history
            };
        }
    }
}