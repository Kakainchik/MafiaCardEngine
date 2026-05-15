using System.Windows.Media;

namespace WPFClientShell.Model.Hub
{
    public class PlayerVotableDecorator : PlayerEntity
    {
        private string? voteFor;
        private Color? targetColor;
        private int ownVotes;

        public ulong? VoteForId { get; set; }

        public string? VoteFor
        {
            get => voteFor;
            set
            {
                voteFor = value;
                OnPropertyChanged(nameof(VoteFor));
            }
        }

        public Color? TargetColor
        {
            get => targetColor;
            set
            {
                targetColor = value;
                OnPropertyChanged(nameof(TargetColor));
            }
        }

        public int OwnVotes
        {
            get => ownVotes;
            set
            {
                ownVotes = value;
                OnPropertyChanged(nameof(OwnVotes));
            }
        }

        public PlayerVotableDecorator(ulong id, string nickname,
            Color ncolor,
            bool isAlive,
            RoleVisual role) 
            : base(id, nickname, ncolor, isAlive, role)
        {

        }
    }
}