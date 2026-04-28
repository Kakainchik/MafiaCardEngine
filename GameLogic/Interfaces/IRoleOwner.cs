using GameLogic.Roles;

namespace GameLogic.Interfaces
{
    public interface IRoleOwner : IIdentifiable
    {
        Role Role { get; }

        void ChangeRole(Role role);
        void SetDeathReason(ITarget? killer);
    }
}