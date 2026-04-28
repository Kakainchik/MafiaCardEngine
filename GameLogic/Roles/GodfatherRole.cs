using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;

namespace GameLogic.Roles
{
    /// <summary>
    /// Godfather role. Belongs to <see cref="Team.MAFIA">Mafia</see>.
    /// Every night orders <see cref="MafiaRole">Mafiosi</see> whom to kill,
    /// Can also recruit  <see cref="RecruitRole">Recruits</see> and <see cref="ProstituteRole">Prostitutes</see>.
    /// Cannot be killed at night.
    /// Unique.
    /// </summary>
    [Unique]
    [Executor(ExecutorType.TARGET, NumberAbilities = 3)]
    [Category(RoleCategory.GOVERMENT)]
    [ChatScope(ChatScope.MAFIA, CanWrite = true)]
    [Team(Team.MAFIA)]
    public class GodfatherRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility = 1, params ITarget[] targets)
        {
            switch(selectAbility)
            {
                case 1:
                {
                    return new RecruiterAbility(this, targets[0]);
                }
                case 2:
                {
                    return new GodfatherOrderAbility(this, targets[0], GodfatherOrderAbility.OrderTarget.MAFIA);
                }
                case 3:
                {
                    return new GodfatherOrderAbility(this, targets[0], GodfatherOrderAbility.OrderTarget.COUNSELOR);
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(selectAbility), "No ability by given number.");
                }
            }
        }
    }
}