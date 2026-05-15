using GameLogic.Attributes;
using GameLogic.Cycles.Night;
using GameLogic.Cycles.Night.Visitors;

namespace GameLogic.Roles
{
    /// <summary>
    /// Role of the cursed. Temporarily belongs to <see cref="Team.TOWN">Town</see>.
    /// If dies at night, will turn into <see cref="ZombieRole">Zombie</see>.
    /// </summary>
    [Executor(ExecutorType.NONE)]
    [Category(RoleCategory.GOVERMENT)]
    [Team(Team.TOWN)]
    public class CursedRole : Role
    {
        public override IEnumerable<ActionLog> AcceptVisitors()
        {
            foreach(ActionLog l in base.AcceptVisitors())
            {
                yield return l;
            }

            //If got dead - ressurect
            if(!IsAlive)
            {
                ResurrectVisitor ressurect = new ResurrectVisitor(this);
                yield return ressurect.VisitTarget(this);
            }
        }
    }
}