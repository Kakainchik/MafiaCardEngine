using Swordfish.NET.Collections;
using WPFClientShell.Model.Hub;

namespace WPFClientShell.Controls.Design
{
    internal class MutableRoleListControlDesign
    {
        public IEnumerable<Enum> Source { get; }
        public ConcurrentObservableDictionary<Enum, int> SelectedItems { get; }

        public MutableRoleListControlDesign()
        {
            //Source
            Array constants = typeof(RoleVisual).GetEnumValues();
            Source = new List<Enum>(constants.OfType<Enum>());

            //SelectedItems
            SelectedItems = new ConcurrentObservableDictionary<Enum, int>();
            foreach(Enum item in Source)
            {
                SelectedItems.Add(item, Random.Shared.Next(3));
            }
        }
    }
}