using GameLogic.Roles;
using GameLogic.Attributes;
using GameLogic.Cycles.Night;
using GameLogic.ParanoiaCorp.Model.Visitors;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;

namespace GameLogic.ParanoiaCorp.Roles
{
    /// <summary>
    /// Role of the Layoff Candidate. Temporarily belongs to <see cref="Team.CORPORATION">Corporation</see>.
    /// If got layoffed at overnight, will turn into <see cref="OutsourcerRole">Outsourcer</see>.
    /// </summary>
    [Executor(ExecutorType.NONE)]
    [Team(Team.CORPORATION)]
    public class LayoffCandidateRole : Role
    {
        public override IEnumerable<ActionLog> AcceptVisitors()
        {
            foreach(ActionLog l in base.AcceptVisitors())
            {
                yield return l;
            }

            //If got layoffed - turn into Outsourcer
            if(!IsAlive)
            {
                ResurrectVisitor ressurect = new ResurrectVisitor(this);
                yield return ressurect.VisitTarget(this);
            }
        }
    }
}