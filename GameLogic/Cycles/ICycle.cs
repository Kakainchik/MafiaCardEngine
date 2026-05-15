namespace GameLogic.Cycles
{
    public interface ICycle
    {
        bool CanFinish();
        ICycle NextCycle();
    }
}