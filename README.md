# Veeam Matrix Screensaver

A Matrix-style Windows screensaver featuring the **Veeam Data Platform** universe — product names, features, and technologies rain down the screen vertically, letter by letter.

![Veeam Matrix Screensaver](.github/preview.png)

## Download

👉 **[Latest Release → VeeamMatrix.scr](../../releases/latest)**

No installer required. .NET Framework 4.x is pre-installed on all Windows 10/11 systems.

## Installation

1. **Download** `VeeamMatrix.scr`
2. **Right-click** the file:
   - **Install** — registers as system screensaver (requires admin)
   - **Test** — runs fullscreen immediately
   - **Configure** — opens the settings dialog

   Or double-click to run directly without installing.

## Configuration

Right-click → **Configure** to open the settings dialog:

| Setting | Description |
|---|---|
| **Rain color** | Color of the falling characters (default: Matrix green) |
| **Head color** | Color of the leading character (default: white) |
| **Font size – Rain** | Character size for the background rain (8–36 px) |
| **Font size – Words** | Character size for the Veeam keyword streams (8–36 px) |
| **Speed** | Animation speed (0.1× – 3.0×) |
| **Trail length** | How long characters glow before fading (low = longer trail) |
| **Direction** | TopDown / BottomUp / LeftRight / RightLeft |
| **Word count** | Number of simultaneous keyword streams (1–30) |
| **Scanlines** | Subtle CRT scanline overlay |
| **Watermark** | Faint VEEAM logo in the background |
| **Custom words** | Add your own terms (comma-separated) |

Settings are saved to `%APPDATA%\VeeamMatrix\config.ini`.

## Build from Source

Requires Windows with .NET Framework 4.x (pre-installed on Windows 10/11).

```powershell
.\Build-VeeamMatrix.ps1
```

This compiles `VeeamMatrix.cs` using the built-in `csc.exe` compiler — no Visual Studio or .NET SDK required.

## Multi-Monitor

The screensaver runs on **all connected monitors simultaneously**.

## Featured Veeam Terms

The screensaver includes 60+ terms from the Veeam ecosystem:

> Backup & Replication · VBR · Veeam Data Cloud · Hardened Repository · Immutability · Air Gap · Zero Trust · Cyber Vault · SureBackup · CDP · SOBR · Kasten · Coveware · Ransomware Recovery · 3-2-1 Rule · RPO · RTO · VMware vSphere · Hyper-V · Nutanix AHV · Kubernetes · AWS · Azure · GCP · and many more…

## License

MIT — free to use, modify and distribute.
