// Option A — Sidebar layout.
// Accounts-led IA. Left rail lists accounts with live status; main pane is
// the selected account's folder picker. Dense, pro tool feel.

function OptionASidebar({ theme }) {
  const t = TOKENS[theme];
  const accent = TOKENS[theme].primary;

  const accounts = [
    { id: 'gd1', kind: 'gdrive',   name: 'jane.dev', email: 'jane.dev@gmail.com',         used: 41.2, total: 100, status: 'syncing', folders: 8, active: true },
    { id: 'gd2', kind: 'gdrive',   name: 'Work',     email: 'jane@astar-dev.com',        used: 184,  total: 200, status: 'ok',      folders: 12 },
    { id: 'od1', kind: 'onedrive', name: 'Personal', email: 'jane.cooper@outlook.com',   used: 28.4, total: 50,  status: 'ok',      folders: 4 },
    { id: 'db1', kind: 'dropbox',  name: 'Side projects', email: 'jc@hey.com',           used: 6.1,  total: 16,  status: 'warn',    folders: 3 },
  ];

  // Folder tree for the selected account
  const tree = [
    { name: 'Documents',         depth: 0, state: 'on',    children: 14, size: '4.2 GB',   updated: 'just now', expanded: true },
    { name: 'Contracts 2025',    depth: 1, state: 'on',    children: 2,  size: '218 MB',   updated: '2 min ago' },
    { name: 'Designs',           depth: 1, state: 'on',    children: 6,  size: '1.4 GB',   updated: '12 min ago' },
    { name: 'Receipts',          depth: 1, state: 'off',   children: 3,  size: '88 MB',    updated: '—' },
    { name: 'Engineering',       depth: 0, state: 'mixed', children: 21, size: '18.9 GB',  updated: '40 s ago', expanded: true, syncing: true },
    { name: 'src/',              depth: 1, state: 'on',    children: 142, size: '2.1 GB',   updated: 'now', syncing: true },
    { name: 'archive/',          depth: 1, state: 'off',   children: 12, size: '14.2 GB',  updated: '—' },
    { name: 'sandbox/',          depth: 1, state: 'on',    children: 8,  size: '412 MB',   updated: '4 hr ago' },
    { name: 'Photos',            depth: 0, state: 'on',    children: 9,  size: '12.1 GB',  updated: 'yesterday' },
    { name: 'Shared with me',    depth: 0, state: 'off',   children: 33, size: '—',        updated: '—' },
  ];

  const sel = accounts.find(a => a.active);
  const selP = PROVIDERS[sel.kind];

  return (
    <WindowChrome theme={theme}>
      <div style={{ display: 'flex', height: '100%', width: '100%' }}>
        {/* ─── Left rail ─────────────────────────────── */}
        <aside style={{
          width: 286, flexShrink: 0,
          background: t.surfaceAlt,
          borderRight: `1px solid ${t.border}`,
          display: 'flex', flexDirection: 'column',
        }}>
          {/* Workspace header */}
          <div style={{ padding: '14px 14px 10px', display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{
              width: 30, height: 30, borderRadius: 8,
              background: t.primary, color: '#fff',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontWeight: 700, fontSize: 14,
            }}>JD</div>
            <div style={{ minWidth: 0, flex: 1 }}>
              <div style={{ fontSize: 13, fontWeight: 700, color: t.ink, lineHeight: 1.2 }}>Jane's Workspace</div>
              <div style={{ fontSize: 11, color: t.ink3, fontFamily: FONT_MONO }}>4 accounts · 5.2 TB total</div>
            </div>
            <Icon d={ICONS.chevD} size={12} color={t.ink3} />
          </div>

          {/* Search */}
          <div style={{ padding: '4px 12px 10px' }}>
            <div style={{
              display: 'flex', alignItems: 'center', gap: 8,
              padding: '0 10px', height: 30,
              background: t.surface,
              border: `1px solid ${t.border}`,
              borderRadius: 7, color: t.ink3,
            }}>
              <Icon d={ICONS.search} size={13} color={t.ink3} />
              <span style={{ fontSize: 12, flex: 1 }}>Search accounts & folders…</span>
              <span style={{ fontSize: 10, fontFamily: FONT_MONO, padding: '1px 5px', border: `1px solid ${t.border}`, borderRadius: 3 }}>⌘K</span>
            </div>
          </div>

          {/* Accounts section */}
          <div style={{
            padding: '12px 16px 6px',
            fontSize: 10.5, fontWeight: 700, letterSpacing: 0.8,
            color: t.ink3, display: 'flex', justifyContent: 'space-between',
          }}>
            <span>ACCOUNTS</span>
            <span style={{ fontFamily: FONT_MONO, fontWeight: 500 }}>4</span>
          </div>

          <div style={{ flex: 1, overflow: 'auto', padding: '0 8px' }}>
            {accounts.map(a => {
              const p = PROVIDERS[a.kind];
              return (
                <div key={a.id} style={{
                  padding: '8px 8px',
                  borderRadius: 7,
                  background: a.active ? (theme === 'dark' ? 'rgba(255,250,240,0.04)' : 'rgba(0,0,0,0.04)') : 'transparent',
                  marginBottom: 2,
                  position: 'relative',
                  borderLeft: a.active ? `2px solid ${t.primary}` : '2px solid transparent',
                  paddingLeft: 10,
                }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 9 }}>
                    <ProviderMark kind={a.kind} size={26} theme={theme} />
                    <div style={{ minWidth: 0, flex: 1 }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                        <span style={{ fontSize: 12.5, fontWeight: 600, color: t.ink, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{a.name}</span>
                        <StatusDot tone={a.status === 'ok' ? 'good' : a.status === 'warn' ? 'warn' : 'primary'} theme={theme} pulse={a.status === 'syncing'} />
                      </div>
                      <div style={{ fontSize: 10.5, color: t.ink3, fontFamily: FONT_MONO, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                        {a.email}
                      </div>
                    </div>
                  </div>
                  <div style={{ marginTop: 6, paddingLeft: 35 }}>
                    <StorageBar used={a.used} total={a.total} theme={theme} color={p.hue} height={3} />
                    <div style={{ marginTop: 4, display: 'flex', justifyContent: 'space-between', fontSize: 10, color: t.ink3, fontFamily: FONT_MONO }}>
                      <span>{a.used} / {a.total} GB</span>
                      <span>{a.folders} folders</span>
                    </div>
                  </div>
                </div>
              );
            })}

            {/* Add account */}
            <button style={{
              marginTop: 4, width: '100%',
              padding: '10px 10px',
              border: `1px dashed ${t.borderStrong}`,
              background: 'transparent', color: t.ink3,
              borderRadius: 7, cursor: 'pointer',
              display: 'flex', alignItems: 'center', gap: 8,
              fontFamily: FONT_UI, fontSize: 12, fontWeight: 600,
            }}>
              <Icon d={ICONS.plus} size={13} color={t.ink3} />
              Add account…
            </button>
          </div>

          {/* Today / throughput card — pinned below the accounts list */}
          <div style={{
            margin: '6px 12px 10px',
            padding: '10px 12px 12px',
            background: t.surface,
            border: `1px solid ${t.border}`,
            borderRadius: 8,
          }}>
            <div style={{
              display: 'flex', alignItems: 'baseline', justifyContent: 'space-between',
              marginBottom: 2,
            }}>
              <div style={{ fontSize: 10, fontWeight: 700, color: t.ink3, letterSpacing: 0.7 }}>TODAY</div>
              <div style={{ fontSize: 10, color: t.primary, fontFamily: FONT_MONO, fontWeight: 600 }}>
                ↓ 142 KB/s
              </div>
            </div>
            <div style={{ display: 'flex', alignItems: 'baseline', gap: 6 }}>
              <span style={{ fontSize: 18, fontWeight: 700, color: t.ink, letterSpacing: -0.4, fontFamily: FONT_MONO }}>1.24</span>
              <span style={{ fontSize: 11, color: t.ink2, fontWeight: 500 }}>GB · 384 files</span>
            </div>
            <div style={{ display: 'flex', alignItems: 'flex-end', gap: 2, marginTop: 8, height: 28 }}>
              {[20, 45, 12, 65, 80, 30, 55, 42, 88, 70, 95, 50, 30, 75, 60, 40, 92, 80, 65, 100, 55, 38, 70, 80].map((h, i) => (
                <div key={i} style={{
                  flex: 1, height: `${h}%`,
                  background: i >= 20 ? t.primary : (theme === 'dark' ? 'rgba(255,250,240,0.16)' : 'rgba(30,24,16,0.14)'),
                  borderRadius: 1,
                }} />
              ))}
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 9.5, color: t.ink3, fontFamily: FONT_MONO, marginTop: 4 }}>
              <span>00:00</span><span>12:00</span><span>now</span>
            </div>
          </div>

          {/* Sidebar footer */}
          <div style={{
            borderTop: `1px solid ${t.border}`,
            padding: '10px 12px',
            display: 'flex', alignItems: 'center', gap: 8,
          }}>
            <Button kind="ghost" size="sm" theme={theme} leading={<Icon d={ICONS.pause} size={11} />}>
              Pause all
            </Button>
            <div style={{ flex: 1 }} />
            <div style={{ width: 28, height: 28, borderRadius: 6, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Icon d={ICONS.bell} size={14} color={t.ink2} />
            </div>
            <div style={{ width: 28, height: 28, borderRadius: 6, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Icon d={ICONS.settings} size={14} color={t.ink2} />
            </div>
          </div>
        </aside>

        {/* ─── Main pane ─────────────────────────────── */}
        <main style={{ flex: 1, display: 'flex', flexDirection: 'column', minWidth: 0, background: t.bg }}>

          {/* Header */}
          <div style={{
            padding: '18px 28px 14px',
            borderBottom: `1px solid ${t.border}`,
            display: 'flex', alignItems: 'flex-start', gap: 16,
          }}>
            <ProviderMark kind={sel.kind} size={42} theme={theme} />
            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ display: 'flex', alignItems: 'baseline', gap: 10, marginBottom: 4 }}>
                <h1 style={{ margin: 0, fontSize: 19, fontWeight: 700, color: t.ink, letterSpacing: -0.3 }}>{sel.name}</h1>
                <span style={{ fontSize: 12, color: t.ink3, fontFamily: FONT_MONO }}>{selP.name}</span>
                <Pill tone="primary" theme={theme}>
                  <span style={{ display: 'inline-flex', alignItems: 'center', gap: 4 }}>
                    <span style={{ width: 6, height: 6, borderRadius: '50%', background: t.primary, animation: 'astar-pulse 1.4s infinite' }} />
                    Syncing
                  </span>
                </Pill>
              </div>
              <div style={{ fontSize: 12.5, color: t.ink3, fontFamily: FONT_MONO }}>{sel.email}</div>
            </div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <Button kind="subtle" size="sm" theme={theme} leading={<Icon d={ICONS.pause} size={11} />}>Pause</Button>
              <Button kind="ghost"  size="sm" theme={theme} leading={<Icon d={ICONS.settings} size={11} />}>Settings</Button>
              <Button kind="ghost"  size="sm" theme={theme}>
                <Icon d={ICONS.more} size={13} />
              </Button>
            </div>
          </div>

          {/* Sub-tabs */}
          <div style={{
            padding: '0 28px',
            borderBottom: `1px solid ${t.border}`,
            display: 'flex', gap: 4,
          }}>
            {['Sync folders', 'Activity', 'Conflicts', 'Settings'].map((tab, i) => {
              const active = i === 0;
              return (
                <div key={tab} style={{
                  padding: '12px 12px 11px',
                  fontSize: 12.5, fontWeight: active ? 700 : 500,
                  color: active ? t.ink : t.ink3,
                  borderBottom: active ? `2px solid ${t.primary}` : '2px solid transparent',
                  cursor: 'pointer',
                  display: 'flex', alignItems: 'center', gap: 6,
                }}>
                  {tab}
                  {tab === 'Conflicts' && <Pill tone="warn" theme={theme} size="xs">2</Pill>}
                </div>
              );
            })}
          </div>

          {/* Toolbar above tree */}
          <div style={{
            padding: '10px 28px',
            display: 'flex', alignItems: 'center', gap: 10,
            background: t.bg,
            borderBottom: `1px solid ${t.border}`,
          }}>
            <div style={{ fontSize: 11, fontFamily: FONT_MONO, color: t.ink3 }}>
              <span style={{ color: t.ink2 }}>~/AStar</span> / {sel.name}
            </div>
            <div style={{ flex: 1 }} />
            <Pill tone="neutral" theme={theme}>
              <span style={{ display: 'inline-flex', alignItems: 'center', gap: 4 }}>
                <Icon d={ICONS.check} size={10} color={t.good} />
                8 of 24 folders selected · 22.6 GB
              </span>
            </Pill>
            <Button kind="ghost" size="sm" theme={theme} leading={<Icon d={ICONS.filter} size={11} />}>Filter</Button>
            <Button kind="primary" size="sm" theme={theme}>Apply changes</Button>
          </div>

          {/* Folder tree */}
          <div style={{ flex: 1, overflow: 'auto', padding: '4px 16px 8px' }}>
            {/* Column headers */}
            <div style={{
              display: 'grid', gridTemplateColumns: '20px 22px 1fr 90px 110px 70px',
              alignItems: 'center', gap: 10,
              padding: '8px 14px',
              fontSize: 10.5, fontWeight: 700, letterSpacing: 0.6,
              color: t.ink3, textTransform: 'uppercase',
              borderBottom: `1px solid ${t.border}`,
            }}>
              <span></span>
              <span></span>
              <span>Folder</span>
              <span style={{ textAlign: 'right' }}>Items</span>
              <span style={{ textAlign: 'right' }}>Size</span>
              <span style={{ textAlign: 'right' }}>Updated</span>
            </div>

            {tree.map((row, i) => (
              <div key={i} style={{
                display: 'grid', gridTemplateColumns: '20px 22px 1fr 90px 110px 70px',
                alignItems: 'center', gap: 10,
                padding: '9px 14px',
                fontSize: 12.5,
                paddingLeft: 14 + row.depth * 16,
                borderRadius: 6,
                background: row.syncing ? (theme === 'dark' ? 'rgba(120,100,255,0.06)' : 'oklch(0.97 0.025 268)') : 'transparent',
                color: t.ink,
              }}>
                <Icon d={row.children ? (row.expanded ? ICONS.chevD : ICONS.chevR) : ''} size={10} color={t.ink3} strokeWidth={2.2} />
                <Checkbox state={row.state} theme={theme} />
                <div style={{ display: 'flex', alignItems: 'center', gap: 8, minWidth: 0 }}>
                  <Icon d={ICONS.folder} size={14} color={row.state === 'off' ? t.ink3 : t.primary} fill={row.state === 'off' ? 'none' : t.primaryWeak} />
                  <span style={{ fontWeight: row.depth === 0 ? 600 : 500, color: row.state === 'off' ? t.ink3 : t.ink, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis', fontFamily: row.name.endsWith('/') ? FONT_MONO : FONT_UI, fontSize: row.name.endsWith('/') ? 12 : 12.5 }}>{row.name}</span>
                  {row.syncing && (
                    <Pill tone="primary" theme={theme} size="xs">
                      <span style={{ display: 'inline-flex', alignItems: 'center', gap: 3 }}>
                        <span style={{ width: 5, height: 5, borderRadius: '50%', background: t.primary, animation: 'astar-pulse 1.4s infinite' }} />
                        syncing
                      </span>
                    </Pill>
                  )}
                </div>
                <span style={{ textAlign: 'right', fontFamily: FONT_MONO, fontSize: 11.5, color: t.ink3 }}>{row.children}</span>
                <span style={{ textAlign: 'right', fontFamily: FONT_MONO, fontSize: 11.5, color: t.ink2 }}>{row.size}</span>
                <span style={{ textAlign: 'right', fontFamily: FONT_MONO, fontSize: 11, color: t.ink3 }}>{row.updated}</span>
              </div>
            ))}
          </div>

          {/* Footer status bar */}
          <div style={{
            height: 30, flexShrink: 0,
            borderTop: `1px solid ${t.border}`,
            background: t.surfaceAlt,
            padding: '0 16px',
            display: 'flex', alignItems: 'center', gap: 16,
            fontSize: 11, fontFamily: FONT_MONO, color: t.ink3,
          }}>
            <span style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
              <StatusDot tone="good" theme={theme} />
              All accounts healthy
            </span>
            <span>↑ 0 B/s</span>
            <span style={{ color: t.primary }}>↓ 142.4 KB/s</span>
            <span>3 files in queue · 188 MB</span>
            <div style={{ flex: 1 }} />
            <span>v2.4.1 · linux-x64</span>
          </div>
        </main>
      </div>
    </WindowChrome>
  );
}

Object.assign(window, { OptionASidebar });
