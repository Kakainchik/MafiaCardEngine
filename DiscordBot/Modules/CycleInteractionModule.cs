using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Extensions;
using DiscordBot.Model;
using DiscordBot.Resources;
using GameLogic;
using GameLogic.Attributes;
using GameLogic.Interfaces;
using GameLogic.ParanoiaCorp.Cycles;
using GameLogic.ParanoiaCorp.Extensions;
using System.Collections.Concurrent;
using WebServer.Shared.ParanoiaCorp.Extensions;

namespace DiscordBot.Modules
{
    public class CycleInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        /// <summary>
        /// This dictionary keeps track of pending selections for each player during the Overtime cycle. The key is the player's ID, and the value is an array of selected target IDs.
        /// </summary>
        private static readonly ConcurrentDictionary<ulong, ulong[]> PendingSelections = new();
        private static readonly ConcurrentDictionary<ulong, ulong> DirectorPendingSelections = new();

        private readonly GameSessionService gameSessionService;

        public CycleInteractionModule(GameSessionService gameSessionService)
        {
            this.gameSessionService = gameSessionService;
        }

        #region Intro Cycle

        [ComponentInteraction("action_intro_ready")]
        public async Task HandleIntroReady()
        {
            GameHolder? holder = gameSessionService.GetGameSession(Context.Guild.Id);
            if(holder == null || holder.Engine.CurrentCycle is not IntroductoryDayCycle introCycle)
                return;

            Player? player = holder.Engine.Players.FirstOrDefault(p => p.Id == Context.User.Id);
            if(player == null)
            {
                await RespondAsync(Miscellaneous.YouNotInGameError, ephemeral: true);
                return;
            }

            introCycle.SetPlayerReady(player);

            string generalDirectorWarning = player.Role.IntoSignature().MapRole() == RoleVisual.GENERAL_DIRECTOR ? string.Empty : Miscellaneous.DontShowDocumentWarning;

            Embed embed = new EmbedBuilder()
                .WithTitle(Miscellaneous.YourContractInfo)
                .WithDescription($"{Miscellaneous.YourPositionTitle} {player.Role.IntoSignature().MapRole().GetLocalizedName()}\n\n{generalDirectorWarning}")
                .WithColor(Color.Default)
                .Build();

            await RespondAsync(Miscellaneous.AttendanceConfirmMessage, embed: embed, ephemeral: true);

            await gameSessionService.TryFinishCycle(holder);
        }

        #endregion

        #region Morning Cycle

        [ComponentInteraction("action_morning_start_board")]
        public async Task HandleMorningStartBoard()
        {
            GameHolder? holder = gameSessionService.GetGameSession(Context.Guild.Id);
            if(holder == null || holder.Engine.CurrentCycle is not MorningCycle morningCycle) return;

            Player? player = holder.Engine.Players.FirstOrDefault(p => p.Id == Context.User.Id);
            if(player == null) return;

            if(player.Role.IntoSignature().MapRole() != RoleVisual.GENERAL_DIRECTOR)
            {
                await RespondAsync(Miscellaneous.OnlyDirectorStartBoardError, ephemeral: true);
                return;
            }

            morningCycle.SetDirectorReady();

            await RespondAsync(Miscellaneous.YouOpenBoardMessage, ephemeral: true);

            await gameSessionService.TryFinishCycle(holder);
        }

        #endregion

        #region Director Board Cycle

        [ComponentInteraction("action_director_target_select")]
        public async Task HandleDirectorTargetSelect(string[] selectedValues)
        {
            GameHolder? holder = gameSessionService.GetGameSession(Context.Guild.Id);
            if(holder == null || holder.Engine.CurrentCycle is not DirectorBoardCycle boardCycle) return;

            Player? player = holder.Engine.Players.FirstOrDefault(p => p.Id == Context.User.Id);
            if(player is null || player.Role.IntoSignature().MapRole() != RoleVisual.GENERAL_DIRECTOR)
            {
                await RespondAsync(Miscellaneous.OnlyDirectorMakeDecision, ephemeral: true);
                return;
            }

            if(ulong.TryParse(selectedValues[0], out ulong targetId))
            {
                DirectorPendingSelections[Context.User.Id] = targetId;
                await RespondAsync(Miscellaneous.CandidateRegisteredToFireInfo, ephemeral: true);
            }
        }

        [ComponentInteraction("action_director_veto")]
        public async Task HandleDirectorVeto()
        {
            SocketMessageComponent component = (SocketMessageComponent)Context.Interaction;
            GameHolder? holder = gameSessionService.GetGameSession(Context.Guild.Id);
            if(holder == null || holder.Engine.CurrentCycle is not DirectorBoardCycle boardCycle) return;

            Player? player = holder.Engine.Players.FirstOrDefault(p => p.Id == Context.User.Id);
            if(player is null || player.Role.IntoSignature().MapRole() != RoleVisual.GENERAL_DIRECTOR)
            {
                await RespondAsync(Miscellaneous.UseVetoDeniedError, ephemeral: true);
                return;
            }

            if(holder.Engine.GeneralDirector.VetoCount <= 0 || boardCycle.VetoUsed)
            {
                await RespondAsync(Miscellaneous.YouNotHaveVetoError, ephemeral: true);
                return;
            }

            boardCycle.UseVeto();
            DirectorPendingSelections.TryRemove(Context.User.Id, out _);

            Embed embed = new EmbedBuilder()
                .WithTitle(Miscellaneous.VetoUsedWarning)
                .WithDescription(Miscellaneous.VetoUsedInfo)
                .WithColor(Color.Red)
                .Build();

            MessageComponent components = DiscordStageController.BuildDirectorBoardComponents(Context.Guild, holder, boardCycle);

            await component.UpdateAsync(x =>
            {
                x.Embed = embed;
                x.Components = components;
            });
        }

        [ComponentInteraction("action_director_fire")]
        public async Task HandleDirectorFire()
        {
            SocketMessageComponent component = (SocketMessageComponent)Context.Interaction;
            GameHolder? holder = gameSessionService.GetGameSession(Context.Guild.Id);
            if(holder == null || holder.Engine.CurrentCycle is not DirectorBoardCycle boardCycle) return;

            Player? player = holder.Engine.Players.FirstOrDefault(p => p.Id == Context.User.Id);
            if(player is null || player.Role.IntoSignature().MapRole() != RoleVisual.GENERAL_DIRECTOR)
            {
                await RespondAsync(Miscellaneous.OnlyDirectorMakeDecision, ephemeral: true);
                return;
            }

            if(!DirectorPendingSelections.TryGetValue(Context.User.Id, out ulong targetId) || targetId == 0)
            {
                await RespondAsync(Miscellaneous.NoPlayerChosenToFireError, ephemeral: true);
                return;
            }

            Player? chosenPlayer = holder.Engine.Players.FirstOrDefault(p => p.Id == targetId);
            boardCycle.ChooseCandidate(chosenPlayer);
            DirectorPendingSelections.TryRemove(Context.User.Id, out _);

            Embed embed = new EmbedBuilder()
                .WithTitle(Miscellaneous.DecisionMadeTitle)
                .WithDescription(string.Format(Miscellaneous.DirectorMadeDesicionInfo, targetId))
                .WithColor(Color.DarkRed)
                .Build();

            await component.UpdateAsync(x =>
            {
                x.Embed = embed;
                x.Components = new ComponentBuilder().Build();
            });

            await gameSessionService.TryFinishCycle(holder);
        }

        [ComponentInteraction("action_director_skip")]
        public async Task HandleDirectorSkip()
        {
            SocketMessageComponent component = (SocketMessageComponent)Context.Interaction;
            GameHolder? holder = gameSessionService.GetGameSession(Context.Guild.Id);
            if(holder == null || holder.Engine.CurrentCycle is not DirectorBoardCycle boardCycle) return;

            Player? player = holder.Engine.Players.FirstOrDefault(p => p.Id == Context.User.Id);
            if(player is null || player.Role.IntoSignature().MapRole() != RoleVisual.GENERAL_DIRECTOR)
            {
                await RespondAsync(Miscellaneous.OnlyDirectorMakeDecision, ephemeral: true);
                return;
            }

            if(!boardCycle.CanSkipTurn)
            {
                await RespondAsync(Miscellaneous.CannotSkipBoardError, ephemeral: true);
                return;
            }

            boardCycle.ChooseCandidate(null);
            DirectorPendingSelections.TryRemove(Context.User.Id, out _);

            Embed embed = new EmbedBuilder()
                .WithTitle(Miscellaneous.DecisionMadeTitle)
                .WithDescription(Miscellaneous.DirectorDecisionMercyInfo)
                .WithColor(Color.Green)
                .Build();

            await component.UpdateAsync(x =>
            {
                x.Embed = embed;
                x.Components = new ComponentBuilder().Build();
            });

            await gameSessionService.TryFinishCycle(holder);
        }

        #endregion

        #region Evening Cycle

        [ComponentInteraction("action_finish_evening")]
        public async Task HandleFinishEvening()
        {
            GameHolder? holder = gameSessionService.GetGameSession(Context.Guild.Id);
            if(holder == null || holder.Engine.CurrentCycle is not EveningCycle eveningCycle) return;

            Player? player = holder.Engine.Players.FirstOrDefault(p => p.Id == Context.User.Id);
            if(player is null) return;

            //Only the General Director or the fired employee can end the day
            if(player.Role.IntoSignature().MapRole() != RoleVisual.GENERAL_DIRECTOR && player.Id != eveningCycle.Elected.Id)
            {
                await RespondAsync(Miscellaneous.OnlyDirectorOrFiredPlayerEndDayError, ephemeral: true);
                return;
            }

            eveningCycle.Fire();

            if(player.Id == eveningCycle.Elected.Id)
            {
                await RespondAsync(Miscellaneous.FinalWordRecorderTitle, ephemeral: true);
            }
            else
            {
                await RespondAsync(Miscellaneous.DirectorInterruptDebateInfo, ephemeral: true);
            }

            await gameSessionService.TryFinishCycle(holder);
        }

        #endregion

        #region Overtime Cycle

        [ComponentInteraction("action_open_terminal")]
        public async Task HandleOpenTerminal()
        {
            GameHolder? holder = gameSessionService.GetGameSession(Context.Guild.Id);
            if(holder == null || holder.Engine.CurrentCycle is not OvertimeCycle overtimeCycle) return;

            Player? player = holder.Engine.Players.FirstOrDefault(p => p.Id == Context.User.Id);
            if(player == null || !player.IsAlive)
            {
                await RespondAsync(Miscellaneous.NoAccessTerminalError, ephemeral: true);
                return;
            }

            Embed embed = new EmbedBuilder()
                .WithTitle(Miscellaneous.TerminalTitle)
                .WithDescription($"{Miscellaneous.PositionTitle} {player.Role.IntoSignature().MapRole().GetLocalizedName()}\n\n{Miscellaneous.SelectTargetMessage}")
                .WithColor(Color.DarkBlue)
                .Build();

            ComponentBuilder builder = new ComponentBuilder();
            ExecutorType executorType = player.Role.GetExecutorType();

            int expectedTargetsCount = executorType switch
            {
                ExecutorType.NONE => 0,
                ExecutorType.TARGET => 1,
                ExecutorType.TARGET_TARGET => 2,
                ExecutorType.TARGET_ANYTARGET => 2,
                _ => 0
            };

            PendingSelections[player.Id] = new ulong[expectedTargetsCount];

            switch(executorType)
            {
                case ExecutorType.NONE:
                    break;
                case ExecutorType.TARGET:
                {
                    //Add options based on alive players in the game
                    IEnumerable<Player> alivePlayers = holder.Engine.AlivePlayers.Where(p => p.IsAlive && p.Id != player.Id);
                    List<SelectMenuOptionBuilder> options = new List<SelectMenuOptionBuilder>();
                    foreach(Player target in alivePlayers)
                    {
                        SocketGuildUser targetUser = Context.Guild.GetUser(target.Id);
                        string targetName = targetUser?.Username ?? $"Employee {target.Id}";
                        options.Add(new SelectMenuOptionBuilder(targetName, target.Id.ToString()));
                    }

                    //Choice of a single target
                    SelectMenuBuilder selectMenu = new SelectMenuBuilder()
                        .WithCustomId("action_overtime_target_select_0")
                        .WithMaxValues(1)
                        .WithPlaceholder(Miscellaneous.SelectFirstTargetMessage)
                        .WithOptions(options);

                    builder.WithSelectMenu(selectMenu);
                    builder.WithButton(Miscellaneous.ClearTargetSelectionButton, "action_overtime_clear_select", ButtonStyle.Secondary);
                    break;
                }
                case ExecutorType.TARGET_TARGET:
                {
                    IEnumerable<Player> alivePlayers = holder.Engine.AlivePlayers.Where(p => p.IsAlive && p.Id != player.Id);
                    List<SelectMenuOptionBuilder> options = new List<SelectMenuOptionBuilder>();
                    foreach(Player target in alivePlayers)
                    {
                        SocketGuildUser targetUser = Context.Guild.GetUser(target.Id);
                        string targetName = targetUser?.Username ?? $"Employee {target.Id}";
                        options.Add(new SelectMenuOptionBuilder(targetName, target.Id.ToString()));
                    }

                    //Choice of two targets
                    SelectMenuBuilder selectMenu1 = new SelectMenuBuilder()
                        .WithCustomId("action_overtime_target_select_0")
                        .WithMaxValues(1)
                        .WithPlaceholder(Miscellaneous.SelectFirstTargetMessage)
                        .WithOptions(options);

                    SelectMenuBuilder selectMenu2 = new SelectMenuBuilder()
                        .WithCustomId("action_overtime_target_select_1")
                        .WithMaxValues(1)
                        .WithPlaceholder(Miscellaneous.SelectSecondTargetMessage)
                        .WithOptions(options);

                    builder.WithSelectMenu(selectMenu1);
                    builder.WithSelectMenu(selectMenu2);
                    builder.WithButton(Miscellaneous.ClearTargetSelectionButton, "action_overtime_clear_select", ButtonStyle.Secondary);
                    break;
                }
                case ExecutorType.TARGET_ANYTARGET:
                {
                    IEnumerable<Player> alivePlayers = holder.Engine.AlivePlayers.Where(p => p.IsAlive);
                    List<SelectMenuOptionBuilder> options1 = new List<SelectMenuOptionBuilder>();
                    List<SelectMenuOptionBuilder> options2 = new List<SelectMenuOptionBuilder>();
                    foreach(Player target in alivePlayers)
                    {
                        SocketGuildUser targetUser = Context.Guild.GetUser(target.Id);
                        string targetName = targetUser?.Username ?? $"Employee {target.Id}";
                        if(target.Id != player.Id)
                        {
                            options1.Add(new SelectMenuOptionBuilder(targetName, target.Id.ToString()));
                        }
                        
                        if(target.Id == player.Id)
                        {
                            options2.Add(new SelectMenuOptionBuilder(targetName, target.Id.ToString(), Miscellaneous.YouCanSelectYourselfInfo));
                        }
                        else
                        {
                            options2.Add(new SelectMenuOptionBuilder(targetName, target.Id.ToString()));
                        }
                    }

                    //Choice of two targets
                    SelectMenuBuilder selectMenu1 = new SelectMenuBuilder()
                        .WithCustomId("action_overtime_target_select_0")
                        .WithMaxValues(1)
                        .WithPlaceholder(Miscellaneous.SelectFirstTargetMessage)
                        .WithOptions(options1);

                    //The player can choose themselves as the second target if they want
                    SelectMenuBuilder selectMenu2 = new SelectMenuBuilder()
                        .WithCustomId("action_overtime_target_select_1")
                        .WithMaxValues(1)
                        .WithPlaceholder(Miscellaneous.SelectSecondTargetMessage)
                        .WithOptions(options2);

                    builder.WithSelectMenu(selectMenu1);
                    builder.WithSelectMenu(selectMenu2);
                    builder.WithButton(Miscellaneous.ClearTargetSelectionButton, "action_overtime_clear_select", ButtonStyle.Secondary);
                    break;
                }
            }

            builder.WithButton(Miscellaneous.ReadyButton, "action_overtime_ready", ButtonStyle.Success);

            await RespondAsync(embed: embed, components: builder.Build(), ephemeral: true);
        }

        [ComponentInteraction("action_overtime_target_select_*")]
        public async Task HandleOvertimeTargetSelection(string targetIndexStr, string[] selectedValues)
        {
            int index;
            ulong targetId;

            if(!int.TryParse(targetIndexStr, out index) || !ulong.TryParse(selectedValues[0], out targetId))
                return;

            ulong[] selections;
            if(PendingSelections.TryGetValue(Context.User.Id, out selections!))
            {
                if(index >= 0 && index < selections.Length)
                {
                    selections[index] = targetId;
                }
            }
            else
            {
                await RespondAsync(Miscellaneous.TerminalSessionExpiredWarning, ephemeral: true);
            }
        }

        [ComponentInteraction("action_overtime_clear_select")]
        public async Task HandleOvertimeClearSelect()
        {
            ulong[] selections;
            if(PendingSelections.TryGetValue(Context.User.Id, out selections!))
            {
                for(int i = 0; i < selections.Length; i++)
                {
                    selections[i] = 0;
                }

                await RespondAsync(Miscellaneous.TargetSelectionClearedMessage, ephemeral: true);
            }
        }

        [ComponentInteraction("action_overtime_ready")]
        public async Task HandleOvertimeReady()
        {
            GameHolder? holder = gameSessionService.GetGameSession(Context.Guild.Id);
            if(holder == null || holder.Engine.CurrentCycle is not OvertimeCycle overtimeCycle) return;

            Player? player = holder.Engine.Players.FirstOrDefault(p => p.Id == Context.User.Id);
            if(player == null) return;

            //Validation
            ulong[] selections;
            if(PendingSelections.TryGetValue(Context.User.Id, out selections!))
            {
                bool hasAnySelection = selections.Any(s => s != 0);
                bool hasMultipleSelection = selections.All(s => s != 0);

                if(hasAnySelection && !hasMultipleSelection)
                {
                    await RespondAsync(Miscellaneous.TargetSelectionNotFinishedError, ephemeral: true);
                    return;
                }

                try
                {
                    if(hasAnySelection)
                    {
                        IRoleOwner[] targets = new IRoleOwner[selections.Length];
                        for(int i = 0; i < targets.Length; i++)
                        {
                            IRoleOwner? target = holder.Engine.AlivePlayers.FirstOrDefault(p => p.Id == selections[i]);
                            targets[i] = target ?? throw new InvalidOperationException(Miscellaneous.TargetNotFoundError);
                        }
                        overtimeCycle.ConfirmAction(player, targets);
                    }
                }
                catch(Exception ex)
                {
                    await RespondAsync($"{Miscellaneous.UnexpectedError} {ex.Message}", ephemeral: true);
                    return;
                }
                finally
                {
                    //Clear cache
                    PendingSelections.TryRemove(Context.User.Id, out _);
                }
            }

            overtimeCycle.SetPlayerReady(player);

            await RespondAsync(Miscellaneous.OvertimeReadyMessage, ephemeral: true);

            await gameSessionService.TryFinishCycle(holder);
        }

        #endregion
    }
}