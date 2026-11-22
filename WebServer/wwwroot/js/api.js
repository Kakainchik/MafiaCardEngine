// Paranoia Corp - API Client & SignalR Real-Time Manager

const API = {
  token: localStorage.getItem('paranoia_jwt') || null,
  user: JSON.parse(localStorage.getItem('paranoia_user') || 'null'),
  hubConnection: null,
  activeRoomId: null,

  setAuth(authResponse) {
    this.token = authResponse.jwtToken;
    this.user = {
      id: authResponse.id,
      username: authResponse.username
    };
    localStorage.setItem('paranoia_jwt', this.token);
    localStorage.setItem('paranoia_user', JSON.stringify(this.user));
  },

  clearAuth() {
    this.token = null;
    this.user = null;
    localStorage.removeItem('paranoia_jwt');
    localStorage.removeItem('paranoia_user');
    if (this.hubConnection) {
      try { this.hubConnection.stop(); } catch(e) {}
      this.hubConnection = null;
    }
  },

  isAuthenticated() {
    return !!this.token && !!this.user;
  },

  async request(path, options = {}) {
    const headers = options.headers || {};
    headers['Content-Type'] = 'application/json';
    if (this.token) {
      headers['Authorization'] = `Bearer ${this.token}`;
    }

    const config = {
      ...options,
      headers
    };

    const res = await fetch(path, config);
    if (res.status === 401) {
      this.clearAuth();
      window.dispatchEvent(new CustomEvent('auth-expired'));
      throw new Error('Authorization expired. Please re-authenticate.');
    }

    if (!res.ok) {
      let errorMsg = `HTTP Error ${res.status}`;
      try {
        const data = await res.json();
        errorMsg = data.error || data.message || errorMsg;
      } catch (e) {}
      throw new Error(errorMsg);
    }

    // Return json if available
    const contentType = res.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      return await res.json();
    }
    return null;
  },

  // Auth Endpoints
  async signIn(username, password) {
    const res = await this.request('/api/auth/sign_in', {
      method: 'POST',
      body: JSON.stringify({ username, password })
    });
    this.setAuth(res);
    return res;
  },

  async signUp(username, password) {
    await this.request('/api/auth/sign_up', {
      method: 'POST',
      body: JSON.stringify({ username, password })
    });
    return await this.signIn(username, password);
  },

  async guestLogin(username) {
    const res = await this.request('/api/auth/guest', {
      method: 'POST',
      body: JSON.stringify({ username: username || null })
    });
    this.setAuth(res);
    return res;
  },

  // Hall & Lobby Endpoints
  async getRooms(page = 1) {
    return await this.request(`/api/hall/${page}/true`);
  },

  async createRoom(title, description, maxSeats = 8) {
    return await this.request('/api/hall/new_lobby', {
      method: 'POST',
      body: JSON.stringify({
        title,
        description,
        maxSeats: parseInt(maxSeats, 10)
      })
    });
  },

  // Paranoia Corp Endpoints
  async getRoleDirectory() {
    return await this.request('/api/paranoia/roles');
  },

  async getParanoiaRoom(roomId) {
    return await this.request(`/api/paranoia/room/${roomId}`);
  },

  async updateRolePool(roomId, role, count) {
    return await this.request(`/api/paranoia/room/${roomId}/role`, {
      method: 'POST',
      body: JSON.stringify({ role, count })
    });
  },

  async clearRolePool(roomId) {
    return await this.request(`/api/paranoia/room/${roomId}/clear_roles`, {
      method: 'POST'
    });
  },

  async startParanoiaGame(roomId) {
    return await this.request(`/api/paranoia/room/${roomId}/start`, {
      method: 'POST'
    });
  },

  async getGameState(roomId) {
    return await this.request(`/api/paranoia/room/${roomId}/state`);
  },

  async submitAction(roomId, actionType, targets = [], candidateId = null) {
    return await this.request(`/api/paranoia/room/${roomId}/action`, {
      method: 'POST',
      body: JSON.stringify({ actionType, targets, candidateId })
    });
  },

  async getChat(roomId, scope = 'PUBLIC') {
    return await this.request(`/api/paranoia/room/${roomId}/chat?scope=${encodeURIComponent(scope)}`);
  },

  async sendChat(roomId, scope, message) {
    return await this.request(`/api/paranoia/room/${roomId}/chat`, {
      method: 'POST',
      body: JSON.stringify({ scope, message })
    });
  },

  // SignalR Real-Time Manager
  async connectSignalR(roomId) {
    if (!this.token) return;
    this.activeRoomId = roomId;

    if (this.hubConnection && this.hubConnection.state === 'Connected') {
      try {
        await this.hubConnection.invoke('JoinParanoiaRoom', roomId);
      } catch (e) {
        console.warn('SignalR Join failed, reconnecting...', e);
      }
      return;
    }

    if (typeof signalR === 'undefined') {
      console.warn('SignalR script not loaded, falling back to REST polling.');
      this.startPollingFallback(roomId);
      return;
    }

    try {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(`/hub/lobby?accessToken=${encodeURIComponent(this.token)}`, {
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

      this.hubConnection.on('ParanoiaRoomUpdated', (rId) => {
        window.dispatchEvent(new CustomEvent('paranoia-room-updated', { detail: { roomId: rId } }));
      });

      this.hubConnection.on('ParanoiaGameStarted', (rId) => {
        window.dispatchEvent(new CustomEvent('paranoia-game-started', { detail: { roomId: rId } }));
      });

      this.hubConnection.on('ParanoiaStateChanged', (rId) => {
        window.dispatchEvent(new CustomEvent('paranoia-state-changed', { detail: { roomId: rId } }));
      });

      this.hubConnection.on('ParanoiaChatUpdated', (rId, scope) => {
        window.dispatchEvent(new CustomEvent('paranoia-chat-updated', { detail: { roomId: rId, scope } }));
      });

      this.hubConnection.on('ParanoiaMorningBrief', (rId) => {
        window.dispatchEvent(new CustomEvent('paranoia-morning-brief', { detail: { roomId: rId } }));
      });

      await this.hubConnection.start();
      console.log('[SignalR] Connected successfully.');
      await this.hubConnection.invoke('JoinParanoiaRoom', roomId);
    } catch (err) {
      console.error('[SignalR] Connection error:', err);
      this.startPollingFallback(roomId);
    }
  },

  pollingInterval: null,
  startPollingFallback(roomId) {
    if (this.pollingInterval) clearInterval(this.pollingInterval);
    this.pollingInterval = setInterval(() => {
      if (this.activeRoomId === roomId) {
        window.dispatchEvent(new CustomEvent('paranoia-state-changed', { detail: { roomId } }));
        window.dispatchEvent(new CustomEvent('paranoia-room-updated', { detail: { roomId } }));
      }
    }, 2500);
  },

  stopPolling() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
      this.pollingInterval = null;
    }
  }
};
