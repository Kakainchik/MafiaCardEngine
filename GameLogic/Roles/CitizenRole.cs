using GameLogic.Attributes;

namespace GameLogic.Roles
{
    /// <summary>
    /// Role of a townsman. Belongs to <see cref="Team.TOWN">Town</see>.
    /// No abilities.
    /// </summary>
    [Executor(ExecutorType.NONE)]
    [Category(RoleCategory.GOVERMENT)]
    [Team(Team.TOWN)]
    public class CitizenRole : Role
    {

    }
}