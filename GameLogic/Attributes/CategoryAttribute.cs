namespace GameLogic.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CategoryAttribute : Attribute
    {
        private readonly RoleCategory type;

        public RoleCategory Category => type;

        public CategoryAttribute(RoleCategory type)
        {
            this.type = type;
        }
    }

    public enum RoleCategory : byte
    {
        GOVERMENT,
        KILLING,
        SUPPORT,
        INVESTIGATE
    }
}