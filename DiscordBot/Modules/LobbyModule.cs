using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Extensions;
using DiscordBot.Model;
using DiscordBot.Resources;
using System.Text;

namespace DiscordBot.Modules
{
    public class LobbyModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly LobbyService lobbyService;
        private readonly GameSessionService gameSessionService;

        public LobbyModule(LobbyService lobbyService, GameSessionService gameSessionService)
        {
            this.lobbyService = lobbyService;
            this.gameSessionService = gameSessionService;
        }

        [SlashCommand("lobby", "Create a lobby to play Paranoia Corp")]
        public async Task CreateLobby()
        {
            if(gameSessionService.HasActiveSession(Context.Guild.Id))
            {
                await RespondAsync(Miscellaneous.ActiveGameSessionExistsError, ephemeral: true);
                return;
            }

            LobbySession lobby = lobbyService.CreateLobby(Context.User.Id, Context.Channel.Id, LobbyMode.Normal);

            Embed embed = BuildLobbyEmbed(lobby);
            MessageComponent components = BuildLobbyComponents(lobby);

            await RespondAsync(embed: embed, components: components);
            IUserMessage message = await GetOriginalResponseAsync();
            lobbyService.RegisterLobbyMessage(message.Id, lobby);
        }

        [SlashCommand("admin_lobby", "Create a lobby in admin mode")]
        public async Task CreateAdminLobby()
        {
            if(gameSessionService.HasActiveSession(Context.Guild.Id))
            {
                await RespondAsync(Miscellaneous.ActiveGameSessionExistsError, ephemeral: true);
                return;
            }

            LobbySession lobby = lobbyService.CreateLobby(Context.User.Id, Context.Channel.Id, LobbyMode.Admin);

            Embed embed = BuildLobbyEmbed(lobby);
            MessageComponent components = BuildLobbyComponents(lobby);

            await RespondAsync(embed: embed, components: components);
            IUserMessage message = await GetOriginalResponseAsync();
            lobbyService.RegisterLobbyMessage(message.Id, lobby);
        }

        [SlashCommand("force_terminate_session", "Force terminate the active game session")]
        public async Task ForceTerminateSession()
        {
            await TerminateLobby();
        }

        [ComponentInteraction("lobby_terminate")]
        public async Task TerminateLobby()
        {
            gameSessionService.EndGameSession(Context.Guild.Id);
        }

        [ComponentInteraction("lobby_join")]
        public async Task JoinLobby()
        {
            SocketMessageComponent component = (SocketMessageComponent)Context.Interaction;
            LobbySession lobby = lobbyService.GetLobby(component.Message.Id);
            if(lobby == null) return;

            if(lobby.Players.Contains(Context.User.Id))
            {
                await RespondAsync(Miscellaneous.PlayerAlreadyInLobbyError, ephemeral: true);
                return;
            }

            lobby.Players.Add(Context.User.Id);
            await UpdateLobbyMessageAsync(lobby, component);
        }

        [ComponentInteraction("lobby_leave")]
        public async Task LeaveLobby()
        {
            SocketMessageComponent component = (SocketMessageComponent)Context.Interaction;
            LobbySession lobby = lobbyService.GetLobby(component.Message.Id);
            if(lobby == null) return;

            if(Context.User.Id == lobby.HostId)
            {
                lobbyService.RemoveLobby(component.Message.Id);

                await component.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                        .WithTitle(Miscellaneous.LobbyDispandedInfo)
                        .WithDescription(Miscellaneous.HostLeftLobbyInfo)
                        .WithColor(Color.Red)
                        .Build();
                    x.Components = new ComponentBuilder().Build();
                });
                return;
            }

            if(!lobby.Players.Contains(Context.User.Id))
            {
                await RespondAsync(Miscellaneous.PlayerIsNotInLobbyError, ephemeral: true);
                return;
            }

            lobby.Players.Remove(Context.User.Id);
            await UpdateLobbyMessageAsync(lobby, component);
        }

        [ComponentInteraction("lobby_add_role")]
        public async Task AddRole(string[] selectedRoles)
        {
            SocketMessageComponent component = (SocketMessageComponent)Context.Interaction;
            LobbySession lobby = lobbyService.GetLobby(component.Message.Id);
            if(lobby == null || Context.User.Id != lobby.HostId)
            {
                await RespondAsync(Miscellaneous.OnlyHostCanConfigureRoleError, ephemeral: true);
                return;
            }

            string roleName = selectedRoles[0];
            RoleVisual selectedRole = Enum.Parse<RoleVisual>(roleName);

            if(selectedRole.IsUnique() && lobby.SelectedRolesPool.ContainsKey(selectedRole))
            {
                await RespondAsync(Miscellaneous.UniqueRolePresentError, ephemeral: true);
                return;
            }

            if(lobby.SelectedRolesPool.ContainsKey(selectedRole))
            {
                lobby.SelectedRolesPool[selectedRole]++;
            }
            else
            {
                lobby.SelectedRolesPool[selectedRole] = 1;
            }

            await UpdateLobbyMessageAsync(lobby, component);
        }

        [ComponentInteraction("lobby_clear_roles")]
        public async Task ClearRoles()
        {
            SocketMessageComponent component = (SocketMessageComponent)Context.Interaction;
            LobbySession lobby = lobbyService.GetLobby(component.Message.Id);
            if(lobby == null || Context.User.Id != lobby.HostId) return;

            lobby.SelectedRolesPool.Clear();
            lobby.SelectedRolesPool.Add(RoleVisual.GENERAL_DIRECTOR, 1); //Mandatory role
            await UpdateLobbyMessageAsync(lobby, component);
        }

        [ComponentInteraction("lobby_view_roles")]
        public async Task ViewRoles()
        {
            //Send an ephemeral message
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle(Miscellaneous.DirectoryRolesTitle)
                .WithColor(Color.Blue);

            RoleVisual[] roleValues = Enum.GetValues<RoleVisual>();
            foreach(RoleVisual role in roleValues)
            {
                EmbedFieldBuilder field = new EmbedFieldBuilder()
                    .WithName(role.GetTeam().GetTeamIndicator() + role.GetLocalizedName())
                    .WithValue(role.GetLocilizedDescription())
                    .WithIsInline(false);
                embedBuilder.AddField(field);
            }

            await RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
        }

        [ComponentInteraction("lobby_start")]
        public async Task StartGame()
        {
            SocketMessageComponent component = (SocketMessageComponent)Context.Interaction;
            LobbySession lobby = lobbyService.GetLobby(component.Message.Id);
            if(lobby == null) return;

            if(Context.User.Id != lobby.HostId)
            {
                await RespondAsync(Miscellaneous.OnlyHostCanStartGameError, ephemeral: true);
                return;
            }

            (bool IsValid, string ErrorMessage) validation = lobbyService.ValidateLobby(lobby);
            if(!validation.IsValid)
            {
                await RespondAsync($"{Miscellaneous.LaunchError} {validation.ErrorMessage}", ephemeral: true);
                return;
            }

            await component.UpdateAsync(x =>
            {
                x.Embed = new EmbedBuilder()
                    .WithTitle("⏳ Preparing contracts...")
                    .WithDescription("Please wait. Distributing employment contracts to employees...")
                    .WithColor(Color.Orange)
                    .Build();
                x.Components = new ComponentBuilder().Build();
            });

            //Prepare game session
            switch(lobby.Mode)
            {
                case LobbyMode.Normal:
                    //Delete the lobby to prevent further interactions
                    lobbyService.RemoveLobby(component.Message.Id);

                    await PrepareNormalSessionAsync(lobby, component);
                    break;
                case LobbyMode.Admin:
                    await PrepareAdminSessionAsync((AdminLobbySession)lobby, component);
                    break;
            }
        }

        private async Task PrepareNormalSessionAsync(LobbySession lobby, SocketMessageComponent component)
        {
            GameHolder holder;
            IReadOnlyDictionary<ulong, RoleVisual> playerRoles;
            try
            {
                holder = gameSessionService.CreateGameSession(Context.Guild, lobby);
                playerRoles = gameSessionService.GetRoleCards(Context.Guild.Id);
            }
            catch(Exception ex)
            {
                await component.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                        .WithTitle(Miscellaneous.FailedStartGameError)
                        .WithDescription($"{Miscellaneous.PrepareGameUnexpectedError}: {ex.Message}")
                        .WithColor(Color.Red)
                        .Build();
                    x.Components = new ComponentBuilder().Build();
                });
                return;
            }

            //Send private messages to players with their assigned roles
            List<ulong> failedDMs = new List<ulong>();

            foreach(KeyValuePair<ulong, RoleVisual> kvp in playerRoles)
            {
                ulong userId = kvp.Key;
                RoleVisual roleVisual = kvp.Value;
                string roleName = roleVisual.GetTeam().GetTeamIndicator() + roleVisual.GetLocalizedName();
                string roleDescription = roleVisual.GetLocilizedDescription();

                try
                {
                    //Try to get the user from the guild first to avoid unnecessary API calls. If the user is not found, attempt to fetch them globally.
                    IUser user = Context.Guild.GetUser(userId) ?? (IUser)await Context.Client.Rest.GetUserAsync(userId);

                    if(user != null)
                    {
                        EmbedBuilder dmEmbed = new EmbedBuilder()
                            .WithTitle(Miscellaneous.YourContractInfo)
                            .WithDescription($"{Miscellaneous.PositionTitle} {roleName}\n\n{Miscellaneous.ResponsobilityTitle}\n{roleDescription}")
                            .WithColor(roleVisual.GetColor());

                        if(roleVisual != RoleVisual.GENERAL_DIRECTOR)
                        {
                            dmEmbed.WithFooter(Miscellaneous.DontShowDocumentWarning);
                        }

                        await user.SendMessageAsync(embed: dmEmbed.Build());
                    }
                }
                catch(Exception)
                {
                    //If the user has closed their DMs, remember their ID
                    failedDMs.Add(userId);
                }
            }

            //Update the lobby message with information about failed DMs, if any
            EmbedBuilder finalEmbed = new EmbedBuilder()
                .WithTitle(Miscellaneous.WorkingDayStartTitle)
                .WithDescription(Miscellaneous.CheckPMForContractInfo)
                .WithColor(Color.Green);

            if(failedDMs.Any())
            {
                string failedPings = string.Join(", ", failedDMs.Select(id => $"<@{id}>"));
                finalEmbed.AddField(Miscellaneous.AttentionTitle, string.Format(Miscellaneous.OpenDMWarning, failedPings));
            }

            if(await Context.Channel.GetMessageAsync(component.Message.Id) is IUserMessage msg)
            {
                await msg.ModifyAsync(x =>
                {
                    x.Embed = finalEmbed.Build();
                    x.Components = new ComponentBuilder().Build();
                });
            }

            //Start the session
            await gameSessionService.StartGameSession(holder);
        }

        private async Task PrepareAdminSessionAsync(AdminLobbySession lobby, SocketMessageComponent component)
        {
            //Admin mode allow the host attach roles to players manually
            lobby.PreserveRolePool();
            await UpdateAdminLobbyMessageAsync(lobby);
        }

        private async Task UpdateAdminLobbyMessageAsync(AdminLobbySession lobby)
        {
            EmbedBuilder dmEmbed = new EmbedBuilder()
                .WithTitle("Console for Admin Lobby")
                .WithDescription("Attach roles to players manually.")
                .WithColor(Color.Default);

            StringBuilder playerToRoleList = new StringBuilder();
            foreach(KeyValuePair<ulong, RoleVisual> kvp in lobby.AttachedRoles)
            {
                playerToRoleList.AppendLine($"<@{kvp.Key}> -> {kvp.Value.GetTeam().GetTeamIndicator()}{kvp.Value.GetLocalizedName()}");
            }

            if(playerToRoleList.Length == 0)
            {
                playerToRoleList.Append("No roles attached yet.");
            }
            else
            {
                dmEmbed.AddField("Attached Roles", playerToRoleList.ToString());
            }

            SelectMenuBuilder selectPlayerMenu = new SelectMenuBuilder()
                .WithCustomId("admin_lobby_attach_role_0")
                .WithPlaceholder("Attach a role to a player");

            foreach(var playerId in lobby.Players)
            {
                selectPlayerMenu.AddOption($"<@{playerId}>", playerId.ToString());
            }

            SelectMenuBuilder selectRoleMenu = new SelectMenuBuilder()
                .WithCustomId("admin_lobby_attach_role_1")
                .WithPlaceholder("Attach a role to a player");

            for(int i = 0; i < lobby.PreservedRolePool.Count; i++)
            {
                selectRoleMenu.AddOption(lobby.PreservedRolePool[i].GetLocalizedName(), i.ToString());
            }

            ComponentBuilder compBuilder = new ComponentBuilder()
                .WithSelectMenu(selectPlayerMenu, row: 0)
                .WithSelectMenu(selectRoleMenu, row: 1)
                .WithButton("Confirm","admin_attach_role_confirm", ButtonStyle.Success, row: 2);

            await RespondAsync(embed: dmEmbed.Build(), components: compBuilder.Build(), ephemeral: true);
        }

        [ComponentInteraction("admin_lobby_attach_role_*")]
        public async Task AdminAttachRole(string targetIndexStr, string[] selectedValues)
        {
            int index;

            if(!int.TryParse(targetIndexStr, out index))
                return;

            SocketMessageComponent component = (SocketMessageComponent)Context.Interaction;
            AdminLobbySession lobby = (AdminLobbySession)lobbyService.GetLobby(component.Message.Id);

            if(index == 0)
            {
                ulong targetId = ulong.Parse(selectedValues[0]);
                lobby.PendingSelectionPlayerId = targetId;
            }
            else if(index == 1)
            {
                if(lobby.PendingSelectionPlayerId == 0)
                {
                    await RespondAsync("Select a player first.", ephemeral: true);
                    return;
                }

                int roleId = int.Parse(selectedValues[0]);
                RoleVisual selectedRole = lobby.PreservedRolePool[roleId];
                lobby.AttachedRoles[lobby.PendingSelectionPlayerId] = selectedRole;
                await UpdateAdminLobbyMessageAsync(lobby);
            }
            else
            {
                await RespondAsync(Miscellaneous.TerminalSessionExpiredWarning, ephemeral: true);
            }
        }

        [ComponentInteraction("admin_attach_role_confirm")]
        public async Task AdminAttachRole()
        {
            SocketMessageComponent component = (SocketMessageComponent)Context.Interaction;
            AdminLobbySession? lobby = lobbyService.GetLobby(component.Message.Id) as AdminLobbySession;
            if(lobby == null) return;

            if(Context.User.Id != lobby.HostId)
            {
                await RespondAsync(Miscellaneous.OnlyHostCanStartGameError, ephemeral: true);
                return;
            }

            GameHolder holder;
            try
            {
                holder = gameSessionService.CreateGameSession(Context.Guild, lobby, lobby.AttachedRoles.AsReadOnly());
            }
            catch(Exception ex)
            {
                await component.UpdateAsync(x =>
                {
                    x.Embed = new EmbedBuilder()
                        .WithTitle(Miscellaneous.FailedStartGameError)
                        .WithDescription($"{Miscellaneous.PrepareGameUnexpectedError}: {ex.Message}")
                        .WithColor(Color.Red)
                        .Build();
                    x.Components = new ComponentBuilder().Build();
                });
                return;
            }

            //Send private messages to players with their assigned roles
            List<ulong> failedDMs = new List<ulong>();

            foreach(KeyValuePair<ulong, RoleVisual> kvp in lobby.AttachedRoles)
            {
                ulong userId = kvp.Key;
                RoleVisual roleVisual = kvp.Value;
                string roleName = roleVisual.GetTeam().GetTeamIndicator() + roleVisual.GetLocalizedName();
                string roleDescription = roleVisual.GetLocilizedDescription();

                try
                {
                    //Try to get the user from the guild first to avoid unnecessary API calls. If the user is not found, attempt to fetch them globally.
                    IUser user = Context.Guild.GetUser(userId) ?? (IUser)await Context.Client.Rest.GetUserAsync(userId);

                    if(user != null)
                    {
                        EmbedBuilder dmEmbed = new EmbedBuilder()
                            .WithTitle(Miscellaneous.YourContractInfo)
                            .WithDescription($"{Miscellaneous.PositionTitle} {roleName}\n\n{Miscellaneous.ResponsobilityTitle}\n{roleDescription}")
                            .WithColor(roleVisual.GetColor());

                        if(roleVisual != RoleVisual.GENERAL_DIRECTOR)
                        {
                            dmEmbed.WithFooter(Miscellaneous.DontShowDocumentWarning);
                        }

                        await user.SendMessageAsync(embed: dmEmbed.Build());
                    }
                }
                catch(Exception)
                {
                    //If the user has closed their DMs, remember their ID
                    failedDMs.Add(userId);
                }
            }

            //Update the lobby message with information about failed DMs, if any
            EmbedBuilder finalEmbed = new EmbedBuilder()
                .WithTitle(Miscellaneous.WorkingDayStartTitle)
                .WithDescription(Miscellaneous.CheckPMForContractInfo)
                .WithColor(Color.Green);

            if(failedDMs.Any())
            {
                string failedPings = string.Join(", ", failedDMs.Select(id => $"<@{id}>"));
                finalEmbed.AddField(Miscellaneous.AttentionTitle, string.Format(Miscellaneous.OpenDMWarning, failedPings));
            }

            if(await Context.Channel.GetMessageAsync(component.Message.Id) is IUserMessage msg)
            {
                await msg.ModifyAsync(x =>
                {
                    x.Embed = finalEmbed.Build();
                    x.Components = new ComponentBuilder().Build();
                });
            }

            //Start the session
            await gameSessionService.StartGameSession(holder);
        }

        private async Task UpdateLobbyMessageAsync(LobbySession lobby, SocketMessageComponent component)
        {
            Embed embed = BuildLobbyEmbed(lobby);
            await component.UpdateAsync(x => x.Embed = embed);
        }

        private Embed BuildLobbyEmbed(LobbySession lobby)
        {

            string rolesList = string.Join("\n",
                lobby.SelectedRolesPool.Select(r => $"- {r.Key.GetTeam().GetTeamIndicator()}{r.Key.GetLocalizedName()}: **{r.Value}**."));

            string playersMentions = string.Empty;

            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle(Miscellaneous.MorningCoffeeLobbyTitle)
                .WithDescription(Miscellaneous.WaitingForEmployeesInfo);

            switch(lobby.Mode)
            {
                case LobbyMode.Normal:
                {
                    playersMentions = string.Join("\n", lobby.Players.Select(id => $"<@{id}>{(id == lobby.HostId ? " 👑" : string.Empty)}"));
                    break;
                }
                case LobbyMode.Admin:
                {
                    playersMentions = string.Join("\n", lobby.Players.Select(id => $"<@{id}>"));
                    embedBuilder.AddField(Miscellaneous.HostNameTitle, $"<@{lobby.HostId}> 👑");
                    break;
                }
            }

            embedBuilder.AddField(string.Format(Miscellaneous.PlayersInLobbyInfo, lobby.Players.Count), string.IsNullOrEmpty(playersMentions) ? Miscellaneous.NoPlayersInLobbyInfo : playersMentions, true)
                .AddField(Miscellaneous.RolesInListTitle, lobby.TotalRolesCount, true)
                .AddField(Miscellaneous.RoleListTitle, string.IsNullOrEmpty(rolesList) ? Miscellaneous.EmptyTitle : rolesList)
                .WithColor(Color.DarkOrange)
                .WithFooter(Miscellaneous.ObservePlayerNumberEqualsRolesWarning);

            return embedBuilder.Build();
        }

        private MessageComponent BuildLobbyComponents(LobbySession lobby)
        {
            ComponentBuilder builder = new ComponentBuilder()
                .WithButton(Miscellaneous.EnterLobbyButton, "lobby_join", ButtonStyle.Success)
                .WithButton(Miscellaneous.LeaveLobbyButton, "lobby_leave", ButtonStyle.Danger)
                .WithButton(Miscellaneous.ViewRolesButton, "lobby_view_roles", ButtonStyle.Secondary)
                .WithButton(Miscellaneous.ClearRolesButton, "lobby_clear_roles", ButtonStyle.Secondary, row: 1)
                .WithButton(Miscellaneous.StartGameButton, "lobby_start", ButtonStyle.Primary, row: 1);

            SelectMenuBuilder selectMenu = new SelectMenuBuilder()
                .WithCustomId("lobby_add_role")
                .WithPlaceholder(Miscellaneous.AddRoleToListTitle);

            RoleVisual[] roleValues = Enum.GetValues<RoleVisual>();
            foreach(RoleVisual role in roleValues)
            {
                if(role.IsUnique() && lobby.SelectedRolesPool.ContainsKey(role)) continue;

                IEmote emoji = role.GetTeam().GetTeamIndicator();
                string? roleDebugName = Enum.GetName(typeof(RoleVisual), role);
                selectMenu.AddOption(role.GetLocalizedName(), roleDebugName, emote: emoji);
            }

            builder.WithSelectMenu(selectMenu, row: 2);

            return builder.Build();
        }
    }
}