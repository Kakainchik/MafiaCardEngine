using System.Windows.Media;
using WPFClientShell.Core;

namespace WPFClientShell.Model.Hub
{
    public class NightTargetEntity : ObservableObject
    {
        private long? targetId;
        private string? nickname;
        private Color? nColor;

        public int Number { get; }

        public long? TargetId
        {
            get => targetId;
            set
            {
                targetId = value;
                OnPropertyChanged(nameof(TargetId));
            }
        }

        public string? Nickname
        {
            get => nickname;
            set
            {
                nickname = value;
                OnPropertyChanged(nameof(Nickname));
            }
        }

        public Color? NColor
        {
            get => nColor;
            set
            {
                nColor = value;
                OnPropertyChanged(nameof(NColor));
            }
        }

        public NightTargetEntity(int number)
        {
            Number = number;
        }

        public void SetTarget(PlayerEntity? target)
        {
            TargetId = target?.Id;
            Nickname = target?.Nickname;
            NColor = target?.NColor;
        }

        public void ClearTarget()
        {
            TargetId = null;
            Nickname = null;
            NColor = null;
        }
    }
}