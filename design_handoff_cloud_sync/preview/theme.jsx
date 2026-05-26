// Shared design tokens, window chrome, and provider primitives
// for AStar Dev Cloud Sync layout options.

const TOKENS = {
  light: {
    bg: '#f5f3ef',          // canvas / window bg (warm white)
    surface: '#ffffff',     // raised cards/panes
    surfaceAlt: '#faf8f4',  // panel inset
    border: 'rgba(30,24,16,0.10)',
    borderStrong: 'rgba(30,24,16,0.18)',
    ink: '#15120e',         // primary text
    ink2: '#3a342c',        // secondary
    ink3: '#7a7268',        // tertiary
    inkInverse: '#fefdfb',
    primary: 'oklch(0.52 0.20 268)',     // confident indigo
    primaryWeak: 'oklch(0.96 0.03 268)',
    primaryInk: '#1c1850',
    accent: 'oklch(0.78 0.16 75)',       // warm saffron
    accentInk: '#5a3a05',
    good: 'oklch(0.62 0.16 152)',
    warn: 'oklch(0.74 0.16 60)',
    danger: 'oklch(0.58 0.20 25)',
    chrome: '#eae6df',
    chromeInk: '#2a251f',
  },
  dark: {
    bg: '#0e0c0a',
    surface: '#1a1714',
    surfaceAlt: '#141210',
    border: 'rgba(255,250,240,0.08)',
    borderStrong: 'rgba(255,250,240,0.16)',
    ink: '#f5f1ea',
    ink2: '#bdb6ab',
    ink3: '#827a6f',
    inkInverse: '#0e0c0a',
    primary: 'oklch(0.72 0.18 268)',
    primaryWeak: 'oklch(0.28 0.10 268)',
    primaryInk: '#e6e2ff',
    accent: 'oklch(0.82 0.15 75)',
    accentInk: '#2a1a02',
    good: 'oklch(0.72 0.16 152)',
    warn: 'oklch(0.80 0.16 60)',
    danger: 'oklch(0.70 0.20 25)',
    chrome: '#08070680',
    chromeInk: '#d8d2c6',
  },
};

const FONT_UI = "'Plus Jakarta Sans', system-ui, -apple-system, sans-serif";
const FONT_MONO = "'JetBrains Mono', 'SF Mono', Consolas, monospace";

// ─── Provider primitives ─────────────────────────────────────
// Generic geometric marks (NOT actual brand logos). Each provider gets
// its own accent hue so users can distinguish accounts at a glance,
// but the marks are abstract.
const PROVIDERS = {
  onedrive: {
    name: 'OneDrive',
    initial: 'O',
    hue: 'oklch(0.55 0.18 250)',  // cool blue-violet
    hueWeak: 'oklch(0.94 0.04 250)',
    hueDarkWeak: 'oklch(0.28 0.08 250)',
  },
  gdrive: {
    name: 'Google Drive',
    initial: 'G',
    hue: 'oklch(0.70 0.18 145)',  // green
    hueWeak: 'oklch(0.94 0.04 145)',
    hueDarkWeak: 'oklch(0.28 0.08 145)',
  },
  dropbox: {
    name: 'Dropbox',
    initial: 'D',
    hue: 'oklch(0.62 0.18 220)',  // teal-blue
    hueWeak: 'oklch(0.94 0.04 220)',
    hueDarkWeak: 'oklch(0.28 0.08 220)',
  },
};

function ProviderMark({ kind, size = 28, theme, bare = false }) {
  const p = PROVIDERS[kind];
  if (!p) return null;
  const t = TOKENS[theme];
  const weak = theme === 'dark' ? p.hueDarkWeak : p.hueWeak;
  // Geometric mark: rounded square with provider's initial, accent slash.
  return (
    <div style={{
      width: size, height: size, borderRadius: size * 0.26,
      background: bare ? 'transparent' : weak,
      color: p.hue,
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      fontFamily: FONT_UI, fontWeight: 700,
      fontSize: size * 0.50, lineHeight: 1,
      position: 'relative', flexShrink: 0,
      border: bare ? `1.5px solid ${p.hue}` : 'none',
    }}>
      <span style={{ position: 'relative', zIndex: 1 }}>{p.initial}</span>
      <div style={{
        position: 'absolute', right: size * 0.14, top: size * 0.14,
        width: size * 0.18, height: size * 0.18, borderRadius: '50%',
        background: p.hue,
      }} />
    </div>
  );
}

// ─── AStar logo ──────────────────────────────────────────────
function AStarLogo({ size = 22, theme }) {
  const t = TOKENS[theme];
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
      <svg width={size} height={size} viewBox="0 0 24 24" fill="none">
        <path d="M12 2 L21 22 L12 17 L3 22 Z" fill={t.primary} />
        <path d="M12 2 L12 17" stroke={t.bg} strokeWidth="1.2" />
        <circle cx="12" cy="6.5" r="1.6" fill={t.accent} />
      </svg>
    </div>
  );
}

// ─── Generic Linux window chrome ─────────────────────────────
function WindowChrome({ theme, children, title, accessory }) {
  const t = TOKENS[theme];
  return (
    <div style={{
      width: '100%', height: '100%',
      background: t.bg, color: t.ink,
      fontFamily: FONT_UI,
      display: 'flex', flexDirection: 'column',
      overflow: 'hidden',
    }}>
      {/* Titlebar */}
      <div style={{
        height: 38, flexShrink: 0,
        background: t.chrome, color: t.chromeInk,
        borderBottom: `1px solid ${t.border}`,
        display: 'flex', alignItems: 'center',
        padding: '0 12px', gap: 12,
        fontSize: 12, fontWeight: 500,
        userSelect: 'none',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 8, opacity: 0.85 }}>
          <AStarLogo size={14} theme={theme} />
          <span style={{ letterSpacing: 0.1 }}>{title || 'AStar Dev Cloud Sync'}</span>
        </div>
        <div style={{ flex: 1, display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 8, fontSize: 11, color: t.ink3 }}>
          {accessory}
        </div>
        <div style={{ display: 'flex', gap: 4 }}>
          {['—', '▢', '✕'].map((g, i) => (
            <div key={i} style={{
              width: 22, height: 22, borderRadius: 11,
              background: i === 2 ? 'transparent' : 'transparent',
              border: `1px solid ${t.borderStrong}`,
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontSize: 9, color: t.ink2,
            }}>{g}</div>
          ))}
        </div>
      </div>
      {/* Content */}
      <div style={{ flex: 1, overflow: 'hidden', position: 'relative' }}>
        {children}
      </div>
    </div>
  );
}

// ─── Inline glyphs (single-purpose SVGs) ────────────────────
const Icon = ({ d, size = 14, color = 'currentColor', strokeWidth = 1.7, fill = 'none' }) => (
  <svg width={size} height={size} viewBox="0 0 16 16" fill={fill} stroke={color} strokeWidth={strokeWidth} strokeLinecap="round" strokeLinejoin="round" style={{ flexShrink: 0 }}>
    <path d={d} />
  </svg>
);

const ICONS = {
  folder:   'M2 4 a1 1 0 0 1 1-1 h3 l1.5 1.5 H13 a1 1 0 0 1 1 1 V12 a1 1 0 0 1 -1 1 H3 a1 1 0 0 1 -1 -1 z',
  file:     'M4 2 h5 l3 3 v8 a1 1 0 0 1 -1 1 H4 a1 1 0 0 1 -1 -1 V3 a1 1 0 0 1 1 -1 z M9 2 v3 h3',
  check:    'M3 8 L7 12 L13 4',
  pause:    'M5 3 v10 M11 3 v10',
  play:     'M5 3 L13 8 L5 13 z',
  plus:     'M8 3 v10 M3 8 h10',
  search:   'M7 12 a5 5 0 1 0 0-10 a5 5 0 0 0 0 10 z M11 11 l3 3',
  settings: 'M8 5.5 a2.5 2.5 0 1 0 0 5 a2.5 2.5 0 0 0 0 -5 z M8 1 v1.5 M8 13.5 v1.5 M14 8 h-1.5 M3.5 8 H2 M12.2 3.8 l-1 1 M5 11 l-1 1 M12.2 12.2 l-1 -1 M5 5 l-1 -1',
  chevR:    'M6 3 l5 5 l-5 5',
  chevD:    'M3 6 l5 5 l5 -5',
  arrowUp:  'M8 13 V3 M4 7 L8 3 L12 7',
  arrowDn:  'M8 3 V13 M4 9 L8 13 L12 9',
  sync:     'M3 8 a5 5 0 0 1 8.5 -3.5 L13 6 M3 6 V2 M13 8 a5 5 0 0 1 -8.5 3.5 L3 10 M13 10 v4',
  alert:    'M8 2 L14 13 H2 z M8 6 V9 M8 11.5 v.5',
  user:     'M8 8 a3 3 0 1 0 0 -6 a3 3 0 0 0 0 6 z M2 14 a6 6 0 0 1 12 0',
  bell:     'M4 11 V8 a4 4 0 0 1 8 0 v3 l1 2 H3 z M6.5 13 a1.5 1.5 0 0 0 3 0',
  bolt:     'M9 2 L4 9 H8 L7 14 L12 7 H8 z',
  dot:      'M8 8 m-2 0 a2 2 0 1 0 4 0 a2 2 0 1 0 -4 0',
  cloud:    'M4 12 H12 a3 3 0 0 0 0 -6 a4 4 0 0 0 -8 0 a2.5 2.5 0 0 0 0 5',
  filter:   'M2 3 H14 L10 8 V13 L6 14 V8 z',
  more:     'M3 8 h.01 M8 8 h.01 M13 8 h.01',
  close:    'M4 4 L12 12 M12 4 L4 12',
};

// ─── Reusable bits ───────────────────────────────────────────
function Pill({ children, tone = 'neutral', theme, size = 'sm' }) {
  const t = TOKENS[theme];
  const map = {
    neutral: { bg: t.surfaceAlt, fg: t.ink2, bd: t.border },
    good:    { bg: theme === 'dark' ? 'oklch(0.28 0.10 152)' : 'oklch(0.95 0.04 152)', fg: t.good, bd: 'transparent' },
    warn:    { bg: theme === 'dark' ? 'oklch(0.28 0.10 60)'  : 'oklch(0.96 0.04 60)',  fg: theme === 'dark' ? t.warn : '#7a5500', bd: 'transparent' },
    primary: { bg: t.primaryWeak, fg: t.primary, bd: 'transparent' },
    accent:  { bg: theme === 'dark' ? 'oklch(0.28 0.08 75)' : 'oklch(0.96 0.04 75)', fg: theme === 'dark' ? t.accent : t.accentInk, bd: 'transparent' },
    danger:  { bg: theme === 'dark' ? 'oklch(0.28 0.10 25)' : 'oklch(0.96 0.04 25)', fg: t.danger, bd: 'transparent' },
  }[tone];
  const sz = size === 'xs'
    ? { padding: '1px 6px', fontSize: 10.5, height: 16, gap: 3 }
    : { padding: '2px 8px', fontSize: 11.5, height: 19, gap: 4 };
  return (
    <span style={{
      display: 'inline-flex', alignItems: 'center',
      ...sz,
      background: map.bg, color: map.fg,
      border: `1px solid ${map.bd}`,
      borderRadius: 999, fontWeight: 600, letterSpacing: 0.1,
      lineHeight: 1, whiteSpace: 'nowrap',
    }}>{children}</span>
  );
}

function StatusDot({ tone = 'good', theme, pulse = false }) {
  const t = TOKENS[theme];
  const c = { good: t.good, warn: t.warn, danger: t.danger, idle: t.ink3, primary: t.primary }[tone];
  return (
    <span style={{
      display: 'inline-block', width: 8, height: 8, borderRadius: '50%',
      background: c,
      boxShadow: pulse ? `0 0 0 3px ${c}33` : 'none',
      flexShrink: 0,
    }} />
  );
}

function StorageBar({ used, total, theme, color, height = 4 }) {
  const t = TOKENS[theme];
  const pct = Math.min(100, (used / total) * 100);
  return (
    <div style={{
      width: '100%', height, borderRadius: height,
      background: t.surfaceAlt, overflow: 'hidden',
    }}>
      <div style={{
        width: `${pct}%`, height: '100%',
        background: color || t.primary, borderRadius: height,
        transition: 'width .3s',
      }} />
    </div>
  );
}

function Button({ children, kind = 'primary', theme, size = 'md', leading, trailing, full, onClick, style }) {
  const t = TOKENS[theme];
  const sz = size === 'sm'
    ? { padding: '0 10px', height: 26, fontSize: 12, gap: 6, radius: 6 }
    : size === 'lg'
    ? { padding: '0 18px', height: 38, fontSize: 14, gap: 8, radius: 8 }
    : { padding: '0 14px', height: 32, fontSize: 13, gap: 7, radius: 7 };
  const kinds = {
    primary: { bg: t.primary, fg: '#fff', bd: t.primary },
    accent:  { bg: t.accent, fg: t.accentInk, bd: t.accent },
    ghost:   { bg: 'transparent', fg: t.ink, bd: t.borderStrong },
    subtle:  { bg: t.surfaceAlt, fg: t.ink, bd: t.border },
    danger:  { bg: 'transparent', fg: t.danger, bd: t.danger + '55' },
  }[kind];
  return (
    <button onClick={onClick} style={{
      display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
      ...sz, padding: sz.padding, height: sz.height, fontSize: sz.fontSize, gap: sz.gap,
      borderRadius: sz.radius,
      background: kinds.bg, color: kinds.fg,
      border: `1px solid ${kinds.bd}`,
      fontFamily: FONT_UI, fontWeight: 600,
      cursor: 'pointer', whiteSpace: 'nowrap',
      width: full ? '100%' : 'auto',
      letterSpacing: 0.1,
      ...style,
    }}>
      {leading}
      {children}
      {trailing}
    </button>
  );
}

function Checkbox({ state = 'off', theme, size = 16 }) {
  const t = TOKENS[theme];
  const fill = state === 'off' ? 'transparent' : t.primary;
  return (
    <div style={{
      width: size, height: size, borderRadius: 4,
      background: fill,
      border: `1.5px solid ${state === 'off' ? t.borderStrong : t.primary}`,
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      flexShrink: 0,
    }}>
      {state === 'on' && (
        <svg width={size * 0.7} height={size * 0.7} viewBox="0 0 16 16" fill="none" stroke="#fff" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round">
          <path d="M3 8 L7 12 L13 4" />
        </svg>
      )}
      {state === 'mixed' && (
        <div style={{ width: size * 0.55, height: 2, background: '#fff', borderRadius: 1 }} />
      )}
    </div>
  );
}

function Switch({ on, theme, size = 28 }) {
  const t = TOKENS[theme];
  const h = Math.round(size * 0.58);
  return (
    <div style={{
      width: size, height: h, borderRadius: h,
      background: on ? t.primary : (theme === 'dark' ? 'rgba(255,250,240,0.18)' : 'rgba(30,24,16,0.18)'),
      position: 'relative', transition: 'background .15s', flexShrink: 0,
    }}>
      <div style={{
        position: 'absolute', top: 2, left: on ? size - h + 2 : 2,
        width: h - 4, height: h - 4, borderRadius: '50%',
        background: '#fff', transition: 'left .15s',
        boxShadow: '0 1px 2px rgba(0,0,0,.2)',
      }} />
    </div>
  );
}

Object.assign(window, {
  TOKENS, FONT_UI, FONT_MONO, PROVIDERS,
  ProviderMark, AStarLogo, WindowChrome,
  Icon, ICONS, Pill, StatusDot, StorageBar, Button, Checkbox, Switch,
});
