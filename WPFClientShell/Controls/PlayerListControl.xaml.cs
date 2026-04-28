using System.Windows;
using System.Windows.Controls;
using WPFClientShell.Model.Hub;

namespace WPFClientShell.Controls
{
    /// <summary>
    /// Interaction logic for PlayerListControl.xaml
    /// </summary>
    public partial class PlayerListControl : UserControl
    {
        public IEnumerable<UserReadinessDecorator> Players
        {
            get => (IEnumerable<UserReadinessDecorator>)GetValue(PlayersProperty);
            set => SetValue(PlayersProperty, value);
        }

        public static readonly DependencyProperty PlayersProperty =
            DependencyProperty.Register(nameof(Players),
                typeof(IEnumerable<UserReadinessDecorator>),
                typeof(PlayerListControl));

        public PlayerListControl()
        {
            InitializeComponent();
        }
    }
}