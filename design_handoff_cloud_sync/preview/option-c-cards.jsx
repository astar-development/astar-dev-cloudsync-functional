// Option C — Cards dashboard.
// Overview-led IA. The home screen is a 2×2 grid of large account cards;
// each card surfaces storage, sync state, recent activity, and primary
// actions. One card is expanded inline as a "focus" state to show how
// folder selection enters this layout.

function OptionCCards({ theme }) {
  const t = TOKENS[theme];

  const accounts = [
    {
      id: 'gd1', kind: 'gdrive',   name: 'Work',     email: 'jane@astar-dev.com',
      used: 184, total: 200, folders: 12, files: 18420,
      state: 'live', queue: '142 MB · 18 files', rate: '↓ 124 KB/s',
      recent: [
        { dir: 'engineering/src/components/', file: 'Sidebar.tsx', time: 'now',  k: 'up' },
        { dir: 'designs/',                    file: 'Onboarding @2x.fig', time: '34 s', k: 'down' },
        { dir: 'engineering/notes/',          file: 'q3-planning.md', time: '14:08', k: 'up' },
      ],
    },
    {
      id: 'od1', kind: 'onedrive', name: 'Personal', email: 'jane.cooper@outlook.com',
      used: 28.4, total: 50, folders: 4, files: 4218,
      state: 'live', queue: '48.1 MB · 1 file', rate: '↓ 9.2 MB/s',
      expanded: true,
      recent: [
        { dir: 'Photos/Iceland 2025/', file: 'IMG_4421.dng', time: 'now', k: 'down' },
        { dir: 'Receipts/2026-05/',    file: 'groceries.jpg', time: '14:22', k: 'up' },
        { dir: 'Documents/Travel/',    file: 'itinerary.md',  time: '2 m', k: 'conflict' },
      ],
    },
    {
      id: 'db1', kind: 'dropbox', name: 'Side projects', email: 'jc@hey.com',
      used: 6.1, total: 16, folders: 3, files: 842,
      state: 'live', queue: '212 KB · 1 file', rate: '↑ 88 KB/s',
      recent: [
        { dir: 'astar-marketing/build/', file: 'index.html', time: 'now',  k: 'up' },
        { dir: 'astar-marketing/src/',   file: 'index.css',  time: '3 m',  k: 'up' },
        { dir: 'astar-marketing/',       file: 'og-image.png', time: '13:54', k: 'down' },
      ],
    },
    {
      id: 'gd2', kind: 'gdrive', name: 'jane.dev',  email: 'jane.dev@gmail.com',
      used: 41.2, total: 100, folders: 8, files: 1284,
      state: 'paused', queue: '—', rate: 'Paused',
      recent: [
        { dir: 'Contracts 2025/',   file: 'freelance-msa-v3.pdf', time: '34 s', k: 'down' },
        { dir: 'Designs/',          file: 'astar-logo.svg',       time: '14:02', k: 'up' },
      ],
    },
  ];

  const expandedFolders = [
    { name: 'Documents',          state: 'on',    items: 14, size: '4.2 GB',  primary: true },
    { name: 'Photos',             state: 'on',    items: 1218, size: '12.1 GB', primary: true },
    { name: 'Receipts',           state: 'off',   items: 3,  size: '88 MB' },
    { name: 'Travel & itinerary', state: 'mixed', items: 8,  size: '120 MB' },
    { name: 'Music',              state: 'off',   items: 412, size: '6.4 GB' },
    { name: 'Shared with me',     state: 'off',   items: 33, size: '—' },
  ];

  // ─── Account card ─────────────────────────────
  const AccountCard = ({ a }) => {
    const p = PROVIDERS[a.kind];
    const pct = (a.used / a.total) * 100;
    const isLive = a.state === 'live';
    return (
      <div style={{
        background: t.surface, borderRadius: 14,
        border: `1px solid ${t.border}`,
        boxShadow: '0 1px 2px rgba(0,0,0,.03), 0 8px 24px rgba(0,0,0,.04)',
        overflow: 'hidden',
        gridColumn: a.expanded ? 'span 2' : 'span 1',
        display: 'flex', flexDirection: 'column',
        position: 'relative',
      }}>
        {/* Accent stripe */}
        <div style={{ height: 4, background: p.hue }} />

        {/* Header */}
        <div style={{ padding: '16px 18px 12px', display: 'flex', alignItems: 'flex-start', gap: 12 }}>
          <ProviderMark kind={a.kind} size={42} theme={theme} />
          <div style={{ flex: 1, minWidth: 0 }}>
            <div style={{ display: 'flex', alignItems: 'baseline', gap: 8, marginBottom: 3 }}>
              <h3 style={{ margin: 0, fontSize: 16, fontWeight: 700, color: t.ink, letterSpacing: -0.3, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                {a.name}
              </h3>
              <span style={{ fontSize: 11, color: t.ink3, fontFamily: FONT_MONO, fontWeight: 500 }}>{p.name}</span>
            </div>
            <div style={{ fontSize: 11.5, color: t.ink3, fontFamily: FONT_MONO, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
              {a.email}
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
            {isLive
              ? <Pill tone="primary" theme={theme}>
                  <span style={{ display: 'inline-flex', alignItems: 'center', gap: 4 }}>
                    <span style={{ width: 6, height: 6, borderRadius: '50%', background: t.primary, animation: 'astar-pulse 1.4s infinite' }} />
                    Syncing
                  </span>
                </Pill>
              : <Pill tone="neutral" theme={theme}>
                  <span style={{ display: 'inline-flex', alignItems: 'center', gap: 4 }}>
                    <Icon d={ICONS.pause} size={9} color={t.ink2} strokeWidth={2.4} />
                    Paused
                  </span>
                </Pill>
            }
            <button style={{
              width: 26, height: 26, borderRadius: 6,
              background: 'transparent', border: 'none', cursor: 'pointer',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              color: t.ink3,
            }}>
              <Icon d={ICONS.more} size={14} color={t.ink3} />
            </button>
          </div>
        </div>

        {/* Storage row */}
        <div style={{ padding: '0 18px 14px' }}>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 6, marginBottom: 5 }}>
            <span style={{ fontSize: 18, fontWeight: 700, fontFamily: FONT_MONO, color: t.ink, letterSpacing: -0.3 }}>
              {a.used}
            </span>
            <span style={{ fontSize: 12, color: t.ink3, fontFamily: FONT_MONO }}>
              / {a.total} GB
            </span>
            <span style={{ flex: 1 }} />
            <span style={{ fontSize: 11, color: pct > 90 ? t.warn : t.ink3, fontFamily: FONT_MONO, fontWeight: 600 }}>
              {pct.toFixed(0)}%
            </span>
          </div>
          <StorageBar used={a.used} total={a.total} theme={theme} color={p.hue} height={6} />
          <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 8, fontSize: 11, color: t.ink3, fontFamily: FONT_MONO }}>
            <span>{a.folders} folders synced · {a.files.toLocaleString()} files</span>
            <span style={{ color: isLive ? t.primary : t.ink3, fontWeight: 600 }}>{a.rate}</span>
          </div>
        </div>

        {/* Recent activity */}
        <div style={{
          margin: '0 18px',
          background: t.surfaceAlt,
          borderRadius: 8,
          padding: '10px 12px',
        }}>
          <div style={{
            fontSize: 10, fontWeight: 700, color: t.ink3, letterSpacing: 0.7,
            marginBottom: 6, display: 'flex', justifyContent: 'space-between',
          }}>
            <span>RECENT ACTIVITY</span>
            <span style={{ fontFamily: FONT_MONO, fontWeight: 500 }}>{a.queue}</span>
          </div>
          {a.recent.slice(0, 3).map((r, i) => {
            const ic = r.k === 'up' ? ICONS.arrowUp : r.k === 'down' ? ICONS.arrowDn : ICONS.alert;
            const ic_col = r.k === 'up' ? t.primary : r.k === 'down' ? t.good : t.warn;
            return (
              <div key={i} style={{
                display: 'flex', alignItems: 'center', gap: 8,
                padding: '5px 0',
                borderTop: i > 0 ? `1px solid ${t.border}` : 'none',
                fontFamily: FONT_MONO, fontSize: 11.5,
              }}>
                <Icon d={ic} size={10} color={ic_col} strokeWidth={2.4} />
                <span style={{ color: t.ink3, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis', minWidth: 0, flex: 1 }}>
                  {r.dir}<span style={{ color: t.ink, fontWeight: 600 }}>{r.file}</span>
                </span>
                <span style={{ color: t.ink3, flexShrink: 0 }}>{r.time}</span>
              </div>
            );
          })}
        </div>

        {/* Expanded folder picker (only for expanded card) */}
        {a.expanded && (
          <div style={{ margin: '12px 18px 0', borderTop: `1px solid ${t.border}`, paddingTop: 12 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 10 }}>
              <Icon d={ICONS.folder} size={13} color={t.ink2} />
              <span style={{ fontSize: 12, fontWeight: 700, color: t.ink, letterSpacing: 0.2 }}>SELECTIVE SYNC</span>
              <span style={{ fontSize: 11, color: t.ink3, fontFamily: FONT_MONO }}>· 4 of 9 folders</span>
              <div style={{ flex: 1 }} />
              <Button kind="ghost" size="sm" theme={theme} leading={<Icon d={ICONS.plus} size={11} />}>
                New sync path
              </Button>
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 6 }}>
              {expandedFolders.map((f, i) => (
                <div key={i} style={{
                  display: 'flex', alignItems: 'center', gap: 10,
                  padding: '8px 10px',
                  background: f.state === 'on' ? PROVIDERS.onedrive.hueWeak : t.surfaceAlt,
                  borderRadius: 7,
                  border: `1px solid ${f.state === 'on' ? PROVIDERS.onedrive.hue + '33' : t.border}`,
                }}>
                  <Checkbox state={f.state} theme={theme} size={15} />
                  <Icon d={ICONS.folder} size={13} color={f.state === 'off' ? t.ink3 : PROVIDERS.onedrive.hue} fill={f.state === 'off' ? 'none' : PROVIDERS.onedrive.hueWeak} />
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ fontSize: 12.5, fontWeight: 600, color: f.state === 'off' ? t.ink3 : t.ink, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {f.name}
                    </div>
                    <div style={{ fontSize: 10.5, color: t.ink3, fontFamily: FONT_MONO }}>
                      {f.items.toLocaleString()} items · {f.size}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Actions */}
        <div style={{ padding: '12px 18px 14px', marginTop: 'auto', display: 'flex', gap: 8, alignItems: 'center' }}>
          {a.expanded ? (
            <React.Fragment>
              <Button kind="primary" size="sm" theme={theme}>Save changes</Button>
              <Button kind="ghost" size="sm" theme={theme}>Cancel</Button>
              <div style={{ flex: 1 }} />
              <Button kind="ghost" size="sm" theme={theme} leading={<Icon d={ICONS.pause} size={11} />}>Pause</Button>
            </React.Fragment>
          ) : (
            <React.Fragment>
              <Button kind="subtle" size="sm" theme={theme} leading={<Icon d={ICONS.folder} size={11} />}>
                Manage folders
              </Button>
              <Button kind="ghost" size="sm" theme={theme} leading={<Icon d={isLive ? ICONS.pause : ICONS.play} size={11} />}>
                {isLive ? 'Pause' : 'Resume'}
              </Button>
              <div style={{ flex: 1 }} />
              <span style={{ fontSize: 11, color: t.ink3, fontFamily: FONT_MONO }}>Last sync · 2 m ago</span>
            </React.Fragment>
          )}
        </div>
      </div>
    );
  };

  return (
    <WindowChrome theme={theme}>
      <div style={{ display: 'flex', flexDirection: 'column', height: '100%', background: t.bg }}>

        {/* Page header */}
        <div style={{
          padding: '24px 32px 18px',
          display: 'flex', alignItems: 'flex-end', gap: 16,
          borderBottom: `1px solid ${t.border}`,
        }}>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: 11, fontWeight: 700, color: t.ink3, letterSpacing: 0.8, marginBottom: 4 }}>
              YOUR CLOUDS
            </div>
            <h1 style={{ margin: 0, fontSize: 26, fontWeight: 700, color: t.ink, letterSpacing: -0.6 }}>
              Everything in one place
              <span style={{ color: t.accent, marginLeft: 4 }}>.</span>
            </h1>
            <div style={{ marginTop: 8, display: 'flex', alignItems: 'center', gap: 16, fontFamily: FONT_MONO, fontSize: 12, color: t.ink3 }}>
              <span style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
                <StatusDot tone="primary" theme={theme} pulse />
                <span style={{ color: t.ink2 }}>3 of 4 syncing</span>
              </span>
              <span>259.7 / 366 GB</span>
              <span style={{ color: t.primary, fontWeight: 600 }}>↓ 9.4 MB/s</span>
              <span>24,764 files</span>
            </div>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <Button kind="ghost"  size="md" theme={theme} leading={<Icon d={ICONS.pause} size={12} />}>
              Pause all
            </Button>
            <Button kind="primary" size="md" theme={theme} leading={<Icon d={ICONS.plus} size={12} />}>
              Add account
            </Button>
            <Button kind="ghost"   size="md" theme={theme}>
              <Icon d={ICONS.settings} size={14} />
            </Button>
          </div>
        </div>

        {/* Grid */}
        <div style={{ flex: 1, overflow: 'auto', padding: '20px 32px 28px' }}>
          <div style={{
            display: 'grid',
            gridTemplateColumns: '1fr 1fr',
            gap: 16,
            alignItems: 'stretch',
          }}>
            {accounts.map(a => <AccountCard key={a.id} a={a} />)}

            {/* Add-account tile */}
            <button style={{
              gridColumn: 'span 2',
              padding: '20px 24px',
              border: `1.5px dashed ${t.borderStrong}`,
              background: 'transparent',
              borderRadius: 14, cursor: 'pointer',
              display: 'flex', alignItems: 'center', gap: 16,
              color: t.ink2, fontFamily: FONT_UI,
              textAlign: 'left',
            }}>
              <div style={{
                width: 44, height: 44, borderRadius: 10,
                background: t.surfaceAlt,
                border: `1px solid ${t.border}`,
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                color: t.ink2,
              }}>
                <Icon d={ICONS.plus} size={18} color={t.ink2} strokeWidth={2} />
              </div>
              <div style={{ flex: 1 }}>
                <div style={{ fontSize: 14, fontWeight: 700, color: t.ink }}>Connect another cloud</div>
                <div style={{ fontSize: 12, color: t.ink3, marginTop: 2 }}>
                  OneDrive · Google Drive · Dropbox — all sit side-by-side.
                </div>
              </div>
              <div style={{ display: 'flex', gap: 6 }}>
                <ProviderMark kind="onedrive" size={26} theme={theme} />
                <ProviderMark kind="gdrive"   size={26} theme={theme} />
                <ProviderMark kind="dropbox"  size={26} theme={theme} />
              </div>
            </button>
          </div>
        </div>
      </div>
    </WindowChrome>
  );
}

Object.assign(window, { OptionCCards });
