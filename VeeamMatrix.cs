// VeeamMatrix.cs  –  Windows Screensaver v1.9
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
    // COLOR PROFILE  +  PROFILE MANAGER
    // =========================================================================
    class ColorProfile
    {
        public string Name;
        public Color  RainColor, HeadColor, WordColor, WordHeadColor, PopupColor;

        public static readonly ColorProfile[] BUILTIN = new ColorProfile[]
        {
            new ColorProfile
            {
                Name          = "Veeam",
                RainColor     = Color.FromArgb(0,   179,  54),   // #00B336 brand green
                HeadColor     = Color.FromArgb(255, 255, 255),
                WordColor     = Color.FromArgb(0,   224,  78),   // brighter accent
                WordHeadColor = Color.FromArgb(170, 255, 196),
                PopupColor    = Color.FromArgb(0,   179,  54),
            },
            new ColorProfile
            {
                Name          = "Hello Kitty",
                RainColor     = Color.FromArgb(255, 105, 180),   // hot pink
                HeadColor     = Color.FromArgb(255, 255, 255),
                WordColor     = Color.FromArgb(255,  20, 147),   // deep pink
                WordHeadColor = Color.FromArgb(255, 215,   0),   // gold bow
                PopupColor    = Color.FromArgb(255, 105, 180),
            },
            new ColorProfile
            {
                Name          = "Matrix Classic",
                RainColor     = Color.FromArgb(0,   255,  65),
                HeadColor     = Color.FromArgb(255, 255, 255),
                WordColor     = Color.FromArgb(0,   255,  65),
                WordHeadColor = Color.FromArgb(255, 255, 255),
                PopupColor    = Color.FromArgb(0,   255,  65),
            },
            new ColorProfile
            {
                Name          = "Cyberpunk",
                RainColor     = Color.FromArgb(0,   240, 255),   // cyan
                HeadColor     = Color.FromArgb(255, 255, 255),
                WordColor     = Color.FromArgb(255,  20, 147),   // magenta
                WordHeadColor = Color.FromArgb(255, 255,   0),   // yellow
                PopupColor    = Color.FromArgb(0,   240, 255),
            },
            new ColorProfile
            {
                Name          = "Amber CRT",
                RainColor     = Color.FromArgb(255, 176,   0),
                HeadColor     = Color.FromArgb(255, 255, 200),
                WordColor     = Color.FromArgb(255, 140,   0),
                WordHeadColor = Color.FromArgb(255, 240, 180),
                PopupColor    = Color.FromArgb(255, 200,  50),
            },
        };

        private static string ProfileFile
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VeeamMatrix", "profiles.ini"); }
        }

        public static List<ColorProfile> LoadAll()
        {
            var list = new List<ColorProfile>(BUILTIN);
            if (!File.Exists(ProfileFile)) return list;
            string cur = null;
            var tmp = new ColorProfile();
            foreach (string raw in File.ReadAllLines(ProfileFile))
            {
                string line = raw.Trim();
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (cur != null) { tmp.Name = cur; list.Add(tmp); tmp = new ColorProfile(); }
                    cur = line.Substring(1, line.Length - 2);
                }
                int eq = line.IndexOf('='); if (eq < 0) continue;
                string k = line.Substring(0, eq).Trim();
                string v = line.Substring(eq + 1).Trim();
                try
                {
                    switch (k)
                    {
                        case "RainColor":     tmp.RainColor     = FromHex(v); break;
                        case "HeadColor":     tmp.HeadColor     = FromHex(v); break;
                        case "WordColor":     tmp.WordColor     = FromHex(v); break;
                        case "WordHeadColor": tmp.WordHeadColor = FromHex(v); break;
                        case "PopupColor":    tmp.PopupColor    = FromHex(v); break;
                    }
                }
                catch { }
            }
            if (cur != null) { tmp.Name = cur; list.Add(tmp); }
            return list;
        }

        public static void SaveUser(ColorProfile p)
        {
            string dir = Path.GetDirectoryName(ProfileFile);
            Directory.CreateDirectory(dir);
            var existing = new List<ColorProfile>();
            if (File.Exists(ProfileFile))
            {
                var all = LoadAll();
                foreach (var x in all)
                {
                    bool builtin = false;
                    foreach (var b in BUILTIN) if (b.Name == x.Name) { builtin = true; break; }
                    if (!builtin) existing.Add(x);
                }
            }
            bool found = false;
            for (int i = 0; i < existing.Count; i++) if (existing[i].Name == p.Name) { existing[i] = p; found = true; break; }
            if (!found) existing.Add(p);

            var sb = new StringBuilder();
            foreach (var x in existing)
            {
                sb.AppendLine("[" + x.Name + "]");
                sb.AppendLine("RainColor="     + ToHex(x.RainColor));
                sb.AppendLine("HeadColor="     + ToHex(x.HeadColor));
                sb.AppendLine("WordColor="     + ToHex(x.WordColor));
                sb.AppendLine("WordHeadColor=" + ToHex(x.WordHeadColor));
                sb.AppendLine("PopupColor="    + ToHex(x.PopupColor));
                sb.AppendLine();
            }
            File.WriteAllText(ProfileFile, sb.ToString(), Encoding.UTF8);
        }

        private static string ToHex(Color c) { return string.Format("{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B); }
        private static Color FromHex(string h)
        {
            h = h.TrimStart('#');
            return Color.FromArgb(Convert.ToInt32(h.Substring(0,2),16), Convert.ToInt32(h.Substring(2,2),16), Convert.ToInt32(h.Substring(4,2),16));
        }
    }

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
        public string WordMode      = "Both";
        public int    WordCount     = 10;
        public int    WordFontSize  = 16;
        public Color  WordColor     = Color.FromArgb(0, 255, 65);
        public Color  WordHeadColor = Color.White;
        public float  GlowChance    = 0.22f;
        // Popup words  — comma-separated list of ENABLED effects
        public string PopupEffects  = "Fade,Flash,Glitch,Scan,Zoom";  // all on by default
        public int    PopupCount    = 5;
        public int    PopupFontSize = 22;
        public Color  PopupColor    = Color.FromArgb(0, 255, 65);
        // General
        public string Orientation     = "TopDown";
        public string WordOrientation  = "Same";    // "Same" follows Orientation; or TopDown/BottomUp/LeftRight/RightLeft
        public string WordStyle        = "Scroll";  // "Scroll" | "Fade" | "Build"
        public float  WordSpeedFactor  = 1.0f;      // separate speed multiplier for word drops
        public bool   ShowVeeam100     = false;
        // Watermark
        public string WatermarkText    = "VEEAM";
        public string WatermarkSubText = "DATA PROTECTION  *  CYBER RESILIENCE  *  ALWAYS-ON";
        public string ExtraWords      = "";
        public string WordFontName    = "Segoe UI";

        private static string ConfigDir
        { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VeeamMatrix"); } }
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
            sb.AppendLine("PopupEffects="  + PopupEffects);
            sb.AppendLine("PopupCount="    + PopupCount);
            sb.AppendLine("PopupFontSize=" + PopupFontSize);
            sb.AppendLine("PopupColor="    + ToHex(PopupColor));
            sb.AppendLine("Orientation="     + Orientation);
            sb.AppendLine("WordOrientation="  + WordOrientation);
            sb.AppendLine("WordStyle="        + WordStyle);
            sb.AppendLine("WordSpeedFactor="  + WordSpeedFactor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendLine("ShowVeeam100="     + ShowVeeam100);
            sb.AppendLine("WatermarkText="    + WatermarkText);
            sb.AppendLine("WatermarkSubText=" + WatermarkSubText);
            sb.AppendLine("ExtraWords="      + ExtraWords);
            sb.AppendLine("WordFontName="    + WordFontName);
            File.WriteAllText(ConfigFile, sb.ToString(), Encoding.UTF8);
        }

        public static Settings Load()
        {
            var s = new Settings();
            if (!File.Exists(ConfigFile)) return s;
            var ic = System.Globalization.CultureInfo.InvariantCulture;
            foreach (string raw in File.ReadAllLines(ConfigFile))
            {
                string line = raw.Trim(); int eq = line.IndexOf('='); if (eq < 0) continue;
                string k = line.Substring(0, eq).Trim(), v = line.Substring(eq+1).Trim();
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
                        case "PopupEffects":  s.PopupEffects  = v; break;
                        // legacy key
                        case "PopupStyle":    s.PopupEffects  = v == "Mixed" ? "Fade,Flash,Glitch,Scan,Zoom" : v; break;
                        case "PopupCount":    s.PopupCount    = int.Parse(v); break;
                        case "PopupFontSize": s.PopupFontSize = int.Parse(v); break;
                        case "PopupColor":    s.PopupColor    = FromHex(v); break;
                        case "Orientation":     s.Orientation     = v; break;
                        case "WordOrientation":  s.WordOrientation  = v; break;
                        case "WordStyle":        s.WordStyle        = v; break;
                        case "WordSpeedFactor":  s.WordSpeedFactor  = float.Parse(v, ic); break;
                        case "ShowVeeam100":     s.ShowVeeam100     = bool.Parse(v); break;
                        case "WatermarkText":    s.WatermarkText    = v; break;
                        case "WatermarkSubText": s.WatermarkSubText = v; break;
                        case "ExtraWords":      s.ExtraWords      = v; break;
                        case "WordFontName":    s.WordFontName    = v; break;
                    }
                }
                catch { }
            }
            return s;
        }

        public void ApplyProfile(ColorProfile p)
        {
            RainColor = p.RainColor; HeadColor = p.HeadColor;
            WordColor = p.WordColor; WordHeadColor = p.WordHeadColor;
            PopupColor = p.PopupColor;
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
            "KASTEN BY VEEAM","COVEWARE BY VEEAM","VEEAM BACKUP FOR SALESFORCE",
            "INSTANT RECOVERY","INSTANT VM RECOVERY","SUREBACKUP","SURE REPLICA",
            "CONTINUOUS DATA PROTECTION","CDP",
            "HARDENED REPOSITORY","IMMUTABLE BACKUPS","IMMUTABILITY",
            "AIR-GAPPED REPOSITORY","AIR GAP","ZERO TRUST",
            "CYBER VAULT","CYBER RESILIENCE","RANSOMWARE RECOVERY","RANSOMWARE PROTECTION",
            "MALWARE DETECTION","THREAT HUNTING",
            "SCALE-OUT BACKUP REPOSITORY","SOBR",
            "PERFORMANCE TIER","CAPACITY TIER","ARCHIVE TIER",
            "DEDUPLICATION","COMPRESSION","WAN ACCELERATION",
            "ENCRYPTION AT REST","ENCRYPTION IN FLIGHT",
            "CLOUD CONNECT","VCSP","MSP","VUL","VEEAM UNIVERSAL LICENSE",
            "VEEAM FOR AWS","VEEAM FOR AZURE","VEEAM FOR GCP",
            "AWS","MICROSOFT AZURE","GOOGLE CLOUD",
            "VMWARE VSPHERE","VMWARE VCF","MICROSOFT HYPER-V",
            "NUTANIX AHV","KUBERNETES","RED HAT OPENSHIFT",
            "OBJECT STORAGE","MICROSOFT 365","AWS S3","AZURE BLOB STORAGE","S3 GLACIER",
            "NFS","SMB / CIFS","ISCSI","FIBRE CHANNEL","TAPE",
            "RPO","RTO","SLA","FAILOVER","FAILBACK","ZERO DATA LOSS","3-2-1 RULE","3-2-1-1-0",
            "GDPR","HIPAA","SOC 2","ISO 27001","DATA SOVEREIGNTY","ALWAYS-ON DATA",
            "DATA PROTECTION","DATA SECURITY","COMPLIANCE",
            "AUTOMATED TESTING","HEALTH CHECK","CAPACITY PLANNING","99.9% UPTIME",
            "SNAPSHOT-BASED BACKUP","AGENT-BASED BACKUP","APPLICATION-AWARE BACKUP",
            "BARE METAL RESTORE","GRANULAR RECOVERY","INSTANT DISK RECOVERY",
        };
        private static readonly char[] RAIN_CHARS =
            "VEEAMBCKUPRSTOLHDNGRY0123456789ZFWXQ#$-.:/\\|".ToCharArray();

        // Veeam 100 community members 2026 — name + designation
        // Source: community.veeam.com/p/veeamvanguard2026
        //         community.veeam.com/p/veeamlegends2026
        //         community.veeam.com/p/veeammvp2026
        private static readonly string[] VEEAM100_PEOPLE = new string[]
        {
            // ── Category headers ───────────────────────────────────────────────
            "VEEAM VANGUARD 2026", "VEEAM LEGEND 2026", "VEEAM MVP 2026",

            // ── Veeam Vanguard 2026 (52) ───────────────────────────────────────
            "MUHAMMAD ADEL · VANGUARD",      "ZAMAAN ALI · VANGUARD",
            "ZANE ALLYN · VANGUARD",          "FALKO BANASZAK · VANGUARD",
            "MATTHIAS BELLER · VANGUARD",     "NICOLAS BONNET · VANGUARD",
            "PETR BOUSKA · VANGUARD",          "MARK BOOTHMAN · VANGUARD",
            "GEOFF BURKE · VANGUARD",          "CHRIS CHILDERHOSE · VANGUARD",
            "CHRISTIAN EROMOSELE · VANGUARD",  "JUSTIN FARRUGIA · VANGUARD",
            "CHRISTOPHER GLEMOT · VANGUARD",   "BEN HARMER · VANGUARD",
            "MARKUS HARTMANN · VANGUARD",      "JOE HOUGHES · VANGUARD",
            "DIDIER VAN HOYE · VANGUARD",      "JIM JONES · VANGUARD",
            "DAVE KAWULA · VANGUARD",          "MAURICE KEVENAAR · VANGUARD",
            "MIKHAIL KISSELYOV · VANGUARD",    "MICHAEL KREBS · VANGUARD",
            "LUKAS KRUSBERSKI · VANGUARD",     "DEREK LOSEKE · VANGUARD",
            "ERIC MACHABERT · VANGUARD",       "TOMASZ MAGDA · VANGUARD",
            "JOS MALIEPAARD · VANGUARD",       "ANDREA MAURO · VANGUARD",
            "JONAH MAY · VANGUARD",            "TOM MAY · VANGUARD",
            "STEVEN NEW · VANGUARD",           "OTHON OLIVEIRA · VANGUARD",
            "NIKLAS PAULI · VANGUARD",         "KRISTOF POPPE · VANGUARD",
            "ANDRE PULIA · VANGUARD",          "AL RASHEED · VANGUARD",
            "IAN SANDERSON · VANGUARD",        "JOAQUIM SANTOS · VANGUARD",
            "STEPHEN SEAGRAVE · VANGUARD",     "VLADAN SEGET · VANGUARD",
            "LUIZ SERRANO · VANGUARD",         "KEIRAN SHELDEN · VANGUARD",
            "MARCO SORRENTINO · VANGUARD",     "ELIZABETH SOUZA · VANGUARD",
            "PETER STEFFAN · VANGUARD",        "NICO STEIN · VANGUARD",
            "CARY SUN · VANGUARD",             "LEAHA TORRES · VANGUARD",
            "PAOLO VALSECCHI · VANGUARD",      "FEDERICO VENIER · VANGUARD",
            "VICTOR WU · VANGUARD",            "MATEUS WOLFF · VANGUARD",

            // ── Veeam Legends 2026 (20) ────────────────────────────────────────
            "CHRIS CHILDERHOSE · LEGEND",      "DAMIEN COMMENGE · LEGEND",
            "ANTONIO D'ANDREA · LEGEND",        "DANNY DE HEER · LEGEND",
            "PHILIPPE DUPUIS · LEGEND",         "CHALID FATHALLAH · LEGEND",
            "LUIS FREIXAS · LEGEND",            "MATHEUS GIOVANINI · LEGEND",
            "MARCEL KACMAR · LEGEND",           "MARKUS KRETZER · LEGEND",
            "STEPHAN LANG · LEGEND",            "TIMO MARFURT · LEGEND",
            "MICHAEL MELTER · LEGEND",          "TOMMY O'SHEA · LEGEND",
            "SCOTT PATTERSON · LEGEND",         "JEAN SCZEPANSKI PERES · LEGEND",
            "LUCA PORFIRI · LEGEND",            "ESTEBAN PRIETO · LEGEND",
            "ALESSANDRO TINIVELLI · LEGEND",    "SHANE WILLIFORD · LEGEND",

            // ── Veeam MVP 2026 (27) ────────────────────────────────────────────
            "ANDRE ATKINSON · MVP",             "CHRIS ARCENEAUX · MVP",
            "DAVID BEWERNICK · MVP",            "PATRICIO CERDA · MVP",
            "DANILO CHIAVARI · MVP",            "ADAM CONGDON · MVP",
            "RICARDO CONZATTI · MVP",           "LUCA DELL'OCA · MVP",
            "JOE GREMILLION · MVP",             "RYAN JOHNSTON · MVP",
            "ADRIAN LOWE · MVP",                "RAGHU MANIVANNAN · MVP",
            "STIJN MARIVOET · MVP",             "WESLEY MARTINS · MVP",
            "MAXIMILIAN MAIER · MVP",           "BRANDON MCCOY · MVP",
            "CHRIS MCDONALD · MVP",             "MARVIN MICHALSKI · MVP",
            "MICHAEL PAUL · MVP",               "YOUSSEF SALEM · MVP",
            "SEAN SIMPSON · MVP",               "TIM SMITH · MVP",
            "BEN THOMAS · MVP",                 "BAPTISTE TELLIER · MVP",
            "DAVID TOSOFF · MVP",               "LEI WEI · MVP",
            "MICHAEL WISNIEWSKI · MVP",
        };

        private class WDrop
        {
            public char[]  Chars;
            public float   X, Y, V;
            public bool    Glow;
            // static-mode fields (Fade / Build)
            public int     Phase, Frame, AppearF, HoldF, FadeF;
        }

        private enum PopupMode { Fade, Flash, Glitch, Scan, Zoom }
        private class WPopup
        {
            public char[] Word, Disp;
            public float  CX, CY;
            public int    FontSize;
            public PopupMode Mode;
            public bool   Glow;
            public int    Phase, Frame, AppearF, HoldF, DisappearF;
        }

        private readonly Settings      s;
        private readonly int           W, H;
        private readonly Random        rng     = new Random();
        private readonly string[]      allTerms;
        private readonly PopupMode[]   enabledModes;

        private Bitmap      buf;
        private Graphics    bg;
        private Font        rainFont, wordFont;
        private SolidBrush  fadeBrush, rainBrush, brightBrush, tmpBrush;
        private Bitmap      scanBmp;

        private float[] lanePos, laneSpeed;
        private bool[]  laneBright;
        private int     laneCount;

        private readonly List<WDrop>  wdrops = new List<WDrop>();
        private readonly List<WPopup> popups = new List<WPopup>();

        private bool IsVertical { get { return s.Orientation == "TopDown"   || s.Orientation == "BottomUp"; } }
        private bool IsForward  { get { return s.Orientation == "TopDown"   || s.Orientation == "LeftRight"; } }

        // Word drops can have their own independent orientation
        private string EffWordOrient { get { return (s.WordOrientation == "Same" || string.IsNullOrEmpty(s.WordOrientation)) ? s.Orientation : s.WordOrientation; } }
        private bool WordIsVertical  { get { return EffWordOrient == "TopDown"  || EffWordOrient == "BottomUp";  } }
        private bool WordIsForward   { get { return EffWordOrient == "TopDown"  || EffWordOrient == "LeftRight"; } }

        public MatrixEngine(Settings settings, int w, int h)
        {
            s = settings; W = w; H = h;
            var list = new List<string>(TERMS);
            if (s.ShowVeeam100)
                list.AddRange(VEEAM100_PEOPLE);
            if (!string.IsNullOrEmpty(s.ExtraWords))
                foreach (string p in s.ExtraWords.Split(','))
                { string t = p.Trim().ToUpper(); if (t.Length > 0) list.Add(t); }
            allTerms = list.ToArray();

            // Parse enabled popup effects
            var modes = new List<PopupMode>();
            foreach (string part in s.PopupEffects.Split(','))
            {
                PopupMode m;
                if (TryParseMode(part.Trim(), out m)) modes.Add(m);
            }
            enabledModes = modes.Count > 0 ? modes.ToArray() : new PopupMode[]{ PopupMode.Fade };

            Build();
        }

        private static bool TryParseMode(string name, out PopupMode m)
        {
            switch (name.ToLower())
            {
                case "fade":   m = PopupMode.Fade;   return true;
                case "flash":  m = PopupMode.Flash;  return true;
                case "glitch": m = PopupMode.Glitch; return true;
                case "scan":   m = PopupMode.Scan;   return true;
                case "zoom":   m = PopupMode.Zoom;   return true;
                default:       m = PopupMode.Fade;   return false;
            }
        }

        private void Build()
        {
            DisposeAll();
            buf = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bg  = Graphics.FromImage(buf);
            bg.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            bg.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.None;
            bg.Clear(Color.Black);

            rainFont    = new Font("Courier New", Math.Max(6, s.FontSize    - 1), FontStyle.Bold, GraphicsUnit.Pixel);
            // Use selected system font for word drops and popups
            wordFont    = new Font(s.WordFontName, Math.Max(6, s.WordFontSize - 1), FontStyle.Bold, GraphicsUnit.Pixel);
            fadeBrush   = new SolidBrush(Color.FromArgb(Math.Max(2, Math.Min(60, s.FadeAlpha)), 0, 0, 0));
            rainBrush   = new SolidBrush(s.RainColor);
            brightBrush = new SolidBrush(s.HeadColor);
            tmpBrush    = new SolidBrush(Color.White);

            int fs = s.FontSize;
            laneCount = IsVertical ? Math.Max(1, W / fs) : Math.Max(1, H / fs);
            int limit = IsVertical ? H / fs : W / fs;
            lanePos   = new float[laneCount]; laneSpeed = new float[laneCount]; laneBright = new bool[laneCount];
            for (int i = 0; i < laneCount; i++)
            {
                lanePos[i]    = (float)(rng.NextDouble() * limit);
                laneSpeed[i]  = (float)((0.2 + rng.NextDouble() * 0.9) * s.SpeedFactor);
                laneBright[i] = rng.NextDouble() < 0.1;
            }

            wdrops.Clear();
            if (s.WordMode == "Rain" || s.WordMode == "Both")
                for (int i = 0; i < s.WordCount; i++) wdrops.Add(SpawnDrop(true));

            popups.Clear();
            if (s.WordMode == "Popup" || s.WordMode == "Both")
                for (int i = 0; i < s.PopupCount; i++) popups.Add(SpawnPopup(true));

            BuildScanlines();
        }

        private WDrop SpawnDrop(bool scatter)
        {
            string term  = allTerms[rng.Next(allTerms.Length)];
            char[] chars = term.ToCharArray();
            int    fs    = s.WordFontSize;

            // ── Static modes: word sits at a fixed position ───────────────────
            if (s.WordStyle == "Fade" || s.WordStyle == "Build")
            {
                // WordSpeedFactor affects how long each phase takes (lower factor = longer display)
                float  spd   = Math.Max(0.1f, s.WordSpeedFactor);
                int    appF  = (int)Math.Round((s.WordStyle == "Build" ? Math.Max(20, chars.Length * 5) : 22) / spd);
                int    holF  = (int)Math.Round((70 + rng.Next(50)) / spd);
                int    fadF  = 30;
                float  estW  = chars.Length * fs * 0.62f;
                float  mgn   = fs * 2.5f;
                float  sx    = mgn + (float)(rng.NextDouble() * Math.Max(1f, W - estW - mgn));
                float  sy    = mgn + (float)(rng.NextDouble() * Math.Max(1f, H - fs * 3f - mgn));
                var    wd    = new WDrop { Chars=chars, X=sx, Y=sy, V=0,
                                           Glow=rng.NextDouble()<s.GlowChance,
                                           AppearF=appF, HoldF=holF, FadeF=fadF };
                if (scatter) // pre-scatter across all phases so screen is populated from the start
                {
                    int tot = appF + holF + fadF;
                    int rf  = rng.Next(tot);
                    if      (rf < appF)       { wd.Phase=0; wd.Frame=rf; }
                    else if (rf < appF+holF)  { wd.Phase=1; wd.Frame=rf-appF; }
                    else                       { wd.Phase=2; wd.Frame=rf-appF-holF; }
                }
                return wd;
            }

            // ── Scroll mode: word travels in a direction ──────────────────────
            float len = chars.Length * fs;
            float v   = (float)((0.6 + rng.NextDouble() * 1.6) * s.WordSpeedFactor);
            float x, y;
            if (WordIsVertical)
            { x=fs*.5f+(float)(rng.NextDouble()*Math.Max(1,W-fs)); y=scatter?(float)(rng.NextDouble()*(H+len)-len):(WordIsForward?-(len+5):H+5); }
            else
            { y=fs*.5f+(float)(rng.NextDouble()*Math.Max(1,H-fs)); x=scatter?(float)(rng.NextDouble()*(W+len)-len):(WordIsForward?-(len+5):W+5); }
            return new WDrop { Chars=chars, X=x, Y=y, V=WordIsForward?v:-v, Glow=rng.NextDouble()<s.GlowChance };
        }

        private WPopup SpawnPopup(bool scatter)
        {
            string term = allTerms[rng.Next(allTerms.Length)];
            char[] word = term.ToCharArray();
            int  fs     = Math.Max(6, s.PopupFontSize);
            PopupMode mode = enabledModes[rng.Next(enabledModes.Length)];

            int appearF, holdF, disappearF;
            switch (mode)
            {
                case PopupMode.Flash:  appearF=4;  holdF=18; disappearF=28; break;
                case PopupMode.Glitch: appearF=55; holdF=90; disappearF=35; break;
                case PopupMode.Scan:   appearF=word.Length*3; holdF=80; disappearF=28; break;
                case PopupMode.Zoom:   appearF=18; holdF=90; disappearF=28; break;
                default:               appearF=22; holdF=90; disappearF=32; break;
            }

            float estW = word.Length * fs * 0.64f, margin = fs * 2f;
            float cx   = margin + estW/2f + (float)(rng.NextDouble() * Math.Max(1, W - estW - 2f*margin));
            float cy   = margin + (float)(rng.NextDouble() * Math.Max(1, H - 2f*margin));
            if (scatter) cy = margin + (float)(rng.NextDouble() * Math.Max(1, H - 2f*margin));

            char[] disp = (char[])word.Clone();
            if (mode == PopupMode.Glitch || mode == PopupMode.Scan)
                for (int i=0;i<disp.Length;i++) disp[i]=word[i]==' '?' ':RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];

            return new WPopup { Word=word, Disp=disp, CX=cx, CY=cy, FontSize=fs, Mode=mode,
                                Glow=rng.NextDouble()<s.GlowChance, Phase=0, Frame=0,
                                AppearF=appearF, HoldF=holdF, DisappearF=disappearF };
        }

        private void BuildScanlines()
        {
            if (!s.ShowScanlines && !s.ShowWatermark) return;
            scanBmp = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(scanBmp))
            {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                if (s.ShowScanlines)
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(20,0,0,0)))
                        for (int y=0;y<H;y+=2) g.FillRectangle(sb,0,y,W,1);
                if (s.ShowWatermark)
                {
                    string wmText = string.IsNullOrWhiteSpace(s.WatermarkText) ? "VEEAM" : s.WatermarkText.ToUpper();
                    string wmSub  = s.WatermarkSubText ?? "";
                    int lsz = Math.Max(24,(int)(W*0.08));
                    using (Font lf = new Font(s.WordFontName, lsz, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (SolidBrush lb = new SolidBrush(Color.FromArgb(10, s.RainColor)))
                    { SizeF ls=g.MeasureString(wmText,lf); g.DrawString(wmText,lf,lb,(W-ls.Width)/2f,(H-ls.Height)/2f); }
                    if (wmSub.Length > 0)
                    {
                        int ssz = Math.Max(7,(int)(W*0.015));
                        using (Font sf = new Font(s.WordFontName, ssz, FontStyle.Regular, GraphicsUnit.Pixel))
                        using (SolidBrush sb = new SolidBrush(Color.FromArgb(7, s.RainColor)))
                        { SizeF ss=g.MeasureString(wmSub,sf); g.DrawString(wmSub,sf,sb,(W-ss.Width)/2f,H/2f+(int)(W*0.045f)); }
                    }
                }
            }
        }

        public void Tick()
        {
            bg.FillRectangle(fadeBrush,0,0,W,H);
            DrawRain();
            if (s.WordMode=="Rain"||s.WordMode=="Both") DrawDrops();
            if (s.WordMode=="Popup"||s.WordMode=="Both") DrawPopups();
            if (scanBmp!=null) bg.DrawImage(scanBmp,0,0);
        }

        private void DrawRain()
        {
            int fs = s.FontSize;
            for (int i=0;i<laneCount;i++)
            {
                char ch = RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];
                float px,py;
                if (IsVertical){px=i*fs;py=IsForward?lanePos[i]*fs:H-lanePos[i]*fs;}
                else           {py=i*fs;px=IsForward?lanePos[i]*fs:W-lanePos[i]*fs;}
                SolidBrush br=laneBright[i]?brightBrush:rainBrush;
                bg.DrawString(ch.ToString(),rainFont,br,px,py);
                lanePos[i]+=laneSpeed[i];
                float maxP=IsVertical?(float)H/fs:(float)W/fs;
                if(lanePos[i]>maxP&&rng.NextDouble()>0.975){lanePos[i]=0;laneSpeed[i]=(float)((0.2+rng.NextDouble()*0.9)*s.SpeedFactor);laneBright[i]=rng.NextDouble()<0.1;}
            }
        }

        private void DrawDrops()
        {
            bool isStatic = (s.WordStyle == "Fade" || s.WordStyle == "Build");
            int  fs       = s.WordFontSize;

            for (int i = wdrops.Count-1; i >= 0; i--)
            {
                WDrop w = wdrops[i];

                if (isStatic)
                {
                    if (!TickStaticDrop(w)) { wdrops.RemoveAt(i); wdrops.Add(SpawnDrop(false)); }
                }
                else
                {
                    // ── Scroll mode ──────────────────────────────────────────
                    int n = w.Chars.Length;
                    for (int j = 0; j < n; j++)
                    {
                        float px=WordIsVertical?w.X:w.X+j*fs, py=WordIsVertical?w.Y+j*fs:w.Y;
                        if (px<-fs||px>W+fs||py<-fs||py>H+fs) continue;
                        float fade=n>1?(float)j/(n-1):0f;
                        int   a   =Clamp((int)(255*(1f-fade*0.55f)));
                        Color col;
                        if (j==0) col=s.WordHeadColor;
                        else if (w.Glow) col=Color.FromArgb(a,Clamp(s.WordColor.R+80),Clamp(s.WordColor.G+20),Clamp(s.WordColor.B+40));
                        else col=Color.FromArgb(a,Clamp((int)(s.WordColor.R*(1-fade*.5f))),Clamp((int)(s.WordColor.G*(1-fade*.3f)+30*(1-fade))),Clamp((int)(s.WordColor.B*(1-fade*.5f))));
                        tmpBrush.Color=col;
                        bg.DrawString(w.Chars[j].ToString(),wordFont,tmpBrush,px,py);
                    }
                    if (WordIsVertical) w.Y+=w.V; else w.X+=w.V;
                    bool gone=WordIsVertical?(WordIsForward?w.Y>H+5:w.Y+n*fs<-5):(WordIsForward?w.X>W+5:w.X+n*fs<-5);
                    if (gone) { wdrops.RemoveAt(i); wdrops.Add(SpawnDrop(false)); }
                }
            }
        }

        // Returns false when the drop has finished and should be respawned.
        private bool TickStaticDrop(WDrop w)
        {
            w.Frame++;
            if (w.Phase==0 && w.Frame>=w.AppearF) { w.Phase=1; w.Frame=0; }
            else if (w.Phase==1 && w.Frame>=w.HoldF)  { w.Phase=2; w.Frame=0; }
            else if (w.Phase==2 && w.Frame>=w.FadeF)   return false;

            float prog  = w.Phase==0 ? (float)w.Frame/Math.Max(1,w.AppearF)
                        : w.Phase==2 ? 1f-(float)w.Frame/Math.Max(1,w.FadeF)
                        : 1f;
            int   alpha = Clamp((int)(prog*255));
            if (alpha < 3) return w.Phase < 2;

            int fs = s.WordFontSize;

            if (s.WordStyle == "Build")
            {
                // Reveal chars left-to-right; active char briefly scrambles before snapping
                int total   = w.Chars.Length;
                int snapped = w.Phase==0
                    ? Math.Min(total, w.Frame * total / Math.Max(1, w.AppearF) + 1)
                    : total;

                for (int j = 0; j < snapped; j++)
                {
                    // Active (last) char in build phase: scramble randomly
                    char drawCh = (j == snapped-1 && w.Phase==0)
                        ? ((w.Frame % 4 < 2) ? RAIN_CHARS[rng.Next(RAIN_CHARS.Length)] : w.Chars[j])
                        : w.Chars[j];
                    if (drawCh == ' ') continue;

                    Color col;
                    if (j == snapped-1 && w.Phase==0)
                        // active position: head colour
                        col = Color.FromArgb(alpha, s.WordHeadColor.R, s.WordHeadColor.G, s.WordHeadColor.B);
                    else if (w.Glow)
                        col = Color.FromArgb(alpha, Clamp(s.WordColor.R+80), Clamp(s.WordColor.G+20), Clamp(s.WordColor.B+40));
                    else
                        col = Color.FromArgb(alpha, s.WordColor.R, s.WordColor.G, s.WordColor.B);

                    tmpBrush.Color = col;
                    bg.DrawString(drawCh.ToString(), wordFont, tmpBrush, w.X + j*fs*0.61f, w.Y);
                }
            }
            else // Fade: whole word appears / disappears as one unit
            {
                Color col = w.Glow
                    ? Color.FromArgb(alpha, Clamp(s.WordColor.R+80), Clamp(s.WordColor.G+20), Clamp(s.WordColor.B+40))
                    : Color.FromArgb(alpha, s.WordColor.R, s.WordColor.G, s.WordColor.B);
                tmpBrush.Color = col;
                bg.DrawString(new string(w.Chars), wordFont, tmpBrush, w.X, w.Y);
            }
            return true;
        }

        private void DrawPopups()
        {
            for(int i=popups.Count-1;i>=0;i--)
            {
                WPopup p=popups[i]; p.Frame++;
                if(p.Phase==0&&p.Frame>=p.AppearF){p.Phase=1;p.Frame=0;}
                else if(p.Phase==1&&p.Frame>=p.HoldF){p.Phase=2;p.Frame=0;}
                else if(p.Phase==2&&p.Frame>=p.DisappearF){p.Phase=3;}
                if(p.Phase==3){popups.RemoveAt(i);popups.Add(SpawnPopup(false));continue;}

                float prog=p.Phase==0?(float)p.Frame/Math.Max(1,p.AppearF):p.Phase==2?1f-(float)p.Frame/Math.Max(1,p.DisappearF):1f;
                switch(p.Mode)
                {
                    case PopupMode.Fade:   PaintPopup(p.Word,p.CX,p.CY,p.FontSize,prog,p.Glow,false); break;
                    case PopupMode.Flash:  PaintPopup(p.Word,p.CX,p.CY,p.FontSize,p.Phase==1?1f:prog,true,p.Phase==1); break;
                    case PopupMode.Glitch: DoGlitch(p,prog); break;
                    case PopupMode.Scan:   DoScan(p,prog);   break;
                    case PopupMode.Zoom:   DoZoom(p,prog);   break;
                }
            }
        }

        private void DoGlitch(WPopup p, float prog)
        {
            for(int j=0;j<p.Word.Length;j++)
            {
                if(p.Word[j]==' '){p.Disp[j]=' ';continue;}
                if(p.Phase==0)      p.Disp[j]=rng.NextDouble()<prog*1.4?p.Word[j]:RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];
                else if(p.Phase==1) p.Disp[j]=rng.NextDouble()<0.025?RAIN_CHARS[rng.Next(RAIN_CHARS.Length)]:p.Word[j];
                else                p.Disp[j]=rng.NextDouble()<prog?p.Word[j]:RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];
            }
            PaintPopup(p.Disp,p.CX,p.CY,p.FontSize,p.Phase==0?prog*0.8f+0.2f:prog,p.Glow,false);
        }

        private void DoScan(WPopup p, float prog)
        {
            int len=p.Word.Length;
            if(p.Phase==0)
            {
                int rev=Math.Min(len,(int)(prog*len));
                for(int j=0;j<len;j++)
                {
                    if(j<rev) p.Disp[j]=p.Word[j];
                    else if(j==rev) p.Disp[j]=(p.Frame%5<3)?'_':' ';
                    else p.Disp[j]=' ';
                }
                PaintPopup(p.Disp,p.CX,p.CY,p.FontSize,1f,p.Glow,false);
            }
            else if(p.Phase==1)
            { for(int j=0;j<len;j++) p.Disp[j]=p.Word[j]; PaintPopup(p.Disp,p.CX,p.CY,p.FontSize,1f,p.Glow,false); }
            else
            {
                int hide=Math.Min(len,(int)((1f-prog)*len));
                for(int j=0;j<len;j++) p.Disp[j]=j>=hide?' ':p.Word[j];
                PaintPopup(p.Disp,p.CX,p.CY,p.FontSize,prog,p.Glow,false);
            }
        }

        private void DoZoom(WPopup p, float prog)
        {
            float scale=p.Phase==0?2f-prog:1f;
            float alpha=p.Phase==0?prog*prog:prog;
            PaintPopup(p.Word,p.CX,p.CY,(int)(p.FontSize*scale),alpha,p.Glow,false);
        }

        private void PaintPopup(char[] chars,float cx,float cy,int fontSize,float alpha,bool glow,bool flash)
        {
            if(alpha<=0.01f) return;
            int a=Clamp((int)(alpha*255));
            Font useFont=null; bool own=false;
            if(fontSize==s.WordFontSize-1||fontSize==s.WordFontSize) useFont=wordFont;
            else { useFont=new Font("Courier New",Math.Max(6,fontSize),FontStyle.Bold,GraphicsUnit.Pixel); own=true; }
            try
            {
                string text=new string(chars);
                SizeF  sz=bg.MeasureString(text,useFont);
                Color  col;
                if(flash) col=Color.FromArgb(a,255,255,255);
                else if(glow) col=Color.FromArgb(a,Clamp(s.PopupColor.R+90),Clamp(s.PopupColor.G+20),Clamp(s.PopupColor.B+60));
                else col=Color.FromArgb(a,s.PopupColor.R,s.PopupColor.G,s.PopupColor.B);
                tmpBrush.Color=col;
                bg.DrawString(text,useFont,tmpBrush,cx-sz.Width/2f,cy-sz.Height/2f);
            }
            finally { if(own&&useFont!=null) useFont.Dispose(); }
        }

        private static int Clamp(int v){return v<0?0:v>255?255:v;}
        public void Render(Graphics g){g.DrawImage(buf,0,0);}

        private void DisposeAll()
        {
            if(fadeBrush  !=null){fadeBrush.Dispose();  fadeBrush  =null;}
            if(rainBrush  !=null){rainBrush.Dispose();  rainBrush  =null;}
            if(brightBrush!=null){brightBrush.Dispose();brightBrush=null;}
            if(tmpBrush   !=null){tmpBrush.Dispose();   tmpBrush   =null;}
            if(bg         !=null){bg.Dispose();         bg         =null;}
            if(buf        !=null){buf.Dispose();        buf        =null;}
            if(rainFont   !=null){rainFont.Dispose();   rainFont   =null;}
            if(wordFont   !=null){wordFont.Dispose();   wordFont   =null;}
            if(scanBmp    !=null){scanBmp.Dispose();    scanBmp    =null;}
        }
        public void Dispose(){DisposeAll();}
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
            StartPosition   = FormStartPosition.Manual;
            Location        = new Point(bounds.Left, bounds.Top);
            Size            = new Size(bounds.Width, bounds.Height);
            BackColor       = Color.Black;
            TopMost         = true;
            ShowInTaskbar   = false;
            SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer, true);

            engine = new MatrixEngine(s, bounds.Width, bounds.Height);
            timer  = new Timer { Interval = 25 };
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

        protected override void OnPaintBackground(PaintEventArgs e){}
        private void OnTick(object sender,EventArgs e){engine.Tick();Invalidate(false);}
        private void OnMouseMove(object sender, MouseEventArgs e)
        { if(firstMove){lastMouse=e.Location;firstMove=false;return;} if(Math.Abs(e.X-lastMouse.X)>5||Math.Abs(e.Y-lastMouse.Y)>5)Close(); }
        protected override void OnPaint(PaintEventArgs e){engine.Render(e.Graphics);}
        protected override void Dispose(bool d){if(d){timer.Dispose();engine.Dispose();}base.Dispose(d);}
    }

    // =========================================================================
    // PREVIEW FORM
    // =========================================================================
    class PreviewForm : Form
    {
        [DllImport("user32.dll")] static extern IntPtr SetParent(IntPtr child,IntPtr parent);
        [DllImport("user32.dll")] static extern bool   GetClientRect(IntPtr hwnd,out NativeRect r);
        [StructLayout(LayoutKind.Sequential)] struct NativeRect{public int Left,Top,Right,Bottom;}

        private readonly MatrixEngine engine;
        private readonly Timer        timer;

        public PreviewForm(Settings s, IntPtr parentHwnd)
        {
            FormBorderStyle=FormBorderStyle.None; BackColor=Color.Black; ShowInTaskbar=false;
            NativeRect nr; GetClientRect(parentHwnd,out nr);
            Bounds=new Rectangle(0,0,nr.Right-nr.Left,nr.Bottom-nr.Top);
            SetParent(Handle,parentHwnd);
            SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);
            engine=new MatrixEngine(s,Width,Height);
            timer=new Timer{Interval=40};
            timer.Tick+=delegate{engine.Tick();Invalidate(false);};
            timer.Start();
        }
        protected override void OnPaintBackground(PaintEventArgs e){}
        protected override void OnPaint(PaintEventArgs e){engine.Render(e.Graphics);}
        protected override void Dispose(bool d){if(d){timer.Dispose();engine.Dispose();}base.Dispose(d);}
    }

    // =========================================================================
    // CONFIG FORM
    // =========================================================================
    class ConfigForm : Form
    {
        private Settings   cur;
        private Button     btnRainColor, btnHeadColor, btnWordColor, btnWordHeadColor, btnPopupColor;
        private TrackBar   trkFade, trkFont, trkSpeed, trkWordCount, trkWordFont, trkPopupCount, trkPopupFont, trkWordSpeed;
        private Label      lblFade, lblFont, lblSpeed, lblWCount, lblWFont, lblPCount, lblPFont, lblWordSpeed;
        private ComboBox   cboOrient, cboWordOrient, cboWordMode, cboWordStyle;
        private TextBox    txtWatermark, txtWatermarkSub, txtExtra;
        private CheckBox   chkFade, chkFlash, chkGlitch, chkScan, chkZoom;
        private CheckBox   chkScanlines, chkWatermark, chkVeeam100;
        private ComboBox   cboProfiles;
        private List<ColorProfile> profiles;
        private ComboBox   cboWordFontName;
        private PictureBox picFontPreview;
        private TextBox    txtFontPreviewText;

        public Settings Result { get; private set; }

        public ConfigForm(Settings s)
        {
            cur = Clone(s);
            Text = "Veeam Matrix  –  Einstellungen";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            ClientSize  = new Size(860, 100);
            BackColor   = Color.FromArgb(18, 18, 18);
            ForeColor   = Color.FromArgb(0, 200, 55);
            Font        = new Font("Segoe UI", 9f);
            profiles    = ColorProfile.LoadAll();
            Build();
        }

        private static Settings Clone(Settings s)
        {
            return new Settings {
                RainColor=s.RainColor, HeadColor=s.HeadColor, FadeAlpha=s.FadeAlpha, FontSize=s.FontSize,
                SpeedFactor=s.SpeedFactor, ShowScanlines=s.ShowScanlines, ShowWatermark=s.ShowWatermark,
                WordMode=s.WordMode, WordCount=s.WordCount, WordFontSize=s.WordFontSize,
                WordColor=s.WordColor, WordHeadColor=s.WordHeadColor, GlowChance=s.GlowChance,
                PopupEffects=s.PopupEffects, PopupCount=s.PopupCount, PopupFontSize=s.PopupFontSize,
                PopupColor=s.PopupColor, Orientation=s.Orientation, WordOrientation=s.WordOrientation,
                WordStyle=s.WordStyle, WordSpeedFactor=s.WordSpeedFactor, ShowVeeam100=s.ShowVeeam100,
                WatermarkText=s.WatermarkText, WatermarkSubText=s.WatermarkSubText, ExtraWords=s.ExtraWords,
                WordFontName=s.WordFontName };
        }

        // ── layout helpers ────────────────────────────────────────────────────
        private static int Clamp(int v, int lo, int hi) { return v<lo?lo:v>hi?hi:v; }
        private static void SetBtn(Button b, Color c)
        { b.BackColor=c; b.ForeColor=c.GetBrightness()>.45f?Color.Black:Color.White; }

        // Coloured section-header bar (dark green bg + bold label)
        private void Section(string title, int x, int y, int w)
        {
            var pnl = new Panel { Location=new Point(x,y), Size=new Size(w,20),
                BackColor=Color.FromArgb(0,50,16) };
            Controls.Add(pnl);
            pnl.Controls.Add(new Label { Text=title, Location=new Point(8,2), AutoSize=true,
                ForeColor=Color.FromArgb(0,220,65),
                Font=new Font("Segoe UI",8.5f,FontStyle.Bold) });
        }

        // Horizontal separator line
        private void HSep(int y, int x=14, int w=832)
        { Controls.Add(new Panel { Location=new Point(x,y), Size=new Size(w,1), BackColor=Color.FromArgb(0,52,16) }); }

        // Dim label (grey, for captions)
        private Label DLbl(string t, int x, int y, int w=-1)
        {
            var l = new Label { Text=t, Location=new Point(x,y), AutoSize=(w<0),
                ForeColor=Color.FromArgb(155,155,155) };
            if (w>0) { l.AutoSize=false; l.Size=new Size(w,15); }
            Controls.Add(l); return l;
        }

        // Coloured button for picking a colour
        private Button ColBtn(string text, Color col, int x, int y, int w=130)
        {
            var b = new Button { Text=text, Location=new Point(x,y), Size=new Size(w,26),
                BackColor=col, ForeColor=col.GetBrightness()>.45f?Color.Black:Color.White,
                FlatStyle=FlatStyle.Flat };
            b.FlatAppearance.BorderColor = Color.FromArgb(0,85,24);
            Controls.Add(b); return b;
        }

        // DropDown combo
        private ComboBox Cbo(int x, int y, int w, string[] items, string sel)
        {
            var c = new ComboBox { Location=new Point(x,y), Size=new Size(w,24),
                DropDownStyle=ComboBoxStyle.DropDownList,
                BackColor=Color.FromArgb(28,28,28), ForeColor=Color.FromArgb(0,210,60) };
            c.Items.AddRange(items); c.Text=sel;
            Controls.Add(c); return c;
        }

        // Checkbox with grey label
        private CheckBox Chk(string text, bool val, int x, int y)
        {
            var c = new CheckBox { Text=text, Checked=val, Location=new Point(x,y),
                AutoSize=true, ForeColor=Color.FromArgb(165,165,165) };
            Controls.Add(c); return c;
        }

        // Single-row slider: [dim label 128px] [──trackbar──] [green value 54px]
        // Returns the TrackBar; vLbl receives the value label reference.
        private TrackBar SlRow(string name, int x, int y, int cw, int min, int max, int val, out Label vLbl)
        {
            const int LW=128, VW=54, G=6;
            DLbl(name, x, y+6, LW);
            var trk = new TrackBar { Location=new Point(x+LW+G,y), Size=new Size(cw-LW-VW-G*2,26),
                Minimum=min, Maximum=max, Value=Clamp(val,min,max),
                TickFrequency=Math.Max(1,(max-min)/10), SmallChange=1,
                BackColor=Color.FromArgb(22,22,22), TickStyle=TickStyle.None };
            Controls.Add(trk);
            vLbl = new Label { Location=new Point(x+cw-VW,y+6), Size=new Size(VW,15),
                ForeColor=Color.FromArgb(0,218,62), TextAlign=ContentAlignment.MiddleRight };
            Controls.Add(vLbl);
            return trk;
        }

        private void Build()
        {
            const int c1=14, cW1=400;   // left column
            const int c2=428, cW2=418;  // right column
            const int SL=42;            // slider step
            const int CM=32;            // combo/button row step

            int y = 14;

            // ── Colour profile strip (full width) ────────────────────────────
            DLbl("Farbprofil:", c1, y+5);
            var profileNames = new List<string>();
            foreach (var p in profiles) profileNames.Add(p.Name);
            cboProfiles = Cbo(c1+88, y, 180, profileNames.ToArray(), "");
            var btnLoad = new Button { Text="Laden", Location=new Point(c1+276,y), Size=new Size(72,24),
                BackColor=Color.FromArgb(0,76,22), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            btnLoad.FlatAppearance.BorderColor = Color.FromArgb(0,110,32);
            var btnSave = new Button { Text="Speichern als…", Location=new Point(c1+356,y), Size=new Size(128,24),
                BackColor=Color.FromArgb(34,34,0), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            btnSave.FlatAppearance.BorderColor = Color.FromArgb(90,90,0);
            btnLoad.Click += delegate { LoadSelectedProfile(); };
            btnSave.Click += delegate { SaveCurrentAsProfile(); };
            Controls.Add(btnLoad); Controls.Add(btnSave);
            y += 34; HSep(y); y += 12;

            // Two independent y cursors — left and right columns
            int yL = y, yR = y;

            // ═══ LEFT COLUMN ═════════════════════════════════════════════════

            // ── REGEN ─────────────────────────────────────────────────────────
            Section("REGEN", c1, yL, cW1); yL += 26;
            DLbl("Zeichen:", c1, yL+5); yL += 20;
            btnRainColor = ColBtn("Zeichen",     cur.RainColor, c1,     yL);
            btnHeadColor = ColBtn("Kopf (hell)", cur.HeadColor, c1+136, yL);
            btnRainColor.Click += delegate { Pick(ref cur.RainColor, btnRainColor); };
            btnHeadColor.Click += delegate { Pick(ref cur.HeadColor, btnHeadColor); };
            yL += 32;

            trkFont  = SlRow("Schriftgroesse", c1,yL,cW1, 8,36, cur.FontSize, out lblFont);
            lblFont.Text = cur.FontSize+" px";
            trkFont.ValueChanged += delegate { cur.FontSize=trkFont.Value; lblFont.Text=cur.FontSize+" px"; };
            yL += SL;

            trkSpeed = SlRow("Geschwindigkeit", c1,yL,cW1, 1,30, (int)(cur.SpeedFactor*10), out lblSpeed);
            lblSpeed.Text = cur.SpeedFactor.ToString("F1")+"x";
            trkSpeed.ValueChanged += delegate { cur.SpeedFactor=trkSpeed.Value/10f; lblSpeed.Text=cur.SpeedFactor.ToString("F1")+"x"; };
            yL += SL;

            trkFade  = SlRow("Spurlaenge", c1,yL,cW1, 2,60, cur.FadeAlpha, out lblFade);
            lblFade.Text = cur.FadeAlpha.ToString();
            trkFade.ValueChanged += delegate { cur.FadeAlpha=trkFade.Value; lblFade.Text=cur.FadeAlpha.ToString(); };
            yL += SL;

            DLbl("Richtung:", c1, yL+5, 72);
            cboOrient   = Cbo(c1+76,  yL, 138, new string[]{"TopDown","BottomUp","LeftRight","RightLeft"}, cur.Orientation);
            DLbl("Wortmodus:", c1+222, yL+5, 68);
            cboWordMode = Cbo(c1+294, yL, 102, new string[]{"Rain","Popup","Both"}, cur.WordMode);
            yL += CM;

            yL += 10;

            // ── POPUP-EFFEKTE ─────────────────────────────────────────────────
            Section("POPUP-EFFEKTE", c1, yL, cW1); yL += 26;
            DLbl("Farbe:", c1, yL+5, 46);
            btnPopupColor = ColBtn("Popup-Farbe", cur.PopupColor, c1+50, yL);
            btnPopupColor.Click += delegate { Pick(ref cur.PopupColor, btnPopupColor); };
            yL += 32;

            bool hasFade  = cur.PopupEffects.Contains("Fade");
            bool hasFlash = cur.PopupEffects.Contains("Flash");
            bool hasGlitch= cur.PopupEffects.Contains("Glitch");
            bool hasScan  = cur.PopupEffects.Contains("Scan");
            bool hasZoom  = cur.PopupEffects.Contains("Zoom");
            chkFade   = Chk("Fade",   hasFade,   c1,     yL);
            chkFlash  = Chk("Flash",  hasFlash,  c1+68,  yL);
            chkGlitch = Chk("Glitch", hasGlitch, c1+144, yL);
            chkScan   = Chk("Scan",   hasScan,   c1+222, yL);
            chkZoom   = Chk("Zoom",   hasZoom,   c1+296, yL);
            yL += 28;

            trkPopupFont  = SlRow("Schriftgroesse", c1,yL,cW1, 10,72, cur.PopupFontSize, out lblPFont);
            lblPFont.Text  = cur.PopupFontSize+" px";
            trkPopupFont.ValueChanged  += delegate { cur.PopupFontSize=trkPopupFont.Value;  lblPFont.Text=cur.PopupFontSize+" px"; };
            yL += SL;

            trkPopupCount = SlRow("Gleichzeitig", c1,yL,cW1, 1,20, cur.PopupCount, out lblPCount);
            lblPCount.Text = cur.PopupCount.ToString();
            trkPopupCount.ValueChanged += delegate { cur.PopupCount=trkPopupCount.Value; lblPCount.Text=cur.PopupCount.ToString(); };
            yL += SL;

            // ═══ RIGHT COLUMN ════════════════════════════════════════════════

            // ── WOERTER ───────────────────────────────────────────────────────
            Section("WÖRTER", c2, yR, cW2); yR += 26;
            DLbl("Farben:", c2, yR+5); yR += 20;
            btnWordColor     = ColBtn("Woerter",     cur.WordColor,     c2,     yR, 130);
            btnWordHeadColor = ColBtn("Kopf (hell)",  cur.WordHeadColor, c2+136, yR, 130);
            btnWordColor.Click     += delegate { Pick(ref cur.WordColor,     btnWordColor); };
            btnWordHeadColor.Click += delegate { Pick(ref cur.WordHeadColor, btnWordHeadColor); };
            yR += 32;

            // Font picker row: [label] [combo] [preview textbox]
            DLbl("Schriftart:", c2, yR+5, 80);
            cboWordFontName = new ComboBox { Location=new Point(c2+84, yR), Size=new Size(200, 24),
                DropDownStyle=ComboBoxStyle.DropDownList,
                BackColor=Color.FromArgb(28,28,28), ForeColor=Color.FromArgb(0,210,60) };
            using (var ifc = new System.Drawing.Text.InstalledFontCollection())
            {
                var fns = new System.Collections.Generic.SortedSet<string>();
                foreach (FontFamily ff in ifc.Families) fns.Add(ff.Name);
                foreach (string fn in fns) cboWordFontName.Items.Add(fn);
            }
            int selIdx = cboWordFontName.Items.IndexOf(cur.WordFontName);
            cboWordFontName.SelectedIndex = selIdx >= 0 ? selIdx : 0;
            Controls.Add(cboWordFontName);

            // Editable preview-text field (right of combo)
            txtFontPreviewText = new TextBox { Location=new Point(c2+292, yR), Size=new Size(cW2-292-4, 24),
                Text="VEEAM", BackColor=Color.FromArgb(28,28,28),
                ForeColor=Color.FromArgb(0,210,60), BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtFontPreviewText);
            yR += 30;

            // Preview panel (full column width, taller)
            picFontPreview = new PictureBox { Location=new Point(c2, yR), Size=new Size(cW2-4, 44),
                BackColor=Color.Black, BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(picFontPreview);
            UpdateFontPreview();
            cboWordFontName.SelectedIndexChanged += delegate { UpdateFontPreview(); };
            txtFontPreviewText.TextChanged       += delegate { UpdateFontPreview(); };
            yR += 50;

            // Style + direction on one row
            DLbl("Stil:", c2, yR+5, 36);
            cboWordStyle  = Cbo(c2+40,  yR, 130, new string[]{"Scroll","Fade","Build"},
                string.IsNullOrEmpty(cur.WordStyle)?"Scroll":cur.WordStyle);
            DLbl("Richtung:", c2+178, yR+5, 64);
            cboWordOrient = Cbo(c2+246, yR, 168, new string[]{"Same","TopDown","BottomUp","LeftRight","RightLeft"},
                string.IsNullOrEmpty(cur.WordOrientation)?"Same":cur.WordOrientation);
            yR += CM;

            trkWordFont  = SlRow("Schriftgroesse", c2,yR,cW2, 8,36, cur.WordFontSize, out lblWFont);
            lblWFont.Text  = cur.WordFontSize+" px";
            trkWordFont.ValueChanged  += delegate { cur.WordFontSize=trkWordFont.Value;   lblWFont.Text=cur.WordFontSize+" px"; };
            yR += SL;

            trkWordSpeed = SlRow("Geschwindigkeit", c2,yR,cW2, 1,30, (int)(cur.WordSpeedFactor*10), out lblWordSpeed);
            lblWordSpeed.Text = cur.WordSpeedFactor.ToString("F1")+"x";
            trkWordSpeed.ValueChanged += delegate { cur.WordSpeedFactor=trkWordSpeed.Value/10f; lblWordSpeed.Text=cur.WordSpeedFactor.ToString("F1")+"x"; };
            yR += SL;

            trkWordCount = SlRow("Gleichzeitig", c2,yR,cW2, 1,30, cur.WordCount, out lblWCount);
            lblWCount.Text = cur.WordCount.ToString();
            trkWordCount.ValueChanged += delegate { cur.WordCount=trkWordCount.Value; lblWCount.Text=cur.WordCount.ToString(); };
            yR += SL;

            yR += 10;

            // ── ALLGEMEIN ─────────────────────────────────────────────────────
            Section("ALLGEMEIN", c2, yR, cW2); yR += 26;
            chkScanlines = Chk("CRT-Scanlines",  cur.ShowScanlines, c2,     yR);
            chkWatermark = Chk("Wasserzeichen",   cur.ShowWatermark, c2+124, yR);
            chkVeeam100  = Chk("Veeam 100 Namen", cur.ShowVeeam100,  c2+252, yR);
            yR += 28;

            DLbl("Wasserzeichen:", c2, yR+5, 92);
            txtWatermark = new TextBox { Location=new Point(c2+96, yR), Size=new Size(174, 24),
                Text=cur.WatermarkText, BackColor=Color.FromArgb(28,28,28),
                ForeColor=Color.FromArgb(0,210,60), BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtWatermark);
            DLbl("Untertitel:", c2+278, yR+5, 62);
            txtWatermarkSub = new TextBox { Location=new Point(c2+344, yR), Size=new Size(cW2-344-4, 24),
                Text=cur.WatermarkSubText, BackColor=Color.FromArgb(28,28,28),
                ForeColor=Color.FromArgb(0,210,60), BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtWatermarkSub);
            yR += 30;

            DLbl("Eigene Begriffe (kommagetrennt):", c2, yR+5);
            yR += 22;
            txtExtra = new TextBox { Location=new Point(c2, yR), Size=new Size(cW2-4, 24),
                Text=cur.ExtraWords, BackColor=Color.FromArgb(28,28,28),
                ForeColor=Color.FromArgb(0,210,60), BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtExtra);
            yR += 30;

            // ── Bottom bar ────────────────────────────────────────────────────
            int yBot = Math.Max(yL, yR) + 14;
            HSep(yBot); yBot += 12;

            int bRight = c2 + cW2;   // right edge of content = 846
            var btnOK = new Button { Text="OK",
                Location=new Point(bRight-232, yBot), Size=new Size(108,32),
                DialogResult=DialogResult.OK,
                BackColor=Color.FromArgb(0,118,34), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(0,200,55);
            btnOK.Click += delegate
            {
                cur.Orientation     = cboOrient.Text;
                cur.WordOrientation = cboWordOrient.Text;
                cur.WordMode        = cboWordMode.Text;
                cur.WordStyle       = cboWordStyle.Text;
                cur.ShowScanlines   = chkScanlines.Checked;
                cur.ShowWatermark   = chkWatermark.Checked;
                cur.ShowVeeam100    = chkVeeam100.Checked;
                cur.WatermarkText   = txtWatermark.Text.Trim();
                cur.WatermarkSubText= txtWatermarkSub.Text.Trim();
                cur.ExtraWords      = txtExtra.Text.Trim();
                if (cboWordFontName.SelectedItem != null) cur.WordFontName = cboWordFontName.SelectedItem.ToString();
                var effects = new List<string>();
                if (chkFade.Checked)   effects.Add("Fade");
                if (chkFlash.Checked)  effects.Add("Flash");
                if (chkGlitch.Checked) effects.Add("Glitch");
                if (chkScan.Checked)   effects.Add("Scan");
                if (chkZoom.Checked)   effects.Add("Zoom");
                cur.PopupEffects = effects.Count>0 ? string.Join(",", effects.ToArray()) : "Fade";
                Result=cur; Result.Save();
            };
            var btnCancel = new Button { Text="Abbrechen",
                Location=new Point(bRight-118, yBot), Size=new Size(118,32),
                DialogResult=DialogResult.Cancel,
                BackColor=Color.FromArgb(50,15,15), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(130,36,36);
            Controls.Add(btnOK); Controls.Add(btnCancel);
            AcceptButton=btnOK; CancelButton=btnCancel;
            ClientSize = new Size(c2+cW2+14, yBot+48);
        }

        private void UpdateFontPreview()
        {
            if (picFontPreview==null || cboWordFontName==null || cboWordFontName.SelectedItem==null) return;
            string fname  = cboWordFontName.SelectedItem.ToString();
            string sample = (txtFontPreviewText!=null && txtFontPreviewText.Text.Trim().Length>0)
                            ? txtFontPreviewText.Text.Trim() : "VEEAM";
            int pw = picFontPreview.Width-2, ph = picFontPreview.Height-2;
            var bmp = new Bitmap(pw, ph);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(8,8,8));
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                try
                {
                    // fit font to available height
                    float fs = ph * 0.56f;
                    using (Font f = new Font(fname, fs, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        SizeF sz = g.MeasureString(sample, f);
                        // scale down if wider than panel
                        if (sz.Width > pw-8)
                        {
                            fs = fs * (pw-8) / sz.Width;
                            f.Dispose();
                            using (Font f2 = new Font(fname, fs, FontStyle.Bold, GraphicsUnit.Pixel))
                            using (SolidBrush b = new SolidBrush(Color.FromArgb(0,210,60)))
                            {
                                SizeF sz2 = g.MeasureString(sample, f2);
                                g.DrawString(sample, f2, b, (pw-sz2.Width)/2f, (ph-sz2.Height)/2f);
                                goto done;
                            }
                        }
                        using (SolidBrush b = new SolidBrush(Color.FromArgb(0,210,60)))
                            g.DrawString(sample, f, b, (pw-sz.Width)/2f, (ph-sz.Height)/2f);
                    }
                    done:;
                }
                catch { }
            }
            var old = picFontPreview.Image;
            picFontPreview.Image = bmp;
            if (old != null) old.Dispose();
        }

        private void LoadSelectedProfile()
        {
            if(cboProfiles.SelectedIndex<0) return;
            string name=cboProfiles.Text;
            foreach(var p in profiles)
            {
                if(p.Name!=name) continue;
                cur.RainColor=p.RainColor; cur.HeadColor=p.HeadColor;
                cur.WordColor=p.WordColor; cur.WordHeadColor=p.WordHeadColor;
                cur.PopupColor=p.PopupColor;
                RefreshColorButtons();
                break;
            }
        }

        private void SaveCurrentAsProfile()
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Profilname:", "Profil speichern", "Mein Profil");
            if(string.IsNullOrWhiteSpace(name)) return;
            var p=new ColorProfile{Name=name.Trim(),RainColor=cur.RainColor,HeadColor=cur.HeadColor,
                                    WordColor=cur.WordColor,WordHeadColor=cur.WordHeadColor,PopupColor=cur.PopupColor};
            ColorProfile.SaveUser(p);
            profiles=ColorProfile.LoadAll();
            cboProfiles.Items.Clear();
            foreach(var x in profiles) cboProfiles.Items.Add(x.Name);
            cboProfiles.Text=p.Name;
        }

        private void RefreshColorButtons()
        {
            SetBtn(btnRainColor,     cur.RainColor);
            SetBtn(btnHeadColor,     cur.HeadColor);
            SetBtn(btnWordColor,     cur.WordColor);
            SetBtn(btnWordHeadColor, cur.WordHeadColor);
            SetBtn(btnPopupColor,    cur.PopupColor);
        }

        private void Pick(ref Color field, Button btn)
        {
            using(ColorDialog dlg=new ColorDialog{Color=field,FullOpen=true})
                if(dlg.ShowDialog(this)==DialogResult.OK){field=dlg.Color;SetBtn(btn,dlg.Color);}
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
                // Primary form runs the message loop.
                // Secondary forms are created in the Load event so the loop is already running.
                Screen primary = Screen.PrimaryScreen;
                var mainForm   = new ScreenSaverForm(s, primary.Bounds, true);

                mainForm.Load += delegate
                {
                    foreach (Screen scr in Screen.AllScreens)
                    {
                        if (scr == primary) continue;
                        var sec = new ScreenSaverForm(s, scr.Bounds, false);
                        // Close secondary when primary closes
                        mainForm.FormClosed += delegate { try { sec.Close(); } catch { } };
                        sec.Show();
                    }
                };

                Application.Run(mainForm);
            }
        }
    }
}
