// VeeaMatrix.cs  –  Windows Screensaver v1.18
// Build: Build-VeeaMatrix.ps1  (outputs VeeaMatrix.scr)
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace VeeaMatrix
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
                RainColor     = Color.FromArgb(0,   179,  54),
                HeadColor     = Color.FromArgb(255, 255, 255),
                WordColor     = Color.FromArgb(0,   224,  78),
                WordHeadColor = Color.FromArgb(170, 255, 196),
                PopupColor    = Color.FromArgb(0,   179,  54),
            },
            new ColorProfile
            {
                Name          = "Hello Kitty",
                RainColor     = Color.FromArgb(255, 105, 180),
                HeadColor     = Color.FromArgb(255, 255, 255),
                WordColor     = Color.FromArgb(255,  20, 147),
                WordHeadColor = Color.FromArgb(255, 215,   0),
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
                RainColor     = Color.FromArgb(0,   240, 255),
                HeadColor     = Color.FromArgb(255, 255, 255),
                WordColor     = Color.FromArgb(255,  20, 147),
                WordHeadColor = Color.FromArgb(255, 255,   0),
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
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VeeaMatrix", "profiles.ini"); }
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
        // Popup words
        public string PopupEffects      = "Glitch";
        public int    PopupCount        = 5;
        public int    PopupFontSize     = 22;
        public Color  PopupColor        = Color.FromArgb(0, 255, 65);
        public float  PopupSpeedFactor  = 1.0f;
        // General
        public string Orientation     = "TopDown";
        public string WordOrientation  = "Same";
        public string WordStyle        = "Scroll";
        public float  WordSpeedFactor  = 1.0f;
        public bool   ShowVeeam100     = false;
        // Watermark
        public string WatermarkText    = "VEEAM";
        public string WatermarkSubText = "DATA PROTECTION  *  CYBER RESILIENCE  *  ALWAYS-ON";
        public string ExtraWords      = "";
        public string WordFontName    = "Segoe UI";
        // UI
        public string Language        = "EN";

        private static string ConfigDir
        { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VeeaMatrix"); } }
        private static string ConfigFile { get { return Path.Combine(ConfigDir, "config.ini"); } }

        // One-time migration from old VeeamMatrix folder
        private static void MigrateIfNeeded()
        {
            string newDir = ConfigDir;
            if (Directory.Exists(newDir)) return;
            string oldDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VeeamMatrix");
            if (!Directory.Exists(oldDir)) return;
            try
            {
                Directory.CreateDirectory(newDir);
                foreach (string f in Directory.GetFiles(oldDir))
                    File.Copy(f, Path.Combine(newDir, Path.GetFileName(f)), false);
            }
            catch { }
        }

        public void Save()
        {
            Directory.CreateDirectory(ConfigDir);
            var sb = new StringBuilder();
            sb.AppendLine("[VeeaMatrix]");
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
            sb.AppendLine("PopupEffects="      + PopupEffects);
            sb.AppendLine("PopupCount="        + PopupCount);
            sb.AppendLine("PopupFontSize="     + PopupFontSize);
            sb.AppendLine("PopupColor="        + ToHex(PopupColor));
            sb.AppendLine("PopupSpeedFactor="  + PopupSpeedFactor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendLine("Orientation="     + Orientation);
            sb.AppendLine("WordOrientation="  + WordOrientation);
            sb.AppendLine("WordStyle="        + WordStyle);
            sb.AppendLine("WordSpeedFactor="  + WordSpeedFactor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendLine("ShowVeeam100="     + ShowVeeam100);
            sb.AppendLine("WatermarkText="    + WatermarkText);
            sb.AppendLine("WatermarkSubText=" + WatermarkSubText);
            sb.AppendLine("ExtraWords="       + ExtraWords);
            sb.AppendLine("WordFontName="     + WordFontName);
            sb.AppendLine("Language="         + Language);
            File.WriteAllText(ConfigFile, sb.ToString(), Encoding.UTF8);
        }

        public static Settings Load()
        {
            MigrateIfNeeded();
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
                        case "PopupStyle":    s.PopupEffects  = v == "Mixed" ? "Glitch" : v; break;
                        case "PopupCount":    s.PopupCount    = int.Parse(v); break;
                        case "PopupFontSize": s.PopupFontSize = int.Parse(v); break;
                        case "PopupColor":        s.PopupColor       = FromHex(v); break;
                        case "PopupSpeedFactor":  s.PopupSpeedFactor = float.Parse(v, ic); break;
                        case "Orientation":     s.Orientation     = v; break;
                        case "WordOrientation":  s.WordOrientation  = v; break;
                        case "WordStyle":        s.WordStyle        = v; break;
                        case "WordSpeedFactor":  s.WordSpeedFactor  = float.Parse(v, ic); break;
                        case "ShowVeeam100":     s.ShowVeeam100     = bool.Parse(v); break;
                        case "WatermarkText":    s.WatermarkText    = v; break;
                        case "WatermarkSubText": s.WatermarkSubText = v; break;
                        case "ExtraWords":       s.ExtraWords       = v; break;
                        case "WordFontName":     s.WordFontName     = v; break;
                        case "Language":         s.Language         = v; break;
                    }
                }
                catch { }
            }
            // v1.16 migrations
            if (s.WordStyle == "Blink") s.WordStyle = "Scramble";
            if (s.PopupEffects.Contains(",") || s.PopupEffects == "Flash")
            {
                string migrated = "Glitch";
                foreach (string part in s.PopupEffects.Split(','))
                {
                    string t = part.Trim();
                    if (t == "Fade" || t == "Glitch" || t == "Scan" || t == "Zoom" || t == "Scramble") { migrated = t; break; }
                }
                s.PopupEffects = migrated;
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

        private static readonly string[] VEEAM100_PEOPLE = new string[]
        {
            "VEEAM VANGUARD 2026", "VEEAM LEGEND 2026", "VEEAM MVP 2026",
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
            public char[]   Chars;
            public float[]  CharOffsets;  // per-char x-offsets (length = Chars.Length+1); null for vertical
            public float    X, Y, V;
            public bool     Glow;
            public int      Phase, Frame, AppearF, HoldF, FadeF;
        }

        private enum PopupMode { Fade, Glitch, Scan, Zoom, Scramble }
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
        private Font        rainFont, wordFont, popupFont;
        private StringFormat typFmt;
        private float       wordLineH;   // actual line height of wordFont in pixels
        private SolidBrush  fadeBrush, rainBrush, brightBrush, tmpBrush;
        private Bitmap      scanBmp;

        private float[] lanePos, laneSpeed;
        private bool[]  laneBright;
        private int     laneCount;

        private readonly List<WDrop>  wdrops = new List<WDrop>();
        private readonly List<WPopup> popups = new List<WPopup>();

        private bool IsVertical { get { return s.Orientation == "TopDown"   || s.Orientation == "BottomUp"; } }
        private bool IsForward  { get { return s.Orientation == "TopDown"   || s.Orientation == "LeftRight"; } }

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
                case "fade":    m = PopupMode.Fade;    return true;
                case "glitch":  m = PopupMode.Glitch;  return true;
                case "scan":    m = PopupMode.Scan;    return true;
                case "zoom":    m = PopupMode.Zoom;    return true;
                case "scramble":m = PopupMode.Scramble;return true;
                default:        m = PopupMode.Glitch;  return false;
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

            rainFont    = new Font("Courier New", Math.Max(6, s.FontSize     - 1), FontStyle.Bold, GraphicsUnit.Pixel);
            wordFont    = new Font(s.WordFontName, Math.Max(6, s.WordFontSize - 1), FontStyle.Bold, GraphicsUnit.Pixel);
            popupFont   = new Font(s.WordFontName, Math.Max(6, s.PopupFontSize- 1), FontStyle.Bold, GraphicsUnit.Pixel);
            typFmt      = StringFormat.GenericTypographic;
            wordLineH   = wordFont.GetHeight(bg);
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

        // Per-char x-offsets using typographic metrics — fixes disproportionate "I" spacing
        private float[] ComputeCharOffsets(char[] chars)
        {
            var offs = new float[chars.Length + 1];
            offs[0] = 0f;
            for (int j = 0; j < chars.Length; j++)
            {
                string ch = chars[j] == ' ' ? "M" : chars[j].ToString();
                SizeF  sz = bg.MeasureString(ch, wordFont, PointF.Empty, typFmt);
                offs[j + 1] = offs[j] + (chars[j] == ' ' ? sz.Width * 0.55f : sz.Width) + 2f;
            }
            return offs;
        }

        private WDrop SpawnDrop(bool scatter)
        {
            string term  = allTerms[rng.Next(allTerms.Length)];
            char[] chars = term.ToCharArray();
            int    fs    = s.WordFontSize;

            if (s.WordStyle == "Fade" || s.WordStyle == "Build" || s.WordStyle == "Scramble" || s.WordStyle == "Glitch")
            {
                float  spd  = Math.Max(0.1f, s.WordSpeedFactor);
                int    appF = 22, holF = 90, fadF = 30;
                switch (s.WordStyle)
                {
                    case "Build":
                        appF = (int)Math.Round(Math.Max(20, chars.Length * 5) / spd);
                        holF = (int)Math.Round((70 + rng.Next(50)) / spd);
                        break;
                    case "Scramble":
                        appF = (int)Math.Round(Math.Max(15, chars.Length * 4) / spd);
                        holF = (int)Math.Round((60 + rng.Next(40)) / spd);
                        fadF = 25;
                        break;
                    case "Glitch":
                        appF = (int)Math.Round(Math.Max(25, chars.Length * 4) / spd);
                        holF = (int)Math.Round((60 + rng.Next(40)) / spd);
                        fadF = (int)Math.Round(Math.Max(25, chars.Length * 4) / spd);
                        break;
                    default: // Fade
                        appF = (int)Math.Round(22 / spd);
                        holF = (int)Math.Round((70 + rng.Next(50)) / spd);
                        break;
                }
                float[] offs = ComputeCharOffsets(chars);
                float   estW = offs[chars.Length];
                float   mgn  = fs * 2.5f;
                float   sx   = mgn + (float)(rng.NextDouble() * Math.Max(1f, W - estW - mgn));
                float   sy   = mgn + (float)(rng.NextDouble() * Math.Max(1f, H - fs * 3f - mgn));
                var     wd   = new WDrop { Chars=chars, X=sx, Y=sy, V=0,
                                           Glow=rng.NextDouble()<s.GlowChance,
                                           AppearF=appF, HoldF=holF, FadeF=fadF,
                                           CharOffsets=offs };
                if (scatter)
                {
                    int tot = appF + holF + fadF;
                    int rf  = rng.Next(tot);
                    if      (rf < appF)       { wd.Phase=0; wd.Frame=rf; }
                    else if (rf < appF+holF)  { wd.Phase=1; wd.Frame=rf-appF; }
                    else                       { wd.Phase=2; wd.Frame=rf-appF-holF; }
                }
                return wd;
            }

            // Scroll mode — use measured widths for horizontal, line height for vertical
            float[] charOff = WordIsVertical ? null : ComputeCharOffsets(chars);
            float   len     = WordIsVertical ? chars.Length * wordLineH : charOff[chars.Length];
            float   v       = (float)((0.6 + rng.NextDouble() * 1.6) * s.WordSpeedFactor);
            float   x, y;
            if (WordIsVertical)
            { x=fs*.5f+(float)(rng.NextDouble()*Math.Max(1,W-fs)); y=scatter?(float)(rng.NextDouble()*(H+len)-len):(WordIsForward?-(len+5):H+5); }
            else
            { y=fs*.5f+(float)(rng.NextDouble()*Math.Max(1,H-fs)); x=scatter?(float)(rng.NextDouble()*(W+len)-len):(WordIsForward?-(len+5):W+5); }
            return new WDrop { Chars=chars, X=x, Y=y, V=WordIsForward?v:-v,
                               Glow=rng.NextDouble()<s.GlowChance, CharOffsets=charOff };
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
                case PopupMode.Glitch:  appearF=55;               holdF=90; disappearF=35; break;
                case PopupMode.Scan:    appearF=word.Length*3;    holdF=80; disappearF=28; break;
                case PopupMode.Zoom:    appearF=18;               holdF=90; disappearF=28; break;
                case PopupMode.Scramble:appearF=word.Length*4;    holdF=80; disappearF=word.Length*4; break;
                default:                appearF=22;               holdF=90; disappearF=32; break;
            }
            // Apply popup speed factor (higher = faster = fewer frames)
            float pspd = Math.Max(0.1f, s.PopupSpeedFactor);
            if (pspd != 1.0f)
            {
                appearF    = Math.Max(1, (int)Math.Round(appearF    / pspd));
                holdF      = Math.Max(1, (int)Math.Round(holdF      / pspd));
                disappearF = Math.Max(1, (int)Math.Round(disappearF / pspd));
            }

            float estW = word.Length * fs * 0.64f, margin = fs * 2f;
            float cx   = margin + estW/2f + (float)(rng.NextDouble() * Math.Max(1, W - estW - 2f*margin));
            float cy   = margin + (float)(rng.NextDouble() * Math.Max(1, H - 2f*margin));
            if (scatter) cy = margin + (float)(rng.NextDouble() * Math.Max(1, H - 2f*margin));

            char[] disp = (char[])word.Clone();
            if (mode == PopupMode.Glitch || mode == PopupMode.Scan || mode == PopupMode.Scramble)
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
            bool isStatic = (s.WordStyle == "Fade" || s.WordStyle == "Build" ||
                             s.WordStyle == "Scramble" || s.WordStyle == "Glitch");
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
                    int n       = w.Chars.Length;
                    int headIdx = WordIsForward ? 0 : n-1;  // head = leading char in movement direction
                    for (int j = 0; j < n; j++)
                    {
                        float px = WordIsVertical ? w.X
                                                  : w.X + (w.CharOffsets != null ? w.CharOffsets[j] : j*(float)fs);
                        float py = WordIsVertical ? w.Y + j * wordLineH : w.Y;
                        if (px<-fs||px>W+fs||py<-fs||py>H+fs) continue;
                        float fade = n>1 ? (float)Math.Abs(j - headIdx)/(n-1) : 0f;
                        int   a    = Clamp((int)(255*(1f-fade*0.55f)));
                        Color col;
                        if (j == headIdx) col = s.WordHeadColor;
                        else if (w.Glow)  col = Color.FromArgb(a,Clamp(s.WordColor.R+80),Clamp(s.WordColor.G+20),Clamp(s.WordColor.B+40));
                        else              col = Color.FromArgb(a,Clamp((int)(s.WordColor.R*(1-fade*.5f))),Clamp((int)(s.WordColor.G*(1-fade*.3f)+30*(1-fade))),Clamp((int)(s.WordColor.B*(1-fade*.5f))));
                        tmpBrush.Color = col;
                        bg.DrawString(w.Chars[j].ToString(), wordFont, tmpBrush, new PointF(px, py), typFmt);
                    }
                    if (WordIsVertical) w.Y+=w.V; else w.X+=w.V;
                    float totalLen = (w.CharOffsets != null) ? w.CharOffsets[n] : n*(float)fs;
                    bool gone = WordIsVertical
                        ? (WordIsForward ? w.Y > H+5 : w.Y + n*wordLineH < -5)
                        : (WordIsForward ? w.X > W+5 : w.X + totalLen < -5);
                    if (gone) { wdrops.RemoveAt(i); wdrops.Add(SpawnDrop(false)); }
                }
            }
        }

        private bool TickStaticDrop(WDrop w)
        {
            w.Frame++;
            if (w.Phase==0 && w.Frame>=w.AppearF) { w.Phase=1; w.Frame=0; }
            else if (w.Phase==1 && w.Frame>=w.HoldF)  { w.Phase=2; w.Frame=0; }
            else if (w.Phase==2 && w.Frame>=w.FadeF)   return false;

            int fs = s.WordFontSize;

            float prog  = w.Phase==0 ? (float)w.Frame/Math.Max(1,w.AppearF)
                        : w.Phase==2 ? 1f-(float)w.Frame/Math.Max(1,w.FadeF)
                        : 1f;
            int   alpha2 = Clamp((int)(prog*255));
            if (alpha2 < 3) return w.Phase < 2;

            // ── Scramble: all chars as noise, resolve in direction order ──────
            if (s.WordStyle == "Scramble")
            {
                int  total         = w.Chars.Length;
                bool rev           = !WordIsForward;
                int  resolvedCount = w.Phase == 0
                    ? Math.Min(total, w.Frame * total / Math.Max(1, w.AppearF) + 1)
                    : total;
                for (int j = 0; j < total; j++)
                {
                    // Forward: j < resolvedCount → resolved; reverse: j >= total-resolvedCount → resolved
                    bool resolved = rev ? (j >= total - resolvedCount) : (j < resolvedCount);
                    bool isCursor = rev ? (j == total - resolvedCount) : (j == resolvedCount - 1);
                    char drawCh   = resolved ? w.Chars[j] : RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];
                    if (drawCh == ' ') continue;
                    Color col;
                    if (isCursor && w.Phase == 0)
                        col = Color.FromArgb(alpha2, s.WordHeadColor.R, s.WordHeadColor.G, s.WordHeadColor.B);
                    else if (w.Glow)
                        col = Color.FromArgb(alpha2, Clamp(s.WordColor.R+80), Clamp(s.WordColor.G+20), Clamp(s.WordColor.B+40));
                    else
                        col = Color.FromArgb(alpha2, s.WordColor.R, s.WordColor.G, s.WordColor.B);
                    tmpBrush.Color = col;
                    float xOff = w.CharOffsets != null ? w.CharOffsets[j] : j * fs * 0.61f;
                    bg.DrawString(drawCh.ToString(), wordFont, tmpBrush, new PointF(w.X + xOff, w.Y), typFmt);
                }
                return true;
            }

            // ── Build: chars decode one-by-one in direction order ─────────────
            if (s.WordStyle == "Build")
            {
                int  total   = w.Chars.Length;
                bool rev     = !WordIsForward;
                int  snapped = w.Phase==0
                    ? Math.Min(total, w.Frame * total / Math.Max(1, w.AppearF) + 1)
                    : total;
                // Forward: reveal 0..snapped-1, cursor at snapped-1
                // Reverse: reveal (total-snapped)..total-1, cursor at total-snapped
                int startJ  = rev ? total - snapped : 0;
                int endJ    = rev ? total            : snapped;
                int cursorJ = rev ? startJ           : endJ - 1;
                for (int j = startJ; j < endJ; j++)
                {
                    char drawCh = (j == cursorJ && w.Phase==0)
                        ? ((w.Frame % 4 < 2) ? RAIN_CHARS[rng.Next(RAIN_CHARS.Length)] : w.Chars[j])
                        : w.Chars[j];
                    if (drawCh == ' ') continue;
                    Color col;
                    if (j == cursorJ && w.Phase==0)
                        col = Color.FromArgb(alpha2, s.WordHeadColor.R, s.WordHeadColor.G, s.WordHeadColor.B);
                    else if (w.Glow)
                        col = Color.FromArgb(alpha2, Clamp(s.WordColor.R+80), Clamp(s.WordColor.G+20), Clamp(s.WordColor.B+40));
                    else
                        col = Color.FromArgb(alpha2, s.WordColor.R, s.WordColor.G, s.WordColor.B);
                    tmpBrush.Color = col;
                    float xOff = w.CharOffsets != null ? w.CharOffsets[j] : j * fs * 0.61f;
                    bg.DrawString(drawCh.ToString(), wordFont, tmpBrush, new PointF(w.X + xOff, w.Y), typFmt);
                }
            }
            // ── Glitch: all chars visible with noise overlay that resolves over time ──
            else if (s.WordStyle == "Glitch")
            {
                float glitchChance;
                if      (w.Phase == 0) glitchChance = 1f - (float)w.Frame / Math.Max(1, w.AppearF);  // noisy → resolve
                else if (w.Phase == 1) glitchChance = 0.04f;                                           // mostly correct, rare noise
                else                   glitchChance = (float)w.Frame / Math.Max(1, w.FadeF);           // resolve → noisy again
                for (int j = 0; j < w.Chars.Length; j++)
                {
                    if (w.Chars[j] == ' ') continue;
                    char drawCh = (rng.NextDouble() < glitchChance)
                        ? RAIN_CHARS[rng.Next(RAIN_CHARS.Length)]
                        : w.Chars[j];
                    Color col = w.Glow
                        ? Color.FromArgb(alpha2, Clamp(s.WordColor.R+80), Clamp(s.WordColor.G+20), Clamp(s.WordColor.B+40))
                        : Color.FromArgb(alpha2, s.WordColor.R, s.WordColor.G, s.WordColor.B);
                    tmpBrush.Color = col;
                    float xOff = w.CharOffsets != null ? w.CharOffsets[j] : j * fs * 0.61f;
                    bg.DrawString(drawCh.ToString(), wordFont, tmpBrush, new PointF(w.X + xOff, w.Y), typFmt);
                }
            }
            else // Fade — draw full string at once
            {
                Color col = w.Glow
                    ? Color.FromArgb(alpha2, Clamp(s.WordColor.R+80), Clamp(s.WordColor.G+20), Clamp(s.WordColor.B+40))
                    : Color.FromArgb(alpha2, s.WordColor.R, s.WordColor.G, s.WordColor.B);
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
                    case PopupMode.Fade:    PaintPopup(p.Word,p.CX,p.CY,p.FontSize,prog,p.Glow); break;
                    case PopupMode.Glitch:  DoGlitch(p,prog);   break;
                    case PopupMode.Scan:    DoScan(p,prog);     break;
                    case PopupMode.Zoom:    DoZoom(p,prog);     break;
                    case PopupMode.Scramble:DoScramblePopup(p,prog); break;
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
            PaintPopup(p.Disp,p.CX,p.CY,p.FontSize,p.Phase==0?prog*0.8f+0.2f:prog,p.Glow);
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
                PaintPopup(p.Disp,p.CX,p.CY,p.FontSize,1f,p.Glow);
            }
            else if(p.Phase==1)
            { for(int j=0;j<len;j++) p.Disp[j]=p.Word[j]; PaintPopup(p.Disp,p.CX,p.CY,p.FontSize,1f,p.Glow); }
            else
            {
                int hide=Math.Min(len,(int)((1f-prog)*len));
                for(int j=0;j<len;j++) p.Disp[j]=j>=hide?' ':p.Word[j];
                PaintPopup(p.Disp,p.CX,p.CY,p.FontSize,prog,p.Glow);
            }
        }

        private void DoZoom(WPopup p, float prog)
        {
            float scale=p.Phase==0?2f-prog:1f;
            float alpha=p.Phase==0?prog*prog:prog;
            PaintPopup(p.Word,p.CX,p.CY,(int)(p.FontSize*scale),alpha,p.Glow);
        }

        // Scramble popup: chars resolve L→R (appear) / R→L (disappear), rest show as noise
        private void DoScramblePopup(WPopup p, float prog)
        {
            int len = p.Word.Length;
            if (p.Phase == 0)
            {
                int rev = (int)(prog * len);
                for (int j = 0; j < len; j++)
                {
                    if (p.Word[j] == ' ') { p.Disp[j] = ' '; continue; }
                    p.Disp[j] = (j < rev) ? p.Word[j] : RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];
                }
                PaintPopup(p.Disp, p.CX, p.CY, p.FontSize, 0.3f + prog * 0.7f, p.Glow);
            }
            else if (p.Phase == 1)
            {
                for (int j = 0; j < len; j++) p.Disp[j] = p.Word[j];
                PaintPopup(p.Disp, p.CX, p.CY, p.FontSize, 1f, p.Glow);
            }
            else // phase 2: prog goes 1→0 — de-resolve R→L while fading
            {
                int rev = (int)(prog * len);   // len→0 as prog 1→0
                for (int j = 0; j < len; j++)
                {
                    if (p.Word[j] == ' ') { p.Disp[j] = ' '; continue; }
                    p.Disp[j] = (j < rev) ? p.Word[j] : RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];
                }
                PaintPopup(p.Disp, p.CX, p.CY, p.FontSize, prog, p.Glow);
            }
        }

        private void PaintPopup(char[] chars,float cx,float cy,int fontSize,float alpha,bool glow)
        {
            if(alpha<=0.01f) return;
            int a=Clamp((int)(alpha*255));
            Font useFont=null; bool own=false;
            if(fontSize==s.PopupFontSize-1||fontSize==s.PopupFontSize) useFont=popupFont;
            else { useFont=new Font(s.WordFontName,Math.Max(6,fontSize),FontStyle.Bold,GraphicsUnit.Pixel); own=true; }
            try
            {
                string text=new string(chars);
                SizeF  sz=bg.MeasureString(text,useFont);
                Color  col;
                if(glow) col=Color.FromArgb(a,Clamp(s.PopupColor.R+90),Clamp(s.PopupColor.G+20),Clamp(s.PopupColor.B+60));
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
            if(popupFont  !=null){popupFont.Dispose();  popupFont  =null;}
            if(typFmt     !=null){typFmt.Dispose();     typFmt     =null;}
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
        [DllImport("user32.dll")] private static extern bool DestroyIcon(IntPtr hIcon);

        private Settings   cur;
        private Button     btnRainColor, btnHeadColor, btnWordColor, btnWordHeadColor, btnPopupColor;
        private TrackBar   trkFade, trkFont, trkSpeed, trkWordCount, trkWordFont, trkPopupCount, trkPopupFont, trkWordSpeed, trkPopupSpeed;
        private Label      lblFade, lblFont, lblSpeed, lblWCount, lblWFont, lblPCount, lblPFont, lblWordSpeed, lblPopupSpeed;
        private ComboBox   cboOrient, cboWordOrient, cboWordMode;
        private Button[]   btnWordStyles;  // single-select word style buttons [Scroll, Fade, Build, Scramble, Glitch]
        private Label      _lblWordOrient; // reference for enable/disable alongside cboWordOrient
        private TextBox    txtWatermark, txtWatermarkSub, txtExtra;
        private Button[]   btnFxEffects;   // single-select popup effect buttons [Fade, Glitch, Scan, Zoom]
        private CheckBox   chkScanlines, chkWatermark, chkVeeam100;
        private ComboBox   cboProfiles;
        private List<ColorProfile> profiles;
        private ComboBox   cboWordFontName;
        private ComboBox   cboLanguage;
        private PictureBox picFontPreview;
        private TextBox    txtFontPreviewText;
        // live preview
        private MatrixEngine _prevEngine;
        private Timer        _prevTimer;
        private PictureBox   picPreview;
        private bool         _previewDirty = true;
        private ToolTip      _tip;
        // Controls grouped by word-mode layer — toggled by SyncWordModeVisibility()
        private List<Control> _streamControls = new List<Control>();
        private List<Control> _popupControls  = new List<Control>();
        private Label         lblPopupHeader;

        public Settings Result { get; private set; }

        // ── Language helpers ─────────────────────────────────────────────────
        private bool   IsDE { get { return cur != null && cur.Language == "DE"; } }
        private string T(string en, string de) { return IsDE ? de : en; }

        public ConfigForm(Settings s)
        {
            cur = Clone(s);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            ShowInTaskbar = true;
            ClientSize  = new Size(860, 100);   // placeholder; Build() sets final size
            BackColor   = Color.FromArgb(18, 18, 18);
            ForeColor   = Color.FromArgb(0, 200, 55);
            Font        = new Font("Segoe UI", 9f);
            Icon        = CreateAppIcon();
            profiles    = ColorProfile.LoadAll();
            // Wire FormClosing once (not inside Build, so it survives RebuildUI)
            FormClosing += OnFormClosingHandler;
            Build();
        }

        // Programmatic 32×32 app icon: dark background with Veeam-green "V"
        private Icon CreateAppIcon()
        {
            using (var bmp = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.Clear(Color.FromArgb(12, 12, 12));
                    // Outer glow ring
                    using (SolidBrush glow = new SolidBrush(Color.FromArgb(30, 0, 179, 54)))
                        g.FillEllipse(glow, 1, 1, 30, 30);
                    // Bold "V" centered
                    using (Font f = new Font("Arial", 21, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(0, 200, 60)))
                    {
                        SizeF sz = g.MeasureString("V", f);
                        g.DrawString("V", f, b, (32f - sz.Width) / 2f, (32f - sz.Height) / 2f + 1f);
                    }
                }
                IntPtr hIco = bmp.GetHicon();
                try   { return (Icon)Icon.FromHandle(hIco).Clone(); }
                finally { DestroyIcon(hIco); }
            }
        }

        // Find banner sidecar image: look next to the .scr and in %APPDATA%\VeeaMatrix
        private static string FindBannerPath()
        {
            string[] names = new string[]{ "VeeaMatrix-banner.jpg", "VeeaMatrix-banner.png",
                                           "VeeaMatrix-banner.jpeg", "banner.jpg" };
            string exeDir = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
            string appDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VeeaMatrix");
            foreach (string dir in new string[]{ exeDir, appDir })
                foreach (string n in names)
                { string p = Path.Combine(dir, n); if (File.Exists(p)) return p; }
            return null;
        }

        private void OnFormClosingHandler(object sender, FormClosingEventArgs e)
        {
            if (_prevTimer  != null) { _prevTimer.Stop(); _prevTimer.Dispose(); _prevTimer = null; }
            if (_prevEngine != null) { _prevEngine.Dispose(); _prevEngine = null; }
            if (_tip        != null) { _tip.Dispose(); _tip = null; }
        }

        // Full UI rebuild (called on language toggle)
        private void RebuildUI()
        {
            if (_prevTimer  != null) { _prevTimer.Stop(); _prevTimer.Dispose(); _prevTimer = null; }
            if (_prevEngine != null) { _prevEngine.Dispose(); _prevEngine = null; }
            Controls.Clear();
            // Reset all control references so Build() re-creates them cleanly
            btnRainColor = btnHeadColor = btnWordColor = btnWordHeadColor = btnPopupColor = null;
            trkFade = trkFont = trkSpeed = trkWordCount = trkWordFont = trkPopupCount = trkPopupFont = trkWordSpeed = trkPopupSpeed = null;
            lblFade = lblFont = lblSpeed = lblWCount = lblWFont = lblPCount = lblPFont = lblWordSpeed = lblPopupSpeed = null;
            cboOrient = cboWordOrient = cboWordMode = cboLanguage = null;
            txtWatermark = txtWatermarkSub = txtExtra = null;
            btnFxEffects = null; btnWordStyles = null; _lblWordOrient = null;
            chkScanlines = chkWatermark = chkVeeam100 = null;
            cboProfiles = cboWordFontName = null;
            picFontPreview = null; txtFontPreviewText = null; picPreview = null;
            _previewDirty = true;
            if (_tip != null) { _tip.Dispose(); _tip = null; }
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
                WordFontName=s.WordFontName, Language=s.Language,
                PopupSpeedFactor=s.PopupSpeedFactor };
        }

        // ── layout helpers ────────────────────────────────────────────────────
        private static int Clamp(int v, int lo, int hi) { return v<lo?lo:v>hi?hi:v; }
        private static void SetBtn(Button b, Color c)
        { b.BackColor=c; b.ForeColor=c.GetBrightness()>.45f?Color.Black:Color.White; }

        private void Section(string title, int x, int y, int w)
        {
            var pnl = new Panel { Location=new Point(x,y), Size=new Size(w,20),
                BackColor=Color.FromArgb(0,50,16) };
            Controls.Add(pnl);
            pnl.Controls.Add(new Label { Text=title, Location=new Point(8,2), AutoSize=true,
                ForeColor=Color.FromArgb(0,220,65),
                Font=new Font("Segoe UI",8.5f,FontStyle.Bold) });
        }

        private void HSep(int y, int x=14, int w=832)
        { Controls.Add(new Panel { Location=new Point(x,y), Size=new Size(w,1), BackColor=Color.FromArgb(0,52,16) }); }

        private Label DLbl(string t, int x, int y, int w=-1)
        {
            var l = new Label { Text=t, Location=new Point(x,y), AutoSize=(w<0),
                ForeColor=Color.FromArgb(155,155,155) };
            if (w>0) { l.AutoSize=false; l.Size=new Size(w,15); }
            Controls.Add(l); return l;
        }

        private Button ColBtn(string text, Color col, int x, int y, int w=130)
        {
            var b = new Button { Text=text, Location=new Point(x,y), Size=new Size(w,26),
                BackColor=col, ForeColor=col.GetBrightness()>.45f?Color.Black:Color.White,
                FlatStyle=FlatStyle.Flat };
            b.FlatAppearance.BorderColor = Color.FromArgb(0,85,24);
            Controls.Add(b); return b;
        }

        private ComboBox Cbo(int x, int y, int w, string[] items, string sel)
        {
            var c = new ComboBox { Location=new Point(x,y), Size=new Size(w,24),
                DropDownStyle=ComboBoxStyle.DropDownList,
                BackColor=Color.FromArgb(28,28,28), ForeColor=Color.FromArgb(0,210,60) };
            c.Items.AddRange(items); c.Text=sel;
            Controls.Add(c); return c;
        }

        private CheckBox Chk(string text, bool val, int x, int y)
        {
            var c = new CheckBox { Text=text, Checked=val, Location=new Point(x,y),
                AutoSize=true, ForeColor=Color.FromArgb(165,165,165) };
            Controls.Add(c); return c;
        }

        private TrackBar SlRow(string name, int x, int y, int cw, int min, int max, int val, out Label vLbl)
        {
            const int LW=132, VW=54, G=6;
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
            _streamControls = new List<Control>();
            _popupControls  = new List<Control>();
            // ── Layout constants ─────────────────────────────────────────────
            const int c1   = 14,  cW1  = 400;   // left column
            const int c2   = 428, cW2  = 418;   // right column
            const int PREV_W = 960, PREV_H = 540; // 16:9 live preview (double size)
            const int c3   = c2 + cW2 + 14;     // = 860  preview column x
            const int cW3  = PREV_W + 2;         // = 482  preview column width (incl. 1px border each side)
            const int fw   = c3 + cW3 + 14;      // = 1356 total form width
            const int SL   = 46;                 // slider row step
            const int CM   = 32;                 // combo row step

            int y = 14;

            // ── Full-width profile strip + language toggle ───────────────────
            DLbl(T("Color Profile:", "Farbprofil:"), c1, y+5, 94);
            var profileNames = new List<string>();
            foreach (var p in profiles) profileNames.Add(p.Name);
            cboProfiles = Cbo(c1+98, y, 180, profileNames.ToArray(), "");
            var btnLoad = new Button { Text=T("Load","Laden"), Location=new Point(c1+286,y), Size=new Size(72,24),
                BackColor=Color.FromArgb(0,76,22), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            btnLoad.FlatAppearance.BorderColor = Color.FromArgb(0,110,32);
            var btnSave = new Button { Text=T("Save as…","Speichern als…"), Location=new Point(c1+366,y), Size=new Size(128,24),
                BackColor=Color.FromArgb(34,34,0), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            btnSave.FlatAppearance.BorderColor = Color.FromArgb(90,90,0);
            btnLoad.Click += delegate { LoadSelectedProfile(); };
            btnSave.Click += delegate { SaveCurrentAsProfile(); };
            Controls.Add(btnLoad); Controls.Add(btnSave);

            // Language selector – right-aligned in profile strip
            DLbl(T("Language:", "Sprache:"), fw-148, y+5, 72);
            cboLanguage = Cbo(fw-72, y, 60, new string[]{"EN","DE"}, cur.Language);
            // Wire language change BEFORE the generic wiring loop (so we can exclude it there)
            cboLanguage.SelectedIndexChanged += delegate { cur.Language = cboLanguage.Text; RebuildUI(); };

            y += 34; HSep(y, 14, fw-28); y += 12;

            int yL = y, yR = y;   // independent column y cursors

            // ═══════════════════════════════════════════════════════════════════
            // LEFT COLUMN — RAIN
            // ═══════════════════════════════════════════════════════════════════
            Section(T("RAIN", "REGEN"), c1, yL, cW1); yL += 26;
            DLbl(T("Characters:", "Zeichen:"), c1, yL+5); yL += 20;
            btnRainColor = ColBtn(T("Characters","Zeichen"),  cur.RainColor, c1,     yL);
            btnHeadColor = ColBtn(T("Head (bright)","Kopf (hell)"), cur.HeadColor, c1+136, yL);
            btnRainColor.Click += delegate { Pick(ref cur.RainColor, btnRainColor); };
            btnHeadColor.Click += delegate { Pick(ref cur.HeadColor, btnHeadColor); };
            yL += 32;

            trkFont = SlRow(T("Font Size","Schriftgröße"), c1,yL,cW1, 8,36, cur.FontSize, out lblFont);
            lblFont.Text = cur.FontSize+" px";
            trkFont.ValueChanged += delegate { cur.FontSize=trkFont.Value; lblFont.Text=cur.FontSize+" px"; };
            yL += SL;

            trkSpeed = SlRow(T("Speed","Geschwindigkeit"), c1,yL,cW1, 1,30, (int)(cur.SpeedFactor*10), out lblSpeed);
            lblSpeed.Text = cur.SpeedFactor.ToString("F1")+"x";
            trkSpeed.ValueChanged += delegate { cur.SpeedFactor=trkSpeed.Value/10f; lblSpeed.Text=cur.SpeedFactor.ToString("F1")+"x"; };
            yL += SL;

            trkFade = SlRow(T("Trail Length","Spurlänge"), c1,yL,cW1, 2,60, cur.FadeAlpha, out lblFade);
            lblFade.Text = cur.FadeAlpha.ToString();
            trkFade.ValueChanged += delegate { cur.FadeAlpha=trkFade.Value; lblFade.Text=cur.FadeAlpha.ToString(); };
            yL += SL;

            DLbl(T("Direction:","Richtung:"), c1, yL+5, 76);
            cboOrient   = Cbo(c1+80,  yL, 138, new string[]{"TopDown","BottomUp","LeftRight","RightLeft"}, cur.Orientation);
            DLbl(T("Word Mode:","Wortmodus:"), c1+226, yL+5, 74);
            cboWordMode = Cbo(c1+304, yL, 102, new string[]{"Rain","Popup","Both"}, cur.WordMode);
            cboOrient.SelectedIndexChanged   += delegate { cur.Orientation = cboOrient.Text; };
            cboWordMode.SelectedIndexChanged += delegate { cur.WordMode = cboWordMode.Text; SyncWordModeVisibility(); SyncWordStyleDirection(); };
            yL += CM;
            yL += 10;

            // ═══════════════════════════════════════════════════════════════════
            // RIGHT COLUMN — WORDS  (includes popup section below)
            // ═══════════════════════════════════════════════════════════════════
            Section(T("WORD STREAMS  (Rain / Both)", "WORT-STREAMS  (Regen / Beides)"), c2, yR, cW2); yR += 26;
            _streamControls.Add(DLbl(T("Colors:", "Farben:"), c2, yR+5)); yR += 20;
            btnWordColor     = ColBtn(T("Words","Wörter"),      cur.WordColor,     c2,     yR, 130);
            btnWordHeadColor = ColBtn(T("Head (bright)","Kopf (hell)"), cur.WordHeadColor, c2+136, yR, 130);
            btnWordColor.Click     += delegate { Pick(ref cur.WordColor,     btnWordColor); };
            btnWordHeadColor.Click += delegate { Pick(ref cur.WordHeadColor, btnWordHeadColor); };
            _streamControls.Add(btnWordColor); _streamControls.Add(btnWordHeadColor);
            yR += 32;

            // Font picker row
            DLbl(T("Font:", "Schriftart:"), c2, yR+5, 80);
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

            txtFontPreviewText = new TextBox { Location=new Point(c2+292, yR), Size=new Size(cW2-292-4, 24),
                Text="VEEAM", BackColor=Color.FromArgb(28,28,28),
                ForeColor=Color.FromArgb(0,210,60), BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtFontPreviewText);
            yR += 30;

            picFontPreview = new PictureBox { Location=new Point(c2, yR), Size=new Size(cW2-4, 44),
                BackColor=Color.Black, BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(picFontPreview);
            UpdateFontPreview();
            cboWordFontName.SelectedIndexChanged += delegate { UpdateFontPreview(); };
            txtFontPreviewText.TextChanged       += delegate { UpdateFontPreview(); };
            yR += 50;

            // ── Word Style single-select buttons ──────────────────────────────
            _streamControls.Add(DLbl(T("Style:","Stil:"), c2, yR+5));
            yR += 22;
            string[] wsNames = new string[]{ "Scroll", "Fade", "Build", "Scramble", "Glitch" };
            btnWordStyles = new Button[wsNames.Length];
            const int WS_W = 79, WS_GAP = 4;
            for (int wi = 0; wi < wsNames.Length; wi++)
            {
                string capturedWS = wsNames[wi];
                var wsBtn = new Button {
                    Text      = capturedWS,
                    Location  = new Point(c2 + wi * (WS_W + WS_GAP), yR),
                    Size      = new Size(WS_W, 26),
                    FlatStyle = FlatStyle.Flat,
                    Tag       = capturedWS
                };
                wsBtn.FlatAppearance.BorderSize = 1;
                wsBtn.Click += delegate { SetWordStyle(capturedWS); MarkDirty(); };
                Controls.Add(wsBtn);
                btnWordStyles[wi] = wsBtn;
                _streamControls.Add(wsBtn);
            }
            SetWordStyle(string.IsNullOrEmpty(cur.WordStyle) ? "Scroll" : cur.WordStyle);
            yR += 32;

            // ── Word Direction ────────────────────────────────────────────────
            _lblWordOrient = DLbl(T("Direction:","Richtung:"), c2, yR+5, 68);
            _streamControls.Add(_lblWordOrient);
            cboWordOrient = Cbo(c2+72, yR, 200, new string[]{"Same","TopDown","BottomUp","LeftRight","RightLeft"},
                string.IsNullOrEmpty(cur.WordOrientation)?"Same":cur.WordOrientation);
            _streamControls.Add(cboWordOrient);
            cboWordOrient.SelectedIndexChanged += delegate { cur.WordOrientation = cboWordOrient.Text; };
            yR += CM;

            trkWordFont = SlRow(T("Font Size","Schriftgröße"), c2,yR,cW2, 8,36, cur.WordFontSize, out lblWFont);
            lblWFont.Text = cur.WordFontSize+" px";
            trkWordFont.ValueChanged += delegate { cur.WordFontSize=trkWordFont.Value; lblWFont.Text=cur.WordFontSize+" px"; };
            _streamControls.Add(trkWordFont); _streamControls.Add(lblWFont);
            yR += SL;

            trkWordSpeed = SlRow(T("Speed","Geschwindigkeit"), c2,yR,cW2, 1,30, (int)(cur.WordSpeedFactor*10), out lblWordSpeed);
            lblWordSpeed.Text = cur.WordSpeedFactor.ToString("F1")+"x";
            trkWordSpeed.ValueChanged += delegate { cur.WordSpeedFactor=trkWordSpeed.Value/10f; lblWordSpeed.Text=cur.WordSpeedFactor.ToString("F1")+"x"; };
            _streamControls.Add(trkWordSpeed); _streamControls.Add(lblWordSpeed);
            yR += SL;

            trkWordCount = SlRow(T("Simultaneous","Gleichzeitig"), c2,yR,cW2, 1,30, cur.WordCount, out lblWCount);
            lblWCount.Text = cur.WordCount.ToString();
            trkWordCount.ValueChanged += delegate { cur.WordCount=trkWordCount.Value; lblWCount.Text=cur.WordCount.ToString(); };
            _streamControls.Add(trkWordCount); _streamControls.Add(lblWCount);
            yR += SL;

            // ── Popup sub-section (within WÖRTER) ────────────────────────────
            yR += 6;
            Controls.Add(new Panel { Location=new Point(c2, yR), Size=new Size(cW2, 1), BackColor=Color.FromArgb(0,60,20) });
            yR += 6;
            lblPopupHeader = new Label { Text=T("  POPUP WORDS  (Popup / Both)", "  POPUP-WÖRTER  (Popup / Beides)"),
                Location=new Point(c2, yR), Size=new Size(cW2, 16), AutoSize=false,
                ForeColor=Color.FromArgb(0,170,50), Font=new Font("Segoe UI",8f,FontStyle.Bold) };
            Controls.Add(lblPopupHeader);
            _popupControls.Add(lblPopupHeader);
            yR += 20;

            _popupControls.Add(DLbl(T("Color:","Farbe:"), c2, yR+5, 50));
            btnPopupColor = ColBtn(T("Popup Color","Popup-Farbe"), cur.PopupColor, c2+54, yR, 148);
            btnPopupColor.Click += delegate { Pick(ref cur.PopupColor, btnPopupColor); };
            _popupControls.Add(btnPopupColor);
            yR += 32;

            // Single-select effect buttons (Fade / Glitch / Scan / Zoom / Scramble)
            _popupControls.Add(DLbl(T("Effect:","Effekt:"), c2, yR+5));
            yR += 22;
            string[] fxNames = new string[]{ "Fade", "Glitch", "Scan", "Zoom", "Scramble" };
            btnFxEffects = new Button[fxNames.Length];
            const int FX_W = 80, FX_GAP = 4;
            for (int fi = 0; fi < fxNames.Length; fi++)
            {
                string capturedName = fxNames[fi];
                var fxBtn = new Button {
                    Text      = capturedName,
                    Location  = new Point(c2 + fi * (FX_W + FX_GAP), yR),
                    Size      = new Size(FX_W, 26),
                    FlatStyle = FlatStyle.Flat,
                    Tag       = capturedName
                };
                fxBtn.FlatAppearance.BorderSize = 1;
                fxBtn.Click += delegate { SetPopupEffect(capturedName); MarkDirty(); };
                Controls.Add(fxBtn);
                btnFxEffects[fi] = fxBtn;
                _popupControls.Add(fxBtn);
            }
            SetPopupEffect(cur.PopupEffects);   // highlight the active button
            yR += 32;

            trkPopupFont = SlRow(T("Font Size","Schriftgröße"), c2,yR,cW2, 10,72, cur.PopupFontSize, out lblPFont);
            lblPFont.Text = cur.PopupFontSize+" px";
            trkPopupFont.ValueChanged += delegate { cur.PopupFontSize=trkPopupFont.Value; lblPFont.Text=cur.PopupFontSize+" px"; };
            _popupControls.Add(trkPopupFont); _popupControls.Add(lblPFont);
            yR += SL;

            trkPopupCount = SlRow(T("Simultaneous","Gleichzeitig"), c2,yR,cW2, 1,20, cur.PopupCount, out lblPCount);
            lblPCount.Text = cur.PopupCount.ToString();
            trkPopupCount.ValueChanged += delegate { cur.PopupCount=trkPopupCount.Value; lblPCount.Text=cur.PopupCount.ToString(); };
            _popupControls.Add(trkPopupCount); _popupControls.Add(lblPCount);
            yR += SL;

            trkPopupSpeed = SlRow(T("Popup Speed","Popup-Geschwindigkeit"), c2,yR,cW2, 1,30, (int)(cur.PopupSpeedFactor*10), out lblPopupSpeed);
            lblPopupSpeed.Text = cur.PopupSpeedFactor.ToString("F1")+"x";
            trkPopupSpeed.ValueChanged += delegate { cur.PopupSpeedFactor=trkPopupSpeed.Value/10f; lblPopupSpeed.Text=cur.PopupSpeedFactor.ToString("F1")+"x"; };
            _popupControls.Add(trkPopupSpeed); _popupControls.Add(lblPopupSpeed);
            yR += SL;

            yR += 10;

            // ── GENERAL ───────────────────────────────────────────────────────
            Section(T("GENERAL","ALLGEMEIN"), c2, yR, cW2); yR += 26;
            chkScanlines = Chk("CRT Scanlines",   cur.ShowScanlines, c2,     yR);
            chkWatermark = Chk(T("Watermark","Wasserzeichen"), cur.ShowWatermark, c2+126, yR);
            chkVeeam100  = Chk(T("Veeam 100 Names","Veeam 100 Namen"), cur.ShowVeeam100, c2+256, yR);
            // Live-sync general checkboxes → cur
            chkScanlines.CheckedChanged += delegate { cur.ShowScanlines = chkScanlines.Checked; };
            chkWatermark.CheckedChanged += delegate { cur.ShowWatermark = chkWatermark.Checked; };
            chkVeeam100.CheckedChanged  += delegate { cur.ShowVeeam100  = chkVeeam100.Checked;  };
            yR += 28;

            // Watermark text – own row
            DLbl(T("Watermark:", "Wasserzeichen:"), c2, yR+5, 96);
            txtWatermark = new TextBox { Location=new Point(c2+100, yR), Size=new Size(cW2-104, 24),
                Text=cur.WatermarkText, BackColor=Color.FromArgb(28,28,28),
                ForeColor=Color.FromArgb(0,210,60), BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtWatermark);
            yR += 30;

            // Subtitle text – own row (wide!)
            DLbl(T("Subtitle:", "Untertitel:"), c2, yR+5, 62);
            txtWatermarkSub = new TextBox { Location=new Point(c2+66, yR), Size=new Size(cW2-70, 24),
                Text=cur.WatermarkSubText, BackColor=Color.FromArgb(28,28,28),
                ForeColor=Color.FromArgb(0,210,60), BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtWatermarkSub);
            yR += 30;

            DLbl(T("Custom terms (comma-separated):","Eigene Begriffe (kommagetrennt):"), c2, yR+5);
            yR += 22;
            txtExtra = new TextBox { Location=new Point(c2, yR), Size=new Size(cW2-4, 24),
                Text=cur.ExtraWords, BackColor=Color.FromArgb(28,28,28),
                ForeColor=Color.FromArgb(0,210,60), BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtExtra);
            yR += 30;

            // ── PREVIEW COLUMN (16:9 fixed) ───────────────────────────────────
            int colH = Math.Max(yL, yR) - y;
            // Thin vertical divider between content and preview
            Controls.Add(new Panel { Location=new Point(858, y), Size=new Size(1, colH+14),
                BackColor=Color.FromArgb(0,52,16) });
            Section(T("LIVE PREVIEW","LIVE-VORSCHAU"), c3, y, cW3);
            picPreview = new PictureBox {
                Location   = new Point(c3, y + 26),
                Size       = new Size(PREV_W + 2, PREV_H + 2),   // +2 for FixedSingle border
                BackColor  = Color.FromArgb(4, 4, 4),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(picPreview);
            picPreview.Paint += delegate(object ps, PaintEventArgs pe)
            {
                if (_prevEngine != null) _prevEngine.Render(pe.Graphics);
            };

            // Wire all controls to mark preview dirty (exclude cboLanguage and txtFontPreviewText)
            foreach (Control ctrl in Controls)
            {
                if (ctrl is TrackBar)
                    ((TrackBar)ctrl).ValueChanged += delegate { MarkDirty(); };
                if (ctrl is CheckBox)
                    ((CheckBox)ctrl).CheckedChanged += delegate { MarkDirty(); };
                if (ctrl is ComboBox && ctrl != cboProfiles && ctrl != cboLanguage)
                    ((ComboBox)ctrl).SelectedIndexChanged += delegate { MarkDirty(); };
                if (ctrl is TextBox && ctrl != txtFontPreviewText)
                    ((TextBox)ctrl).TextChanged += delegate { MarkDirty(); };
            }

            // Build initial engine and start animation
            RebuildPreview();
            _prevTimer = new Timer { Interval = 25 };
            _prevTimer.Tick += OnPreviewTick;
            _prevTimer.Start();

            // ── Bottom bar ────────────────────────────────────────────────────
            int yBot = Math.Max(yL, yR) + 14;
            HSep(yBot, 14, fw-28); yBot += 12;

            int bRight = c2 + cW2;
            var btnOK = new Button { Text="OK",
                Location=new Point(bRight-232, yBot), Size=new Size(108,32),
                DialogResult=DialogResult.OK,
                BackColor=Color.FromArgb(0,118,34), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(0,200,55);
            btnOK.Click += delegate
            {
                cur.Orientation      = cboOrient.Text;
                cur.WordOrientation  = cboWordOrient.Text;
                cur.WordMode         = cboWordMode.Text;
                // cur.WordStyle already synced by SetWordStyle()
                cur.ShowScanlines    = chkScanlines.Checked;
                cur.ShowWatermark    = chkWatermark.Checked;
                cur.ShowVeeam100     = chkVeeam100.Checked;
                cur.WatermarkText    = txtWatermark.Text.Trim();
                cur.WatermarkSubText = txtWatermarkSub.Text.Trim();
                cur.ExtraWords       = txtExtra.Text.Trim();
                cur.Language         = cboLanguage  != null ? cboLanguage.Text  : cur.Language;
                if (trkPopupSpeed != null) cur.PopupSpeedFactor = trkPopupSpeed.Value / 10f;
                if (cboWordFontName.SelectedItem != null) cur.WordFontName = cboWordFontName.SelectedItem.ToString();
                // cur.PopupEffects already in sync via SetPopupEffect()
                Result=cur; Result.Save();
            };
            var btnCancel = new Button { Text=T("Cancel","Abbrechen"),
                Location=new Point(bRight-118, yBot), Size=new Size(118,32),
                DialogResult=DialogResult.Cancel,
                BackColor=Color.FromArgb(50,15,15), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(130,36,36);
            Controls.Add(btnOK); Controls.Add(btnCancel);
            AcceptButton=btnOK; CancelButton=btnCancel;

            // ── Banner image (sidecar: VeeaMatrix-banner.jpg next to .scr) ───
            int finalH = yBot + 48;
            try
            {
                string bannerPath = FindBannerPath();
                if (bannerPath != null)
                {
                    var bannerImg = Image.FromFile(bannerPath);
                    int bannerW = bRight - c1 - 4;
                    int bannerH = Math.Min(120, (int)((double)bannerW / bannerImg.Width * bannerImg.Height));
                    var picBanner = new PictureBox {
                        Location    = new Point(c1, yBot + 44),
                        Size        = new Size(bannerW, bannerH),
                        SizeMode    = PictureBoxSizeMode.Zoom,
                        Image       = bannerImg,
                        BackColor   = Color.Black,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    Controls.Add(picBanner);
                    finalH = yBot + 44 + bannerH + 12;
                }
            }
            catch { }

            // ── Enable/disable sections based on initial WordMode + style ────
            SyncWordModeVisibility();
            SyncWordStyleDirection();

            // ── Hover tooltips ────────────────────────────────────────────────
            _tip = new ToolTip { AutoPopDelay=9000, InitialDelay=500, ReshowDelay=300, ShowAlways=true };
            Action<Control,string,string> tip = (ctrl,en,de) => { if (ctrl!=null) _tip.SetToolTip(ctrl, T(en,de)); };
            // Rain section
            tip(btnRainColor,    "Color of the falling background characters",                                "Farbe der fallenden Hintergrund-Zeichen");
            tip(btnHeadColor,    "Color of the bright leading character in each rain column",                 "Farbe des hellen Kopfzeichens im Regen");
            tip(trkFont,         "Character size for the background rain (px)",                              "Zeichengröße des Hintergrundregens (px)");
            tip(trkSpeed,        "Overall animation speed multiplier (1x = default)",                        "Animationsgeschwindigkeit (1x = Standard)");
            tip(trkFade,         "Trail persistence — lower value = longer glowing trail",                   "Spurlänge — kleiner Wert = längere Leuchtspur");
            tip(cboOrient,       "Direction the rain falls: TopDown / BottomUp / LeftRight / RightLeft",     "Regenrichtung: Von oben / unten / links / rechts");
            tip(cboWordMode,     "Rain = keyword scrolls only · Popup = blips only · Both = mixed",          "Wortmodus: Regen / Popup / Beides");
            // Words section
            tip(btnWordColor,    "Color of the keyword stream characters",                                   "Farbe der Keyword-Stream-Zeichen");
            tip(btnWordHeadColor,"Color of the leading (head) character in keyword streams",                 "Farbe des Kopfzeichens in Keyword-Streams");
            tip(cboWordFontName, "Font used for keyword streams, popups and watermark",                      "Schriftart für Keyword-Streams, Popups und Wasserzeichen");
            tip(txtFontPreviewText,"Edit the sample text shown in the font preview box",                     "Vorschautext für die Schriftart-Vorschau ändern");
            if (btnWordStyles != null && btnWordStyles.Length == 5)
            {
                tip(btnWordStyles[0], "Scroll — keyword scrolls across the screen",                         "Scroll — Keyword scrollt über den Bildschirm");
                tip(btnWordStyles[1], "Fade — keyword fades in and out in place",                           "Fade — Keyword blendet an Ort und Stelle ein/aus");
                tip(btnWordStyles[2], "Build — chars decode left-to-right (direction-aware)",               "Build — Zeichen werden von links nach rechts eingeblendet");
                tip(btnWordStyles[3], "Scramble — noise resolves to the correct word sequentially",         "Scramble — Rauschen löst sich sequenziell auf");
                tip(btnWordStyles[4], "Glitch — word appears through noise that gradually clears",          "Glitch — Wort erscheint durch Rauschen, das sich auflöst");
            }
            tip(cboWordOrient,   "Direction for keyword streams — only active for Scroll style",            "Richtung der Keyword-Streams — nur bei Scroll-Stil aktiv");
            tip(trkWordFont,     "Character size for keyword streams (px)",                                  "Zeichengröße der Keyword-Streams (px)");
            tip(trkWordSpeed,    "Speed multiplier for keyword streams",                                     "Geschwindigkeit der Keyword-Streams");
            tip(trkWordCount,    "Number of simultaneous keyword streams on screen",                         "Anzahl gleichzeitiger Keyword-Streams");
            // Popup section
            tip(btnPopupColor,   "Color of popup word blips",                                               "Farbe der Popup-Wörter");
            if (btnFxEffects != null && btnFxEffects.Length == 5)
            {
                tip(btnFxEffects[0], "Fade — popup fades in and out smoothly",                             "Fade — Popup blendet sanft ein/aus");
                tip(btnFxEffects[1], "Glitch — popup decodes from random noise, holds with rare glitch",   "Glitch — Popup löst sich aus Rauschen, hält mit seltenen Störungen");
                tip(btnFxEffects[2], "Scan — popup types out left-to-right, then de-resolves",             "Scan — Popup tippt sich von links nach rechts ein und auf");
                tip(btnFxEffects[3], "Zoom — popup zooms in from large to normal size",                    "Zoom — Popup zoomt von groß auf normale Größe");
                tip(btnFxEffects[4], "Scramble — chars resolve left-to-right, then reverse on exit",       "Scramble — Zeichen lösen sich L→R auf und kehren beim Verschwinden um");
            }
            tip(trkPopupFont,    "Font size for popup word blips (px)",                                     "Schriftgröße der Popup-Wörter (px)");
            tip(trkPopupCount,   "Number of simultaneous popup blips on screen",                            "Anzahl gleichzeitiger Popup-Wörter");
            tip(trkPopupSpeed,   "Speed of popup appearance and disappearance (higher = faster)",           "Geschwindigkeit der Popup-Ein-/Ausblendung (höher = schneller)");
            // General section
            tip(chkScanlines,    "Adds a subtle CRT monitor scanline overlay",                              "Fügt einen dezenten CRT-Scanline-Effekt hinzu");
            tip(chkWatermark,    "Shows faint watermark text centered in the background",                   "Zeigt Wasserzeichen-Text im Hintergrund");
            tip(chkVeeam100,     "Include Veeam Vanguard / Legend / MVP 2026 member names in streams",      "Veeam Vanguard / Legend / MVP 2026 Namen in Streams einblenden");
            tip(txtWatermark,    "Main watermark text shown in the background center",                      "Haupt-Wasserzeichentext in der Bildschirmmitte");
            tip(txtWatermarkSub, "Subtitle line shown below the main watermark",                            "Untertitelzeile unterhalb des Wasserzeichens");
            tip(txtExtra,        "Add your own terms, comma-separated  e.g. MYPRODUCT,FEATURE A",          "Eigene Begriffe kommagetrennt  z.B. MEIN PRODUKT,FEATURE A");

            ClientSize = new Size(fw, finalH);
            Text = T("VeeaMatrix  –  Settings", "VeeaMatrix  –  Einstellungen");
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
                    float fs = ph * 0.56f;
                    using (Font f = new Font(fname, fs, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        SizeF sz = g.MeasureString(sample, f);
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

        private void MarkDirty() { _previewDirty = true; }

        // Single-select: highlight chosen button and update cur.PopupEffects
        private void SetPopupEffect(string name)
        {
            if (btnFxEffects == null) return;
            string[] valid = new string[]{ "Fade", "Glitch", "Scan", "Zoom", "Scramble" };
            bool found = false;
            foreach (string n in valid) if (n == name) { found = true; break; }
            if (!found) name = "Glitch";
            cur.PopupEffects = name;
            foreach (Button b in btnFxEffects)
            {
                bool active = ((string)b.Tag == name);
                b.BackColor = active ? Color.FromArgb(0,100,28)       : Color.FromArgb(28,28,28);
                b.ForeColor = active ? Color.White                     : Color.FromArgb(155,155,155);
                b.FlatAppearance.BorderColor = active ? Color.FromArgb(0,185,55) : Color.FromArgb(55,55,55);
            }
        }

        // Single-select: highlight chosen word style button and update cur.WordStyle
        private void SetWordStyle(string name)
        {
            if (btnWordStyles == null) return;
            string[] valid = new string[]{ "Scroll", "Fade", "Build", "Scramble", "Glitch" };
            bool found = false;
            foreach (string n in valid) if (n == name) { found = true; break; }
            if (!found) name = "Scroll";
            cur.WordStyle = name;
            foreach (Button b in btnWordStyles)
            {
                bool active = ((string)b.Tag == name);
                b.BackColor = active ? Color.FromArgb(0,100,28)       : Color.FromArgb(28,28,28);
                b.ForeColor = active ? Color.White                     : Color.FromArgb(155,155,155);
                b.FlatAppearance.BorderColor = active ? Color.FromArgb(0,185,55) : Color.FromArgb(55,55,55);
            }
            SyncWordStyleDirection();
        }

        // Disable word Direction controls when style has no scrolling direction (static styles)
        private void SyncWordStyleDirection()
        {
            bool isStatic = (cur.WordStyle == "Fade" || cur.WordStyle == "Build" ||
                             cur.WordStyle == "Scramble" || cur.WordStyle == "Glitch");
            if (isStatic)
            {
                if (cboWordOrient  != null) cboWordOrient.Enabled  = false;
                if (_lblWordOrient != null) _lblWordOrient.Enabled = false;
            }
            // Enabling is handled by SyncWordModeVisibility — call it first for correct combined state
        }

        // Enable / disable controls depending on which layers are active
        private void SyncWordModeVisibility()
        {
            string mode = cboWordMode != null ? cboWordMode.Text : cur.WordMode;
            bool hasStream = (mode == "Rain" || mode == "Both");
            bool hasPopup  = (mode == "Popup" || mode == "Both");
            foreach (var c in _streamControls) c.Enabled = hasStream;
            foreach (var c in _popupControls)  c.Enabled = hasPopup;
        }

        private void RebuildPreview()
        {
            _previewDirty = false;
            var s = Clone(cur);
            if (cboOrient       != null) s.Orientation     = cboOrient.Text;
            if (cboWordOrient   != null) s.WordOrientation  = string.IsNullOrEmpty(cboWordOrient.Text) ? "Same" : cboWordOrient.Text;
            if (cboWordMode     != null) s.WordMode         = cboWordMode.Text;
            s.WordStyle = cur.WordStyle;
            if (cboWordFontName != null && cboWordFontName.SelectedItem != null)
                s.WordFontName = cboWordFontName.SelectedItem.ToString();
            if (trkFont         != null) s.FontSize         = trkFont.Value;
            if (trkSpeed        != null) s.SpeedFactor      = trkSpeed.Value / 10f;
            if (trkFade         != null) s.FadeAlpha        = trkFade.Value;
            if (trkWordFont     != null) s.WordFontSize     = trkWordFont.Value;
            if (trkWordSpeed    != null) s.WordSpeedFactor  = trkWordSpeed.Value / 10f;
            if (trkWordCount    != null) s.WordCount        = trkWordCount.Value;
            if (trkPopupFont    != null) s.PopupFontSize    = trkPopupFont.Value;
            if (trkPopupCount   != null) s.PopupCount       = trkPopupCount.Value;
            if (trkPopupSpeed   != null) s.PopupSpeedFactor = trkPopupSpeed.Value / 10f;
            if (txtWatermark    != null) s.WatermarkText    = txtWatermark.Text.Trim();
            if (txtWatermarkSub != null) s.WatermarkSubText = txtWatermarkSub.Text.Trim();
            if (txtExtra        != null) s.ExtraWords       = txtExtra.Text.Trim();
            // Effects – kept in sync by SetPopupEffect()
            s.PopupEffects = cur.PopupEffects;
            // General flags – live-synced to cur, but read directly for safety
            if (chkScanlines != null) s.ShowScanlines = chkScanlines.Checked;
            if (chkWatermark != null) s.ShowWatermark = chkWatermark.Checked;
            if (chkVeeam100  != null) s.ShowVeeam100  = chkVeeam100.Checked;

            if (_prevEngine != null) { _prevEngine.Dispose(); _prevEngine = null; }
            if (picPreview != null && picPreview.Width > 8 && picPreview.Height > 8)
                _prevEngine = new MatrixEngine(s, picPreview.Width-2, picPreview.Height-2);
        }

        private void OnPreviewTick(object sender, EventArgs e)
        {
            if (_previewDirty) RebuildPreview();
            if (_prevEngine == null || picPreview == null) return;
            _prevEngine.Tick();
            picPreview.Invalidate();
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
                MarkDirty();
                break;
            }
        }

        private void SaveCurrentAsProfile()
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(
                T("Profile name:", "Profilname:"),
                T("Save Profile", "Profil speichern"),
                T("My Profile", "Mein Profil"));
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
            using (ColorDialog dlg = new ColorDialog { Color=field, FullOpen=true })
                if (dlg.ShowDialog(this)==DialogResult.OK) { field=dlg.Color; SetBtn(btn,dlg.Color); MarkDirty(); }
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
                Screen primary = Screen.PrimaryScreen;
                var mainForm   = new ScreenSaverForm(s, primary.Bounds, true);

                mainForm.Load += delegate
                {
                    foreach (Screen scr in Screen.AllScreens)
                    {
                        if (scr == primary) continue;
                        var sec = new ScreenSaverForm(s, scr.Bounds, false);
                        mainForm.FormClosed += delegate { try { sec.Close(); } catch { } };
                        sec.Show();
                    }
                };

                Application.Run(mainForm);
            }
        }
    }
}
