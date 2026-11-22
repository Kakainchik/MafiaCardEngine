using WebServer.Shared.ParanoiaCorp.GameObjects;

namespace WebServer.Model.ParanoiaCorp
{
    public static class ParanoiaRoleCatalogue
    {
        private static readonly Dictionary<RoleSignature, RoleInfoDTO> RolesMap = new();

        static ParanoiaRoleCatalogue()
        {
            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.GENERAL_DIRECTOR,
                RoleKey = "GENERAL_DIRECTOR",
                NameEn = "General Director",
                NameRu = "Генеральный Директор",
                DescriptionEn = "A mandatory, unique, open position. You cannot be targeted or fired during overtime. During the board meeting, you make the sole decision on employee dismissals. You have 2 Veto Powers per game.",
                DescriptionRu = "Обязательная, уникальная, открытая должность. Вас невозможно устранить во время сверхурочных. На совете директоров вы единолично принимаете решения об увольнении. У вас есть 2 права вето на игру.",
                AbilityEn = "Dismiss an employee or exercise Veto power at the Board Meeting.",
                AbilityRu = "Уволить сотрудника или применить право вето на совете директоров.",
                Team = 0,
                TeamName = "Corporation",
                TeamNameRu = "Корпорация",
                FactionIndicator = "🏢",
                FactionColor = "#0d9488", // Teal
                IsUnique = true,
                ExecutorType = "NONE"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.CLERK,
                RoleKey = "CLERK",
                NameEn = "Clerk",
                NameRu = "Клерк",
                DescriptionEn = "Faithful employee of the Corporation. No overtime duties (you sleep peacefully at home). During board meetings, participate in discussions to deduce enemies of the company.",
                DescriptionRu = "Рядовой добросовестный сотрудник Корпорации. Сверхурочных обязанностей нет (ночью спокойно спите дома). На заседаниях участвуете в обсуждениях, вычисляя врагов компании.",
                AbilityEn = "No active overtime ability.",
                AbilityRu = "Нет активной ночной способности.",
                Team = 0,
                TeamName = "Corporation",
                TeamNameRu = "Корпорация",
                FactionIndicator = "🏢",
                FactionColor = "#0d9488",
                IsUnique = false,
                ExecutorType = "NONE"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.SECURITY_SPECIALIST,
                RoleKey = "SECURITY_SPECIALIST",
                NameEn = "Security Specialist",
                NameRu = "Специалист СБ",
                DescriptionEn = "Conducts confidential security checks on colleagues. Detects whether an employee is loyal (Corporation) or dangerous (Syndicate / Outsiders). Note: Shadow Director and Startup Founder have sanitized records and show as peaceful.",
                DescriptionRu = "Проводит конфиденциальные проверки службы безопасности. Определяет, лоялен ли сотрудник (Корпорация) или опасен (Синдикат / другие). Теневой Директор и Основатель Стартапа маскируются под мирных.",
                AbilityEn = "Investigate 1 employee overnight to detect loyalty status.",
                AbilityRu = "Проверить 1 сотрудника сверхурочно на благонадёжность.",
                Team = 0,
                TeamName = "Corporation",
                TeamNameRu = "Корпорация",
                FactionIndicator = "🏢",
                FactionColor = "#0d9488",
                IsUnique = false,
                ExecutorType = "TARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.AUDITOR,
                RoleKey = "AUDITOR",
                NameEn = "Auditor",
                NameRu = "Аудитор",
                DescriptionEn = "Deep forensic analysis of company affairs. During overtime you audit one employee. Due to bureaucratic audit processing, the full findings and exact role are delivered to you at the NEXT overtime shift.",
                DescriptionRu = "Глубокая финансовая проверка дел компании. Во время сверхурочных вы аудируете сотрудника. Из-за бюрократии точный отчет и роль доставляются вам в начале СЛЕДУЮЩИХ сверхурочных.",
                AbilityEn = "Audit 1 colleague (findings revealed during next overtime).",
                AbilityRu = "Провести аудит 1 коллеги (результат в следующих сверхурочных).",
                Team = 0,
                TeamName = "Corporation",
                TeamNameRu = "Корпорация",
                FactionIndicator = "🏢",
                FactionColor = "#0d9488",
                IsUnique = false,
                ExecutorType = "TARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.HR_MANAGER,
                RoleKey = "HR_MANAGER",
                NameEn = "HR Manager",
                NameRu = "HR Менджер",
                DescriptionEn = "Protects employees by concealing questionable entries in personnel files. If someone tries to fire your target overnight, you deflect the dismissal. Note: cannot protect against multiple concurrent attacks.",
                DescriptionRu = "Защищает сотрудников, скрывая сомнительные записи в личных делах. Если на цель собирают компромат на увольнение, вы аннулируете угрозу. Не спасает от двойного удара за ночь.",
                AbilityEn = "Protect 1 colleague's personnel file during overtime.",
                AbilityRu = "Защитить личное дело 1 сотрудника от увольнения сверхурочно.",
                Team = 0,
                TeamName = "Corporation",
                TeamNameRu = "Корпорация",
                FactionIndicator = "🏢",
                FactionColor = "#0d9488",
                IsUnique = false,
                ExecutorType = "TARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.SYSTEM_ADMINISTRATOR,
                RoleKey = "SYSTEM_ADMINISTRATOR",
                NameEn = "System Administrator",
                NameRu = "Системный Администратор",
                DescriptionEn = "Network gatekeeper. Overtime you revoke the system access of one colleague, blocking all actions they attempt to take during that shift.",
                DescriptionRu = "Хранитель цифровой инфраструктуры. Сверхурочно отзывает доступы выбранного сотрудника, блокируя любые его действия в эту смену.",
                AbilityEn = "Block 1 employee's terminal and actions during overtime.",
                AbilityRu = "Заблокировать терминал и действия 1 сотрудника сверхурочно.",
                Team = 0,
                TeamName = "Corporation",
                TeamNameRu = "Корпорация",
                FactionIndicator = "🏢",
                FactionColor = "#0d9488",
                IsUnique = false,
                ExecutorType = "TARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.ANTICRISIS_MANAGER,
                RoleKey = "ANTICRISIS_MANAGER",
                NameEn = "Anticrisis Manager",
                NameRu = "Антикризисный Менеджер",
                DescriptionEn = "Corporate hatchet-man. Submits a Critical Incompetence Memo to the Director against any colleague, placing them on the dismissal shortlist. Has limited memos: [headcount / 4] + 1 charges.",
                DescriptionRu = "Корпоративный ликвидатор. Направляет докладную записку о критической некомпетентности на любого сотрудника, внося в список на увольнение. Лимит служебок: [число игроков / 4] + 1.",
                AbilityEn = "Submit a Critical Incompetence dismissal memo on 1 target.",
                AbilityRu = "Подать служебную записку на увольнение 1 сотрудника.",
                Team = 0,
                TeamName = "Corporation",
                TeamNameRu = "Корпорация",
                FactionIndicator = "🏢",
                FactionColor = "#0d9488",
                IsUnique = false,
                ExecutorType = "TARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.INTERN,
                RoleKey = "INTERN",
                NameEn = "Intern",
                NameRu = "Стажёр",
                DescriptionEn = "Eager corporate newcomer without overtime powers. Susceptible to lucrative bribes: can be recruited by the Shadow Director to become a Master of Compromising.",
                DescriptionRu = "Молодой сотрудник без сверхурочных полномочий. Восприимчив к финансовым соблазнам: Теневой Директор может завербовать вас в Мастера Компромата.",
                AbilityEn = "No active overtime duties. Potential recruitment target.",
                AbilityRu = "Нет активных обязанностей. Может быть завербован Синдикатом.",
                Team = 0,
                TeamName = "Corporation",
                TeamNameRu = "Корпорация",
                FactionIndicator = "🏢",
                FactionColor = "#0d9488",
                IsUnique = false,
                ExecutorType = "NONE"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.SCRUM_MASTER,
                RoleKey = "SCRUM_MASTER",
                NameEn = "Scrum Master",
                NameRu = "Scrum Мастер",
                DescriptionEn = "Agile facilitator who shuffles corporate workflows. During overtime you swap two employees: any actions or investigations directed at one will strike the other instead.",
                DescriptionRu = "Фасилитатор процессов. Сверхурочно меняет местами двух сотрудников в спринте: любые действия и проверки, направленные на одного, попадают в другого.",
                AbilityEn = "Swap 2 colleagues in the task queue during overtime.",
                AbilityRu = "Поменять местами двух сотрудников в очереди задач.",
                Team = 0,
                TeamName = "Corporation",
                TeamNameRu = "Корпорация",
                FactionIndicator = "🏢",
                FactionColor = "#0d9488",
                IsUnique = false,
                ExecutorType = "TARGET_TARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.SHAREHOLDER,
                RoleKey = "SHAREHOLDER",
                NameEn = "Shareholder",
                NameRu = "Акционер",
                DescriptionEn = "Owner of substantial equity. Has access to the exclusive private Shareholder communication channel during overtime. Immune to dismissal in normal situations unless compromised.",
                DescriptionRu = "Владелец крупного пакета акций. Имеет доступ в закрытый канал акционеров во время сверхурочных. Обладает повышенным влиянием.",
                AbilityEn = "Access to private Shareholder communications channel.",
                AbilityRu = "Доступ к закрытому каналу связи акционеров.",
                Team = 0,
                TeamName = "Corporation",
                TeamNameRu = "Корпорация",
                FactionIndicator = "🏢",
                FactionColor = "#0d9488",
                IsUnique = false,
                ExecutorType = "NONE"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.LAYOFF_CANDIDATE,
                RoleKey = "LAYOFF_CANDIDATE",
                NameEn = "Layoff Candidate",
                NameRu = "Кандидат на Увольнение",
                DescriptionEn = "Employee sitting on a hot seat due to budget restructuring. If dismissed by the General Director, causes significant corporate turbulence.",
                DescriptionRu = "Сотрудник под угрозой сокращения. Если Генеральный Директор принимает решение о вашем увольнении, это вызывает серьезный резонанс в компании.",
                AbilityEn = "No active overtime duties.",
                AbilityRu = "Нет активных сверхурочных полномочий.",
                Team = 0,
                TeamName = "Corporation",
                TeamNameRu = "Корпорация",
                FactionIndicator = "🏢",
                FactionColor = "#0d9488",
                IsUnique = false,
                ExecutorType = "NONE"
            });

            // Syndicate
            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.MASTER_OF_COMPROMISING,
                RoleKey = "MASTER_OF_COMPROMISING",
                NameEn = "Master of Compromising",
                NameRu = "Мастер Компромата",
                DescriptionEn = "Syndicate operative specializing in targeted leaks. Submits lethal compromising dossiers to put targets on the Director's dismissal shortlist. Can also orchestrate covert influence.",
                DescriptionRu = "Оперативник Синдиката, мастер направленных сливов. Направляет убийственный компромат, отправляя коллегу на увольнение. Общается в закрытом канале Синдиката.",
                AbilityEn = "Submit compromising dossier against 1 target or execute control.",
                AbilityRu = "Слить компромат на 1 сотрудника или провести манипуляцию.",
                Team = 1,
                TeamName = "Syndicate",
                TeamNameRu = "Синдикат",
                FactionIndicator = "🕵️",
                FactionColor = "#b91c1c", // Brick Red
                IsUnique = false,
                ExecutorType = "TARGET_ANYTARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.SPY,
                RoleKey = "SPY",
                NameEn = "Spy",
                NameRu = "Шпион",
                DescriptionEn = "Syndicate intelligence specialist. Infiltrates personnel records overnight to uncover the EXACT job role of any target. Communicates in the private Syndicate chat.",
                DescriptionRu = "Разведчик Синдиката. Проникает в закрытые базы кадров и узнает ТОЧНУЮ должность любого сотрудника. Общается в закрытом чате Синдиката.",
                AbilityEn = "Investigate 1 employee to discover their exact position.",
                AbilityRu = "Проверить 1 сотрудника и узнать его точную должность.",
                Team = 1,
                TeamName = "Syndicate",
                TeamNameRu = "Синдикат",
                FactionIndicator = "🕵️",
                FactionColor = "#b91c1c",
                IsUnique = false,
                ExecutorType = "TARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.HACKER,
                RoleKey = "HACKER",
                NameEn = "Hacker",
                NameRu = "Хакер",
                DescriptionEn = "Syndicate cyber-operative. Locks an employee's workstation, preventing them from carrying out actions during overtime. Cannot target the same colleague on consecutive shifts.",
                DescriptionRu = "Кибер-диверсант Синдиката. Блокирует учетную запись сотрудника, отменяя его ночные действия. Нельзя взламывать одного и того же два раза подряд.",
                AbilityEn = "Hack and disable 1 employee's terminal overnight.",
                AbilityRu = "Взломать терминал 1 сотрудника и заблокировать его действия.",
                Team = 1,
                TeamName = "Syndicate",
                TeamNameRu = "Синдикат",
                FactionIndicator = "🕵️",
                FactionColor = "#b91c1c",
                IsUnique = false,
                ExecutorType = "TARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.SHADOW_DIRECTOR,
                RoleKey = "SHADOW_DIRECTOR",
                NameEn = "Shadow Director",
                NameRu = "Теневой Директор",
                DescriptionEn = "Secret mastermind behind the corporate conspiracy. Recruits loyal employees (especially Interns) into the Syndicate as Masters of Compromising. Investigated as peaceful by Security.",
                DescriptionRu = "Теневой лидер Синдиката. Вербует сотрудников (особенно стажеров) в Синдикат, превращая в Мастеров Компромата. Для СБ выглядит как благонадёжный мирный.",
                AbilityEn = "Attempt to recruit 1 employee into the Syndicate.",
                AbilityRu = "Попытаться завербовать 1 сотрудника в Синдикат.",
                Team = 1,
                TeamName = "Syndicate",
                TeamNameRu = "Синдикат",
                FactionIndicator = "🕵️",
                FactionColor = "#b91c1c",
                IsUnique = true,
                ExecutorType = "TARGET"
            });

            // Startup
            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.STARUP_FOUNDER,
                RoleKey = "STARUP_FOUNDER",
                NameEn = "Startup Founder",
                NameRu = "Основатель Стартапа",
                DescriptionEn = "Disruptor poaching corporate talent for a rogue venture. Overtime recruits employees to become Startup Evangelists. Can silently read/spy on the Evangelist chat channel without posting.",
                DescriptionRu = "Создатель венчурного стартапа, переманивающий таланты. Ночью вербует сотрудников в Евангелисты. Может скрытно читать канал Евангелистов, но не писать в него.",
                AbilityEn = "Poach 1 employee to join the Startup as an Evangelist.",
                AbilityRu = "Переманить 1 сотрудника в Стартап в качестве Евангелиста.",
                Team = 2,
                TeamName = "Startup",
                TeamNameRu = "Стартап",
                FactionIndicator = "🚀",
                FactionColor = "#4d7c0f", // Olive Green
                IsUnique = true,
                ExecutorType = "TARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.EVANGELIST,
                RoleKey = "EVANGELIST",
                NameEn = "Evangelist",
                NameRu = "Евангелист",
                DescriptionEn = "Devoted believer in the Startup's vision. Communicates with fellow Evangelists in their encrypted channel during overtime. Does not know the identity of the Founder.",
                DescriptionRu = "Преданный сторонник философии Стартапа. Общается с другими Евангелистами в закрытом канале сверхурочно. Личность Основателя вам неизвестна.",
                AbilityEn = "Communicate in the encrypted Startup chat channel.",
                AbilityRu = "Общение в закрытом канале связи Стартапа.",
                Team = 2,
                TeamName = "Startup",
                TeamNameRu = "Стартап",
                FactionIndicator = "🚀",
                FactionColor = "#4d7c0f",
                IsUnique = false,
                ExecutorType = "NONE"
            });

            // Singles / Independent
            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.RAIDER,
                RoleKey = "RAIDER",
                NameEn = "Raider",
                NameRu = "Рейдер",
                DescriptionEn = "Ruthless independent corporate raider preparing a hostile takeover. Launches aggressive attacks to dismantle employees and send them to the dismissal shortlist.",
                DescriptionRu = "Хищный независимый рейдер, готовящий недружественное поглощение.",
                AbilityEn = "Launch hostile raid to eliminate 1 employee overnight.",
                AbilityRu = "В сверхурочные применяет к цели либо блокировку, либо увольнение. Результат определяется случайно.",
                Team = 4,
                TeamName = "Independent",
                TeamNameRu = "Независимые",
                FactionIndicator = "⚖️",
                FactionColor = "#701a75", // Plum
                IsUnique = false,
                ExecutorType = "TARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.POLITICAL_STRATEGIST,
                RoleKey = "POLITICAL_STRATEGIST",
                NameEn = "Political Strategist",
                NameRu = "Политтехнолог",
                DescriptionEn = "Master of corporate intrigue. Overtime manipulates an employee into redirecting their scheduled action towards a different target of your choosing.",
                DescriptionRu = "Мастер внутрикорпоративных манипуляций. Сверхурочно перехватывает управление действием сотрудника и перенаправляет его на выбранную цель.",
                AbilityEn = "Control Employee A to redirect their action to Target B.",
                AbilityRu = "Подчинить Сотрудника А и направить его действие на Цель Б.",
                Team = 4,
                TeamName = "Independent",
                TeamNameRu = "Независимые",
                FactionIndicator = "⚖️",
                FactionColor = "#701a75",
                IsUnique = false,
                ExecutorType = "TARGET_ANYTARGET"
            });

            // Outsource
            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.OUTSOURCER,
                RoleKey = "OUTSOURCER",
                NameEn = "Outsourcer",
                NameRu = "Аутсорсер",
                DescriptionEn = "External subcontractor looking to expand contractor headcount. Overtime resurrects a dismissed colleague, returning them to the company as an Outsourcer.",
                DescriptionRu = "Внешний подрядчик. Сверхурочно может воскресить уволенного коллегу, вернув его в строй в качестве нового Аутсорсера по B2B контракту.",
                AbilityEn = "Resurrect 1 dismissed colleague as an Outsourcer.",
                AbilityRu = "Воскресить 1 уволенного коллегу в статусе Аутсорсера.",
                Team = 3,
                TeamName = "Outsource",
                TeamNameRu = "Аутсорс",
                FactionIndicator = "💼",
                FactionColor = "#b45309", // Ochre
                IsUnique = false,
                ExecutorType = "TARGET"
            });

            RegisterRole(new RoleInfoDTO
            {
                Signature = (byte)RoleSignature.ALUMNI_MANAGER,
                RoleKey = "ALUMNI_MANAGER",
                NameEn = "Alumni Manager",
                NameRu = "Alumni Менеджер",
                DescriptionEn = "Corporate relations officer who maintains contact with former staff. Overtime communicates in both the Outsource chat and the Former Colleagues Club (dismissed chat). Immune to Startup poaching.",
                DescriptionRu = "Координатор по связям с уволенными. Сверхурочно общается и в чате Аутсорса, и в Клубе Бывших Коллег (среди уволенных). Иммунитет к вербовке в Стартап.",
                AbilityEn = "Access to Outsource chat and Club of Former Colleagues.",
                AbilityRu = "Доступ к чату Аутсорса и Клубу Бывших Коллег.",
                Team = 3,
                TeamName = "Outsource",
                TeamNameRu = "Аутсорс",
                FactionIndicator = "💼",
                FactionColor = "#b45309",
                IsUnique = true,
                ExecutorType = "NONE"
            });
        }

        private static void RegisterRole(RoleInfoDTO dto)
        {
            RolesMap[(RoleSignature)dto.Signature] = dto;
        }

        public static IReadOnlyCollection<RoleInfoDTO> GetAllRoles() => RolesMap.Values.ToList().AsReadOnly();

        public static RoleInfoDTO? GetRoleInfo(RoleSignature signature)
        {
            RolesMap.TryGetValue(signature, out var role);
            return role;
        }

        public static RoleInfoDTO? GetRoleInfo(string roleKey)
        {
            return RolesMap.Values.FirstOrDefault(r => r.RoleKey.Equals(roleKey, StringComparison.OrdinalIgnoreCase));
        }
    }
}
