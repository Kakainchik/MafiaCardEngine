using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFClientShell.Controls
{
    /// <summary>
    /// Interaction logic for HostPlayerListItemControl.xaml
    /// </summary>
    public partial class HostPlayerListItemControl : UserControl
    {
        public long PlayerId
        {
            get => (long)GetValue(PlayerIdProperty);
            set => SetValue(PlayerIdProperty, value);
        }

        public string Player
        {
            get => (string)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public bool IsReady
        {
            get => (bool)GetValue(IsReadyProperty);
            set => SetValue(IsReadyProperty, value);
        }

        public ICommand KickCommand
        {
            get => (ICommand)GetValue(KickCommandProperty);
            set => SetValue(KickCommandProperty, value);
        }

        public static readonly DependencyProperty PlayerIdProperty =
            DependencyProperty.Register(nameof(PlayerId),
                typeof(long),
                typeof(HostPlayerListItemControl));

        public static readonly DependencyProperty PlayerProperty =
            DependencyProperty.Register(nameof(Player),
                typeof(string),
                typeof(HostPlayerListItemControl));

        public static readonly DependencyProperty KickCommandProperty =
            DependencyProperty.Register(nameof(KickCommand),
                typeof(ICommand),
                typeof(HostPlayerListItemControl));

        public static readonly DependencyProperty IsReadyProperty =
            DependencyProperty.Register(nameof(IsReady),
                typeof(bool),
                typeof(HostPlayerListItemControl));

        public HostPlayerListItemControl()
        {
            InitializeComponent();
        }
    }
}