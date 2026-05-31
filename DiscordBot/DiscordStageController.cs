using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Extensions;
using DiscordBot.Model;
using DiscordBot.Resources;
using GameLogic;
using GameLogic.Cycles;
using GameLogic.ParanoiaCorp.Attributes;
using GameLogic.ParanoiaCorp.Cycles;
using GameLogic.ParanoiaCorp.Extensions;
using GameLogic.ParanoiaCorp.Roles;
using System.Collections.Concurrent;
using WebServer.Shared.ParanoiaCorp.Extensions;

namespace DiscordBot
{
    public class DiscordStageController
    {
        private static readonly string SyndicateNeutralName = ChatScope.SYNDICATE.ToString();
        private static readonly string ShareholderNeutralName = ChatScope.SHAREHOLDER.ToString();
        private static readonly string StartupFounderNeutralName = RoleVisual.STARUP_FOUNDER.ToString();
        private static readonly string EvangelistNeutralName = RoleVisual.EVANGELIST.ToString();
        private static readonly string OutsourceNeutralName = ChatScope.OUTSOURCE.ToString();
        private static readonly string FiredNeutralName = ChatScope.FIRED.ToString();
        private static readonly string AlumniNeutralName = RoleVisual.ALUMNI_MANAGER.ToString();

        private readonly DiscordSocketClient client;
        private readonly DiscordEndGameController endGameController;
        private readonly ConcurrentDictionary<ulong, DiscordSessionEnvironment> environments = new();

        public DiscordStageController(DiscordSocketClient client, DiscordEndGameController endGameController)
        {
            this.client = client;
            this.endGameController = endGameController;
            this.client.MessageReceived += Client_MessageReceived;
        }

        /// <summary>
        /// Creates a new category with channels and roles for the game session.
        /// </summary>
        public async Task SetupEnvironmentAsync(GameHolder holder)
        {
            SocketGuild guild = holder.Guild;
            DiscordSessionEnvironment env = new DiscordSessionEnvironment();

            RestCategoryChannel category = await guild.CreateCategoryChannelAsync(Miscellaneous.ParanoiaCorpSessionTitle);
            env.CategoryId = category.Id;

            RestTextChannel generalText = await guild.CreateTextChannelAsync("Morning Meeting", x => x.CategoryId = category.Id);
            RestVoiceChannel voiceBoard = await guild.CreateVoiceChannelAsync("Boardroom", x => x.CategoryId = category.Id);

            env.GeneralTextChannelId = generalText.Id;
            env.VoiceBoardChannelId = voiceBoard.Id;

            RestRole syndicateScope = await guild.CreateRoleAsync(SyndicateNeutralName, isHoisted: false);
            RestRole shareholderScope = await guild.CreateRoleAsync(ShareholderNeutralName, isHoisted: false);
            RestRole startupFounderScope = await guild.CreateRoleAsync(StartupFounderNeutralName, isHoisted: false);
            RestRole evangelistScope = await guild.CreateRoleAsync(EvangelistNeutralName, isHoisted: false);
            RestRole outsourceScope = await guild.CreateRoleAsync(OutsourceNeutralName, isHoisted: false);
            RestRole alumniScope = await guild.CreateRoleAsync(AlumniNeutralName, isHoisted: false);
            RestRole firedScope = await guild.CreateRoleAsync(FiredNeutralName, isHoisted: false);

            env.FractionRoles.Add(syndicateScope.Name, syndicateScope.Id);
            env.FractionRoles.Add(shareholderScope.Name, shareholderScope.Id);
            env.FractionRoles.Add(startupFounderScope.Name, startupFounderScope.Id);
            env.FractionRoles.Add(evangelistScope.Name, evangelistScope.Id);
            env.FractionRoles.Add(outsourceScope.Name, outsourceScope.Id);
            env.FractionRoles.Add(alumniScope.Name, alumniScope.Id);
            env.FractionRoles.Add(firedScope.Name, firedScope.Id);

            foreach(Player player in holder.Engine.Players)
            {
                SocketGuildUser discordUser = guild.GetUser(player.Id);
                if(discordUser == null) continue;

                ChatScopeAttribute[] chatScopeAttrs = player.Role.GetChatScopeAttributes();

                for(int i = 0; i < chatScopeAttrs.Length; i++)
                {
                    switch(chatScopeAttrs[i].Scope)
                    {
                        case ChatScope.SYNDICATE:
                            await discordUser.AddRoleAsync(syndicateScope);
                            break;
                        case ChatScope.SHAREHOLDER:
                            await discordUser.AddRoleAsync(shareholderScope);
                            break;
                        case ChatScope.STARTUP when chatScopeAttrs[i].CanWrite:
                            await discordUser.AddRoleAsync(evangelistScope);
                            break;
                        case ChatScope.STARTUP when !chatScopeAttrs[i].CanWrite:
                            await discordUser.AddRoleAsync(startupFounderScope);
                            break;
                        case ChatScope.OUTSOURCE:
                            await discordUser.AddRoleAsync(outsourceScope);
                            break;
                        case ChatScope.FIRED when chatScopeAttrs[i].CanWrite:
                            await discordUser.AddRoleAsync(firedScope);
                            break;
                    }
                }
            }

            OverwritePermissions denyEveryone = new OverwritePermissions(viewChannel: PermValue.Deny);

            RestTextChannel shareholderChannel = await guild.CreateTextChannelAsync("Private Shareholder Channel", x =>
            {
                x.CategoryId = category.Id;
                x.PermissionOverwrites = new[] {
                    new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, denyEveryone),
                    new Overwrite(shareholderScope.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow))
                };
            });

            RestTextChannel syndicateChannel = await guild.CreateTextChannelAsync("Private Syndicate Channel", x =>
            {
                x.CategoryId = category.Id;
                x.PermissionOverwrites = new[] {
                    new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, denyEveryone),
                    new Overwrite(syndicateScope.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow))
                };
            });

            RestTextChannel startupFounderChannel = await guild.CreateTextChannelAsync("Spy Startup Founder Channel", x =>
            {
                x.CategoryId = category.Id;
                x.PermissionOverwrites = new[] {
                    new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, denyEveryone),
                    new Overwrite(startupFounderScope.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny))
                };
            });

            RestTextChannel evangelistChannel = await guild.CreateTextChannelAsync("Private Evangelist Channel", x =>
            {
                x.CategoryId = category.Id;
                x.PermissionOverwrites = new[] {
                    new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, denyEveryone),
                    new Overwrite(evangelistScope.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow))
                };
            });

            RestTextChannel outsourceChannel = await guild.CreateTextChannelAsync("Private Outsource Channel", x =>
            {
                x.CategoryId = category.Id;
                x.PermissionOverwrites = new[] {
                    new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, denyEveryone),
                    new Overwrite(outsourceScope.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow))
                };
            });

            RestTextChannel firedChannel = await guild.CreateTextChannelAsync("Club of Former Colleagues", x =>
            {
                x.CategoryId = category.Id;
                x.PermissionOverwrites = new[] {
                    new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, denyEveryone),
                    new Overwrite(firedScope.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow))
                };
            });

            //Fired players should not have access to the general text channel and voice channel, so we set that up here at the start
            await generalText.AddPermissionOverwriteAsync(firedScope, new OverwritePermissions(sendMessages: PermValue.Deny));
            await voiceBoard.AddPermissionOverwriteAsync(firedScope, new OverwritePermissions(speak: PermValue.Deny));

            env.PrivateChannels.Add(shareholderScope.Name, shareholderChannel.Id);
            env.PrivateChannels.Add(syndicateScope.Name, syndicateChannel.Id);
            env.PrivateChannels.Add(evangelistScope.Name, evangelistChannel.Id);
            env.PrivateChannels.Add(outsourceScope.Name, outsourceChannel.Id);
            env.PrivateChannels.Add(firedScope.Name, firedChannel.Id);

            environments[guild.Id] = env;
        }

        /// <summary>
        /// The method to call whenever the game cycle changes.
        /// It updates permissions and sends announcements in the general text channel based on the current cycle.
        /// This keeps players informed about the current phase of the game and ensures they have access to the appropriate channels for discussion and coordination.
        /// </summary>
        public async Task SyncCycleWithDiscordAsync(GameHolder holder)
        {
            SocketGuild guild = holder.Guild;
            DiscordSessionEnvironment env = environments[guild.Id];
            ICycle currentCycle = holder.Engine.CurrentCycle;
            SocketTextChannel generalText = guild.GetTextChannel(env.GeneralTextChannelId);
            SocketVoiceChannel voiceBoard = guild.GetVoiceChannel(env.VoiceBoardChannelId);

            ulong firedScopeId = env.FractionRoles[ChatScope.FIRED.ToString()];
            SocketRole firedScope = guild.GetRole(firedScopeId);

            for(int i = 0; i < holder.Engine.Players.Length; i++)
            {
                Player player = holder.Engine.Players[i];
                if(!player.IsAlive)
                {
                    SocketGuildUser discordUser = guild.GetUser(player.Id);

                    //If the player still has no the "Fired" role
                    if(discordUser != null && !discordUser.Roles.Contains(firedScope))
                    {
                        IEnumerable<SocketRole> rolesToRemove = env.FractionRoles
                            .Where(kvp => kvp.Value != firedScopeId)
                            .Select(kvp => guild.GetRole(kvp.Value))
                            .Where(r => r != null);

                        await discordUser.RemoveRolesAsync(rolesToRemove);
                        await discordUser.AddRoleAsync(firedScope);
                    }
                }
            }

            OverwritePermissions allowReadWrite = new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow);
            OverwritePermissions denyAll = new OverwritePermissions(viewChannel: PermValue.Deny);

            if(currentCycle is IntroductoryDayCycle)
            {
                await HandleIntroductoryCycle(guild, env, generalText, voiceBoard, denyAll);
            }
            else if(currentCycle is MorningCycle morningCycle)
            {
                await HandleMorningCycle(guild, env, generalText, voiceBoard, denyAll, morningCycle);
            }
            else if(currentCycle is DirectorBoardCycle boardCycle)
            {
                await HandleBoardCycle(holder, guild, env, generalText, voiceBoard, denyAll, boardCycle);
            }
            else if(currentCycle is EveningCycle eveningCycle)
            {
                await HandleEveningCycle(generalText, eveningCycle);
            }
            else if(currentCycle is OvertimeCycle)
            {
                await HandleOvertimeCycle(holder, guild, env, generalText, voiceBoard, allowReadWrite);
            }
            else if(currentCycle is EndGameCycle endGameCycle)
            {
                await HandleEndGameCycle(guild, env, generalText, voiceBoard, allowReadWrite, denyAll, endGameCycle);
                return;
            }
        }

        private static async Task HandleIntroductoryCycle(SocketGuild guild, DiscordSessionEnvironment env, SocketTextChannel generalText, SocketVoiceChannel voiceBoard, OverwritePermissions denyAll)
        {
            //At the start of the game, disallow everyone to send messages in the general text channel and speak in the voice channel until the first board meeting starts
            await generalText.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(sendMessages: PermValue.Deny));
            await voiceBoard.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(speak: PermValue.Deny));

            //Also disallow everyone to see private channels until the first board meeting starts
            await ApplyFractionChannelPermissions(guild, env, SyndicateNeutralName, denyAll, SyndicateNeutralName);
            await ApplyFractionChannelPermissions(guild, env, ShareholderNeutralName, denyAll, ShareholderNeutralName);
            await ApplyFractionChannelPermissions(guild, env, StartupFounderNeutralName, denyAll, StartupFounderNeutralName);
            await ApplyFractionChannelPermissions(guild, env, EvangelistNeutralName, denyAll, EvangelistNeutralName);
            await ApplyFractionChannelPermissions(guild, env, OutsourceNeutralName, denyAll, OutsourceNeutralName);

            MessageComponent components = new ComponentBuilder()
                .WithButton(Miscellaneous.ActionIntroReadyButton, "action_intro_ready", ButtonStyle.Success)
                .Build();

            await generalText.SendMessageAsync(Miscellaneous.IntroDayMessageInfo, components: components);
        }

        private static async Task HandleMorningCycle(SocketGuild guild, DiscordSessionEnvironment env, SocketTextChannel generalText, SocketVoiceChannel voiceBoard, OverwritePermissions denyAll, MorningCycle morningCycle)
        {
            await generalText.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(sendMessages: PermValue.Deny));
            await voiceBoard.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(speak: PermValue.Deny));

            await ApplyFractionChannelPermissions(guild, env, SyndicateNeutralName, denyAll, SyndicateNeutralName);
            await ApplyFractionChannelPermissions(guild, env, ShareholderNeutralName, denyAll, ShareholderNeutralName);
            await ApplyFractionChannelPermissions(guild, env, StartupFounderNeutralName, denyAll, StartupFounderNeutralName);
            await ApplyFractionChannelPermissions(guild, env, EvangelistNeutralName, denyAll, EvangelistNeutralName);
            await ApplyFractionChannelPermissions(guild, env, OutsourceNeutralName, denyAll, OutsourceNeutralName);

            // Send logs to PMs and publish public news in the general channel based on the logs
            Queue<PostmanPresenter> postman = ActionLogFacade.ZipLogs(morningCycle.ActionLogs);
            var postponedMessages = await ActionLogFacade.InterpretLogs(postman, guild, generalText);
            env.PostponedMessages = postponedMessages;

            //The button for the General Director
            MessageComponent components = new ComponentBuilder()
                .WithButton(Miscellaneous.ActionMorningStartBoardButton, "action_morning_start_board", ButtonStyle.Danger)
                .Build();

            await generalText.SendMessageAsync(Miscellaneous.MorningReadyMessageInfo, components: components);
        }

        private static async Task HandleBoardCycle(GameHolder holder, SocketGuild guild, DiscordSessionEnvironment env, SocketTextChannel generalText, SocketVoiceChannel voiceBoard, OverwritePermissions denyAll, DirectorBoardCycle boardCycle)
        {
            //Allow everyone to send messages in the general text channel and speak in the voice channel
            await generalText.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(sendMessages: PermValue.Allow));
            await voiceBoard.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(speak: PermValue.Allow));

            //Disallow everyone to see private channels, then allow only players of the corresponding fractions to see their channels
            await ApplyFractionChannelPermissions(guild, env, SyndicateNeutralName, denyAll, SyndicateNeutralName);
            await ApplyFractionChannelPermissions(guild, env, ShareholderNeutralName, denyAll, ShareholderNeutralName);
            await ApplyFractionChannelPermissions(guild, env, StartupFounderNeutralName, denyAll, StartupFounderNeutralName);
            await ApplyFractionChannelPermissions(guild, env, EvangelistNeutralName, denyAll, EvangelistNeutralName);
            await ApplyFractionChannelPermissions(guild, env, OutsourceNeutralName, denyAll, OutsourceNeutralName);

            string vetoRemainder = string.Format(Miscellaneous.VetoReminderMessage, holder.Engine.GeneralDirector.VetoCount);
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle(Miscellaneous.BoardMeetingStartTitle)
                .WithDescription(vetoRemainder)
                .WithColor(Color.Gold);

            if(boardCycle.CandidatesForFiring.Any())
            {
                string playerMentions = string.Join("\n", boardCycle.CandidatesForFiring.Select(p => $"- <@{p.Id}>"));
                embedBuilder.AddField(Miscellaneous.CandidateForFireExistInfo, playerMentions, true);
            }
            else
            {
                embedBuilder.AddField(Miscellaneous.NoCandidatesForFireInfo, Miscellaneous.GeneralDirectorCanSelectCandidateReminder, true);
            }

            MessageComponent components = BuildDirectorBoardComponents(guild, holder, boardCycle);

            await generalText.SendMessageAsync(embed: embedBuilder.Build(), components: components);
        }

        private static async Task HandleEveningCycle(SocketTextChannel generalText, EveningCycle eveningCycle)
        {
            RoleVisual firedRole = eveningCycle.Elected.Role.IntoSignature().MapRole();
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle(Miscellaneous.DirectorBoardOverTitle)
                .WithDescription($"{string.Format(Miscellaneous.EmployeeMustRevealRoleInfo, eveningCycle.Elected.Id)}\n\n" +
                $"{Miscellaneous.TheirPositionTitle} {firedRole.GetTeam().GetTeamIndicator()} {firedRole.GetLocalizedName()}.")
                .WithColor(Color.DarkBlue);

            ComponentBuilder components = new ComponentBuilder()
                .WithButton(Miscellaneous.ActionFinishEveningButton, "action_finish_evening", ButtonStyle.Primary);

            await generalText.SendMessageAsync(embed: embedBuilder.Build(), components: components.Build());
        }

        private static async Task HandleOvertimeCycle(GameHolder holder, SocketGuild guild, DiscordSessionEnvironment env, SocketTextChannel generalText, SocketVoiceChannel voiceBoard, OverwritePermissions allowReadWrite)
        {
            //Disallow everyone to send messages in the general text channel and speak in the voice channel
            await generalText.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(sendMessages: PermValue.Deny));
            await voiceBoard.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(speak: PermValue.Deny));

            OverwritePermissions allowReadOnly = new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny);

            await ApplyFractionChannelPermissions(guild, env, SyndicateNeutralName, allowReadWrite, SyndicateNeutralName);
            await ApplyFractionChannelPermissions(guild, env, ShareholderNeutralName, allowReadWrite, ShareholderNeutralName);
            await ApplyFractionChannelPermissions(guild, env, StartupFounderNeutralName, allowReadOnly, StartupFounderNeutralName);
            await ApplyFractionChannelPermissions(guild, env, EvangelistNeutralName, allowReadWrite, EvangelistNeutralName);
            await ApplyFractionChannelPermissions(guild, env, OutsourceNeutralName, allowReadWrite, OutsourceNeutralName);

            //Send any postponed messages to the corresponding players via DM
            while(env.PostponedMessages.TryPop(out var composeMessage))
            {
                SocketGuildUser user = guild.GetUser(composeMessage.user);
                if(user != null)
                {
                    await user.SendMessageAsync($"{Miscellaneous.PostMessageInfo} {composeMessage.message}");
                }
            }

            //Notify Anticrisis Managers about their remaining reports to write
            IEnumerable<AnticrisisManagerRole> anticrisisManagers = holder.Engine.AlivePlayers.Where(p => p.Role is AnticrisisManagerRole)
                .Select(p => (AnticrisisManagerRole)p.Role);
            foreach(AnticrisisManagerRole anticrisisManager in anticrisisManagers)
            {
                SocketGuildUser user = guild.GetUser(anticrisisManager.Owner!.Id);
                if(user != null)
                {
                    await user.SendMessageAsync(string.Format(Miscellaneous.TokenNotificationInfo, anticrisisManager.Items));
                }
            }

            MessageComponent components = new ComponentBuilder()
                .WithButton(Miscellaneous.ActionOpenTerminalButton, "action_open_terminal", ButtonStyle.Primary)
                .Build();

            await generalText.SendMessageAsync(Miscellaneous.WorkdayOverTitle, components: components);
        }

        private async Task HandleEndGameCycle(SocketGuild guild, DiscordSessionEnvironment env, SocketTextChannel generalText, SocketVoiceChannel voiceBoard, OverwritePermissions allowReadWrite, OverwritePermissions denyAll, EndGameCycle endGameCycle)
        {
            await generalText.AddPermissionOverwriteAsync(guild.EveryoneRole, allowReadWrite);
            await voiceBoard.AddPermissionOverwriteAsync(guild.EveryoneRole, new OverwritePermissions(speak: PermValue.Allow));

            //Hide private channels from everyone
            await ApplyFractionChannelPermissions(guild, env, SyndicateNeutralName, denyAll, SyndicateNeutralName);
            await ApplyFractionChannelPermissions(guild, env, ShareholderNeutralName, denyAll, ShareholderNeutralName);
            await ApplyFractionChannelPermissions(guild, env, StartupFounderNeutralName, denyAll, StartupFounderNeutralName);
            await ApplyFractionChannelPermissions(guild, env, EvangelistNeutralName, denyAll, EvangelistNeutralName);
            await ApplyFractionChannelPermissions(guild, env, OutsourceNeutralName, denyAll, OutsourceNeutralName);
            await ApplyFractionChannelPermissions(guild, env, FiredNeutralName, denyAll, FiredNeutralName);
            await ApplyFractionChannelPermissions(guild, env, AlumniNeutralName, denyAll, AlumniNeutralName);

            await CleanupEnvironmentAsync(guild, env, false);

            await endGameController.ProcessEndGameAsync(endGameCycle, generalText);
        }

        /// <summary>
        /// Deletes all channels and roles created for the game session. Should be called after the game ends to clean up the Discord server.
        /// </summary>
        public async Task CleanupEnvironmentAsync(SocketGuild guild, DiscordSessionEnvironment env, bool deleteGeneralChannel = true)
        {
            SocketCategoryChannel category = guild.GetCategoryChannel(env.CategoryId);
            if(category != null)
            {
                foreach(SocketGuildChannel channel in category.Channels)
                {
                    if(!deleteGeneralChannel && channel.Id == env.GeneralTextChannelId) continue;
                    await channel.DeleteAsync();
                }

                await category.DeleteAsync();
            }

            foreach(ulong roleId in env.ManagedRoles)
            {
                SocketRole role = guild.GetRole(roleId);
                if(role != null) await role.DeleteAsync();
            }

            environments.TryRemove(guild.Id, out _);
        }

        public static MessageComponent BuildDirectorBoardComponents(SocketGuild guild, GameHolder holder, DirectorBoardCycle boardCycle)
        {
            ComponentBuilder builder = new ComponentBuilder();

            bool canChooseAnyone = boardCycle.VetoUsed || !boardCycle.CandidatesForFiring.Any();
            IEnumerable<Player> availableTargets = canChooseAnyone
                ? holder.Engine.AlivePlayers
                : boardCycle.CandidatesForFiring;

            if(availableTargets.Any())
            {
                SelectMenuBuilder selectMenu = new SelectMenuBuilder()
                    .WithCustomId("action_director_target_select")
                    .WithPlaceholder(Miscellaneous.ActionDirectorTargetSelect);

                foreach(Player target in availableTargets)
                {
                    SocketGuildUser targetUser = guild.GetUser(target.Id);
                    string targetName = targetUser?.Username ?? $"Employee {target.Id}";
                    selectMenu.AddOption(targetName, target.Id.ToString());
                }

                builder.WithSelectMenu(selectMenu);
            }

            builder.WithButton(Miscellaneous.FirePlayerButton, "action_director_fire", ButtonStyle.Danger);

            if(!boardCycle.VetoUsed || (holder.Engine.GeneralDirector.VetoCount > 0 && !boardCycle.CandidatesForFiring.Any()))
            {
                builder.WithButton(Miscellaneous.UseVetoButton, "action_director_veto", ButtonStyle.Primary);
            }

            if(boardCycle.CanSkipTurn)
            {
                builder.WithButton(Miscellaneous.DontFireButton, "action_director_skip", ButtonStyle.Secondary);
            }

            return builder.Build();
        }

        private static async Task ApplyFractionChannelPermissions(SocketGuild guild, DiscordSessionEnvironment env, string channelKey, OverwritePermissions perms, string roleKey)
        {
            if(env.PrivateChannels.TryGetValue(channelKey, out ulong chanellId) && env.FractionRoles.TryGetValue(roleKey, out ulong roleId))
            {
                SocketTextChannel channel = guild.GetTextChannel(chanellId);
                SocketRole role = guild.GetRole(roleId);
                if(channel != null && role != null)
                {
                    await channel.AddPermissionOverwriteAsync(role, perms);
                }
            }
        }

        private async Task Client_MessageReceived(SocketMessage message)
        {
            //Ignore messages from bots
            if(message.Author.IsBot) return;

            //Ensure the message is from a text channel in a guild
            if(message.Channel is not SocketTextChannel textChannel) return;

            //Ensure there is an environment set up for the guild
            if(environments.TryGetValue(textChannel.Guild.Id, out DiscordSessionEnvironment? env))
            {
                //If the message is sent in the "Private Evangelist Channel", forward it to the "Spy Startup Founder Channel"
                if(env.PrivateChannels.TryGetValue(EvangelistNeutralName, out ulong evangelistChannelId) && textChannel.Id == evangelistChannelId)
                {
                    //Search for the "Spy Startup Founder Channel" and if it exists, forward the message there
                    if(env.PrivateChannels.TryGetValue(StartupFounderNeutralName, out ulong startupFounderChannelId))
                    {
                        SocketTextChannel spyChannel = textChannel.Guild.GetTextChannel(startupFounderChannelId);
                        if(spyChannel != null)
                        {
                            //Forward messages
                            await spyChannel.SendMessageAsync($"🕵️ **{message.Author.Username}**: {message.Content}");
                        }
                    }
                }
            }
        }
    }
}