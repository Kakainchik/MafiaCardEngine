using WPFClientShell.Model.Hub;

namespace WPFClientShell.Controls.Design
{
    internal class MutableRoleListItemControlDesign
    {
        public RoleVisual Role { get; }
        public int Quantity { get; }

        public MutableRoleListItemControlDesign()
        {
            Role = RoleVisual.DETECTIVE;
            Quantity = 0;
        }
    }
}
