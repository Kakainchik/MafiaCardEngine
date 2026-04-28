using GameLogic.Attributes;
using GameLogic.Interfaces;
using GameLogic.Roles;
using System.Reflection;
using WebServer.Shared.Attributes;
using WebServer.Shared.GameObjects;

namespace WebServer.Shared.Extensions
{
    public static class RoleExtension
    {
        private const string ROLE_VALID_ERROR = "Such role is not valid.";

        public static Type GetRoleType(this RoleSignature signature)
        {
            FieldInfo? fi = signature.GetType().GetField(signature.ToString());
            RoleTypeAttribute? attr = fi?.GetCustomAttribute<RoleTypeAttribute>();

            return attr?.RoleType ??
                throw new CustomAttributeFormatException("The signature is not associated to any role.");
        }

        /// <summary>
        /// Convert App role enumerator into Logic Game Role instance.
        /// </summary>
        /// <returns>Logic role instance.</returns>
        public static Role MakeGameRole(this RoleSignature signature)
        {
            Type roleType = signature.GetRoleType();
            ConstructorInfo? ci = roleType.GetConstructor(Type.EmptyTypes);

            return (Role?)ci?.Invoke(null) ??
                throw new ArgumentException(ROLE_VALID_ERROR, nameof(signature));
        }

        /// <summary>
        /// Represent Role class as App enumerator of roles.
        /// </summary>
        /// <returns>Role signature value.</returns>
        public static RoleSignature IntoSignature(this ITarget role)
        {
            return role switch
            {
                CitizenRole => RoleSignature.CITIZEN,
                CursedRole => RoleSignature.CURSED,
                DetectiveRole => RoleSignature.DETECTIVE,
                DoctorRole => RoleSignature.DOCTOR,
                DriverRole => RoleSignature.DRIVER,
                MasonRole => RoleSignature.MASON,
                PolicemanRole => RoleSignature.POLICEMAN,
                ProstituteRole => RoleSignature.PROSTITUTE,
                RecruitRole => RoleSignature.RECRUIT,
                VigilanteRole => RoleSignature.VIGILANTE,
                CounselorRole => RoleSignature.COUNSELOR,
                GodfatherRole => RoleSignature.GODFATHER,
                MafiaRole => RoleSignature.MAFIA,
                SurgeonRole => RoleSignature.SURGEON,
                WhoreRole => RoleSignature.WHORE,
                CultistRole => RoleSignature.CULTIST,
                CultusLeaderRole => RoleSignature.CULTUS_LEADER,
                SerialKillerRole => RoleSignature.SERIAL_KILLER,
                TerroristRole => RoleSignature.TERRORIST,
                WitchRole => RoleSignature.WITCH,
                PsychicRole => RoleSignature.PSYCHIC,
                ZombieRole => RoleSignature.ZOMBIE,
                _ => throw new ArgumentException(ROLE_VALID_ERROR, nameof(role))
            };
        }


        public static Team GetTeam(this ITarget target)
        {
            TeamAttribute? teamAttr = target.GetType().GetCustomAttribute<TeamAttribute>();

            return teamAttr?.ItsTeam ?? default;
        }


        public static Team GetTeam(this RoleSignature signature)
        {
            Type roleType = signature.GetRoleType();
            TeamAttribute? teamAttr = roleType.GetCustomAttribute<TeamAttribute>();

            return teamAttr?.ItsTeam ?? default;
        }

        public static ChatScopeAttribute[] GetChatScopes(this RoleSignature signature)
        {
            Type roleType = signature.GetRoleType();
            ChatScopeAttribute[] attrs = (ChatScopeAttribute[])roleType.GetCustomAttributes(typeof(ChatScopeAttribute), false);

            return attrs;
        }

        public static ExecutorAttribute GetExecutorAttribute(this RoleSignature signature)
        {
            Type roleType = signature.GetRoleType();
            ExecutorAttribute? attr = roleType.GetCustomAttribute<ExecutorAttribute>();

            if(attr is null)
            {
                throw new CustomAttributeFormatException("This role does not contain an Executor attribute");
            }

            return attr;
        }
    }
}