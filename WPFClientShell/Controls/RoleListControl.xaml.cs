using System.Windows;
using System.Windows.Controls;
using WPFClientShell.Model.Hub;

namespace WPFClientShell.Controls
{
    /// <summary>
    /// Interaction logic for RoleListControl.xaml
    /// </summary>
    public partial class RoleListControl : UserControl
    {
        public IDictionary<RoleVisual, int> SelectedRoles
        {
            get => (IDictionary<RoleVisual, int>)GetValue(SelectedRolesProperty);
            set => SetValue(SelectedRolesProperty, value);
        }

        public static readonly DependencyProperty SelectedRolesProperty =
            DependencyProperty.Register(nameof(SelectedRoles),
                typeof(IDictionary<RoleVisual, int>),
                typeof(RoleListControl),
                new PropertyMetadata(new Dictionary<RoleVisual, int>()));

        public RoleListControl()
        {
            InitializeComponent();
        }
    }
}