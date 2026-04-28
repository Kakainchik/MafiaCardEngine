using WPFClientShell.Core;

namespace WPFClientShell.Model.Hub
{
    public class UserEntity : ObservableObject
    {
        public long Id { get; }
        public string Username { get; }

        public UserEntity(long id, string username)
        {
            Id = id;
            Username = username;
        }

        public override bool Equals(object? obj)
        {
            if(obj is null || obj is not UserEntity)
            {
                return false;
            }

            return ((UserEntity)obj).Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}