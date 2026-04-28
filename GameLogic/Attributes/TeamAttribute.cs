namespace GameLogic.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TeamAttribute : Attribute
    {
        private readonly Team team;

        public TeamAttribute(Team team)
        {
            this.team = team;
        }

        public Team ItsTeam => team;
    }

    /// <summary>
    /// The team enumerator to indicate which role belongs to which side.
    /// </summary>
    public enum Team
    {
        TOWN,
        MAFIA,
        CULTUS,
        UNDEAD,
        SERIAL_KILLER,
        WITCH,
        TERRORIST
    }
}