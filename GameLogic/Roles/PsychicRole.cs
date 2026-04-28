using GameLogic.Attributes;

namespace GameLogic.Roles
{
    /// <summary>
    /// The role of a medium. Belongs to <see cref="Team.UNDEAD">Undead</see>.
    /// At night, can speak to the dead.
    /// Unique.
    /// </summary>
    [Unique]
    [Executor(ExecutorType.NONE)]
    [Category(RoleCategory.GOVERMENT)]
    [ChatScope(ChatScope.UNDEAD, CanWrite = true)]
    [ChatScope(ChatScope.DEAD, CanWrite = true)]
    [Team(Team.UNDEAD)]
    public class PsychicRole : Role
    {

    }
}