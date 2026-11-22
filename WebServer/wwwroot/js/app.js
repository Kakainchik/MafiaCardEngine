// Paranoia Corp - Main Application Controller

const App = {
  state: {
    currentScreen: 'auth', // 'auth' | 'hall' | 'lobby' | 'game'
    mobileTab: 'workspace', // 'workspace' | 'roster' | 'comms'
    roomId: null,
    lobbyData: null,
    gameState: null,
    activeChatScope: 'PUBLIC',
    chatMessages: {},
    roleDirectory: [],
    selectedOvertimeTargets: [],
    overtimeSkipped: false,
    directorSelectedCandidate: null,
    activeModal: null,
    inspectingRole: null
  },

  async init() {
    this.bindEvents();

    // Check language
    const currentLang = Locales.current;
    document.documentElement.lang = currentLang;

    // Load roles catalogue
    try {
      this.state.roleDirectory = await API.getRoleDirectory();
    } catch (e) {
      console.warn('Could not prefetch role directory:', e);
    }

    if (API.isAuthenticated()) {
      this.showHall();
    } else {
      this.showAuth();
    }
  },

  bindEvents() {
    window.addEventListener('auth-expired', () => {
      this.showAuth();
      this.showToast('Authentication expired. Re-enter credentials.');
    });

    window.addEventListener('paranoia-room-updated', async (e) => {
      if (this.state.currentScreen === 'lobby' && this.state.roomId === e.detail.roomId) {
        await this.refreshLobby();
      }
    });

    window.addEventListener('paranoia-game-started', (e) => {
      if (this.state.roomId === e.detail.roomId) {
        this.showGame(e.detail.roomId);
      }
    });

    window.addEventListener('paranoia-state-changed', async (e) => {
      if (this.state.currentScreen === 'game' && this.state.roomId === e.detail.roomId) {
        await this.refreshGameState();
      }
    });

    window.addEventListener('paranoia-chat-updated', async (e) => {
      if (this.state.currentScreen === 'game' && this.state.roomId === e.detail.roomId) {
        await this.refreshChat(e.detail.scope);
      }
    });

    window.addEventListener('paranoia-morning-brief', async (e) => {
      if (this.state.currentScreen === 'game' && this.state.roomId === e.detail.roomId) {
        await this.refreshGameState();
      }
    });
  },

  setLanguage(lang) {
    Locales.setLanguage(lang);
    document.documentElement.lang = lang;
    this.render();
  },

  toggleLanguage() {
    const next = Locales.current === 'en' ? 'ru' : 'en';
    this.setLanguage(next);
  },

  setMobileTab(tab) {
    this.state.mobileTab = tab;
    const grid = document.querySelector('.game-grid');
    if (grid) {
      grid.classList.remove('view-active-workspace', 'view-active-roster', 'view-active-comms');
      grid.classList.add(`view-active-${tab}`);
    }
    document.querySelectorAll('.mobile-tab-btn').forEach(btn => {
      btn.classList.toggle('active', btn.dataset.tab === tab);
    });
  },

  showToast(message, isError = false) {
    let toast = document.getElementById('toast');
    if (!toast) {
      toast = document.createElement('div');
      toast.id = 'toast';
      toast.style.position = 'fixed';
      toast.style.bottom = '20px';
      toast.style.left = '50%';
      toast.style.transform = 'translateX(-50%)';
      toast.style.background = '#1e2428';
      toast.style.color = '#f7f4ec';
      toast.style.padding = '10px 18px';
      toast.style.border = '1px solid #d5cbba';
      toast.style.boxShadow = '0 4px 14px rgba(0,0,0,0.5)';
      toast.style.fontFamily = 'var(--font-mono)';
      toast.style.fontSize = '12px';
      toast.style.zIndex = '999';
      toast.style.transition = 'opacity 0.2s';
      document.body.appendChild(toast);
    }
    toast.textContent = message;
    toast.style.borderColor = isError ? 'var(--vermilion)' : 'var(--paper-rule-dark)';
    toast.style.display = 'block';
    toast.style.opacity = '1';
    setTimeout(() => {
      toast.style.opacity = '0';
      setTimeout(() => { toast.style.display = 'none'; }, 200);
    }, 3200);
  },

  // Navigation Screen Methods
  showAuth() {
    API.stopPolling();
    this.state.currentScreen = 'auth';
    this.state.roomId = null;
    this.render();
  },

  async showHall() {
    API.stopPolling();
    this.state.currentScreen = 'hall';
    this.state.roomId = null;
    this.render();
    await this.refreshHallRooms();
  },

  async showLobby(roomId) {
    this.state.currentScreen = 'lobby';
    this.state.roomId = roomId;
    this.render();
    await API.connectSignalR(roomId);
    await this.refreshLobby();
  },

  async showGame(roomId) {
    this.state.currentScreen = 'game';
    this.state.roomId = roomId;
    this.render();
    await API.connectSignalR(roomId);
    await this.refreshGameState();
  },

  // Data Fetching
  async refreshHallRooms() {
    try {
      const rooms = await API.getRooms(1);
      this.state.hallRooms = rooms || [];
      this.renderHallList();
    } catch (e) {
      this.showToast(e.message, true);
    }
  },

  async refreshLobby() {
    try {
      const data = await API.getParanoiaRoom(this.state.roomId);
      this.state.lobbyData = data;
      if (data.isGameStarted) {
        this.showGame(this.state.roomId);
        return;
      }
      this.render();
    } catch (e) {
      this.showToast(e.message, true);
    }
  },

  async refreshGameState() {
    try {
      const state = await API.getGameState(this.state.roomId);
      this.state.gameState = state;

      // Ensure active chat scope is valid
      const scopes = state.availableChatScopes || [];
      if (!scopes.some(s => s.scope === this.state.activeChatScope)) {
        this.state.activeChatScope = scopes[0]?.scope || 'PUBLIC';
      }

      this.render();
      await this.refreshChat(this.state.activeChatScope);
    } catch (e) {
      console.error('Failed to refresh game state:', e);
    }
  },

  async refreshChat(scope) {
    try {
      const msgs = await API.getChat(this.state.roomId, scope);
      this.state.chatMessages[scope] = msgs || [];
      this.renderChatMessages();
    } catch (e) {
      console.warn('Failed to fetch chat for scope:', scope, e);
    }
  },

  // Main Render Switch
  render() {
    const appEl = document.getElementById('app');
    if (!appEl) return;

    let contentHtml = '';
    switch (this.state.currentScreen) {
      case 'auth':
        contentHtml = this.renderAuthView();
        break;
      case 'hall':
        contentHtml = this.renderHallView();
        break;
      case 'lobby':
        contentHtml = this.renderLobbyView();
        break;
      case 'game':
        contentHtml = this.renderGameView();
        break;
    }

    const headerHtml = this.renderHeader();
    const modalHtml = this.renderModal();
    const mobileNavHtml = this.state.currentScreen === 'game' ? this.renderMobileNav() : '';

    appEl.innerHTML = `
      ${headerHtml}
      <div class="main-content">
        ${contentHtml}
      </div>
      ${mobileNavHtml}
      ${modalHtml}
    `;

    // Attach listeners after DOM update
    this.attachDomEvents();
  },

  renderHeader() {
    const isEn = Locales.current === 'en';
    const isGame = this.state.currentScreen === 'game' && this.state.gameState;
    const gs = this.state.gameState;

    let statusBar = '';
    if (isGame) {
      const phaseTitle = isEn ? gs.phaseTitleEn : gs.phaseTitleRu;
      const directive = isEn ? gs.directiveEn : gs.directiveRu;
      statusBar = `
        <div class="portal-status-bar">
          <div class="status-badge-item">
            <span class="status-label">${Locales.t('docketPrefix')}${gs.roomId}</span>
            <span class="status-value">${phaseTitle}</span>
          </div>
          <div class="status-badge-item">
            <span class="status-label">${Locales.t('fiscalCycle')}</span>
            <span class="status-value">#${gs.fiscalTurn}</span>
          </div>
          <div class="status-directive-box">
            <span class="status-label">${Locales.t('nextAction')}</span>
            <span class="status-directive-text" title="${directive}">${directive}</span>
          </div>
        </div>
      `;
    }

    let userActions = '';
    if (API.isAuthenticated()) {
      userActions = `
        ${isGame ? `<button class="btn-utility" id="btn-toggle-dossier">${Locales.t('myDossier')}</button>` : ''}
        <button class="btn-utility" id="btn-browse-roles">${Locales.t('browseRoles')}</button>
        <button class="btn-utility btn-lang" id="btn-toggle-lang">${isEn ? 'RU' : 'EN'}</button>
        <button class="btn-utility" id="btn-sign-out">${Locales.t('signOut')}</button>
      `;
    } else {
      userActions = `
        <button class="btn-utility" id="btn-browse-roles">${Locales.t('browseRoles')}</button>
        <button class="btn-utility btn-lang" id="btn-toggle-lang">${isEn ? 'RU' : 'EN'}</button>
      `;
    }

    return `
      <header class="portal-header">
        <div class="portal-brand">
          <div class="portal-seal">
            <span class="portal-seal-icon">P</span>
            <span>PARANOIA CORP.</span>
          </div>
          <span class="portal-clearance-pill">${Locales.t('clearanceLevel')}</span>
        </div>
        ${statusBar}
        <div class="header-actions">
          ${userActions}
        </div>
      </header>
    `;
  },

  // 1. Auth View
  renderAuthView() {
    return `
      <div class="workspace-column" style="justify-content: center; width: 100%;">
        <div class="workspace-card" style="max-width: 480px;">
          <div class="paper-memo-header">
            <div class="memo-meta">
              <span class="memo-dept">${Locales.t('authHeader')}</span>
              <span class="memo-docket">CLEARANCE DIRECTIVE 01-A</span>
            </div>
            <span class="stamp stamp-vermilion">${Locales.t('confidentialStamp')}</span>
          </div>
          <div class="paper-memo-body">
            <p style="font-size: 13px; color: var(--ink-muted);">${Locales.t('authSubhead')}</p>

            <form id="form-auth" style="display: flex; flex-direction: column; gap: 14px;">
              <div class="selector-group">
                <label class="selector-label">${Locales.t('username')}</label>
                <input class="form-input" id="auth-username" required placeholder="e.g. Agent_Vance" />
              </div>

              <div class="selector-group">
                <label class="selector-label">${Locales.t('password')}</label>
                <input class="form-input" id="auth-password" type="password" required placeholder="••••••••" />
              </div>

              <div style="display: flex; gap: 10px; margin-top: 6px;">
                <button type="submit" class="btn-primary" style="flex: 1;" id="btn-submit-signin">${Locales.t('signIn')}</button>
                <button type="button" class="btn-secondary" id="btn-submit-signup">${Locales.t('signUp')}</button>
              </div>
            </form>

            <div style="border-top: 1px dashed var(--paper-rule-dark); margin: 8px 0;"></div>

            <div style="display: flex; flex-direction: column; gap: 6px;">
              <button class="btn-secondary" id="btn-guest-login" style="background: var(--paper-ivory-aged); font-weight: bold; padding: 12px;">
                ${Locales.t('enterAsGuest')}
              </button>
              <span style="font-family: var(--font-mono); font-size: 10px; color: var(--ink-muted); text-align: center;">
                ${Locales.t('guestHint')}
              </span>
            </div>
          </div>
        </div>
      </div>
    `;
  },

  // 2. Hall Directory View
  renderHallView() {
    return `
      <div class="workspace-column" style="width: 100%; max-width: 960px; margin: 0 auto;">
        <div class="workspace-card" style="max-width: 900px;">
          <div class="paper-memo-header">
            <div class="memo-meta">
              <span class="memo-dept">${Locales.t('boardroomDirectory')}</span>
              <span class="memo-docket">EXECUTIVE CODES & SESSIONS</span>
            </div>
            <div style="display: flex; gap: 10px;">
              <button class="btn-secondary" id="btn-refresh-hall">${Locales.t('refreshRooms')}</button>
              <button class="btn-primary" id="btn-open-create-room">${Locales.t('requisitionBoardroom')}</button>
            </div>
          </div>
          <div class="paper-memo-body">
            <div id="hall-rooms-container" style="display: flex; flex-direction: column; gap: 10px;">
              <div style="font-family: var(--font-mono); font-size: 12px; color: var(--ink-muted); padding: 20px; text-align: center;">
                Scanning internal frequencies for convened boardrooms...
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  },

  renderHallList() {
    const container = document.getElementById('hall-rooms-container');
    if (!container) return;

    const rooms = this.state.hallRooms || [];
    if (rooms.length === 0) {
      container.innerHTML = `
        <div style="padding: 30px; text-align: center; color: var(--ink-muted);">
          <p style="font-size: 14px; margin-bottom: 12px;">${Locales.t('noRoomsFound')}</p>
          <button class="btn-primary" id="btn-empty-create">${Locales.t('requisitionBoardroom')}</button>
        </div>
      `;
      document.getElementById('btn-empty-create')?.addEventListener('click', () => {
        this.openModal('createRoom');
      });
      return;
    }

    container.innerHTML = rooms.map(r => `
      <div style="background: var(--paper-ivory-light); border: 1px solid var(--paper-rule); padding: 14px 18px; display: flex; align-items: center; justify-content: space-between; border-radius: var(--radius-sm);">
        <div style="display: flex; flex-direction: column; gap: 3px;">
          <div style="display: flex; align-items: center; gap: 10px;">
            <span style="font-family: var(--font-heading); font-size: 17px; font-weight: 700; color: var(--ink-black);">${r.title}</span>
            <span style="font-family: var(--font-mono); font-size: 10px; color: var(--ink-muted); background: var(--paper-rule); padding: 1px 6px;">#${r.id}</span>
          </div>
          <span style="font-size: 12px; color: var(--ink-muted);">${r.description || 'Routine corporate oversight session.'}</span>
          <span style="font-family: var(--font-mono); font-size: 11px; color: var(--ink-charcoal);">
            ${Locales.t('convenor')}: <strong>${r.host?.username || 'Executive'}</strong> | ${Locales.t('seatsOccupied')}: ${r.fullness} / ${r.maxSeats}
          </span>
        </div>
        <button class="btn-primary btn-join-room" data-room-id="${r.id}">${Locales.t('join')}</button>
      </div>
    `).join('');

    container.querySelectorAll('.btn-join-room').forEach(btn => {
      btn.addEventListener('click', () => {
        const rId = parseInt(btn.dataset.roomId, 10);
        this.showLobby(rId);
      });
    });
  },

  // 3. Lobby Waiting Room View
  renderLobbyView() {
    const lobby = this.state.lobbyData;
    if (!lobby) {
      return `<div class="workspace-column"><div style="padding: 40px; font-family: var(--font-mono);">Loading boardroom docket...</div></div>`;
    }

    const isEn = Locales.current === 'en';
    const isHost = lobby.host.id === API.user?.id;
    const allRoles = this.state.roleDirectory;

    return `
      <div class="workspace-column" style="width: 100%; max-width: 960px; margin: 0 auto;">
        <div class="workspace-card" style="max-width: 900px;">
          <div class="paper-memo-header">
            <div class="memo-meta">
              <span class="memo-dept">${lobby.title}</span>
              <span class="memo-docket">${Locales.t('roomCode')}: #${lobby.id} | ${Locales.t('convenor')}: ${lobby.host.username}</span>
            </div>
            <span class="stamp stamp-teal">CONVENING</span>
          </div>

          <div class="paper-memo-body">
            <!-- Attendee Roster Grid -->
            <div>
              <div style="font-family: var(--font-heading); font-size: 14px; font-weight: 700; color: var(--ink-black); margin-bottom: 8px;">
                ${Locales.t('rosterHeader')} (${lobby.players.length} / ${lobby.maxSeats})
              </div>
              <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 10px;">
                ${lobby.players.map(p => `
                  <div style="background: var(--paper-ivory-light); border: 1px solid var(--paper-rule); padding: 8px 12px; display: flex; align-items: center; justify-content: space-between;">
                    <div style="display: flex; flex-direction: column;">
                      <span style="font-weight: 600; font-size: 13px; color: var(--ink-black);">
                        ${p.username} ${p.id === API.user?.id ? `(${Locales.t('youBadge')})` : ''}
                      </span>
                      <span style="font-family: var(--font-mono); font-size: 9px; color: var(--ink-muted);">
                        ${p.isHost ? Locales.t('hostBadge') : 'MEMBER'}
                      </span>
                    </div>
                    <span class="badge-tag ${p.isReady ? 'badge-ready' : 'badge-pending'}">
                      ${p.isReady ? Locales.t('statusReady') : Locales.t('statusPending')}
                    </span>
                  </div>
                `).join('')}
              </div>
            </div>

            <!-- Role Pool Configuration -->
            <div style="margin-top: 14px;">
              <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px;">
                <span style="font-family: var(--font-heading); font-size: 14px; font-weight: 700; color: var(--ink-black);">
                  ${Locales.t('rolePoolHeader')} (${lobby.totalRolesCount} / ${lobby.players.length})
                </span>
                ${isHost ? `<button class="btn-secondary" id="btn-clear-roles" style="font-size: 10px; padding: 4px 8px;">${Locales.t('clearRoles')}</button>` : ''}
              </div>

              <div style="display: flex; flex-direction: column; gap: 6px; max-height: 280px; overflow-y: auto; padding-right: 4px;">
                ${allRoles.map(role => {
                  const count = lobby.rolesPool[role.roleKey] || 0;
                  const name = isEn ? role.nameEn : role.nameRu;
                  const faction = isEn ? role.teamName : role.teamNameRu;
                  return `
                    <div style="background: var(--paper-ivory-light); border: 1px solid var(--paper-rule); padding: 6px 12px; display: flex; align-items: center; justify-content: space-between;">
                      <div style="display: flex; align-items: center; gap: 8px;">
                        <span style="font-size: 14px;">${role.factionIndicator}</span>
                        <div style="display: flex; flex-direction: column;">
                          <span style="font-weight: 600; font-size: 12px; color: var(--ink-black); cursor: pointer;" class="role-inspect-btn" data-role-key="${role.roleKey}">
                            ${name} ${role.isUnique ? `(${Locales.t('uniqueRoleBadge')})` : ''}
                          </span>
                          <span style="font-family: var(--font-mono); font-size: 9px; color: ${role.factionColor};">
                            ${faction}
                          </span>
                        </div>
                      </div>
                      ${isHost ? `
                        <div style="display: flex; align-items: center; gap: 6px;">
                          <button class="btn-secondary btn-role-minus" data-role="${role.roleKey}" style="padding: 2px 8px; font-weight: bold;">-</button>
                          <span style="font-family: var(--font-mono); font-size: 13px; font-weight: bold; width: 22px; text-align: center;">${count}</span>
                          <button class="btn-secondary btn-role-plus" data-role="${role.roleKey}" style="padding: 2px 8px; font-weight: bold;" ${role.isUnique && count >= 1 ? 'disabled' : ''}>+</button>
                        </div>
                      ` : `
                        <span style="font-family: var(--font-mono); font-size: 12px; font-weight: bold;">${count}</span>
                      `}
                    </div>
                  `;
                }).join('')}
              </div>
            </div>

            <!-- Validation Feedback Box -->
            ${!lobby.isValid ? `
              <div style="background: #fff1f2; border-left: 3px solid var(--vermilion); padding: 10px 14px; font-size: 12px; color: #991b1b; display: flex; flex-direction: column; gap: 4px;">
                <span style="font-weight: bold; font-family: var(--font-heading); text-transform: uppercase;">${Locales.t('cannotStartNotice')}</span>
                <ul style="padding-left: 18px; margin: 0;">
                  ${lobby.validationErrors.map(err => `<li>${err}</li>`).join('')}
                </ul>
              </div>
            ` : ''}

            <!-- Bottom Actions -->
            <div style="display: flex; align-items: center; justify-content: space-between; margin-top: 10px; border-top: 1px solid var(--paper-rule); padding-top: 16px;">
              <button class="btn-secondary" id="btn-toggle-ready">
                ${Locales.t('toggleReady')}
              </button>
              ${isHost ? `
                <button class="btn-primary" id="btn-convene-game" ${!lobby.isValid ? 'disabled style="opacity:0.5; cursor:not-allowed;"' : ''}>
                  ${Locales.t('conveneGame')}
                </button>
              ` : `
                <span style="font-family: var(--font-mono); font-size: 11px; color: var(--ink-muted);">
                  Awaiting host (${lobby.host.username}) to execute contracts...
                </span>
              `}
            </div>
          </div>
        </div>
      </div>
    `;
  },

  // 4. In-Game View (3-Column Desktop Grid)
  renderGameView() {
    const gs = this.state.gameState;
    if (!gs) {
      return `<div class="workspace-column"><div style="padding: 40px; font-family: var(--font-mono);">Decrypting classified session...</div></div>`;
    }

    return `
      <div class="game-grid view-active-${this.state.mobileTab}">
        <!-- Left Column: Personnel Roster -->
        ${this.renderRosterColumn()}

        <!-- Center Column: Active Operation Workspace -->
        <main class="workspace-column">
          ${this.renderWorkspaceContent()}
        </main>

        <!-- Right Column: Scoped Communications & Reports -->
        ${this.renderCommsColumn()}
      </div>
    `;
  },

  renderRosterColumn() {
    const gs = this.state.gameState;
    const isEn = Locales.current === 'en';

    return `
      <aside class="roster-column">
        <div class="column-header">
          <span class="column-title">${Locales.t('rosterHeader')}</span>
          <span style="font-family: var(--font-mono); font-size: 11px; color: var(--shell-text-muted);">
            ${gs.roster.filter(p => p.isAlive).length} ACTIVE
          </span>
        </div>
        <div class="roster-list">
          ${gs.roster.map(p => {
            const isSelf = p.id === gs.self.id;
            const roleName = isEn ? p.revealedRoleNameEn : p.revealedRoleNameRu;
            const factionName = isEn ? p.revealedFaction : p.revealedFactionRu;
            const status = isEn ? p.statusEn : p.statusRu;
            return `
              <div class="roster-card ${isSelf ? 'is-self' : ''} ${!p.isAlive ? 'is-dismissed' : ''}">
                <div class="roster-card-header">
                  <span class="roster-name">
                    ${p.username} ${isSelf ? `<span style="font-size:10px; color:#38bdf8;">(${Locales.t('youBadge')})</span>` : ''}
                  </span>
                  <div class="roster-badges">
                    ${!p.isAlive ? `<span class="badge-tag badge-dismissed">${Locales.t('dismissedStamp')}</span>` : `
                      <span class="badge-tag ${p.isReady ? 'badge-ready' : 'badge-pending'}">
                        ${p.isReady ? 'READY' : 'WAITING'}
                      </span>
                    `}
                  </div>
                </div>
                <div class="roster-role-info">
                  ${roleName ? `
                    <span class="roster-faction-marker" style="background: ${p.revealedFactionColor || '#64748b'};"></span>
                    <span style="color: ${p.revealedFactionColor || '#d5cbba'}; font-weight: 600;">${roleName}</span>
                    <span style="font-size: 10px; color: var(--shell-text-muted);">(${factionName})</span>
                  ` : `
                    <span style="font-family: var(--font-mono); font-size: 10px; color: var(--shell-text-muted);">
                      [CLASSIFIED DOSSIER] — ${status}
                    </span>
                  `}
                </div>
              </div>
            `;
          }).join('')}
        </div>
      </aside>
    `;
  },

  renderWorkspaceContent() {
    const gs = this.state.gameState;
    const phase = gs.phase;

    switch (phase) {
      case 'Intro':
        return this.renderIntroWorkspace();
      case 'Overtime':
        return this.renderOvertimeWorkspace();
      case 'Morning':
        return this.renderMorningWorkspace();
      case 'BoardMeeting':
        return this.renderBoardMeetingWorkspace();
      case 'Evening':
        return this.renderEveningWorkspace();
      case 'EndGame':
        return this.renderEndGameWorkspace();
      default:
        return `<div>Awaiting instructions...</div>`;
    }
  },

  // Workspace Phase Screens
  renderIntroWorkspace() {
    const gs = this.state.gameState;
    const isEn = Locales.current === 'en';
    const self = gs.self;

    return `
      <div class="workspace-card dossier-card">
        <div class="dossier-portrait-frame">
          <div style="width: 200px; height: 230px;">
            ${RolePortraits.getPortraitSvg(self.portraitKey, self.factionColor)}
          </div>
          <span style="font-family: var(--font-mono); font-size: 10px; color: var(--shell-text-muted); margin-top: 8px;">
            IDENTIFIER: #${self.id}
          </span>
        </div>
        <div class="dossier-details">
          <div class="dossier-title-line">
            <h2 class="dossier-role-title">${isEn ? self.roleNameEn : self.roleNameRu}</h2>
            <span class="dossier-faction-tag" style="background: ${self.factionColor};">
              ${isEn ? self.faction : self.factionRu}
            </span>
          </div>

          <div>
            <div class="dossier-section-header">OPERATIONAL RESPONSIBILITIES</div>
            <p class="dossier-text">${isEn ? self.descriptionEn : self.descriptionRu}</p>
          </div>

          <div>
            <div class="dossier-section-header">CONFIDENTIAL OVERTIME CAPACITY</div>
            <p class="dossier-text" style="font-weight: 600;">${isEn ? self.abilityEn : self.abilityRu}</p>
          </div>

          ${self.usageLimitsEn ? `
            <div style="background: var(--paper-ivory-aged); padding: 8px 12px; border-left: 3px solid var(--faction-corp); font-family: var(--font-mono); font-size: 11px;">
              ${isEn ? self.usageLimitsEn : self.usageLimitsRu}
            </div>
          ` : ''}

          <div style="margin-top: 10px; display: flex; align-items: center; justify-content: space-between; border-top: 1px solid var(--paper-rule); padding-top: 14px;">
            <span style="font-family: var(--font-mono); font-size: 11px; color: var(--ink-muted);">
              CONTRACT ACKNOWLEDGEMENT
            </span>
            <button class="btn-primary" id="btn-intro-ready">
              Confirm Attendance & Readiness
            </button>
          </div>
        </div>
      </div>
    `;
  },

  renderOvertimeWorkspace() {
    const gs = this.state.gameState;
    const isEn = Locales.current === 'en';
    const action = gs.overtimeAction;
    const self = gs.self;

    if (!self.isAlive) {
      return `
        <div class="workspace-card">
          <div class="paper-memo-header">
            <span class="memo-dept">${Locales.t('overtimeTitle')}</span>
            <span class="stamp stamp-vermilion">${Locales.t('dismissedStamp')}</span>
          </div>
          <div class="paper-memo-body">
            <p style="font-size: 14px; color: var(--ink-charcoal);">
              You have been relieved of active duties. You do not hold overtime authority.
              Proceed to the <strong>Club of Former Colleagues</strong> in communications.
            </p>
          </div>
        </div>
      `;
    }

    if (!action || !action.canAct) {
      return `
        <div class="workspace-card">
          <div class="paper-memo-header">
            <span class="memo-dept">${Locales.t('overtimeTitle')}</span>
            <span class="stamp stamp-teal">NO OVERTIME DUTIES</span>
          </div>
          <div class="paper-memo-body">
            <p style="font-size: 13px; color: var(--ink-charcoal);">
              Your position (${isEn ? self.roleNameEn : self.roleNameRu}) does not require overtime shifts. You may rest at home or monitor authorized channels.
            </p>
            <div class="action-buttons-row">
              <span></span>
              ${!self.isGeneralDirector ? `
                <button class="btn-primary" id="btn-overtime-ready" ${action?.isReady ? 'disabled style="opacity:0.6;"' : ''}>
                  ${action?.isReady ? Locales.t('statusReady') : Locales.t('confirmReadiness')}
                </button>
              ` : ''}
            </div>
          </div>
        </div>
      `;
    }

    const t1Label = isEn ? action.target1LabelEn : action.target1LabelRu;
    const t2Label = isEn ? action.target2LabelEn : action.target2LabelRu;
    const selectedTargets = this.state.selectedOvertimeTargets;

    return `
      <div class="workspace-card">
        <div class="paper-memo-header">
          <div class="memo-meta">
            <span class="memo-dept">${Locales.t('overtimeTitle')}</span>
            <span class="memo-docket">${isEn ? self.roleNameEn : self.roleNameRu} | DIRECTIVE SPECIFICATION</span>
          </div>
          <span class="stamp stamp-teal">ACTIVE DUTY</span>
        </div>

        <div class="paper-memo-body">
          <p style="font-size: 13px; color: var(--ink-charcoal);">
            ${isEn ? self.abilityEn : self.abilityRu}
          </p>

          <div class="action-box">
            <!-- Target 1 Selector -->
            <div class="selector-group">
              <label class="selector-label">${t1Label}</label>
              <select class="form-select" id="select-target-0">
                <option value="">-- Choose Colleague --</option>
                ${action.eligibleTargets1.map(t => `
                  <option value="${t.id}" ${selectedTargets[0] == t.id ? 'selected' : ''}>
                    ${t.name}
                  </option>
                `).join('')}
              </select>
            </div>

            <!-- Target 2 Selector if 2 targets -->
            ${t2Label ? `
              <div class="selector-group">
                <label class="selector-label">${t2Label}</label>
                <select class="form-select" id="select-target-1">
                  <option value="">-- Choose Second Target --</option>
                  ${action.eligibleTargets2.map(t => `
                    <option value="${t.id}" ${selectedTargets[1] == t.id ? 'selected' : ''}>
                      ${t.name}
                    </option>
                  `).join('')}
                </select>
              </div>
            ` : ''}

            <!-- Review Selection Card -->
            <div class="action-review-card">
              <span style="font-family: var(--font-heading); font-size: 12px; font-weight: 700; text-transform: uppercase;">
                ${Locales.t('reviewChoice')}:
              </span>
              <span id="review-selection-text">
                ${selectedTargets.length > 0 ? `Target(s): ${selectedTargets.join(', ')}` : 'No action targets selected.'}
              </span>
            </div>

            <div class="action-buttons-row">
              <button class="btn-secondary" id="btn-overtime-skip">
                ${Locales.t('skipOvertime')}
              </button>
              <div style="display: flex; gap: 10px;">
                <button class="btn-primary" id="btn-submit-action">
                  ${Locales.t('submitOvertime')}
                </button>
                <button class="btn-primary" id="btn-overtime-ready" style="background:#15803d;" ${action.isReady ? 'disabled style="opacity:0.6;"' : ''}>
                  ${action.isReady ? Locales.t('statusReady') : Locales.t('confirmReadiness')}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  },

  renderMorningWorkspace() {
    const gs = this.state.gameState;
    const isEn = Locales.current === 'en';
    const isDirector = gs.self.isGeneralDirector;

    return `
      <div class="workspace-card">
        <div class="paper-memo-header">
          <div class="memo-meta">
            <span class="memo-dept">${Locales.t('phase_Morning')}</span>
            <span class="memo-docket">FISCAL CYCLE #${gs.fiscalTurn} WIRE</span>
          </div>
          <span class="stamp stamp-teal">MORNING DOSSIER</span>
        </div>

        <div class="paper-memo-body">
          <!-- Public News Wire -->
          <div>
            <div style="font-family: var(--font-heading); font-size: 15px; font-weight: 700; color: var(--ink-black); margin-bottom: 6px;">
              ${Locales.t('morningNewsTitle')}
            </div>
            <div style="background: var(--paper-ivory-light); border: 1px solid var(--paper-rule); padding: 12px 16px;">
              ${gs.publicNews.map(n => `
                <div class="feed-item">
                  <div class="feed-item-header">
                    <span class="feed-category">${n.category}</span>
                    <span class="feed-timestamp">${n.timestamp}</span>
                  </div>
                  <p class="feed-message">${isEn ? n.messageEn : n.messageRu}</p>
                </div>
              `).join('')}
            </div>
          </div>

          <!-- Private Confidential Reports -->
          <div>
            <div style="font-family: var(--font-heading); font-size: 15px; font-weight: 700; color: var(--ink-black); margin-bottom: 6px;">
              ${Locales.t('confidentialReportsTitle')}
            </div>
            <div style="background: #fdfbf7; border: 1px solid var(--paper-rule); padding: 12px 16px;">
              ${gs.confidentialReports.length > 0 ? gs.confidentialReports.map(r => `
                <div class="feed-item">
                  <div class="feed-item-header">
                    <span class="feed-category" style="background:#e0e7ff; color:#3730a3;">CONFIDENTIAL MEMO</span>
                    <span class="feed-timestamp">${r.timestamp}</span>
                  </div>
                  <p class="feed-message" style="font-weight: 500;">${isEn ? r.messageEn : r.messageRu}</p>
                </div>
              `).join('') : `
                <span style="font-size: 12px; color: var(--ink-muted);">No confidential reports issued to your terminal this morning.</span>
              `}
            </div>
          </div>

          <!-- Convene Board Meeting Button for General Director -->
          <div style="border-top: 1px solid var(--paper-rule); padding-top: 16px; display: flex; align-items: center; justify-content: space-between;">
            ${isDirector ? `
              <button class="btn-vermilion" id="btn-convene-board" style="width: 100%; font-size: 16px; padding: 14px;">
                ${Locales.t('conveneBoardButton')}
              </button>
            ` : `
              <span style="font-family: var(--font-mono); font-size: 12px; color: var(--ink-muted);">
                ${Locales.t('awaitingDirectorMeeting')}
              </span>
            `}
          </div>
        </div>
      </div>
    `;
  },

  renderBoardMeetingWorkspace() {
    const gs = this.state.gameState;
    const isEn = Locales.current === 'en';
    const bm = gs.boardMeeting;
    const isDirector = gs.self.isGeneralDirector;

    const candidates = bm?.candidatesForFiring || [];
    const hasCandidates = candidates.length > 0;

    return `
      <div class="workspace-card">
        <div class="paper-memo-header">
          <div class="memo-meta">
            <span class="memo-dept">${Locales.t('boardAgendaTitle')}</span>
            <span class="memo-docket">EXECUTIVE SESSION | FISCAL CYCLE #${gs.fiscalTurn}</span>
          </div>
          <span class="stamp ${hasCandidates ? 'stamp-vermilion' : 'stamp-teal'}">
            ${hasCandidates ? 'DISMISSAL REVIEW' : 'ROUTINE DEBATE'}
          </span>
        </div>

        <div class="paper-memo-body">
          <!-- Shortlisted Candidates Box -->
          <div>
            <div style="font-family: var(--font-heading); font-size: 15px; font-weight: 700; color: var(--ink-black); margin-bottom: 6px;">
              ${Locales.t('candidatesListTitle')}
            </div>
            ${hasCandidates ? `
              <div style="display: flex; flex-direction: column; gap: 6px;">
                ${candidates.map(c => `
                  <div style="background: var(--vermilion-surface); border: 1px solid var(--vermilion-border); padding: 10px 14px; display: flex; align-items: center; justify-content: space-between; border-radius: var(--radius-sm);">
                    <span style="font-weight: 700; color: var(--vermilion);">${c.name}</span>
                    ${bm.selectedCandidate === c.id ? `
                      <span class="badge-tag badge-dismissed">${Locales.t('selectedForDismissal')}</span>
                    ` : `
                      <span class="badge-tag badge-dismissed">${Locales.t('nominatedForDismissal')}</span>
                    `}
                  </div>
                `).join('')}
              </div>
            ` : `
              <div style="background: var(--paper-ivory-light); border: 1px solid var(--paper-rule); padding: 12px; font-size: 13px; color: var(--ink-muted);">
                ${Locales.t('noCandidatesText')}
              </div>
            `}
          </div>

          <!-- Morning memos remain available during the board meeting -->
          <div style="margin-top: 14px;">
            <div style="font-family: var(--font-heading); font-size: 15px; font-weight: 700; margin-bottom: 6px;">
              ${Locales.t('morningNewsTitle')}
            </div>
            <div style="background: var(--paper-ivory-light); border: 1px solid var(--paper-rule); padding: 12px 16px;">
              ${(gs.publicNews || []).map(n => `
                <div class="feed-item">
                  <div class="feed-item-header">
                    <span class="feed-category">${n.category}</span>
                    <span class="feed-timestamp">${n.timestamp}</span>
                  </div>
                  <p class="feed-message">${isEn ? n.messageEn : n.messageRu}</p>
                </div>
              `).join('') || `<span style="font-size:12px;color:var(--ink-muted);">${Locales.t('noMorningNews')}</span>`}
            </div>
          </div>

          <div style="margin-top: 10px;">
            <div style="font-family: var(--font-heading); font-size: 15px; font-weight: 700; margin-bottom: 6px;">
              ${Locales.t('confidentialReportsTitle')}
            </div>
            <div style="background:#fdfbf7;border:1px solid var(--paper-rule);padding:12px 16px;">
              ${(gs.confidentialReports || []).length ? (gs.confidentialReports || []).map(r => `
                <div class="feed-item">
                  <div class="feed-item-header">
                    <span class="feed-category">${Locales.t('confidentialMemo')}</span>
                    <span class="feed-timestamp">${r.timestamp}</span>
                  </div>
                  <p class="feed-message">${isEn ? r.messageEn : r.messageRu}</p>
                </div>
              `).join('') : `<span style="font-size:12px;color:var(--ink-muted);">${Locales.t('noConfidentialReports')}</span>`}
            </div>
          </div>

          <!-- General Director Controls Panel -->
          ${isDirector ? `
            <div class="action-box" style="border: 2px solid var(--vermilion); background: #fffbfb;">
              <div style="display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid #fecdd3; padding-bottom: 8px;">
                <span style="font-family: var(--font-heading); font-size: 16px; font-weight: 800; color: var(--vermilion); letter-spacing: 0.08em;">
                  ${Locales.t('directorExecutivePowers')}
                </span>
                <span style="font-family: var(--font-mono); font-size: 12px; font-weight: bold; color: var(--vermilion);">
                  ${Locales.t('remainingVetoes')}: ${bm.vetoCount} / 2 ${bm.vetoUsed ? `(${Locales.t('vetoActive')})` : ''}
                </span>
              </div>

              <!-- Candidate Selection -->
              <div class="selector-group">
                <label class="selector-label">${Locales.t('selectEmployeeDismissal')}</label>
                <select class="form-select" id="select-director-candidate">
                  <option value="">${Locales.t('chooseCandidate')}</option>
                  ${(bm.vetoUsed || !hasCandidates ? gs.roster.filter(p => p.isAlive && p.id !== gs.self.id) : candidates).map(c => `
                    <option value="${c.id}" ${this.state.directorSelectedCandidate == c.id ? 'selected' : ''}>
                      ${c.username || c.name}
                    </option>
                  `).join('')}
                </select>
              </div>

              <!-- Action Buttons -->
              <div class="action-buttons-row">
                <div style="display: flex; gap: 8px;">
                  <button class="btn-secondary" id="btn-director-veto" ${bm.vetoCount <= 0 || bm.vetoUsed ? 'disabled style="opacity:0.5;"' : ''}>
                    ${Locales.t('useVetoButton')}
                  </button>
                  ${bm.canSkipTurn ? `
                    <button class="btn-secondary" id="btn-director-skip">
                      ${Locales.t('skipBoardButton')}
                    </button>
                  ` : ''}
                </div>
                <button class="btn-vermilion" id="btn-director-fire">
                  ${Locales.t('fireCandidateButton')}
                </button>
              </div>
            </div>
          ` : `
            <div style="background: var(--paper-ivory-aged); padding: 12px 16px; border: 1px solid var(--paper-rule-dark); font-size: 13px; color: var(--ink-charcoal);">
              The floor is open in the <strong>Boardroom discussion</strong> (right panel). The General Director is currently reviewing the dismissal docket.
            </div>
          `}
        </div>
      </div>
    `;
  },

  renderEveningWorkspace() {
    const gs = this.state.gameState;
    const isEn = Locales.current === 'en';
    const ev = gs.evening;

    return `
      <div class="workspace-card dossier-card" style="border-color: var(--vermilion);">
        <div class="dossier-portrait-frame" style="border-color: var(--vermilion);">
          <div style="width: 200px; height: 230px;">
            ${RolePortraits.getPortraitSvg(ev.dismissedRoleKey, ev.dismissedFactionColor)}
          </div>
          <span class="stamp stamp-vermilion" style="margin-top: 12px;">${Locales.t('dismissedStamp')}</span>
        </div>
        <div class="dossier-details">
          <div class="dossier-title-line" style="border-color: var(--vermilion);">
            <div>
              <span style="font-family: var(--font-mono); font-size: 11px; color: var(--vermilion); font-weight: bold;">
                ${Locales.t('dismissalOrderTitle')}
              </span>
              <h2 class="dossier-role-title" style="color: var(--vermilion);">${ev.dismissedEmployeeName}</h2>
            </div>
            <span class="dossier-faction-tag" style="background: ${ev.dismissedFactionColor};">
              ${isEn ? ev.dismissedFaction : ev.dismissedFactionRu}
            </span>
          </div>

          <div>
            <div class="dossier-section-header">${Locales.t('revealedRoleLabel')}</div>
            <p class="dossier-text" style="font-size: 18px; font-weight: 700; color: var(--ink-black);">
              ${isEn ? ev.dismissedRoleNameEn : ev.dismissedRoleNameRu}
            </p>
          </div>

          <p class="dossier-text" style="color: var(--ink-muted);">
            By official decree of the General Director, this employee's security credentials have been revoked.
          </p>

          <div style="margin-top: 14px; border-top: 1px solid var(--paper-rule); padding-top: 16px; display: flex; align-items: center; justify-content: flex-end;">
            ${ev.canConfirmDismissal ? `
              <button class="btn-vermilion" id="btn-evening-finish">
                ${Locales.t('finalizeDismissal')}
              </button>
            ` : `
              <span style="font-family: var(--font-mono); font-size: 12px; color: var(--ink-muted);">
                Awaiting General Director or employee to sign termination paperwork...
              </span>
            `}
          </div>
        </div>
      </div>
    `;
  },

  renderEndGameWorkspace() {
    const gs = this.state.gameState;
    const isEn = Locales.current === 'en';
    const eg = gs.endGame;

    return `
      <div class="workspace-card" style="max-width: 900px;">
        <div class="paper-memo-header" style="background: ${eg.winnerColor}; color: #ffffff;">
          <div class="memo-meta">
            <span class="memo-dept" style="color: #ffffff;">${Locales.t('gameOverTitle')}</span>
            <span class="memo-docket" style="color: rgba(255,255,255,0.85);">${isEn ? eg.winnerTitleEn : eg.winnerTitleRu}</span>
          </div>
          <span class="stamp" style="color: #ffffff; border-color: #ffffff;">FINAL AUDIT</span>
        </div>

        <div class="paper-memo-body">
          <div style="text-align: center; padding: 18px 0; border-bottom: 2px solid var(--paper-rule);">
            <h1 style="font-family: var(--font-heading); font-size: 38px; font-weight: 800; color: ${eg.winnerColor}; letter-spacing: 0.08em;">
              ${isEn ? eg.winnerTitleEn : eg.winnerTitleRu}
            </h1>
          </div>

          <!-- Personnel Register Table -->
          <div>
            <div style="font-family: var(--font-heading); font-size: 16px; font-weight: 700; color: var(--ink-black); margin-bottom: 8px;">
              ${Locales.t('personnelRegisterTitle')}
            </div>
            <table style="width: 100%; border-collapse: collapse; font-size: 13px; text-align: left;">
              <thead>
                <tr style="background: var(--paper-ivory-aged); border-bottom: 2px solid var(--paper-rule-dark);">
                  <th style="padding: 8px 12px; font-family: var(--font-mono);">EMPLOYEE</th>
                  <th style="padding: 8px 12px; font-family: var(--font-mono);">STATUS</th>
                  <th style="padding: 8px 12px; font-family: var(--font-mono);">FINAL POSITION</th>
                  <th style="padding: 8px 12px; font-family: var(--font-mono);">FACTION</th>
                </tr>
              </thead>
              <tbody>
                ${eg.personnelRegister.map(p => `
                  <tr style="border-bottom: 1px solid var(--paper-rule);">
                    <td style="padding: 8px 12px; font-weight: 600;">${p.username}</td>
                    <td style="padding: 8px 12px;">
                      <span class="badge-tag ${p.isAlive ? 'badge-ready' : 'badge-dismissed'}">
                        ${p.isAlive ? 'SURVIVED' : 'DISMISSED'}
                      </span>
                    </td>
                    <td style="padding: 8px 12px; font-weight: 600;">${isEn ? p.roleNameEn : p.roleNameRu}</td>
                    <td style="padding: 8px 12px;">
                      <span style="color: ${p.factionColor}; font-weight: bold;">
                        ${isEn ? p.faction : p.factionRu}
                      </span>
                    </td>
                  </tr>
                `).join('')}
              </tbody>
            </table>
          </div>

          <!-- Chronological Turn History -->
          ${eg.turnHistory?.length ? `
            <div style="margin-top: 16px;">
              <div style="font-family: var(--font-heading); font-size: 16px; font-weight: 700; color: var(--ink-black); margin-bottom: 8px;">
                ${Locales.t('turnHistoryTitle')}
              </div>
              <table style="width: 100%; border-collapse: collapse; font-size: 12px; text-align: left;">
                <thead>
                  <tr style="background: var(--paper-ivory-aged); border-bottom: 2px solid var(--paper-rule-dark);">
                    <th style="padding: 8px 12px;">TURN</th>
                    <th style="padding: 8px 12px;">BOARD ACTIONS</th>
                    <th style="padding: 8px 12px;">OVERTIME ACTIONS</th>
                  </tr>
                </thead>
                <tbody>
                  ${eg.turnHistory.map(round => `
                    <tr style="border-bottom: 1px solid var(--paper-rule);">
                      <td style="padding: 8px 12px; font-weight: 700;">${round.turn}</td>
                      <td style="padding: 8px 12px;">
                        ${round.firedPlayerName ? `Dismissed: ${round.firedPlayerName}` : (round.candidatesForFiring?.length ? `Candidates: ${round.candidatesForFiring.join(', ')}` : 'No dismissal action')}
                      </td>
                      <td style="padding: 8px 12px;">
                        ${round.overtimeActionsSummary?.length ? round.overtimeActionsSummary.join('<br>') : 'No overnight actions'}
                      </td>
                    </tr>
                  `).join('')}
                </tbody>
              </table>
            </div>
          ` : ''}

          <div style="display: flex; justify-content: center; margin-top: 18px;">
            <button class="btn-primary" id="btn-end-game-hall">
              ${Locales.t('playAgain')}
            </button>
          </div>
        </div>
      </div>
    `;
  },

  // Right Column: Scoped Communications & Reports
  renderCommsColumn() {
    const gs = this.state.gameState;
    const isEn = Locales.current === 'en';
    const scopes = gs.availableChatScopes || [];
    const activeScope = scopes.find(s => s.scope === this.state.activeChatScope) || scopes[0];

    return `
      <aside class="comms-column">
        <!-- Tabs Header -->
        <div class="comms-tabs-bar">
          ${scopes.map(s => {
            const isActive = s.scope === this.state.activeChatScope;
            const label = isEn ? s.labelEn : s.labelRu;
            return `
              <div class="comms-tab ${isActive ? 'active' : ''}" data-scope="${s.scope}">
                ${label}
              </div>
            `;
          }).join('')}
        </div>

        <!-- Audience Persistent Header -->
        <div class="audience-header-label">
          <span>${isEn ? activeScope?.audienceBadgeEn : activeScope?.audienceBadgeRu}</span>
          ${!activeScope?.canWrite ? `<span style="color:var(--vermilion); font-size:9px;">LOCKED</span>` : ''}
        </div>

        <!-- Message Feed -->
        <div class="chat-messages-container" id="chat-messages-container">
          <!-- Populated dynamically -->
        </div>

        <!-- Chat Input Bar -->
        <form class="chat-input-bar" id="form-chat">
          <input class="chat-input" id="chat-input" placeholder="${activeScope?.canWrite ? Locales.t('typeMessage') : Locales.t('lockedChannel')}" ${!activeScope?.canWrite ? 'disabled' : ''} />
          <button class="chat-send-btn" type="submit" ${!activeScope?.canWrite ? 'disabled style="opacity:0.5;"' : ''}>
            ${Locales.t('sendChat')}
          </button>
        </form>
      </aside>
    `;
  },

  renderChatMessages() {
    const container = document.getElementById('chat-messages-container');
    if (!container) return;

    const scope = this.state.activeChatScope;
    const messages = this.state.chatMessages[scope] || [];

    if (messages.length === 0) {
      container.innerHTML = `
        <div style="font-family: var(--font-mono); font-size: 11px; color: var(--shell-text-muted); text-align: center; margin: auto;">
          ${Locales.t('noMessagesYet')}
        </div>
      `;
      return;
    }

    container.innerHTML = messages.map(m => `
      <div class="chat-message-bubble">
        <div class="chat-meta">
          <span class="chat-sender">${m.senderName}</span>
          <span class="chat-time">${m.timestamp}</span>
        </div>
        <p class="chat-text">${m.content}</p>
      </div>
    `).join('');

    container.scrollTop = container.scrollHeight;
  },

  renderMobileNav() {
    const current = this.state.mobileTab;
    return `
      <nav class="mobile-nav-bar">
        <button class="mobile-tab-btn ${current === 'workspace' ? 'active' : ''}" data-tab="workspace">
          ${Locales.t('tabWorkspace')}
        </button>
        <button class="mobile-tab-btn ${current === 'roster' ? 'active' : ''}" data-tab="roster">
          ${Locales.t('tabRoster')}
        </button>
        <button class="mobile-tab-btn ${current === 'comms' ? 'active' : ''}" data-tab="comms">
          ${Locales.t('tabComms')}
        </button>
      </nav>
    `;
  },

  // Modals
  renderModal() {
    if (!this.state.activeModal) return '';

    const isEn = Locales.current === 'en';

    if (this.state.activeModal === 'createRoom') {
      return `
        <div class="modal-overlay" id="modal-overlay">
          <div class="modal-dialog">
            <div class="modal-header">
              <span class="modal-title">${Locales.t('requisitionBoardroom')}</span>
              <button class="btn-utility" id="btn-close-modal">✕</button>
            </div>
            <div class="modal-body">
              <form id="form-create-room" style="display:flex; flex-direction:column; gap:14px;">
                <div class="selector-group">
                  <label class="selector-label">${Locales.t('roomTitle')}</label>
                  <input class="form-input" id="create-room-title" required placeholder="e.g. Q4 Executive Committee" />
                </div>
                <div class="selector-group">
                  <label class="selector-label">${Locales.t('roomDescription')}</label>
                  <input class="form-input" id="create-room-desc" placeholder="Special convened oversight session" />
                </div>
                <div class="selector-group">
                  <label class="selector-label">${Locales.t('roomSeats')}</label>
                  <input class="form-input" id="create-room-seats" type="number" min="2" max="25" value="6" required />
                </div>
                <div class="modal-footer" style="padding: 12px 0 0 0; border-top: none;">
                  <button type="button" class="btn-secondary" id="btn-cancel-modal">${Locales.t('cancel')}</button>
                  <button type="submit" class="btn-primary">${Locales.t('createRoom')}</button>
                </div>
              </form>
            </div>
          </div>
        </div>
      `;
    }

    if (this.state.activeModal === 'roleDirectory') {
      const roles = this.state.roleDirectory;
      return `
        <div class="modal-overlay" id="modal-overlay">
          <div class="modal-dialog" style="max-width: 820px;">
            <div class="modal-header">
              <span class="modal-title">${Locales.t('roleDirectoryTitle')}</span>
              <button class="btn-utility" id="btn-close-modal">✕</button>
            </div>
            <div class="modal-body" style="display: grid; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr)); gap: 14px;">
              ${roles.map(r => `
                <div style="background: var(--paper-ivory-light); border: 1px solid var(--paper-rule); padding: 12px; display: flex; flex-direction: column; gap: 6px;">
                  <div style="display: flex; align-items: center; justify-content: space-between;">
                    <span style="font-family: var(--font-heading); font-size: 15px; font-weight: 700; color: var(--ink-black);">
                      ${r.factionIndicator} ${isEn ? r.nameEn : r.nameRu}
                    </span>
                    <span class="dossier-faction-tag" style="background: ${r.factionColor}; font-size: 9px;">
                      ${isEn ? r.teamName : r.teamNameRu}
                    </span>
                  </div>
                  <p style="font-size: 11px; color: var(--ink-charcoal); line-height: 1.4;">
                    ${isEn ? r.descriptionEn : r.descriptionRu}
                  </p>
                  <div style="margin-top: auto; border-top: 1px dashed var(--paper-rule); padding-top: 4px; font-family: var(--font-mono); font-size: 10px; color: var(--ink-muted);">
                    ABILITY: ${isEn ? r.abilityEn : r.abilityRu}
                  </div>
                </div>
              `).join('')}
            </div>
            <div class="modal-footer">
              <button class="btn-secondary" id="btn-close-modal-footer">${Locales.t('close')}</button>
            </div>
          </div>
        </div>
      `;
    }

    if (this.state.activeModal === 'dossier') {
      const gs = this.state.gameState;
      const self = gs?.self;
      if (!self) return '';
      return `
        <div class="modal-overlay" id="modal-overlay">
          <div class="modal-dialog" style="max-width: 650px;">
            <div class="modal-header">
              <span class="modal-title">${Locales.t('myDossier')}</span>
              <button class="btn-utility" id="btn-close-modal">✕</button>
            </div>
            <div class="modal-body" style="padding: 0;">
              <div class="dossier-card" style="border: none; box-shadow: none;">
                <div class="dossier-portrait-frame">
                  <div style="width: 180px; height: 210px;">
                    ${RolePortraits.getPortraitSvg(self.portraitKey, self.factionColor)}
                  </div>
                </div>
                <div class="dossier-details" style="padding: 20px;">
                  <h2 class="dossier-role-title">${isEn ? self.roleNameEn : self.roleNameRu}</h2>
                  <span class="dossier-faction-tag" style="background: ${self.factionColor}; width: fit-content;">
                    ${isEn ? self.faction : self.factionRu}
                  </span>
                  <p class="dossier-text">${isEn ? self.descriptionEn : self.descriptionRu}</p>
                  <div class="dossier-section-header">AUTHORITY</div>
                  <p class="dossier-text" style="font-weight: 600;">${isEn ? self.abilityEn : self.abilityRu}</p>
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button class="btn-secondary" id="btn-close-modal-footer">${Locales.t('close')}</button>
            </div>
          </div>
        </div>
      `;
    }

    if (this.state.activeModal === 'confirmFire') {
      const candidateId = this.state.directorSelectedCandidate;
      const p = this.state.gameState?.roster?.find(x => x.id == candidateId);
      const name = p?.username || `Employee #${candidateId}`;
      return `
        <div class="modal-overlay" id="modal-overlay">
          <div class="modal-dialog modal-vermilion">
            <div class="modal-header" style="background:#fff1f2; border-color:var(--vermilion);">
              <span class="modal-title" style="color:var(--vermilion);">${Locales.t('confirmDismissalDialog')}</span>
              <button class="btn-utility" id="btn-close-modal">✕</button>
            </div>
            <div class="modal-body">
              <p style="font-size: 15px; font-weight: 700; color: var(--ink-black);">
                TARGET TO TERMINATE: <span style="color:var(--vermilion);">${name}</span>
              </p>
              <p style="font-size: 13px; color: var(--ink-charcoal);">
                ${Locales.t('confirmDismissalPrompt')}
              </p>
            </div>
            <div class="modal-footer" style="background:#fff1f2; border-color:var(--vermilion);">
              <button class="btn-secondary" id="btn-cancel-modal">${Locales.t('cancel')}</button>
              <button class="btn-vermilion" id="btn-confirm-fire-final">${Locales.t('confirmFire')}</button>
            </div>
          </div>
        </div>
      `;
    }

    if (this.state.activeModal === 'confirmVeto') {
      return `
        <div class="modal-overlay" id="modal-overlay">
          <div class="modal-dialog modal-vermilion">
            <div class="modal-header" style="background:#fff1f2; border-color:var(--vermilion);">
              <span class="modal-title" style="color:var(--vermilion);">${Locales.t('confirmVetoDialog')}</span>
              <button class="btn-utility" id="btn-close-modal">✕</button>
            </div>
            <div class="modal-body">
              <p style="font-size: 13px; color: var(--ink-charcoal);">
                ${Locales.t('confirmVetoPrompt')}
              </p>
            </div>
            <div class="modal-footer" style="background:#fff1f2; border-color:var(--vermilion);">
              <button class="btn-secondary" id="btn-cancel-modal">${Locales.t('cancel')}</button>
              <button class="btn-vermilion" id="btn-confirm-veto-final">${Locales.t('confirmVeto')}</button>
            </div>
          </div>
        </div>
      `;
    }

    return '';
  },

  openModal(modalName) {
    this.state.activeModal = modalName;
    this.render();
  },

  closeModal() {
    this.state.activeModal = null;
    this.render();
  },

  // Attach DOM Event Listeners
  attachDomEvents() {
    // Header
    document.getElementById('btn-toggle-lang')?.addEventListener('click', () => this.toggleLanguage());
    document.getElementById('btn-browse-roles')?.addEventListener('click', () => this.openModal('roleDirectory'));
    document.getElementById('btn-toggle-dossier')?.addEventListener('click', () => this.openModal('dossier'));
    document.getElementById('btn-sign-out')?.addEventListener('click', () => {
      API.clearAuth();
      this.showAuth();
    });

    // Mobile Navigation
    document.querySelectorAll('.mobile-tab-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        this.setMobileTab(btn.dataset.tab);
      });
    });

    // Modal Close
    document.getElementById('btn-close-modal')?.addEventListener('click', () => this.closeModal());
    document.getElementById('btn-close-modal-footer')?.addEventListener('click', () => this.closeModal());
    document.getElementById('btn-cancel-modal')?.addEventListener('click', () => this.closeModal());

    // Auth screen events
    document.getElementById('form-auth')?.addEventListener('submit', async (e) => {
      e.preventDefault();
      const u = document.getElementById('auth-username').value;
      const p = document.getElementById('auth-password').value;
      try {
        await API.signIn(u, p);
        this.showHall();
      } catch (err) {
        this.showToast(err.message, true);
      }
    });

    document.getElementById('btn-submit-signup')?.addEventListener('click', async () => {
      const u = document.getElementById('auth-username').value;
      const p = document.getElementById('auth-password').value;
      if (!u || !p) {
        this.showToast('Please provide employee alias and passcode.', true);
        return;
      }
      try {
        await API.signUp(u, p);
        this.showHall();
      } catch (err) {
        this.showToast(err.message, true);
      }
    });

    document.getElementById('btn-guest-login')?.addEventListener('click', async () => {
      const u = document.getElementById('auth-username')?.value;
      try {
        await API.guestLogin(u);
        this.showHall();
      } catch (err) {
        this.showToast(err.message, true);
      }
    });

    // Hall screen events
    document.getElementById('btn-refresh-hall')?.addEventListener('click', () => this.refreshHallRooms());
    document.getElementById('btn-open-create-room')?.addEventListener('click', () => this.openModal('createRoom'));

    // Create room form
    document.getElementById('form-create-room')?.addEventListener('submit', async (e) => {
      e.preventDefault();
      const title = document.getElementById('create-room-title').value;
      const desc = document.getElementById('create-room-desc').value;
      const seats = document.getElementById('create-room-seats').value;
      try {
        const room = await API.createRoom(title, desc, seats);
        this.closeModal();
        this.showLobby(room.id);
      } catch (err) {
        this.showToast(err.message, true);
      }
    });

    // Lobby screen events
    document.getElementById('btn-toggle-ready')?.addEventListener('click', async () => {
      try {
        if (API.hubConnection && API.hubConnection.state === 'Connected') {
          await API.hubConnection.invoke('ToggleParanoiaReady', this.state.roomId);
        } else {
          // Re-fetch
          await this.refreshLobby();
        }
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    document.getElementById('btn-clear-roles')?.addEventListener('click', async () => {
      try {
        await API.clearRolePool(this.state.roomId);
        await this.refreshLobby();
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    document.querySelectorAll('.btn-role-plus').forEach(btn => {
      btn.addEventListener('click', async () => {
        const role = btn.dataset.role;
        const current = this.state.lobbyData?.rolesPool[role] || 0;
        try {
          await API.updateRolePool(this.state.roomId, role, current + 1);
          await this.refreshLobby();
        } catch (e) {
          this.showToast(e.message, true);
        }
      });
    });

    document.querySelectorAll('.btn-role-minus').forEach(btn => {
      btn.addEventListener('click', async () => {
        const role = btn.dataset.role;
        const current = this.state.lobbyData?.rolesPool[role] || 0;
        if (current <= 0) return;
        try {
          await API.updateRolePool(this.state.roomId, role, current - 1);
          await this.refreshLobby();
        } catch (e) {
          this.showToast(e.message, true);
        }
      });
    });

    document.getElementById('btn-convene-game')?.addEventListener('click', async () => {
      try {
        await API.startParanoiaGame(this.state.roomId);
        this.showGame(this.state.roomId);
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    // Game screen: Intro Ready
    document.getElementById('btn-intro-ready')?.addEventListener('click', async () => {
      try {
        await API.submitAction(this.state.roomId, 'intro_ready');
        await this.refreshGameState();
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    // Game screen: Overtime Target Selectors
    const sel0 = document.getElementById('select-target-0');
    const sel1 = document.getElementById('select-target-1');
    const updateOvertimeSelections = () => {
      const targets = [];
      if (sel0 && sel0.value) targets.push(parseInt(sel0.value, 10));
      if (sel1 && sel1.value) targets.push(parseInt(sel1.value, 10));
      this.state.selectedOvertimeTargets = targets;
      const revText = document.getElementById('review-selection-text');
      if (revText) {
        revText.textContent = targets.length > 0 ? `Target(s): ${targets.join(', ')}` : 'No action targets selected.';
      }
    };
    sel0?.addEventListener('change', updateOvertimeSelections);
    sel1?.addEventListener('change', updateOvertimeSelections);

    document.getElementById('btn-submit-action')?.addEventListener('click', async () => {
      try {
        const targets = this.state.selectedOvertimeTargets;
        await API.submitAction(this.state.roomId, 'overtime_action', targets);
        this.showToast('Overtime assignment filed successfully.');
        await this.refreshGameState();
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    document.getElementById('btn-overtime-skip')?.addEventListener('click', async () => {
      try {
        await API.submitAction(this.state.roomId, 'overtime_skip');
        this.state.selectedOvertimeTargets = [];
        this.showToast('Overtime duties skipped.');
        await this.refreshGameState();
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    document.getElementById('btn-overtime-ready')?.addEventListener('click', async () => {
      try {
        await API.submitAction(this.state.roomId, 'overtime_ready');
        await this.refreshGameState();
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    // Game screen: Morning Convene Board
    document.getElementById('btn-convene-board')?.addEventListener('click', async () => {
      try {
        await API.submitAction(this.state.roomId, 'director_start_board');
        await this.refreshGameState();
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    // Game screen: Board Meeting Controls
    document.getElementById('select-director-candidate')?.addEventListener('change', async (e) => {
      const val = e.target.value ? parseInt(e.target.value, 10) : null;
      this.state.directorSelectedCandidate = val;
      if (val) {
        try {
          await API.submitAction(this.state.roomId, 'director_select', [], val);
          await this.refreshGameState();
        } catch (err) {
          this.showToast(err.message, true);
        }
      }
    });

    document.getElementById('btn-director-fire')?.addEventListener('click', () => {
      if (!this.state.directorSelectedCandidate) {
        this.showToast(Locales.t('selectCandidatePrompt'), true);
        return;
      }
      this.openModal('confirmFire');
    });

    document.getElementById('btn-confirm-fire-final')?.addEventListener('click', async () => {
      try {
        const candidateId = this.state.directorSelectedCandidate;
        await API.submitAction(this.state.roomId, 'director_fire', [], candidateId);
        this.closeModal();
        await this.refreshGameState();
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    document.getElementById('btn-director-veto')?.addEventListener('click', () => {
      this.openModal('confirmVeto');
    });

    document.getElementById('btn-confirm-veto-final')?.addEventListener('click', async () => {
      try {
        await API.submitAction(this.state.roomId, 'director_veto');
        this.closeModal();
        await this.refreshGameState();
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    document.getElementById('btn-director-skip')?.addEventListener('click', async () => {
      try {
        await API.submitAction(this.state.roomId, 'director_skip');
        await this.refreshGameState();
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    // Game screen: Evening Finish
    document.getElementById('btn-evening-finish')?.addEventListener('click', async () => {
      try {
        await API.submitAction(this.state.roomId, 'evening_finish');
        await this.refreshGameState();
      } catch (e) {
        this.showToast(e.message, true);
      }
    });

    // Game screen: End Game Return
    document.getElementById('btn-end-game-hall')?.addEventListener('click', async () => {
      try {
        await API.endParanoiaSession(this.state.roomId);
      } catch (e) {
        this.showToast(e.message, true);
      }
      this.showHall();
    });

    // Comms: Chat tab switching
    document.querySelectorAll('.comms-tab').forEach(tab => {
      tab.addEventListener('click', async () => {
        const scope = tab.dataset.scope;
        this.state.activeChatScope = scope;
        document.querySelectorAll('.comms-tab').forEach(t => t.classList.toggle('active', t.dataset.scope === scope));
        this.render();
        await this.refreshChat(scope);
      });
    });

    // Comms: Chat send
    document.getElementById('form-chat')?.addEventListener('submit', async (e) => {
      e.preventDefault();
      const input = document.getElementById('chat-input');
      const text = input?.value?.trim();
      if (!text) return;
      try {
        await API.sendChat(this.state.roomId, this.state.activeChatScope, text);
        input.value = '';
        await this.refreshChat(this.state.activeChatScope);
      } catch (err) {
        this.showToast(err.message, true);
      }
    });
  }
};

// Bootstrap application on load
window.addEventListener('DOMContentLoaded', () => {
  App.init();
});
