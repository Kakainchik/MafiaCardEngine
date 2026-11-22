using GameLogic;
using GameLogic.Cycles.Night;
using GameLogic.ParanoiaCorp;
using GameLogic.ParanoiaCorp.Attributes;
using GameLogic.ParanoiaCorp.Extensions;
using GameLogic.ParanoiaCorp.Roles;
using WebServer.Shared.ParanoiaCorp.Extensions;
using WebServer.Shared.ParanoiaCorp.GameObjects;
using ActionType = GameLogic.Model.ActionType;

namespace WebServer.Model.ParanoiaCorp
{
    public class ParanoiaLogProcessingResult
    {
        public List<NewsItemDTO> PublicNews { get; set; } = new();
        public Dictionary<ulong, List<ReportItemDTO>> PrivateReports { get; set; } = new();
        public Dictionary<ulong, List<ReportItemDTO>> PostponedReports { get; set; } = new();
    }

    public static class ParanoiaLogFacade
    {
        public static ParanoiaLogProcessingResult ProcessMorningLogs(
            IReadOnlyCollection<ActionLog> logs,
            GameEngine engine,
            int currentDay)
        {
            var result = new ParanoiaLogProcessingResult();
            string timestamp = DateTime.UtcNow.ToString("HH:mm:ss");

            void AddPublicNews(string en, string ru, string category = "NOTICE")
            {
                result.PublicNews.Add(new NewsItemDTO
                {
                    MessageEn = en,
                    MessageRu = ru,
                    Timestamp = timestamp,
                    Category = category
                });
            }

            void AddPrivateReport(ulong userId, string en, string ru, bool isPostponed = false)
            {
                var targetDict = isPostponed ? result.PostponedReports : result.PrivateReports;
                if (!targetDict.ContainsKey(userId))
                {
                    targetDict[userId] = new List<ReportItemDTO>();
                }
                targetDict[userId].Add(new ReportItemDTO
                {
                    MessageEn = en,
                    MessageRu = ru,
                    Timestamp = timestamp,
                    IsPostponed = isPostponed
                });
            }

            // 1. Process Control Actions
            foreach (var action in logs.Where(a => a.Action == ActionType.CONTROL))
            {
                if (action.Executor.Owner != null)
                {
                    AddPrivateReport(action.Executor.Owner.Id,
                        "A fake directive was issued under executive stationery. The employee's actions were manipulated.",
                        "Фейковое распоряжение руководства разослано. Поведение сотрудника успешно скорректировано.");
                }
                if (action.Target.Owner != null)
                {
                    AddPrivateReport(action.Target.Owner.Id,
                        "You received an urgent, irregular order from leadership. You were forced to alter your scheduled overtime duties.",
                        "Вы получили срочное внештатное распоряжение руководства. Вам пришлось отложить свои планы и подчиниться.");
                }
            }

            // 2. Process Swap Actions
            foreach (var action in logs.Where(a => a.Action == ActionType.SWAP))
            {
                if (action.Target.Owner != null)
                {
                    AddPrivateReport(action.Target.Owner.Id,
                        "Your shift assignments in the system were shuffled unexpectedly overnight.",
                        "Ваши назначения в корпоративной системе были неожиданно перетасованы за ночь.");
                }
            }

            // 3. Process Recruitment Actions
            foreach (var action in logs.Where(a => a.Action == ActionType.RECRUIT))
            {
                if (action.Executor is ShadowDirectorRole)
                {
                    if (action.Executor.Owner != null)
                    {
                        if (action.Success)
                        {
                            AddPrivateReport(action.Executor.Owner.Id,
                                "The Syndicate's financial proposition was accepted. We have a new agent inside the Corporation.",
                                "Предложение Синдиката принято. У нас появился новый оперативник внутри компании.");
                        }
                        else
                        {
                            AddPrivateReport(action.Executor.Owner.Id,
                                "The candidate proved too principled and refused the offer. They may become an obstacle.",
                                "Кандидат отказался от денег Синдиката. Переманить его не удалось.");
                        }
                    }

                    if (action.Target.Owner != null)
                    {
                        if (action.Success)
                        {
                            AddPrivateReport(action.Target.Owner.Id,
                                "You accepted an offer you could not refuse. Substantial offshore compensation for corporate secrets. You are now a Master of Compromising (Syndicate).",
                                "Вам сделали предложение, от которого невозможно отказаться. Солидное офшорное вознаграждение за компромат. Вы теперь Мастер Компромата (Синдикат).");

                            // Convert to Master of Compromising
                            Player? player = engine.Players.FirstOrDefault(p => p.Id == action.Target.Owner.Id);
                            if (player != null && player.IsAlive)
                            {
                                player.ChangeRole(new MasterOfCompromisingRole());
                            }
                        }
                        else
                        {
                            AddPrivateReport(action.Target.Owner.Id,
                                "A suspicious encrypted collaboration proposal arrived in your inbox. You flagged it as spam.",
                                "На вашу почту поступило подозрительное предложение о тайном сотрудничестве. Вы отправили его в спам.");
                        }
                    }
                }
                else if (action.Executor is StartupFounderRole)
                {
                    if (action.Executor.Owner != null)
                    {
                        if (action.Success)
                        {
                            AddPrivateReport(action.Executor.Owner.Id,
                                "You successfully pitched the venture vision. Another cog in the machine has joined the Startup as an Evangelist.",
                                "Вы успешно продали идею единорога. Еще один сотрудник перешел в Стартап в качестве Евангелиста.");
                        }
                        else
                        {
                            AddPrivateReport(action.Executor.Owner.Id,
                                "The candidate clung to corporate health insurance and quarterly bonuses. Pitch failed.",
                                "Кандидат предпочел стабильный корпоративный оклад и ДМС. Вербовка не удалась.");
                        }
                    }

                    if (action.Target.Owner != null)
                    {
                        if (action.Success)
                        {
                            AddPrivateReport(action.Target.Owner.Id,
                                "You bought into the revolutionary startup vision. Stock options and freedom. You are now an Evangelist (Startup).",
                                "Вы поверили в перспективы венчурного стартапа. Опционы и свобода. Вы теперь Евангелист (Стартап).");

                            Player? player = engine.Players.FirstOrDefault(p => p.Id == action.Target.Owner.Id);
                            if (player != null && player.IsAlive)
                            {
                                player.ChangeRole(new EvangelistRole());
                            }
                        }
                        else
                        {
                            AddPrivateReport(action.Target.Owner.Id,
                                "A headhunter tried to lure you into an unproven startup. You declined.",
                                "Хедхантер пытался переманить вас в сомнительный стартап. Вы вежливо отказались.");
                        }
                    }
                }
            }

            // 4. Process Block Actions
            foreach (var action in logs.Where(a => a.Action == ActionType.BLOCK))
            {
                if (action.Target.Owner != null)
                {
                    if (action.Target == action.Executor)
                    {
                        AddPrivateReport(action.Target.Owner.Id,
                            "Due to an administrative error, you revoked your own security token for the night.",
                            "По глупой ошибке вы заблокировали собственный токен доступа на всю ночь.");
                    }
                    else if (action.Target is SystemAdministratorRole || action.Target is HackerRole)
                    {
                        AddPrivateReport(action.Target.Owner.Id,
                            "You attempted server intrusion, but another system operator intercepted your connection and reset your credentials.",
                            "Вы попытались войти на сервер, но другой администратор перехватил сессию и сбросил ваши пароли.");
                    }
                    else
                    {
                        AddPrivateReport(action.Target.Owner.Id,
                            "Your corporate workstation account was suspended due to 'irregular activity'. Overtime duties halted.",
                            "Ваша учетная запись была временно заблокирована из-за 'подозрительной активности'. Действия отменены.");
                    }
                }
            }

            // 5. Process Investigation Actions
            foreach (var action in logs.Where(a => a.Action == ActionType.INVESTIGATE))
            {
                static (string roleEn, string roleRu) ResolveRoleName(ActionLog invAction)
                {
                    if (invAction.Target is ShadowDirectorRole || invAction.Target is StartupFounderRole)
                    {
                        // Masked as a random loyal Corporation role
                        var corpRoles = new[] { RoleSignature.CLERK, RoleSignature.HR_MANAGER, RoleSignature.INTERN, RoleSignature.SCRUM_MASTER };
                        var picked = corpRoles[Random.Shared.Next(corpRoles.Length)];
                        var info = ParanoiaRoleCatalogue.GetRoleInfo(picked);
                        return (info?.NameEn ?? "Clerk", info?.NameRu ?? "Клерк");
                    }
                    else
                    {
                        var sig = invAction.Target.IntoSignature();
                        var info = ParanoiaRoleCatalogue.GetRoleInfo(sig);
                        return (info?.NameEn ?? "Employee", info?.NameRu ?? "Сотрудник");
                    }
                }

                if (action.Executor is SpyRole)
                {
                    if (action.Executor.Owner != null)
                    {
                        var (rEn, rRu) = ResolveRoleName(action);
                        string targetName = action.Target.Owner?.ToString() ?? "Colleague";
                        AddPrivateReport(action.Executor.Owner.Id,
                            $"According to internal confidential registers, target {targetName} holds position: {rEn}.",
                            $"По данным закрытого штатного расписания, сотрудник {targetName} занимает должность: {rRu}.");
                    }
                }
                else if (action.Executor is SecuritySpecialistRole)
                {
                    if (action.Executor.Owner != null)
                    {
                        bool isPeaceful = action.Target.GetTeam() == Team.CORPORATION;
                        if (action.Target is ShadowDirectorRole || action.Target is StartupFounderRole)
                        {
                            isPeaceful = true; // Masked as peaceful
                        }

                        if (isPeaceful)
                        {
                            AddPrivateReport(action.Executor.Owner.Id,
                                "Background investigation concluded: Employee verified as LOYAL to the Corporation.",
                                "Проверка безопасности завершена: Сотрудник проверен, ЛОЯЛЕН Корпорации.");
                        }
                        else
                        {
                            AddPrivateReport(action.Executor.Owner.Id,
                                "Background investigation concluded: Employee verified as DANGEROUS / Disloyal.",
                                "Проверка безопасности завершена: Сотрудник ОПАСЕН / Неблагонадёжен.");
                        }
                    }
                }
                else if (action.Executor is AuditorRole)
                {
                    if (action.Executor.Owner != null)
                    {
                        var (rEn, rRu) = ResolveRoleName(action);
                        string targetName = action.Target.Owner?.ToString() ?? "Colleague";
                        // Postponed report!
                        AddPrivateReport(action.Executor.Owner.Id,
                            $"[Audit Report #{currentDay}] Investigation concluded: Employee {targetName} confirmed as: {rEn}.",
                            $"[Аудиторское Заключение #{currentDay}] Проверка завершена: Должность сотрудника {targetName}: {rRu}.",
                            isPostponed: true);
                    }
                }
            }

            // 6. Process Fire / Kill Actions
            foreach (var action in logs.Where(a => a.Action == ActionType.KILL))
            {
                if (!action.Success)
                {
                    if (action.Executor.Owner != null)
                    {
                        AddPrivateReport(action.Executor.Owner.Id,
                            "Your dismissal dossier was rejected by legal counsel due to a golden parachute clause.",
                            "Подготовленный компромат на увольнение был отклонен юристами из-за 'золотого парашюта'.");
                    }
                }
                else if (action.Executor == action.Target)
                {
                    bool swapped = logs.Any(l => l.Action == ActionType.SWAP && l.Target == action.Executor);
                    if (swapped)
                    {
                        AddPublicNews(
                            "Severe IT confusion in task tickets. An employee accidentally filed for their own termination!",
                            "Сбой в корпоративной системе задач. Сотрудник по ошибке уволил сам себя из-за путаницы в тикетах!",
                            "INCIDENT");
                    }
                    else
                    {
                        if (action.Target.Owner != null)
                        {
                            AddPrivateReport(action.Target.Owner.Id,
                                "You reached your breaking point with workplace stress and submitted your immediate resignation.",
                                "Вы не выдержали корпоративного давления и написали заявление об уходе по собственному желанию.");
                        }
                        AddPublicNews(
                            "A farewell memo was posted in the company chat. A colleague cracked under pressure and resigned overnight.",
                            "Утром в общем чате появилось прощальное письмо. Один из коллег не выдержал стресса и уволился ночью.",
                            "RESIGNATION");
                    }
                }
                else if (action.Executor is MasterOfCompromisingRole)
                {
                    if (action.Target.Owner != null)
                    {
                        AddPrivateReport(action.Target.Owner.Id,
                            "You discover that your email was compromised and private conversations forwarded to the Director. You have been placed on the dismissal shortlist.",
                            "Утром вы обнаружили, что ваша почта взломана, а переписка передана Директору. Вы внесены в шорт-лист на увольнение.");
                    }
                    AddPublicNews(
                        "Dark rumors circulate the watercooler. An employee will soon be called into the Director's office for misconduct.",
                        "По офису ползут мрачные слухи. Скоро кого-то вызовут на ковёр к Генеральному Директору за грубые нарушения.",
                        "DISMISSAL_THREAT");
                }
                else if (action.Executor is AnticrisisManagerRole)
                {
                    if (action.Target.Owner != null)
                    {
                        AddPrivateReport(action.Target.Owner.Id,
                            "Notice from Anticrisis Management: 'Your role does not fit the new restructuring paradigm.' You are nominated for dismissal.",
                            "Уведомление от антикризисного управления: 'Ваша позиция не вписывается в новую парадигму'. Вы в списке на увольнение.");
                    }
                    AddPublicNews(
                        "Management published an aggressive staff optimization memorandum. Downsizing has commenced.",
                        "Руководство выпустило меморандум об оптимизации кадров. Корпоративные сокращения начались.",
                        "DISMISSAL_THREAT");
                }
                else if (action.Executor is RaiderRole)
                {
                    if (action.Target.Owner != null)
                    {
                        AddPrivateReport(action.Target.Owner.Id,
                            "Hostile raid attempt detected: Your performance KPIs were falsified in the registry. You are listed for termination.",
                            "Рейдерская атака: ваши KPI в системе были сфальсифицированы. Вы выставлены на увольнение.");
                    }
                    AddPublicNews(
                        "Whispers of an aggressive hostile restructuring echo down executive hallways.",
                        "По коридорам разносятся слухи о готовящемся недружественном поглощении и чистке кадров.",
                        "DISMISSAL_THREAT");
                }
            }

            // 7. Process Protection Actions
            foreach (var action in logs.Where(a => a.Action == ActionType.PROTECT))
            {
                if (action.Success)
                {
                    if (action.Executor.Owner != null)
                    {
                        AddPrivateReport(action.Executor.Owner.Id,
                            "You sanitized the employee's dossier and purged questionable logs before the Director could inspect them.",
                            "Вы подчистили личное дело сотрудника и уничтожили сомнительные логи до передачи Директору.");
                    }
                    if (action.Target.Owner != null)
                    {
                        AddPrivateReport(action.Target.Owner.Id,
                            "Colleagues attempted to build a dismissal case against you, but HR expunged the record in time.",
                            "Против вас пытались состряпать компромат на увольнение, но кадровая служба вовремя всё удалила.");
                    }
                }
            }

            // 8. Process Resurrection Actions
            foreach (var action in logs.Where(a => a.Action == ActionType.RESSURECT))
            {
                if (action.Success)
                {
                    if (action.Target.Owner != null)
                    {
                        AddPrivateReport(action.Target.Owner.Id,
                            "You were dismissed, but immediately signed an off-the-books B2B contractor agreement. You are back as an Outsourcer.",
                            "Вас уволили, но вы тут же подписали серый B2B-контракт. Вы вернулись в строй в качестве Аутсорсера.");

                        Player? player = engine.Players.FirstOrDefault(p => p.Id == action.Target.Owner.Id);
                        if (player != null)
                        {
                            player.IsAlive = true;
                            player.ChangeRole(new OutsourcerRole());
                            player.Role.IsAlive = true;
                        }
                    }
                    AddPublicNews(
                        "A familiar departed name has unexpectedly resurfaced on the external vendor subcontractor roster.",
                        "Знакомая фамилия уволенного сотрудника неожиданно появилась в реестре внешних подрядчиков компании.",
                        "RETURN");
                }
            }

            if (result.PublicNews.Count == 0)
            {
                AddPublicNews(
                    "The night shift concluded routinely. No corporate incidents recorded on the morning ledger.",
                    "Ночная смена завершилась без происшествий. В утренней сводке нарушений не зафиксировано.",
                    "ROUTINE");
            }

            return result;
        }
    }
}
