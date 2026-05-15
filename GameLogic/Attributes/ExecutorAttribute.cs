namespace GameLogic.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ExecutorAttribute : Attribute
    {
        private readonly ExecutorType executorType;

        public ExecutorType EType => executorType;

        public int NumberAbilities { get; set; } = 1;

        public ExecutorAttribute(ExecutorType executorType)
        {
            this.executorType = executorType;

            if(this.executorType == ExecutorType.NONE)
            {
                NumberAbilities = 0;
            }
        }
    }

    public enum ExecutorType
    {
        NONE,
        TARGET,
        TARGET_TARGET,
        TARGET_ANYTARGET
    }
}
