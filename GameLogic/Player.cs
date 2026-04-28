using GameLogic.Interfaces;
using GameLogic.Roles;

namespace GameLogic
{
    public class Player : IVoter, IVotable, ILynch, IRoleOwner
    {
        #region Properties

        /// <summary>
        /// Unique id of player.
        /// </summary>
        public long Id { get; }
        public Role Role { get; private set; }
        public string? LastMessage { get; set; }
        public string? LastWill { get; set; }
        public IVotable? VoteTarget { get; set; }
        public ICollection<IVoter> Voters { get; set; }
        public ITarget? DeathReason { get; private set; }

        public bool IsAlive => Role.IsAlive;
        public int Votes => Voters.Count;

        #endregion

        public Player(long id, Role role)
        {
            Id = id;
            Role = role;
            Role.Owner = this;
            Voters = new List<IVoter>();
        }

        public bool VoteFor(IVotable whom)
        {
            //May not vote itself
            if(whom == this) return false;

            //Only alive player can vote
            if(!IsAlive) return false;

            //Remove vote from previous target
            if(VoteTarget != null && VoteTarget != whom)
            {
                VoteTarget.RemoveVote(this);
            }

            if(whom.AddVoteFrom(this))
            {
                VoteTarget = whom;
                return true;
            }
            else return false;
        }

        public void Unvote()
        {
            VoteTarget?.RemoveVote(this);
            VoteTarget = null;
        }

        public bool AddVoteFrom(IVoter from)
        {
            //There is already such voter or the player is dead
            if(Voters.Contains(from) || !IsAlive) return false;

            Voters.Add(from);
            return true;
        }

        public void RemoveVote(IVoter voter)
        {
            Voters.Remove(voter);
        }

        public void Lynch()
        {
            Role.IsAlive = false;
        }

        public void ChangeRole(Role role)
        {
            Role = role;
            Role.Owner = this;
        }

        public void SetDeathReason(ITarget? killer)
        {
            DeathReason = killer;
        }

        public override string ToString()
        {
            return $"Id = {Id}, IsAlive = {IsAlive}";
        }
    }
}