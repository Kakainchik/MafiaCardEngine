using System.Windows;
using System.Windows.Controls;
using WPFClientShell.Model.Hub;

namespace WPFClientShell.Controls
{
    /// <summary>
    /// Interaction logic for RoleListItemControl.xaml
    /// </summary>
    public partial class RoleListItemControl : UserControl
    {
        public RoleVisual Role
        {
            get => (RoleVisual)GetValue(RoleProperty);
            set => SetValue(RoleProperty, value);
        }

        public int Quantity
        {
            get => (int)GetValue(QuantityProperty);
            set => SetValue(QuantityProperty, value);
        }

        public static readonly DependencyProperty RoleProperty =
            DependencyProperty.Register(nameof(Role),
                typeof(RoleVisual),
                typeof(RoleListItemControl));

        public static readonly DependencyProperty QuantityProperty =
            DependencyProperty.Register(nameof(Quantity),
                typeof(int),
                typeof(RoleListItemControl));

        public RoleListItemControl()
        {
            InitializeComponent();
        }
    }
}