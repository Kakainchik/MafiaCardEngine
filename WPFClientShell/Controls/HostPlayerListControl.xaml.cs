using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPFClientShell.Model.Hub;

namespace WPFClientShell.Controls
{
    /// <summary>
    /// Interaction logic for HostPlayerListControl.xaml
    /// </summary>
    public partial class HostPlayerListControl : UserControl
    {
        public IEnumerable<UserReadinessDecorator> Players
        {
            get => (IEnumerable<UserReadinessDecorator>)GetValue(PlayersProperty);
            set => SetValue(PlayersProperty, value);
        }

        public ICommand PlayerKickedCommand
        {
            get => (ICommand)GetValue(PlayerKickedCommandProperty);
            set => SetValue(PlayerKickedCommandProperty, value);
        }

        public static readonly DependencyProperty PlayersProperty =
            DependencyProperty.Register(nameof(Players),
                typeof(IEnumerable<UserReadinessDecorator>),
                typeof(HostPlayerListControl));

        public static readonly DependencyProperty PlayerKickedCommandProperty =
            DependencyProperty.Register(nameof(PlayerKickedCommand),
                typeof(ICommand),
                typeof(HostPlayerListControl));

        public HostPlayerListControl()
        {
            InitializeComponent();
        }
    }
}