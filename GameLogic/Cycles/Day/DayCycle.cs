using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic.Cycles.Day
{
    public class DayCycle
    {
        protected const string PLAYER_NOT_FOUND_EX = $"The player is dead or not exist";
        protected const string ELECTION_COUNTING_EX = "There is no superiority in votes. Election should proceed";

        private IVotable nonLynchObject;

        protected List<Player> alivePlayers;

        public bool IsBallotBegan { get; set; }
        public int DayNumber {  get; }
        public int NonLynchVotes => nonLynchObject.Votes;

        public DayCycle(List<Player> alivePlayers, int dayNumber)
        {
            this.alivePlayers = alivePlayers;
            DayNumber = dayNumber;
            nonLynchObject = new Penguin();
        }

        public bool VoteFor(IVoter voter, IVotable target)
        {
            if(!IsBallotBegan) return false;

            IVoter? v = alivePlayers.Find(a => voter.Id.Equals(a.Id));
            IVotable? t = alivePlayers.Find(a => target.Id.Equals(a.Id));

            IList<ArgumentException> exes = new List<ArgumentException>(2);
            if(v is null)
            {
                exes.Add(new ArgumentException(PLAYER_NOT_FOUND_EX, nameof(voter)));
            }
            if(t is null)
            {
                exes.Add(new ArgumentException(PLAYER_NOT_FOUND_EX, nameof(target)));
            }

            if(exes.Count > 0)
            {
                throw new AggregateException(exes);
            }

            return v!.VoteFor(t!);
        }

        public bool VoteForNonLynch(IVoter voter)
        {
            if(!IsBallotBegan) return false;

            IVoter? v = alivePlayers.Find(a => voter.Id.Equals(a.Id));
            if(v is null)
            {
                throw new ArgumentException(PLAYER_NOT_FOUND_EX, nameof(voter));
            }

            return v.VoteFor(nonLynchObject);
        }

        public void Unvote(IVoter voter)
        {
            if(!IsBallotBegan) return;

            IVoter? v = alivePlayers.Find(a => voter.Id.Equals(a.Id));
            if(v is null)
            {
                throw new ArgumentException(PLAYER_NOT_FOUND_EX, nameof(voter));
            }

            v.Unvote();
        }

        public IVotable GetElectionResult()
        {
            int max = 0;
            IVotable player = alivePlayers[0];
            for(int i = 0; i < alivePlayers.Count; i++)
            {
                if(alivePlayers[i].Votes > max)
                {
                    max = alivePlayers[i].Votes;
                    player = alivePlayers[i];
                }
            }

            if(NonLynchVotes > max)
            {
                max = NonLynchVotes;
            }

            const float P51 = 0.51F;
            float procent = (float)max / alivePlayers.Count;
            if(procent < P51)
            {
                throw new ArithmeticException(ELECTION_COUNTING_EX);
            }

            //Return null if penguin was elected
            if(NonLynchVotes > player.Votes)
            {
                player = nonLynchObject;
            }

            return player;
        }

        public bool TryGetElectionResult(out IVotable? result)
        {
            int max = 0;
            IVotable player = alivePlayers[0];
            for(int i = 0; i < alivePlayers.Count; i++)
            {
                if(alivePlayers[i].Votes > max)
                {
                    max = alivePlayers[i].Votes;
                    player = alivePlayers[i];
                }
            }

            if(NonLynchVotes > max)
            {
                max = NonLynchVotes;
            }

            const float P51 = 0.51F;
            float procent = (float)max / alivePlayers.Count;
            if(procent < P51)
            {
                result = null;
                return false;
            }

            //Return null if penguin was elected
            if(NonLynchVotes > player.Votes)
            {
                player = nonLynchObject;
            }

            result = player;
            return true;
        }

        /// <summary>
        /// Clears all election dependencies in this cycle.
        /// </summary>
        public void ClearVotes()
        {
            foreach(var p in alivePlayers)
            {
                p.Voters.Clear();
                p.VoteTarget = null;
            }
            nonLynchObject.Voters.Clear();
        }

        public static bool IsItPenguin(IVotable votable) => votable is Penguin;

        /// <summary>
        /// Merely a friendly bird which is voted to avoid any death at current election.
        /// </summary>
        private sealed class Penguin : IVotable
        {
            public ICollection<IVoter> Voters { get; set; }

            public ulong Id => 0L;
            public int Votes => Voters.Count;

            public string? LastMessage { get; set; } = null;
            public Role Role => throw new NotSupportedException();

            public Penguin()
            {
                Voters = new List<IVoter>();
            }

            public bool AddVoteFrom(IVoter from)
            {
                //There is already such voter or the player is dead
                if(Voters.Contains(from)) return false;

                Voters.Add(from);
                return true;
            }

            public void RemoveVote(IVoter voter)
            {
                Voters.Remove(voter);
            }

            public void Kill()
            {
                throw new NotSupportedException();
            }

            public void ChangeRole(Role role)
            {
                throw new NotSupportedException();
            }

            public void SetDeathReason(ITarget? killer)
            {
                throw new NotSupportedException();
            }
        }
    }
}