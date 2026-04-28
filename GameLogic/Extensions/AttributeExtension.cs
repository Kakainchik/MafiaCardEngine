using GameLogic.Attributes;
using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
using GameLogic.Roles;
using System.Reflection;

namespace GameLogic.Extensions
{
    public static class AttributeExtension
    {
        public static Team GetTeam(this ITarget target)
        {
            TeamAttribute? attr = target.GetType().GetCustomAttribute<TeamAttribute>();
            return attr?.ItsTeam
                ?? throw new InvalidOperationException($"The {nameof(TeamAttribute)} is not set for [{target}].");
        }

        public static RoleCategory GetRCategory(this Role role)
        {
            CategoryAttribute? attr = role.GetType().GetCustomAttribute<CategoryAttribute>();
            return attr?.Category
                ?? throw new InvalidOperationException($"The {nameof(CategoryAttribute)} is not set for [{role}].");
        }

        public static int GetNumberAbilities(this Role role)
        {
            ExecutorAttribute? attr = role.GetType().GetCustomAttribute<ExecutorAttribute>();
            return attr?.NumberAbilities
                ?? throw new InvalidOperationException($"The {nameof(ExecutorAttribute)} is not set for [{role}].");
        }

        public static int GetNumberAbilities(this IExecutor executor)
        {
            ExecutorAttribute? attr = executor.GetType().GetCustomAttribute<ExecutorAttribute>();
            return attr?.NumberAbilities
                ?? throw new InvalidOperationException($"The {nameof(ExecutorAttribute)} is not set for [{executor}].");
        }

        public static ExecutorType GetExecutorType(this Role role)
        {
            ExecutorAttribute? attr = role.GetType().GetCustomAttribute<ExecutorAttribute>();
            return attr?.EType
                ?? throw new InvalidOperationException($"The {nameof(ExecutorAttribute)} is not set for [{role}].");
        }

        public static ExecutorType GetExecutorType(this IExecutor executor)
        {
            ExecutorAttribute? attr = executor.GetType().GetCustomAttribute<ExecutorAttribute>();
            return attr?.EType
                ?? throw new InvalidOperationException($"The {nameof(ExecutorAttribute)} is not set for [{executor}].");
        }

        public static ChatScopeAttribute GetChatScopeAttribute(this Role role)
        {
            ChatScopeAttribute? attr = role.GetType().GetCustomAttribute<ChatScopeAttribute>();
            return attr
                ?? throw new InvalidOperationException($"The {nameof(ChatScopeAttribute)} is not set for [{role}].");
        }

        public static ChatScopeAttribute GetChatScopeAttribute(this Type roleType)
        {
            InvalidOperationException ex = new InvalidOperationException($"The {nameof(ChatScopeAttribute)} is not set for [{roleType}].");
            if(roleType.IsSubclassOf(typeof(Role)))
            {
                ChatScopeAttribute? attr = roleType.GetCustomAttribute<ChatScopeAttribute>();
                return attr ?? throw ex;
            }
            else
            {
                throw ex;
            }
        }

        public static bool IsUniqie(this Role role)
        {
            UniqueAttribute? attr = role.GetType().GetCustomAttribute<UniqueAttribute>();
            return attr is not null;
        }

        public static APriority GetAbilityPriority(this IAbility ability)
        {
            AbilityPriorityAttribute? attr = ability.GetType().GetCustomAttribute<AbilityPriorityAttribute>();
            return attr?.Priority
                ?? throw new InvalidOperationException($"The {nameof(AbilityPriorityAttribute)} is not set for [{ability}].");
        }
    }
}