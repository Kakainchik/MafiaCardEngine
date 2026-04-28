using System.Windows.Media;
using WPFClientShell.Core;

namespace WPFClientShell.Model.Hub
{
    public class PlayerEntity : ObservableObject
    {
        private string nickname;
        private Color ncolor;
        private bool isAlive;
        private RoleVisual role;

        public long Id { get; }

        public string Nickname
        {
            get => nickname;
            set
            {
                nickname = value;
                OnPropertyChanged(nameof(Nickname));
            }
        }

        public Color NColor
        {
            get => ncolor;
            set
            {
                ncolor = value;
                OnPropertyChanged(nameof(NColor));
            }
        }

        public bool IsAlive
        {
            get => isAlive;
            set
            {
                isAlive = value;
                OnPropertyChanged(nameof(IsAlive));
            }
        }

        public RoleVisual Role
        {
            get => role;
            set
            {
                role = value;
                OnPropertyChanged(nameof(Role));
            }
        }

        public PlayerEntity(long id, string nickname, Color ncolor, bool isAlive, RoleVisual role)
        {
            this.Id = id;
            this.nickname = nickname;
            this.ncolor = ncolor;
            this.isAlive = isAlive;
            this.role = role;
        }
    }
}