// Option B — Tabs layout.
// Task-led IA. Top tabs switch the whole workspace (Accounts · Folders ·
// Activity · Settings). Hero shown is the Activity feed — a unified stream
// across all accounts with a summary rail.

function OptionBTabs({ theme }) {
  const t = TOKENS[theme];

  const tabs = [
    { id: 'accounts', label: 'Accounts',   count: 3 },
    { id: 'folders',  label: 'Folders',    count: 24 },
    { id: 'activity', label: 'Activity',   count: null, active: true },
    { id: 'settings', label: 'Settings',   count: null },
  ];

  const feed = [
    {
      group: 'NOW SYNCING',
      items: [
        { kind: 'up',   account: 'gdrive',   acctLabel: 'Work',          path: 'engineering/src/components/Sidebar.tsx',     size: '14.2 KB', time: 'now',   pct: 78 },
        { kind: 'down', account: 'onedrive', acctLabel: 'Personal',      path: 'Photos/Iceland 2025/IMG_4421.dng',           size: '48.1 MB', time: 'now',   pct: 32 },
        { kind: 'up',   account: 'dropbox',  acctLabel: 'Side projects', path: 'astar-marketing/build/index.html',           size: '212 KB',  time: 'now',   pct: 91 },
      ],
    },
    {
      group: 'LAST 5 MINUTES',
      items: [
        { kind: 'down', account: 'gdrive',   acctLabel: 'jane.dev', path: 'Contracts 2025/freelance-msa-v3.pdf', size: '1.2 MB', time: '34 s',  status: 'done' },
        { kind: 'up',   account: 'gdrive',   acctLabel: 'Work',     path: 'engineering/src/server/router.go',    size: '8.1 KB', time: '1 m',   status: 'done' },
        { kind: 'conflict', account: 'onedrive', acctLabel: 'Personal', path: 'Documents/Travel/itinerary.md',  size: '4.2 KB', time: '2 m',  status: 'conflict' },
        { kind: 'up',   account: 'dropbox',  acctLabel: 'Side projects', path: 'astar-marketing/src/index.css', size: '22 KB',  time: '3 m', status: 'done' },
        { kind: 'down', account: 'gdrive',   acctLabel: 'Work',     path: 'designs/Onboarding @2x.fig',           size: '14.8 MB', time: '4 m', status: 'done' },
      ],
    },
    {
      group: 'EARLIER TODAY',
      items: [
        { kind: 'up',   account: 'onedrive', acctLabel: 'Personal', path: 'Receipts/2026-05/groceries.jpg',       size: '480 KB', time: '14:22', status: 'done' },
        { kind: 'up',   account: 'gdrive',   acctLabel: 'Work',     path: 'engineering/notes/q3-planning.md',     size: '11 KB',  time: '14:08', status: 'done' },
        { kind: 'down', account: 'dropbox',  acctLabel: 'Side projects', path: 'astar-marketing/og-image.png',    size: '1.1 MB', time: '13:54', status: 'done' },
      ],
    },
  ];

  return (
    <WindowChrome theme={theme}>
      <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>

        {/* ─── Tab strip ─────────────────────────────── */}
        <div style={{
          display: 'flex', alignItems: 'flex-end',
          padding: '12px 24px 0',
          background: t.bg,
          borderBottom: `1px solid ${t.border}`,
          gap: 4,
        }}>
          {tabs.map(tb => (
            <div key={tb.id} style={{
              padding: '10px 16px',
              borderRadius: '8px 8px 0 0',
              background: tb.active ? t.surface : 'transparent',
              border: tb.active ? `1px solid ${t.border}` : '1px solid transparent',
              borderBottom: tb.active ? `1px solid ${t.surface}` : '1px solid transparent',
              marginBottom: -1,
              fontSize: 13, fontWeight: tb.active ? 700 : 500,
              color: tb.active ? t.ink : t.ink3,
              display: 'flex', alignItems: 'center', gap: 8,
              cursor: 'pointer', position: 'relative',
            }}>
              {tb.active && (
                <span style={{
                  position: 'absolute', top: 0, left: 12, right: 12, height: 2,
                  background: t.primary, borderRadius: 2,
                }} />
              )}
              {tb.label}
              {tb.count !== null && (
                <span style={{
                  fontSize: 10.5, fontFamily: FONT_MONO,
                  color: tb.active ? t.ink2 : t.ink3,
                  background: t.surfaceAlt,
                  padding: '1px 6px', borderRadius: 999,
                  fontWeight: 500,
                }}>{tb.count}</span>
              )}
            </div>
          ))}
          <div style={{ flex: 1 }} />
          <div style={{ paddingBottom: 8, display: 'flex', alignItems: 'center', gap: 6 }}>
            <div style={{
              display: 'flex', alignItems: 'center', gap: 6,
              padding: '4px 10px', height: 26,
              background: t.surface, border: `1px solid ${t.border}`,
              borderRadius: 999, fontSize: 11.5, color: t.ink2,
            }}>
              <Icon d={ICONS.search} size={11} color={t.ink3} />
              <span>Find a file…</span>
              <span style={{ fontSize: 10, fontFamily: FONT_MONO, color: t.ink3 }}>⌘K</span>
            </div>
            <Button kind="ghost" size="sm" theme={theme}><Icon d={ICONS.bell} size={13} /></Button>
            <Button kind="ghost" size="sm" theme={theme}><Icon d={ICONS.settings} size={13} /></Button>
          </div>
        </div>

        {/* ─── Workspace ─────────────────────────────── */}
        <div style={{
          flex: 1, display: 'grid', minHeight: 0,
          gridTemplateColumns: '1fr 320px',
          background: t.surface,
        }}>

          {/* ─── Activity feed (hero) ──────────────── */}
          <div style={{ display: 'flex', flexDirection: 'column', minWidth: 0, borderRight: `1px solid ${t.border}` }}>

            {/* Filter row */}
            <div style={{
              padding: '18px 28px 14px',
              display: 'flex', alignItems: 'center', gap: 10,
              borderBottom: `1px solid ${t.border}`,
            }}>
              <h2 style={{ margin: 0, fontSize: 18, fontWeight: 700, color: t.ink, letterSpacing: -0.3 }}>
                Activity
              </h2>
              <Pill tone="primary" theme={theme}>
                <span style={{ display: 'inline-flex', alignItems: 'center', gap: 4 }}>
                  <span style={{ width: 6, height: 6, borderRadius: '50%', background: t.primary, animation: 'astar-pulse 1.4s infinite' }} />
                  Live · 3 active
                </span>
              </Pill>
              <div style={{ flex: 1 }} />
              <div style={{ display: 'flex', gap: 4, padding: 3, background: t.surfaceAlt, borderRadius: 7, border: `1px solid ${t.border}` }}>
                {[
                  { label: 'All', active: true, count: 11 },
                  { label: 'G',  account: 'gdrive', count: 5 },
                  { label: 'O',  account: 'onedrive', count: 2 },
                  { label: 'D',  account: 'dropbox', count: 4 },
                ].map((c, i) => (
                  <div key={i} style={{
                    padding: '4px 9px', borderRadius: 5,
                    background: c.active ? t.surface : 'transparent',
                    boxShadow: c.active ? '0 1px 2px rgba(0,0,0,.06)' : 'none',
                    fontSize: 12, fontWeight: 600,
                    color: c.active ? t.ink : t.ink3,
                    display: 'flex', alignItems: 'center', gap: 5,
                    cursor: 'pointer',
                  }}>
                    {c.account && <ProviderMark kind={c.account} size={14} theme={theme} />}
                    {c.label}
                    <span style={{ color: t.ink3, fontFamily: FONT_MONO, fontWeight: 500, fontSize: 11 }}>{c.count}</span>
                  </div>
                ))}
              </div>
              <Button kind="ghost" size="sm" theme={theme} leading={<Icon d={ICONS.filter} size={11} />}>Filter</Button>
            </div>

            {/* Feed */}
            <div style={{ flex: 1, overflow: 'auto', padding: '8px 0 16px' }}>
              {feed.map((grp, gi) => (
                <div key={gi}>
                  <div style={{
                    padding: '14px 28px 8px',
                    fontSize: 10.5, fontWeight: 700, letterSpacing: 0.8,
                    color: t.ink3, display: 'flex', alignItems: 'center', gap: 8,
                  }}>
                    {grp.group}
                    <div style={{ flex: 1, height: 1, background: t.border }} />
                  </div>
                  {grp.items.map((it, i) => {
                    const p = PROVIDERS[it.account];
                    const isLive = grp.group === 'NOW SYNCING';
                    const arrowColor = it.kind === 'up' ? t.primary
                      : it.kind === 'down' ? t.good
                      : t.warn;
                    const arrowIcon = it.kind === 'up' ? ICONS.arrowUp
                      : it.kind === 'down' ? ICONS.arrowDn
                      : ICONS.alert;
                    const parts = it.path.split('/');
                    const file = parts.pop();
                    const dir = parts.join('/') + (parts.length ? '/' : '');
                    return (
                      <div key={i} style={{
                        padding: '9px 28px',
                        display: 'grid',
                        gridTemplateColumns: '22px 1fr auto auto',
                        alignItems: 'center', gap: 12,
                        borderTop: i === 0 ? 'none' : `1px solid ${t.border}`,
                        position: 'relative',
                      }}>
                        <div style={{
                          width: 22, height: 22, borderRadius: 6,
                          background: it.kind === 'conflict' ? (theme === 'dark' ? 'oklch(0.28 0.10 60)' : 'oklch(0.96 0.04 60)') : t.surfaceAlt,
                          display: 'flex', alignItems: 'center', justifyContent: 'center',
                        }}>
                          <Icon d={arrowIcon} size={11} color={arrowColor} strokeWidth={2.2} />
                        </div>

                        <div style={{ minWidth: 0 }}>
                          <div style={{ display: 'flex', alignItems: 'baseline', gap: 6, fontFamily: FONT_MONO, fontSize: 12.5, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                            <span style={{ color: t.ink3 }}>{dir}</span>
                            <span style={{ color: t.ink, fontWeight: 600 }}>{file}</span>
                          </div>
                          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 3, fontSize: 11, color: t.ink3 }}>
                            <ProviderMark kind={it.account} size={14} theme={theme} />
                            <span>{p.name} · {it.acctLabel}</span>
                            {it.status === 'conflict' && <Pill tone="warn" theme={theme} size="xs">Conflict</Pill>}
                          </div>
                        </div>

                        <div style={{ textAlign: 'right', minWidth: 80 }}>
                          {isLive ? (
                            <div>
                              <div style={{ width: 110, height: 4, borderRadius: 2, background: t.surfaceAlt, overflow: 'hidden' }}>
                                <div style={{ width: `${it.pct}%`, height: '100%', background: t.primary, borderRadius: 2 }} />
                              </div>
                              <div style={{ fontFamily: FONT_MONO, fontSize: 10.5, color: t.ink3, marginTop: 3 }}>
                                {it.pct}% · {it.size}
                              </div>
                            </div>
                          ) : (
                            <div style={{ fontFamily: FONT_MONO, fontSize: 11.5, color: t.ink2 }}>{it.size}</div>
                          )}
                        </div>

                        <div style={{ fontFamily: FONT_MONO, fontSize: 11, color: t.ink3, minWidth: 38, textAlign: 'right' }}>
                          {it.time}
                        </div>
                      </div>
                    );
                  })}
                </div>
              ))}
            </div>
          </div>

          {/* ─── Right rail: summary ───────────────── */}
          <aside style={{
            background: t.surfaceAlt,
            padding: '18px 18px 14px',
            display: 'flex', flexDirection: 'column', gap: 14,
            overflow: 'auto',
          }}>
            {/* Today summary card */}
            <div style={{
              background: t.surface, borderRadius: 10,
              border: `1px solid ${t.border}`,
              padding: '14px 16px',
            }}>
              <div style={{ fontSize: 11, fontWeight: 700, color: t.ink3, letterSpacing: 0.7 }}>TODAY</div>
              <div style={{ display: 'flex', alignItems: 'baseline', gap: 8, marginTop: 6 }}>
                <span style={{ fontSize: 28, fontWeight: 700, color: t.ink, letterSpacing: -0.6, fontFamily: FONT_MONO }}>1.24</span>
                <span style={{ fontSize: 14, color: t.ink2, fontWeight: 500 }}>GB synced</span>
              </div>
              <div style={{ fontSize: 12, color: t.ink3, marginTop: 2, fontFamily: FONT_MONO }}>
                384 files · 12 conflicts resolved
              </div>

              {/* Mini bars: hourly throughput */}
              <div style={{ display: 'flex', alignItems: 'flex-end', gap: 3, marginTop: 12, height: 32 }}>
                {[20, 45, 12, 65, 80, 30, 55, 42, 88, 70, 95, 50, 30, 75, 60, 40, 92, 80, 65, 100, 55, 38, 70, 80].map((h, i) => (
                  <div key={i} style={{
                    flex: 1, height: `${h}%`,
                    background: i >= 20 ? t.primary : (theme === 'dark' ? 'rgba(255,250,240,0.18)' : 'rgba(30,24,16,0.14)'),
                    borderRadius: 1,
                  }} />
                ))}
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 9.5, color: t.ink3, fontFamily: FONT_MONO, marginTop: 4 }}>
                <span>00:00</span><span>12:00</span><span>now</span>
              </div>
            </div>

            {/* Pause-all card */}
            <div style={{
              background: t.surface, borderRadius: 10,
              border: `1px solid ${t.border}`,
              padding: '12px 14px',
              display: 'flex', alignItems: 'center', gap: 12,
            }}>
              <div style={{
                width: 36, height: 36, borderRadius: 8,
                background: t.primaryWeak, color: t.primary,
                display: 'flex', alignItems: 'center', justifyContent: 'center',
              }}>
                <Icon d={ICONS.bolt} size={16} color={t.primary} fill={t.primary} strokeWidth={0} />
              </div>
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ fontSize: 12.5, fontWeight: 700, color: t.ink }}>Sync running</div>
                <div style={{ fontSize: 11, color: t.ink3, fontFamily: FONT_MONO }}>↓ 142 KB/s · 3 active</div>
              </div>
              <Switch on={true} theme={theme} />
            </div>

            {/* Per-account mini stats */}
            <div>
              <div style={{ padding: '4px 4px 8px', fontSize: 10.5, fontWeight: 700, color: t.ink3, letterSpacing: 0.7 }}>
                ACCOUNTS
              </div>
              {[
                { kind: 'gdrive',   label: 'Work',          synced: '+ 642 MB', files: 184, status: 'live' },
                { kind: 'gdrive',   label: 'jane.dev',      synced: '+ 218 MB', files: 96,  status: 'ok' },
                { kind: 'onedrive', label: 'Personal',      synced: '+ 312 MB', files: 78,  status: 'live' },
                { kind: 'dropbox',  label: 'Side projects', synced: '+ 68 MB',  files: 26,  status: 'live' },
              ].map((a, i) => (
                <div key={i} style={{
                  padding: '10px 12px',
                  background: t.surface, borderRadius: 8,
                  border: `1px solid ${t.border}`,
                  marginBottom: 6,
                  display: 'flex', alignItems: 'center', gap: 10,
                }}>
                  <ProviderMark kind={a.kind} size={26} theme={theme} />
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                      <span style={{ fontSize: 12.5, fontWeight: 600, color: t.ink, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{a.label}</span>
                      {a.status === 'live' && <StatusDot tone="primary" theme={theme} pulse />}
                    </div>
                    <div style={{ fontSize: 10.5, color: t.ink3, fontFamily: FONT_MONO }}>{a.synced} · {a.files} files</div>
                  </div>
                </div>
              ))}
            </div>
          </aside>
        </div>
      </div>
    </WindowChrome>
  );
}

Object.assign(window, { OptionBTabs });
