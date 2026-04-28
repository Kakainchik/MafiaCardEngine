using WPFClientShell.Model.Hub;

namespace WPFClientShell.Controls.Design
{
    internal class PlayerListControlDesign
    {
        public IEnumerable<UserReadinessDecorator> Players { get; }

        public PlayerListControlDesign()
        {
            Players = new List<UserReadinessDecorator>()
            {
                new UserReadinessDecorator(1, "Alice")
                {
                    IsReady = true
                },
                new UserReadinessDecorator(2, "Bob"),
                new UserReadinessDecorator(3, "Carol"),
                new UserReadinessDecorator(4, "David")
            };
        }
    }
}