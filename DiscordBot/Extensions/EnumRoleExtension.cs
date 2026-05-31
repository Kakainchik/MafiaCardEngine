using GameLogic.ParanoiaCorp.Attributes;
using System.Reflection;
using WebServer.Shared.ParanoiaCorp.GameObjects;
using WebServer.Shared.Extensions;
using DiscordBot.Model;
using DiscordBot.Resources;
using Discord;
using ExecutorAttribute = GameLogic.Attributes.ExecutorAttribute;
using WebServer.Shared.ParanoiaCorp.Extensions;

namespace DiscordBot.Extensions
{
    internal static class EnumRoleExtension
    {
        internal static RoleSignature MapRole(this RoleVisual role) => (RoleSignature)role;

        internal static RoleVisual MapRole(this RoleSignature signature) => (RoleVisual)signature;

        internal static bool IsUnique(this RoleVisual role) => role.MapRole().IsUnique();

        internal static string GetLocalizedName(this RoleVisual role)
        {
            string standart = string.Empty;
            FieldInfo? fi = role.GetType().GetField(role.ToString());
            if(fi is not null)
            {
                LocalizedDescriptionAttribute attr =
                    fi.GetCustomAttributes<LocalizedDescriptionAttribute>()
                    .Single(a => a.Locilize == RoleLocilize.NAME);

                return attr?.Description ?? standart;
            }
            return standart;
        }

        internal static string GetLocilizedDescription(this RoleVisual role)
        {
            string standart = string.Empty;
            FieldInfo? fi = role.GetType().GetField(role.ToString());
            if(fi is not null)
            {
                LocalizedDescriptionAttribute attr =
                    fi.GetCustomAttributes<LocalizedDescriptionAttribute>()
                    .Single(a => a.Locilize == RoleLocilize.DESCRIPTION);

                return attr?.Description ?? standart;
            }
            return standart;
        }

        internal static string GetLocilizedAbility(this RoleVisual role)
        {
            string standart = string.Empty;
            FieldInfo? fi = role.GetType().GetField(role.ToString());
            if(fi is not null)
            {
                LocalizedDescriptionAttribute attr =
                    fi.GetCustomAttributes<LocalizedDescriptionAttribute>()
                    .Single(a => a.Locilize == RoleLocilize.ABILITY);

                return attr?.Description ?? standart;
            }
            return standart;
        }

        internal static string GetLocilizedBelonging(this RoleVisual role)
        {
            switch(role.GetTeam())
            {
                case Team.CORPORATION:
                    return TeamNames.Corporation;
                case Team.SYNDICATE:
                    return TeamNames.Syndicate;
                case Team.STARTUP:
                    return TeamNames.Startup;
                case Team.OUTSOURCE:
                    return TeamNames.Outsource;
                case Team.SINGLES:
                    return TeamNames.Singles;
                default:
                    throw new ArgumentException("Invalid team", nameof(role));
            }
        }

        internal static string GetLocilizedBelonging(this Team team)
        {
            switch(team)
            {
                case Team.CORPORATION:
                    return TeamNames.Corporation;
                case Team.SYNDICATE:
                    return TeamNames.Syndicate;
                case Team.STARTUP:
                    return TeamNames.Startup;
                case Team.OUTSOURCE:
                    return TeamNames.Outsource;
                case Team.SINGLES:
                    return TeamNames.Singles;
                default:
                    throw new ArgumentException("Invalid team", nameof(team));
            }
        }

        internal static ChatScopeAttribute[] GetChatScopes(this RoleVisual role) => role.MapRole().GetChatScopes();

        internal static ExecutorAttribute GetExecutorAttribute(this RoleVisual role) => role.MapRole().GetExecutorAttribute();

        internal static Team GetTeam(this RoleVisual role) => role.MapRole().GetTeam();

        internal static IEmote GetTeamIndicator(this Team team)
        {
            return team switch
            {
                Team.CORPORATION => new Emoji("\U0001f7e9"),
                Team.SYNDICATE => new Emoji("\U0001f7e5"),
                Team.STARTUP => new Emoji("\U0001f7ea"),
                Team.OUTSOURCE => new Emoji("⬜"),
                Team.SINGLES => new Emoji("\U0001f7e7"),
                _ => throw new ArgumentException("Invalid team", nameof(team))
            };
        }

        internal static Color GetColor(this RoleVisual role) => GetColor(role.GetTeam());

        internal static Color GetColor(this Team team)
        {
            Color color = team switch
            {
                Team.CORPORATION => Color.Green,
                Team.SYNDICATE => Color.Red,
                Team.STARTUP => Color.Purple,
                Team.OUTSOURCE => Color.LighterGrey,
                Team.SINGLES => Color.LightOrange,
                _ => throw new ArgumentException("Invalid team", nameof(team))
            };
            return color;
        }
    }
}