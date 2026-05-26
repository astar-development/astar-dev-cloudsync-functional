# Handoff — AStar Dev Cloud Sync · Main Window (Option A)

Cross-platform desktop app (Avalonia UI / .NET) that lets a user connect
multiple cloud accounts (OneDrive, Google Drive, Dropbox) and pick which
folders sync. This handoff is for the **main window** layout the team
selected after a three-option exploration.

---

## About the design files

The files under `preview/` are **HTML/React design references** — clickable
prototypes showing intended look and behaviour. They are **not** production
code to copy. Your job is to **recreate the chosen layout in Avalonia
(XAML + C#)** using the codebase's existing patterns (MVVM, styles,
resources, etc.).

The reference doc was built with React purely because it's a fast medium
for showing layout intent. Every Flex/Grid in the JSX maps cleanly onto
Avalonia's `Grid` / `DockPanel` / `StackPanel` primitives.

Open `preview/AStar Cloud Sync - Layout Options.html` in a browser to see
all three explored variants side-by-side on the design canvas. Use the
Tweaks panel (toolbar, top-right) to toggle light/dark. Option A is the
chosen one.

Reference screenshots of the chosen layout (full 1280 × 820, both themes)
live in `screenshots/`:

- `screenshots/option-a-light.png`
- `screenshots/option-a-dark.png`

## Fidelity

**High-fidelity.** Colors, spacing, type, and layout are intentional. The
implementation should match the prototype's measurements within a few
pixels. The visual brand (indigo primary, saffron accent, type pairing) is
new to this project — there is no existing design system to defer to, so
use the tokens in this doc as the source of truth.

---

## The chosen layout — Option A · Sidebar

Two-column main window. The left rail is the accounts hub; the right pane
is the work surface for whichever account is selected.

```
┌─────────────────────────────────────────────────────────────────────┐
│  ☆ AStar Dev Cloud Sync                       —  □  ✕   (titlebar) │
├──────────────────────┬──────────────────────────────────────────────┤
│ workspace header     │  account header (icon · name · email · pill) │
│ search ⌘K            │                                              │
│                      ├──────────────────────────────────────────────┤
│ ACCOUNTS         (4) │  sub-tabs: Folders · Activity · Conflicts ·… │
│  ▸ gdrive   Work  ●  │                                              │
│  ▸ gdrive   jane.dev │  toolbar: breadcrumb · selection chip · …    │
│  ▸ onedrive Personal │                                              │
│  ▸ dropbox  Side prj │  ┌──── folder tree ──────────────────────┐   │
│  + Add account       │  │ ▾ ☑ Documents       14    4.2 GB now  │   │
│                      │  │   ☑ Contracts 2025   2    218 MB 2 m  │   │
│  ┌─ TODAY ────────┐  │  │ ▾ ◧ Engineering     21   18.9 GB 40s  │   │
│  │ 1.24 GB · 384  │  │  │   ☑ src/           142    2.1 GB now  │   │
│  │ ▮▮▮▮▮▮▮▮▮▮▮▮▮▮ │  │  │   ☐ archive/        12   14.2 GB —    │   │
│  │ 00:00  12:00 ⏵ │  │  │   ☑ sandbox/         8    412 MB 4h   │   │
│  └────────────────┘  │  │ … etc                                 │   │
│                      │  └───────────────────────────────────────┘   │
│ ⏸ Pause all   🔔 ⚙   ├──────────────────────────────────────────────┤
│                      │  ● healthy  ↑ 0 B/s  ↓ 142 KB/s  3 in queue │
└──────────────────────┴──────────────────────────────────────────────┘
```

### Window

- **Default size:** 1280 × 820 (resizable; min 960 × 600 recommended).
- **Titlebar:** 38px tall. App icon + name on the left. Right side has
  three 22×22 round buttons (minimize, maximize, close). Use Avalonia's
  custom titlebar (`Window.ExtendClientAreaToDecorationsHint=true`) so
  the design works identically across Windows / macOS / Linux. The
  prototype shows the Linux-style right-aligned controls; per-OS variants
  are fine if your team prefers native placement.

### Column layout

- **Left rail (sidebar):** fixed 286 px, background = surface-alt.
- **Right pane (main):** fills remaining width, background = bg.

### Sidebar — top to bottom

1. **Workspace header** — 14 px padding. 30×30 avatar (rounded 8 px,
   primary color, monogram), workspace name (13 px / 700) + subtitle
   "4 accounts · 5.2 TB total" (11 px / mono / ink-3), chevron-down.
2. **Search bar** — 30 px tall pill, surface bg, 1 px border, search
   glyph + placeholder "Search accounts & folders…" + ⌘K hint.
3. **ACCOUNTS header** — 10.5 px / 700 / letter-spacing 0.8 / ink-3,
   "ACCOUNTS" + count on the right.
4. **Accounts list** (scrolls) — each row:
   - 8 px vertical padding, 7 px corner radius, 2 px left accent border
     (primary color when selected, transparent otherwise).
   - Provider mark (26 px), account name (12.5 px / 600) + status dot,
     email (10.5 px / mono / ink-3).
   - Below: storage bar (3 px tall, provider hue) + `used / total GB` +
     `N folders` row in 10 px mono.
5. **"+ Add account…" button** — full-width dashed-border (border-strong),
   `+` icon, ink-3 text.
6. **TODAY card** — pinned below the scroll region:
   - Bordered, 8 px radius, surface bg, 10/12 px padding.
   - Header row: "TODAY" label (10 px / 700 / letter-spacing 0.7) ·
     right-aligned "↓ 142 KB/s" (10 px / mono / primary).
   - Big number: "1.24" (18 px / 700 / mono) + "GB · 384 files"
     (11 px / 500 / ink-2).
   - 24-bar histogram (28 px tall, 2 px gap, 1 px radius). Bars 0–19
     use ink-3 at 14–16 % opacity (history); bars 20–23 use primary
     (current period).
   - Axis labels: `00:00  12:00  now` (9.5 px / mono / ink-3).
7. **Sidebar footer** — 1 px top border. "Pause all" ghost button (with
   pause glyph), spacer, bell glyph button, settings glyph button.

### Main pane — top to bottom

1. **Account header** — 18/28 px padding, 1 px bottom border:
   - 42 px provider mark.
   - Title row: account name (19 px / 700 / -0.3 letter-spacing) ·
     provider name in mono (12 px / ink-3) · status pill ("Syncing"
     primary tone with pulsing dot, or "All synced" good tone).
   - Email (12.5 px / mono / ink-3).
   - Right side: `Pause`, `Settings`, `⋯` buttons.
2. **Sub-tabs** — 12/28 px padding, 1 px bottom border.
   Labels: `Sync folders` · `Activity` · `Conflicts` · `Settings`.
   Active tab: 700 weight, ink text, 2 px primary underline.
   Conflicts gets a warn-tone badge with the count (`2`).
3. **Toolbar above tree** — 10/28 px padding, bg, 1 px bottom border:
   - Breadcrumb: `~/AStar / <account name>` (11 px / mono).
   - Right side: selection summary pill (`8 of 24 folders · 22.6 GB`),
     `Filter` ghost button, `Apply changes` primary button.
4. **Folder tree** (scrolls) — 6-column grid:
   `20 px · 22 px · 1fr · 90 px · 110 px · 70 px`
   - Column headers: 10.5 px / 700 uppercase / ink-3 — `Folder`,
     `Items` (right), `Size` (right), `Updated` (right).
   - Rows: 9/14 px padding, 6 px radius, indent = `14 + depth × 16` px.
     A `syncing` row uses a tinted background (light: `oklch(0.97 0.025 268)`;
     dark: `rgba(120,100,255,0.06)`).
   - Each row: expand chevron (right when collapsed, down when expanded),
     checkbox (off / on / mixed), folder glyph (uses primary color when
     selected, ink-3 when off, weak-primary fill when selected), name
     (12.5 px / 600 for top-level, 500 for children — switch to mono if
     name ends in `/`), and the three right-aligned mono numbers.
   - When a row is actively syncing, add a small "syncing" pill with a
     pulsing dot beside the folder name.
5. **Status bar** — 30 px tall, surface-alt bg, 1 px top border.
   All 11 px / mono / ink-3:
   `● All accounts healthy   ↑ 0 B/s   ↓ 142.4 KB/s (primary color)   3 files in queue · 188 MB   …   v2.4.1 · linux-x64`

---

## Design tokens

Drop these into `App.axaml` (or your central resource dictionary) so XAML
files can bind by key.

### Color — light theme

| Token            | Value                  | Use                                      |
|------------------|------------------------|------------------------------------------|
| `bg`             | `#f5f3ef`              | Main pane background                     |
| `surface`        | `#ffffff`              | Cards, raised panes                      |
| `surfaceAlt`     | `#faf8f4`              | Sidebar, status bar, inset rows          |
| `border`         | `rgba(30,24,16,0.10)`  | 1 px dividers                            |
| `borderStrong`   | `rgba(30,24,16,0.18)`  | Dashed borders, button outlines          |
| `ink`            | `#15120e`              | Primary text                             |
| `ink2`           | `#3a342c`              | Secondary text                           |
| `ink3`           | `#7a7268`              | Tertiary text, mono captions             |
| `primary`        | `oklch(0.52 0.20 268)` | Brand indigo · buttons, links, accents   |
| `primaryWeak`    | `oklch(0.96 0.03 268)` | Tinted backgrounds (syncing rows, pills) |
| `accent`         | `oklch(0.78 0.16 75)`  | Warm saffron — used very sparingly       |
| `good`           | `oklch(0.62 0.16 152)` | Healthy / completed                      |
| `warn`           | `oklch(0.74 0.16 60)`  | Conflicts, near-full storage             |
| `danger`         | `oklch(0.58 0.20 25)`  | Destructive actions                      |
| `chrome`         | `#eae6df`              | Titlebar                                 |
| `chromeInk`      | `#2a251f`              | Titlebar text                            |

### Color — dark theme

| Token            | Value                    |
|------------------|--------------------------|
| `bg`             | `#0e0c0a`                |
| `surface`        | `#1a1714`                |
| `surfaceAlt`     | `#141210`                |
| `border`         | `rgba(255,250,240,0.08)` |
| `borderStrong`   | `rgba(255,250,240,0.16)` |
| `ink`            | `#f5f1ea`                |
| `ink2`           | `#bdb6ab`                |
| `ink3`           | `#827a6f`                |
| `primary`        | `oklch(0.72 0.18 268)`   |
| `primaryWeak`    | `oklch(0.28 0.10 268)`   |
| `accent`         | `oklch(0.82 0.15 75)`    |
| `good`           | `oklch(0.72 0.16 152)`   |
| `warn`           | `oklch(0.80 0.16 60)`    |
| `danger`         | `oklch(0.70 0.20 25)`    |
| `chrome`         | `#08070680`              |
| `chromeInk`      | `#d8d2c6`                |

Both palettes are in `preview/theme.jsx` under the `TOKENS` const if you
want to copy-paste verbatim. Avalonia does not parse `oklch()` directly —
either convert to sRGB hex at build time (recommended) or supply pre-computed
hex values per theme. The sRGB equivalents:

| Token             | Light sRGB | Dark sRGB |
|-------------------|------------|-----------|
| `primary`         | `#5046C8`  | `#9E91FF` |
| `primaryWeak`     | `#EEEBFA`  | `#2C2160` |
| `accent`          | `#D49333`  | `#E1A03D` |
| `good`            | `#3F9663`  | `#5DBA84` |
| `warn`            | `#C89132`  | `#DCA63D` |
| `danger`          | `#C44A37`  | `#E47961` |

### Provider accent hues

Each connected provider gets its own hue used in the storage bar and folder
glyph — distinct from the providers' real brand colors so we don't infringe.

| Provider     | Hue                    | Weak (light)            | Weak (dark)             |
|--------------|------------------------|-------------------------|-------------------------|
| OneDrive     | `oklch(0.55 0.18 250)` | `oklch(0.94 0.04 250)`  | `oklch(0.28 0.08 250)`  |
| Google Drive | `oklch(0.70 0.18 145)` | `oklch(0.94 0.04 145)`  | `oklch(0.28 0.08 145)`  |
| Dropbox      | `oklch(0.62 0.18 220)` | `oklch(0.94 0.04 220)`  | `oklch(0.28 0.08 220)`  |

### Typography

| Role              | Family              | Weight  | Size      |
|-------------------|---------------------|---------|-----------|
| Display (h1)      | Plus Jakarta Sans   | 700     | 19 / 26 px |
| Section header    | Plus Jakarta Sans   | 700     | 10–11 px, uppercase, letter-spacing 0.7 |
| UI label / button | Plus Jakarta Sans   | 600     | 12–13 px  |
| Body              | Plus Jakarta Sans   | 500     | 12.5 px   |
| Caption           | Plus Jakarta Sans   | 500     | 11 px     |
| Mono / numeric    | JetBrains Mono      | 500     | 10–12 px  |

Both fonts are free OFL (bundle the .ttf in `Assets/Fonts/`). System
fallback: `Segoe UI` (Win), `-apple-system` (mac), `Inter` (Linux).

### Spacing scale

`2 · 4 · 6 · 8 · 10 · 12 · 14 · 16 · 18 · 20 · 24 · 28 · 32`

Mostly 8/12/14/16/28 in the actual layout.

### Border radius

| Use                          | Radius |
|------------------------------|--------|
| Small chips, checkbox        | 4 px   |
| Buttons (sm)                 | 6 px   |
| Buttons (md), pills, cards   | 7–8 px |
| Provider mark (relative)     | 26 % of size |
| Pills, switches, dots        | 999 px (full) |

### Shadows

Window frame only. Inside the app, prefer 1 px borders over shadows.
The sidebar's TODAY card has no shadow — it's a bordered surface.

---

## Components to build

Map the JSX components in `preview/option-a-sidebar.jsx` and `preview/theme.jsx`
to Avalonia user controls. Suggested decomposition:

- **`AccountListItem`** — row in the accounts list. Selected state =
  background tint + left accent border. Bind to `Account` view-model
  (kind, name, email, used, total, folders, status, isSelected).
- **`StorageBar`** — thin progress bar; takes `Used`, `Total`, `Color`.
- **`ProviderMark`** — rounded square with provider initial + accent dot.
  Pure decoration; takes `Kind` ("onedrive" / "gdrive" / "dropbox") and
  `Size`.
- **`StatusDot`** — colored dot with optional pulse animation.
  Pulse = `Storyboard` on `Opacity` and `RenderTransform.ScaleX/Y`,
  duration 1.4s, repeating.
- **`StatusPill`** — small rounded-rectangle label, tone variants
  (neutral, good, warn, primary, accent, danger).
- **`AppButton`** — kinds: primary, accent, ghost, subtle, danger.
  Sizes: sm (h26), md (h32), lg (h38). Supports leading + trailing
  glyphs.
- **`TriStateCheckbox`** — off / on / mixed. Primary fill when on or
  mixed; mixed shows a horizontal bar instead of a check.
- **`FolderTreeRow`** — the 6-column grid row; takes a `FolderNode`
  with depth, state, children count, size, updated, isSyncing.
- **`ThroughputCard`** — the TODAY card. Takes 24 bucket values + the
  totals + current rate. The histogram is plain `Rectangle` shapes in
  a `Grid` or `UniformGrid` — no charting library needed.
- **`AccountHeader`** — large header at the top of the main pane.
- **`SubTabBar`** — flat tabs with primary-color underline on active.
- **`StatusBar`** — bottom strip; tabular numeric throughput readouts.

For iconography, the JSX uses inline 16-px SVG paths (`ICONS` const in
`theme.jsx`). The same paths can be rendered in Avalonia via `PathIcon`
+ `StreamGeometry.Parse(...)`, or you can import an icon set such as
**Lucide.Avalonia** which already includes everything used here
(folder, file, check, pause, play, plus, search, settings, chevron-right,
chevron-down, arrow-up, arrow-down, sync, alert-triangle, user, bell,
zap, more-horizontal, cloud, filter, x).

---

## Interactions & behaviour

- **Selecting an account** in the left rail updates the entire main pane
  (header, sub-tabs, folder tree). Selection is single-select and
  persistent across launches.
- **Account list pulse:** the status dot pulses (1.4 s ease-in-out,
  scale 1 → 1.18 → 1, opacity 1 → 0.55 → 1) whenever the account is
  actively syncing.
- **Folder tree:**
  - Click chevron to expand/collapse. Children render with 16 px extra
    left indent per depth level.
  - Click checkbox to toggle sync. Parent state becomes `mixed` whenever
    any child is `on` and any is `off`. Toggling a `mixed` parent goes
    to `on` (selects all children); toggling again goes to `off`.
  - The `Apply changes` button in the toolbar is disabled until at
    least one folder's selection has changed; on click, persist + kick
    off a delta sync.
  - A row that is currently transferring shows a tinted background and
    a pulsing "syncing" pill next to the name.
- **Pause all** (sidebar footer) and **Pause** (main pane header) are
  separate scopes: global vs current-account. Both toggle their label
  to `Resume` when paused, and the affected status dots/pills shift to
  the `idle` / "Paused" treatment.
- **Add account…** opens the OAuth flow for OneDrive / Google Drive /
  Dropbox — not designed in this handoff; coordinate with the provider
  authentication spike.
- **⌘K / Ctrl-K** focuses the search field in the sidebar.
- **Theme toggle:** light/dark switch lives in the global Settings;
  bind to a `ThemeVariant` resource. The prototype defaults to dark
  per the user's preference.
- **Status bar:** numbers update at 500 ms cadence. Use tabular-number
  font feature on JetBrains Mono (it's the default) to prevent jitter.

---

## State management (suggested view-models)

```
WorkspaceViewModel
  ├── ObservableCollection<AccountViewModel> Accounts
  ├── AccountViewModel? SelectedAccount
  ├── ThroughputSnapshot TodaySnapshot   // 24 buckets + totals + rate
  ├── bool IsGlobalPaused
  ├── string SearchQuery
  └── ICommand PauseAllCommand, AddAccountCommand, …

AccountViewModel
  ├── ProviderKind Kind                  // OneDrive | GoogleDrive | Dropbox
  ├── string Name, Email
  ├── long UsedBytes, TotalBytes
  ├── int FolderCount
  ├── SyncStatus Status                  // Ok | Syncing | Warn | Paused
  ├── ObservableCollection<FolderNode> Folders   // hierarchical
  ├── ICommand PauseCommand, OpenSettingsCommand
  └── …

FolderNode
  ├── string Path, Name
  ├── int Depth, ChildCount
  ├── long SizeBytes
  ├── DateTimeOffset LastSync
  ├── CheckState SelectionState          // Off | On | Mixed
  ├── bool IsExpanded, IsSyncing
  └── ObservableCollection<FolderNode> Children
```

Reactive sync feeds from the OneDrive / Google Drive / Dropbox client
SDKs should update these view-models on the UI thread.

---

## Files in this bundle

```
design_handoff_cloud_sync/
├── README.md                            ← this file
├── screenshots/
│   ├── option-a-light.png               ← chosen layout, light theme (1280×820)
│   └── option-a-dark.png                ← chosen layout, dark theme  (1280×820)
└── preview/
    ├── AStar Cloud Sync - Layout Options.html   ← open in browser
    ├── theme.jsx                                 ← tokens, ProviderMark, Button, …
    ├── option-a-sidebar.jsx                      ← the chosen layout
    ├── option-b-tabs.jsx                         ← alternative (not chosen)
    ├── option-c-cards.jsx                        ← alternative (not chosen)
    ├── design-canvas.jsx                         ← canvas host
    └── tweaks-panel.jsx                          ← light/dark toggle host
```

The alternatives are kept around in case the team revisits a sub-area
(e.g. the activity feed in Option B is a good reference for the
"Activity" sub-tab in Option A).

## Out of scope for this handoff

- Add-account / OAuth onboarding flow
- Per-file conflict resolution UI
- Bandwidth / schedule / sync-direction settings
- Notification center
- Settings & preferences pages
- First-run / empty state

Reach out before implementing any of those — they have their own
constraints that ought to be designed first.
