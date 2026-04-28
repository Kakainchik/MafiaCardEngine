using GameLogic.Attributes;

namespace GameLogic.Roles
{
    /// <summary>
    /// Role of a cultist. Belongs to <see cref="Team.CULTUS">Cult</see>.
    /// At night, can talk to other cultists, but the identity of the Leader remains a secret.
    /// </summary>
    [Executor(ExecutorType.NONE)]
    [Category(RoleCategory.GOVERMENT)]
    [ChatScope(ChatScope.CULTUS, CanWrite = true)]
    [Team(Team.CULTUS)]
    public class CultistRole : Role
    {

    }
}