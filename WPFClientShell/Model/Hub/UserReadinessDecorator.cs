namespace WPFClientShell.Model.Hub
{
    public class UserReadinessDecorator : UserEntity
    {
        private bool isReady;

        public bool IsReady
        {
            get => isReady;
            set
            {
                isReady = value;
                OnPropertyChanged(nameof(IsReady));
            }
        }

        public UserReadinessDecorator(ulong id, string username) : base(id, username)
        {

        }

        public UserReadinessDecorator(ulong id, string username, bool isReady) : base(id, username)
        {
            this.isReady = isReady;
        }
    }
}