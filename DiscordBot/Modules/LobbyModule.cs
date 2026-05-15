using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Extensions;
using DiscordBot.Model;
using DiscordBot.Resources;

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
            var culture = Thread.CurrentThread.CurrentUICulture;
            if(gameSessionService.HasActiveSession(Context.Guild.Id))
            {
                await RespondAsync(Miscellaneous.ActiveGameSessionExistsError, ephemeral: true);
                return;
            }

            LobbySession lobby = lobbyService.CreateLobby(Context.User.Id, Context.Channel.Id);

            Embed embed = BuildLobbyEmbed(lobby);
            MessageComponent components = BuildLobbyComponents(lobby);

            await RespondAsync(embed: embed, components: components);
            IUserMessage message = await GetOriginalResponseAsync();
            lobbyService.RegisterLobbyMessage(message.Id, lobby);
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

            if(!lobby.Players.Contains(Context.User.Id))
            {
                await RespondAsync(Miscellaneous.PlayerIsNotInLobbyError, ephemeral: true);
                return;
            }

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

            //Delete the lobby to prevent further interactions
            lobbyService.RemoveLobby(component.Message.Id);

            //Prepare game session
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
            

            await component.UpdateAsync(x =>
            {
                x.Embed = new EmbedBuilder()
                    .WithTitle(Miscellaneous.WorkingDayStartTitle)
                    .WithDescription(Miscellaneous.CheckPMForContractInfo)
                    .WithColor(Color.Green)
                    .Build();
                x.Components = new ComponentBuilder().Build();
            });

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
                        Embed dmEmbed = new EmbedBuilder()
                            .WithTitle(Miscellaneous.YourContractInfo)
                            .WithDescription($"{Miscellaneous.PositionTitle} {roleName}\n\n{Miscellaneous.ResponsobilityTitle}\n{roleDescription}")
                            .WithColor(roleVisual.GetColor())
                            .WithFooter(Miscellaneous.DontShowDocumentWarning)
                            .Build();

                        await user.SendMessageAsync(embed: dmEmbed);
                    }
                }
                catch(Exception)
                {
                    //If the user has closed their DMs, remember their ID
                    failedDMs.Add(userId);
                }
            }

            //Update the lobby message with information about failed DMs, if any
            await component.UpdateAsync(x =>
            {
                EmbedBuilder finalEmbed = new EmbedBuilder()
                    .WithTitle(Miscellaneous.WorkingDayStartTitle)
                    .WithDescription(Miscellaneous.CheckPMForContractInfo)
                    .WithColor(Color.Green);

                if(failedDMs.Any())
                {
                    string failedPings = string.Join(", ", failedDMs.Select(id => $"<@{id}>"));
                    finalEmbed.AddField(Miscellaneous.AttentionTitle, string.Format(Miscellaneous.OpenDMWarning, failedPings));
                }

                x.Embed = finalEmbed.Build();
                x.Components = new ComponentBuilder().Build();
            });

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

            string playersMentions = string.Join("\n", lobby.Players.Select(id => $"<@{id}>{(id == lobby.HostId ? " 👑" : string.Empty)}"));

            return new EmbedBuilder()
                .WithTitle(Miscellaneous.MorningCoffeeLobbyTitle)
                .WithDescription(Miscellaneous.WaitingForEmployeesInfo)
                .AddField(string.Format(Miscellaneous.PlayersInLobbyInfo, lobby.Players.Count), string.IsNullOrEmpty(playersMentions) ? Miscellaneous.NoPlayersInLobbyInfo : playersMentions, true)
                .AddField(Miscellaneous.RolesInListTitle, lobby.TotalRolesCount, true)
                .AddField(Miscellaneous.RoleListTitle, string.IsNullOrEmpty(rolesList) ? Miscellaneous.EmptyTitle : rolesList)
                .WithColor(Color.DarkOrange)
                .WithFooter(Miscellaneous.ObservePlayerNumberEqualsRolesWarning)
                .Build();
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