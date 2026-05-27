# VeeaMatrix Screensaver

A Matrix-style Windows screensaver featuring the **Veeam Data Platform** universe — product names, features, and technologies rain down the screen vertically, letter by letter.

![VeeaMatrix Screensaver](.github/preview.png)

## Download

👉 **[Latest Release → VeeaMatrix.scr](../../releases/latest)**

No installer required. .NET Framework 4.x is pre-installed on all Windows 10/11 systems.

## Installation

1. **Download** `VeeaMatrix.scr`
2. **Right-click** the file:
   - **Install** — registers as system screensaver (requires admin)
   - **Test** — runs fullscreen immediately
   - **Configure** — opens the settings dialog

   Or double-click to run directly without installing.

## Configuration

Right-click → **Configure** to open the settings dialog:

| Setting | Description |
|---|---|
| **Language** | UI language toggle — EN (default) or DE |
| **Color Profile** | Load a built-in or saved color profile (Veeam, Hello Kitty, Matrix Classic, Cyberpunk, Amber CRT) |
| **Rain color** | Color of the falling background characters |
| **Head color** | Color of the leading (head) character |
| **Word color** | Color of the falling Veeam keyword streams |
| **Word head color** | Color of the leading character in keyword streams |
| **Popup color** | Color of the popup/blip word effects |
| **Font** | System font used for keyword streams, popups and watermark — pick any installed font with live "VEEAM" preview |
| **Font size – Rain** | Character size for the background rain (8–36 px) |
| **Font size – Words** | Character size for the Veeam keyword streams (8–36 px) |
| **Font size – Popup** | Character size for popup word effects (10–72 px) |
| **Speed** | Animation speed (0.1× – 3.0×) |
| **Word speed** | Separate speed multiplier for keyword streams (0.1× – 3.0×) |
| **Trail length** | How long characters glow before fading (low = longer trail) |
| **Direction** | TopDown / BottomUp / LeftRight / RightLeft |
| **Word direction** | Independent direction for keyword streams (or Same as rain) |
| **Word style** | Scroll (falling) / Fade (appear in place) / Build (character-by-character decode) |
| **Word mode** | Rain (falling only) / Popup (blip only) / Both |
| **Word count** | Number of simultaneous keyword streams (1–30) |
| **Popup count** | Number of simultaneous popup blips (1–20) |
| **Effects** | Individually enable/disable: Fade · Flash · Glitch · Scan · Zoom (grouped under WORDS) |
| **Scanlines** | Subtle CRT scanline overlay |
| **Watermark** | Faint logo in the background — text is fully customisable |
| **Subtitle** | Second watermark line — wide, full-row text field |
| **Veeam 100 names** | Show Veeam Vanguard / Legend / MVP 2026 member names in the streams |
| **Custom words** | Add your own terms (comma-separated) |

Settings are saved to `%APPDATA%\VeeaMatrix\config.ini`.  
Color profiles are saved to `%APPDATA%\VeeaMatrix\profiles.ini`.

> **Upgrading from v1.9?** Settings are migrated automatically from `%APPDATA%\VeeamMatrix`.

## Color Profiles

Five built-in profiles are included:

| Profile | Description |
|---|---|
| **Veeam** | Official Veeam green (`#00B336`) with white heads |
| **Hello Kitty** | Hot pink rain with gold heads |
| **Matrix Classic** | Original movie green (`#00FF41`) |
| **Cyberpunk** | Electric cyan with magenta heads |
| **Amber CRT** | Retro amber terminal look |

You can save your own profiles via **Save as…** in the config dialog.

## Build from Source

Requires Windows with .NET Framework 4.x (pre-installed on Windows 10/11).

```powershell
.\Build-VeeamMatrix.ps1
```

This compiles `VeeamMatrix.cs` using the built-in `csc.exe` compiler — no Visual Studio or .NET SDK required. Output: `VeeaMatrix.scr`.

## Multi-Monitor

The screensaver runs on **all connected monitors simultaneously**.

## Featured Veeam Terms

The screensaver includes 100+ terms from the Veeam ecosystem:

> Backup & Replication · VBR · Veeam Data Cloud · Hardened Repository · Immutability · Air Gap · Zero Trust · Cyber Vault · SureBackup · CDP · SOBR · Kasten · Coveware · Ransomware Recovery · 3-2-1 Rule · RPO · RTO · VMware vSphere · Hyper-V · Nutanix AHV · Kubernetes · AWS · Azure · GCP · and many more…

## License

MIT — free to use, modify and distribute.
