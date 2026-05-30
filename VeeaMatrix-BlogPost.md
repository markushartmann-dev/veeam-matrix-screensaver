# VeeaMatrix — The Veeam-Themed Matrix Screensaver Nobody Asked For (But Everyone Needs)

*A love letter to green rain, Courier New, and data resilience.*

---

![VeeaMatrix Hero Screenshot](screenshots/hero.png)
*[📸 INSERT SCREENSHOT — fullscreen matrix rain with a Veeam popup visible]*

---

## What Is This?

Imagine the iconic Matrix "digital rain" — but instead of Japanese katakana, your screen fills with `IMMUTABILITY`, `RANSOMWARE RECOVERY`, `ZERO-TRUST RESILIENCE`, and `VEEAM DATA CLOUD VAULT` cascading down in glowing green. That's **VeeaMatrix**: a fully functional Windows screensaver (`.scr`) built entirely in C#, compiled without Visual Studio, and packed with more Veeam buzzwords than a Keynote slide deck.

It started as a side project. It became an obsession. Twenty-five versions later, it has a live preview, a multi-tab settings dialog, Light/Dark mode, five built-in color profiles, embedded banner support, and a 60+ term catalog covering everything from `VBR V13` to `AI TRiSM`.

---

## Features at a Glance

| Feature | Details |
|---|---|
| **Matrix rain** | GDI+ double-buffered, per-column speed variation, glyph scramble |
| **Veeam terms** | 60+ terms: products, features, buzzwords — all caps, proportionally sized |
| **Popup modes** | Fade, Glitch, Scan, Zoom, Scramble — randomly selected |
| **Color profiles** | 5 built-ins: Classic, Cyberpunk, Amber Retro, Blood Moon, Deep Space |
| **Watermark** | Logo + subtitle, bottom-right, optional, respects Bold/Italic setting |
| **Settings UI** | Three-column, 1836 px wide, live preview, Light/Dark mode toggle |
| **Banner image** | Compiled directly into the `.scr` — no sidecar file at runtime |
| **Multi-monitor** | One render surface per screen; primary drives the app lifecycle |
| **Font control** | Rain font, word font, popup font — size, face, Bold, Italic |
| **Custom terms** | Edit `terms.txt` in `%APPDATA%\VeeaMatrix\` |
| **Zero install** | Single `.scr` file; right-click → Install, or copy to `System32` |

---

## Screenshots

![Settings UI — Dark Mode](screenshots/settings-dark.png)
*[📸 INSERT SCREENSHOT — Settings dialog, Dark mode, banner visible in right column]*

![Settings UI — Light Mode](screenshots/settings-light.png)
*[📸 INSERT SCREENSHOT — Settings dialog, Light mode toggle active]*

![Color Profiles](screenshots/profiles.png)
*[📸 INSERT SCREENSHOT — profile dropdown open, Deep Space selected, preview showing violet rain]*

![Popup — Glitch Mode](screenshots/popup-glitch.png)
*[📸 INSERT SCREENSHOT — fullscreen with a glitching "RANSOMWARE RECOVERY" popup]*

---

## Color Profiles

Five built-in profiles — pick your mood:

| Profile | Rain | Popups | Vibe |
|---|---|---|---|
| **Classic** | Matrix green | Bright green | The original |
| **Cyberpunk** | Electric cyan | Hot magenta | Blade Runner energy |
| **Amber Retro** | Warm amber | Orange-yellow | Old-school terminal |
| **Blood Moon** | Deep crimson | Bright red | Threat-hunting mode |
| **Deep Space** | Vivid violet | Gold / bright violet | Galaxy brain, literally |

You can also define custom profiles — stored in `%APPDATA%\VeeaMatrix\profiles.ini`.

---

## The Term Catalog

The default catalog ships with 60+ Veeam-specific terms. A selection:

```
VEEAM DATA PLATFORM · VEEAM DATA CLOUD · VEEAM DATA CLOUD VAULT
BACKUP & REPLICATION · VBR · VBR V13 · VEEAM ONE
VEEAM RECOVERY ORCHESTRATOR · VEEAM SERVICE PROVIDER CONSOLE
HIGH AVAILABILITY · HA · HARDENED REPOSITORY · IMMUTABLE BACKUPS
AIR-GAPPED REPOSITORY · ZERO TRUST · ZERO-TRUST RESILIENCE
CYBER VAULT · RANSOMWARE RECOVERY · MALWARE DETECTION · THREAT HUNTING
RECON SCANNER · AGENT COMMANDER · SECURITI AI
DSPM, DSP & AI TRISM · VEEAM'S DATA COMMAND GRAPH
VDP PREMIUM · VDP ADVANCED · VDP ESSENTIALS
3-2-1-1-0 RULE · RPO · RTO · SLA
```

Want to customize? Edit (or replace) `%APPDATA%\VeeaMatrix\terms.txt` — one term per line.

---

## Technical Deep-Dive

> *For the curious. Skip if you just want the screensaver.*

### Single-file, no IDE

The entire screensaver is one `VeeaMatrix.cs` file (~2 100 lines), compiled via the .NET Framework 4.x command-line compiler:

```powershell
csc.exe /target:winexe /out:VeeaMatrix.scr /r:System.Windows.Forms.dll,System.Drawing.dll /optimize+ VeeaMatrix.cs
```

The build script (`Build-VeeaMatrix.ps1`) optionally embeds a banner image as a compiled resource — so the final `.scr` is truly self-contained.

### Rendering loop

Each frame:
1. Fill with a semi-transparent black overlay (creates the fade trail)
2. For each active column: draw the next glyph, advance position
3. Spawn new columns probabilistically; retire finished ones
4. Overlay any active word popups (with their own state machines: fade-in → hold → fade-out)
5. Draw watermark (bottom-right)
6. `BufferedGraphics.Render()` to screen

`StringFormat.GenericTypographic` gives precise per-character control; font sizes are derived from screen height so the effect scales cleanly across 1080p, 1440p, and 4K.

### Popup state machine

Each popup cycles through: **Idle → Appearing → Visible → Fading → Done**. The five popup modes (`Fade`, `Glitch`, `Scan`, `Zoom`, `Scramble`) differ in how the Appearing/Fading phases are rendered — glitch shuffles characters, scan draws a horizontal reveal bar, zoom scales from 0 → full size.

### Settings architecture

`ConfigForm` uses a `RebuildUI()` → `Build()` pattern: switching language or toggling Light/Dark mode simply writes the new value to the in-memory `Settings` object and recreates all controls from scratch. This keeps the theming logic in one place (13 color fields computed from `cur.DarkMode` at the top of `Build()`) without conditional branches scattered everywhere.

---

## How It Evolved

### Phase 1 — "Does this even work?" (v1.0–v1.10)

The project started with the simplest possible question: *can you compile a working Windows screensaver from a single C# file without Visual Studio?* The answer is yes — `csc.exe` ships with .NET Framework and produces a valid `.scr` directly. Early versions proved the rendering loop, established the columnar rain mechanics, and wired up the three standard screensaver entry points (`/s`, `/p`, `/c`).

### Phase 2 — Bringing it to life (v1.11–v1.18)

With a working renderer, attention turned to what makes VeeaMatrix *feel* different from a generic Matrix clone. Word streams were added — Veeam terms appearing in a contrasting color mid-rain — followed by full popup overlays with five distinct animation modes. Multi-monitor support landed, font controls were added, and the first color profiles (Classic, Cyberpunk, Amber Retro) appeared.

### Phase 3 — Settings UI & polish (v1.19–v1.22)

A proper three-column settings dialog replaced the minimal config form: live preview (880×495, 16:9), organized sections for Rain / Words / Popups / Watermark / Profiles, Light/Dark mode toggle, and banner image support compiled directly into the `.scr` via the build script. The form width settled at 1836 px — wide enough to show the preview at full size without crowding the controls.

### Phase 4 — Final refinements (v1.23–v1.25)

The term catalog was significantly expanded (VBR V13, HA, VDP tiers, Veeam Recovery Orchestrator, RECON SCANNER, AGENT COMMANDER, Securiti AI, DSPM/AI TRiSM, Veeam's Data Command Graph, Zero-Trust Resilience, and more). Bold/Italic font style flags were added and applied consistently across word streams, popups, *and* the watermark. The **Deep Space** color profile (vivid violet rain, gold words, pale lavender head glyphs) replaced an earlier profile that was too similar to Cyberpunk — now every built-in profile occupies genuinely distinct visual territory.

---

## Built with AI

This project was developed entirely through a conversation with Claude (Anthropic). No IDE, no StackOverflow tabs, no manual debugging sessions — just a chat window, a text editor, and `Build-VeeaMatrix.ps1`.

A few things that worked surprisingly well:
- **Iterative refinement**: describing a visual effect in natural language ("the glitch mode should scramble characters during the reveal, not just flicker opacity") and getting working GDI+ code back
- **Architecture decisions**: the `RebuildUI()` pattern, the `FontStyle` flags enum approach, the embedded-resource strategy — all emerged from back-and-forth discussion rather than upfront design
- **Staying in .NET 4.x**: no LINQ, no modern C# features, no NuGet — just classic WinForms and GDI+. The constraints actually kept the code clean

Total development time: a few evenings across about a week.

---

## Installation

**Quick start (no install):**
1. Download `VeeaMatrix.scr`
2. Double-click → screensaver runs immediately
3. Right-click → **Configure** for the settings dialog

**System-wide install:**
```
Right-click VeeaMatrix.scr → Install
```
Or copy manually to `C:\Windows\System32\` (requires Admin).

**Build from source:**
```powershell
# Clone / download the repo, then:
.\Build-VeeaMatrix.ps1
```
Requires .NET Framework 4.x (ships with Windows 10/11). Optionally place a `VeeaMatrix-banner.jpg` next to the script — it gets compiled into the `.scr`.

---

## What's Next

A few ideas on the backlog:

- **Audio** — a subtle low digital hum or typing sound (opt-in, obviously)
- **Custom color profile editor** — full GUI instead of editing `profiles.ini`
- **More popup modes** — typewriter reveal, vertical scan, matrix-decode effect
- **Per-monitor profiles** — different color scheme on each screen
- **Veeam v13 product name updates** — keep the term catalog current as the portfolio evolves

---

## Download

> **[📎 VeeaMatrix.scr]** — grab the latest compiled binary from the releases page, or build from source using the instructions above.

Source: [GitHub / your repo link here]

---

*Questions, feedback, pull requests? Drop them in the comments below or ping me on the Veeam Community Forums.*

*— Markus*

---

**Tags:** `screensaver` `veeam` `matrix` `csharp` `winforms` `fun-project` `AI-assisted`

---

### Cross-post note for community.veeam.com

> **TL;DR for the Veeam Community Forums:**
>
> I built a Matrix-style screensaver where the falling glyphs spell out Veeam product names, features, and buzzwords — `IMMUTABILITY`, `ZERO-TRUST RESILIENCE`, `VBR V13`, `VEEAM DATA CLOUD VAULT`, and 60+ more. It's a single `.scr` file, no install required, with a full settings UI, five color profiles, and live preview. Built entirely with C# + `csc.exe` (no Visual Studio) through an AI-assisted development process.
>
> **Download / source:** [link]
>
> Happy to answer questions about the build process or the rendering approach — it's a fun rabbit hole if you're into WinForms/GDI+ or just want something ridiculous on your office monitor.
