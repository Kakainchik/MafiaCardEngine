# Paranoia Corp. — Social Deduction Server & Web Client

An ASP.NET Core server and embedded mid-century corporate noir Web UI for **Paranoia Corp.**, a multiplayer social deduction game about corporate intrigue, espionage, and internal sabotage.

Employees receive confidential dossiers, investigate and manipulate colleagues during Overtime, and assemble in the Boardroom to plead their case before the General Director’s dismissal decisions.

The web client runs directly inside modern web browsers on desktop and mobile without requiring any desktop client or plugins, synchronizing all players in real time via WebSockets (SignalR). Voice chat is deliberately excluded to maintain the quiet, confidential paper-trail atmosphere of the corporate noir setting.

---

## Table of Contents
1. [Visual Aesthetic & Design](#visual-aesthetic--design)
2. [Architecture & Features](#architecture--features)
3. [Gameplay Cycles & Mechanics](#gameplay-cycles--mechanics)
4. [Roles & Factions Overview](#roles--factions-overview)
5. [Local Development Quick Start](#local-development-quick-start)
6. [VPS Production Deployment](#vps-production-deployment)
   - [Method 1: Docker & Docker Compose (Recommended)](#method-1-docker--docker-compose-recommended)
   - [Method 2: Linux systemd Service + Nginx Reverse Proxy](#method-2-linux-systemd-service--nginx-reverse-proxy)
   - [Method 3: Turnkey HTTPS with Caddy](#method-3-turnkey-https-with-caddy)
7. [Database Options (SQLite / PostgreSQL)](#database-options)
8. [Playing the Game: Host & Player Walkthrough](#playing-the-game-host--player-walkthrough)
9. [SignalR WebSockets Configuration](#signalr-websockets-configuration)
10. [Troubleshooting & FAQ](#troubleshooting--faq)

---

## Visual Aesthetic & Design

The web interface is styled as the company’s internal intranet personnel portal:

* **Atmosphere**: Mid-century corporate noir with clean modern usability. Personnel dossiers, stamped memoranda, typewriter-style timestamps, and high-contrast ink-and-gouache silhouettes with angled Venetian blinds lighting.
* **Palette**:
  * **Shell**: Charcoal (`#121920`) and Deep Petrol Blue (`#1c2830`).
  * **Surfaces**: Warm Ivory (`#f4f0e6`) for dossier cards and memoranda.
  * **Decisions**: Restrained Vermilion (`#d9483b`) for dismissal notices, vetoes, and irreversible actions.
* **Faction Accents & Indicators**:
  * 🏢 **Corporation** — Teal (`#0d9488`)
  * 🕶️ **Syndicate** — Brick Red (`#b91c1c`)
  * 🚀 **Startup** — Olive Green (`#4d7c0f`)
  * 💼 **Outsource** — Ochre (`#b45309`)
  * ⚖️ **Independents** — Plum (`#701a75`)
* **Typography**:
  * **Headings**: *Barlow Condensed* (bold, authoritarian corporate tone).
  * **Interface Text**: *IBM Plex Sans* (clean modern legibility).
  * **Timestamps & Codes**: *IBM Plex Mono* (teletype audit trail).
  * Full bilingual support (**English** & **Russian**) tested with long Cyrillic titles (e.g. *«Кандидат на Увольнение»*, *«Системный Администратор»*).
* **Responsive Layout**:
  * **Desktop**: 3-column view: Employee Roster (left), Current Phase / Workspace (center), Scoped Communications & Reports (right).
  * **Mobile**: Streamlined view prioritizing the current active decision, with tabs for Dossier, Roster, and Channels.

---

## Architecture & Features

```
               [ Web Browser Clients (Desktop / Mobile) ]
                                   │
                    HTTP REST API  │  SignalR WebSockets
                     & Static SPA  │  (/hub/lobby)
                                   ▼
        ┌────────────────────────────────────────────────────────┐
        │                 ASP.NET Core WebServer                 │
        │                                                        │
        │  ┌───────────────────────┐   ┌──────────────────────┐  │
        │  │ ParanoiaController    │   │ LobbyHub (SignalR)   │  │
        │  │ - Auth / Guest login  │   │ - Room broadcasts    │  │
        │  │ - Role catalogue      │   │ - Real-time actions  │  │
        │  │ - Game views & chats  │   │ - State sync         │  │
        │  └───────────────────────┘   └──────────────────────┘  │
        │                             │                          │
        │  ┌──────────────────────────┴───────────────────────┐  │
        │  │            ParanoiaGameSession Manager           │  │
        │  │  - Action validation & resolution                │  │
        │  │  - Scoped communication permissions              │  │
        │  │  - ParanoiaLogFacade (announcements/reports)     │  │
        │  └──────────────────────────┬───────────────────────┘  │
        │                             │                          │
        │  ┌──────────────────────────▼───────────────────────┐  │
        │  │           GameLogic.ParanoiaCorp Core            │  │
        │  │  - Engine, Cycles, Roles, Win Conditions         │  │
        │  └──────────────────────────────────────────────────┘  │
        │                             │                          │
        │  ┌──────────────────────────▼───────────────────────┐  │
        │  │        EF Core Database (SQLite / PostgreSQL)    │  │
        │  └──────────────────────────────────────────────────┘  │
        └────────────────────────────────────────────────────────┘
```

* **Full Cycle Synchronization**: Drives games through Introductory Day, Overtime, Morning Brief, Board Meeting, Evening, and End Game.
* **Role-Specific Scoped Chat Channels**:
  * **Boardroom (Public)**: Active during Board Meetings for living employees; read-only at other times.
  * **Syndicate (Encrypted)**: Visible exclusively to Syndicate operatives.
  * **Startup Wire**: Communication between Startup Founders and Evangelists.
  * **Shareholder Backchannel**: Private channel for company Shareholders.
  * **Outsource Network**: Channel for Outsourcers and the Alumni Manager.
  * **Dismissed Employees (Archive)**: Channel for dead/dismissed staff and the Alumni Manager.
* **Turnkey Persistence**: Zero-setup SQLite database by default; automatically switches to PostgreSQL when a connection string is supplied.
* **Fast Guest Authentication**: Players can join with one click by providing a display name, or register permanent accounts.

---

## Gameplay Cycles & Mechanics

1. **Introductory Day (`IntroductoryDayCycle`)**:
   * Employees sign confidentiality contracts and inspect their role dossiers.
   * Host reviews readiness; once all employees confirm, Overtime commences.
2. **Overtime (`OvertimeCycle`)**:
   * Employees submit their confidential night abilities (protection, investigation, redirection, compromise, recruitment, sabotage).
   * For dual-target abilities, targets are clearly labelled by purpose (e.g. *«Employee to control»* and *«New target»*).
   * Players can review their choice before submitting, or choose *«Skip Action»*.
   * When all living employees are ready, actions resolve deterministically.
3. **Morning Brief (`MorningCycle`)**:
   * Overnight wire reports are published.
   * Public dismissal/survival announcements are broadcast to all.
   * Private confidential reports and delayed auditor findings are delivered strictly to authorized dossiers.
   * The General Director convenes the Board Meeting.
4. **Board Meeting (`DirectorBoardCycle`)**:
   * Live public discussion is opened.
   * Dismissal shortlist is generated from overnight triggers and nominations.
   * The General Director reviews candidates and chooses:
     * **Confirm Dismissal**: Fires the employee.
     * **Use Veto**: Spares the candidate using one of the Director's 2 Veto powers.
     * **Executive Mercy / Skip**: Proceeds without dismissing anyone (if permitted by rules).
5. **Evening (`EveningCycle`)**:
   * Dismissed employee's personnel file and true role are revealed to the company.
   * Final words and archive transition.
6. **End Game (`EndGameCycle`)**:
   * Evaluates victory conditions (Corporation, Syndicate, Startup, Outsource, or Independent).
   * Declassifies the full Personnel Register, revealing every player's true role and team.

---

## Roles & Factions Overview

Paranoia Corp includes **21 distinct corporate roles** across 5 factions:

| Role Name (EN / RU) | Faction | Ability & Limits |
| :--- | :--- | :--- |
| **General Director**<br>*(Генеральный Директор)* | 🏢 Corporation | Sole dismissal authority at the Board Meeting. Holds 2 Veto Powers per game. Cannot be killed during Overtime. |
| **Clerk**<br>*(Клерк)* | 🏢 Corporation | Faithful employee. No night duties. Deduces traitors during Board Meetings. |
| **System Administrator**<br>*(Системный Администратор)* | 🏢 Corporation | Protects a colleague's workstation overnight from hostile actions. |
| **Auditor**<br>*(Аудитор)* | 🏢 Corporation | Investigates an employee; receives a delayed forensic report the following day. |
| **Security Specialist**<br>*(Специалист Безопасности)* | 🏢 Corporation | Inspects a target's desk to verify whether they were visited overnight. |
| **HR Manager**<br>*(HR Менеджер)* | 🏢 Corporation | Conducts an emergency interview, blocking the target's overtime action. |
| **Anticrisis Manager**<br>*(Антикризисный Менеджер)* | 🏢 Corporation | Carries limited emergency items; can fire a suspected saboteur directly. |
| **Scrum Master**<br>*(Скрам-Мастер)* | 🏢 Corporation | Swaps access rights between two employees, redirecting overnight targets. |
| **Intern**<br>*(Стажер)* | 🏢 Corporation | Vulnerable corporate intern. Can be coerced or recruited into the Syndicate. |
| **Layoff Candidate**<br>*(Кандидат на Увольнение)* | 🏢 Corporation | If dismissed overnight, converts into an independent Outsourcer. |
| **Hacker**<br>*(Хакер)* | 🕶️ Syndicate | Executes syndicate eliminations overnight against targeted employees. |
| **Spy**<br>*(Шпион)* | 🕶️ Syndicate | Infiltrates corporate departments to uncover employee roles. |
| **Master of Compromising**<br>*(Мастер Компромата)* | 🕶️ Syndicate | Plants fabricated evidence, framing an employee on the Dismissal Shortlist. |
| **Shadow Director**<br>*(Теневой Директор)* | 🕶️ Syndicate | Recruits corporate employees (Interns, Security) into the Syndicate. |
| **Startup Founder**<br>*(Основатель Стартапа)* | 🚀 Startup | Poaches corporate staff with equity offers, converting them to the Startup. |
| **Evangelist**<br>*(Евангелист)* | 🚀 Startup | Inspires an employee to protect them from dismissal for one turn. |
| **Outsourcer**<br>*(Аутсорсер)* | 💼 Outsource | Resurrects a dismissed colleague as a contractor under a B2B agreement. |
| **Alumni Manager**<br>*(Alumni Менеджер)* | 💼 Outsource | Maintains relations with former staff; accesses Outsource and Dismissed chats. |
| **Political Strategist**<br>*(Политтехнолог)* | ⚖️ Independent | Controls an employee overnight, forcing them to target another player. |
| **Raider**<br>*(Рейдер)* | ⚖️ Independent | Hostile takeover specialist. Wins when all other executive factions fall. |
| **Shareholder**<br>*(Акционер)* | 🏢 Corporation | Influential stakeholder with private overnight shareholder communication. |

---

## Local Development Quick Start

### Prerequisites
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed.

### 1. Clone & Build
```bash
git clone https://github.com/Kakainchik/MafiaCardEngine.git
cd MafiaCardEngine
dotnet build
```

### 2. Run the WebServer
```bash
dotnet run --project WebServer
```

By default, the server listens on **`http://localhost:8080`**.

### 3. Open the Web Client
Open your browser and navigate to:
```
http://localhost:8080
```
* Click **Guest Login** (or enter any display name) to instantly enter the Corporate Directory.
* Click **Convene New Boardroom** to create a lobby as Host.
* To test multiplayer locally, open another browser window (or Incognito mode) at `http://localhost:8080`, log in as a second guest, and click **Join Room**.

---

## VPS Production Deployment

Deploying Paranoia Corp on a Linux Virtual Private Server (Ubuntu 22.04 / 24.04 or Debian 12) takes only a few minutes.

### Method 1: Docker & Docker Compose (Recommended)

This is the fastest, cleanest deployment method. It packages .NET 9, SQLite database persistence, and optional automatic SSL.

#### Step 1: Install Docker & Docker Compose on your VPS
```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com | sudo sh

# Add your user to the docker group
sudo usermod -aG docker $USER
newgrp docker

# Verify installation
docker --version
docker compose version
```

#### Step 2: Clone the Repository
```bash
git clone https://github.com/Kakainchik/MafiaCardEngine.git /opt/paranoiacorp
cd /opt/paranoiacorp
```

#### Step 3: Configure Environment Variables
You can customize the secret key and domain by creating a `.env` file or editing `docker-compose.yml`:
```bash
cat << 'EOF' > .env
JWT_KEY=YourSuperConfidentialProductionKeyWithAtLeast32Characters2026!
ASPNETCORE_ENVIRONMENT=Production
EOF
```

#### Step 4: Build and Launch
```bash
docker compose up -d --build
```

The server is now live and listening on port **8080**!
Check the logs with:
```bash
docker compose logs -f paranoiacorp-server
```

---

### Method 2: Linux systemd Service + Nginx Reverse Proxy

If you prefer running .NET 9 directly on the host machine without Docker:

#### Step 1: Install .NET 9 Runtime on Ubuntu / Debian
```bash
# Add Microsoft package repository
sudo apt install -y wget apt-transport-https
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install ASP.NET Core 9.0 Runtime and .NET 9 SDK
sudo apt update
sudo apt install -y aspnetcore-runtime-9.0 dotnet-sdk-9.0
```

#### Step 2: Publish the WebServer
```bash
cd /opt/paranoiacorp
dotnet publish WebServer/WebServer.csproj -c Release -o /var/www/paranoiacorp /p:UseAppHost=false
```

#### Step 3: Create a Dedicated System User
```bash
sudo useradd -r -s /bin/false -d /var/www/paranoiacorp paranoiacorp
sudo chown -R paranoiacorp:paranoiacorp /var/www/paranoiacorp
```

#### Step 4: Create a systemd Service
Create `/etc/systemd/system/paranoiacorp.service`:
```ini
[Unit]
Description=Paranoia Corp Game Server & Intranet Portal
After=network.target

[Service]
WorkingDirectory=/var/www/paranoiacorp
ExecStart=/usr/bin/dotnet /var/www/paranoiacorp/WebServer.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=paranoiacorp
User=paranoiacorp
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:8080
Environment=JWT_KEY=ProductionConfidentialSecuritySecretKey2026MustBeLong!
Environment=ConnectionStrings__DefaultConnection=Data Source=/var/www/paranoiacorp/paranoiacorp.db

[Install]
WantedBy=multi-user.target
```

Enable and start the service:
```bash
sudo systemctl daemon-reload
sudo systemctl enable --now paranoiacorp
sudo systemctl status paranoiacorp
```

#### Step 5: Configure Nginx with WebSocket Support
Install Nginx and Certbot:
```bash
sudo apt install -y nginx certbot python3-certbot-nginx
```

Create `/etc/nginx/sites-available/paranoiacorp`:
```nginx
server {
    server_name paranoia.yourdomain.com; # Replace with your domain or VPS IP

    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_http_version 1.1;

        # WebSocket support (Required for SignalR /hub/lobby)
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";

        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        # Keep WebSockets alive without premature timeouts
        proxy_read_timeout 86400s;
        proxy_send_timeout 86400s;
        proxy_buffering off;
        proxy_cache off;
    }
}
```

Enable the configuration and reload Nginx:
```bash
sudo ln -s /etc/nginx/sites-available/paranoiacorp /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

Obtain a free Let's Encrypt SSL certificate:
```bash
sudo certbot --nginx -d paranoia.yourdomain.com
```

---

### Method 3: Turnkey HTTPS with Caddy

If using Docker, you can get automatic SSL with zero certificate configuration using Caddy.

1. Edit `Caddyfile` in the repository root:
```caddy
paranoia.yourdomain.com {
    reverse_proxy paranoiacorp-server:8080
}
```
2. Start the stack with the `proxy` profile:
```bash
docker compose --profile proxy up -d
```
Caddy will automatically obtain and renew Let's Encrypt certificates for your domain on ports 80 and 443.

---

## Database Options

The server dynamically initializes its database on startup:

### 1. SQLite (Default / Zero-Config)
No external database service is required. By default, the application creates a `paranoiacorp.db` file in the working directory (or `/app/data/paranoiacorp.db` in Docker).

### 2. PostgreSQL
To use an external managed PostgreSQL database or a Docker PostgreSQL service, configure the connection string via environment variable:
```bash
ConnectionStrings__DefaultConnection=Host=localhost;Database=paranoiadb;Username=postgres;Password=mysecretpassword
```
or via standard `DATABASE_URL`:
```bash
DATABASE_URL=postgres://postgres:mysecretpassword@localhost:5432/paranoiadb
```
The server will automatically detect PostgreSQL and configure Npgsql accordingly.

---

## Playing the Game: Host & Player Walkthrough

### 1. Signing In
* Navigate to the server URL.
* Under **Sign In / Guest Access**, enter your desired display name and click **Instant Guest Sign In**.
* Or use **Register New Account** to keep your employee record permanently.

### 2. Room Directory & Lobby Setup (Host)
* In the Boardroom Directory, click **Convene New Boardroom**.
* Set the boardroom name (e.g., *«Executive Boardroom A»*), description, and capacity.
* Share the Room ID or invite code with your players.
* In the lobby, click **Manage Role Pool**:
  * Ensure exactly **1 General Director** is seated.
  * Configure roles matching the exact seated headcount.
  * Ensure at least **2 distinct factions** are represented in the pool.
* When all employees click **Confirm Ready**, the Host clicks **Convene Board & Start Game**.

### 3. Joining as a Player
* Other players sign in on their phones or laptops.
* In the Boardroom Directory, they will see the available room. Click **Take Seat / Join**.
* Click **Confirm Ready** when prepared.

### 4. Navigating the Corporate Noir Interface
* **Dossier (Top Right)**: View your confidential personnel file, true faction, role silhouette portrait, and ability rules at any time.
* **Employee Directory (Top Bar)**: Browse all 21 corporate roles to study suspect profiles.
* **Language Switcher (EN / RU)**: Switch interface language on the fly between English and Russian.
* **Active Directive Panel**: Center screen displays the current phase requirements (Overtime target selection, Morning wire announcements, or Boardroom dismissal votes).
* **Audience-Labeled Communications (Right Side)**: Chat tabs dynamically unlock based on your current role and phase permissions (e.g. Syndicate Encrypted wire, Outsource Network, or Boardroom Public discussion).

---

## SignalR WebSockets Configuration

Real-time synchronization uses ASP.NET Core SignalR at endpoint **`/hub/lobby`**.

* **Event Broadcasts**:
  * `ParanoiaStateChanged`: Dispatched when the game advances through any phase (Intro -> Overtime -> Morning -> Board Meeting -> Evening -> End Game).
  * `ParanoiaChatUpdated`: Dispatched when a new message is received in an authorized scoped channel.
  * `ParanoiaMorningBrief`: Dispatched when overnight news and delayed auditor findings are published.
* **Fallback Polling**: If WebSockets are blocked by aggressive corporate proxies, the client automatically falls back to HTTP Long Polling / polling intervals without disrupting gameplay.

---

## Troubleshooting & FAQ

#### Q: How many players are needed to start a game?
The engine enforces a statutory minimum of **2 players** (e.g. 1 General Director + 1 Opposing Faction employee) for testing, though games of **5 to 12 players** provide the richest social deduction intrigue.

#### Q: The game won't let the Host start. What is missing?
The lobby status indicator will display the exact statutory requirement:
1. Every seated employee must have clicked **Confirm Ready**.
2. Total configured roles in the Role Pool must equal the number of seated players.
3. Exactly **1 General Director** must be configured.
4. At least **2 different factions** must be included.

#### Q: Do mobile players need to install an app?
No. The web client is fully responsive and optimized for mobile browsers (iOS Safari, Android Chrome/Firefox) with dedicated bottom navigation tabs for Roster, Workspace, and Channels.

#### Q: Can I run both the Discord Bot and Web Server simultaneously?
Yes. The Discord bot and ASP.NET Core WebServer both share the underlying `GameLogic.dll` library.

---

## License & Credits

Built with .NET 9, ASP.NET Core, EF Core, and SignalR.
Mid-century corporate noir aesthetic and vector silhouette art created for Paranoia Corp.
All rights reserved.
