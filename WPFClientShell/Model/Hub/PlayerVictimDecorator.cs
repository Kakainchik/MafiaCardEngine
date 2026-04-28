using System.Windows.Media;

namespace WPFClientShell.Model.Hub
{
    public class PlayerVictimDecorator : PlayerEntity
    {
        private string? lastWill;

        public string? LastWill
        {
            get => lastWill;
            set
            {
                lastWill = value;
                OnPropertyChanged(nameof(LastWill));
            }
        }

        public PlayerVictimDecorator(long id,
            string nickname,
            Color ncolor,
            bool isAlive,
            RoleVisual role,
            string? lastWill)
            : base(id, nickname, ncolor, isAlive, role)
        {
            this.lastWill = lastWill;
        }
    }
}