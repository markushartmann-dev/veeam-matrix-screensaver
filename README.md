# VeeaMatrix — The Veeam-Themed Matrix Screensaver

A fully-functional Windows screensaver (`.scr`) where the Matrix digital rain spells out Veeam product names, features, and buzzwords. `IMMUTABILITY`, `ZERO-TRUST RESILIENCE`, `VBR V13`, `VEEAM DATA CLOUD VAULT` — 70+ terms cascading down your screen in glowing green (or violet, or teal, or crawl-yellow…).

![VeeaMatrix Screensaver](PNGs/banner.png)

## Screenshots

| Matrix Rain | Star Wars CRAWL Intro |
|---|---|
| ![Settings – Matrix Rain](PNGs/settings-matrix.png) | ![Settings – CRAWL Mode](PNGs/settings-crawl.png) |

![CRAWL Intro Phase](PNGs/crawl-intro.png)

---

## Download

| Option | When to use |
|---|---|
| 👉 **[VeeaMatrix.scr](https://github.com/markushartmann-dev/veeam-matrix-screensaver/raw/main/VeeaMatrix.scr)** | Direct download |

Single `.scr` file — no installer, no dependencies. .NET Framework 4.x ships with every Windows 10/11 system.

---

## ⚠️ Windows / Browser blocks the file — step-by-step fix

`VeeaMatrix.scr` is not commercially code-signed. Windows and browsers will flag it — here is exactly what to do in each situation:

---

### 🔴 Browser blocks the .scr download (Edge / Chrome)

You see: *"VeeaMatrix.scr wird häufig nicht heruntergeladen"*

- **Edge:** Click **`…`** next to the file → **Beibehalten** → **Trotzdem beibehalten**
- **Chrome:** Click the **arrow ▾** → **Keep** / **Trotzdem behalten**

➡️ Then unblock the file via Properties (see below) before running.

---

### 🔴 Unblock via file Properties — always do this before first run

> Right-click `VeeaMatrix.scr` → **Eigenschaften / Properties** → tick **Zulassen / Unblock** → **OK**

This removes the "downloaded from internet" flag and prevents the Defender warning.

---

> 🔍 **Why does this happen?** Windows SmartScreen blocks unsigned `.scr` / `.exe` files from the internet. The full source code of VeeaMatrix is publicly available in this repo — nothing is hidden.

---

## Installation

1. **Download** (see options above) and **unblock** the `.scr` file
2. **Right-click** `VeeaMatrix.scr`:
   - **Installieren / Install** — registers as system screensaver (requires admin)
   - **Testen / Test** — runs fullscreen immediately
   - **Konfigurieren / Configure** — opens the settings dialog

   Or double-click to run immediately.

---

## Features

| Feature | Details |
|---|---|
| **Matrix Rain** | 7 word effects: Scroll, Fade, Build, Scramble, Scan, Zoom, Glitch |
| **Star Wars Intro** | Star Wars–style perspective scroll with static intro phase |
| **CRAWL templates** | Episode IV, Spaceballs, Veeam Edition — fully editable |
| **Dual banner** | Matrix-style banner in Word Stream mode, Jedi banner in CRAWL mode — both hard-embedded |
| **Color profiles** | 7 built-ins: Veeam, Cyberpunk, Amber CRT, Deep Space, Aurora, Star Wars, Hello Kitty |
| **Color picker** | 2 consolidated buttons — Color + Head (bright) — apply to all layers at once |
| **No RAIN** | Suppress background rain during word effects or CRAWL |
| **Watermark** | Logo + subtitle, bottom-right, optional |
| **Settings UI** | Three-column, 1556 px wide, live preview, Light/Dark mode toggle |
| **Banner image** | Two banners hard-embedded as Base64 — switches automatically with mode |
| **Credits button** | Clickable tribute with LinkedIn + blog links |
| **Multi-monitor** | One render surface per screen; primary drives the app lifecycle |
| **Font control** | System font picker, size, Bold, Italic — shared across all word effects |
| **Custom terms** | Edit the built-in catalog per session or save to `%APPDATA%\VeeaMatrix\` |
| **Zero install** | Single `.scr` file; right-click → Install, or copy to `System32` |

---

## Color Profiles

Seven built-in profiles — pick your mood:

| Profile | Rain | Head | Vibe |
|---|---|---|---|
| **Veeam** | Veeam green | White | The original |
| **Cyberpunk** | Electric cyan | Yellow | Blade Runner energy |
| **Amber CRT** | Warm amber | Pale yellow | Old-school terminal |
| **Deep Space** | Vivid violet | Pale lavender | Galaxy brain, literally |
| **Aurora** | Emerald-teal | Icy blue | Nordic lights |
| **Star Wars** | Crawl yellow | Pale yellow-white | May the Schwartz be with you |
| **Hello Kitty** | Hot pink | Gold | Chaos |

Custom profiles are stored in `%APPDATA%\VeeaMatrix\profiles.ini`.

---

## Word Effects

Seven distinct effects in a single unified selector:

| Effect | Mode | Direction |
|---|---|---|
| **Scroll** | Word Stream | All 4 directions |
| **Fade** | Stream + Popup | — |
| **Build** | Word Stream | Left / Right |
| **Scramble** | Stream + Popup | Left / Right |
| **Scan** | Popup-style | Left / Right |
| **Zoom** | Popup-style | — |
| **Glitch** | Stream + Popup | — |

---

## CRAWL Mode

A fully perspective-projected Star Wars–style text crawl with:

- **Static intro** — first paragraph fades in centered on screen, just like *"A long time ago…"*
- **Perspective scroll** — remaining text with full 3D projection (tilt, focal length, per-line scale + color gradient)
- **Star-field background** — optional starfield behind the crawl text
- **No RAIN** — background rain automatically suppressed when CRAWL is active

Three built-in templates: **Episode IV**, **Spaceballs**, and **Veeam Edition** (*Episode XIII: The Rise of Cyber Resilience*). All fully editable with Save/Load support.

---

## Term Catalog

70+ Veeam-specific terms ship with the screensaver. A selection:

```
VEEAM DATA PLATFORM · VEEAM DATA CLOUD · VEEAM DATA CLOUD VAULT
BACKUP & REPLICATION · VBR · VBR V13 · VEEAM ONE
VEEAM RECOVERY ORCHESTRATOR · KASTEN BY VEEAM · COVEWARE BY VEEAM
HIGH AVAILABILITY · HARDENED REPOSITORY · IMMUTABLE BACKUPS
AIR-GAPPED REPOSITORY · ZERO TRUST · CYBER VAULT
RANSOMWARE RECOVERY · MALWARE DETECTION · THREAT HUNTING
RECON SCANNER · AGENT COMMANDER · SECURITI AI
DSPM, DSP & AI TRISM · VEEAM'S DATA COMMAND GRAPH
VDP PREMIUM · VDP ADVANCED · VDP ESSENTIALS
3-2-1-1-0 RULE · RPO · RTO · SLA · ALWAYS-ON DATA
```

Customize via **Adjust catalog with built-in terms** in the MISCELLANEOUS section — edit inline per session, or save permanently to `%APPDATA%\VeeaMatrix\terms.txt`.

---

## Build from Source

Requires Windows 10/11 (`.NET Framework 4.x` is pre-installed).

```powershell
.\Build-VeeaMatrix.ps1
```

Compiles `VeeaMatrix.cs` with the built-in `csc.exe` — no Visual Studio, no SDK. Output: `VeeaMatrix.scr`.

To embed a custom banner image, place `VeeaMatrix-banner.jpg` next to the script before building. The banner is also hardcoded as a Base64 fallback in the source, so the distributed `.scr` always shows it.

---

## Multi-Monitor

Runs on **all connected monitors simultaneously**. The primary monitor drives the app lifecycle.

---

## Changelog

| Version | Highlights |
|---|---|
| **v1.87** | Random seed per screen — each monitor shows different names and terms |
| **v1.81–v1.86** | **Settings Profiles** (save/load full settings as `.ini`); Credits popup with LinkedIn, Blog & GitHub links; cinema letterbox bars on both banners; mode buttons (DE+EN); UI 5% shorter; Term Catalog dark/light mode; CRAWL font and intro phase respect Bold / Italic settings |
| **v1.71–v1.80** | **Dual banner** (Matrix ↔ Jedi depending on mode, hard-embedded); left column narrower (420 px, fw=1556 px); `.scr` compressed from 1.9 MB → 766 KB; column alignment fixes; DE/EN labels throughout |
| **v1.70** | Major UX overhaul — Star Wars CRAWL intro phase; 7-effect unified selector; consolidated No RAIN checkbox; persistence fixes; Light mode as default |
| **v1.60–v1.69** | CRAWL perspective scroll; full layout restructure; PopupHideRain; 3-column settings UI polish |
| **v1.35–v1.56** | CRAWL mode (Star Wars–style scroll + Veeam Edition template); **Star Wars** color profile; color picker consolidated to 2 buttons; Word Mode selector |
| **v1.19–v1.28** | Three-column settings UI with live 16:9 preview; Light/Dark toggle; **Aurora** profile; banner hard-embedded as Base64; multi-monitor support; content isolation |
| **v1.0–v1.18** | Initial release — Matrix rain + word drops + popups; font controls; color profiles; five popup animation modes |

---

## License

MIT — free to use, modify, and distribute.

---

*Built entirely through an AI-assisted development process (Claude by Anthropic) — no IDE, no StackOverflow, just a chat window and `csc.exe`.*
