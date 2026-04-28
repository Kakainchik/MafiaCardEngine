using GameLogic.Attributes;

namespace GameLogic.Roles
{
    /// <summary>
    /// Recruit role. Temporarily belongs to <see cref="Team.TOWN">Town</see>.
    /// No abilities. Recruits into <see cref="MafiaRole">Mafiosi</see>.
    /// </summary>
    [Executor(ExecutorType.NONE)]
    [Category(RoleCategory.GOVERMENT)]
    [Team(Team.TOWN)]
    public class RecruitRole : Role
    {

    }
}