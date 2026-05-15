using Discord;
using Discord.WebSocket;
using DiscordBot.Extensions;
using DiscordBot.Resources;
using GameLogic.Cycles.Night;
using GameLogic.ParanoiaCorp.Attributes;
using GameLogic.ParanoiaCorp.Extensions;
using GameLogic.ParanoiaCorp.Roles;
using WebServer.Shared.ParanoiaCorp.Extensions;
using ActionType = GameLogic.Model.ActionType;

namespace DiscordBot.Model
{
    public static class ActionLogFacade
    {
        public static Queue<PostmanPresenter> ZipLogs(IReadOnlyCollection<ActionLog> logs)
        {
            Queue<PostmanPresenter> queue = new Queue<PostmanPresenter>();

            IEnumerable<ActionLog> controlActions = logs.Where(action => action.Action == ActionType.CONTROL);
            foreach(ActionLog action in controlActions)
            {
                LogContext executorControl = new LogContext(InfoType.EXECUTOR_CONTROL, action.Success);
                queue.Enqueue(new PostmanPresenter(executorControl, action.Executor.Owner?.Id));

                LogContext targetControl = new LogContext(InfoType.TARGET_CONTROL, action.Success);
                queue.Enqueue(new PostmanPresenter(targetControl, action.Target.Owner?.Id));
            }

            IEnumerable<ActionLog> swapActions = logs.Where(action => action.Action == ActionType.SWAP);
            foreach(ActionLog action in swapActions)
            {
                LogContext targetSwap = new LogContext(InfoType.TARGET_SWAP, action.Success);
                queue.Enqueue(new PostmanPresenter(targetSwap, action.Target.Owner?.Id));
            }

            IEnumerable<ActionLog> recruitActions = logs.Where(log => log.Action == ActionType.RECRUIT);
            foreach(ActionLog action in recruitActions)
            {
                if(action.Executor is ShadowDirectorRole)
                {
                    LogContext executorShadowDirector = new LogContext(InfoType.EXECUTOR_SHADOW_DIRECTOR_RECRUIT, action.Success);
                    queue.Enqueue(new PostmanPresenter(executorShadowDirector, action.Executor.Owner?.Id));

                    LogContext targetShadowDirector = new LogContext(InfoType.TARGET_SHADOW_DIRECTOR_RECRUIT, action.Success);
                    queue.Enqueue(new PostmanPresenter(targetShadowDirector, action.Target.Owner?.Id));
                }
                
                if(action.Executor is StartupFounderRole)
                {
                    LogContext executorStartupFounder = new LogContext(InfoType.EXECUTOR_STARTUP_FOUNDER_RECRUIT, action.Success);
                    queue.Enqueue(new PostmanPresenter(executorStartupFounder, action.Executor.Owner?.Id));

                    LogContext targetStartupFounder = new LogContext(InfoType.TARGET_STARTUP_FOUNDER_RECRUIT, action.Success);
                    queue.Enqueue(new PostmanPresenter(targetStartupFounder, action.Target.Owner?.Id));
                }
            }

            IEnumerable<ActionLog> blockActions = logs.Where(action => action.Action == ActionType.BLOCK);
            foreach(ActionLog action in blockActions)
            {
                if(action.Target == action.Executor)
                {
                    LogContext targetBlockSelf = new LogContext(InfoType.TARGET_BLOCK_SELF, action.Success);
                    queue.Enqueue(new PostmanPresenter(targetBlockSelf, action.Target.Owner?.Id));
                }
                else if(action.Target is SystemAdministratorRole || action.Target is HackerRole)
                {
                    LogContext targetBlockOtherBlocker = new LogContext(InfoType.TARGET_BLOCK_OTHER_BLOCKER, action.Success);
                    queue.Enqueue(new PostmanPresenter(targetBlockOtherBlocker, action.Target.Owner?.Id));
                }
                else
                {
                    LogContext targetBlock = new LogContext(InfoType.TARGET_BLOCK, action.Success);
                    queue.Enqueue(new PostmanPresenter(targetBlock, action.Target.Owner?.Id));
                }
            }

            IEnumerable<ActionLog> investigateActions = logs.Where(log => log.Action == ActionType.INVESTIGATE);
            foreach(ActionLog action in investigateActions)
            {
                static RoleVisual CheckAndGetRandomPeacefulRole(ActionLog investigateAction)
                {
                    if(investigateAction.Target is ShadowDirectorRole || investigateAction.Target is StartupFounderRole)
                    {
                        RoleVisual[] corporationRoles = Enum.GetValues<RoleVisual>()
                        .Where(visual => visual.GetTeam() == Team.CORPORATION)
                        .ToArray();
                        Random.Shared.Shuffle(corporationRoles);
                        return corporationRoles[0];
                    }
                    else
                    {
                        return investigateAction.Target.IntoSignature().MapRole();
                    }
                }

                if(action.Executor is SpyRole)
                {
                    RoleVisual investigatedRole = CheckAndGetRandomPeacefulRole(action);
                    LogContext executorSpy = new LogContext(InfoType.EXECUTOR_INVESTIGATE_ROLE, action.Success, investigatedRole);
                    queue.Enqueue(new PostmanPresenter(executorSpy, action.Executor.Owner?.Id));
                }
                else if(action.Executor is SecuritySpecialistRole)
                {
                    InfoType infoType = action.Target.GetTeam() == Team.CORPORATION ? InfoType.EXECUTOR_DETECT_PEACEFUL : InfoType.EXECUTOR_DETECT_DANGEROUS;
                    if(action.Target is ShadowDirectorRole || action.Target is StartupFounderRole)
                    {
                        infoType = InfoType.EXECUTOR_DETECT_PEACEFUL;
                    }

                    LogContext executorSecutirySpecialist = new LogContext(infoType, action.Success);
                    queue.Enqueue(new PostmanPresenter(executorSecutirySpecialist, action.Executor.Owner?.Id));
                }
                else if(action.Executor is AuditorRole)
                {
                    RoleVisual investigatedRole = CheckAndGetRandomPeacefulRole(action);
                    LogContext executor = new LogContext(InfoType.EXECUTOR_INVESTIGATE_ROLE_POSTPONE, action.Success, investigatedRole);
                    queue.Enqueue(new PostmanPresenter(executor, action.Executor.Owner?.Id));
                }
            }

            IEnumerable<ActionLog> fireActions = logs.Where(action => action.Action == ActionType.KILL);
            foreach(ActionLog action in fireActions)
            {
                if(!action.Success)
                {
                    LogContext executorFireImmune = new LogContext(InfoType.EXECUTOR_FIRE_IMMUNE, action.Success);
                    queue.Enqueue(new PostmanPresenter(executorFireImmune, action.Executor.Owner?.Id));
                }
                else if(action.Executor == action.Target)
                {
                    bool swapHappened = logs.Any(log => log.Action == ActionType.SWAP && log.Target == action.Executor);
                    if(swapHappened)
                    {
                        LogContext allSwapIncedent = new LogContext(InfoType.ALL_SWAP_INCEDENT, action.Success);
                        queue.Enqueue(new PostmanPresenter(allSwapIncedent));
                    }
                    else
                    {
                        LogContext targetSuicide = new LogContext(InfoType.TARGET_SUICIDE, action.Success);
                        queue.Enqueue(new PostmanPresenter(targetSuicide, action.Target.Owner?.Id));

                        LogContext otherSuicide = new LogContext(InfoType.ALL_SUICIDE, action.Success);
                        queue.Enqueue(new PostmanPresenter(otherSuicide, Except: action.Target.Owner?.Id));
                    }
                }
                else if(action.Executor is MasterOfCompromisingRole)
                {
                    LogContext targetSyndicateMaster = new LogContext(InfoType.TARGET_SYNDICATE_MASTER_FIRE, action.Success);
                    queue.Enqueue(new PostmanPresenter(targetSyndicateMaster, action.Target.Owner?.Id));

                    LogContext otherSyndicateMaster = new LogContext(InfoType.ALL_SYNDICATE_MASTER_FIRE, action.Success);
                    queue.Enqueue(new PostmanPresenter(otherSyndicateMaster, Except: action.Target.Owner?.Id));
                }
                else if(action.Executor is AnticrisisManagerRole)
                {
                    LogContext targetAnticrisisManager = new LogContext(InfoType.TARGET_ANTICRISIS_FIRE, action.Success);
                    queue.Enqueue(new PostmanPresenter(targetAnticrisisManager, action.Target.Owner?.Id));

                    LogContext otherAnticrisisManager = new LogContext(InfoType.ALL_ANTICRISIS_FIRE, action.Success);
                    queue.Enqueue(new PostmanPresenter(otherAnticrisisManager, Except: action.Target.Owner?.Id));
                }
                else if(action.Executor is RaiderRole)
                {
                    LogContext targetRaider = new LogContext(InfoType.TARGET_RAIDER_FIRE, action.Success);
                    queue.Enqueue(new PostmanPresenter(targetRaider, action.Target.Owner?.Id));

                    LogContext otherRaider = new LogContext(InfoType.ALL_RAIDER_FIRE, action.Success);
                    queue.Enqueue(new PostmanPresenter(otherRaider, Except: action.Target.Owner?.Id));
                }
            }

            IEnumerable<ActionLog> protectActions = logs.Where(log => log.Action == ActionType.PROTECT);
            foreach (ActionLog action in protectActions)
            {
                LogContext executorProtect = new LogContext(InfoType.EXECUTOR_PROTECT, action.Success);
                queue.Enqueue(new PostmanPresenter(executorProtect, action.Executor.Owner?.Id));

                LogContext targetProtect = new LogContext(InfoType.TARGET_PROTECT, action.Success);
                queue.Enqueue(new PostmanPresenter(targetProtect, action.Target.Owner?.Id));
            }

            IEnumerable<ActionLog> ressurectActions = logs.Where(log => log.Action == ActionType.RESSURECT);
            foreach(ActionLog action in ressurectActions)
            {
                LogContext targetRessurect = new LogContext(InfoType.TARGET_RESSURECT, action.Success);
                queue.Enqueue(new PostmanPresenter(targetRessurect, action.Target.Owner?.Id));

                LogContext otherRessurect = new LogContext(InfoType.ALL_RESURRECT, action.Success);
                queue.Enqueue(new PostmanPresenter(otherRessurect, Except: action.Target.Owner?.Id));
            }

            return queue;
        }

        public static async Task<Stack<(ulong user, string message)>> InterpretLogs(Queue<PostmanPresenter> postman, SocketGuild guild, SocketTextChannel generalChannel)
        {
            Stack<(ulong user, string message)> postponedMessages = new Stack<(ulong user, string message)>();
            ICollection<string> newsMessages = new List<string>();
            PostmanPresenter presenter;
            while(postman.TryDequeue(out presenter))
            {
                switch(presenter.Log.Info)
                {
                    case InfoType.EXECUTOR_STARTUP_FOUNDER_RECRUIT:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser executorUser = guild.GetUser(presenter.Receiver.Value);
                            if(presenter.Log.Success)
                            {
                                await SafeSendDMAsync(executorUser, VisitorDescriptions.ExecutorStartupFounderSuccess);
                            }
                            else
                            {
                                await SafeSendDMAsync(executorUser, VisitorDescriptions.ExecutorStartupFounderFail);
                            }
                        }
                        break;
                    }
                    case InfoType.EXECUTOR_SHADOW_DIRECTOR_RECRUIT:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser executorUser = guild.GetUser(presenter.Receiver.Value);
                            if(presenter.Log.Success)
                            {
                                await SafeSendDMAsync(executorUser, VisitorDescriptions.ExecutorShadowDirectorSuccess);
                            }
                            else
                            {
                                await SafeSendDMAsync(executorUser, VisitorDescriptions.ExecutorShadowDirectorFail);
                            }
                        }
                        break;
                    }
                    case InfoType.EXECUTOR_DETECT_DANGEROUS:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser executorUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(executorUser, VisitorDescriptions.ExecutorSecurityDetectDangerous);
                        }
                        break;
                    }
                    case InfoType.EXECUTOR_DETECT_PEACEFUL:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser executorUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(executorUser, VisitorDescriptions.ExecutorSecurityDetectPeaceful);
                        }
                        break;
                    }
                    case InfoType.EXECUTOR_INVESTIGATE_ROLE:
                    {
                        if(presenter.Receiver.HasValue && presenter.Log.Target.HasValue)
                        {
                            SocketGuildUser executorUser = guild.GetUser(presenter.Receiver.Value);

                            RoleVisual targetRole = presenter.Log.Target.Value;
                            string roleName = targetRole.GetLocalizedName();
                            string message = string.Format(VisitorDescriptions.ExecutorInvestigateRole, roleName);
                            await SafeSendDMAsync(executorUser, message);
                        }
                        break;
                    }    
                    case InfoType.EXECUTOR_INVESTIGATE_ROLE_POSTPONE:
                    {
                        if(presenter.Receiver.HasValue && presenter.Log.Target.HasValue)
                        {
                            SocketGuildUser executorUser = guild.GetUser(presenter.Receiver.Value);
                            RoleVisual targetRole = presenter.Log.Target.Value;
                            string roleName = targetRole.GetLocalizedName();
                            string message = string.Format(VisitorDescriptions.ExecutorInvestigateRole, roleName);
                            postponedMessages.Push((presenter.Receiver.Value, message));
                        }
                        break;
                    }
                    case InfoType.EXECUTOR_FIRE_IMMUNE:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser executorUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(executorUser, VisitorDescriptions.ExecutorFireImmune);
                        }
                        break;
                    }
                    case InfoType.EXECUTOR_CONTROL:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser executorUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(executorUser, VisitorDescriptions.ExecutorControl);
                        }
                        break;
                    }
                    case InfoType.EXECUTOR_PROTECT:
                    {
                        if(presenter.Receiver.HasValue && presenter.Log.Success)
                        {
                            SocketGuildUser executorUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(executorUser, VisitorDescriptions.ExecutorProtect);
                        }
                        break;
                    }
                    case InfoType.ALL_SWAP_INCEDENT:
                    {
                        newsMessages.Add(VisitorDescriptions.AllSwapIncident);
                        break;
                    }
                    case InfoType.ALL_SUICIDE:
                    { 
                        newsMessages.Add(VisitorDescriptions.AllSuicide);
                        break; 
                    }
                    case InfoType.ALL_SYNDICATE_MASTER_FIRE:
                    {
                        newsMessages.Add(VisitorDescriptions.AllBlackmailMasterFire);
                        break;
                    }
                    case InfoType.ALL_RAIDER_FIRE:
                    {
                        newsMessages.Add(VisitorDescriptions.AllRaiderFire);
                        break;
                    }
                    case InfoType.ALL_ANTICRISIS_FIRE:
                    {
                        newsMessages.Add(VisitorDescriptions.AllAnticrisisFire);
                        break;
                    }
                    case InfoType.ALL_RESURRECT:
                    {
                        if(presenter.Log.Success)
                        {
                            newsMessages.Add(VisitorDescriptions.AllResurrect);
                        }
                        break;
                    }
                    case InfoType.TARGET_STARTUP_FOUNDER_RECRUIT:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            if(presenter.Log.Success)
                            {
                                await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetStartupFounderSuccess);
                            }
                            else
                            {
                                await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetStartupFounderFail);
                            }
                        }
                        break;
                    }
                    case InfoType.TARGET_SHADOW_DIRECTOR_RECRUIT:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            if(presenter.Log.Success)
                            {
                                await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetShadowDirectorSuccess);
                            }
                            else
                            {
                                await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetShadowDirectorFail);
                            }
                        }
                        break;
                    }
                    case InfoType.TARGET_SWAP:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetSwap);
                        }
                        break;
                    }
                    case InfoType.TARGET_BLOCK:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetBlock);
                        }
                        break;
                    }
                    case InfoType.TARGET_BLOCK_OTHER_BLOCKER:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetBlockBlocker);
                        }
                        break;
                    }
                    case InfoType.TARGET_BLOCK_SELF:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetBlockSelf);
                        }
                        break;
                    }
                    case InfoType.TARGET_PROTECT:
                    {
                        if(presenter.Receiver.HasValue && presenter.Log.Success)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetProtect);
                        }
                        break;
                    }
                    case InfoType.TARGET_SYNDICATE_MASTER_FIRE:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetBlackmailMasterFire);
                        }
                        break;
                    }
                    case InfoType.TARGET_RAIDER_FIRE:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetRaiderFire);
                        }
                        break;
                    }
                    case InfoType.TARGET_ANTICRISIS_FIRE:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetAnticrisisManagerFire);
                        }
                        break;
                    }
                    case InfoType.TARGET_SUICIDE:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetSuicide);
                        }
                        break;
                    }
                    case InfoType.TARGET_RESSURECT:
                    {
                        if(presenter.Receiver.HasValue && presenter.Log.Success)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetResurrect);
                        }
                        break;
                    }
                    case InfoType.TARGET_CONTROL:
                    {
                        if(presenter.Receiver.HasValue)
                        {
                            SocketGuildUser targetUser = guild.GetUser(presenter.Receiver.Value);
                            await SafeSendDMAsync(targetUser, VisitorDescriptions.TargetControl);
                        }
                        break;
                    }
                }
            }

            if(newsMessages.Count > 0)
            {
                await PublishNews(generalChannel, newsMessages);
            }

            return postponedMessages;
        }

        private static async Task SafeSendDMAsync(SocketGuildUser user, string message)
        {
            if(user is null) return;

            try
            {
                await user.SendMessageAsync(message);
            }
            catch
            {
                //Ignore the exception, DMs are closed.
            }
        }

        private static async Task PublishNews(SocketTextChannel generalChannel, ICollection<string> messages)
        {
            if(generalChannel is null) return;

            ContainerBuilder container = new ContainerBuilder()
                .WithAccentColor(Color.LightGrey);

            foreach(string message in messages)
            {
                container.WithTextDisplay(message);
            }

            MessageComponent messageComponent = new ComponentBuilderV2(container).Build();

            await generalChannel.SendMessageAsync("📉 **Publish News:**", components: messageComponent);
        }
    }

    public readonly record struct LogContext(InfoType Info, bool Success, RoleVisual? Target = null);

    public readonly record struct PostmanPresenter(LogContext Log, ulong? Receiver = null, ulong? Except = null);

    public enum InfoType
    {
        EXECUTOR_STARTUP_FOUNDER_RECRUIT,
        EXECUTOR_SHADOW_DIRECTOR_RECRUIT,
        EXECUTOR_DETECT_DANGEROUS,
        EXECUTOR_DETECT_PEACEFUL,
        EXECUTOR_INVESTIGATE_ROLE,
        EXECUTOR_INVESTIGATE_ROLE_POSTPONE,
        EXECUTOR_FIRE_IMMUNE,
        EXECUTOR_CONTROL,
        EXECUTOR_PROTECT,

        ALL_SWAP_INCEDENT,
        ALL_SUICIDE,
        ALL_SYNDICATE_MASTER_FIRE,
        ALL_RAIDER_FIRE,
        ALL_ANTICRISIS_FIRE,
        ALL_RESURRECT,

        TARGET_STARTUP_FOUNDER_RECRUIT,
        TARGET_SHADOW_DIRECTOR_RECRUIT,
        TARGET_SWAP,
        TARGET_BLOCK,
        TARGET_BLOCK_OTHER_BLOCKER,
        TARGET_BLOCK_SELF,
        TARGET_PROTECT,
        TARGET_SYNDICATE_MASTER_FIRE,
        TARGET_RAIDER_FIRE,
        TARGET_ANTICRISIS_FIRE,
        TARGET_SUICIDE,
        TARGET_RESSURECT,
        TARGET_CONTROL
    }
}