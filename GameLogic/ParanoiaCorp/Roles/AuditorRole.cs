using GameLogic.Roles;
using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// The role of an auditor. Belongs to <see cref="Team.CORPORATION">Corporation</see>.
    /// Every night can study one person, as a result of which will learn one's role completely, will learn it next shift.
    /// Sees the <see cref="ShadowDirectorRole"/> as an existing corporation member.
    /// Sees the <see cref="StartupFounderRole"/> as an Evangelist.
    /// </summary>
    [Executor(ExecutorType.TARGET)]
    [Team(Team.CORPORATION)]
    public class AuditorRole : Role, IExecutor
    {
        public IAbility GetAbility(int selectAbility, params ITarget[] targets)
        {
            return new InvestigateAbility(this, targets[0]);
        }
    }
}