using GameLogic.Interfaces;
using GameLogic.Attributes;
using GameLogic.ParanoiaCorp.Roles;
using System.Reflection;
using WebServer.Shared.Attributes;
using WebServer.Shared.ParanoiaCorp.GameObjects;
using Role = GameLogic.Roles.Role;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;
using ChatScopeAttribute = GameLogic.ParanoiaCorp.Attributes.ChatScopeAttribute;

namespace WebServer.Shared.ParanoiaCorp.Extensions
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

        public static bool IsUnique(this RoleSignature signature)
        {
            Type roleType = signature.GetRoleType();
            UniqueAttribute? attr = roleType.GetCustomAttribute<UniqueAttribute>();
            return attr != null;
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

        public static RoleSignature IntoSignature(this Type roleType)
        {
            if(roleType == typeof(GeneralDirectorRole)) return RoleSignature.GENERAL_DIRECTOR;
            if(roleType == typeof(ClerkRole)) return RoleSignature.CLERK;
            if(roleType == typeof(SecuritySpecialistRole)) return RoleSignature.SECURITY_SPECIALIST;
            if(roleType == typeof(AuditorRole)) return RoleSignature.AUDITOR;
            if(roleType == typeof(HRManagerRole)) return RoleSignature.HR_MANAGER;
            if(roleType == typeof(SystemAdministratorRole)) return RoleSignature.SYSTEM_ADMINISTRATOR;
            if(roleType == typeof(AnticrisisManagerRole)) return RoleSignature.ANTICRISIS_MANAGER;
            if(roleType == typeof(InternRole)) return RoleSignature.INTERN;
            if(roleType == typeof(ScrumMasterRole)) return RoleSignature.SCRUM_MASTER;
            if(roleType == typeof(ShareholderRole)) return RoleSignature.SHAREHOLDER;
            if(roleType == typeof(LayoffCandidateRole)) return RoleSignature.LAYOFF_CANDIDATE;
            if(roleType == typeof(MasterOfCompromisingRole)) return RoleSignature.MASTER_OF_COMPROMISING;
            if(roleType == typeof(SpyRole)) return RoleSignature.SPY;
            if(roleType == typeof(HackerRole)) return RoleSignature.HACKER;
            if(roleType == typeof(ShadowDirectorRole)) return RoleSignature.SHADOW_DIRECTOR;
            if(roleType == typeof(StartupFounderRole)) return RoleSignature.STARUP_FOUNDER;
            if(roleType == typeof(EvangelistRole)) return RoleSignature.EVANGELIST;
            if(roleType == typeof(RaiderRole)) return RoleSignature.RAIDER;
            if(roleType == typeof(PoliticalStrategistRole)) return RoleSignature.POLITICAL_STRATEGIST;
            if(roleType == typeof(OutsourcerRole)) return RoleSignature.OUTSOURCER;
            if(roleType == typeof(AlumniManagerRole)) return RoleSignature.ALUMNI_MANAGER;
            throw new ArgumentException(ROLE_VALID_ERROR, nameof(roleType));
        }

        /// <summary>
        /// Represent Role class as App enumerator of roles.
        /// </summary>
        /// <returns>Role signature value.</returns>
        public static RoleSignature IntoSignature(this ITarget role)
        {
            return role switch
            {
                GeneralDirectorRole => RoleSignature.GENERAL_DIRECTOR,
                ClerkRole => RoleSignature.CLERK,
                SecuritySpecialistRole => RoleSignature.SECURITY_SPECIALIST,
                AuditorRole => RoleSignature.AUDITOR,
                HRManagerRole => RoleSignature.HR_MANAGER,
                SystemAdministratorRole => RoleSignature.SYSTEM_ADMINISTRATOR,
                AnticrisisManagerRole => RoleSignature.ANTICRISIS_MANAGER,
                InternRole => RoleSignature.INTERN,
                ScrumMasterRole => RoleSignature.SCRUM_MASTER,
                ShareholderRole => RoleSignature.SHAREHOLDER,
                LayoffCandidateRole => RoleSignature.LAYOFF_CANDIDATE,
                MasterOfCompromisingRole => RoleSignature.MASTER_OF_COMPROMISING,
                SpyRole => RoleSignature.SPY,
                HackerRole => RoleSignature.HACKER,
                ShadowDirectorRole => RoleSignature.SHADOW_DIRECTOR,
                StartupFounderRole => RoleSignature.STARUP_FOUNDER,
                EvangelistRole => RoleSignature.EVANGELIST,
                RaiderRole => RoleSignature.RAIDER,
                PoliticalStrategistRole => RoleSignature.POLITICAL_STRATEGIST,
                OutsourcerRole => RoleSignature.OUTSOURCER,
                AlumniManagerRole => RoleSignature.ALUMNI_MANAGER,
                _ => throw new ArgumentException(ROLE_VALID_ERROR, nameof(role))
            };
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