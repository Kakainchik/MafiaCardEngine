// Paranoia Corp - Ink-and-Gouache Role Portraits with Venetian Blinds Shadows
// Mid-Century Corporate Noir Silhouette Art

const RolePortraits = {
  // Common Venetian Blinds Shadow Pattern
  getBlindsOverlay() {
    return `
      <g opacity="0.32" fill="#0b1116">
        <polygon points="0,0 260,30 260,52 0,22" />
        <polygon points="0,50 260,80 260,102 0,72" />
        <polygon points="0,100 260,130 260,152 0,122" />
        <polygon points="0,150 260,180 260,202 0,172" />
        <polygon points="0,200 260,230 260,252 0,222" />
        <polygon points="0,250 260,280 260,302 0,272" />
      </g>
    `;
  },

  getPortraitSvg(roleKey, factionColor = '#0d9488') {
    const generator = this[roleKey] || this.DEFAULT;
    return generator(factionColor, this.getBlindsOverlay());
  },

  GENERAL_DIRECTOR(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <defs>
        <radialGradient id="gd-bg" cx="65%" cy="30%" r="75%">
          <stop offset="0%" stop-color="#364954" />
          <stop offset="60%" stop-color="#182329" />
          <stop offset="100%" stop-color="#0e1519" />
        </radialGradient>
      </defs>
      <rect width="240" height="280" fill="url(#gd-bg)" />
      <!-- Angled Window Beam -->
      <polygon points="20,0 240,0 240,240 100,280" fill="#f7f4ec" opacity="0.08" />
      ${blinds}
      <!-- Executive Desk Surface -->
      <polygon points="0,225 240,215 240,280 0,280" fill="#11171b" />
      <line x1="0" y1="225" x2="240" y2="215" stroke="${color}" stroke-width="2" opacity="0.8" />
      <!-- Executive Leather High-back Chair -->
      <path d="M50 85 Q120 45 190 85 L200 240 L40 240 Z" fill="#14191d" />
      <!-- Stern Silhouette Head & Torso -->
      <path d="M85 85 C85 60 155 60 155 85 C155 105 145 118 120 120 C95 118 85 105 85 85 Z" fill="#1d252b" />
      <!-- Sidelight highlight on face profile -->
      <path d="M120 70 L138 72 L142 85 L135 95 L140 105 L130 115 L120 120 Z" fill="#d9d1c1" opacity="0.85" />
      <!-- Tailored Suit & Shoulders -->
      <path d="M48 240 L70 145 L105 135 L120 160 L135 135 L170 145 L192 240 Z" fill="#1b232a" />
      <polygon points="105,135 120,165 135,135" fill="#f5f2ea" />
      <polygon points="116,145 124,145 122,185 118,185" fill="${color}" />
      <!-- Cigar Smoke Wisp -->
      <path d="M145 185 Q170 160 155 135 Q140 110 165 80" stroke="#f5f2ea" stroke-width="2.5" fill="none" opacity="0.35" stroke-linecap="round" />
      <!-- Frame Border -->
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  MASTER_OF_COMPROMISING(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#181f25" />
      <polygon points="0,0 180,0 240,280 60,280" fill="#fff" opacity="0.06" />
      ${blinds}
      <!-- Fedora Hat Silhouette -->
      <path d="M40 105 C70 95 170 95 200 105 C185 85 170 65 120 65 C70 65 55 85 40 105 Z" fill="#11161a" />
      <path d="M65 98 C90 92 150 92 175 98 L168 76 C150 72 90 72 72 76 Z" fill="#1b2329" />
      <rect x="70" y="88" width="100" height="7" fill="${color}" opacity="0.9" />
      <!-- Shadowed Face with Rim Light -->
      <ellipse cx="120" cy="115" rx="32" ry="38" fill="#161c21" />
      <path d="M92 110 Q98 135 120 148 Q108 140 102 125 Z" fill="#e2dac9" opacity="0.75" />
      <!-- Trench Coat Turned-up Collar -->
      <path d="M50 260 L75 160 L100 150 L120 185 L140 150 L165 160 L190 260 Z" fill="#141a1f" />
      <polygon points="75,160 95,140 105,170" fill="#1f272e" />
      <polygon points="165,160 145,140 135,170" fill="#1f272e" />
      <!-- Manila Compromising Folder in Hand -->
      <polygon points="120,205 185,185 210,230 145,250" fill="#d4a359" />
      <text x="148" y="222" font-family="'IBM Plex Mono', monospace" font-size="9" fill="#161c21" font-weight="bold">CONFIDENTIAL</text>
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  SPY(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#141a20" />
      <polygon points="100,0 240,40 180,280 40,280" fill="#fff" opacity="0.05" />
      ${blinds}
      <!-- Leaning in doorway / filing cabinet -->
      <rect x="180" y="20" width="60" height="260" fill="#0f1418" />
      <!-- Figure Silhouette with Turned Up Collar -->
      <ellipse cx="110" cy="88" rx="26" ry="32" fill="#192026" />
      <!-- Sharp rim light on sunglasses / profile -->
      <path d="M100 78 L124 82 L118 94 L98 90 Z" fill="#101518" stroke="#e8e2d5" stroke-width="1.5" />
      <!-- Cigarette glow -->
      <circle cx="122" cy="112" r="3" fill="#e11d48" />
      <path d="M125 112 Q145 95 135 70" stroke="#f5f2ea" stroke-width="1.5" fill="none" opacity="0.4" />
      <path d="M50 260 L80 140 L105 125 L125 155 L150 135 L175 260 Z" fill="#101518" />
      <polygon points="80,140 105,120 115,160" fill="#222b33" />
      <!-- Tape recorder reel in hand -->
      <circle cx="95" cy="225" r="14" fill="#2d3748" />
      <circle cx="95" cy="225" r="5" fill="#f5f2ea" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  HACKER(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#12171c" />
      <!-- Mainframe Rack in Background -->
      <rect x="25" y="20" width="70" height="200" fill="#1c232a" stroke="#2b3640" stroke-width="1" />
      <circle cx="45" cy="45" r="12" fill="#11161a" stroke="#4a5568" stroke-width="2" />
      <circle cx="75" cy="45" r="12" fill="#11161a" stroke="#4a5568" stroke-width="2" />
      <line x1="30" y1="80" x2="90" y2="80" stroke="#b91c1c" stroke-width="2" opacity="0.8" />
      <line x1="30" y1="95" x2="90" y2="95" stroke="${color}" stroke-width="2" opacity="0.8" />
      ${blinds}
      <!-- Operator Silhouette at Teletype / Switchboard -->
      <ellipse cx="145" cy="110" rx="30" ry="36" fill="#1b232a" />
      <!-- Sidelight from glowing vacuum tubes -->
      <path d="M125 90 L138 90 L145 105 L135 125 L125 135 Z" fill="#d97706" opacity="0.7" />
      <path d="M90 260 L120 165 L145 155 L180 170 L210 260 Z" fill="#141a20" />
      <!-- Switchboard patching cables -->
      <path d="M60 120 Q110 180 145 220" stroke="#f59e0b" stroke-width="2.5" fill="none" />
      <path d="M80 140 Q130 190 170 215" stroke="#ef4444" stroke-width="2.5" fill="none" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  SHADOW_DIRECTOR(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#0f1418" />
      <polygon points="120,0 240,0 240,240" fill="#fff" opacity="0.04" />
      ${blinds}
      <!-- High back leather chair seen from three-quarter rear -->
      <path d="M60 40 Q120 25 180 40 L195 240 L45 240 Z" fill="#171e24" />
      <!-- Heavy silhouette head partially turned -->
      <ellipse cx="120" cy="95" rx="32" ry="38" fill="#101418" />
      <path d="M142 80 C155 95 152 115 140 125" stroke="#e2dac9" stroke-width="2" fill="none" opacity="0.8" />
      <!-- Arm resting on desk with heavy signet ring -->
      <rect x="30" y="225" width="180" height="55" fill="#141a20" />
      <!-- Rotary Desk Phone Silhouette -->
      <rect x="50" y="195" width="45" height="30" rx="4" fill="#0a0d10" />
      <ellipse cx="72" cy="190" rx="22" ry="8" fill="#1c242b" />
      <!-- Hand with ring -->
      <circle cx="160" cy="225" r="7" fill="#f59e0b" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  CLERK(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#1c2227" />
      <polygon points="0,0 160,0 240,280 80,280" fill="#f7f4ec" opacity="0.08" />
      ${blinds}
      <!-- Stack of Paper Folders on Desk -->
      <rect x="160" y="150" width="60" height="15" fill="#e2dac9" />
      <rect x="155" y="165" width="65" height="15" fill="#d5cbba" />
      <rect x="150" y="180" width="70" height="15" fill="#c4b8a4" />
      <rect x="145" y="195" width="75" height="15" fill="#b0a28d" />
      <!-- Clerk Hunched Silhouette with Green Eyeshade Visor -->
      <ellipse cx="105" cy="105" rx="28" ry="32" fill="#151b20" />
      <!-- Green Eyeshade Visor -->
      <path d="M80 92 Q115 80 145 92 L140 102 Q115 95 85 102 Z" fill="#15803d" opacity="0.9" />
      <ellipse cx="102" cy="112" rx="7" ry="7" fill="none" stroke="#e2dac9" stroke-width="1.5" />
      <!-- Shirt and sleeve garters -->
      <path d="M50 260 L75 160 L105 145 L135 160 L160 260 Z" fill="#e6ded0" />
      <rect x="68" y="180" width="14" height="6" fill="#1b232a" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  SECURITY_SPECIALIST(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#13181d" />
      <polygon points="40,0 200,0 240,280 80,280" fill="#fff" opacity="0.06" />
      ${blinds}
      <!-- Broad-shouldered Security Agent Silhouette -->
      <ellipse cx="120" cy="85" rx="30" ry="36" fill="#192026" />
      <!-- Harsh Sidelight on jaw and earphone -->
      <path d="M98 75 L115 72 L125 85 L120 105 L108 115 Z" fill="#e2dac9" opacity="0.8" />
      <!-- Surveillance coiled earpiece wire -->
      <path d="M138 88 Q150 95 142 105 Q152 115 144 125 L140 150" stroke="#f5f2ea" stroke-width="2" fill="none" opacity="0.8" />
      <!-- Dark Suit and Security Badge -->
      <path d="M40 260 L70 140 L105 130 L120 160 L135 130 L170 140 L200 260 Z" fill="#11161b" />
      <!-- Enamel Corporate Security Shield Badge -->
      <path d="M78 175 L92 175 L92 190 L85 198 L78 190 Z" fill="#0d9488" stroke="#f5f2ea" stroke-width="1" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  AUDITOR(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#171c21" />
      <!-- Cone of Desk Lamp Light -->
      <polygon points="120,40 20,260 220,260" fill="#fef08a" opacity="0.08" />
      ${blinds}
      <!-- Auditor Head in Wire-rim Glasses with Ledger -->
      <ellipse cx="120" cy="95" rx="27" ry="33" fill="#1b2228" />
      <!-- Wire Glasses Highlight -->
      <circle cx="110" cy="95" r="7" fill="none" stroke="#fef08a" stroke-width="1.5" />
      <circle cx="128" cy="95" r="7" fill="none" stroke="#fef08a" stroke-width="1.5" />
      <line x1="117" y1="95" x2="121" y2="95" stroke="#fef08a" stroke-width="1.5" />
      <!-- Holding Thick Financial Ledger / Stamp -->
      <polygon points="60,200 180,180 195,255 75,275" fill="#f4ede1" />
      <line x1="75" y1="215" x2="185" y2="198" stroke="#b91c1c" stroke-width="1.5" />
      <line x1="78" y1="230" x2="188" y2="213" stroke="#2b3236" stroke-width="1" />
      <line x1="81" y1="245" x2="191" y2="228" stroke="#2b3236" stroke-width="1" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  HR_MANAGER(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#191e23" />
      <polygon points="30,0 210,0 240,280 60,280" fill="#fff" opacity="0.05" />
      ${blinds}
      <!-- Woman with structured 1950s roll hairstyle silhouette -->
      <path d="M85 95 C80 60 155 60 155 95 C165 95 160 130 145 130 C130 135 110 135 95 130 C80 130 75 105 85 95 Z" fill="#12171b" />
      <!-- Sharp red lipstick and pearl earring highlight -->
      <circle cx="95" cy="105" r="3" fill="#f8fafc" />
      <path d="M102 118 Q112 122 120 118" stroke="#e11d48" stroke-width="2" fill="none" />
      <!-- Tailored skirt suit with pen in hand -->
      <path d="M55 260 L80 155 L105 140 L120 165 L135 140 L160 155 L185 260 Z" fill="#1c252d" />
      <!-- Red editing pencil -->
      <line x1="140" y1="180" x2="175" y2="145" stroke="#ef4444" stroke-width="3" stroke-linecap="round" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  SYSTEM_ADMINISTRATOR(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#14181d" />
      <polygon points="0,0 120,0 240,280 120,280" fill="#38bdf8" opacity="0.05" />
      ${blinds}
      <!-- Server Rack with Blinking Lights -->
      <rect x="150" y="20" width="80" height="240" fill="#0f1317" stroke="#1f2937" stroke-width="1" />
      <circle cx="170" cy="50" r="3" fill="#22c55e" />
      <circle cx="185" cy="50" r="3" fill="#ef4444" />
      <circle cx="200" cy="50" r="3" fill="#eab308" />
      <circle cx="170" cy="80" r="3" fill="#22c55e" />
      <circle cx="185" cy="80" r="3" fill="#22c55e" />
      <!-- SysAdmin Silhouette with Screwdriver and Key Ring -->
      <ellipse cx="95" cy="95" rx="28" ry="34" fill="#1a2128" />
      <!-- Glasses rim reflection -->
      <rect x="80" y="90" width="14" height="9" rx="2" fill="none" stroke="#38bdf8" stroke-width="1.5" />
      <rect x="100" y="90" width="14" height="9" rx="2" fill="none" stroke="#38bdf8" stroke-width="1.5" />
      <!-- Big Brass Master Key Ring at belt -->
      <circle cx="65" cy="235" r="9" fill="none" stroke="#f59e0b" stroke-width="2.5" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  ANTICRISIS_MANAGER(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#181d22" />
      <polygon points="60,0 240,0 180,280 0,280" fill="#fff" opacity="0.06" />
      ${blinds}
      <!-- Slick Fixer Silhouette with Loosened Tie and Rolled Sleeves -->
      <ellipse cx="120" cy="85" rx="28" ry="34" fill="#12171b" />
      <!-- Sidelight on sharp jawline -->
      <path d="M102 75 L120 70 L135 85 L128 105 L112 118 Z" fill="#e2dac9" opacity="0.85" />
      <!-- Shirt with rolled sleeves & loosened tie -->
      <path d="M50 260 L75 145 L105 130 L120 160 L135 130 L165 145 L190 260 Z" fill="#f8fafc" />
      <!-- Loosened Crimson Tie -->
      <polygon points="116,145 124,145 130,210 114,210" fill="#b91c1c" />
      <!-- Red Ink Rubber Stamp in Hand -->
      <rect x="145" y="185" width="26" height="36" rx="3" fill="#b91c1c" />
      <rect x="153" y="170" width="10" height="18" fill="#78350f" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  INTERN(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#1b2025" />
      <polygon points="20,0 180,0 240,280 80,280" fill="#fff" opacity="0.06" />
      ${blinds}
      <!-- Nervous Young Employee Silhouette with Big Pile of Folders -->
      <ellipse cx="120" cy="80" rx="24" ry="30" fill="#14191d" />
      <!-- Oversized collar and timid profile -->
      <path d="M108 72 L124 72 L128 85 L122 96 L112 104 Z" fill="#e2dac9" opacity="0.8" />
      <!-- Big carton box / overflowing manila files in arms -->
      <rect x="70" y="140" width="100" height="75" fill="#ca8a04" opacity="0.85" />
      <polygon points="65,140 115,120 175,140" fill="#d97706" />
      <rect x="80" y="125" width="20" height="35" fill="#f8fafc" />
      <rect x="110" y="120" width="25" height="40" fill="#fef08a" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  SCRUM_MASTER(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#161b20" />
      <!-- Agile Board / Sprint Columns in Background -->
      <line x1="30" y1="40" x2="210" y2="40" stroke="#334155" stroke-width="1" />
      <line x1="90" y1="40" x2="90" y2="200" stroke="#334155" stroke-width="1" stroke-dasharray="4,4" />
      <line x1="150" y1="40" x2="150" y2="200" stroke="#334155" stroke-width="1" stroke-dasharray="4,4" />
      <rect x="35" y="55" width="22" height="16" fill="#facc15" opacity="0.7" />
      <rect x="95" y="70" width="22" height="16" fill="#38bdf8" opacity="0.7" />
      <rect x="155" y="55" width="22" height="16" fill="#4ade80" opacity="0.7" />
      ${blinds}
      <!-- Silhouette holding stopwatch and pointer -->
      <ellipse cx="115" cy="95" rx="26" ry="32" fill="#1b2229" />
      <!-- Sidelight -->
      <path d="M100 85 L118 82 L128 95 L122 110 L108 120 Z" fill="#e2dac9" opacity="0.8" />
      <!-- Stopwatch in hand -->
      <circle cx="165" cy="180" r="14" fill="#0f172a" stroke="#f5f2ea" stroke-width="2" />
      <line x1="165" y1="180" x2="165" y2="172" stroke="#ef4444" stroke-width="2" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  SHAREHOLDER(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#14181c" />
      <polygon points="100,0 240,0 240,260 80,260" fill="#f59e0b" opacity="0.05" />
      ${blinds}
      <!-- Distinguished Executive Silhouette with Gold Pocket Watch Chain -->
      <ellipse cx="120" cy="85" rx="29" ry="35" fill="#11161a" />
      <path d="M105 75 L125 72 L136 85 L130 102 L116 114 Z" fill="#d4af37" opacity="0.6" />
      <!-- Pinstripe Vest -->
      <path d="M50 260 L75 140 L105 130 L120 160 L135 130 L165 140 L190 260 Z" fill="#1a2228" />
      <!-- Gold Watch Chain swooping across vest -->
      <path d="M95 190 Q120 215 140 195" stroke="#f59e0b" stroke-width="2.5" fill="none" />
      <!-- Share certificates in hand -->
      <polygon points="140,210 195,190 210,240 155,260" fill="#fdfbf7" stroke="#b45309" stroke-width="1.5" />
      <text x="156" y="228" font-family="'IBM Plex Mono', monospace" font-size="8" fill="#1e293b" font-weight="bold">SHARES</text>
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  LAYOFF_CANDIDATE(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#1a1e22" />
      <polygon points="0,0 140,0 200,280 60,280" fill="#fff" opacity="0.05" />
      ${blinds}
      <!-- Anxious Employee Holding Belongings Box -->
      <ellipse cx="120" cy="85" rx="27" ry="33" fill="#13171b" />
      <path d="M102 75 L120 72 L128 85 L122 102 L110 114 Z" fill="#e2dac9" opacity="0.75" />
      <!-- Cardboard box of desk items (potted cactus, mug) -->
      <rect x="65" y="160" width="110" height="80" fill="#a16207" opacity="0.9" />
      <ellipse cx="85" cy="150" rx="8" ry="12" fill="#15803d" />
      <rect x="135" y="145" width="18" height="20" rx="2" fill="#f8fafc" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  STARUP_FOUNDER(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#131915" />
      <!-- Disruptive Exponential Graph on Blackboard in Background -->
      <path d="M30 180 Q100 170 140 110 T210 30" stroke="${color}" stroke-width="2.5" fill="none" opacity="0.6" />
      ${blinds}
      <!-- Visionary in Knit Turtleneck Pacing -->
      <ellipse cx="115" cy="85" rx="27" ry="33" fill="#0d1410" />
      <!-- Sharp rim light on profile and glasses -->
      <path d="M100 75 L118 72 L128 85 L124 105 L112 116 Z" fill="#d9f99d" opacity="0.8" />
      <circle cx="115" cy="85" r="8" fill="none" stroke="#d9f99d" stroke-width="1.5" />
      <!-- Black Turtleneck Collar -->
      <rect x="100" y="115" width="30" height="22" rx="4" fill="#090e0b" />
      <path d="M55 260 L80 140 L150 140 L175 260 Z" fill="#090e0b" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  EVANGELIST(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#151b17" />
      <polygon points="50,0 210,0 240,280 80,280" fill="#a3e635" opacity="0.05" />
      ${blinds}
      <!-- Charismatic Speaker with Raised Hand & Brochure -->
      <ellipse cx="115" cy="85" rx="27" ry="33" fill="#101712" />
      <path d="M100 75 L118 70 L130 85 L124 102 L110 114 Z" fill="#d9f99d" opacity="0.75" />
      <path d="M50 260 L75 145 L105 130 L120 160 L135 130 L165 145 L190 260 Z" fill="#1a251e" />
      <!-- Startup Pamphlet in hand -->
      <polygon points="145,170 190,150 205,200 160,220" fill="#84cc16" />
      <text x="156" y="188" font-family="'IBM Plex Mono', monospace" font-size="8" fill="#0f172a" font-weight="bold">MANIFESTO</text>
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  RAIDER(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#18141c" />
      <polygon points="120,0 240,0 160,280 40,280" fill="#c084fc" opacity="0.05" />
      ${blinds}
      <!-- Predatory Corporate Raider Silhouette with Sharp Silhouette -->
      <ellipse cx="120" cy="85" rx="28" ry="35" fill="#130e17" />
      <!-- Sidelight on sharp jawline -->
      <path d="M102 75 L122 70 L134 85 L126 105 L112 118 Z" fill="#e9d5ff" opacity="0.8" />
      <!-- Double-breasted sharp pinstripe suit -->
      <path d="M40 260 L70 140 L105 125 L120 160 L135 125 L170 140 L200 260 Z" fill="#1f1625" />
      <!-- Steel Briefcase -->
      <rect x="45" y="210" width="55" height="45" rx="4" fill="#64748b" />
      <rect x="62" y="200" width="20" height="12" fill="none" stroke="#64748b" stroke-width="2.5" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  POLITICAL_STRATEGIST(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#18151c" />
      <polygon points="0,0 140,0 220,280 80,280" fill="#a855f7" opacity="0.05" />
      ${blinds}
      <!-- Whispering on Rotary Phone in shadowy office alcove -->
      <ellipse cx="120" cy="85" rx="27" ry="33" fill="#130f17" />
      <path d="M104 75 L122 72 L132 85 L126 104 L114 116 Z" fill="#f3e8ff" opacity="0.8" />
      <!-- Hand holding telephone receiver to ear -->
      <path d="M135 75 Q150 95 135 115 L125 110 Q138 95 125 80 Z" fill="#0a070c" />
      <path d="M135 115 Q145 160 115 190" stroke="#a855f7" stroke-width="2" fill="none" />
      <path d="M50 260 L75 145 L105 135 L120 165 L135 135 L165 145 L190 260 Z" fill="#1a1320" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  OUTSOURCER(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#191714" />
      <polygon points="30,0 190,0 240,280 80,280" fill="#d97706" opacity="0.05" />
      ${blinds}
      <!-- External Subcontractor Silhouette with Visitor Badge -->
      <ellipse cx="115" cy="85" rx="28" ry="34" fill="#14110e" />
      <path d="M98 75 L118 72 L128 85 L122 105 L108 116 Z" fill="#fed7aa" opacity="0.8" />
      <path d="M45 260 L75 145 L105 135 L120 165 L135 135 L165 145 L195 260 Z" fill="#1f1b16" />
      <!-- VISITOR Lanyard & Badge -->
      <path d="M105 135 L120 175 L135 135" stroke="#ea580c" stroke-width="2" fill="none" />
      <rect x="110" y="175" width="20" height="26" rx="2" fill="#fff" stroke="#ea580c" stroke-width="1" />
      <text x="113" y="190" font-family="'IBM Plex Mono', monospace" font-size="6" fill="#ea580c" font-weight="bold">GUEST</text>
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  ALUMNI_MANAGER(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#181613" />
      <polygon points="60,0 220,0 180,280 20,280" fill="#f59e0b" opacity="0.05" />
      ${blinds}
      <!-- Looking out of rain-streaked blind-shadowed window -->
      <ellipse cx="120" cy="85" rx="27" ry="33" fill="#13100d" />
      <path d="M102 75 L122 72 L132 85 L126 105 L112 116 Z" fill="#fed7aa" opacity="0.8" />
      <path d="M50 260 L75 145 L105 135 L120 165 L135 135 L165 145 L190 260 Z" fill="#1c1813" />
      <!-- File of departed alumni in hand -->
      <polygon points="65,190 120,180 130,230 75,240" fill="#fef3c7" stroke="#b45309" stroke-width="1.5" />
      <text x="73" y="210" font-family="'IBM Plex Mono', monospace" font-size="7" fill="#78350f" font-weight="bold">ALUMNI</text>
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  },

  DEFAULT(color, blinds) {
    return `
    <svg viewBox="0 0 240 280" class="portrait-svg" xmlns="http://www.w3.org/2000/svg">
      <rect width="240" height="280" fill="#15191d" />
      ${blinds}
      <ellipse cx="120" cy="90" rx="30" ry="36" fill="#1b232a" />
      <path d="M102 80 L122 75 L134 90 L126 110 L112 120 Z" fill="#e2dac9" opacity="0.8" />
      <path d="M50 260 L75 145 L105 135 L120 165 L135 135 L165 145 L190 260 Z" fill="#131a21" />
      <rect x="1" y="1" width="238" height="278" fill="none" stroke="${color}" stroke-width="1.5" opacity="0.6" />
    </svg>`;
  }
};
