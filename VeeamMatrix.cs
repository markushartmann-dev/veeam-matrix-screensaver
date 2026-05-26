// VeeamMatrix.cs  –  Windows Screensaver v1.3
// Kompilieren: Build-VeeamMatrix.ps1
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace VeeamMatrix
{
    // =========================================================================
    // SETTINGS
    // =========================================================================
    class Settings
    {
        // Rain
        public Color  RainColor     = Color.FromArgb(0, 255, 65);
        public Color  HeadColor     = Color.White;
        public int    FadeAlpha     = 12;
        public int    FontSize      = 14;
        public float  SpeedFactor   = 1.0f;
        public bool   ShowScanlines = true;
        public bool   ShowWatermark = true;
        // Falling words
        public string WordMode      = "Both";   // Rain | Popup | Both
        public int    WordCount     = 10;
        public int    WordFontSize  = 16;
        public Color  WordColor     = Color.FromArgb(0, 255, 65);   // body color
        public Color  WordHeadColor = Color.White;                   // head color
        public float  GlowChance    = 0.22f;
        // Popup words
        public string PopupStyle    = "Mixed";  // Fade|Flash|Glitch|Scan|Zoom|Mixed
        public int    PopupCount    = 5;        // 1-12
        public int    PopupFontSize = 22;       // 10-48
        public Color  PopupColor    = Color.FromArgb(0, 255, 65);   // popup base color
        // General
        public string Orientation   = "TopDown"; // TopDown|BottomUp|LeftRight|RightLeft
        public string ExtraWords    = "";

        private static string ConfigDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VeeamMatrix"); }
        }
        private static string ConfigFile { get { return Path.Combine(ConfigDir, "config.ini"); } }

        public void Save()
        {
            Directory.CreateDirectory(ConfigDir);
            var sb = new StringBuilder();
            sb.AppendLine("[VeeamMatrix]");
            sb.AppendLine("RainColor="     + ToHex(RainColor));
            sb.AppendLine("HeadColor="     + ToHex(HeadColor));
            sb.AppendLine("FadeAlpha="     + FadeAlpha);
            sb.AppendLine("FontSize="      + FontSize);
            sb.AppendLine("SpeedFactor="   + SpeedFactor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendLine("ShowScanlines=" + ShowScanlines);
            sb.AppendLine("ShowWatermark=" + ShowWatermark);
            sb.AppendLine("WordMode="      + WordMode);
            sb.AppendLine("WordCount="     + WordCount);
            sb.AppendLine("WordFontSize="  + WordFontSize);
            sb.AppendLine("WordColor="     + ToHex(WordColor));
            sb.AppendLine("WordHeadColor=" + ToHex(WordHeadColor));
            sb.AppendLine("GlowChance="    + GlowChance.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendLine("PopupStyle="    + PopupStyle);
            sb.AppendLine("PopupCount="    + PopupCount);
            sb.AppendLine("PopupFontSize=" + PopupFontSize);
            sb.AppendLine("PopupColor="    + ToHex(PopupColor));
            sb.AppendLine("Orientation="   + Orientation);
            sb.AppendLine("ExtraWords="    + ExtraWords);
            File.WriteAllText(ConfigFile, sb.ToString(), Encoding.UTF8);
        }

        public static Settings Load()
        {
            var s = new Settings();
            if (!File.Exists(ConfigFile)) return s;
            var ic = System.Globalization.CultureInfo.InvariantCulture;
            foreach (string raw in File.ReadAllLines(ConfigFile))
            {
                string line = raw.Trim();
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                string k = line.Substring(0, eq).Trim();
                string v = line.Substring(eq + 1).Trim();
                try
                {
                    switch (k)
                    {
                        case "RainColor":     s.RainColor     = FromHex(v); break;
                        case "HeadColor":     s.HeadColor     = FromHex(v); break;
                        case "FadeAlpha":     s.FadeAlpha     = int.Parse(v); break;
                        case "FontSize":      s.FontSize      = int.Parse(v); break;
                        case "SpeedFactor":   s.SpeedFactor   = float.Parse(v, ic); break;
                        case "ShowScanlines": s.ShowScanlines = bool.Parse(v); break;
                        case "ShowWatermark": s.ShowWatermark = bool.Parse(v); break;
                        case "WordMode":      s.WordMode      = v; break;
                        case "WordCount":     s.WordCount     = int.Parse(v); break;
                        case "WordFontSize":  s.WordFontSize  = int.Parse(v); break;
                        case "WordColor":     s.WordColor     = FromHex(v); break;
                        case "WordHeadColor": s.WordHeadColor = FromHex(v); break;
                        case "GlowChance":    s.GlowChance    = float.Parse(v, ic); break;
                        case "PopupStyle":    s.PopupStyle    = v; break;
                        case "PopupCount":    s.PopupCount    = int.Parse(v); break;
                        case "PopupFontSize": s.PopupFontSize = int.Parse(v); break;
                        case "PopupColor":    s.PopupColor    = FromHex(v); break;
                        case "Orientation":   s.Orientation   = v; break;
                        case "ExtraWords":    s.ExtraWords    = v; break;
                    }
                }
                catch { }
            }
            return s;
        }

        private static string ToHex(Color c) { return string.Format("{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B); }
        private static Color FromHex(string h)
        {
            h = h.TrimStart('#');
            return Color.FromArgb(Convert.ToInt32(h.Substring(0,2),16), Convert.ToInt32(h.Substring(2,2),16), Convert.ToInt32(h.Substring(4,2),16));
        }
    }

    // =========================================================================
    // MATRIX ENGINE
    // =========================================================================
    class MatrixEngine : IDisposable
    {
        private static readonly string[] TERMS = new string[]
        {
            "VEEAM","VEEAM DATA PLATFORM","VEEAM DATA CLOUD",
            "BACKUP & REPLICATION","VBR","VBR 12.1","VEEAM ONE",
            "VEEAM AGENT","BACKUP FOR MICROSOFT 365","VBO365",
            "KASTEN BY VEEAM","COVEWARE BY VEEAM",
            "VEEAM BACKUP FOR SALESFORCE",
            "INSTANT RECOVERY","INSTANT VM RECOVERY",
            "SUREBACKUP","SURE REPLICA",
            "CONTINUOUS DATA PROTECTION","CDP",
            "HARDENED REPOSITORY","IMMUTABLE BACKUPS","IMMUTABILITY",
            "AIR-GAPPED REPOSITORY","AIR GAP","ZERO TRUST",
            "CYBER VAULT","CYBER RESILIENCE",
            "RANSOMWARE RECOVERY","RANSOMWARE PROTECTION",
            "MALWARE DETECTION","THREAT HUNTING",
            "SCALE-OUT BACKUP REPOSITORY","SOBR",
            "PERFORMANCE TIER","CAPACITY TIER","ARCHIVE TIER",
            "DEDUPLICATION","COMPRESSION","WAN ACCELERATION",
            "ENCRYPTION AT REST","ENCRYPTION IN FLIGHT",
            "CLOUD CONNECT","VCSP","MSP","VUL",
            "VEEAM UNIVERSAL LICENSE",
            "VEEAM FOR AWS","VEEAM FOR AZURE","VEEAM FOR GCP",
            "AWS","MICROSOFT AZURE","GOOGLE CLOUD",
            "VMWARE VSPHERE","VMWARE VCF","MICROSOFT HYPER-V",
            "NUTANIX AHV","KUBERNETES","RED HAT OPENSHIFT",
            "OBJECT STORAGE","MICROSOFT 365","AWS S3",
            "AZURE BLOB STORAGE","S3 GLACIER","S3-COMPATIBLE",
            "NFS","SMB / CIFS","ISCSI","FIBRE CHANNEL","TAPE",
            "RPO","RTO","SLA","FAILOVER","FAILBACK",
            "ZERO DATA LOSS","3-2-1 RULE","3-2-1-1-0",
            "GDPR","HIPAA","SOC 2","ISO 27001",
            "DATA SOVEREIGNTY","ALWAYS-ON DATA",
            "DATA PROTECTION","DATA SECURITY","COMPLIANCE",
            "AUTOMATED TESTING","HEALTH CHECK",
            "CAPACITY PLANNING","REPORTING","99.9% UPTIME",
            "SNAPSHOT-BASED BACKUP","AGENT-BASED BACKUP",
            "APPLICATION-AWARE BACKUP","BARE METAL RESTORE",
            "GRANULAR RECOVERY","INSTANT DISK RECOVERY",
        };

        private static readonly char[] RAIN_CHARS =
            "VEEAMBCKUPRSTOLHDNGRY0123456789ZFWXQ#$-.:/\\|".ToCharArray();

        // ── Falling word drops ────────────────────────────────────────────────
        private class WDrop
        {
            public char[] Chars;
            public float X, Y, V;
            public bool Glow;
        }

        // ── Popup word blips ──────────────────────────────────────────────────
        private enum PopupMode { Fade, Flash, Glitch, Scan, Zoom }

        private class WPopup
        {
            public char[]    Word;       // canonical word
            public char[]    Disp;       // what's drawn  (modified by Glitch/Scan)
            public float     CX, CY;    // center on screen
            public int       FontSize;
            public PopupMode Mode;
            public bool      Glow;
            // state machine
            public int  Phase;          // 0=appear 1=hold 2=disappear 3=done
            public int  Frame;          // frames into current phase
            public int  AppearF, HoldF, DisappearF;
        }

        private readonly Settings       s;
        private readonly int            W, H;
        private readonly Random         rng      = new Random();
        private readonly string[]       allTerms;

        private Bitmap     buf;
        private Graphics   bg;
        private Font       rainFont, wordFont;
        private SolidBrush fadeBrush, rainBrush, brightBrush, tmpBrush;
        private Bitmap     scanBmp;

        private float[] lanePos, laneSpeed;
        private bool[]  laneBright;
        private int     laneCount;

        private readonly List<WDrop>  wdrops  = new List<WDrop>();
        private readonly List<WPopup> popups  = new List<WPopup>();

        private bool IsVertical { get { return s.Orientation == "TopDown"  || s.Orientation == "BottomUp";  } }
        private bool IsForward  { get { return s.Orientation == "TopDown"  || s.Orientation == "LeftRight"; } }

        public MatrixEngine(Settings settings, int w, int h)
        {
            s = settings; W = w; H = h;
            var list = new List<string>(TERMS);
            if (!string.IsNullOrEmpty(s.ExtraWords))
                foreach (string p in s.ExtraWords.Split(','))
                { string t = p.Trim().ToUpper(); if (t.Length > 0) list.Add(t); }
            allTerms = list.ToArray();
            Build();
        }

        private void Build()
        {
            DisposeManagedResources();

            buf = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bg  = Graphics.FromImage(buf);
            bg.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            bg.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.None;
            bg.Clear(Color.Black);

            rainFont   = new Font("Courier New", Math.Max(6, s.FontSize - 1),    FontStyle.Bold, GraphicsUnit.Pixel);
            wordFont   = new Font("Courier New", Math.Max(6, s.WordFontSize - 1), FontStyle.Bold, GraphicsUnit.Pixel);
            fadeBrush  = new SolidBrush(Color.FromArgb(Math.Max(2, Math.Min(60, s.FadeAlpha)), 0, 0, 0));
            rainBrush  = new SolidBrush(s.RainColor);
            brightBrush= new SolidBrush(s.HeadColor);
            tmpBrush   = new SolidBrush(Color.White);

            // Rain lanes
            int fs    = s.FontSize;
            laneCount = IsVertical ? Math.Max(1, W / fs) : Math.Max(1, H / fs);
            int limit = IsVertical ? H / fs : W / fs;
            lanePos   = new float[laneCount];
            laneSpeed = new float[laneCount];
            laneBright= new bool[laneCount];
            for (int i = 0; i < laneCount; i++)
            {
                lanePos[i]    = (float)(rng.NextDouble() * limit);
                laneSpeed[i]  = (float)((0.2 + rng.NextDouble() * 0.9) * s.SpeedFactor);
                laneBright[i] = rng.NextDouble() < 0.1;
            }

            // Falling word drops
            wdrops.Clear();
            bool doDrops = s.WordMode == "Rain" || s.WordMode == "Both";
            if (doDrops)
                for (int i = 0; i < s.WordCount; i++) wdrops.Add(SpawnDrop(true));

            // Popup blips
            popups.Clear();
            bool doPopups = s.WordMode == "Popup" || s.WordMode == "Both";
            if (doPopups)
                for (int i = 0; i < s.PopupCount; i++) popups.Add(SpawnPopup(true));

            BuildScanlines();
        }

        // ── Falling drops ─────────────────────────────────────────────────────
        private WDrop SpawnDrop(bool scatter)
        {
            string term  = allTerms[rng.Next(allTerms.Length)];
            int    fs    = s.WordFontSize;
            char[] chars = term.ToCharArray();
            float  len   = chars.Length * fs;
            float  v     = (float)((0.6 + rng.NextDouble() * 1.6) * s.SpeedFactor);
            float x, y;
            if (IsVertical)
            {
                x = fs * 0.5f + (float)(rng.NextDouble() * Math.Max(1, W - fs));
                y = scatter ? (float)(rng.NextDouble() * (H + len) - len) : (IsForward ? -(len+5) : H+5);
            }
            else
            {
                y = fs * 0.5f + (float)(rng.NextDouble() * Math.Max(1, H - fs));
                x = scatter ? (float)(rng.NextDouble() * (W + len) - len) : (IsForward ? -(len+5) : W+5);
            }
            return new WDrop { Chars = chars, X = x, Y = y, V = IsForward ? v : -v, Glow = rng.NextDouble() < s.GlowChance };
        }

        // ── Popup blips ───────────────────────────────────────────────────────
        private PopupMode PickMode()
        {
            switch (s.PopupStyle)
            {
                case "Fade":  return PopupMode.Fade;
                case "Flash": return PopupMode.Flash;
                case "Glitch":return PopupMode.Glitch;
                case "Scan":  return PopupMode.Scan;
                case "Zoom":  return PopupMode.Zoom;
                default:      return (PopupMode)(rng.Next(5)); // Mixed
            }
        }

        private WPopup SpawnPopup(bool scatter)
        {
            string    term  = allTerms[rng.Next(allTerms.Length)];
            char[]    word  = term.ToCharArray();
            int       fs    = Math.Max(6, s.PopupFontSize);
            PopupMode mode  = PickMode();
            bool      glow  = rng.NextDouble() < s.GlowChance;

            // Phase durations (frames @ 40 fps)
            int appearF, holdF, disappearF;
            switch (mode)
            {
                case PopupMode.Flash:  appearF =  4; holdF = 18; disappearF = 28; break;
                case PopupMode.Glitch: appearF = 55; holdF = 90; disappearF = 35; break;
                case PopupMode.Scan:   appearF = word.Length * 3; holdF = 80; disappearF = 28; break;
                case PopupMode.Zoom:   appearF = 18; holdF = 90; disappearF = 28; break;
                default: /* Fade */    appearF = 22; holdF = 90; disappearF = 32; break;
            }

            // Center position — keep well inside screen
            float estW   = word.Length * fs * 0.64f;
            float margin = fs * 2f;
            float cx     = margin + estW / 2f + (float)(rng.NextDouble() * Math.Max(1, W - estW - 2f * margin));
            float cy;
            if (scatter) cy = margin + (float)(rng.NextDouble() * Math.Max(1, H - 2f * margin));
            else         cy = margin + (float)(rng.NextDouble() * Math.Max(1, H - 2f * margin));

            // Initial display chars
            char[] disp = (char[])word.Clone();
            if (mode == PopupMode.Glitch || mode == PopupMode.Scan)
                for (int i = 0; i < disp.Length; i++)
                    disp[i] = word[i] == ' ' ? ' ' : RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];

            return new WPopup
            {
                Word = word, Disp = disp, CX = cx, CY = cy,
                FontSize = fs, Mode = mode, Glow = glow,
                Phase = 0, Frame = 0,
                AppearF = appearF, HoldF = holdF, DisappearF = disappearF
            };
        }

        // ── Scanlines / watermark bitmap (static) ─────────────────────────────
        private void BuildScanlines()
        {
            if (!s.ShowScanlines && !s.ShowWatermark) return;
            scanBmp = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(scanBmp))
            {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                if (s.ShowScanlines)
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                        for (int y = 0; y < H; y += 2) g.FillRectangle(sb, 0, y, W, 1);
                if (s.ShowWatermark)
                {
                    int logoSz = Math.Max(24, (int)(W * 0.08));
                    using (Font lf = new Font("Courier New", logoSz, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (SolidBrush lb = new SolidBrush(Color.FromArgb(10, s.RainColor)))
                    {
                        SizeF ls = g.MeasureString("VEEAM", lf);
                        g.DrawString("VEEAM", lf, lb, (W - ls.Width) / 2f, (H - ls.Height) / 2f);
                    }
                    int subSz = Math.Max(7, (int)(W * 0.015));
                    using (Font sf = new Font("Courier New", subSz, FontStyle.Regular, GraphicsUnit.Pixel))
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(7, s.RainColor)))
                    {
                        const string sub = "DATA PROTECTION  *  CYBER RESILIENCE  *  ALWAYS-ON";
                        SizeF ss = g.MeasureString(sub, sf);
                        g.DrawString(sub, sf, sb, (W - ss.Width) / 2f, H / 2f + (int)(W * 0.045f));
                    }
                }
            }
        }

        // ── Main tick ─────────────────────────────────────────────────────────
        public void Tick()
        {
            bg.FillRectangle(fadeBrush, 0, 0, W, H);
            DrawRain();
            if (s.WordMode == "Rain" || s.WordMode == "Both") DrawDrops();
            if (s.WordMode == "Popup"|| s.WordMode == "Both") DrawPopups();
            if (scanBmp != null) bg.DrawImage(scanBmp, 0, 0);
        }

        // ── Rain ──────────────────────────────────────────────────────────────
        private void DrawRain()
        {
            int fs = s.FontSize;
            for (int i = 0; i < laneCount; i++)
            {
                char       ch = RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];
                SolidBrush br = laneBright[i] ? brightBrush : rainBrush;
                float px, py;
                if (IsVertical) { px = i * fs; py = IsForward ? lanePos[i]*fs : H - lanePos[i]*fs; }
                else            { py = i * fs; px = IsForward ? lanePos[i]*fs : W - lanePos[i]*fs; }
                bg.DrawString(ch.ToString(), rainFont, br, px, py);

                lanePos[i] += laneSpeed[i];
                float maxPos = IsVertical ? (float)H/fs : (float)W/fs;
                if (lanePos[i] > maxPos && rng.NextDouble() > 0.975)
                { lanePos[i] = 0; laneSpeed[i] = (float)((0.2+rng.NextDouble()*0.9)*s.SpeedFactor); laneBright[i] = rng.NextDouble()<0.1; }
            }
        }

        // ── Falling word drops ────────────────────────────────────────────────
        private void DrawDrops()
        {
            int fs = s.WordFontSize;
            for (int i = wdrops.Count - 1; i >= 0; i--)
            {
                WDrop w = wdrops[i];
                int   n = w.Chars.Length;
                for (int j = 0; j < n; j++)
                {
                    float px = IsVertical ? w.X          : w.X + j*fs;
                    float py = IsVertical ? w.Y + j*fs   : w.Y;
                    if (px<-fs||px>W+fs||py<-fs||py>H+fs) continue;
                    float fade = n>1 ? (float)j/(n-1) : 0f;
                    int   a    = Clamp((int)(255*(1f-fade*0.55f)));
                    Color col;
                    if (j==0) col = s.WordHeadColor;
                    else if (w.Glow) col = Color.FromArgb(a, Clamp(s.WordColor.R+80), Clamp(s.WordColor.G+20), Clamp(s.WordColor.B+40));
                    else col = Color.FromArgb(a, Clamp((int)(s.WordColor.R*(1-fade*.5f))), Clamp((int)(s.WordColor.G*(1-fade*.3f)+30*(1-fade))), Clamp((int)(s.WordColor.B*(1-fade*.5f))));
                    tmpBrush.Color = col;
                    bg.DrawString(w.Chars[j].ToString(), wordFont, tmpBrush, px, py);
                }
                if (IsVertical) w.Y += w.V; else w.X += w.V;
                bool gone = IsVertical ? (IsForward?w.Y>H+5:w.Y+n*fs<-5) : (IsForward?w.X>W+5:w.X+n*fs<-5);
                if (gone) { wdrops.RemoveAt(i); wdrops.Add(SpawnDrop(false)); }
            }
        }

        // ── Popup blips ───────────────────────────────────────────────────────
        private void DrawPopups()
        {
            for (int i = popups.Count - 1; i >= 0; i--)
            {
                WPopup p = popups[i];
                p.Frame++;

                // Phase transitions
                if      (p.Phase == 0 && p.Frame >= p.AppearF)    { p.Phase = 1; p.Frame = 0; }
                else if (p.Phase == 1 && p.Frame >= p.HoldF)      { p.Phase = 2; p.Frame = 0; }
                else if (p.Phase == 2 && p.Frame >= p.DisappearF) { p.Phase = 3; }

                if (p.Phase == 3)
                {
                    popups.RemoveAt(i);
                    popups.Add(SpawnPopup(false));
                    continue;
                }

                // Progress within phase (0→1)
                float prog = p.Phase == 0 ? (float)p.Frame / Math.Max(1, p.AppearF)
                           : p.Phase == 2 ? 1f - (float)p.Frame / Math.Max(1, p.DisappearF)
                           : 1f;

                switch (p.Mode)
                {
                    case PopupMode.Fade:   DrawFade(p, prog);  break;
                    case PopupMode.Flash:  DrawFlash(p, prog); break;
                    case PopupMode.Glitch: DrawGlitch(p, prog);break;
                    case PopupMode.Scan:   DrawScan(p, prog);  break;
                    case PopupMode.Zoom:   DrawZoom(p, prog);  break;
                }
            }
        }

        // FADE — smooth alpha in/out
        private void DrawFade(WPopup p, float alpha)
        {
            PaintPopup(p.Word, p.CX, p.CY, p.FontSize, alpha, p.Glow, false);
        }

        // FLASH — very bright, quick
        private void DrawFlash(WPopup p, float alpha)
        {
            bool isHold = p.Phase == 1;
            float boost = isHold ? 1f : alpha;
            // hard bright white with strong glow during hold
            PaintPopup(p.Word, p.CX, p.CY, p.FontSize, boost, true, isHold);
        }

        // GLITCH — chars scramble → resolve → scramble
        private void DrawGlitch(WPopup p, float prog)
        {
            for (int j = 0; j < p.Word.Length; j++)
            {
                if (p.Word[j] == ' ') { p.Disp[j] = ' '; continue; }
                if (p.Phase == 0)   // appearing: resolve chars progressively
                    p.Disp[j] = rng.NextDouble() < prog * 1.4 ? p.Word[j] : RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];
                else if (p.Phase == 1) // holding: mostly correct, tiny flicker
                    p.Disp[j] = rng.NextDouble() < 0.025 ? RAIN_CHARS[rng.Next(RAIN_CHARS.Length)] : p.Word[j];
                else                // disappearing: scramble again
                    p.Disp[j] = rng.NextDouble() < prog ? p.Word[j] : RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];
            }
            PaintPopup(p.Disp, p.CX, p.CY, p.FontSize, p.Phase == 0 ? prog * 0.8f + 0.2f : prog, p.Glow, false);
        }

        // SCAN — typewriter left-to-right, then reverse on exit
        private void DrawScan(WPopup p, float prog)
        {
            int len = p.Word.Length;
            if (p.Phase == 0) // appearing: reveal chars L→R with cursor
            {
                int revealed = Math.Min(len, (int)(prog * len));
                for (int j = 0; j < len; j++)
                {
                    if      (j < revealed)  p.Disp[j] = p.Word[j];
                    else if (j == revealed) p.Disp[j] = (p.Frame % 5 < 3) ? '_' : ' ';
                    else                    p.Disp[j] = ' ';
                }
                PaintPopup(p.Disp, p.CX, p.CY, p.FontSize, 1f, p.Glow, false);
            }
            else if (p.Phase == 1) // holding: full word, blinking cursor after
            {
                for (int j = 0; j < len; j++) p.Disp[j] = p.Word[j];
                PaintPopup(p.Disp, p.CX, p.CY, p.FontSize, 1f, p.Glow, false);
            }
            else // disappearing: erase chars R→L
            {
                int hide = Math.Min(len, (int)((1f-prog) * len));
                for (int j = 0; j < len; j++)
                    p.Disp[j] = j >= hide ? ' ' : p.Word[j];
                PaintPopup(p.Disp, p.CX, p.CY, p.FontSize, prog, p.Glow, false);
            }
        }

        // ZOOM — scales from 2× down to 1× while fading in, fades out at normal size
        private void DrawZoom(WPopup p, float prog)
        {
            float scale  = p.Phase == 0 ? 2f - prog : 1f;
            int   drawFs = Math.Max(6, (int)(p.FontSize * scale));
            float alpha  = p.Phase == 0 ? prog * prog : prog; // ease-in on appear
            PaintPopup(p.Word, p.CX, p.CY, drawFs, alpha, p.Glow, false);
        }

        // ── Low-level popup painter ───────────────────────────────────────────
        private void PaintPopup(char[] chars, float cx, float cy, int fontSize, float alpha, bool glow, bool flash)
        {
            if (alpha <= 0.01f) return;
            int a = Clamp((int)(alpha * 255));

            Font useFont  = null;
            bool ownFont  = false;
            if (fontSize == s.WordFontSize - 1 || fontSize == s.WordFontSize)
                useFont = wordFont;
            else
            { useFont = new Font("Courier New", Math.Max(6, fontSize), FontStyle.Bold, GraphicsUnit.Pixel); ownFont = true; }

            try
            {
                string text = new string(chars);
                SizeF  sz   = bg.MeasureString(text, useFont);
                float  drawX = cx - sz.Width  / 2f;
                float  drawY = cy - sz.Height / 2f;

                Color col;
                if (flash)
                    col = Color.FromArgb(a, 255, 255, 255);
                else if (glow)
                    col = Color.FromArgb(a, Clamp(s.PopupColor.R+90), Clamp(s.PopupColor.G+20), Clamp(s.PopupColor.B+60));
                else
                    col = Color.FromArgb(a, s.PopupColor.R, s.PopupColor.G, s.PopupColor.B);

                tmpBrush.Color = col;
                bg.DrawString(text, useFont, tmpBrush, drawX, drawY);
            }
            finally
            {
                if (ownFont && useFont != null) useFont.Dispose();
            }
        }

        private static int Clamp(int v) { return v < 0 ? 0 : v > 255 ? 255 : v; }

        public void Render(Graphics g) { g.DrawImage(buf, 0, 0); }

        private void DisposeManagedResources()
        {
            if (fadeBrush  != null) { fadeBrush.Dispose();   fadeBrush  = null; }
            if (rainBrush  != null) { rainBrush.Dispose();   rainBrush  = null; }
            if (brightBrush!= null) { brightBrush.Dispose(); brightBrush= null; }
            if (tmpBrush   != null) { tmpBrush.Dispose();    tmpBrush   = null; }
            if (bg         != null) { bg.Dispose();          bg         = null; }
            if (buf        != null) { buf.Dispose();         buf        = null; }
            if (rainFont   != null) { rainFont.Dispose();    rainFont   = null; }
            if (wordFont   != null) { wordFont.Dispose();    wordFont   = null; }
            if (scanBmp    != null) { scanBmp.Dispose();     scanBmp    = null; }
        }
        public void Dispose() { DisposeManagedResources(); }
    }

    // =========================================================================
    // SCREENSAVER FORM
    // =========================================================================
    class ScreenSaverForm : Form
    {
        [DllImport("user32.dll")] static extern bool ShowCursor(bool show);
        [DllImport("user32.dll")] static extern bool SetCursorPos(int x, int y);

        private readonly MatrixEngine engine;
        private readonly Timer        timer;
        private Point lastMouse;
        private bool  firstMove = true;

        public ScreenSaverForm(Settings s, Rectangle bounds, bool isPrimary)
        {
            FormBorderStyle = FormBorderStyle.None;
            Bounds          = bounds;
            BackColor       = Color.Black;
            TopMost         = true;
            ShowInTaskbar   = false;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            engine = new MatrixEngine(s, bounds.Width, bounds.Height);
            timer  = new Timer();
            timer.Interval = 25;
            timer.Tick += OnTick;
            timer.Start();

            if (isPrimary)
            {
                ShowCursor(false);
                SetCursorPos(bounds.Left, bounds.Top);
                KeyDown    += delegate { Close(); };
                MouseDown  += delegate { Close(); };
                MouseMove  += OnMouseMove;
                FormClosed += delegate { ShowCursor(true); };
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }
        private void OnTick(object sender, EventArgs e) { engine.Tick(); Invalidate(false); }
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (firstMove) { lastMouse = e.Location; firstMove = false; return; }
            if (Math.Abs(e.X - lastMouse.X) > 5 || Math.Abs(e.Y - lastMouse.Y) > 5) Close();
        }
        protected override void OnPaint(PaintEventArgs e) { engine.Render(e.Graphics); }
        protected override void Dispose(bool disposing)
        { if (disposing) { timer.Dispose(); engine.Dispose(); } base.Dispose(disposing); }
    }

    // =========================================================================
    // PREVIEW FORM
    // =========================================================================
    class PreviewForm : Form
    {
        [DllImport("user32.dll")] static extern IntPtr SetParent(IntPtr child, IntPtr parent);
        [DllImport("user32.dll")] static extern bool   GetClientRect(IntPtr hwnd, out NativeRect r);
        [StructLayout(LayoutKind.Sequential)]
        struct NativeRect { public int Left, Top, Right, Bottom; }

        private readonly MatrixEngine engine;
        private readonly Timer        timer;

        public PreviewForm(Settings s, IntPtr parentHwnd)
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor       = Color.Black;
            ShowInTaskbar   = false;
            NativeRect nr;
            GetClientRect(parentHwnd, out nr);
            Bounds = new Rectangle(0, 0, nr.Right - nr.Left, nr.Bottom - nr.Top);
            SetParent(Handle, parentHwnd);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            engine = new MatrixEngine(s, Width, Height);
            timer  = new Timer();
            timer.Interval = 40;
            timer.Tick += delegate { engine.Tick(); Invalidate(false); };
            timer.Start();
        }
        protected override void OnPaintBackground(PaintEventArgs e) { }
        protected override void OnPaint(PaintEventArgs e) { engine.Render(e.Graphics); }
        protected override void Dispose(bool disposing)
        { if (disposing) { timer.Dispose(); engine.Dispose(); } base.Dispose(disposing); }
    }

    // =========================================================================
    // CONFIG FORM
    // =========================================================================
    class ConfigForm : Form
    {
        private Settings cur;
        private Button   btnRainColor, btnHeadColor;
        private TrackBar trkFade, trkFont, trkSpeed, trkWordCount, trkWordFont;
        private TrackBar trkPopupCount, trkPopupFont;
        private Label    lblFade, lblFont, lblSpeed, lblWCount, lblWFont, lblPCount, lblPFont;
        private ComboBox cboOrient, cboWordMode, cboPopupStyle;
        private CheckBox chkScanlines, chkWatermark;
        private TextBox  txtExtra;

        public Settings Result { get; private set; }

        public ConfigForm(Settings s)
        {
            cur             = Clone(s);
            Text            = "Veeam Matrix – Einstellungen";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ClientSize      = new Size(520, 100); // height set at end of Build()
            BackColor       = Color.FromArgb(18, 18, 18);
            ForeColor       = Color.FromArgb(0, 200, 55);
            Font            = new Font("Segoe UI", 9f);
            Build();
        }

        private static Settings Clone(Settings s)
        {
            return new Settings
            {
                RainColor    = s.RainColor,    HeadColor     = s.HeadColor,
                FadeAlpha    = s.FadeAlpha,    FontSize      = s.FontSize,
                SpeedFactor  = s.SpeedFactor,  ShowScanlines = s.ShowScanlines,
                ShowWatermark= s.ShowWatermark, WordMode     = s.WordMode,
                WordCount    = s.WordCount,    WordFontSize  = s.WordFontSize,
                WordColor    = s.WordColor,    WordHeadColor = s.WordHeadColor,
                GlowChance   = s.GlowChance,   PopupStyle   = s.PopupStyle,
                PopupCount   = s.PopupCount,   PopupFontSize = s.PopupFontSize,
                PopupColor   = s.PopupColor,
                Orientation  = s.Orientation,  ExtraWords   = s.ExtraWords
            };
        }

        private Label AddLabel(string text, int x, int y)
        {
            var l = new Label { Text = text, Location = new Point(x, y), AutoSize = true, ForeColor = Color.FromArgb(0, 200, 55) };
            Controls.Add(l); return l;
        }
        private void AddSep(int y)
        {
            Controls.Add(new Panel { Location = new Point(14, y), Size = new Size(492, 1), BackColor = Color.FromArgb(0, 80, 25) });
        }
        private TrackBar AddTrack(int x, int y, int w, int min, int max, int val)
        {
            var t = new TrackBar { Location = new Point(x,y), Size = new Size(w,36), Minimum=min, Maximum=max, Value=val,
                                   TickFrequency=Math.Max(1,(max-min)/10), SmallChange=1, BackColor=Color.FromArgb(18,18,18) };
            Controls.Add(t); return t;
        }
        private Button AddColorBtn(string text, Color col, int x, int y)
        {
            var b = new Button { Text=text, Location=new Point(x,y), Size=new Size(140,28), BackColor=col,
                                 ForeColor=col.GetBrightness()>0.45f?Color.Black:Color.White, FlatStyle=FlatStyle.Flat };
            b.FlatAppearance.BorderColor = Color.FromArgb(0,160,45);
            Controls.Add(b); return b;
        }
        private ComboBox AddCombo(int x, int y, int w, string[] items, string selected)
        {
            var c = new ComboBox { Location=new Point(x,y), Size=new Size(w,24), DropDownStyle=ComboBoxStyle.DropDownList,
                                   BackColor=Color.FromArgb(30,30,30), ForeColor=Color.FromArgb(0,200,55) };
            c.Items.AddRange(items); c.Text = selected; Controls.Add(c); return c;
        }

        private void Build()
        {
            int y = 16;

            // ── Farben
            AddLabel("Regen-Zeichen", 14, y); y += 20;
            btnRainColor = AddColorBtn("Zeichen",       cur.RainColor,     14,  y);
            btnHeadColor = AddColorBtn("Kopf (hell)",   cur.HeadColor,     162, y);
            btnRainColor.Click += delegate { PickColor(ref cur.RainColor, btnRainColor); };
            btnHeadColor.Click += delegate { PickColor(ref cur.HeadColor, btnHeadColor); };
            y += 36;

            AddLabel("Fallende Woerter", 14, y); y += 20;
            Button btnWordColor     = AddColorBtn("Woerter",        cur.WordColor,     14,  y);
            Button btnWordHeadColor = AddColorBtn("Kopf (hell)",    cur.WordHeadColor, 162, y);
            btnWordColor.Click     += delegate { PickColor(ref cur.WordColor,     btnWordColor);     };
            btnWordHeadColor.Click += delegate { PickColor(ref cur.WordHeadColor, btnWordHeadColor); };
            y += 36;

            AddLabel("Popup-Woerter", 14, y); y += 20;
            Button btnPopupColor = AddColorBtn("Popup-Farbe", cur.PopupColor, 14, y);
            btnPopupColor.Click += delegate { PickColor(ref cur.PopupColor, btnPopupColor); };
            y += 36; AddSep(y); y += 10;

            // ── Regen
            lblFont = AddLabel("Schriftgroesse Regen:  " + cur.FontSize + " px", 14, y); y += 18;
            trkFont = AddTrack(14, y, 360, 8, 36, cur.FontSize);
            trkFont.ValueChanged += delegate { cur.FontSize = trkFont.Value; lblFont.Text = "Schriftgroesse Regen:  " + cur.FontSize + " px"; };
            y += 44;
            lblSpeed = AddLabel("Geschwindigkeit:  " + cur.SpeedFactor.ToString("F1") + "x", 14, y); y += 18;
            trkSpeed = AddTrack(14, y, 360, 1, 30, (int)(cur.SpeedFactor * 10));
            trkSpeed.ValueChanged += delegate { cur.SpeedFactor = trkSpeed.Value / 10f; lblSpeed.Text = "Geschwindigkeit:  " + cur.SpeedFactor.ToString("F1") + "x"; };
            y += 44;
            lblFade = AddLabel("Spur-Laenge (niedrig = laenger):  " + cur.FadeAlpha, 14, y); y += 18;
            trkFade = AddTrack(14, y, 360, 2, 60, cur.FadeAlpha);
            trkFade.ValueChanged += delegate { cur.FadeAlpha = trkFade.Value; lblFade.Text = "Spur-Laenge (niedrig = laenger):  " + cur.FadeAlpha; };
            y += 44; AddSep(y); y += 10;

            // ── Richtung
            AddLabel("Richtung:", 14, y);
            cboOrient = AddCombo(130, y - 2, 160, new string[]{"TopDown","BottomUp","LeftRight","RightLeft"}, cur.Orientation);
            y += 32; AddSep(y); y += 10;

            // ── Wortmodus
            AddLabel("Wortmodus:", 14, y);
            cboWordMode = AddCombo(130, y - 2, 160, new string[]{"Rain","Popup","Both"}, cur.WordMode);
            AddLabel("(Rain = fallend  |  Popup = aufblitzen  |  Both = beides)", 300, y);
            y += 32; AddSep(y); y += 10;

            // ── Fallende Woerter (Rain / Both)
            AddLabel("── Fallende Woerter ──────────────────", 14, y); y += 20;
            lblWFont = AddLabel("Schriftgroesse:  " + cur.WordFontSize + " px", 14, y); y += 18;
            trkWordFont = AddTrack(14, y, 360, 8, 36, cur.WordFontSize);
            trkWordFont.ValueChanged += delegate { cur.WordFontSize = trkWordFont.Value; lblWFont.Text = "Schriftgroesse:  " + cur.WordFontSize + " px"; };
            y += 44;
            lblWCount = AddLabel("Gleichzeitige Woerter:  " + cur.WordCount, 14, y); y += 18;
            trkWordCount = AddTrack(14, y, 360, 1, 30, cur.WordCount);
            trkWordCount.ValueChanged += delegate { cur.WordCount = trkWordCount.Value; lblWCount.Text = "Gleichzeitige Woerter:  " + cur.WordCount; };
            y += 44; AddSep(y); y += 10;

            // ── Popup-Woerter
            AddLabel("── Popup-Woerter ────────────────────", 14, y); y += 20;
            AddLabel("Stil:", 14, y);
            cboPopupStyle = AddCombo(80, y - 2, 150, new string[]{"Mixed","Fade","Flash","Glitch","Scan","Zoom"}, cur.PopupStyle);
            AddLabel("Mixed = zufaellig", 240, y);
            y += 32;
            lblPFont = AddLabel("Schriftgroesse:  " + cur.PopupFontSize + " px", 14, y); y += 18;
            trkPopupFont = AddTrack(14, y, 360, 10, 72, cur.PopupFontSize);
            trkPopupFont.ValueChanged += delegate { cur.PopupFontSize = trkPopupFont.Value; lblPFont.Text = "Schriftgroesse:  " + cur.PopupFontSize + " px"; };
            y += 44;
            lblPCount = AddLabel("Gleichzeitige Popups:  " + cur.PopupCount, 14, y); y += 18;
            trkPopupCount = AddTrack(14, y, 360, 1, 16, cur.PopupCount);
            trkPopupCount.ValueChanged += delegate { cur.PopupCount = trkPopupCount.Value; lblPCount.Text = "Gleichzeitige Popups:  " + cur.PopupCount; };
            y += 44; AddSep(y); y += 10;

            // ── Extras
            chkScanlines = new CheckBox { Text = "CRT-Scanlines",       Location = new Point(14,  y), Checked = cur.ShowScanlines, AutoSize = true, ForeColor = Color.FromArgb(0, 200, 55) };
            chkWatermark = new CheckBox { Text = "VEEAM-Wasserzeichen", Location = new Point(180, y), Checked = cur.ShowWatermark, AutoSize = true, ForeColor = Color.FromArgb(0, 200, 55) };
            Controls.Add(chkScanlines); Controls.Add(chkWatermark);
            y += 28; AddSep(y); y += 10;

            // ── Eigene Begriffe
            AddLabel("Eigene Begriffe (kommagetrennt):", 14, y); y += 20;
            txtExtra = new TextBox { Location = new Point(14, y), Size = new Size(492, 22), Text = cur.ExtraWords,
                                     BackColor = Color.FromArgb(28,28,28), ForeColor = Color.FromArgb(0,200,55), BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(txtExtra);
            y += 36; AddSep(y); y += 14;

            // ── Buttons
            int bx = ClientSize.Width - 228;
            var btnOK = new Button { Text = "OK", Location = new Point(bx, y), Size = new Size(90, 30),
                                     DialogResult = DialogResult.OK, BackColor = Color.FromArgb(0,130,38), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(0,200,55);
            btnOK.Click += delegate
            {
                cur.Orientation   = cboOrient.Text;
                cur.WordMode      = cboWordMode.Text;
                cur.PopupStyle    = cboPopupStyle.Text;
                cur.ShowScanlines = chkScanlines.Checked;
                cur.ShowWatermark = chkWatermark.Checked;
                cur.ExtraWords    = txtExtra.Text.Trim();
                Result = cur;
                Result.Save();
            };
            var btnCancel = new Button { Text = "Abbrechen", Location = new Point(bx + 98, y), Size = new Size(108, 30),
                                         DialogResult = DialogResult.Cancel, BackColor = Color.FromArgb(55,18,18), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(140,40,40);
            Controls.Add(btnOK); Controls.Add(btnCancel);
            AcceptButton = btnOK; CancelButton = btnCancel;

            ClientSize = new Size(ClientSize.Width, y + 50);
        }

        private void PickColor(ref Color field, Button btn)
        {
            using (ColorDialog dlg = new ColorDialog { Color = field, FullOpen = true })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                { field = dlg.Color; btn.BackColor = dlg.Color; btn.ForeColor = dlg.Color.GetBrightness() > 0.45f ? Color.Black : Color.White; }
            }
        }
    }

    // =========================================================================
    // ENTRY POINT
    // =========================================================================
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Settings s    = Settings.Load();
            string   mode = args.Length > 0 ? args[0].ToLower() : "/s";

            if (mode == "/c" || mode.StartsWith("/c:"))
            {
                using (ConfigForm cfg = new ConfigForm(s)) cfg.ShowDialog();
            }
            else if (mode == "/p" && args.Length > 1)
            {
                IntPtr hwnd = new IntPtr(long.Parse(args[1]));
                using (PreviewForm pf = new PreviewForm(s, hwnd)) Application.Run(pf);
            }
            else
            {
                var forms  = new List<ScreenSaverForm>();
                ScreenSaverForm main = null;
                foreach (Screen scr in Screen.AllScreens)
                {
                    var f = new ScreenSaverForm(s, scr.Bounds, scr.Primary);
                    forms.Add(f);
                    if (scr.Primary) main = f; else f.Show();
                }
                if (main != null)
                {
                    main.FormClosed += delegate { foreach (var f in forms) try { f.Close(); } catch { } };
                    Application.Run(main);
                }
            }
        }
    }
}
