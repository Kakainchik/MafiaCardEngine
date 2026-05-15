using GameLogic.Cycles.Night.Abilities;
using GameLogic.Interfaces;
using System.Reflection;
using GameLogic.Roles;
using GameLogic.Attributes;
using TeamAttribute = GameLogic.ParanoiaCorp.Attributes.TeamAttribute;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using ChatScopeAttribute = GameLogic.ParanoiaCorp.Attributes.ChatScopeAttribute;

namespace GameLogic.ParanoiaCorp.Extensions
{
    public static class AttributeExtension
    {
        public static Team GetTeam(this ITarget target)
        {
            TeamAttribute? attr = target.GetType().GetCustomAttribute<TeamAttribute>();
            return attr?.ItsTeam
                ?? throw new InvalidOperationException($"The {nameof(TeamAttribute)} is not set for [{target}].");
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

        public static ChatScopeAttribute[] GetChatScopeAttributes(this Role role)
        {
            ChatScopeAttribute[] attrs = (ChatScopeAttribute[])role.GetType().GetCustomAttributes(typeof(ChatScopeAttribute), false);

            return attrs;
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