using GameLogic;
using GameLogic.Attributes;
using GameLogic.Cycles;
using GameLogic.Interfaces;
using GameLogic.ParanoiaCorp;
using GameLogic.ParanoiaCorp.Cycles;
using GameLogic.ParanoiaCorp.Extensions;
using GameLogic.ParanoiaCorp.Roles;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using WebServer.Hubs;
using WebServer.Model.Game;
using WebServer.Shared.HubObjects;
using WebServer.Shared.ParanoiaCorp.Extensions;
using WebServer.Shared.ParanoiaCorp.GameObjects;
using Team = GameLogic.ParanoiaCorp.Attributes.Team;
using ChatScope = GameLogic.ParanoiaCorp.Attributes.ChatScope;
using ChatScopeAttribute = GameLogic.ParanoiaCorp.Attributes.ChatScopeAttribute;
using ActionLog = GameLogic.Cycles.Night.ActionLog;


namespace WebServer.Model.ParanoiaCorp
{
    public class ParanoiaGameSession
    {
        private readonly object syncRoot = new();
        private readonly IHubContext<LobbyHub> hubContext;

        public int RoomId { get; }
        public GameEngine Engine { get; }
        public IReadOnlyList<PlayerDomain> Players { get; }
        public DateTime StartedAt { get; } = DateTime.UtcNow;

        private readonly ConcurrentDictionary<ulong, ulong[]> pendingOvertimeSelections = new();
        private readonly ConcurrentDictionary<ulong, bool> overtimeReadyStatus = new();
        private readonly ConcurrentDictionary<ulong, bool> introReadyStatus = new();
        private readonly ConcurrentDictionary<ulong, List<ReportItemDTO>> playerPrivateReports = new();
        private readonly ConcurrentDictionary<ulong, List<ReportItemDTO>> playerPostponedReports = new();
        private readonly List<NewsItemDTO> currentMorningNews = new();
        private readonly List<ParanoiaChatMessageDTO> chatMessages = new();

        private ulong? directorSelectedCandidate;
        private EveningCycle? currentEveningCycle;

        public ParanoiaGameSession(int roomId, PlayerDomain[] players, IHubContext<LobbyHub> hubContext)
        {
            RoomId = roomId;
            Players = players;
            this.hubContext = hubContext;

            var logicPlayers = players.Select(p => p.GameRepresentation).ToArray();
            var initialCycle = new IntroductoryDayCycle();
            Engine = new GameEngine(logicPlayers, initialCycle);
            initialCycle.Engine = Engine;

            // Welcome announcement
            currentMorningNews.Add(new NewsItemDTO
            {
                MessageEn = "Special administrative convening. All personnel must review their contracts and sign attendance.",
                MessageRu = "Специальное административное собрание. Все сотрудники обязаны ознакомиться с контрактами и подтвердить явку.",
                Timestamp = DateTime.UtcNow.ToString("HH:mm:ss"),
                Category = "CONVENING"
            });
        }

        #region Player State View Builder

        public ParanoiaPlayerViewDTO GetPlayerView(ulong userId)
        {
            lock (syncRoot)
            {
                PlayerDomain? domain = Players.FirstOrDefault(p => p.Id == userId);
                if (domain == null)
                {
                    throw new ArgumentException($"Player {userId} not found in room {RoomId}.");
                }

                Player player = domain.GameRepresentation;
                RoleSignature sig = player.Role.IntoSignature();
                RoleInfoDTO roleInfo = ParanoiaRoleCatalogue.GetRoleInfo(sig) ?? new RoleInfoDTO
                {
                    NameEn = player.Role.GetType().Name,
                    NameRu = player.Role.GetType().Name,
                    TeamName = "Corporation",
                    TeamNameRu = "Корпорация",
                    FactionColor = "#0d9488"
                };

                var view = new ParanoiaPlayerViewDTO
                {
                    RoomId = RoomId,
                    FiscalTurn = Engine.Day,
                    Self = new PlayerSelfDTO
                    {
                        Id = player.Id,
                        Username = domain.Nickname,
                        IsAlive = player.IsAlive,
                        RoleSignature = (byte)sig,
                        RoleKey = sig.ToString(),
                        RoleNameEn = roleInfo.NameEn,
                        RoleNameRu = roleInfo.NameRu,
                        Faction = roleInfo.TeamName,
                        FactionRu = roleInfo.TeamNameRu,
                        FactionColor = roleInfo.FactionColor,
                        PortraitKey = sig.ToString(),
                        AbilityEn = roleInfo.AbilityEn,
                        AbilityRu = roleInfo.AbilityRu,
                        DescriptionEn = roleInfo.DescriptionEn,
                        DescriptionRu = roleInfo.DescriptionRu,
                        IsGeneralDirector = player.Role is GeneralDirectorRole,
                        VetoCount = Engine.GeneralDirector.VetoCount
                    }
                };

                // Add usage limits note if applicable
                if (player.Role is AnticrisisManagerRole acm)
                {
                    view.Self.UsageLimitsEn = $"Critical Memos remaining: {acm.Items}";
                    view.Self.UsageLimitsRu = $"Осталось служебных записок: {acm.Items}";
                }
                else if (player.Role is GeneralDirectorRole gd)
                {
                    view.Self.UsageLimitsEn = $"Veto Powers remaining: {gd.VetoCount}";
                    view.Self.UsageLimitsRu = $"Осталось прав вето: {gd.VetoCount}";
                }

                // Roster
                foreach (var p in Players)
                {
                    Player rep = p.GameRepresentation;
                    bool isSelf = rep.Id == userId;
                    bool isGd = rep.Role is GeneralDirectorRole;
                    bool isGameOver = Engine.CurrentCycle is EndGameCycle;
                    bool isDismissedInEvening = currentEveningCycle != null && currentEveningCycle.Elected.Id == rep.Id;
                    bool showRole = isSelf || isGd || isGameOver || isDismissedInEvening || (!rep.IsAlive && Engine.CurrentCycle is not IntroductoryDayCycle);

                    RoleSignature pSig = rep.Role.IntoSignature();
                    RoleInfoDTO? pInfo = ParanoiaRoleCatalogue.GetRoleInfo(pSig);

                    string statusEn = rep.IsAlive ? "Active" : "Dismissed";
                    string statusRu = rep.IsAlive ? "В строю" : "Уволен";

                    if (Engine.CurrentCycle is DirectorBoardCycle dbc && dbc.CandidatesForFiring.Any(c => c.Id == rep.Id))
                    {
                        statusEn = "Shortlisted for Dismissal";
                        statusRu = "Кандидат на увольнение";
                    }

                    view.Roster.Add(new PlayerRosterDTO
                    {
                        Id = rep.Id,
                        Username = p.Nickname,
                        IsAlive = rep.IsAlive,
                        IsReady = GetPlayerReadyStatus(rep.Id),
                        IsHost = false,
                        RevealedRoleKey = showRole ? pSig.ToString() : null,
                        RevealedRoleNameEn = showRole ? pInfo?.NameEn : null,
                        RevealedRoleNameRu = showRole ? pInfo?.NameRu : null,
                        RevealedFaction = showRole ? pInfo?.TeamName : null,
                        RevealedFactionRu = showRole ? pInfo?.TeamNameRu : null,
                        RevealedFactionColor = showRole ? pInfo?.FactionColor : null,
                        StatusEn = statusEn,
                        StatusRu = statusRu
                    });
                }

                // Phase details
                ICycle cycle = Engine.CurrentCycle;

                if (cycle is IntroductoryDayCycle)
                {
                    view.Phase = "Intro";
                    view.PhaseTitleEn = "Introductory Shift";
                    view.PhaseTitleRu = "Вводный Инструктаж";
                    view.DirectiveEn = introReadyStatus.TryGetValue(userId, out bool r) && r
                        ? "Employment contract acknowledged. Waiting for all colleagues to confirm attendance."
                        : "Review your confidential personnel dossier and sign attendance.";
                    view.DirectiveRu = introReadyStatus.TryGetValue(userId, out bool r2) && r2
                        ? "Трудовой договор подтверждён. Ожидание подтверждения явки всеми коллегами."
                        : "Ознакомьтесь с секретным досье и подтвердите явку на смену.";
                }
                else if (cycle is OvertimeCycle overtimeCycle)
                {
                    view.Phase = "Overtime";
                    view.PhaseTitleEn = "Overtime Shift";
                    view.PhaseTitleRu = "Сверхурочная Смена";

                    bool ready = overtimeReadyStatus.TryGetValue(userId, out bool ovrReady) && ovrReady;
                    bool submitted = pendingOvertimeSelections.ContainsKey(userId);

                    if (player.IsAlive)
                    {
                        view.DirectiveEn = ready
                            ? "Overtime report submitted. Waiting for colleagues to finish their tasks."
                            : (submitted ? "Action recorded. Review choices and confirm your readiness." : "Perform confidential overtime duty or skip shift.");
                        view.DirectiveRu = ready
                            ? "Сверхурочный отчёт отправлен. Ожидание завершения задач коллегами."
                            : (submitted ? "Действие записано. Проверьте выбор и подтвердите готовность." : "Выполните секретную сверхурочную задачу или пропустите смену.");

                        view.OvertimeAction = BuildOvertimeActionDTO(player, ready, submitted);
                    }
                    else
                    {
                        view.DirectiveEn = "You have been dismissed from service. You may communicate in the Club of Former Colleagues.";
                        view.DirectiveRu = "Вы уволены из штата компании. Вы можете общаться в Клубе Бывших Коллег.";
                    }
                }
                else if (cycle is MorningCycle morningCycle)
                {
                    view.Phase = "Morning";
                    view.PhaseTitleEn = "Morning Brief";
                    view.PhaseTitleRu = "Утренняя Летучка";

                    bool isDirector = player.Role is GeneralDirectorRole;
                    view.DirectiveEn = isDirector
                        ? "You are the General Director. Review overnight incident reports and convene the Board Meeting."
                        : "Review overnight incidents and personnel reports. Waiting for the General Director to convene the Board Meeting.";
                    view.DirectiveRu = isDirector
                        ? "Вы — Генеральный Директор. Изучите сводки за ночь и откройте заседание Совета Директоров."
                        : "Изучите сводки происшествий за ночь. Ожидание созыва заседания Генеральным Директором.";
                }
                else if (cycle is DirectorBoardCycle boardCycle)
                {
                    view.Phase = "BoardMeeting";
                    view.PhaseTitleEn = "Board Meeting";
                    view.PhaseTitleRu = "Совет Директоров";

                    bool isDirector = player.Role is GeneralDirectorRole;
                    var candidates = boardCycle.CandidatesForFiring.ToList();

                    view.DirectiveEn = isDirector
                        ? "Conducted debate. Select a candidate from the shortlist to dismiss, exercise Veto, or adjourn without dismissal."
                        : "Floor is open for general discussion. Awaiting the General Director's final dismissal ruling.";
                    view.DirectiveRu = isDirector
                        ? "Обсуждение открыто. Выберите кандидата из шорт-листа на увольнение, примените Вето или помилуйте."
                        : "Слово предоставлено коллегам. Ожидание решения Генерального Директора об увольнении.";

                    view.BoardMeeting = new BoardMeetingDTO
                    {
                        IsGeneralDirector = isDirector,
                        VetoCount = Engine.GeneralDirector.VetoCount,
                        VetoUsed = boardCycle.VetoUsed,
                        CanSkipTurn = boardCycle.CanSkipTurn,
                        SelectedCandidate = directorSelectedCandidate,
                        CandidatesForFiring = candidates.Select(c =>
                        {
                            var pDomain = Players.FirstOrDefault(p => p.Id == c.Id);
                            return new TargetOptionDTO
                            {
                                Id = c.Id,
                                Name = pDomain?.Nickname ?? $"Employee #{c.Id}"
                            };
                        }).ToList()
                    };
                }
                else if (cycle is EveningCycle eveningCycle)
                {
                    view.Phase = "Evening";
                    view.PhaseTitleEn = "Evening Adjournment";
                    view.PhaseTitleRu = "Вечернее Подведение Итогов";

                    Player elected = eveningCycle.Elected;
                    PlayerDomain? elDomain = Players.FirstOrDefault(p => p.Id == elected.Id);
                    RoleSignature elSig = elected.Role.IntoSignature();
                    RoleInfoDTO? elInfo = ParanoiaRoleCatalogue.GetRoleInfo(elSig);

                    bool canConfirm = (player.Role is GeneralDirectorRole) || (player.Id == elected.Id);

                    view.DirectiveEn = canConfirm
                        ? "Dismissal execution ready. Sign the termination papers to conclude the workday."
                        : "The General Director has ruled. Witness the dismissed employee's personnel revelation.";
                    view.DirectiveRu = canConfirm
                        ? "Приказ об увольнении готов. Подпишите документы для завершения рабочего дня."
                        : "Решение принято. Ознакомьтесь с раскрытием должности уволенного сотрудника.";

                    view.Evening = new EveningDTO
                    {
                        DismissedEmployeeId = elected.Id,
                        DismissedEmployeeName = elDomain?.Nickname ?? $"Employee #{elected.Id}",
                        DismissedRoleKey = elSig.ToString(),
                        DismissedRoleNameEn = elInfo?.NameEn ?? "Employee",
                        DismissedRoleNameRu = elInfo?.NameRu ?? "Сотрудник",
                        DismissedFaction = elInfo?.TeamName ?? "Corporation",
                        DismissedFactionRu = elInfo?.TeamNameRu ?? "Корпорация",
                        DismissedFactionColor = elInfo?.FactionColor ?? "#0d9488",
                        CanConfirmDismissal = canConfirm
                    };
                }
                else if (cycle is EndGameCycle endGameCycle)
                {
                    view.Phase = "EndGame";
                    view.PhaseTitleEn = "Personnel Audit";
                    view.PhaseTitleRu = "Итоговый Аудит Штаб-Квартиры";

                    Team? winner = endGameCycle.EndGameHistory.Winner;
                    string winEn = winner.HasValue ? $"{winner.Value} VICTORY" : "CORPORATE DEADLOCK (DRAW)";
                    string winRu = winner.HasValue ? $"ПОБЕДА ФРАКЦИИ: {winner.Value}" : "НИЧЬЯ / ВЗАИМНОЕ УНИЧТОЖЕНИЕ";
                    string winColor = winner.HasValue ? GetTeamColor(winner.Value) : "#9ca3af";

                    view.DirectiveEn = "The corporate conflict has resolved. All dossiers and classified operations are revealed.";
                    view.DirectiveRu = "Корпоративное противостояние завершено. Все личные дела и тайные операции раскрыты.";

                    view.EndGame = new EndGameDTO
                    {
                        WinnerFaction = winner?.ToString(),
                        WinnerTitleEn = winEn,
                        WinnerTitleRu = winRu,
                        WinnerColor = winColor,
                        PersonnelRegister = Players.Select(p =>
                        {
                            RoleSignature pSig = p.GameRepresentation.Role.IntoSignature();
                            RoleInfoDTO? pInfo = ParanoiaRoleCatalogue.GetRoleInfo(pSig);
                            return new PersonnelRegisterItemDTO
                            {
                                Id = p.Id,
                                Username = p.Nickname,
                                IsAlive = p.GameRepresentation.IsAlive,
                                RoleKey = pSig.ToString(),
                                RoleNameEn = pInfo?.NameEn ?? pSig.ToString(),
                                RoleNameRu = pInfo?.NameRu ?? pSig.ToString(),
                                Faction = pInfo?.TeamName ?? "Corporation",
                                FactionRu = pInfo?.TeamNameRu ?? "Корпорация",
                                FactionColor = pInfo?.FactionColor ?? "#0d9488"
                            };
                        }).ToList()
                    };
                }

                // News & Reports
                view.PublicNews = currentMorningNews.ToList();

                if (playerPrivateReports.TryGetValue(userId, out var reports))
                {
                    view.ConfidentialReports = reports.ToList();
                }

                // Available Chat Scopes
                view.AvailableChatScopes = GetAuthorizedChatScopes(player);

                return view;
            }
        }

        private bool GetPlayerReadyStatus(ulong userId)
        {
            if (Engine.CurrentCycle is IntroductoryDayCycle)
            {
                return introReadyStatus.TryGetValue(userId, out bool r) && r;
            }
            if (Engine.CurrentCycle is OvertimeCycle)
            {
                return overtimeReadyStatus.TryGetValue(userId, out bool r) && r;
            }
            return true;
        }

        private OvertimeActionDTO BuildOvertimeActionDTO(Player player, bool isReady, bool isSubmitted)
        {
            RoleSignature sig = player.Role.IntoSignature();
            RoleInfoDTO? info = ParanoiaRoleCatalogue.GetRoleInfo(sig);
            ExecutorAttribute execAttr = sig.GetExecutorAttribute();

            var dto = new OvertimeActionDTO
            {
                CanAct = execAttr.EType != ExecutorType.NONE && player.IsAlive,
                ExecutorType = execAttr.EType.ToString(),
                IsActionSubmitted = isSubmitted,
                IsReady = isReady
            };

            if (pendingOvertimeSelections.TryGetValue(player.Id, out var pending))
            {
                dto.PendingSelections = pending;
            }

            var aliveColleagues = Engine.AlivePlayers
                .Where(p => p.Id != player.Id)
                .Select(p => new TargetOptionDTO
                {
                    Id = p.Id,
                    Name = Players.FirstOrDefault(x => x.Id == p.Id)?.Nickname ?? $"Employee #{p.Id}"
                })
                .ToList();

            var allAlive = Engine.AlivePlayers
                .Select(p => new TargetOptionDTO
                {
                    Id = p.Id,
                    Name = (p.Id == player.Id) ? $"{Players.FirstOrDefault(x => x.Id == p.Id)?.Nickname} (Self)" : (Players.FirstOrDefault(x => x.Id == p.Id)?.Nickname ?? $"Employee #{p.Id}")
                })
                .ToList();

            switch (execAttr.EType)
            {
                case ExecutorType.NONE:
                    dto.CanAct = false;
                    break;
                case ExecutorType.TARGET:
                    dto.Target1LabelEn = sig switch
                    {
                        RoleSignature.SECURITY_SPECIALIST => "Colleague to investigate for loyalty",
                        RoleSignature.AUDITOR => "Colleague for financial forensic audit",
                        RoleSignature.HR_MANAGER => "Colleague's personnel file to protect",
                        RoleSignature.SYSTEM_ADMINISTRATOR => "Colleague to block from terminal access",
                        RoleSignature.ANTICRISIS_MANAGER => "Colleague to target with Incompetence Memo",
                        RoleSignature.SPY => "Colleague to infiltrate and uncover exact role",
                        RoleSignature.HACKER => "Colleague to lock out of system",
                        RoleSignature.SHADOW_DIRECTOR => "Colleague to recruit into the Syndicate",
                        RoleSignature.STARUP_FOUNDER => "Colleague to poach into the Startup",
                        RoleSignature.RAIDER => "Colleague to target for hostile dismissal",
                        RoleSignature.OUTSOURCER => "Dismissed colleague to resurrect as Outsourcer",
                        _ => "Target employee"
                    };
                    dto.Target1LabelRu = sig switch
                    {
                        RoleSignature.SECURITY_SPECIALIST => "Сотрудник для проверки на благонадёжность",
                        RoleSignature.AUDITOR => "Сотрудник для финансового аудита",
                        RoleSignature.HR_MANAGER => "Сотрудник, чьё дело нужно защитить",
                        RoleSignature.SYSTEM_ADMINISTRATOR => "Сотрудник для блокировки терминала",
                        RoleSignature.ANTICRISIS_MANAGER => "Сотрудник для докладной на увольнение",
                        RoleSignature.SPY => "Сотрудник для выяснения точной должности",
                        RoleSignature.HACKER => "Сотрудник для взлома и блокировки",
                        RoleSignature.SHADOW_DIRECTOR => "Сотрудник для вербовки в Синдикат",
                        RoleSignature.STARUP_FOUNDER => "Сотрудник для переманивания в Стартап",
                        RoleSignature.RAIDER => "Сотрудник для рейдерского увольнения",
                        RoleSignature.OUTSOURCER => "Уволенный сотрудник для воскрешения",
                        _ => "Сотрудник-цель"
                    };

                    if (sig == RoleSignature.OUTSOURCER)
                    {
                        // Outsourcer targets dismissed employees
                        dto.EligibleTargets1 = Players
                            .Where(p => !p.GameRepresentation.IsAlive)
                            .Select(p => new TargetOptionDTO
                            {
                                Id = p.Id,
                                Name = $"{p.Nickname} (Dismissed)"
                            })
                            .ToList();
                    }
                    else
                    {
                        dto.EligibleTargets1 = aliveColleagues;
                    }
                    break;
                case ExecutorType.TARGET_TARGET:
                    // Scrum Master
                    dto.Target1LabelEn = "First colleague to swap";
                    dto.Target1LabelRu = "Первый сотрудник для перестановки";
                    dto.Target2LabelEn = "Second colleague to swap";
                    dto.Target2LabelRu = "Второй сотрудник для перестановки";
                    dto.EligibleTargets1 = aliveColleagues;
                    dto.EligibleTargets2 = aliveColleagues;
                    break;
                case ExecutorType.TARGET_ANYTARGET:
                    // Master of Compromising / Political Strategist
                    dto.Target1LabelEn = "Employee to control";
                    dto.Target1LabelRu = "Сотрудник для перехвата управления";
                    dto.Target2LabelEn = "New target";
                    dto.Target2LabelRu = "Новая цель воздействия";
                    dto.EligibleTargets1 = aliveColleagues;
                    dto.EligibleTargets2 = allAlive;
                    break;
            }

            return dto;
        }

        private List<ChatScopeInfoDTO> GetAuthorizedChatScopes(Player player)
        {
            var scopes = new List<ChatScopeInfoDTO>();
            bool isAlive = player.IsAlive;
            RoleSignature sig = player.Role.IntoSignature();
            ChatScopeAttribute[] attrs = sig.GetChatScopes();

            // Public Boardroom
            bool publicCanWrite = isAlive && (Engine.CurrentCycle is DirectorBoardCycle);
            scopes.Add(new ChatScopeInfoDTO
            {
                Scope = "PUBLIC",
                LabelEn = "Boardroom",
                LabelRu = "Зал Заседаний",
                AudienceBadgeEn = "ALL EMPLOYEES",
                AudienceBadgeRu = "ОБЩЕЕ СОБРАНИЕ",
                CanWrite = publicCanWrite,
                Color = "#475569"
            });

            // Syndicate
            if (attrs.Any(a => a.Scope == ChatScope.SYNDICATE))
            {
                scopes.Add(new ChatScopeInfoDTO
                {
                    Scope = "SYNDICATE",
                    LabelEn = "Syndicate Frequency",
                    LabelRu = "Канал Синдиката",
                    AudienceBadgeEn = "SYNDICATE ONLY (RESTRICTED)",
                    AudienceBadgeRu = "ТОЛЬКО СИНДИКАТ (СОВ. СЕКРЕТНО)",
                    CanWrite = isAlive,
                    Color = "#b91c1c"
                });
            }

            // Shareholder
            if (attrs.Any(a => a.Scope == ChatScope.SHAREHOLDER))
            {
                scopes.Add(new ChatScopeInfoDTO
                {
                    Scope = "SHAREHOLDER",
                    LabelEn = "Shareholder Hotline",
                    LabelRu = "Канал Акционеров",
                    AudienceBadgeEn = "BOARD OF INVESTORS",
                    AudienceBadgeRu = "СОВЕТ АКЦИОНЕРОВ",
                    CanWrite = isAlive,
                    Color = "#ca8a04"
                });
            }

            // Startup
            if (sig == RoleSignature.STARUP_FOUNDER)
            {
                // Founder can READ / SPY on startup chat, but cannot post
                scopes.Add(new ChatScopeInfoDTO
                {
                    Scope = "STARTUP",
                    LabelEn = "Startup Channel",
                    LabelRu = "Канал Стартапа",
                    AudienceBadgeEn = "STARTUP SURVEILLANCE (MONITORING ONLY)",
                    AudienceBadgeRu = "ПРОСЛУШКА СТАРТАПА (ТОЛЬКО ЧТЕНИЕ)",
                    CanWrite = false,
                    Color = "#4d7c0f"
                });
            }
            else if (attrs.Any(a => a.Scope == ChatScope.STARTUP))
            {
                scopes.Add(new ChatScopeInfoDTO
                {
                    Scope = "STARTUP",
                    LabelEn = "Startup Channel",
                    LabelRu = "Канал Стартапа",
                    AudienceBadgeEn = "STARTUP FOUNDATION",
                    AudienceBadgeRu = "КОМАНДА СТАРТАПА",
                    CanWrite = isAlive,
                    Color = "#4d7c0f"
                });
            }

            // Outsource
            if (attrs.Any(a => a.Scope == ChatScope.OUTSOURCE))
            {
                scopes.Add(new ChatScopeInfoDTO
                {
                    Scope = "OUTSOURCE",
                    LabelEn = "Outsource Frequency",
                    LabelRu = "Канал Аутсорса",
                    AudienceBadgeEn = "CONTRACTOR NETWORK",
                    AudienceBadgeRu = "СЕТЬ ПОДРЯДЧИКОВ",
                    CanWrite = isAlive,
                    Color = "#b45309"
                });
            }

            // Fired Club
            bool canSeeFired = !isAlive || (sig == RoleSignature.ALUMNI_MANAGER);
            if (canSeeFired)
            {
                scopes.Add(new ChatScopeInfoDTO
                {
                    Scope = "FIRED",
                    LabelEn = "Club of Former Colleagues",
                    LabelRu = "Клуб Бывших Коллег",
                    AudienceBadgeEn = "DISMISSED PERSONNEL LOUNGE",
                    AudienceBadgeRu = "ЛОЖА УВОЛЕННЫХ СОТРУДНИКОВ",
                    CanWrite = true,
                    Color = "#71717a"
                });
            }

            return scopes;
        }

        private static string GetTeamColor(Team team) => team switch
        {
            Team.CORPORATION => "#0d9488",
            Team.SYNDICATE => "#b91c1c",
            Team.STARTUP => "#4d7c0f",
            Team.OUTSOURCE => "#b45309",
            Team.SINGLES => "#701a75",
            _ => "#64748b"
        };

        #endregion

        #region Player Actions & Cycle Progression

        public async Task HandleIntroReadyAsync(ulong userId)
        {
            lock (syncRoot)
            {
                if (Engine.CurrentCycle is not IntroductoryDayCycle intro) return;
                Player? p = Engine.Players.FirstOrDefault(x => x.Id == userId);
                if (p == null) return;

                intro.SetPlayerReady(p);
                introReadyStatus[userId] = true;

                if (intro.CanFinish())
                {
                    Engine.NextTurn(); // Advances to OvertimeCycle (Turn 0)
                    PrepareOvertimeCycle();
                }
            }

            await BroadcastStateUpdateAsync();
        }

        public async Task HandleOvertimeActionAsync(ulong userId, ulong[] targets, bool isSkip)
        {
            lock (syncRoot)
            {
                if (Engine.CurrentCycle is not OvertimeCycle overtime) return;
                Player? p = Engine.Players.FirstOrDefault(x => x.Id == userId);
                if (p == null || !p.IsAlive) return;

                if (isSkip || targets == null || targets.Length == 0)
                {
                    pendingOvertimeSelections.TryRemove(userId, out _);
                }
                else
                {
                    pendingOvertimeSelections[userId] = targets;

                    // Register action in engine
                    IRoleOwner[] targetOwners = targets
                        .Select(tId => Engine.Players.FirstOrDefault(x => x.Id == tId))
                        .Where(x => x != null)
                        .Cast<IRoleOwner>()
                        .ToArray();

                    if (targetOwners.Length > 0)
                    {
                        try
                        {
                            overtime.ConfirmAction(p, targetOwners);
                        }
                        catch
                        {
                            // Ignore re-confirmation exceptions if already added
                        }
                    }
                }
            }

            await BroadcastStateUpdateAsync();
        }

        public async Task HandleOvertimeReadyAsync(ulong userId)
        {
            bool shouldAdvance = false;

            lock (syncRoot)
            {
                if (Engine.CurrentCycle is not OvertimeCycle overtime) return;
                Player? p = Engine.Players.FirstOrDefault(x => x.Id == userId);
                if (p == null || !p.IsAlive) return;

                overtime.SetPlayerReady(p);
                overtimeReadyStatus[userId] = true;

                if (overtime.CanFinish())
                {
                    shouldAdvance = true;
                    // Execute actions & advance to Morning
                    Engine.NextTurn(); // Calls ExecuteActions(), creates MorningCycle
                    ProcessMorningCycle();
                }
            }

            if (shouldAdvance)
            {
                await BroadcastMorningAnnouncementsAsync();
            }

            await BroadcastStateUpdateAsync();
        }

        private void PrepareOvertimeCycle()
        {
            overtimeReadyStatus.Clear();
            pendingOvertimeSelections.Clear();

            // Deliver any postponed reports due for Overtime (e.g. Auditor reports)
            foreach (var kvp in playerPostponedReports)
            {
                if (kvp.Value.Count > 0)
                {
                    if (!playerPrivateReports.ContainsKey(kvp.Key))
                    {
                        playerPrivateReports[kvp.Key] = new List<ReportItemDTO>();
                    }
                    playerPrivateReports[kvp.Key].AddRange(kvp.Value);
                    kvp.Value.Clear();
                }
            }

            // Notify Anticrisis Managers about their remaining items
            var acms = Engine.AlivePlayers.Where(p => p.Role is AnticrisisManagerRole);
            foreach (var acm in acms)
            {
                var role = (AnticrisisManagerRole)acm.Role;
                if (!playerPrivateReports.ContainsKey(acm.Id))
                {
                    playerPrivateReports[acm.Id] = new List<ReportItemDTO>();
                }
                playerPrivateReports[acm.Id].Add(new ReportItemDTO
                {
                    MessageEn = $"Anticrisis Management budget status: You have {role.Items} Critical Incompetence Memos remaining.",
                    MessageRu = $"Статус антикризисного бюджета: У вас осталось {role.Items} служебных записок на увольнение.",
                    Timestamp = DateTime.UtcNow.ToString("HH:mm:ss")
                });
            }
        }

        private void ProcessMorningCycle()
        {
            if (Engine.CurrentCycle is not MorningCycle morning) return;

            currentMorningNews.Clear();

            // Interpret logs using pure ParanoiaLogFacade
            var facadeResult = ParanoiaLogFacade.ProcessMorningLogs(morning.ActionLogs, Engine, Engine.Day);
            currentMorningNews.AddRange(facadeResult.PublicNews);

            foreach (var kvp in facadeResult.PrivateReports)
            {
                if (!playerPrivateReports.ContainsKey(kvp.Key))
                {
                    playerPrivateReports[kvp.Key] = new List<ReportItemDTO>();
                }
                playerPrivateReports[kvp.Key].AddRange(kvp.Value);
            }

            foreach (var kvp in facadeResult.PostponedReports)
            {
                if (!playerPostponedReports.ContainsKey(kvp.Key))
                {
                    playerPostponedReports[kvp.Key] = new List<ReportItemDTO>();
                }
                playerPostponedReports[kvp.Key].AddRange(kvp.Value);
            }

            // Check if game has ended
            if (morning.ResolveWinner(out Team? winner))
            {
                Engine.NextTurn(); // Advances to EndGameCycle
            }
        }

        public async Task HandleDirectorStartBoardAsync(ulong userId)
        {
            lock (syncRoot)
            {
                if (Engine.CurrentCycle is not MorningCycle morning) return;
                Player? p = Engine.Players.FirstOrDefault(x => x.Id == userId);
                if (p == null || p.Role is not GeneralDirectorRole) return;

                morning.SetDirectorReady();
                directorSelectedCandidate = null;

                if (morning.CanFinish())
                {
                    Engine.NextTurn(); // Advances to DirectorBoardCycle or EndGameCycle
                }
            }

            await BroadcastStateUpdateAsync();
        }

        public async Task HandleDirectorSelectCandidateAsync(ulong userId, ulong candidateId)
        {
            lock (syncRoot)
            {
                if (Engine.CurrentCycle is not DirectorBoardCycle board) return;
                Player? p = Engine.Players.FirstOrDefault(x => x.Id == userId);
                if (p == null || p.Role is not GeneralDirectorRole) return;

                directorSelectedCandidate = candidateId;
            }

            await BroadcastStateUpdateAsync();
        }

        public async Task HandleDirectorUseVetoAsync(ulong userId)
        {
            lock (syncRoot)
            {
                if (Engine.CurrentCycle is not DirectorBoardCycle board) return;
                Player? p = Engine.Players.FirstOrDefault(x => x.Id == userId);
                if (p == null || p.Role is not GeneralDirectorRole) return;

                bool used = board.UseVeto();
                if (used)
                {
                    directorSelectedCandidate = null;
                    currentMorningNews.Add(new NewsItemDTO
                    {
                        MessageEn = "EXECUTIVE VETO EXERCISED. The General Director exercised statutory veto power. Dismissal parameters reset.",
                        MessageRu = "ПРАВО ВЕТО ПРИМЕНЕНО. Генеральный Директор использовал уставное вето. Параметры увольнений сброшены.",
                        Timestamp = DateTime.UtcNow.ToString("HH:mm:ss"),
                        Category = "VETO"
                    });
                }
            }

            await BroadcastStateUpdateAsync();
        }

        public async Task HandleDirectorFireAsync(ulong userId)
        {
            lock (syncRoot)
            {
                if (Engine.CurrentCycle is not DirectorBoardCycle board) return;
                Player? p = Engine.Players.FirstOrDefault(x => x.Id == userId);
                if (p == null || p.Role is not GeneralDirectorRole) return;

                if (!directorSelectedCandidate.HasValue) return;

                Player? target = Engine.Players.FirstOrDefault(x => x.Id == directorSelectedCandidate.Value);
                if (target == null) return;

                board.ChooseCandidate(target);
                directorSelectedCandidate = null;

                Engine.NextTurn(); // Advances to EveningCycle
                currentEveningCycle = Engine.CurrentCycle as EveningCycle;
            }

            await BroadcastStateUpdateAsync();
        }

        public async Task HandleDirectorSkipAsync(ulong userId)
        {
            lock (syncRoot)
            {
                if (Engine.CurrentCycle is not DirectorBoardCycle board) return;
                Player? p = Engine.Players.FirstOrDefault(x => x.Id == userId);
                if (p == null || p.Role is not GeneralDirectorRole) return;

                if (!board.CanSkipTurn) return;

                board.ChooseCandidate(null);
                directorSelectedCandidate = null;

                currentMorningNews.Add(new NewsItemDTO
                {
                    MessageEn = "The General Director granted executive clemency. No employees were dismissed today.",
                    MessageRu = "Генеральный Директор проявил снисхождение. Сегодня никто не был уволен.",
                    Timestamp = DateTime.UtcNow.ToString("HH:mm:ss"),
                    Category = "MERCY"
                });

                Engine.NextTurn(); // Advances directly to OvertimeCycle
                PrepareOvertimeCycle();
            }

            await BroadcastStateUpdateAsync();
        }

        public async Task HandleEveningFinishAsync(ulong userId)
        {
            lock (syncRoot)
            {
                if (Engine.CurrentCycle is not EveningCycle evening) return;
                Player? p = Engine.Players.FirstOrDefault(x => x.Id == userId);
                if (p == null) return;

                // Either the General Director or the Dismissed player can finalize
                if (p.Role is not GeneralDirectorRole && p.Id != evening.Elected.Id) return;

                evening.Fire(); // Elected.Kill()
                currentEveningCycle = null;

                currentMorningNews.Add(new NewsItemDTO
                {
                    MessageEn = $"Termination order executed. Employee #{evening.Elected.Id} was relieved of all corporate duties.",
                    MessageRu = $"Приказ об увольнении приведен в исполнение. Сотрудник #{evening.Elected.Id} отстранён от должности.",
                    Timestamp = DateTime.UtcNow.ToString("HH:mm:ss"),
                    Category = "DISMISSAL"
                });

                // Check winner
                var morning = new MorningCycle(Engine, Array.Empty<ActionLog>());
                if (morning.ResolveWinner(out Team? winner))
                {
                    Engine.NextTurn();
                }
                else
                {
                    Engine.NextTurn(); // Advances to OvertimeCycle
                    PrepareOvertimeCycle();
                }
            }

            await BroadcastStateUpdateAsync();
        }

        #endregion

        #region Scoped Chat

        public async Task<bool> SendChatMessageAsync(ulong senderId, string scope, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;

            lock (syncRoot)
            {
                PlayerDomain? domain = Players.FirstOrDefault(p => p.Id == senderId);
                if (domain == null) return false;

                Player player = domain.GameRepresentation;
                var scopes = GetAuthorizedChatScopes(player);
                var matchedScope = scopes.FirstOrDefault(s => s.Scope.Equals(scope, StringComparison.OrdinalIgnoreCase));

                if (matchedScope == null || !matchedScope.CanWrite)
                {
                    return false; // Unauthorized to write in this scope
                }

                var msg = new ParanoiaChatMessageDTO
                {
                    RoomId = RoomId,
                    SenderId = senderId,
                    SenderName = domain.Nickname,
                    Scope = matchedScope.Scope,
                    Content = content.Trim(),
                    Timestamp = DateTime.UtcNow.ToString("HH:mm:ss"),
                    AudienceLabelEn = matchedScope.AudienceBadgeEn,
                    AudienceLabelRu = matchedScope.AudienceBadgeRu
                };

                chatMessages.Add(msg);
            }

            await BroadcastChatMessageAsync(scope);
            return true;
        }

        public List<ParanoiaChatMessageDTO> GetChatHistory(ulong userId, string scope)
        {
            lock (syncRoot)
            {
                PlayerDomain? domain = Players.FirstOrDefault(p => p.Id == userId);
                if (domain == null) return new();

                Player player = domain.GameRepresentation;
                var scopes = GetAuthorizedChatScopes(player);
                if (!scopes.Any(s => s.Scope.Equals(scope, StringComparison.OrdinalIgnoreCase)))
                {
                    return new(); // Unauthorized to read
                }

                return chatMessages
                    .Where(m => m.Scope.Equals(scope, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(m => m.Timestamp)
                    .ToList();
            }
        }

        #endregion

        #region SignalR Broadcasts

        public async Task BroadcastStateUpdateAsync()
        {
            if (hubContext == null) return;
            string groupName = LobbyHubConstants.MapGroupName(RoomId);
            // Notify clients to refresh state
            await hubContext.Clients.Group(groupName).SendAsync("ParanoiaStateChanged", RoomId);
        }

        private async Task BroadcastChatMessageAsync(string scope)
        {
            if (hubContext == null) return;
            string groupName = LobbyHubConstants.MapGroupName(RoomId);
            await hubContext.Clients.Group(groupName).SendAsync("ParanoiaChatUpdated", RoomId, scope);
        }

        private async Task BroadcastMorningAnnouncementsAsync()
        {
            if (hubContext == null) return;
            string groupName = LobbyHubConstants.MapGroupName(RoomId);
            await hubContext.Clients.Group(groupName).SendAsync("ParanoiaMorningBrief", RoomId);
        }

        #endregion
    }
}
