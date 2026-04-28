using GameLogic.Cycles.Night.Abilities;

namespace GameLogic.Interfaces
{
    public interface IExecutor : ITarget
    {
        IAbility GetAbility(int selectAbility, params ITarget[] targets);
    }
}