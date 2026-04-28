namespace GameLogic.Exceptions
{
    public class InitializingGameException : GameException
    {
        public InitializingGameException()
        {

        }

        public InitializingGameException(string message) : base(message)
        {

        }

        public InitializingGameException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
