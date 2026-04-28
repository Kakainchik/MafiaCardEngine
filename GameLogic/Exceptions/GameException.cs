namespace GameLogic.Exceptions
{
    public class GameException : Exception
    {
        public GameException()
        {

        }

        public GameException(string message) : base(message)
        {

        }

        public GameException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}