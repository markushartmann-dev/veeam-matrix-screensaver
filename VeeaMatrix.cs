// VeeaMatrix.cs  –  Windows Screensaver v1.48
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
            new ColorProfile
            {
                Name          = "Deep Space",
                RainColor     = Color.FromArgb(160,  32, 240),  // vivid violet
                HeadColor     = Color.FromArgb(230, 200, 255),  // pale lavender
                WordColor     = Color.FromArgb(255, 200,  50),  // gold
                WordHeadColor = Color.FromArgb(255, 255, 255),  // white
                PopupColor    = Color.FromArgb(180,  80, 255),  // bright violet
            },
            new ColorProfile
            {
                Name          = "Aurora",
                RainColor     = Color.FromArgb(  0, 210, 160),  // deep emerald-teal
                HeadColor     = Color.FromArgb(200, 255, 240),  // pale ice-green
                WordColor     = Color.FromArgb(  0, 255, 200),  // bright mint
                WordHeadColor = Color.FromArgb(180, 255, 255),  // icy blue-white
                PopupColor    = Color.FromArgb( 80, 255, 220),  // neon mint
            },
            new ColorProfile
            {
                Name          = "Star Wars",
                RainColor     = Color.FromArgb(255, 232,  31),  // classic crawl yellow
                HeadColor     = Color.FromArgb(255, 255, 200),  // pale yellow-white
                WordColor     = Color.FromArgb(255, 210,   0),  // gold
                WordHeadColor = Color.FromArgb(255, 255, 255),  // white
                PopupColor    = Color.FromArgb(255, 160,   0),  // amber
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
        public int    FadeAlpha     = 16;
        public int    FontSize      = 20;
        public float  SpeedFactor   = 0.5f;
        public bool   ShowScanlines = true;
        public bool   ShowWatermark = true;
        // Falling words
        public string WordMode      = "Both";
        public int    WordCount     = 3;
        public int    WordFontSize  = 20;
        public Color  WordColor     = Color.FromArgb(0, 255, 65);
        public Color  WordHeadColor = Color.White;
        public float  GlowChance    = 0.22f;
        // Popup words
        public string PopupEffects      = "Glitch";
        public int    PopupCount        = 3;
        public int    PopupFontSize     = 20;
        public Color  PopupColor        = Color.FromArgb(0, 255, 65);
        public float  PopupSpeedFactor  = 1.0f;
        // General
        public string Orientation     = "TopDown";
        public string WordOrientation  = "LeftRight";
        public string WordStyle        = "Glitch";
        public float  WordSpeedFactor  = 0.5f;
        public bool   CrawlHideRain    = false;   // suppress background rain while Crawl is active
        public bool   CrawlStarfield   = false;   // draw star field behind Crawl words
        public bool   OrderedTerms     = false;   // use terms in sequential order, no random
        public bool   ShowVeeam100     = true;
        public bool   UseBuiltinTerms  = true;
        // Watermark
        public string WatermarkText    = "VEEAM";
        public string WatermarkSubText = "The Veeam-Themed Matrix Screensaver: | Nobody Asked For (But Everyone Needs)";
        public string ExtraWords      = "";
        public string WordFontName    = "Verdana";
        public bool   WordFontBold    = true;
        public bool   WordFontItalic  = false;
        // UI
        public string Language        = "EN";
        public bool   DarkMode        = true;

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
            sb.AppendLine("CrawlHideRain="    + CrawlHideRain);
            sb.AppendLine("CrawlStarfield="   + CrawlStarfield);
            sb.AppendLine("OrderedTerms="     + OrderedTerms);
            sb.AppendLine("ShowVeeam100="     + ShowVeeam100);
            sb.AppendLine("UseBuiltinTerms="  + UseBuiltinTerms);
            sb.AppendLine("WatermarkText="    + WatermarkText);
            sb.AppendLine("WatermarkSubText=" + WatermarkSubText);
            sb.AppendLine("ExtraWords="       + ExtraWords);
            sb.AppendLine("WordFontName="     + WordFontName);
            sb.AppendLine("WordFontBold="     + WordFontBold);
            sb.AppendLine("WordFontItalic="   + WordFontItalic);
            sb.AppendLine("Language="         + Language);
            sb.AppendLine("DarkMode="         + DarkMode);
            File.WriteAllText(ConfigFile, sb.ToString(), Encoding.UTF8);
        }

        public static Settings Load()
        {
            MigrateIfNeeded();
            var s = new Settings();
            if (!File.Exists(ConfigFile))
            {
                try { s.Save(); } catch { }  // create config.ini with defaults on first run
                return s;
            }
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
                        case "CrawlHideRain":    s.CrawlHideRain    = bool.Parse(v); break;
                        case "CrawlStarfield":   s.CrawlStarfield   = bool.Parse(v); break;
                        case "OrderedTerms":     s.OrderedTerms     = bool.Parse(v); break;
                        case "ShowVeeam100":     s.ShowVeeam100     = bool.Parse(v); break;
                        case "UseBuiltinTerms":  s.UseBuiltinTerms  = bool.Parse(v); break;
                        case "WatermarkText":    s.WatermarkText    = v; break;
                        case "WatermarkSubText": s.WatermarkSubText = v; break;
                        case "ExtraWords":       s.ExtraWords       = v; break;
                        case "WordFontName":     s.WordFontName     = v; break;
                        case "WordFontBold":     s.WordFontBold     = bool.Parse(v); break;
                        case "WordFontItalic":   s.WordFontItalic   = bool.Parse(v); break;
                        case "Language":         s.Language         = v; break;
                        case "DarkMode":         s.DarkMode         = bool.Parse(v); break;
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
            // Migrate old/empty/partial subtitle to the current default and persist
            string newSub = "The Veeam-Themed Matrix Screensaver: | Nobody Asked For (But Everyone Needs)";
            string sub = s.WatermarkSubText ?? "";
            bool needsMigration =
                string.IsNullOrWhiteSpace(sub) ||
                sub == "DATA PROTECTION * CYBER RESILIENCE * ALWAYS-ON" ||
                sub == "DATA PROTECTION  *  CYBER RESILIENCE  *  ALWAYS-ON" ||
                // Partial new subtitle: starts correctly but missing second half
                (sub.StartsWith("The Veeam-Themed Matrix Screensaver") &&
                 !sub.Contains("Nobody Asked For"));
            if (needsMigration)
            {
                s.WatermarkSubText = newSub;
                try { s.Save(); } catch { }
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
        internal static readonly string[] TERMS = new string[]
        {
            "VEEAM","VEEAM DATA PLATFORM","VEEAM DATA CLOUD","VEEAM DATA CLOUD VAULT",
            "BACKUP & REPLICATION","VBR","VBR V13","VEEAM ONE","VEEAM RECOVERY ORCHESTRATOR",
            "VEEAM AGENT","BACKUP FOR MICROSOFT 365","VBO365",
            "KASTEN BY VEEAM","COVEWARE BY VEEAM","VEEAM BACKUP FOR SALESFORCE",
            "INSTANT RECOVERY","INSTANT VM RECOVERY","SUREBACKUP","SURE REPLICA",
            "CONTINUOUS DATA PROTECTION","CDP",
            "HIGH AVAILABILITY","HA",
            "HARDENED REPOSITORY","IMMUTABLE BACKUPS","IMMUTABILITY",
            "AIR-GAPPED REPOSITORY","AIR GAP","ZERO TRUST","ZERO-TRUST RESILIENCE",
            "CYBER VAULT","CYBER RESILIENCE","RANSOMWARE RECOVERY","RANSOMWARE PROTECTION",
            "MALWARE DETECTION","THREAT HUNTING",
            "RECON SCANNER","AGENT COMMANDER","SECURITI AI","DSPM, DSP & AI TRISM",
            "SCALE-OUT BACKUP REPOSITORY","SOBR",
            "PERFORMANCE TIER","CAPACITY TIER","ARCHIVE TIER",
            "DEDUPLICATION","COMPRESSION","WAN ACCELERATION",
            "ENCRYPTION AT REST","ENCRYPTION IN FLIGHT",
            "CLOUD CONNECT","VCSP","MSP","VUL","VEEAM UNIVERSAL LICENSE",
            "VDP PREMIUM","VDP ADVANCED","VDP ESSENTIALS",
            "VEEAM FOR AWS","VEEAM FOR AZURE","VEEAM FOR GCP",
            "AWS","MICROSOFT AZURE","GOOGLE CLOUD",
            "VMWARE VSPHERE","VMWARE VCF","MICROSOFT HYPER-V",
            "NUTANIX AHV","KUBERNETES","RED HAT OPENSHIFT",
            "OBJECT STORAGE","MICROSOFT 365","AWS S3","AZURE BLOB STORAGE","S3 GLACIER",
            "NFS","SMB / CIFS","ISCSI","FIBRE CHANNEL","TAPE",
            "RPO","RTO","SLA","FAILOVER","FAILBACK","ZERO DATA LOSS","3-2-1 RULE","3-2-1-1-0",
            "GDPR","HIPAA","SOC 2","ISO 27001","DATA SOVEREIGNTY","ALWAYS-ON DATA",
            "DATA PROTECTION","DATA SECURITY","COMPLIANCE",
            "VEEAM'S DATA COMMAND GRAPH",
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

        private float[] starX, starY, starBright, starSize, starTwinkle;
        private int     _termIndex = 0;   // sequential term counter (used when OrderedTerms=true)

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
            var list = new List<string>();
            if (s.UseBuiltinTerms)
            {
                string termsFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "VeeaMatrix", "terms.txt");
                if (File.Exists(termsFile))
                {
                    try
                    {
                        foreach (string ln in File.ReadAllLines(termsFile, Encoding.UTF8))
                        { string t = ln.Trim(); if (t.Length > 0 && !t.StartsWith("#")) list.Add(t.ToUpper()); }
                    }
                    catch { list.AddRange(TERMS); }
                }
                else { list.AddRange(TERMS); }
            }
            if (s.ShowVeeam100)
                list.AddRange(VEEAM100_PEOPLE);
            if (!string.IsNullOrEmpty(s.ExtraWords))
                foreach (string p in s.ExtraWords.Split(new char[]{',','|','\n','\r'}, StringSplitOptions.RemoveEmptyEntries))
                { string t = p.Trim().ToUpper(); if (t.Length > 0) list.Add(t); }
            // No fallback to "VEEAM" — if every Veeam source is off and no custom words
            // are defined, allTerms stays empty and no word drops / popups are spawned.
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

            FontStyle wfs = FontStyle.Regular;
            if (s.WordFontBold)   wfs |= FontStyle.Bold;
            if (s.WordFontItalic) wfs |= FontStyle.Italic;
            if (wfs == FontStyle.Regular) wfs = FontStyle.Regular; // keep Regular as fallback
            rainFont  = new Font("Courier New", Math.Max(6, s.FontSize     - 1), FontStyle.Bold,  GraphicsUnit.Pixel);
            wordFont  = new Font(s.WordFontName, Math.Max(6, s.WordFontSize - 1), wfs,             GraphicsUnit.Pixel);
            popupFont = new Font(s.WordFontName, Math.Max(6, s.PopupFontSize- 1), wfs,             GraphicsUnit.Pixel);
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
            if ((s.WordMode == "Rain" || s.WordMode == "Both") && allTerms.Length > 0)
            {
                for (int i = 0; i < s.WordCount; i++) wdrops.Add(SpawnDrop(true));
            }

            popups.Clear();
            if ((s.WordMode == "Popup" || s.WordMode == "Both") && allTerms.Length > 0)
                for (int i = 0; i < s.PopupCount; i++) popups.Add(SpawnPopup(true));

            BuildScanlines();
            if (s.CrawlStarfield) InitStars(); else starX = null;
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

        // ── Collision-avoidance helpers ──────────────────────────────────────────

        // Current on-screen bounding box of a word drop
        private RectangleF DropBounds(WDrop w)
        {
            float wid, hgt;
            if (w.CharOffsets != null)           // static mode or horizontal scroll
            { wid = w.CharOffsets[w.Chars.Length]; hgt = wordLineH; }
            else if (WordIsVertical)              // vertical scroll: chars stacked
            { wid = s.WordFontSize * 1.5f; hgt = w.Chars.Length * wordLineH; }
            else                                  // horizontal scroll, no charOffsets
            { wid = w.Chars.Length * s.WordFontSize * 0.65f; hgt = wordLineH; }
            return new RectangleF(w.X, w.Y, wid, hgt);
        }

        // Bounding box of a popup (centered on CX/CY)
        private RectangleF PopupBounds(WPopup p)
        {
            float wid = p.Word.Length * p.FontSize * 0.70f;
            float hgt = p.FontSize * 1.5f;
            return new RectangleF(p.CX - wid * 0.5f, p.CY - hgt * 0.5f, wid, hgt);
        }

        // True if r (expanded by pad) overlaps any currently visible word.
        // Static styles: checks drops + popups.  Scroll styles: popups only (drops move).
        private bool CollidesWithActive(RectangleF r, float pad)
        {
            RectangleF pr = RectangleF.Inflate(r, pad, pad);
            bool isStatic = (s.WordStyle == "Fade"  || s.WordStyle == "Build" ||
                             s.WordStyle == "Scramble" || s.WordStyle == "Glitch");
            if (isStatic)
                foreach (WDrop d in wdrops)
                    if (DropBounds(d).IntersectsWith(pr)) return true;
            foreach (WPopup p in popups)
                if (PopupBounds(p).IntersectsWith(pr)) return true;
            return false;
        }

        // ─────────────────────────────────────────────────────────────────────────

        private string NextTerm()
        {
            if (allTerms.Length == 0) return "";
            if (s.OrderedTerms) return allTerms[_termIndex++ % allTerms.Length];
            return allTerms[rng.Next(allTerms.Length)];
        }

        private WDrop SpawnDrop(bool scatter)
        {
            string term  = NextTerm();
            char[] chars = term.ToCharArray();
            int    fs    = s.WordFontSize;

            // Crawl: perspective scroll upward, centered, font scales with Y
            if (s.WordStyle == "Crawl")
            {
                float cv = -(float)(1.5 * s.WordSpeedFactor);  // fixed speed — all words same pace, no overtaking
                float cx = W / 2f;
                // Initial base: first word enters at screen bottom immediately;
                // replacements queue just below screen. Queue loop then spaces all subsequent words.
                float cy = scatter ? H : H + fs * 4f;
                foreach (WDrop d in wdrops)
                {
                    float needed = d.Y + fs * 9.0f;  // min safe gap for CRAWL_SCALE 6.0 — no overlap
                    if (needed > cy) cy = needed;
                }
                return new WDrop { Chars=chars, X=cx, Y=cy, V=cv,
                                   Glow=rng.NextDouble()<s.GlowChance, CharOffsets=null };
            }

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
                // Retry up to 20 positions to avoid overlapping existing words or popups
                for (int attempt = 1; attempt < 20; attempt++)
                {
                    if (!CollidesWithActive(new RectangleF(sx, sy, estW, wordLineH), fs * 0.6f)) break;
                    sx = mgn + (float)(rng.NextDouble() * Math.Max(1f, W - estW - mgn));
                    sy = mgn + (float)(rng.NextDouble() * Math.Max(1f, H - fs * 3f - mgn));
                }
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
            // Lane check: avoid placing a new scroll drop in the same perpendicular lane
            // as an existing one, which would cause them to overlap when they converge.
            float laneMin = WordIsVertical ? fs * 2.2f : wordLineH * 1.8f;
            for (int attempt = 1; attempt < 20; attempt++)
            {
                bool conflict = false;
                foreach (WDrop d in wdrops)
                    if ((WordIsVertical ? Math.Abs(d.X - x) : Math.Abs(d.Y - y)) < laneMin)
                    { conflict = true; break; }
                if (!conflict) break;
                if (WordIsVertical)
                    x = fs * 0.5f + (float)(rng.NextDouble() * Math.Max(1, W - fs));
                else
                    y = fs * 0.5f + (float)(rng.NextDouble() * Math.Max(1, H - fs));
            }
            return new WDrop { Chars=chars, X=x, Y=y, V=WordIsForward?v:-v,
                               Glow=rng.NextDouble()<s.GlowChance, CharOffsets=charOff };
        }

        private WPopup SpawnPopup(bool scatter)
        {
            string term = NextTerm();
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
            // Retry up to 20 positions to avoid overlapping existing words or popups
            for (int attempt = 1; attempt < 20; attempt++)
            {
                if (!CollidesWithActive(new RectangleF(cx - estW * 0.5f, cy - fs * 0.75f, estW, fs * 1.5f), fs * 0.8f)) break;
                cx = margin + estW/2f + (float)(rng.NextDouble() * Math.Max(1, W - estW - 2f*margin));
                cy = margin + (float)(rng.NextDouble() * Math.Max(1, H - 2f*margin));
            }

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
                    string wmSub  = (s.WatermarkSubText ?? "").Replace(" | ", "\n").Replace("|", "\n");
                    FontStyle wmFs = FontStyle.Regular;
                    if (s.WordFontBold)   wmFs |= FontStyle.Bold;
                    if (s.WordFontItalic) wmFs |= FontStyle.Italic;
                    int lsz = Math.Max(24,(int)(W*0.08));
                    using (Font lf = new Font(s.WordFontName, lsz, wmFs, GraphicsUnit.Pixel))
                    using (SolidBrush lb = new SolidBrush(Color.FromArgb(10, s.RainColor)))
                    { SizeF ls=g.MeasureString(wmText,lf); g.DrawString(wmText,lf,lb,(W-ls.Width)/2f,(H-ls.Height)/2f); }
                    if (wmSub.Length > 0)
                    {
                        int ssz = Math.Max(7,(int)(W*0.015));
                        using (Font sf = new Font(s.WordFontName, ssz, wmFs, GraphicsUnit.Pixel))
                        using (SolidBrush sb = new SolidBrush(Color.FromArgb(7, s.RainColor)))
                        { SizeF ss=g.MeasureString(wmSub,sf); g.DrawString(wmSub,sf,sb,(W-ss.Width)/2f,H/2f+(int)(W*0.045f)); }
                    }
                }
            }
        }

        private void InitStars()
        {
            const int N = 280;
            starX = new float[N]; starY = new float[N];
            starBright = new float[N]; starSize = new float[N]; starTwinkle = new float[N];
            for (int i = 0; i < N; i++)
            {
                starX[i]      = (float)(rng.NextDouble() * W);
                starY[i]      = (float)(rng.NextDouble() * H);
                starBright[i] = 0.25f + (float)(rng.NextDouble() * 0.75f);
                starSize[i]   = 0.8f  + (float)(rng.NextDouble() * 2.0f);
                starTwinkle[i]= (float)(rng.NextDouble() * Math.PI * 2.0);
            }
        }

        private void DrawStars()
        {
            if (starX == null) return;
            for (int i = 0; i < starX.Length; i++)
            {
                starTwinkle[i] += 0.018f;
                float alpha = starBright[i] * (0.55f + 0.45f * (float)Math.Sin(starTwinkle[i]));
                int   a     = Math.Max(0, Math.Min(255, (int)(alpha * 230)));
                float sz    = starSize[i];
                tmpBrush.Color = Color.FromArgb(a, 255, 252, 220);  // warm white stars
                bg.FillEllipse(tmpBrush, starX[i] - sz * 0.5f, starY[i] - sz * 0.5f, sz, sz);
            }
        }

        public void Tick()
        {
            bool suppressRain = (s.WordStyle == "Crawl" && s.CrawlHideRain);
            if (suppressRain)
                bg.FillRectangle(Brushes.Black, 0, 0, W, H);  // pure black — no trail on Crawl words
            else
                bg.FillRectangle(fadeBrush, 0, 0, W, H);
            if (s.WordStyle == "Crawl" && s.CrawlStarfield) DrawStars();
            if (!suppressRain) DrawRain();
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
                    if (!TickStaticDrop(w)) { wdrops.RemoveAt(i); if (allTerms.Length > 0) wdrops.Add(SpawnDrop(false)); }
                }
                else if (s.WordStyle == "Crawl")
                {
                    // Perspective crawl — Star Wars horizon projection.
                    // HORIZON_FRAC: where the text vanishes (top area).
                    // t = 0 at horizon, 1 at screen bottom.
                    // Power curve keeps text large through most of screen; shrinks slowly toward top.
                    const float CRAWL_SCALE  = 6.00f;  // 2× previous 3.0 as requested (+100%)
                    const float HORIZON_FRAC = 0.30f;  // horizon at 30% from top — text shrinks visibly earlier
                    float rawT       = w.Y / (float)H;
                    float t          = Math.Max(0.0f, Math.Min(1.0f,
                                           (rawT - HORIZON_FRAC) / (1.0f - HORIZON_FRAC)));
                    // t^0.65: text stays large most of the journey, shrinks gradually near horizon
                    float tP         = (float)Math.Pow(t, 0.65);
                    float scaledSize = Math.Max(8f, fs * CRAWL_SCALE * tP);
                    FontStyle crawlFs = FontStyle.Bold | FontStyle.Italic;
                    bool gone = w.Y < (float)H * (HORIZON_FRAC - 0.05f);
                    if (!gone)
                    {
                        try
                        {
                            using (Font crawlFont = new Font(s.WordFontName, scaledSize, crawlFs, GraphicsUnit.Pixel))
                            {
                                string text  = new string(w.Chars);
                                SizeF  sz    = bg.MeasureString(text, crawlFont);
                                float  drawX = (W - sz.Width) / 2f;
                                float  drawY = w.Y - sz.Height / 2f;
                                // Fade: fully visible below t=0.2, smoothly transparent above t=0.05.
                                // Font at t=0.2 ≈ 70px (no jitter); at t=0.05 ≈ 27px (already faded to 0).
                                const float FADE_START = 0.20f;
                                const float FADE_END   = 0.05f;
                                float tFade    = Math.Max(0f, Math.Min(1f,
                                                     (t - FADE_END) / (FADE_START - FADE_END)));
                                float eFade    = (H - w.Y + sz.Height) / Math.Max(1f, sz.Height * 2f);
                                float fade     = Math.Min(1f, Math.Min(tFade, eFade));
                                int   alpha    = Clamp((int)(fade * 255));
                                if (alpha > 4)
                                {
                                    int cr = Clamp((int)(s.WordColor.R + (s.WordHeadColor.R - s.WordColor.R) * t));
                                    int cg = Clamp((int)(s.WordColor.G + (s.WordHeadColor.G - s.WordColor.G) * t));
                                    int cb = Clamp((int)(s.WordColor.B + (s.WordHeadColor.B - s.WordColor.B) * t));
                                    Color col = w.Glow
                                        ? Color.FromArgb(alpha, Clamp(cr+60), Clamp(cg+20), Clamp(cb+40))
                                        : Color.FromArgb(alpha, cr, cg, cb);
                                    tmpBrush.Color = col;
                                    bg.DrawString(text, crawlFont, tmpBrush, new PointF(drawX, drawY));
                                }
                            }
                        }
                        catch { }
                    }
                    w.Y += w.V;
                    if (gone) { wdrops.RemoveAt(i); if (allTerms.Length > 0) wdrops.Add(SpawnDrop(false)); }
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
                    if (gone) { wdrops.RemoveAt(i); if (allTerms.Length > 0) wdrops.Add(SpawnDrop(false)); }
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
                if(p.Phase==3){popups.RemoveAt(i); if(allTerms.Length > 0) popups.Add(SpawnPopup(false)); continue;}

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
        private CheckBox   chkScanlines, chkWatermark, chkVeeam100, chkBuiltinTerms;
        private CheckBox   chkWordFontBold, chkWordFontItalic;
        private CheckBox   chkCrawlHideRain;
        private CheckBox   chkCrawlStarfield;
        private CheckBox   chkOrderedTerms;
        private bool       _syncingOrient;
        // Theme colours — initialised at the top of Build() from cur.DarkMode
        private bool  _dark;
        private Color _panelBg, _sep, _lbl, _chk, _inputBg, _inputFg, _trkBg, _valFg;
        private Color _secTxt, _subHdr, _btnIna, _btnInaFg, _btnInaBdr;
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

        // Banner image hardcoded as Base64 (600x450 JPEG, 60% quality).
        // Priority order in LoadBannerImage: sidecar file > embedded resource > this constant.
        // This guarantees the banner is always visible even on a fresh download of the .scr.
        private const string BANNER_B64 =
            "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";

        // Load banner image: sidecar file (next to .scr or in %APPDATA%\VeeaMatrix) first,
        // then compiled /resource:, then hard-coded Base64 above as final fallback.
        // IMPORTANT: always return a stream-independent Bitmap copy so GDI+ does not keep
        // a dangling pointer into a MemoryStream that the GC is free to collect.
        private static Image LoadBannerImage()
        {
            string[] names  = new string[]{ "VeeaMatrix-banner.jpg", "VeeaMatrix-banner.png",
                                            "VeeaMatrix-banner.jpeg", "banner.jpg" };
            string exeDir = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
            string appDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VeeaMatrix");
            foreach (string dir in new string[]{ exeDir, appDir })
                foreach (string n in names)
                {
                    string p = Path.Combine(dir, n);
                    if (File.Exists(p))
                    {
                        try
                        {
                            // Read into byte[] first so we don't lock the file and don't leave
                            // a MemoryStream alive that GDI+ depends on via an unmanaged pointer.
                            byte[] data = File.ReadAllBytes(p);
                            using (var ms = new MemoryStream(data))
                            using (var tmp = Image.FromStream(ms))
                            {
                                var bmp = new Bitmap(tmp.Width, tmp.Height,
                                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                using (var g = Graphics.FromImage(bmp))
                                    g.DrawImage(tmp, 0, 0);
                                return bmp;
                            }
                        }
                        catch { }
                    }
                }
            // Embedded resource (compiled in via Build-VeeaMatrix.ps1 /resource flag)
            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var rs  = asm.GetManifestResourceStream("VeeaMatrix.banner");
                if (rs != null)
                {
                    using (rs)
                    {
                        // Slurp into a MemoryStream, decode, then copy into a standalone Bitmap.
                        // If we returned Image.FromStream(ms) directly the GC could collect ms
                        // while GDI+ still holds an unmanaged pointer into its buffer.
                        var ms = new MemoryStream((int)rs.Length);
                        rs.CopyTo(ms);
                        ms.Position = 0;
                        using (ms)
                        using (var tmp = Image.FromStream(ms))
                        {
                            var bmp = new Bitmap(tmp.Width, tmp.Height,
                                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            using (var g = Graphics.FromImage(bmp))
                                g.DrawImage(tmp, 0, 0);
                            return bmp;
                        }
                    }
                }
            }
            catch { }
            // Hard-coded fallback: Base64 banner compiled directly into the source
            try
            {
                byte[] data = Convert.FromBase64String(BANNER_B64);
                using (var ms = new MemoryStream(data))
                using (var tmp = Image.FromStream(ms))
                {
                    var bmp = new Bitmap(tmp.Width, tmp.Height,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(bmp))
                        g.DrawImage(tmp, 0, 0);
                    return bmp;
                }
            }
            catch { }
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
            chkScanlines = chkWatermark = chkVeeam100 = chkBuiltinTerms = null;
            chkWordFontBold = chkWordFontItalic = null;
            chkCrawlHideRain = null; chkCrawlStarfield = null; chkOrderedTerms = null;
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
                WordStyle=s.WordStyle, WordSpeedFactor=s.WordSpeedFactor, CrawlHideRain=s.CrawlHideRain, CrawlStarfield=s.CrawlStarfield, OrderedTerms=s.OrderedTerms,
                ShowVeeam100=s.ShowVeeam100,
                WatermarkText=s.WatermarkText, WatermarkSubText=s.WatermarkSubText, ExtraWords=s.ExtraWords,
                WordFontName=s.WordFontName, WordFontBold=s.WordFontBold, WordFontItalic=s.WordFontItalic,
                Language=s.Language,
                PopupSpeedFactor=s.PopupSpeedFactor,
                UseBuiltinTerms=s.UseBuiltinTerms,
                DarkMode=s.DarkMode };
        }

        // ── layout helpers ────────────────────────────────────────────────────
        private static int Clamp(int v, int lo, int hi) { return v<lo?lo:v>hi?hi:v; }
        private static void SetBtn(Button b, Color c)
        { b.BackColor=c; b.ForeColor=c.GetBrightness()>.45f?Color.Black:Color.White; }

        private void Section(string title, int x, int y, int w)
        {
            var pnl = new Panel { Location=new Point(x,y), Size=new Size(w,20),
                BackColor=_panelBg };
            Controls.Add(pnl);
            pnl.Controls.Add(new Label { Text=title, Location=new Point(8,2), AutoSize=true,
                ForeColor=_secTxt,
                Font=new Font("Segoe UI",8.5f,FontStyle.Bold) });
        }

        private void HSep(int y, int x=14, int w=832)
        { Controls.Add(new Panel { Location=new Point(x,y), Size=new Size(w,1), BackColor=_sep }); }

        private Label DLbl(string t, int x, int y, int w=-1)
        {
            var l = new Label { Text=t, Location=new Point(x,y), AutoSize=(w<0),
                ForeColor=_lbl };
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
                BackColor=_inputBg, ForeColor=_inputFg };
            c.Items.AddRange(items); c.Text=sel;
            Controls.Add(c); return c;
        }

        private CheckBox Chk(string text, bool val, int x, int y)
        {
            var c = new CheckBox { Text=text, Checked=val, Location=new Point(x,y),
                AutoSize=true, ForeColor=_chk };
            Controls.Add(c); return c;
        }

        private TrackBar SlRow(string name, int x, int y, int cw, int min, int max, int val, out Label vLbl)
        {
            const int LW=132, VW=54, G=6;
            DLbl(name, x, y+6, LW);
            var trk = new TrackBar { Location=new Point(x+LW+G,y), Size=new Size(cw-LW-VW-G*2,26),
                Minimum=min, Maximum=max, Value=Clamp(val,min,max),
                TickFrequency=Math.Max(1,(max-min)/10), SmallChange=1,
                BackColor=_trkBg, TickStyle=TickStyle.None };
            Controls.Add(trk);
            vLbl = new Label { Location=new Point(x+cw-VW,y+6), Size=new Size(VW,15),
                ForeColor=_valFg, TextAlign=ContentAlignment.MiddleRight };
            Controls.Add(vLbl);
            return trk;
        }

        private void Build()
        {
            _streamControls = new List<Control>();
            _popupControls  = new List<Control>();

            // ── Theme setup ──────────────────────────────────────────────────
            _dark     = cur.DarkMode;
            BackColor = _dark ? Color.FromArgb(54,57,54)    : Color.FromArgb(240,242,240);
            ForeColor = _dark ? Color.FromArgb(0,210,60)    : Color.FromArgb(0,140,45);
            _panelBg  = _dark ? Color.FromArgb(0,64,20)     : Color.FromArgb(0,148,46);
            _sep      = _dark ? Color.FromArgb(0,80,28)     : Color.FromArgb(0,155,48);
            _lbl      = _dark ? Color.FromArgb(218,222,218) : Color.FromArgb(35,35,35);
            _chk      = _dark ? Color.FromArgb(222,226,222) : Color.FromArgb(35,35,35);
            _inputBg  = _dark ? Color.FromArgb(68,72,68)    : Color.FromArgb(252,254,252);
            _inputFg  = _dark ? Color.FromArgb(0,220,65)    : Color.FromArgb(0,128,38);
            _trkBg    = BackColor;
            _valFg    = _dark ? Color.FromArgb(0,218,62)    : Color.FromArgb(0,128,38);
            _secTxt   = _dark ? Color.FromArgb(0,220,65)    : Color.White;
            _subHdr   = _dark ? Color.FromArgb(0,170,50)    : Color.FromArgb(0,115,35);
            _btnIna   = _dark ? Color.FromArgb(44,46,44)    : Color.FromArgb(210,215,210);
            _btnInaFg = _dark ? Color.FromArgb(155,155,155) : Color.FromArgb(60,65,60);
            _btnInaBdr= _dark ? Color.FromArgb(55,55,55)    : Color.FromArgb(148,158,148);

            // ── Layout constants ─────────────────────────────────────────────
            const int div2  = 506, div3  = 938;       // vertical divider x positions (1px lines)
            const int c1    = 14,  cW1   = 480;       // left column — RAIN + banner
            const int c2    = 516, cW2   = 412;       // middle column: 10px from div2 left, 10px from div3 right
            const int PREV_W = 640, PREV_H = 360;     // 16:9 live preview
            const int c3    = 948, cW3   = PREV_W+2; // right column: 10px from div3; cW3=642
            const int fw    = 1616;                   // form width (unchanged)
            const int SL    = 46;                     // slider row step
            const int CM    = 32;                     // combo row step

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

            // Theme toggle — right side, before language selector
            var btnTheme = new Button {
                Text      = _dark ? "☀ Light" : "🌙 Dark",
                Location  = new Point(fw-228, y), Size=new Size(72, 24),
                FlatStyle = FlatStyle.Flat,
                BackColor = _dark ? Color.FromArgb(60,62,60) : Color.FromArgb(200,210,200),
                ForeColor = _dark ? Color.FromArgb(220,220,220) : Color.FromArgb(30,30,30)
            };
            btnTheme.FlatAppearance.BorderColor = _dark ? Color.FromArgb(80,82,80) : Color.FromArgb(130,145,130);
            btnTheme.Click += delegate { cur.DarkMode = !cur.DarkMode; RebuildUI(); };
            Controls.Add(btnTheme);

            // Language selector – right-aligned in profile strip
            DLbl(T("Language:", "Sprache:"), fw-148, y+5, 72);
            cboLanguage = Cbo(fw-72, y, 60, new string[]{"EN","DE"}, cur.Language);
            // Wire language change BEFORE the generic wiring loop (so we can exclude it there)
            cboLanguage.SelectedIndexChanged += delegate { cur.Language = cboLanguage.Text; RebuildUI(); };

            y += 34; HSep(y, 14, fw-28); y += 12;

            int yL = y, yM = y, yR = y;   // independent column y cursors

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
            // MIDDLE COLUMN — WORD STREAMS
            // ═══════════════════════════════════════════════════════════════════
            Section(T("WORD STREAMS  (Rain / Both)", "WORT-STREAMS  (Regen / Beides)"), c2, yM, cW2); yM += 26;
            _streamControls.Add(DLbl(T("Colors:", "Farben:"), c2, yM+5)); yM += 20;
            btnWordColor     = ColBtn(T("Words","Wörter"),      cur.WordColor,     c2,     yM, 130);
            btnWordHeadColor = ColBtn(T("Head (bright)","Kopf (hell)"), cur.WordHeadColor, c2+136, yM, 130);
            btnWordColor.Click     += delegate { Pick(ref cur.WordColor,     btnWordColor); };
            btnWordHeadColor.Click += delegate { Pick(ref cur.WordHeadColor, btnWordHeadColor); };
            _streamControls.Add(btnWordColor); _streamControls.Add(btnWordHeadColor);
            yM += 32;

            // Font picker row
            DLbl(T("Font:", "Schriftart:"), c2, yM+5, 80);
            cboWordFontName = new ComboBox { Location=new Point(c2+84, yM), Size=new Size(200, 24),
                DropDownStyle=ComboBoxStyle.DropDownList,
                BackColor=_inputBg, ForeColor=_inputFg };
            using (var ifc = new System.Drawing.Text.InstalledFontCollection())
            {
                var fns = new System.Collections.Generic.SortedSet<string>();
                foreach (FontFamily ff in ifc.Families) fns.Add(ff.Name);
                foreach (string fn in fns) cboWordFontName.Items.Add(fn);
            }
            int selIdx = cboWordFontName.Items.IndexOf(cur.WordFontName);
            cboWordFontName.SelectedIndex = selIdx >= 0 ? selIdx : 0;
            Controls.Add(cboWordFontName);

            txtFontPreviewText = new TextBox { Location=new Point(c2+292, yM), Size=new Size(cW2-292-4, 24),
                Text="VEEAM", BackColor=_inputBg,
                ForeColor=_inputFg, BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtFontPreviewText);
            yM += 30;

            // Font style checkboxes
            chkWordFontBold   = Chk(T("Bold","Fett"),       cur.WordFontBold,   c2,     yM);
            chkWordFontItalic = Chk(T("Italic","Kursiv"),   cur.WordFontItalic, c2+68,  yM);
            chkWordFontBold.CheckedChanged   += delegate { cur.WordFontBold   = chkWordFontBold.Checked;   UpdateFontPreview(); MarkDirty(); };
            chkWordFontItalic.CheckedChanged += delegate { cur.WordFontItalic = chkWordFontItalic.Checked; UpdateFontPreview(); MarkDirty(); };
            _streamControls.Add(chkWordFontBold);
            _streamControls.Add(chkWordFontItalic);
            yM += 26;

            picFontPreview = new PictureBox { Location=new Point(c2, yM), Size=new Size(cW2-4, 44),
                BackColor=Color.Black, BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(picFontPreview);
            UpdateFontPreview();
            cboWordFontName.SelectedIndexChanged += delegate { UpdateFontPreview(); };
            txtFontPreviewText.TextChanged       += delegate { UpdateFontPreview(); };
            yM += 50;

            // ── Word Style single-select buttons ──────────────────────────────
            _streamControls.Add(DLbl(T("Style:","Stil:"), c2, yM+5));
            yM += 22;
            string[] wsNames = new string[]{ "Scroll", "Fade", "Build", "Scramble", "Glitch", "Crawl" };
            btnWordStyles = new Button[wsNames.Length];
            const int WS_W = 57, WS_GAP = 4;
            for (int wi = 0; wi < wsNames.Length; wi++)
            {
                string capturedWS = wsNames[wi];
                var wsBtn = new Button {
                    Text      = capturedWS,
                    Location  = new Point(c2 + wi * (WS_W + WS_GAP), yM),
                    Size      = new Size(WS_W, 26),
                    FlatStyle = FlatStyle.Flat,
                    Tag       = capturedWS
                };
                wsBtn.FlatAppearance.BorderSize = 1;
                wsBtn.Click += delegate { SetWordStyle(capturedWS, applyStyleDefaults: true); MarkDirty(); };
                Controls.Add(wsBtn);
                btnWordStyles[wi] = wsBtn;
                _streamControls.Add(wsBtn);
            }
            SetWordStyle(string.IsNullOrEmpty(cur.WordStyle) ? "Scroll" : cur.WordStyle);
            yM += 32;

            // ── Crawl-only options ────────────────────────────────────────────
            chkCrawlHideRain = Chk(T("Disable RAIN while Crawl active", "REGEN während Crawl ausblenden"),
                                   cur.CrawlHideRain, c2, yM);
            chkCrawlHideRain.CheckedChanged += delegate { cur.CrawlHideRain = chkCrawlHideRain.Checked; };
            _streamControls.Add(chkCrawlHideRain);
            Controls.Add(chkCrawlHideRain);
            chkCrawlStarfield = Chk(T("Star field background", "Sternenhimmel-Hintergrund"),
                                    cur.CrawlStarfield, c2 + 4, yM + 22);
            chkCrawlStarfield.CheckedChanged += delegate { cur.CrawlStarfield = chkCrawlStarfield.Checked; };
            _streamControls.Add(chkCrawlStarfield);
            Controls.Add(chkCrawlStarfield);
            yM += 48;

            // ── Word Direction ────────────────────────────────────────────────
            _lblWordOrient = DLbl(T("Direction:","Richtung:"), c2, yM+5, 68);
            _streamControls.Add(_lblWordOrient);
            cboWordOrient = Cbo(c2+72, yM, 200, new string[]{"Same","TopDown","BottomUp","LeftRight","RightLeft"},
                string.IsNullOrEmpty(cur.WordOrientation)?"Same":cur.WordOrientation);
            _streamControls.Add(cboWordOrient);
            cboWordOrient.SelectedIndexChanged += delegate { if (!_syncingOrient && cboWordOrient.SelectedIndex >= 0) cur.WordOrientation = cboWordOrient.Text; };
            yM += CM;

            trkWordFont = SlRow(T("Font Size","Schriftgröße"), c2,yM,cW2, 8,36, cur.WordFontSize, out lblWFont);
            lblWFont.Text = cur.WordFontSize+" px";
            trkWordFont.ValueChanged += delegate { cur.WordFontSize=trkWordFont.Value; lblWFont.Text=cur.WordFontSize+" px"; };
            _streamControls.Add(trkWordFont); _streamControls.Add(lblWFont);
            yM += SL;

            trkWordSpeed = SlRow(T("Speed","Geschwindigkeit"), c2,yM,cW2, 1,30, (int)(cur.WordSpeedFactor*10), out lblWordSpeed);
            lblWordSpeed.Text = cur.WordSpeedFactor.ToString("F1")+"x";
            trkWordSpeed.ValueChanged += delegate { cur.WordSpeedFactor=trkWordSpeed.Value/10f; lblWordSpeed.Text=cur.WordSpeedFactor.ToString("F1")+"x"; };
            _streamControls.Add(trkWordSpeed); _streamControls.Add(lblWordSpeed);
            yM += SL;

            trkWordCount = SlRow(T("Simultaneous","Gleichzeitig"), c2,yM,cW2, 1,30, cur.WordCount, out lblWCount);
            lblWCount.Text = cur.WordCount.ToString();
            trkWordCount.ValueChanged += delegate { cur.WordCount=trkWordCount.Value; lblWCount.Text=cur.WordCount.ToString(); };
            _streamControls.Add(trkWordCount); _streamControls.Add(lblWCount);
            yM += SL;

            yM += 12;
            HSep(yM, div2, div3-div2+1); yM += 12;

            // ═══════════════════════════════════════════════════════════════════
            // MIDDLE COLUMN — POPUP WORDS
            // ═══════════════════════════════════════════════════════════════════
            {
                var popHdrPnl = new Panel { Location=new Point(c2, yM), Size=new Size(cW2, 20), BackColor=_panelBg };
                popHdrPnl.Controls.Add(new Label {
                    Text=T("POPUP WORDS  (Popup / Both)", "POPUP-WÖRTER  (Popup / Beides)"),
                    Location=new Point(8,2), AutoSize=true,
                    ForeColor=_secTxt, Font=new Font("Segoe UI",8.5f,FontStyle.Bold) });
                Controls.Add(popHdrPnl);
                lblPopupHeader = popHdrPnl.Controls[0] as Label;
                _popupControls.Add(popHdrPnl);
            }
            yM += 26;

            _popupControls.Add(DLbl(T("Color:","Farbe:"), c2, yM+5, 50));
            btnPopupColor = ColBtn(T("Popup Color","Popup-Farbe"), cur.PopupColor, c2+54, yM, 148);
            btnPopupColor.Click += delegate { Pick(ref cur.PopupColor, btnPopupColor); };
            _popupControls.Add(btnPopupColor);
            yM += 32;

            // Single-select effect buttons (Fade / Glitch / Scan / Zoom / Scramble)
            _popupControls.Add(DLbl(T("Effect:","Effekt:"), c2, yM+5));
            yM += 22;
            string[] fxNames = new string[]{ "Fade", "Glitch", "Scan", "Zoom", "Scramble" };
            btnFxEffects = new Button[fxNames.Length];
            const int FX_W = 74, FX_GAP = 4;
            for (int fi = 0; fi < fxNames.Length; fi++)
            {
                string capturedName = fxNames[fi];
                var fxBtn = new Button {
                    Text      = capturedName,
                    Location  = new Point(c2 + fi * (FX_W + FX_GAP), yM),
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
            yM += 32;

            trkPopupFont = SlRow(T("Font Size","Schriftgröße"), c2,yM,cW2, 10,72, cur.PopupFontSize, out lblPFont);
            lblPFont.Text = cur.PopupFontSize+" px";
            trkPopupFont.ValueChanged += delegate { cur.PopupFontSize=trkPopupFont.Value; lblPFont.Text=cur.PopupFontSize+" px"; };
            _popupControls.Add(trkPopupFont); _popupControls.Add(lblPFont);
            yM += SL;

            trkPopupCount = SlRow(T("Simultaneous","Gleichzeitig"), c2,yM,cW2, 1,20, cur.PopupCount, out lblPCount);
            lblPCount.Text = cur.PopupCount.ToString();
            trkPopupCount.ValueChanged += delegate { cur.PopupCount=trkPopupCount.Value; lblPCount.Text=cur.PopupCount.ToString(); };
            _popupControls.Add(trkPopupCount); _popupControls.Add(lblPCount);
            yM += SL;

            trkPopupSpeed = SlRow(T("Popup Speed","Popup-Geschwindigkeit"), c2,yM,cW2, 1,30, (int)(cur.PopupSpeedFactor*10), out lblPopupSpeed);
            lblPopupSpeed.Text = cur.PopupSpeedFactor.ToString("F1")+"x";
            trkPopupSpeed.ValueChanged += delegate { cur.PopupSpeedFactor=trkPopupSpeed.Value/10f; lblPopupSpeed.Text=cur.PopupSpeedFactor.ToString("F1")+"x"; };
            _popupControls.Add(trkPopupSpeed); _popupControls.Add(lblPopupSpeed);
            yM += SL;

            yM += 12;

            // ═══════════════════════════════════════════════════════════════════
            // MIDDLE COLUMN — GENERAL
            // ═══════════════════════════════════════════════════════════════════
            HSep(yM, div2, div3-div2+1); yM += 12;
            Section(T("GENERAL","ALLGEMEIN"), c2, yM, cW2); yM += 26;
            chkScanlines = Chk("CRT Scanlines",   cur.ShowScanlines, c2,     yM);
            chkWatermark = Chk(T("Watermark","Wasserzeichen"), cur.ShowWatermark, c2+130, yM);
            chkVeeam100  = Chk(T("Veeam 100 Names","Veeam 100 Namen"), cur.ShowVeeam100, c2+260, yM);
            // Live-sync general checkboxes → cur
            chkScanlines.CheckedChanged += delegate { cur.ShowScanlines = chkScanlines.Checked; };
            chkWatermark.CheckedChanged += delegate { cur.ShowWatermark = chkWatermark.Checked; };
            chkVeeam100.CheckedChanged  += delegate { cur.ShowVeeam100  = chkVeeam100.Checked;  };
            yM += 28;

            chkBuiltinTerms = Chk(T("Built-in terms","Eingebaut. Begriffe"), cur.UseBuiltinTerms, c2, yM);
            chkBuiltinTerms.CheckedChanged += delegate { cur.UseBuiltinTerms = chkBuiltinTerms.Checked; };
            var btnCatalog = new Button {
                Text=T("Catalog…","Katalog…"), Location=new Point(c2+190, yM-1), Size=new Size(86,26),
                FlatStyle=FlatStyle.Flat, BackColor=Color.FromArgb(0,55,18), ForeColor=Color.White };
            btnCatalog.FlatAppearance.BorderColor = Color.FromArgb(0,100,32);
            btnCatalog.Click += delegate { ShowTermsCatalog(); };
            Controls.Add(btnCatalog);
            yM += 30;

            // Watermark text – own row
            DLbl(T("Watermark:", "Wasserzeichen:"), c2, yM+5, 96);
            txtWatermark = new TextBox { Location=new Point(c2+100, yM), Size=new Size(cW2-104, 24),
                Text=cur.WatermarkText, BackColor=_inputBg,
                ForeColor=_inputFg, BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtWatermark);
            yM += 30;

            // Subtitle text – two-line multiline box; | acts as line-break in the watermark
            DLbl(T("Subtitle:  ( | = line break in watermark)",
                   "Untertitel:  ( | = Zeilenumbruch im Wasserzeichen)"), c2, yM+5);
            yM += 22;
            txtWatermarkSub = new TextBox {
                Location    = new Point(c2, yM),
                Size        = new Size(cW2-4, 42),
                Text        = cur.WatermarkSubText,
                BackColor   = _inputBg,
                ForeColor   = _inputFg,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline   = true,
                WordWrap    = true,
                ScrollBars  = ScrollBars.None
            };
            Controls.Add(txtWatermarkSub);
            yM += 48;

            DLbl(T("Custom terms (comma / | / newline separated):","Eigene Begriffe (Komma / | / Zeilenumbruch):"), c2, yM+5);
            yM += 22;
            txtExtra = new TextBox { Location=new Point(c2, yM), Size=new Size(cW2-4, 24),
                Text=cur.ExtraWords, BackColor=_inputBg,
                ForeColor=_inputFg, BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtExtra);
            yM += 30;

            // ── Sequential / random + Star Wars preset ────────────────────────
            chkOrderedTerms = Chk(T("Sequential order (no random)","Sequenziell (kein Zufall)"),
                                  cur.OrderedTerms, c2, yM);
            chkOrderedTerms.CheckedChanged += delegate { cur.OrderedTerms = chkOrderedTerms.Checked; };
            Controls.Add(chkOrderedTerms);
            // Star Wars / Spaceballs text preset
            var btnSwPreset = new Button {
                Text = T("★ Star Wars Text","★ Star Wars Text"),
                Location = new Point(c2 + cW2/2, yM - 1), Size = new Size(cW2/2 - 4, 24),
                BackColor = Color.FromArgb(10,10,80), ForeColor = Color.FromArgb(255,232,31),
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8f, FontStyle.Bold)
            };
            btnSwPreset.FlatAppearance.BorderColor = Color.FromArgb(100,100,180);
            btnSwPreset.Click += delegate {
                string swText =
                    "A LONG TIME AGO|IN A GALAXY|FAR FAR AWAY|" +
                    "A RUTHLESS RACE|KNOWN AS|SPACEBALLS|" +
                    "HAVING FOOLISHLY|SQUANDERED|THEIR OWN ATMOSPHERE|" +
                    "THESE MONGRELS|NOW PLOT TO STEAL|THE AIR FROM|" +
                    "THEIR PEACEFUL|NEIGHBORS|" +
                    "DRUIDIA|LONE STARR|VS DARK HELMET|" +
                    "MAY THE SCHWARTZ|BE WITH YOU|" +
                    "TODAY'S STORY|BEGINS NOW";
                if (txtExtra != null) txtExtra.Text = swText;
                if (chkOrderedTerms != null) { chkOrderedTerms.Checked = true; cur.OrderedTerms = true; }
                SetWordStyle("Crawl", applyStyleDefaults: true);
                MarkDirty();
            };
            Controls.Add(btnSwPreset);
            yM += 30;

            // ═══════════════════════════════════════════════════════════════════
            // RIGHT COLUMN — LIVE PREVIEW  +  BACKUP OPERATIONS  +  CHANGE LOG
            // ═══════════════════════════════════════════════════════════════════
            Section(T("LIVE PREVIEW","LIVE-VORSCHAU"), c3, yR, cW3); yR += 26;
            picPreview = new PictureBox {
                Location    = new Point(c3, yR),
                Size        = new Size(PREV_W + 2, PREV_H + 2),   // +2 for FixedSingle border
                BackColor   = Color.FromArgb(4, 4, 4),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(picPreview);
            picPreview.Paint += delegate(object ps, PaintEventArgs pe)
            {
                if (_prevEngine != null) _prevEngine.Render(pe.Graphics);
            };
            yR += PREV_H + 2 + 10;

            // ── BACKUP OPERATIONS (Easter-egg) — right column ─────────────────
            HSep(yR, div3, fw-div3-14); yR += 12;
            Section(T("BACKUP OPERATIONS","BACKUP-OPERATIONEN"), c3, yR, cW3); yR += 26;
            {
                string[] btnLabels = new string[] {
                    T("Configuration Backup", "Konfigurations-Backup"),
                    T("License",              "Lizenz")
                };
                string[] btnMsgs = new string[] {
                    T("For that you should use\nVeeam Backup & Replication!",
                      "Dafuer solltest du besser\nVeeam Backup & Replication verwenden!"),
                    T("You already have the Premium features.",
                      "Du hast bereits die Premium-Features.")
                };
                int ebw3 = (cW3 - 8) / 2;
                for (int ei = 0; ei < btnLabels.Length; ei++)
                {
                    var eb = new Button {
                        Text      = btnLabels[ei],
                        Location  = new Point(c3 + ei * (ebw3 + 8), yR),
                        Size      = new Size(ebw3, 28),
                        BackColor = Color.FromArgb(0, 75, 22),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    eb.FlatAppearance.BorderColor = Color.FromArgb(0, 170, 55);
                    string capturedMsg = btnMsgs[ei];
                    eb.Click += delegate {
                        MessageBox.Show(capturedMsg, "VeeaMatrix",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    };
                    Controls.Add(eb);
                }
                yR += 36;
            }

            // ── CHANGE LOG — right column ─────────────────────────────────────
            HSep(yR, div3, fw-div3-14); yR += 12;
            Section(T("CHANGE LOG","ÄNDERUNGSPROTOKOLL"), c3, yR, cW3); yR += 26;
            {
                string changelog =
                    "v1.46  Crawl ×3 scale, horizon at 15%, t^0.65 power curve, no-jitter fade; separator geometry fix\r\n" +
                    "v1.45  Crawl horizon perspective (HORIZON_FRAC 0.35); star field option; BACKUP+CHANGELOG → right col\r\n" +
                    "v1.44  Crawl: WordCount controls queue depth; simultaneous slider restored; gap 2.5×fs\r\n" +
                    "v1.43  Crawl: first word enters from screen bottom immediately; HSep between WORD/POPUP; banner height fix\r\n" +
                    "v1.42  Crawl: enforce 1 word at a time (true Star Wars queue); subtitle partial-value migration + auto-save\r\n" +
                    "v1.41  Crawl font scale 1.2→1.5 (+50 %); spawn gap updated for new scale\r\n" +
                    "v1.40  Restore 3-column layout; preview scaled to 75 % (660×371); banner back to right column\r\n" +
                    "v1.39  Layout refactor: 2-column 1414 px form; preview removed temporarily\r\n" +
                    "v1.38  Banner fit-to-width (letterbox); subtitle auto-migration; Crawl trail artefact fix; Clone missing field fix\r\n" +
                    "v1.37  Crawl: all words same speed (no overtaking); 'Disable RAIN' option; cinematic image bars on banner\r\n" +
                    "v1.36  Crawl: fixed word overlap, corrected color gradient, font 20 % larger\r\n" +
                    "v1.35  Crawl word style (Star Wars perspective scroll); Star Wars color profile; Glitch as default style\r\n" +
                    "v1.34  Subtitle field: multiline TextBox + pipe | line-break hint label\r\n" +
                    "v1.33  Settings UI: improvements to layout, DarkMode, profile system, language switch DE/EN\r\n" +
                    "v1.32  Easter-egg BACKUP OPERATIONS section in settings (Config Backup, License buttons)\r\n" +
                    "v1.31  Reset to Default button in settings dialog\r\n" +
                    "v1.30  Default settings updated to match intended visual\r\n" +
                    "v1.29  Collision avoidance for word drops and popups (20-retry bounding-box check)\r\n" +
                    "v1.28  Aurora color profile; banner hardcoded into binary; content isolation; blog post\r\n" +
                    "v1.25  Replace Neon Tokyo with Deep Space color profile\r\n" +
                    "v1.24  Wider banner column; watermark font style respect; Neon Tokyo profile\r\n" +
                    "v1.23  Expanded term catalog; font Bold/Italic style option\r\n" +
                    "v1.22  Restore form width; Light/Dark theme toggle; banner embedded into .scr at build time\r\n" +
                    "v1.21  Direction filter; remove-profile button; term catalog dialog; form width −15 %; banner left\r\n" +
                    "v1.20  Fix direction control; banner aspect-crop; UI readability improvements\r\n" +
                    "v1.19  Move banner to preview column with fill-crop rendering\r\n" +
                    "v1.18  App icon for taskbar; banner image support (external file)\r\n" +
                    "v1.17  Button-style word-style selector; Glitch word style; Scramble popup effect; direction lock\r\n" +
                    "v1.16  Remove Blink style and Flash popup effect; single-select popup effect buttons\r\n" +
                    "v1.15  Grey out inactive layer controls based on Word Mode\r\n" +
                    "v1.14  Direction in Build/Scramble; real line-height measurement; clearer UI labels\r\n" +
                    "v1.13  Per-char spacing fix for proportional fonts; direction head-char; hover tooltips; Scramble/Blink styles\r\n" +
                    "v1.12  Fix: live-sync all controls to cur; complete RebuildPreview coverage\r\n" +
                    "v1.11  Popup speed control; correct popup font; 960×540 (16:9) preview size\r\n" +
                    "v1.10  Rename to VeeaMatrix; 16:9 preview; popup in WORDS section; EN/DE toggle; wider subtitle box\r\n" +
                    "v1.9   Live preview panel in config dialog; modern 2-column UI; custom font preview text; system font picker\r\n" +
                    "v1.8   Veeam font; word speed slider; custom watermark text\r\n" +
                    "v1.7   Update Veeam 100 People list from official 2026 directories\r\n" +
                    "v1.6   Static word styles: Fade + Build\r\n" +
                    "v1.5   Word direction control; Veeam 100 Names toggle\r\n" +
                    "v1.4   Selective effects per layer; multi-monitor fix; color profiles (Veeam Green, Matrix, Amber)\r\n" +
                    "v1.3   Separate colors for rain, falling words, and popups\r\n" +
                    "v1.2   Popup word effects: Fade / Flash / Glitch / Scan / Zoom / Mixed\r\n" +
                    "v1.0   Initial release — Matrix rain + word drops + popups";
                var txtLog = new TextBox {
                    Location    = new Point(c3, yR),
                    Size        = new Size(cW3, 240),
                    Text        = changelog,
                    BackColor   = _dark ? Color.FromArgb(4,10,4) : Color.FromArgb(235,245,235),
                    ForeColor   = _dark ? Color.FromArgb(0,190,55) : Color.FromArgb(0,120,35),
                    BorderStyle = BorderStyle.FixedSingle,
                    Multiline   = true,
                    ReadOnly    = true,
                    ScrollBars  = ScrollBars.Vertical,
                    Font        = new Font("Consolas", 8f),
                    WordWrap    = false
                };
                Controls.Add(txtLog);
                yR += 246;
            }

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
            int yBot = Math.Max(yL, Math.Max(yM, yR)) + 14;
            HSep(yBot, 14, fw-28); yBot += 12;

            // ── Vertical dividers: span exactly from top separator to bottom separator ──
            int sepTop = y - 12;        // top HSep was drawn here
            int sepBot = yBot - 12;     // bottom HSep was drawn here (before yBot += 12)
            int divH   = sepBot - sepTop + 1;
            Controls.Add(new Panel { Location=new Point(div2, sepTop), Size=new Size(1, divH), BackColor=_sep });
            Controls.Add(new Panel { Location=new Point(div3, sepTop), Size=new Size(1, divH), BackColor=_sep });

            int bRight = c3 + cW3;  // = 1602
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
                if (chkBuiltinTerms != null) cur.UseBuiltinTerms = chkBuiltinTerms.Checked;
                cur.WatermarkText    = txtWatermark.Text.Trim();
                cur.WatermarkSubText = txtWatermarkSub.Text.Trim();
                cur.ExtraWords       = txtExtra.Text.Trim();
                cur.Language         = cboLanguage  != null ? cboLanguage.Text  : cur.Language;
                if (trkPopupSpeed != null) cur.PopupSpeedFactor = trkPopupSpeed.Value / 10f;
                if (cboWordFontName.SelectedItem != null) cur.WordFontName = cboWordFontName.SelectedItem.ToString();
                if (chkWordFontBold   != null) cur.WordFontBold   = chkWordFontBold.Checked;
                if (chkWordFontItalic != null) cur.WordFontItalic = chkWordFontItalic.Checked;
                if (chkCrawlHideRain  != null) cur.CrawlHideRain  = chkCrawlHideRain.Checked;
                if (chkCrawlStarfield != null) cur.CrawlStarfield = chkCrawlStarfield.Checked;
                if (chkOrderedTerms   != null) cur.OrderedTerms   = chkOrderedTerms.Checked;
                // cur.PopupEffects already in sync via SetPopupEffect()
                Result=cur; Result.Save();
            };
            var btnCancel = new Button { Text=T("Cancel","Abbrechen"),
                Location=new Point(bRight-118, yBot), Size=new Size(118,32),
                DialogResult=DialogResult.Cancel,
                BackColor=Color.FromArgb(50,15,15), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(130,36,36);

            // Reset to Default — preserves UI prefs (Language, DarkMode), resets everything else
            var btnReset = new Button { Text=T("Reset to Default","Standard wiederherstellen"),
                Location=new Point(bRight-404, yBot), Size=new Size(166,32),
                BackColor=Color.FromArgb(70,50,0), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(180,120,0);
            btnReset.Click += delegate
            {
                string lang = cur.Language;
                bool   dark = cur.DarkMode;
                cur          = new Settings();
                cur.Language = lang;
                cur.DarkMode = dark;
                RebuildUI();
            };

            Controls.Add(btnReset); Controls.Add(btnOK); Controls.Add(btnCancel);
            AcceptButton=btnOK; CancelButton=btnCancel;

            // ── Banner image — left column bottom (below RAIN section) ──────────
            int finalH = yBot + 48;
            try
            {
                var bannerImg = LoadBannerImage();
                int bannerY   = yL + 8;
                int bannerH   = (yBot - 12) - bannerY - 8;  // yBot-12 = separator position (before += 12)
                if (bannerImg != null && bannerH > 50)
                {
                    Image capturedBanner = bannerImg;
                    var picBanner = new PictureBox {
                        Location    = new Point(c1, bannerY),
                        Size        = new Size(cW1, bannerH),
                        BackColor   = Color.Black,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    // Fit-to-width: show FULL image (no cropping), black bars top/bottom.
                    picBanner.Paint += delegate(object bps, PaintEventArgs bpe)
                    {
                        if (capturedBanner == null || capturedBanner.Width == 0) return;
                        var pb   = (PictureBox)bps;
                        double srcAR = (double)capturedBanner.Width / capturedBanner.Height;
                        int dw = pb.Width;
                        int dh = (int)(dw / srcAR);
                        if (dh > pb.Height) { dh = pb.Height; dw = (int)(dh * srcAR); }
                        int dx = (pb.Width  - dw) / 2;
                        int dy = (pb.Height - dh) / 2;
                        bpe.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        bpe.Graphics.DrawImage(capturedBanner,
                            new Rectangle(dx, dy, dw, dh),
                            new Rectangle(0, 0, capturedBanner.Width, capturedBanner.Height), GraphicsUnit.Pixel);
                    };
                    Controls.Add(picBanner);
                }
                else if (bannerH > 50)
                {
                    // No banner found — show an instructional placeholder in the left column
                    bool dm = cur.DarkMode;
                    var picPlaceholder = new Panel {
                        Location    = new Point(c1, bannerY),
                        Size        = new Size(cW1, bannerH),
                        BackColor   = dm ? Color.FromArgb(8, 14, 8) : Color.FromArgb(230, 240, 230),
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    var lblNoImg = new Label {
                        Text        = "No banner image\n\nTo add one:\nPlace VeeaMatrix-banner.jpg\nnext to the .scr or .ps1\nand rebuild with Build-VeeaMatrix.ps1",
                        TextAlign   = ContentAlignment.MiddleCenter,
                        Dock        = DockStyle.Fill,
                        ForeColor   = dm ? Color.FromArgb(60, 100, 60) : Color.FromArgb(100, 140, 100),
                        Font        = new Font("Segoe UI", 8.5f)
                    };
                    picPlaceholder.Controls.Add(lblNoImg);
                    Controls.Add(picPlaceholder);
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
            tip(cboWordFontName,    "Font used for keyword streams, popups and watermark",                    "Schriftart für Keyword-Streams, Popups und Wasserzeichen");
            tip(txtFontPreviewText, "Edit the sample text shown in the font preview box",                    "Vorschautext für die Schriftart-Vorschau ändern");
            tip(chkWordFontBold,    "Render keyword streams and popups in bold weight",                      "Keyword-Streams und Popups fett darstellen");
            tip(chkWordFontItalic,  "Render keyword streams and popups in italic",                           "Keyword-Streams und Popups kursiv darstellen");
            if (btnWordStyles != null && btnWordStyles.Length == 6)
            {
                tip(btnWordStyles[0], "Scroll — keyword scrolls across the screen",                         "Scroll — Keyword scrollt über den Bildschirm");
                tip(btnWordStyles[1], "Fade — keyword fades in and out in place",                           "Fade — Keyword blendet an Ort und Stelle ein/aus");
                tip(btnWordStyles[2], "Build — chars decode left-to-right",                                 "Build — Zeichen werden von links nach rechts eingeblendet");
                tip(btnWordStyles[3], "Scramble — noise resolves to the correct word sequentially",         "Scramble — Rauschen löst sich sequenziell auf");
                tip(btnWordStyles[4], "Glitch — word appears through noise that gradually clears",          "Glitch — Wort erscheint durch Rauschen, das sich auflöst");
                tip(btnWordStyles[5], "Crawl — Star Wars-style perspective scroll; words queue up from the bottom one after another (Simultaneous = queue depth)",  "Crawl — Star-Wars-Stil: Wörter scrollen nacheinander von unten nach oben (Gleichzeitig = Queue-Tiefe)");
            }
            tip(cboWordOrient,   "Scroll: all 4 directions · Build/Scramble/Glitch: LeftRight or RightLeft only · Fade: disabled",  "Scroll: alle 4 Richtungen · Build/Scramble/Glitch: nur Links↔Rechts · Fade: deaktiviert");
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
            FontStyle pfs = FontStyle.Regular;
            if (chkWordFontBold   != null && chkWordFontBold.Checked)   pfs |= FontStyle.Bold;
            if (chkWordFontItalic != null && chkWordFontItalic.Checked) pfs |= FontStyle.Italic;
            int pw = picFontPreview.Width-2, ph = picFontPreview.Height-2;
            var bmp = new Bitmap(pw, ph);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(8,8,8));
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                try
                {
                    float fs = ph * 0.56f;
                    using (Font f = new Font(fname, fs, pfs, GraphicsUnit.Pixel))
                    {
                        SizeF sz = g.MeasureString(sample, f);
                        if (sz.Width > pw-8)
                        {
                            fs = fs * (pw-8) / sz.Width;
                            f.Dispose();
                            using (Font f2 = new Font(fname, fs, pfs, GraphicsUnit.Pixel))
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
                b.BackColor = active ? Color.FromArgb(0,100,28) : _btnIna;
                b.ForeColor = active ? Color.White               : _btnInaFg;
                b.FlatAppearance.BorderColor = active ? Color.FromArgb(0,185,55) : _btnInaBdr;
            }
        }

        // Single-select: highlight chosen word style button and update cur.WordStyle
        private bool _settingStyle = false;  // guard: suppress Crawl-defaults during init
        private void SetWordStyle(string name, bool applyStyleDefaults = false)
        {
            if (btnWordStyles == null) return;
            string[] valid = new string[]{ "Scroll", "Fade", "Build", "Scramble", "Glitch", "Crawl" };
            bool found = false;
            foreach (string n in valid) if (n == name) { found = true; break; }
            if (!found) name = "Glitch";
            cur.WordStyle = name;
            foreach (Button b in btnWordStyles)
            {
                bool active = ((string)b.Tag == name);
                b.BackColor = active ? Color.FromArgb(0,100,28) : _btnIna;
                b.ForeColor = active ? Color.White               : _btnInaFg;
                b.FlatAppearance.BorderColor = active ? Color.FromArgb(0,185,55) : _btnInaBdr;
            }
            // Apply Crawl-specific defaults when user switches to Crawl
            if (applyStyleDefaults && name == "Crawl")
            {
                if (trkWordCount != null) { trkWordCount.Value = 30; cur.WordCount = 30; if (lblWCount != null) lblWCount.Text = "30"; }
                if (trkWordFont  != null) { trkWordFont.Value  = 36; cur.WordFontSize = 36; if (lblWFont != null) lblWFont.Text = "36 px"; }
                if (trkWordSpeed != null) { trkWordSpeed.Value = 20; cur.WordSpeedFactor = 2.0f; if (lblWordSpeed != null) lblWordSpeed.Text = "2.0x"; }
            }
            SyncWordStyleDirection();
        }

        // Direction rules:
        //   Fade/Glitch/Scramble/Crawl → direction hidden (not applicable)
        //   Build                       → horizontal only (LeftRight/RightLeft)
        //   Scroll                      → all five options
        private void SyncWordStyleDirection()
        {
            bool isFade  = (cur.WordStyle == "Fade");
            bool hideDir = (cur.WordStyle == "Glitch" || cur.WordStyle == "Scramble" ||
                            cur.WordStyle == "Crawl"  || cur.WordStyle == "Fade");
            string mode = cboWordMode != null ? cboWordMode.Text : cur.WordMode;
            bool streamActive = (mode == "Rain" || mode == "Both");
            if (cboWordOrient     != null) { cboWordOrient.Visible     = !hideDir; cboWordOrient.Enabled     = !hideDir && streamActive; }
            if (_lblWordOrient   != null) { _lblWordOrient.Visible   = !hideDir; _lblWordOrient.Enabled   = !hideDir && streamActive; }
            // CrawlHideRain checkbox: only relevant when Crawl style is active
            bool isCrawl = (cur.WordStyle == "Crawl");
            if (chkCrawlHideRain  != null) { chkCrawlHideRain.Visible  = isCrawl; }
            if (chkCrawlStarfield != null) { chkCrawlStarfield.Visible = isCrawl; }
            // Crawl uses WordCount as queue depth — slider stays visible for all styles

            // Rebuild orientation options when style switches between horizontal-only and all-directions
            if (cboWordOrient != null && streamActive && !isFade)
            {
                bool hOnly = (cur.WordStyle == "Build");
                string[] allOrients = new string[]{ "Same", "TopDown", "BottomUp", "LeftRight", "RightLeft" };
                string[] hOrients   = new string[]{ "Same", "LeftRight", "RightLeft" };
                string[] want = hOnly ? hOrients : allOrients;
                bool needRebuild = (cboWordOrient.Items.Count != want.Length);
                if (!needRebuild)
                    for (int i = 0; i < want.Length; i++)
                        if ((string)cboWordOrient.Items[i] != want[i]) { needRebuild = true; break; }
                if (needRebuild)
                {
                    string prev = cboWordOrient.Text;
                    _syncingOrient = true;
                    cboWordOrient.Items.Clear();
                    foreach (string o in want) cboWordOrient.Items.Add(o);
                    _syncingOrient = false;
                    bool kept = false;
                    foreach (string o in want) if (o == prev) { cboWordOrient.Text = prev; kept = true; break; }
                    if (!kept) cboWordOrient.Text = "LeftRight";
                    if (cboWordOrient.SelectedIndex >= 0) cur.WordOrientation = cboWordOrient.Text;
                }
            }
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
            if (chkWordFontBold   != null) s.WordFontBold   = chkWordFontBold.Checked;
            if (chkWordFontItalic != null) s.WordFontItalic = chkWordFontItalic.Checked;
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
            if (chkVeeam100      != null) s.ShowVeeam100   = chkVeeam100.Checked;
            if (chkOrderedTerms  != null) s.OrderedTerms   = chkOrderedTerms.Checked;
            if (chkCrawlHideRain != null) s.CrawlHideRain  = chkCrawlHideRain.Checked;
            if (chkCrawlStarfield!= null) s.CrawlStarfield = chkCrawlStarfield.Checked;

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

        // Opens the term catalog editor — shows built-in or custom terms.txt one per line,
        // saves to %APPDATA%\VeeaMatrix\terms.txt on OK.
        private void ShowTermsCatalog()
        {
            string termsDir  = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VeeaMatrix");
            string termsFile = Path.Combine(termsDir, "terms.txt");
            string initial;
            bool hasCustom = File.Exists(termsFile);
            if (hasCustom)
            {
                try   { initial = File.ReadAllText(termsFile, Encoding.UTF8); }
                catch { initial = string.Join(Environment.NewLine, MatrixEngine.TERMS); }
            }
            else { initial = string.Join(Environment.NewLine, MatrixEngine.TERMS); }

            var dlg = new Form {
                Text = T("Term Catalog  –  one term per line", "Wort-Katalog  –  ein Begriff pro Zeile"),
                Size = new Size(680, 640),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.FromArgb(34, 36, 34),
                ForeColor = Color.FromArgb(0, 200, 55)
            };
            var lblInfo = new Label {
                Text = T("Edit the term list. Each line = one term. Lines starting with # are ignored.",
                         "Begriffsliste bearbeiten. Jede Zeile = ein Begriff. Zeilen mit # werden ignoriert."),
                Location = new Point(10, 8), Size = new Size(650, 18),
                ForeColor = Color.FromArgb(160,160,160), Font = new Font("Segoe UI", 8.5f)
            };
            var txt = new TextBox {
                Multiline = true, ScrollBars = ScrollBars.Vertical,
                Location = new Point(10, 30), Size = new Size(648, 510),
                Text = initial,
                BackColor = Color.FromArgb(44, 46, 44),
                ForeColor = Color.FromArgb(0, 210, 60),
                Font = new Font("Courier New", 8.5f),
                BorderStyle = BorderStyle.FixedSingle,
                WordWrap = false
            };
            var btnReset = new Button {
                Text = T("Reset to defaults","Auf Standard zurücksetzen"),
                Location = new Point(10, 548), Size = new Size(186, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 35, 0), ForeColor = Color.White
            };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(140, 90, 0);
            btnReset.Click += delegate { txt.Text = string.Join(Environment.NewLine, MatrixEngine.TERMS); };

            var btnOK = new Button {
                Text = "OK", Location = new Point(482, 548), Size = new Size(80, 30),
                DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 28), ForeColor = Color.White
            };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(0, 185, 55);
            var btnCancel = new Button {
                Text = T("Cancel","Abbrechen"), Location = new Point(572, 548), Size = new Size(86, 30),
                DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 15, 15), ForeColor = Color.White
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(130, 36, 36);

            dlg.Controls.Add(lblInfo); dlg.Controls.Add(txt);
            dlg.Controls.Add(btnReset); dlg.Controls.Add(btnOK); dlg.Controls.Add(btnCancel);
            dlg.AcceptButton = btnOK; dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                string content = txt.Text.Trim();
                if (content.Length > 0)
                {
                    try { Directory.CreateDirectory(termsDir); File.WriteAllText(termsFile, content, Encoding.UTF8); }
                    catch { }
                }
                else if (hasCustom)
                {
                    try { File.Delete(termsFile); } catch { }
                }
                MarkDirty();
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
