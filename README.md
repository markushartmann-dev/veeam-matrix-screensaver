# VeeaMatrix — The Veeam-Themed Matrix Screensaver

A fully-functional Windows screensaver (`.scr`) where the Matrix digital rain spells out Veeam product names, features, and buzzwords. `IMMUTABILITY`, `ZERO-TRUST RESILIENCE`, `VBR V13`, `VEEAM DATA CLOUD VAULT` — 60+ terms cascading down your screen in glowing green (or violet, or teal, or amber…).

![VeeaMatrix Screensaver](.github/preview.png)

## Download

👉 **[Latest Release → VeeaMatrix.scr](../../releases/latest)**

Single `.scr` file — no installer, no dependencies. .NET Framework 4.x ships with every Windows 10/11 system.

---

## Installation

1. **Download** `VeeaMatrix.scr`
2. **Right-click** the file:
   - **Install** — registers as system screensaver (requires admin)
   - **Test** — runs fullscreen immediately
   - **Configure** — opens the settings dialog

   Or double-click to run it directly.

> **⚠️ Windows SmartScreen warning?**
> Because this file is not commercially code-signed, Windows Defender SmartScreen may block it on first run.
> **Fix:** Right-click `VeeaMatrix.scr` → **Properties** → tick **Unblock** at the bottom → **OK** — then run normally.
> Alternatively click **"Weitere Informationen"** (More info) in the SmartScreen dialog → **"Trotzdem ausführen"** (Run anyway).

---

## Features

| Feature | Details |
|---|---|
| **Matrix rain** | GDI+ double-buffered, per-column speed variation, glyph scramble |
| **Veeam terms** | 60+ terms: products, features, buzzwords — proportionally sized |
| **Popup modes** | Fade · Glitch · Scan · Zoom · Scramble — randomly selected |
| **Color profiles** | 6 built-ins + unlimited custom profiles |
| **Watermark** | Logo + subtitle, bottom-right, optional |
| **Settings UI** | Three-column, 1836 px wide, live preview, Light/Dark mode toggle |
| **Banner image** | Hardcoded into the `.scr` — always visible, no sidecar file needed |
| **Multi-monitor** | One render surface per screen |
| **Font control** | Rain / word / popup font — size, face, Bold, Italic |
| **Veeam 100 Names** | Stream Vanguard / Legend / MVP 2026 member names |
| **Custom terms** | Edit `%APPDATA%\VeeaMatrix\terms.txt` — one term per line |
| **Zero install** | Single `.scr` — right-click → Install, or copy to `System32` |

---

## Color Profiles

Six built-in profiles — pick your mood:

| Profile | Rain | Popups | Vibe |
|---|---|---|---|
| **Veeam** | Veeam green `#00B336` | Bright green | The official look |
| **Hello Kitty** | Hot pink | Gold | Unhinged office energy |
| **Cyberpunk** | Electric cyan | Hot magenta | Blade Runner |
| **Amber CRT** | Warm amber | Orange-yellow | Retro terminal |
| **Deep Space** | Vivid violet | Gold / bright violet | Galaxy brain |
| **Aurora** | Emerald teal | Neon mint | Northern lights |

Custom profiles are saved to `%APPDATA%\VeeaMatrix\profiles.ini`.

---

## Settings

Right-click → **Configure**:

| Setting | Description |
|---|---|
| **Language** | EN / DE toggle |
| **Color profile** | Built-in or saved custom profile |
| **Rain / Word / Popup colors** | Per-layer color control |
| **Font** | Any installed font, with live preview |
| **Font sizes** | Rain (8–36 px) · Words (8–36 px) · Popup (10–72 px) |
| **Speed** | Global (0.1× – 3.0×) plus separate word and popup multipliers |
| **Trail length** | How long characters glow before fading |
| **Direction** | TopDown / BottomUp / LeftRight / RightLeft, independently for rain and words |
| **Word style** | Scroll · Fade · Build · Scramble · Blink |
| **Word mode** | Rain only · Popup only · Both |
| **Word / Popup count** | 1–30 streams, 1–20 popups |
| **Popup effects** | Enable/disable Fade · Glitch · Scan · Zoom · Scramble individually |
| **Scanlines** | Subtle CRT overlay |
| **Watermark** | Customisable logo text + subtitle line |
| **Veeam 100 Names** | Toggle Vanguard / Legend / MVP names in streams |
| **Built-in terms** | Toggle the default 60+ Veeam term catalog |
| **Custom words** | Comma-separated extra terms |

> When all Veeam sources (Built-in terms, Veeam 100 Names, Custom words) are disabled, the screen shows pure matrix rain — no Veeam content at all.

Settings are saved to `%APPDATA%\VeeaMatrix\config.ini`.

---

## Term Catalog

60+ Veeam-specific terms ship with the screensaver. A selection:

```
VEEAM DATA PLATFORM · VEEAM DATA CLOUD · VEEAM DATA CLOUD VAULT
BACKUP & REPLICATION · VBR · VBR V13 · VEEAM ONE
VEEAM RECOVERY ORCHESTRATOR · VEEAM SERVICE PROVIDER CONSOLE
HARDENED REPOSITORY · IMMUTABLE BACKUPS · AIR-GAPPED REPOSITORY
ZERO TRUST · ZERO-TRUST RESILIENCE · CYBER VAULT
RANSOMWARE RECOVERY · MALWARE DETECTION · RECON SCANNER
3-2-1-1-0 RULE · RPO · RTO · SLA
```

Customise: edit `%APPDATA%\VeeaMatrix\terms.txt` — one term per line.

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
| **v1.28** | New **Aurora** color profile (emerald teal / neon mint northern-lights look) |
| **v1.27** | Banner always visible — hardcoded Base64 fallback in source + embedded resource + sidecar file; GDI+ stream/GC bug fixed; settings UI shows placeholder when no banner found; build script warns if no banner image present |
| **v1.26** | Content isolation: disabling all Veeam sources now produces pure matrix rain with zero Veeam terms |
| **v1.25** | Expanded term catalog (VBR V13, HA, VDP tiers, RECON SCANNER, AI TRiSM, …); Bold/Italic flags applied to word streams, popups and watermark; **Deep Space** profile replaces earlier duplicate |
| **v1.24** | Wider banner column, watermark font style control |
| **v1.23** | Font Bold/Italic style option |
| **v1.22** | Light/Dark theme toggle in settings, banner embedded in `.scr` via build script |
| **v1.19** | Three-column settings UI with live preview (880×495), banner fill-crop |
| **v1.16** | Multi-monitor support, five popup animation modes |

---

## License

MIT — free to use, modify, and distribute.

---

*Built entirely through an AI-assisted development process (Claude by Anthropic) — no IDE, no StackOverflow, just a chat window and `csc.exe`.*
