using GameLogic.Interfaces;
using GameLogic;
using WebServer.Hubs;
using WebServer.Shared.HubObjects;
using GameLogic.Cycles.Day;
using Timer = System.Timers.Timer;
using System.Timers;

namespace WebServer.Model.Game
{
    public class DayStage
    {
        private const short STEP_INTERVAL = 5;
        private const short MEETING_DISCUSS_TIME = 30;
        private const short ELECTION_TIME = 20;

        private readonly Timer timer;
        private readonly Timer electionTimer;
        private readonly DayCycle day;
        private readonly IReadOnlyList<Player> players;
        private readonly IHubCommand sendAllCommand;
        private readonly int roomId;

        public IVotable? GonnaElected { get; private set; }

        public DayStage(DayCycle day, IReadOnlyList<Player> players, IHubCommand sendAllCommand, int roomId)
        {
            this.day = day;
            this.players = players;
            this.sendAllCommand = sendAllCommand;
            this.roomId = roomId;

            TimeSpan span = TimeSpan.FromSeconds(STEP_INTERVAL);
            timer = new Timer(span);

            TimeSpan electionSpan = TimeSpan.FromSeconds(ELECTION_TIME);
            electionTimer = new Timer(electionSpan);
        }

        public event EventHandler? DayEnded;

        public void Run()
        {
            timer.Elapsed += StartDay;
            timer.Start();
        }

        public async Task VoteFromToAsync(long voter, long votable)
        {
            Player voterP = players.Single(p => p.Id == voter);
            Player votableP = players.Single(p => p.Id == votable);

            if(!voterP.IsAlive || !votableP.IsAlive)
            {
                return;
            }

            //Get the previous voting target if any
            IVotable? previousVotable = voterP.VoteTarget;
            ReceiveVoteContext.VoteTarget? previous = null;
            if(previousVotable is not null)
            {
                previous = new ReceiveVoteContext.VoteTarget
                {
                    TargetId = previousVotable.Id,
                    VotesNumber = previousVotable.Votes
                };
            }

            bool success = day?.VoteFor(voterP, votableP!) ?? false;

            if(success)
            {
                ReceiveVoteContext.VoteTarget current = new ReceiveVoteContext.VoteTarget
                {
                    TargetId = votableP.Id,
                    VotesNumber = votableP.Votes
                };

                ReceiveVoteContext context = new ReceiveVoteContext
                {
                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                    Voter = voter,
                    PreviousTarget = previous,
                    CurrentTarget = current
                };

                await sendAllCommand.ExecuteAsync(context);

                await EnsureResultAsync();
            }
        }

        public async Task VoteForNonLynchAsync(long voter)
        {
            Player voterP = players.Single(p => p.Id == voter);

            if(!voterP.IsAlive)
            {
                return;
            }

            //Get the previous voting target if any
            IVotable? previousVotable = voterP.VoteTarget;
            ReceiveVoteContext.VoteTarget? previous = null;
            if(previousVotable is not null)
            {
                previous = new ReceiveVoteContext.VoteTarget
                {
                    TargetId = previousVotable.Id,
                    VotesNumber = previousVotable.Votes
                };
            }

            bool success = day?.VoteForNonLynch(voterP) ?? false;

            if(success)
            {
                ReceiveVoteContext.VoteTarget current = new ReceiveVoteContext.VoteTarget
                {
                    TargetId = 0L,
                    VotesNumber = day!.NonLynchVotes
                };

                ReceiveVoteContext context = new ReceiveVoteContext
                {
                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                    Voter = voter,
                    PreviousTarget = previous,
                    CurrentTarget = current
                };

                await sendAllCommand.ExecuteAsync(context);

                await EnsureResultAsync();
            }
        }

        public async Task CancelVoteAsync(long voter)
        {
            Player voterP = players.Single(p => p.Id == voter);

            if(!voterP.IsAlive)
            {
                return;
            }

            //Get the previous voting target if any
            IVotable? previousVotable = voterP.VoteTarget;
            ReceiveVoteContext.VoteTarget? previous = null;
            if(previousVotable is not null)
            {
                previous = new ReceiveVoteContext.VoteTarget
                {
                    TargetId = previousVotable.Id,
                    VotesNumber = previousVotable.Votes
                };
            }

            day?.Unvote(voterP);

            ReceiveVoteContext context = new ReceiveVoteContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Voter = voter,
                PreviousTarget = previous,
                CurrentTarget = null
            };

            await sendAllCommand.ExecuteAsync(context);

            await EnsureResultAsync();
        }

        private async Task EnsureResultAsync()
        {
            IVotable? result;
            if(day.TryGetElectionResult(out result))
            {
                if(GonnaElected == result)
                {
                    //Result is the same, do not notify players
                    return;
                }

                //Notify players about new result and reset timer
                GonnaElected = result;

                TimerContext tc = new TimerContext
                {
                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                    Seconds = ELECTION_TIME,
                    ToStart = true
                };
                await sendAllCommand.ExecuteAsync(tc);

                ResetElectionTimer(TimeSpan.FromSeconds(ELECTION_TIME));

                WarningVoteContext wvc = new WarningVoteContext
                {
                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                    VotedId = result!.Id
                };
                await sendAllCommand.ExecuteAsync(wvc);
            }
            else
            {
                //Proceed election, no result
                GonnaElected = null;

                electionTimer.Stop();

                //Stop timer
                TimerContext tc = new TimerContext
                {
                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                    Seconds = default,
                    ToStart = false
                };
                await sendAllCommand.ExecuteAsync(tc);

                WarningVoteContext wvc = new WarningVoteContext
                {
                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                    VotedId = null
                };
                await sendAllCommand.ExecuteAsync(wvc);
            }
        }

        private void StartDay(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= StartDay;

            DayStepContext dayContext = new DayStepContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Step = DayStepContext.DayStep.START_DAY
            };
            sendAllCommand.Execute(dayContext);

            Thread.Sleep(1_000);

            TimerContext timerContext = new TimerContext
            {
                Presenter = LobbyHubConstants.ServerPresenter(roomId),
                Seconds = MEETING_DISCUSS_TIME,
                ToStart = true
            };
            sendAllCommand.Execute(timerContext);

            timer.Interval = TimeSpan.FromSeconds(MEETING_DISCUSS_TIME).TotalMilliseconds;
            timer.Elapsed += StartBallot;
            timer.Start();
        }

        private void StartBallot(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= StartBallot;
            timer.Interval = TimeSpan.FromSeconds(STEP_INTERVAL).TotalMilliseconds;

            //As game just started there is not elections
            if(day.DayNumber == 1)
            {
                DayStepContext context = new DayStepContext()
                {
                    Presenter = LobbyHubConstants.ServerPresenter(roomId),
                    Step = DayStepContext.DayStep.FIRST_DAY_CASE
                };
                sendAllCommand.Execute(context);

                //Next turn
                timer.Elapsed += EndBallot;
                timer.Start();
            }
            else
            {
                day.IsBallotBegan = true;

                DayStepContext context = new DayStepContext
                {
                    Step = DayStepContext.DayStep.START_BALLOT
                };
                sendAllCommand.Execute(context);
            }
        }

        private void EndBallot(object? sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Elapsed -= EndBallot;
            timer.Dispose();

            electionTimer?.Dispose();

            DayEnded?.Invoke(this, EventArgs.Empty);
        }

        private void ResetElectionTimer(TimeSpan interval)
        {
            electionTimer.Close();
            electionTimer.Elapsed -= ElectionTimer_Elapsed;

            electionTimer.Interval = interval.TotalMilliseconds;

            electionTimer.Elapsed += ElectionTimer_Elapsed;
            electionTimer.Start();
        }

        private void ElectionTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            day.IsBallotBegan = false;

            electionTimer.Stop();
            electionTimer.Dispose();

            timer.Elapsed += EndBallot;
            timer.Start();
        }
    }
}