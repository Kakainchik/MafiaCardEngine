// Paranoia Corp - Localization Resources (English & Russian)

const Locales = {
  current: localStorage.getItem('paranoia_lang') || 'en',

  setLanguage(lang) {
    this.current = lang;
    localStorage.setItem('paranoia_lang', lang);
  },

  t(key, fallback = '') {
    const dict = this[this.current] || this.en;
    return dict[key] || this.en[key] || fallback || key;
  },

  en: {
    // Portal Shell
    portalTitle: 'PARANOIA CORP. INTERNAL PORTAL',
    clearanceLevel: 'CLEARANCE LEVEL 4 — EXECUTIVE ACCESS',
    docketPrefix: 'DOCKET #',
    fiscalCycle: 'FISCAL CYCLE',
    nextAction: 'IMMEDIATE DIRECTIVE',
    myDossier: 'MY PERSONNEL FILE',
    confidentialStamp: 'CONFIDENTIAL',
    dismissedStamp: 'DISMISSED',
    vetoStamp: 'VETO EXERCISED',

    // Factions
    faction_Corporation: 'Corporation',
    faction_Syndicate: 'Syndicate',
    faction_Startup: 'Startup',
    faction_Outsource: 'Outsource',
    faction_Independent: 'Independent',

    // Phases
    phase_Intro: 'Introductory Shift',
    phase_Overtime: 'Overtime',
    phase_Morning: 'Morning Brief',
    phase_BoardMeeting: 'Board Meeting',
    phase_Evening: 'Evening Adjournment',
    phase_EndGame: 'Personnel Audit',

    // Navigation & Tabs
    tabWorkspace: 'Action / Workspace',
    tabRoster: 'Personnel Register',
    tabComms: 'Communications',
    tabDossier: 'My Dossier',

    // Auth
    authHeader: 'PERSONNEL CLEARANCE GATEWAY',
    authSubhead: 'Access restricted to authorized employees. Identify credentials.',
    username: 'Employee Alias',
    password: 'Security Passcode',
    signIn: 'Authorize Access',
    signUp: 'Enroll New Employee',
    enterAsGuest: 'Enter as Guest Employee',
    guestHint: 'No enrollment required. Ideal for quick deployment or testing.',
    signOut: 'Sign Out',

    // Lobby Directory
    boardroomDirectory: 'CONVENED BOARDROOMS',
    refreshRooms: 'Refresh Directory',
    requisitionBoardroom: 'Requisition New Boardroom',
    noRoomsFound: 'No active boardroom sessions found. Convene a new session below.',
    roomTitle: 'Docket Subject / Title',
    roomDescription: 'Operational Memo (Optional)',
    roomSeats: 'Authorized Seats',
    createRoom: 'Convene Boardroom',
    cancel: 'Cancel',
    join: 'Enter Boardroom',
    seatsOccupied: 'Seats',

    // Lobby Waiting Room
    roomCode: 'BOARDROOM CODE',
    convenor: 'CONVENING HOST',
    rosterHeader: 'ATTENDEE ROSTER',
    rolePoolHeader: 'AUTHORIZED POSITION POOL',
    addRole: 'Add Role',
    clearRoles: 'Reset to General Director',
    browseRoles: 'Open Role Catalogue',
    statusReady: 'Ready for Shift',
    statusPending: 'Awaiting Signature',
    toggleReady: 'Confirm Readiness',
    conveneGame: 'Execute Contracts & Convene Board',
    cannotStartNotice: 'Cannot convene board yet:',
    hostBadge: 'CONVENOR',
    youBadge: 'YOU',

    // Overtime
    overtimeTitle: 'Overtime Duty Operations',
    overtimeDesc: 'Select confidential operational assignments for this overtime shift.',
    skipOvertime: 'Skip Overtime Duties',
    reviewChoice: 'Review Selected Parameters',
    submitOvertime: 'Submit Duty Report',
    confirmReadiness: 'Confirm Shift Readiness',
    waitingColleagues: 'Waiting for colleagues to complete their shift duties...',
    actionSubmitted: 'Duty report filed successfully.',

    // Morning
    morningNewsTitle: 'Morning Incident Wire',
    confidentialReportsTitle: 'Confidential Personnel Memoranda',
    conveneBoardButton: 'Convene Executive Board Meeting',
    awaitingDirectorMeeting: 'Awaiting the General Director to officially convene the Board Meeting.',

    // Board Meeting
    boardAgendaTitle: 'Boardroom Agenda & Dismissal Shortlist',
    candidatesListTitle: 'Shortlisted Candidates for Immediate Termination',
    noCandidatesText: 'No candidates currently shortlisted. The General Director may target any employee or adjourn without dismissal.',
    remainingVetoes: 'Remaining Veto Powers',
    fireCandidateButton: 'Execute Dismissal',
    useVetoButton: 'Use Veto',
    skipBoardButton: 'Adjourn Without Dismissal (Mercy)',
    confirmDismissalDialog: 'CONFIRM IMMEDIATE EMPLOYEE TERMINATION',
    confirmDismissalPrompt: 'This action is irreversible. The dismissed employee will be stripped of security clearance and transferred to the Club of Former Colleagues.',
    confirmFire: 'Confirm Irreversible Dismissal',
    confirmVetoDialog: 'CONFIRM STATUTORY VETO EXERCISE',
    confirmVetoPrompt: 'Using a Veto will expend 1 of your 2 available Veto Powers and reset current dismissal nominations.',
    confirmVeto: 'Exercise Statutory Veto',
    selectedForDismissal: 'Selected for dismissal',
    nominatedForDismissal: 'Nominated for dismissal',
    directorExecutivePowers: "GENERAL DIRECTOR'S EXECUTIVE POWERS",
    selectEmployeeDismissal: 'Select Employee for Dismissal',
    chooseCandidate: '-- Choose Candidate to Terminate --',
    vetoActive: 'VETO ACTIVE',
    noMorningNews: 'No morning news issued.',
    noConfidentialReports: 'No confidential reports issued to your terminal this morning.',
    confidentialMemo: 'Confidential Memo',
    selectCandidatePrompt: 'Please select a candidate to terminate from the dropdown.',

    // Evening
    dismissalOrderTitle: 'Executive Termination Order',
    dismissedColleague: 'Dismissed Employee',
    revealedRoleLabel: 'Contractual Role Revealed',
    finalizeDismissal: 'Finalize Dismissal Papers',

    // End Game
    gameOverTitle: 'Corporate Governance Audit Concluded',
    personnelRegisterTitle: 'Final Personnel Register (All Positions Revealed)',
    turnHistoryTitle: 'Chronological Turn & Investigation Timeline',
    playAgain: 'Return to Boardroom Directory',

    // Chat
    sendChat: 'Transmit Memo',
    typeMessage: 'Type message to channel...',
    lockedChannel: 'Transmissions locked in this channel during current operational phase.',
    noMessagesYet: 'No transmissions recorded on this frequency.',

    // Role Catalogue
    roleDirectoryTitle: 'Paranoia Corp. Corporate Position Directory',
    close: 'Close Directory',
    uniqueRoleBadge: 'UNIQUE POSITION'
  },

  ru: {
    // Portal Shell
    portalTitle: 'ВНУТРЕННИЙ ПОРТАЛ PARANOIA CORP.',
    clearanceLevel: 'УРОВЕНЬ ДОПУСКА 4 — РУКОВОДЯЩИЙ СОСТАВ',
    docketPrefix: 'ДЕЛО №',
    fiscalCycle: 'ФИСКАЛЬНЫЙ ЦИКЛ',
    nextAction: 'ТЕКУЩЕЕ РАСПОРЯЖЕНИЕ',
    myDossier: 'МОЁ ЛИЧНОЕ ДЕЛО',
    confidentialStamp: 'СОВЕРШЕННО СЕКРЕТНО',
    dismissedStamp: 'УВОЛЕН',
    vetoStamp: 'ВЕТО ПРИМЕНЕНО',

    // Factions
    faction_Corporation: 'Корпорация',
    faction_Syndicate: 'Синдикат',
    faction_Startup: 'Стартап',
    faction_Outsource: 'Аутсорс',
    faction_Independent: 'Независимые',

    // Phases
    phase_Intro: 'Вводный Инструктаж',
    phase_Overtime: 'Сверхурочные',
    phase_Morning: 'Утренняя Летучка',
    phase_BoardMeeting: 'Совет Директоров',
    phase_Evening: 'Вечернее Подведение Итогов',
    phase_EndGame: 'Итоговый Аудит',

    // Navigation & Tabs
    tabWorkspace: 'Действие / Рабочий стол',
    tabRoster: 'Реестр Сотрудников',
    tabComms: 'Связь и Каналы',
    tabDossier: 'Моё Досье',

    // Auth
    authHeader: 'ШЛЮЗ АВТОРИЗАЦИИ ПЕРСОНАЛА',
    authSubhead: 'Доступ строго по регламенту. Подтвердите служебные полномочия.',
    username: 'Служебный псевдоним',
    password: 'Код доступа',
    signIn: 'Войти в Систему',
    signUp: 'Зарегистрировать Сотрудника',
    enterAsGuest: 'Войти как Гостевой Сотрудник',
    guestHint: 'Без регистрации. Идеально для быстрого подключения и тестов.',
    signOut: 'Выйти',

    // Lobby Directory
    boardroomDirectory: 'СОЗВАННЫЕ ЗАСЕДАНИЯ',
    refreshRooms: 'Обновить Реестр',
    requisitionBoardroom: 'Запросить Новый Зал Заседаний',
    noRoomsFound: 'Активных заседаний не обнаружено. Запросите открытие нового зала ниже.',
    roomTitle: 'Тема заседания / Название',
    roomDescription: 'Служебная записка (необязательно)',
    roomSeats: 'Выделено мест',
    createRoom: 'Открыть Заседание',
    cancel: 'Отмена',
    join: 'Войти в Зал',
    seatsOccupied: 'Мест',

    // Lobby Waiting Room
    roomCode: 'НОМЕР ЗАСЕДАНИЯ',
    convenor: 'СОЗЫВАЮЩИЙ ХОСТ',
    rosterHeader: 'ЯВКА СОТРУДНИКОВ',
    rolePoolHeader: 'УТВЕРЖДЁННЫЙ ПУЛ ДОЛЖНОСТЕЙ',
    addRole: 'Добавить Должность',
    clearRoles: 'Сбросить до Гендиректора',
    browseRoles: 'Открыть Каталог Должностей',
    statusReady: 'К смене готов',
    statusPending: 'Ожидает подписи',
    toggleReady: 'Подтвердить Готовность',
    conveneGame: 'Подписать Контракты и Начать',
    cannotStartNotice: 'Невозможно начать заседание:',
    hostBadge: 'СОЗЫВАЮЩИЙ',
    youBadge: 'ВЫ',

    // Overtime
    overtimeTitle: 'Сверхурочные Обязанности',
    overtimeDesc: 'Выберите секретные поручения на текущую сверхурочную смену.',
    skipOvertime: 'Пропустить Сверхурочные',
    reviewChoice: 'Проверить Параметры Выбора',
    submitOvertime: 'Отправить Отчет по Смене',
    confirmReadiness: 'Подтвердить Готовность к Завершению',
    waitingColleagues: 'Ожидание завершения задач остальными коллегами...',
    actionSubmitted: 'Служебный отчёт успешно зафиксирован.',

    // Morning
    morningNewsTitle: 'Сводка Ночных Происшествий',
    confidentialReportsTitle: 'Секретные Служебные Докладные',
    conveneBoardButton: 'Открыть Заседание Совета Директоров',
    awaitingDirectorMeeting: 'Ожидание официального открытия заседания Генеральным Директором.',

    // Board Meeting
    boardAgendaTitle: 'Повестка Дня и Шорт-Лист на Увольнение',
    candidatesListTitle: 'Кандидаты на Немедленное Расторжение Договора',
    noCandidatesText: 'В списке нет кандидатов. Генеральный Директор вправе выбрать любого сотрудника либо помиловать всех.',
    remainingVetoes: 'Осталось Прав Вето',
    fireCandidateButton: 'Уволить Сотрудника',
    useVetoButton: 'Применить Вето',
    skipBoardButton: 'Завершить без Увольнения (Помиловать)',
    confirmDismissalDialog: 'ПОДТВЕРЖДЕНИЕ УВОЛЬНЕНИЯ СОТРУДНИКА',
    confirmDismissalPrompt: 'Действие необратимо. Сотрудник будет лишён всех доступов и переведён в Клуб Бывших Коллег.',
    confirmFire: 'Подтвердить Необратимое Увольнение',
    confirmVetoDialog: 'ПОДТВЕРЖДЕНИЕ ПРАВА ВЕТО',
    confirmVetoPrompt: 'Применение вето израсходует 1 из 2 доступных прав и сбросит текущие номинации на увольнение.',
    confirmVeto: 'Использовать Право Вето',
    selectedForDismissal: 'Выбран для увольнения',
    nominatedForDismissal: 'Кандидат на увольнение',
    directorExecutivePowers: 'ПОЛНОМОЧИЯ ГЕНЕРАЛЬНОГО ДИРЕКТОРА',
    selectEmployeeDismissal: 'Выберите сотрудника для увольнения',
    chooseCandidate: '-- Выберите кандидата для увольнения --',
    vetoActive: 'ВЕТО АКТИВНО',
    noMorningNews: 'Утренних новостей нет.',
    noConfidentialReports: 'Конфиденциальные доклады вам сегодня не направлялись.',
    confidentialMemo: 'Конфиденциальная записка',
    selectCandidatePrompt: 'Пожалуйста, выберите кандидата для увольнения в списке.',

    // Evening
    dismissalOrderTitle: 'Приказ об Увольнении',
    dismissedColleague: 'Уволенный Сотрудник',
    revealedRoleLabel: 'Раскрытая Должность',
    finalizeDismissal: 'Подписать Приказ об Увольнении',

    // End Game
    gameOverTitle: 'Итоговый Корпоративный Аудит Завершён',
    personnelRegisterTitle: 'Итоговый Штатный Реестр (Все Должности Раскрыты)',
    turnHistoryTitle: 'Хронологическая Лента Расследований и Действий',
    playAgain: 'Вернуться в Список Заседаний',

    // Chat
    sendChat: 'Отправить',
    typeMessage: 'Введите сообщение...',
    lockedChannel: 'Отправка сообщений в этот канал закрыта на текущей фазе.',
    noMessagesYet: 'Сообщений на этой частоте пока нет.',

    // Role Catalogue
    roleDirectoryTitle: 'Справочник Должностей Paranoia Corp.',
    close: 'Закрыть Справочник',
    uniqueRoleBadge: 'УНИКАЛЬНАЯ ДОЛЖНОСТЬ'
  }
};
