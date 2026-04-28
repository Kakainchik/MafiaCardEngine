namespace WPFClientShell.Core
{
    public abstract class BaseViewModel : ObservableObject
    {
        private bool isBusy;
        private string? busyReason;

        public virtual bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
        
        public virtual string? BusyReason
        {
            get => busyReason;
            set
            {
                busyReason = value;
                OnPropertyChanged(nameof(BusyReason));
            }
        }
    }
}