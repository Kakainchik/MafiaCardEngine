using GameLogic.Attributes;

namespace GameLogic.Roles
{
    /// <summary>
    /// Role of a Mason. Belongs to <see cref="Team.TOWN">Town</see>.
    /// Can communicate with other Masons at night.
    /// </summary>
    [Executor(ExecutorType.NONE)]
    [Category(RoleCategory.GOVERMENT)]
    [ChatScope(ChatScope.MASON, CanWrite = true)]
    [Team(Team.TOWN)]
    public class MasonRole : Role
    {

    }
}