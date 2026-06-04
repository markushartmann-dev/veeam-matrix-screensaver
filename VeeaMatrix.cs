// VeeaMatrix.cs  –  Windows Screensaver v1.70
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
        public string WordMode      = "Rain";
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
        public bool   PopupHideRain    = false;   // suppress background rain while Popup is active
        public bool   CrawlStarfield   = false;   // draw star field behind Crawl words
        public bool   OrderedTerms     = false;   // use terms in sequential order, no random
        public string CrawlText        = "";      // text used exclusively by the Crawl effect (independent of ExtraWords)
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
        public bool   DarkMode        = false;

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
            sb.AppendLine("PopupHideRain="    + PopupHideRain);
            sb.AppendLine("CrawlStarfield="   + CrawlStarfield);
            sb.AppendLine("OrderedTerms="     + OrderedTerms);
            sb.AppendLine("CrawlText="        + CrawlText);
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
                        case "PopupHideRain":    s.PopupHideRain    = bool.Parse(v); break;
                        case "CrawlStarfield":   s.CrawlStarfield   = bool.Parse(v); break;
                        case "OrderedTerms":     s.OrderedTerms     = bool.Parse(v); break;
                        case "CrawlText":        s.CrawlText        = v;            break;
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
            // v1.56 migration: "Both" removed — map to "Rain"
            if (s.WordMode == "Both") s.WordMode = "Rain";
            // v1.60 migration: ExtraWords that starts with crawl-style phrases was set by mistake — clear it
            // (CRAWL uses CrawlText exclusively; ExtraWords is only for WORD STREAM / POPUP custom terms)
            if (!string.IsNullOrEmpty(s.ExtraWords))
            {
                string ew = s.ExtraWords.ToUpper();
                if (ew.StartsWith("A LONG TIME AGO") || ew.StartsWith("EPISODE") || ew.StartsWith("IT IS A PERIOD"))
                    s.ExtraWords = "";
            }
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
        private int     _termIndex   = 0;    // sequential term counter (used when OrderedTerms=true)
        private float    _crawlScroll = 0f;    // continuous scroll position for the perspective crawl
        private string[] _crawlTerms = null;  // terms used exclusively by Crawl (from s.CrawlText)
        private string[] _crawlIntro = null;  // intro lines shown statically before the crawl starts
        private int      _crawlIntroTick = 0; // tick counter for the intro phase (0 = just starting)

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

            _crawlScroll    = 0f;
            _crawlIntroTick = 0;
            // Build Crawl-exclusive term list — preserve blank lines (paragraph separators)
            {
                var ct = new List<string>();
                if (!string.IsNullOrEmpty(s.CrawlText))
                {
                    foreach (string p in s.CrawlText.Split('|'))
                    {
                        string t = p.Trim().ToUpper();
                        ct.Add(t);  // empty string = blank line / paragraph break
                    }
                    // Trim leading/trailing blank lines
                    while (ct.Count > 0 && ct[0]  == "") ct.RemoveAt(0);
                    while (ct.Count > 0 && ct[ct.Count-1] == "") ct.RemoveAt(ct.Count-1);
                }
                // Detect intro: first paragraph (lines before first blank line) shown as static intro
                var introLines = new List<string>();
                int bodyStart  = 0;
                for (int ii = 0; ii < ct.Count; ii++)
                {
                    if (ct[ii] == "") { bodyStart = ii + 1; break; }  // blank line = end of intro
                    introLines.Add(ct[ii]);
                    if (ii == ct.Count - 1) bodyStart = ct.Count;    // no blank → all is intro (no body)
                }
                // Only use intro if there IS a body after it
                if (bodyStart < ct.Count && introLines.Count > 0)
                {
                    _crawlIntro = introLines.ToArray();
                    _crawlTerms = ct.GetRange(bodyStart, ct.Count - bodyStart).ToArray();
                }
                else
                {
                    _crawlIntro = null;  // no intro
                    _crawlTerms = ct.ToArray();
                }
            }
            wdrops.Clear();
            // Crawl uses the perspective engine (DrawCrawlPerspective) — no wdrops needed
            if (s.WordMode == "Rain" && allTerms.Length > 0
                && s.WordStyle != "Crawl")
            {
                for (int i = 0; i < s.WordCount; i++) wdrops.Add(SpawnDrop(true));
            }

            popups.Clear();
            if (s.WordMode == "Popup" && allTerms.Length > 0)
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

        // ── Perspective Crawl — matches CSS rotateX+perspective from veeam_star_wars_crawl.html ───
        // ── Star Wars–style intro: lines shown statically centered, then perspective crawl begins ──
        private const int CRAWL_INTRO_TICKS = 160;  // ~5-6 s at 25-30 fps: 50 fade-in + 60 hold + 50 fade-out

        private void DrawCrawlPerspective()
        {
            string[] terms = _crawlTerms;
            if ((terms == null || terms.Length == 0) && _crawlIntro == null) return;

            // ── Perspective parameters ───────────────────────────────────────────
            const float TILT_DEG = 25f;
            float tilt  = TILT_DEG * (float)Math.PI / 180f;
            float cosT  = (float)Math.Cos(tilt);
            float sinT  = (float)Math.Sin(tilt);
            float focal = H * 0.55f;

            float baseSize = Math.Max(10f, s.WordFontSize * 4.0f);
            float lineH    = baseSize * 1.75f;
            float paraH    = baseSize * 1.50f;   // EXTRA space added for blank paragraph separators
            float stageW   = W * 0.74f;
            FontStyle crawlFs = FontStyle.Bold | FontStyle.Italic;

            // ── Intro phase ──────────────────────────────────────────────────────
            bool hasIntro   = (_crawlIntro != null && _crawlIntro.Length > 0);
            bool introActive = hasIntro && _crawlIntroTick < CRAWL_INTRO_TICKS;
            if (introActive)
            {
                _crawlIntroTick++;
                float t = (float)_crawlIntroTick / CRAWL_INTRO_TICKS;
                float alpha;
                if (t < 0.32f)      alpha = t / 0.32f;                   // fade in
                else if (t < 0.68f) alpha = 1f;                           // hold
                else                alpha = (1f - t) / 0.32f;            // fade out
                alpha = Math.Max(0f, Math.Min(1f, alpha));

                var prevHint2 = bg.TextRenderingHint;
                bg.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                float introFontSize = Math.Max(10f, s.WordFontSize * 1.1f);
                float introLineH    = introFontSize * 1.6f;
                float totalIntroH   = _crawlIntro.Length * introLineH;
                float introY        = (H - totalIntroH) / 2f;
                int ca = Clamp((int)(alpha * 255));
                using (Font iFont = new Font(s.WordFontName, introFontSize, FontStyle.Italic, GraphicsUnit.Point))
                {
                    foreach (string line in _crawlIntro)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            SizeF sz = bg.MeasureString(line, iFont);
                            tmpBrush.Color = Color.FromArgb(ca, s.WordHeadColor);
                            bg.DrawString(line, iFont, tmpBrush, (W - sz.Width) / 2f, introY);
                        }
                        introY += introLineH;
                    }
                }
                bg.TextRenderingHint = prevHint2;
                return;  // intro active — don't render the crawl yet
            }

            if (terms == null || terms.Length == 0) return;

            // ── Advance scroll — only after intro ────────────────────────────────
            _crawlScroll += s.WordSpeedFactor * 1.5f;

            // Calculate total height including blank-line extra spacing
            float totalH = 0f;
            foreach (string term in terms) totalH += string.IsNullOrEmpty(term) ? paraH : lineH;
            totalH += H * 1.5f;
            if (_crawlScroll > totalH) { _crawlScroll = 0f; if (hasIntro) _crawlIntroTick = 0; }

            var prevHint = bg.TextRenderingHint;
            bg.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // ── Render each term with blank-line paragraph spacing ────────────────
            float y_cursor = 0f;  // logical Y position accumulator
            for (int i = 0; i < terms.Length; i++)
            {
                float stepH = string.IsNullOrEmpty(terms[i]) ? paraH : lineH;
                float y_log = _crawlScroll - y_cursor;
                y_cursor += stepH;

                if (string.IsNullOrEmpty(terms[i])) continue;  // blank line = extra space only, no drawing
                if (y_log < -lineH) continue;

                float z = y_log * sinT + focal;
                if (z <= 0.001f) continue;

                float scale   = focal / z;
                float screenY = H - y_log * cosT * focal / z;
                if (screenY < -baseSize * scale * 2f || screenY > H + baseSize * scale) continue;

                float fontSize    = Math.Max(5f, baseSize * scale);
                float horizonFade = Math.Min(1f, (scale - 0.08f) / 0.12f);
                float entryFade   = Math.Min(1f, (y_log + lineH) / lineH);
                float fade        = Math.Min(horizonFade, entryFade);
                if (fade <= 0f) continue;

                float ct2 = Math.Max(0f, Math.Min(1f, scale));
                int   cr  = Clamp((int)(s.WordColor.R + (s.WordHeadColor.R - s.WordColor.R) * ct2));
                int   cg  = Clamp((int)(s.WordColor.G + (s.WordHeadColor.G - s.WordColor.G) * ct2));
                int   cb  = Clamp((int)(s.WordColor.B + (s.WordHeadColor.B - s.WordColor.B) * ct2));
                int   ca  = Clamp((int)(fade * 255));

                try
                {
                    using (Font cf = new Font(s.WordFontName, fontSize, crawlFs, GraphicsUnit.Pixel))
                    {
                        SizeF sz      = bg.MeasureString(terms[i], cf);
                        Font  useFont = cf;
                        Font  shrunk  = null;
                        if (sz.Width > stageW * scale && sz.Width > 1f)
                        {
                            float df = Math.Max(5f, fontSize * stageW * scale / sz.Width);
                            shrunk   = new Font(s.WordFontName, df, crawlFs, GraphicsUnit.Pixel);
                            useFont  = shrunk;
                            sz       = bg.MeasureString(terms[i], useFont);
                        }
                        tmpBrush.Color = Color.FromArgb(ca, cr, cg, cb);
                        bg.DrawString(terms[i], useFont, tmpBrush,
                            new PointF((W - sz.Width) / 2f, screenY - sz.Height / 2f));
                        if (shrunk != null) shrunk.Dispose();
                    }
                }
                catch { }
            }

            bg.TextRenderingHint = prevHint;

            int fadeTopH = Math.Max(2, H / 4);
            using (var lgb = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Point(0, 0), new Point(0, fadeTopH), Color.Black, Color.Transparent))
            {
                lgb.WrapMode = System.Drawing.Drawing2D.WrapMode.TileFlipXY;
                bg.FillRectangle(lgb, 0, 0, W, fadeTopH);
            }
            bg.FillRectangle(Brushes.Black, 0, H - 3, W, 3);
        }

        public void Tick()
        {
            bool suppressRain = s.CrawlHideRain || s.PopupHideRain;  // consolidated — both set by "No RAIN" checkbox
            if (suppressRain)
                bg.FillRectangle(Brushes.Black, 0, 0, W, H);  // pure black — no trail on Crawl words
            else
                bg.FillRectangle(fadeBrush, 0, 0, W, H);
            bool isCrawlMode = (s.WordStyle == "Crawl");
            if (isCrawlMode && s.CrawlStarfield) DrawStars();
            if (!suppressRain) DrawRain();
            if (s.WordMode=="Rain")
            {
                if (isCrawlMode) DrawCrawlPerspective();
                else             DrawDrops();
            }
            if (s.WordMode=="Popup") DrawPopups();
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
            int len = p.Word.Length;
            bool rl = (s.WordOrientation == "RightLeft");
            if (p.Phase == 0)
            {
                int rev = Math.Min(len, (int)(prog * len));
                for (int j = 0; j < len; j++)
                {
                    int pos = rl ? (len - 1 - j) : j;  // reveal order: RL starts from rightmost char
                    if (pos < rev)       p.Disp[j] = p.Word[j];
                    else if (pos == rev) p.Disp[j] = (p.Frame % 5 < 3) ? '_' : ' ';
                    else                 p.Disp[j] = ' ';
                }
                PaintPopup(p.Disp, p.CX, p.CY, p.FontSize, 1f, p.Glow);
            }
            else if (p.Phase == 1)
            { for (int j = 0; j < len; j++) p.Disp[j] = p.Word[j]; PaintPopup(p.Disp, p.CX, p.CY, p.FontSize, 1f, p.Glow); }
            else
            {
                int hide = Math.Min(len, (int)((1f - prog) * len));
                for (int j = 0; j < len; j++) p.Disp[j] = j >= hide ? ' ' : p.Word[j];
                PaintPopup(p.Disp, p.CX, p.CY, p.FontSize, prog, p.Glow);
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
        private TrackBar   trkCrawlFont, trkCrawlSpeed, trkCrawlCount;
        private Label      lblFade, lblFont, lblSpeed, lblWCount, lblWFont, lblPCount, lblPFont, lblWordSpeed, lblPopupSpeed;
        private Label      lblCrawlFont, lblCrawlSpeed, lblCrawlCount;
        private ComboBox   cboOrient, cboWordOrient;
        private Button[]   btnWordModes;   // 3-way single-select: Crawl / Rain / Popup
        private Button[]   btnWordStyles;   // kept for null-reset compat; replaced by btnWordEffects
        private Button[]   btnFxEffects;   // kept for null-reset compat; replaced by btnWordEffects
        private Button[]   btnWordEffects; // 7-way effect selector [Scroll,Fade,Build,Scramble,Scan,Zoom,Glitch]
        private Label      _lblWordOrient; // reference for enable/disable alongside cboWordOrient
        private TextBox    txtWatermark, txtWatermarkSub, txtExtra;
        private CheckBox   chkScanlines, chkWatermark, chkVeeam100, chkBuiltinTerms;
        private CheckBox   chkWordFontBold, chkWordFontItalic;
        private CheckBox   chkCrawlHideRain, chkPopupHideRain;
        private CheckBox   chkCrawlStarfield;
        private CheckBox   chkOrderedTerms;
        private Button     _btnCrawlText;
        private PictureBox _bannerPic;         // banner in left column — swapped on CRAWL toggle
        private Image      _bannerDefault;     // standard banner (Word Stream / Popup mode)
        private Image      _bannerCrawl;       // Jedi banner (CRAWL mode)
        private bool       _showCrawlBanner;  // true when CRAWL mode is active (controls which banner paints)
        private Panel      _crawlSectionPnl, _streamSectionPnl, _popupSectionPnl;
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

        private static readonly string _crawlDefaultVeeam =
            // ── INTRO (shown statically centered before scroll begins) ──
            "A LONG TIME AGO, IN A DATACENTER FAR, FAR AWAY…\r\n" +
            // ── BODY (perspective crawl — blank lines = paragraph breaks) ──
            "\r\n" +
            "VEEAM\r\n\r\n" +
            "BACKUP & REPLICATION\r\n\r\n" +
            "EPISODE XIII\r\n\r\n" +
            "THE RISE OF CYBER RESILIENCE\r\n\r\n" +
            "THE GALAXY IS UNDER SIEGE.\r\n" +
            "A RUTHLESS ALLIANCE OF RANSOMWARE OPERATORS\r\n" +
            "HAS UNLEASHED ENCRYPTED CHAOS ACROSS THOUSANDS OF DATACENTERS,\r\n" +
            "SILENCING CRITICAL WORKLOADS\r\n" +
            "AND HOLDING ENTIRE ORGANIZATIONS HOSTAGE.\r\n\r\n" +
            "TRADITIONAL DEFENSES HAVE CRUMBLED.\r\n" +
            "ONLY THOSE WHO EMBRACED THE TRUE POWER OF IMMUTABILITY —\r\n" +
            "BACKED BY OBJECT STORAGE AND AIR-GAPPED REPOSITORIES —\r\n" +
            "HAVE SURVIVED THE ONSLAUGHT.\r\n\r\n" +
            "DEEP IN THE CLOUD, A REBEL ALLIANCE OF ENGINEERS\r\n" +
            "HAS RALLIED AROUND A NEW HOPE:\r\n" +
            "VEEAM DATA CLOUD —\r\n" +
            "A SOVEREIGN FORTRESS OF RESILIENT BACKUPS,\r\n" +
            "ALWAYS-ON RECOVERY, AND ZERO-TRUST ARCHITECTURE\r\n" +
            "STRETCHING ACROSS HYPERSCALERS AND PRIVATE CLOUDS ALIKE.\r\n\r\n" +
            "VEEAM BACKUP & REPLICATION STANDS AT THE CENTER OF THE RESISTANCE —\r\n" +
            "ORCHESTRATING PROTECTION FOR VIRTUAL MACHINES,\r\n" +
            "PHYSICAL SERVERS, AND CLOUD-NATIVE WORKLOADS\r\n" +
            "ACROSS THE KNOWN UNIVERSE.\r\n" +
            "NO WORKLOAD LEFT BEHIND. NO RECOVERY POINT FORGOTTEN.\r\n\r\n" +
            "VEEAM BACKUP FOR MICROSOFT 365\r\n" +
            "GUARDS THE COLLABORATION FRONTIER —\r\n" +
            "PRESERVING EVERY MESSAGE, EVERY FILE, EVERY CONVERSATION\r\n" +
            "IN IMMUTABLE VAULTS BEYOND THE EMPIRE'S REACH.\r\n\r\n" +
            "THE 3-2-1-1-0 RULE —\r\n" +
            "THREE COPIES, TWO MEDIA, ONE OFFSITE,\r\n" +
            "ONE IMMUTABLE, ZERO ERRORS VERIFIED —\r\n" +
            "ECHOES THROUGH THE CORRIDORS OF EVERY RESILIENT ORGANIZATION.\r\n" +
            "IT IS SCRIPTURE. IT IS SURVIVAL.\r\n\r\n" +
            "YOU CANNOT PROTECT WHAT YOU HAVE NOT BACKED UP.\r\n" +
            "YOU CANNOT RECOVER WHAT YOU CANNOT TRUST.\r\n\r\n" +
            "THE EMPIRE'S ENCRYPTION KEYS SHATTER\r\n" +
            "AGAINST THE WALLS OF AIR-GAPPED IMMUTABILITY.\r\n" +
            "RANSOM DEMANDS GO UNANSWERED.\r\n" +
            "RECOVERY COMPLETES. SYSTEMS BREATHE AGAIN.\r\n\r\n" +
            "AND ACROSS THE GALAXY, ONE NAME IS WHISPERED\r\n" +
            "IN THE SERVER ROOMS, IN THE BOARDROOMS,\r\n" +
            "IN THE NOCS AND THE SOCS —\r\n\r\n" +
            "VEEAM.\r\n\r\n" +
            "ALWAYS ON. ALWAYS AVAILABLE. ALWAYS PROTECTED.";
        private ToolTip      _tip;
        // Controls grouped by word-mode layer — toggled by SyncWordModeVisibility()
        private List<Control> _streamControls = new List<Control>();
        private List<Control> _popupControls  = new List<Control>();
        private List<Control> _crawlControls  = new List<Control>();
        private List<Control> _miscControls        = new List<Control>();
        private List<Control> _wordEffectsControls = new List<Control>();
        private Panel         _wordEffectsPnl;
        private Panel         _miscSectionPnl;
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

        private const string CRAWL_BANNER_B64 =
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


        private static Image LoadCrawlBannerImage()
        {
            try
            {
                byte[] data = Convert.FromBase64String(CRAWL_BANNER_B64);
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
            cboOrient = cboWordOrient = cboLanguage = null;
            btnWordModes = null;
            txtWatermark = txtWatermarkSub = txtExtra = null;
            btnFxEffects = null; btnWordStyles = null; btnWordEffects = null; _lblWordOrient = null;
            chkScanlines = chkWatermark = chkVeeam100 = chkBuiltinTerms = null;
            chkWordFontBold = chkWordFontItalic = null;
            chkCrawlHideRain = null; chkPopupHideRain = null; chkCrawlStarfield = null; chkOrderedTerms = null; _btnCrawlText = null;
            _bannerPic = null; _bannerDefault = null; _bannerCrawl = null;
            _crawlSectionPnl = null; _streamSectionPnl = null; _popupSectionPnl = null; _miscSectionPnl = null; _wordEffectsPnl = null;
            trkCrawlFont = trkCrawlSpeed = trkCrawlCount = null;
            lblCrawlFont = lblCrawlSpeed = lblCrawlCount = null;
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
                WordStyle=s.WordStyle, WordSpeedFactor=s.WordSpeedFactor, CrawlHideRain=s.CrawlHideRain, CrawlStarfield=s.CrawlStarfield, OrderedTerms=s.OrderedTerms, CrawlText=s.CrawlText, PopupHideRain=s.PopupHideRain,
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
        // Safe TrackBar setter — adjusts Maximum (up or down) then sets Value within range
        private static void TrkSet(TrackBar t, int max, int val)
        {
            if (t == null) return;
            int clamped = Clamp(val, t.Minimum, max);
            // When reducing Maximum below current Value, reduce Value first to avoid exception
            if (max < t.Maximum && t.Value > max) t.Value = max;
            t.Maximum       = max;
            t.TickFrequency = Math.Max(1, max / 10);
            t.Value         = clamped;
        }
        // Converts a \r\n-based template string into pipe-separated CrawlText format
        // so it can be stored safely in a single config.ini line
        private static string TemplateToPipe(string template)
        {
            var result = new System.Collections.Generic.List<string>();
            bool prevBlank = false;
            foreach (string line in template.Split(new char[]{'\r','\n'}))
            {
                string t = line.Trim().ToUpper();
                if (t.Length == 0) { if (!prevBlank) result.Add(""); prevBlank = true; }
                else               { result.Add(t);  prevBlank = false; }
            }
            while (result.Count > 0 && result[0]               == "") result.RemoveAt(0);
            while (result.Count > 0 && result[result.Count - 1] == "") result.RemoveAt(result.Count - 1);
            return string.Join("|", result);
        }

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
            _streamControls      = new List<Control>();
            _popupControls       = new List<Control>();
            _crawlControls       = new List<Control>();
            _miscControls        = new List<Control>();
            _wordEffectsControls = new List<Control>();

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
            const int c1    = 14,  cW1   = 420;       // left column — RAIN + banner
            const int c2    = 456, cW2   = 412;       // middle column: 10px from div2 left, 10px from div3 right
            const int PREV_W = 640, PREV_H = 360;     // 16:9 live preview
            const int c3    = 888, cW3   = PREV_W+2; // right column: 10px from div3; cW3=642
            const int fw    = 1556;                   // form width
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
            // LEFT COLUMN — GENERAL (background rain + global settings)
            // ═══════════════════════════════════════════════════════════════════
            Section(T("GENERAL  (Background / Rain Effect)", "ALLGEMEIN  (Hintergrund / Regeneffekt)"), c1, yL, cW1); yL += 26;
            btnRainColor = ColBtn(T("Color","Farbe"),             cur.RainColor, c1,     yL, 200);
            btnHeadColor = ColBtn(T("Head (bright)","Kopf (hell)"), cur.HeadColor, c1+208, yL, 200);
            btnRainColor.Click += delegate {
                using (var dlg = new ColorDialog { Color=cur.RainColor, FullOpen=true })
                    if (dlg.ShowDialog(this)==DialogResult.OK) {
                        cur.RainColor=cur.WordColor=cur.PopupColor=dlg.Color;
                        SetBtn(btnRainColor, dlg.Color); MarkDirty(); }
            };
            btnHeadColor.Click += delegate {
                using (var dlg = new ColorDialog { Color=cur.HeadColor, FullOpen=true })
                    if (dlg.ShowDialog(this)==DialogResult.OK) {
                        cur.HeadColor=cur.WordHeadColor=dlg.Color;
                        SetBtn(btnHeadColor, dlg.Color); MarkDirty(); }
            };
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
            cboOrient = Cbo(c1+80, yL, 138, new string[]{"TopDown","BottomUp","LeftRight","RightLeft"}, cur.Orientation);
            cboOrient.SelectedIndexChanged += delegate { cur.Orientation = cboOrient.Text; };
            yL += CM;

            // Word Mode — 2 exclusive buttons: WORD STREAM / POPUP and CRAWL
            DLbl(T("Word Mode:","Wortmodus:"), c1, yL+5, 76);
            string[] _wmLabels = new string[]{ "WORD STREAM / POPUP", "CRAWL" };
            string[] _wmKeys   = new string[]{ "Rain",                "Crawl" };
            int[]    _wmWidths = new int[]   { 196,                   196     };  // 76+4+196+4+196 = 476 ≤ cW1(480)
            btnWordModes = new Button[_wmLabels.Length];
            int wmX = c1 + 80;
            for (int wmi = 0; wmi < _wmLabels.Length; wmi++)
            {
                string capturedKey = _wmKeys[wmi];
                var wmBtn = new Button {
                    Text      = _wmLabels[wmi],
                    Location  = new Point(wmX, yL),
                    Size      = new Size(_wmWidths[wmi], 26),
                    FlatStyle = FlatStyle.Flat,
                    Tag       = capturedKey,
                    Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                    BackColor = Color.FromArgb(90, 90, 90),  // start grey; SetWordModeButton() sets the active one
                    ForeColor = Color.White
                };
                wmBtn.FlatAppearance.BorderSize  = 1;
                wmBtn.FlatAppearance.BorderColor = Color.FromArgb(115, 115, 115);
                wmBtn.Click += delegate { SetWordMode(capturedKey); MarkDirty(); };
                Controls.Add(wmBtn);
                btnWordModes[wmi] = wmBtn;
                wmX += _wmWidths[wmi] + 4;
            }
            yL += CM;

            // One consolidated "No RAIN" checkbox — controls both CrawlHideRain and PopupHideRain
            chkCrawlHideRain = Chk(T("No RAIN (all word modes)","Kein REGEN (alle Wort-Modi)"),
                cur.CrawlHideRain || cur.PopupHideRain, c1, yL);
            chkCrawlHideRain.CheckedChanged += delegate {
                cur.CrawlHideRain = cur.PopupHideRain = chkCrawlHideRain.Checked;
            };
            yL += 26;

            // ═══════════════════════════════════════════════════════════════════
            // LEFT COLUMN — FONT  (shared by all word modes)
            // ═══════════════════════════════════════════════════════════════════
            HSep(yL, c1, div2-c1); yL += 12;
            Section(T("FONT", "SCHRIFT"), c1, yL, cW1); yL += 26;

            DLbl(T("Font:", "Schriftart:"), c1, yL+5, 80);
            cboWordFontName = new ComboBox { Location=new Point(c1+84, yL), Size=new Size(200, 24),
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
            txtFontPreviewText = new TextBox { Location=new Point(c1+292, yL), Size=new Size(cW1-292-4, 24),
                Text="VEEAM", BackColor=_inputBg, ForeColor=_inputFg, BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtFontPreviewText);
            yL += 30;

            chkWordFontBold   = Chk(T("Bold","Fett"),     cur.WordFontBold,   c1,    yL);
            chkWordFontItalic = Chk(T("Italic","Kursiv"), cur.WordFontItalic, c1+68, yL);
            chkWordFontBold.CheckedChanged   += delegate { cur.WordFontBold   = chkWordFontBold.Checked;   UpdateFontPreview(); MarkDirty(); };
            chkWordFontItalic.CheckedChanged += delegate { cur.WordFontItalic = chkWordFontItalic.Checked; UpdateFontPreview(); MarkDirty(); };
            yL += 26;

            picFontPreview = new PictureBox { Location=new Point(c1, yL), Size=new Size(cW1-4, 44),
                BackColor=Color.Black, BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(picFontPreview);
            UpdateFontPreview();
            cboWordFontName.SelectedIndexChanged += delegate { UpdateFontPreview(); MarkDirty(); };
            txtFontPreviewText.TextChanged       += delegate { UpdateFontPreview(); };
            yL += 54;

            // ═══════════════════════════════════════════════════════════════════
            // MIDDLE COLUMN — WORD EFFECTS  (Rain + Popup consolidated)
            // ═══════════════════════════════════════════════════════════════════
            _wordEffectsPnl = new Panel { Location=new Point(c2, yM), Size=new Size(cW2, 20), BackColor=_panelBg };
            _wordEffectsPnl.Controls.Add(new Label { Text=T("WORD EFFECTS","WORT-EFFEKTE"), Location=new Point(8,2),
                AutoSize=true, ForeColor=_secTxt, Font=new Font("Segoe UI",8.5f,FontStyle.Bold) });
            Controls.Add(_wordEffectsPnl);
            _wordEffectsControls.Add(_wordEffectsPnl);
            yM += 26;

            // ── 7 effect buttons: Scroll Fade Build Scramble Scan Zoom Glitch ──
            // widths: 54+44+48+70+44+44+52 = 356 + 6×4 = 380 ≤ cW2(412)
            string[] weNames  = new string[]{ "Scroll", "Fade", "Build", "Scramble", "Scan", "Zoom", "Glitch" };
            int[]    weWidths = new int[]   { 54,       44,     48,      70,          44,     44,     52      };
            const int WE_GAP = 4;
            btnWordEffects = new Button[weNames.Length];
            int weX = c2;
            for (int wei = 0; wei < weNames.Length; wei++)
            {
                string capturedWE = weNames[wei];
                var weBtn = new Button {
                    Text      = capturedWE,
                    Location  = new Point(weX, yM),
                    Size      = new Size(weWidths[wei], 26),
                    FlatStyle = FlatStyle.Flat,
                    Tag       = capturedWE,
                    Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    BackColor = Color.FromArgb(90, 90, 90),
                    ForeColor = Color.White
                };
                weBtn.FlatAppearance.BorderSize  = 1;
                weBtn.FlatAppearance.BorderColor = Color.FromArgb(115, 115, 115);
                weBtn.Click += delegate { SetWordEffect(capturedWE); MarkDirty(); };
                Controls.Add(weBtn);
                btnWordEffects[wei] = weBtn;
                _wordEffectsControls.Add(weBtn);
                weX += weWidths[wei] + WE_GAP;
            }
            // Init highlight only — do NOT call SetWordEffect() here, it would overwrite cur
            // (Crawl mode would lose its WordStyle="Crawl" setting)
            {
                string initHighlight = (cur.WordStyle == "Crawl" || string.IsNullOrEmpty(cur.WordStyle))
                    ? "" : cur.WordStyle;
                foreach (Button b in btnWordEffects)
                {
                    bool active = (initHighlight.Length > 0 && (string)b.Tag == initHighlight);
                    b.BackColor = active ? Color.FromArgb(0, 100, 28) : Color.FromArgb(90, 90, 90);
                    b.ForeColor = Color.White;
                    b.FlatAppearance.BorderColor = active
                        ? Color.FromArgb(0, 185, 55) : Color.FromArgb(115, 115, 115);
                }
            }
            yM += 32;

            // ── Direction ─────────────────────────────────────────────────────
            _lblWordOrient = DLbl(T("Direction:","Richtung:"), c2, yM+5, 68);
            _wordEffectsControls.Add(_lblWordOrient);
            {
                string initOrient = (cur.WordOrientation == "Same" || string.IsNullOrEmpty(cur.WordOrientation))
                    ? "LeftRight" : cur.WordOrientation;
                cboWordOrient = Cbo(c2+72, yM, 200, new string[]{"TopDown","BottomUp","LeftRight","RightLeft"}, initOrient);
            }
            _wordEffectsControls.Add(cboWordOrient);
            cboWordOrient.SelectedIndexChanged += delegate { if (!_syncingOrient && cboWordOrient.SelectedIndex >= 0) cur.WordOrientation = cboWordOrient.Text; };
            yM += CM;

            trkWordFont = SlRow(T("Font Size","Schriftgröße"), c2,yM,cW2, 8,72, cur.WordFontSize, out lblWFont);
            lblWFont.Text = cur.WordFontSize+" px";
            trkWordFont.ValueChanged += delegate {
                cur.WordFontSize = cur.PopupFontSize = trkWordFont.Value;
                lblWFont.Text = trkWordFont.Value+" px";
            };
            _wordEffectsControls.Add(trkWordFont); _wordEffectsControls.Add(lblWFont);
            yM += SL;

            trkWordSpeed = SlRow(T("Speed","Geschwindigkeit"), c2,yM,cW2, 1,30, (int)(cur.WordSpeedFactor*10), out lblWordSpeed);
            lblWordSpeed.Text = cur.WordSpeedFactor.ToString("F1")+"x";
            trkWordSpeed.ValueChanged += delegate {
                cur.WordSpeedFactor = cur.PopupSpeedFactor = trkWordSpeed.Value/10f;
                lblWordSpeed.Text = cur.WordSpeedFactor.ToString("F1")+"x";
            };
            _wordEffectsControls.Add(trkWordSpeed); _wordEffectsControls.Add(lblWordSpeed);
            yM += SL;

            trkWordCount = SlRow(T("Simultaneous","Gleichzeitig"), c2,yM,cW2, 1,30, cur.WordCount, out lblWCount);
            lblWCount.Text = cur.WordCount.ToString();
            trkWordCount.ValueChanged += delegate {
                cur.WordCount = cur.PopupCount = trkWordCount.Value;
                lblWCount.Text = trkWordCount.Value.ToString();
            };
            _wordEffectsControls.Add(trkWordCount); _wordEffectsControls.Add(lblWCount);
            yM += SL;

            // ═══════════════════════════════════════════════════════════════════
            // MIDDLE COLUMN — CRAWL
            // ═══════════════════════════════════════════════════════════════════
            HSep(yM, div2, div3-div2+1); yM += 12;
            _crawlSectionPnl = new Panel { Location=new Point(c2, yM), Size=new Size(cW2, 20), BackColor=_panelBg };
            _crawlSectionPnl.Controls.Add(new Label { Text=T("CRAWL  (like Star Wars Intro…)","CRAWL  (wie Star Wars Intro…)"), Location=new Point(8,2), AutoSize=true,
                ForeColor=_secTxt, Font=new Font("Segoe UI",8.5f,FontStyle.Bold) });
            Controls.Add(_crawlSectionPnl);
            _crawlControls.Add(_crawlSectionPnl);
            yM += 26;
            chkCrawlStarfield = Chk(T("Star field background", "Sternenhimmel-Hintergrund"),
                                    cur.CrawlStarfield, c2, yM);
            chkCrawlStarfield.CheckedChanged += delegate { cur.CrawlStarfield = chkCrawlStarfield.Checked; };
            Controls.Add(chkCrawlStarfield);
            _crawlControls.Add(chkCrawlStarfield);
            yM += 28;
            _btnCrawlText = new Button {
                Text      = T("✦ Select Text","✦ Text wählen"),
                Location  = new Point(c2, yM),
                Size      = new Size(cW2, 26),
                BackColor = Color.FromArgb(8, 8, 60),
                ForeColor = Color.FromArgb(255, 232, 31),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            _btnCrawlText.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 160);
            _btnCrawlText.Click += delegate { ShowCrawlTextEditor(); };
            Controls.Add(_btnCrawlText);
            _crawlControls.Add(_btnCrawlText);
            yM += 30;

            trkCrawlFont = SlRow(T("Font Size","Schriftgröße"), c2, yM, cW2, 8, 72, cur.WordFontSize, out lblCrawlFont);
            lblCrawlFont.Text = cur.WordFontSize + " px";
            trkCrawlFont.ValueChanged += delegate { cur.WordFontSize = trkCrawlFont.Value; lblCrawlFont.Text = cur.WordFontSize + " px"; MarkDirty(); };
            _crawlControls.Add(trkCrawlFont); _crawlControls.Add(lblCrawlFont);
            yM += SL;

            trkCrawlSpeed = SlRow(T("Speed","Geschwindigkeit"), c2, yM, cW2, 1, 120, (int)(cur.WordSpeedFactor * 10), out lblCrawlSpeed);
            lblCrawlSpeed.Text = cur.WordSpeedFactor.ToString("F1") + "x";
            trkCrawlSpeed.ValueChanged += delegate { cur.WordSpeedFactor = trkCrawlSpeed.Value / 10f; lblCrawlSpeed.Text = cur.WordSpeedFactor.ToString("F1") + "x"; MarkDirty(); };
            _crawlControls.Add(trkCrawlSpeed); _crawlControls.Add(lblCrawlSpeed);
            yM += SL;

            // Queue Depth removed — Crawl engine ignores WordCount (continuous perspective scroll)

            yM += 10;  // aligned with CHANGELOG separator in right column

            // ═══════════════════════════════════════════════════════════════════
            // MIDDLE COLUMN — MISCELLANEOUS
            // ═══════════════════════════════════════════════════════════════════
            HSep(yM, div2, div3-div2+1); yM += 12;
            _miscSectionPnl = new Panel { Location=new Point(c2, yM), Size=new Size(cW2, 20), BackColor=_panelBg };
            _miscSectionPnl.Controls.Add(new Label { Text=T("MISCELLANEOUS","SONSTIGES"), Location=new Point(8,2),
                AutoSize=true, ForeColor=_secTxt, Font=new Font("Segoe UI",8.5f,FontStyle.Bold) });
            Controls.Add(_miscSectionPnl);
            _miscControls.Add(_miscSectionPnl);
            yM += 26;

            chkScanlines = Chk("CRT Scanlines",   cur.ShowScanlines, c2,     yM);
            chkWatermark = Chk(T("Watermark","Wasserzeichen"), cur.ShowWatermark, c2+148, yM);
            chkVeeam100  = Chk(T("Veeam 100 Names","Veeam 100 Namen"), cur.ShowVeeam100, c2+282, yM);
            chkScanlines.CheckedChanged += delegate { cur.ShowScanlines = chkScanlines.Checked; };
            chkWatermark.CheckedChanged += delegate { cur.ShowWatermark = chkWatermark.Checked; };
            chkVeeam100.CheckedChanged  += delegate { cur.ShowVeeam100  = chkVeeam100.Checked;  };
            _miscControls.Add(chkScanlines); _miscControls.Add(chkWatermark); _miscControls.Add(chkVeeam100);
            yM += 28;

            // chkBuiltinTerms + btnCatalog on the SAME row — button starts at c2+120 (after checkbox ~110px)
            chkBuiltinTerms = Chk(T("Built-in terms","Eingebaut. Begriffe"), cur.UseBuiltinTerms, c2, yM+3);
            chkBuiltinTerms.CheckedChanged += delegate { cur.UseBuiltinTerms = chkBuiltinTerms.Checked; };
            _miscControls.Add(chkBuiltinTerms);
            int catX = 148, catW = cW2 - 152;   // button x-offset and width (also used for Watermark field)
            var btnCatalog = new Button {
                Text=T("Adjust catalog with built-in terms","Katalog mit eingebaut. Begriffen anpassen"),
                Location=new Point(c2+catX, yM), Size=new Size(catW, 24),
                FlatStyle=FlatStyle.Flat, BackColor=Color.FromArgb(0,76,22), ForeColor=Color.White };
            btnCatalog.FlatAppearance.BorderColor = Color.FromArgb(0,110,32);
            btnCatalog.Click += delegate { ShowTermsCatalog(); };
            Controls.Add(btnCatalog);
            _miscControls.Add(btnCatalog);
            yM += 28;

            // Watermark text — aligned with Adjust catalog button (same left edge = c2+catX)
            _miscControls.Add(DLbl(T("Watermark:", "Wasserzeichen:"), c2, yM+5, catX - 4));
            txtWatermark = new TextBox { Location=new Point(c2+catX, yM), Size=new Size(catW, 24),
                Text=cur.WatermarkText, BackColor=_inputBg,
                ForeColor=_inputFg, BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtWatermark);
            _miscControls.Add(txtWatermark);
            yM += 30;

            // Subtitle text – two-line multiline box; | acts as line-break in the watermark
            _miscControls.Add(DLbl(T("Subtitle:  ( | = line break in watermark)",
                   "Untertitel:  ( | = Zeilenumbruch im Wasserzeichen)"), c2, yM+5));
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
            _miscControls.Add(txtWatermarkSub);
            yM += 48;

            _miscControls.Add(DLbl(T("Custom terms (comma / | / newline separated):","Eigene Begriffe (Komma / | / Zeilenumbruch):"), c2, yM+5));
            {
                var btnClearExtra = new Button {
                    Text      = T("✕ Clear","✕ Löschen"),
                    Location  = new Point(c2 + cW2 - 76, yM),
                    Size      = new Size(72, 22),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0, 76, 22),
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 7.5f)
                };
                btnClearExtra.FlatAppearance.BorderColor = Color.FromArgb(0, 110, 32);
                btnClearExtra.Click += delegate { if (txtExtra != null) { txtExtra.Text = ""; cur.ExtraWords = ""; MarkDirty(); } };
                Controls.Add(btnClearExtra);
                _miscControls.Add(btnClearExtra);
            }
            yM += 22;
            txtExtra = new TextBox { Location=new Point(c2, yM), Size=new Size(cW2-4, 24),
                Text=cur.ExtraWords, BackColor=_inputBg,
                ForeColor=_inputFg, BorderStyle=BorderStyle.FixedSingle };
            Controls.Add(txtExtra);
            _miscControls.Add(txtExtra);
            yM += 30;

            // (Sequential order checkbox is in the Crawl section — see _streamControls)

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
                    "v1.70  Star Wars CRAWL intro phase; blank-line paragraph spacing; Veeam template expanded; Queue Depth removed\r\n" +
                    "       Persistence fix (init no longer overwrites cur); TrkSet helper prevents slider exceptions\r\n" +
                    "       No RAIN consolidated to one checkbox; CRAWL auto-disables all MISC; Word Stream defaults on mode switch\r\n" +
                    "       DE UI overlap fix (MISC checkbox spacing); CrawlText stored in pipe-format (multi-line config bug fixed)\r\n" +
                    "v1.69  Catalog button renamed + row aligned with Watermark field; Term Catalog Enter-key; No RAIN renamed\r\n" +
                    "       Episode IV / Spaceballs templates with Star Wars intro line; Built-in term guard on effect selection\r\n" +
                    "v1.68  Scan direction RL working; Glitch: no direction; Scramble button width fixed; Light mode as default\r\n" +
                    "       Catalog + Clear buttons: consistent green style\r\n" +
                    "v1.67  No RAIN applies to Stream + Popup; Direction: removed Same, Scramble/Scan=LR, Scroll=all 4; separator aligned\r\n" +
                    "v1.66  CRAWL fix (SetWordStyle null guard removed); 2-button Word Mode: WORD STREAM/POPUP + CRAWL\r\n" +
                    "v1.65  WORD EFFECTS: unified 7-button section (Scroll/Fade/Build/Scramble/Scan/Zoom/Glitch)\r\n" +
                    "v1.64  MISCELLANEOUS section greyed/disabled while CRAWL is active\r\n" +
                    "v1.63  Color + Head consolidated to 2 buttons (Rain/Word/Popup share one color picker)\r\n" +
                    "v1.62  HideRain in GENERAL; Crawl auto-populates Veeam template on first use\r\n" +
                    "v1.61  PopupHideRain; Word Mode order Rain/Crawl/Popup; colors in GENERAL; slider order\r\n" +
                    "v1.60  Section headers grey when inactive; Clear button for Custom Terms\r\n" +
                    "v1.59  Layout: GENERAL(left)+FONT(left); WORD STREAMS+CRAWL+POPUP in middle; all buttons grey/white inactive\r\n" +
                    "v1.58  Font/Bold/Italic moved to GENERAL section; Word Mode buttons grey/white inactive, green/white active\r\n" +
                    "v1.57  CRAWL section always visible (greyed when inactive); own Font/Speed/Queue sliders; CrawlText independent\r\n" +
                    "v1.56  Word Mode: 3-way exclusive selector (CRAWL / WORD STREAM Rain / POPUP WORDS); no more 'Both'\r\n" +
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
                // cur.WordMode and cur.WordStyle already synced by SetWordMode() / SetWordStyle()
                cur.ShowScanlines    = chkScanlines.Checked;
                cur.ShowWatermark    = chkWatermark.Checked;
                cur.ShowVeeam100     = chkVeeam100.Checked;
                if (chkBuiltinTerms != null) cur.UseBuiltinTerms = chkBuiltinTerms.Checked;
                cur.WatermarkText    = txtWatermark.Text.Trim();
                cur.WatermarkSubText = txtWatermarkSub.Text.Trim();
                cur.ExtraWords       = txtExtra.Text.Trim();
                // CrawlText is already live-synced via ShowCrawlTextEditor dialog
                cur.Language         = cboLanguage  != null ? cboLanguage.Text  : cur.Language;
                if (trkPopupSpeed != null) cur.PopupSpeedFactor = trkPopupSpeed.Value / 10f;
                if (cboWordFontName.SelectedItem != null) cur.WordFontName = cboWordFontName.SelectedItem.ToString();
                if (chkWordFontBold   != null) cur.WordFontBold   = chkWordFontBold.Checked;
                if (chkWordFontItalic != null) cur.WordFontItalic = chkWordFontItalic.Checked;
                if (chkCrawlHideRain  != null) { cur.CrawlHideRain = cur.PopupHideRain = chkCrawlHideRain.Checked; }
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
                _bannerDefault = LoadBannerImage();
                _bannerCrawl   = LoadCrawlBannerImage();
                _showCrawlBanner  = (cur.WordStyle == "Crawl");
                Image _bannerImgUnused = null;
                int bannerY   = yL + 8;
                int bannerH   = (yBot - 12) - bannerY - 8;  // yBot-12 = separator position (before += 12)
                Image anyBanner = _bannerDefault ?? _bannerCrawl;
                if (anyBanner != null && bannerH > 50)
                {
                    _bannerPic = new PictureBox {
                        Location    = new Point(c1, bannerY),
                        Size        = new Size(cW1, bannerH),
                        BackColor   = Color.Black,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    // Show Jedi banner during CRAWL, standard banner otherwise.
                    _bannerPic.Paint += delegate(object bps, PaintEventArgs bpe)
                    {
                        Image img = (_showCrawlBanner && _bannerCrawl != null) ? _bannerCrawl : _bannerDefault;
                        if (img == null || img.Width == 0) return;
                        var pb   = (PictureBox)bps;
                        double srcAR = (double)img.Width / img.Height;
                        int dw = pb.Width;
                        int dh = (int)(dw / srcAR);
                        int dx = 0;
                        int dy = (pb.Height - dh) / 2;
                        bpe.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        bpe.Graphics.DrawImage(img,
                            new Rectangle(dx, dy, dw, dh),
                            new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
                    };
                    Controls.Add(_bannerPic);
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
            SetWordModeButton(GetActiveWordModeKey());
            SyncWordModeVisibility();
            SyncWordStyleDirection();

            // ── Hover tooltips ────────────────────────────────────────────────
            _tip = new ToolTip { AutoPopDelay=9000, InitialDelay=500, ReshowDelay=300, ShowAlways=true };
            Action<Control,string,string> tip = (ctrl,en,de) => { if (ctrl!=null) _tip.SetToolTip(ctrl, T(en,de)); };
            // Rain section — consolidated color buttons
            tip(btnRainColor,    "Main color — applies to Rain characters, Word Stream and Popup simultaneously", "Hauptfarbe — gilt für Regen-Zeichen, Word-Stream und Popup gemeinsam");
            tip(btnHeadColor,    "Head (bright) color — applies to rain column head and word stream head",        "Kopf (hell) Farbe — gilt für Regenkopf und Word-Stream-Kopf gemeinsam");
            tip(trkFont,         "Character size for the background rain (px)",                              "Zeichengröße des Hintergrundregens (px)");
            tip(trkSpeed,        "Overall animation speed multiplier (1x = default)",                        "Animationsgeschwindigkeit (1x = Standard)");
            tip(trkFade,         "Trail persistence — lower value = longer glowing trail",                   "Spurlänge — kleiner Wert = längere Leuchtspur");
            tip(cboOrient,       "Direction the rain falls: TopDown / BottomUp / LeftRight / RightLeft",     "Regenrichtung: Von oben / unten / links / rechts");
            if (btnWordModes != null && btnWordModes.Length == 3)
            {
                tip(btnWordModes[0], "[ MATRIX WORDS ] — keywords rain and popup effects",                 "[ MATRIX WÖRTER ] — Keywords als Regen und Popup-Effekte");
                tip(btnWordModes[1], "★ STAR WARS INTRO — perspective crawl like the opening credits",     "★ STAR WARS INTRO — Perspektiv-Scroll wie im Vorspann");
                tip(btnWordModes[2], "POPUP WORDS — words appear as blips in random positions",            "POPUP-WÖRTER — Wörter erscheinen als Blips an zufälligen Positionen");
            }
            // General / Font section
            tip(cboWordFontName,    "Font used for keyword streams, popups and watermark",                    "Schriftart für Keyword-Streams, Popups und Wasserzeichen");
            tip(txtFontPreviewText, "Edit the sample text shown in the font preview box",                    "Vorschautext für die Schriftart-Vorschau ändern");
            tip(chkWordFontBold,    "Render keyword streams and popups in bold weight",                      "Keyword-Streams und Popups fett darstellen");
            tip(chkWordFontItalic,  "Render keyword streams and popups in italic",                           "Keyword-Streams und Popups kursiv darstellen");
            // Word Effects section
            if (btnWordEffects != null && btnWordEffects.Length == 7)
            {
                tip(btnWordEffects[0], "Scroll — keyword scrolls across screen (Rain only; all directions)",         "Scroll — Keyword scrollt über den Bildschirm (Regen; alle Richtungen)");
                tip(btnWordEffects[1], "Fade — word fades in and out in place (Rain + Popup)",                       "Fade — Wort blendet an Ort und Stelle ein/aus (Regen + Popup)");
                tip(btnWordEffects[2], "Build — chars decode left-to-right (Rain only; horizontal)",                 "Build — Zeichen werden L→R eingeblendet (Regen; horizontal)");
                tip(btnWordEffects[3], "Scramble — noise resolves to word, then reverses (Rain + Popup; all dir.)", "Scramble — Rauschen löst sich auf, kehrt um (Regen + Popup; alle Richt.)");
                tip(btnWordEffects[4], "Scan — popup types out left-to-right, then de-resolves (Popup best)",       "Scan — Popup tippt sich L→R ein und auf (am besten für Popup)");
                tip(btnWordEffects[5], "Zoom — popup zooms in from large to normal size (Popup best)",               "Zoom — Popup zoomt von groß auf normal (am besten für Popup)");
                tip(btnWordEffects[6], "Glitch — word appears through noise (Rain + Popup; all directions)",         "Glitch — Wort erscheint durch Rauschen (Regen + Popup; alle Richtungen)");
            }
            tip(cboWordOrient,   "Scroll/Scramble/Glitch: all 4 directions · Build: horizontal only · Fade/Scan/Zoom: no direction", "Scroll/Scramble/Glitch: alle 4 Richtungen · Build: horizontal · Fade/Scan/Zoom: keine");
            tip(trkWordFont,     "Font size for word effects — applies to both Rain and Popup (px)",                 "Schriftgröße für Wort-Effekte — gilt für Regen und Popup (px)");
            tip(trkWordSpeed,    "Speed for word effects — applies to both Rain and Popup",                          "Geschwindigkeit für Wort-Effekte — gilt für Regen und Popup");
            tip(trkWordCount,    "Simultaneous words — applies to both Rain streams and Popup blips",                "Gleichzeitige Wörter — gilt für Regen-Streams und Popup-Blips");
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
                b.BackColor = active ? Color.FromArgb(0, 100, 28) : Color.FromArgb(90, 90, 90);
                b.ForeColor = Color.White;
                b.FlatAppearance.BorderColor = active ? Color.FromArgb(0, 185, 55) : Color.FromArgb(115, 115, 115);
            }
        }

        // Unified effect selector — sets both WordStyle and PopupEffects, highlights btnWordEffects
        private void SetWordEffect(string name)
        {
            if (btnWordEffects == null) return;
            string[] valid = new string[]{ "Scroll", "Fade", "Build", "Scramble", "Scan", "Zoom", "Glitch" };
            bool found = false;
            foreach (string n in valid) if (n == name) { found = true; break; }
            if (!found) name = "Glitch";
            cur.WordStyle    = name;   // Rain/Crawl mode uses this
            cur.PopupEffects = name;   // Popup mode uses this (unknown names → Glitch via TryParseMode)
            // Ensure at least one term source is active (Veeam100 / Built-in terms / Custom terms)
            {
                bool hasTerms = (chkVeeam100     != null && chkVeeam100.Checked) ||
                                (chkBuiltinTerms != null && chkBuiltinTerms.Checked) ||
                                (txtExtra != null && txtExtra.Text.Trim().Length > 0);
                if (!hasTerms && chkBuiltinTerms != null)
                { chkBuiltinTerms.Checked = true; cur.UseBuiltinTerms = true; }
            }
            // Auto-determine internal WordMode from effect:
            // Scan/Zoom → Popup blips; Crawl stays Crawl; all others → Rain word streams
            if (name == "Scan" || name == "Zoom")
                cur.WordMode = "Popup";
            else if (name != "Crawl")
                cur.WordMode = "Rain";
            foreach (Button b in btnWordEffects)
            {
                bool active = ((string)b.Tag == name);
                b.BackColor = active ? Color.FromArgb(0, 100, 28) : Color.FromArgb(90, 90, 90);
                b.ForeColor = Color.White;
                b.FlatAppearance.BorderColor = active ? Color.FromArgb(0, 185, 55) : Color.FromArgb(115, 115, 115);
            }
            SyncWordStyleDirection();
            _showCrawlBanner = (cur.WordStyle == "Crawl");
            if (_bannerPic != null) _bannerPic.Refresh();
        }

        // Single-select: highlight chosen word style button and update cur.WordStyle
        private bool _settingStyle = false;  // guard: suppress Crawl-defaults during init
        private void SetWordStyle(string name, bool applyStyleDefaults = false)
        {
            string[] valid = new string[]{ "Scroll", "Fade", "Build", "Scramble", "Glitch", "Crawl" };
            bool found = false;
            foreach (string n in valid) if (n == name) { found = true; break; }
            if (!found) name = "Glitch";
            cur.WordStyle = name;
            // Highlight legacy btnWordStyles if present (may be null — Crawl logic runs regardless)
            if (btnWordStyles != null)
            {
                foreach (Button b in btnWordStyles)
                {
                    bool active = ((string)b.Tag == name);
                    b.BackColor = active ? Color.FromArgb(0, 100, 28) : Color.FromArgb(90, 90, 90);
                    b.ForeColor = Color.White;
                    b.FlatAppearance.BorderColor = active ? Color.FromArgb(0, 185, 55) : Color.FromArgb(115, 115, 115);
                }
            }
            // Apply Crawl-specific defaults when user switches to Crawl
            if (applyStyleDefaults && name == "Crawl")
            {
                // Slider defaults
                TrkSet(trkWordCount, 30,  30); cur.WordCount      = 30;  if (lblWCount    != null) lblWCount.Text    = "30";
                TrkSet(trkWordFont,  72,  50); cur.WordFontSize   = 50;  if (lblWFont     != null) lblWFont.Text     = "50 px";
                TrkSet(trkWordSpeed, 120, 50); cur.WordSpeedFactor = 5.0f; if (lblWordSpeed != null) lblWordSpeed.Text = "5.0x";
                TrkSet(trkCrawlFont, 72,  50);                            if (lblCrawlFont  != null) lblCrawlFont.Text  = "50 px";
                TrkSet(trkCrawlSpeed,120, 50);                            if (lblCrawlSpeed != null) lblCrawlSpeed.Text = "5.0x";
                // Sequential order is always mandatory for Crawl (enforced in engine)
                cur.OrderedTerms = true;
                // Auto-populate Veeam template when CrawlText is empty on first use
                // Convert from \r\n format → pipe format (safe for single-line config.ini storage)
                if (string.IsNullOrEmpty(cur.CrawlText))
                    cur.CrawlText = TemplateToPipe(_crawlDefaultVeeam);
                // Disable background rain, enable star field
                if (chkCrawlHideRain  != null) chkCrawlHideRain.Checked  = true;  cur.CrawlHideRain  = true;
                if (chkCrawlStarfield != null) chkCrawlStarfield.Checked = true;  cur.CrawlStarfield = true;
                // Word Mode: Crawl is its own mode — highlight the CRAWL button
                cur.WordMode = "Rain";
                SetWordModeButton("Crawl");
                // Disable all MISCELLANEOUS for Crawl — clean cinematic display
                if (chkScanlines    != null) chkScanlines.Checked    = false;
                cur.ShowScanlines   = false;
                if (chkWatermark    != null) chkWatermark.Checked    = false;
                cur.ShowWatermark   = false;
                if (chkVeeam100     != null) chkVeeam100.Checked     = false;
                cur.ShowVeeam100    = false;
                if (chkBuiltinTerms != null) chkBuiltinTerms.Checked = false;
                cur.UseBuiltinTerms = false;
                SyncWordModeVisibility();
            }
            SyncWordStyleDirection();
        }

        // Direction rules:
        //   Fade / Crawl / Zoom         → direction hidden
        //   Build / Scramble / Glitch / Scan → LeftRight / RightLeft only
        //   Scroll                      → all four directions
        private void SyncWordStyleDirection()
        {
            // Fade, Crawl, Zoom, Glitch have no direction
            bool hideDir = (cur.WordStyle == "Fade"  || cur.WordStyle == "Crawl" ||
                            cur.WordStyle == "Zoom"  || cur.WordStyle == "Glitch");
            bool streamActive = (GetActiveWordModeKey() == "Rain");
            if (!streamActive) hideDir = true;
            if (cboWordOrient     != null) { cboWordOrient.Visible     = !hideDir; cboWordOrient.Enabled     = !hideDir && streamActive; }
            if (_lblWordOrient   != null) { _lblWordOrient.Visible   = !hideDir; _lblWordOrient.Enabled   = !hideDir && streamActive; }
            // CRAWL section enable/disable is handled by SyncWordModeVisibility() via _crawlControls
            // chkOrderedTerms removed — Crawl always uses sequential order internally
            bool isCrawl = (cur.WordStyle == "Crawl");
            // Speed slider: expand to 120 when Crawl is active — keep expanded when leaving Crawl
            // so the user's speed setting (e.g. 5.0x) is preserved on mode switch
            if (trkWordSpeed != null && isCrawl && trkWordSpeed.Maximum < 120)
            {
                trkWordSpeed.Maximum       = 120;
                trkWordSpeed.TickFrequency = 12;
            }
            // Crawl uses WordCount as queue depth — slider stays visible for all styles

            // Rebuild orientation options when style switches between horizontal-only and all-directions
            if (cboWordOrient != null && streamActive && !hideDir)
            {
                // Build/Scramble/Scan → horizontal only; Scroll → all 4 directions
                bool hOnly = (cur.WordStyle == "Build" || cur.WordStyle == "Scramble" ||
                              cur.WordStyle == "Scan");
                string[] allOrients = new string[]{ "TopDown", "BottomUp", "LeftRight", "RightLeft" };
                string[] hOrients   = new string[]{ "LeftRight", "RightLeft" };
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

        // Returns the active word-mode key for UI purposes: "Crawl" or "Rain"
        // Both Rain and Popup internal modes map to "Rain" (= "WORD STREAM / POPUP" button)
        private string GetActiveWordModeKey()
        {
            if (cur.WordStyle == "Crawl") return "Crawl";
            return "Rain";  // covers both Rain (streams) and Popup (blips, when Scan/Zoom active)
        }

        // Highlight the matching Word Mode button
        // Active = green / white   Inactive = grey / white
        private void SetWordModeButton(string key)
        {
            if (btnWordModes == null) return;
            string[] tags = new string[]{ "Rain", "Crawl" };  // must match _wmKeys order
            for (int i = 0; i < btnWordModes.Length; i++)
            {
                bool active = (tags[i] == key);
                btnWordModes[i].BackColor = active
                    ? Color.FromArgb(0, 105, 30)           // green
                    : Color.FromArgb(90, 90, 90);          // grey
                btnWordModes[i].ForeColor = Color.White;   // always white
                btnWordModes[i].FlatAppearance.BorderColor = active
                    ? Color.FromArgb(0, 185, 55)           // bright green border
                    : Color.FromArgb(115, 115, 115);       // grey border
            }
        }

        // Switch the active word mode (called by the 2 Word Mode buttons)
        private void SetWordMode(string key)
        {
            if (key == "Crawl")
            {
                if (cur.WordStyle != "Crawl")
                    SetWordStyle("Crawl", applyStyleDefaults: true);
                // clicking CRAWL while already in Crawl → no-op (button stays green)
            }
            else  // "Rain" = combined WORD STREAM / POPUP mode
            {
                bool wasCrawl = (cur.WordStyle == "Crawl");
                if (wasCrawl) SetWordEffect("Glitch");   // exit Crawl → restore to Glitch stream
                // WordMode (Rain vs Popup) is auto-determined by the selected effect
                // Scan/Zoom → Popup blips; everything else → Rain word streams
                if (cur.WordStyle == "Scan" || cur.WordStyle == "Zoom")
                    cur.WordMode = "Popup";
                else
                    cur.WordMode = "Rain";
                SetWordModeButton("Rain");
                if (wasCrawl)
                {
                    // Reset Word Effects sliders to Word Stream defaults
                    TrkSet(trkWordFont,  72, 20); cur.WordFontSize    = 20;   if (lblWFont     != null) lblWFont.Text     = "20 px";
                    TrkSet(trkWordSpeed, 30,  5); cur.WordSpeedFactor = 0.5f; if (lblWordSpeed != null) lblWordSpeed.Text = "0.5x";
                    TrkSet(trkWordCount, 30,  3); cur.WordCount       = 3;    if (lblWCount    != null) lblWCount.Text    = "3";
                    // Restore MISC defaults that Crawl turned off
                    if (chkScanlines    != null) { chkScanlines.Checked    = true; cur.ShowScanlines   = true; }
                    if (chkWatermark    != null) { chkWatermark.Checked    = true; cur.ShowWatermark   = true; }
                    if (chkVeeam100     != null) { chkVeeam100.Checked     = true; cur.ShowVeeam100    = true; }
                    if (chkBuiltinTerms != null) { chkBuiltinTerms.Checked = true; cur.UseBuiltinTerms = true; }
                    if (chkCrawlHideRain != null) { chkCrawlHideRain.Checked = false; cur.CrawlHideRain = cur.PopupHideRain = false; }
                }
            }
            SyncWordModeVisibility();
            SyncWordStyleDirection();
            _showCrawlBanner = (cur.WordStyle == "Crawl");
            if (_bannerPic != null) _bannerPic.Refresh();
        }

        // Enable / disable controls depending on which layers are active
        private void SyncWordModeVisibility()
        {
            string key = GetActiveWordModeKey();
            bool isCrawl   = (key == "Crawl");
            bool hasStream = (key == "Rain");
            bool hasPopup  = (key == "Popup");
            foreach (var c in _crawlControls)       c.Enabled = isCrawl;
            foreach (var c in _wordEffectsControls) c.Enabled = !isCrawl;
            foreach (var c in _miscControls)        c.Enabled = !isCrawl;
            // Section header panels: green when active, grey when inactive
            Color hdrActive = _panelBg;
            Color hdrInactive = Color.FromArgb(55, 55, 55);
            if (_crawlSectionPnl  != null) _crawlSectionPnl.BackColor  = isCrawl  ? hdrActive : hdrInactive;
            if (_wordEffectsPnl   != null) _wordEffectsPnl.BackColor   = !isCrawl ? hdrActive : hdrInactive;
            if (_miscSectionPnl   != null) _miscSectionPnl.BackColor   = !isCrawl ? hdrActive : hdrInactive;
        }

        private void RebuildPreview()
        {
            _previewDirty = false;
            var s = Clone(cur);
            if (cboOrient       != null) s.Orientation     = cboOrient.Text;
            if (cboWordOrient   != null) s.WordOrientation  = string.IsNullOrEmpty(cboWordOrient.Text) ? "Same" : cboWordOrient.Text;
            // cur.WordMode is always in sync via SetWordMode / SetWordStyle
            s.WordStyle = cur.WordStyle;
            if (cboWordFontName != null && cboWordFontName.SelectedItem != null)
                s.WordFontName = cboWordFontName.SelectedItem.ToString();
            if (chkWordFontBold   != null) s.WordFontBold   = chkWordFontBold.Checked;
            if (chkWordFontItalic != null) s.WordFontItalic = chkWordFontItalic.Checked;
            if (trkFont         != null) s.FontSize         = trkFont.Value;
            if (trkSpeed        != null) s.SpeedFactor      = trkSpeed.Value / 10f;
            if (trkFade         != null) s.FadeAlpha        = trkFade.Value;
            // Use CRAWL sliders when in Crawl mode, stream sliders otherwise
            if (GetActiveWordModeKey() == "Crawl")
            {
                if (trkCrawlFont  != null) s.WordFontSize    = trkCrawlFont.Value;
                if (trkCrawlSpeed != null) s.WordSpeedFactor = trkCrawlSpeed.Value / 10f;
            }
            else
            {
                if (trkWordFont  != null) s.WordFontSize    = trkWordFont.Value;
                if (trkWordSpeed != null) s.WordSpeedFactor = trkWordSpeed.Value / 10f;
                if (trkWordCount != null) s.WordCount       = trkWordCount.Value;
            }
            if (trkPopupFont    != null) s.PopupFontSize    = trkPopupFont.Value;
            if (trkPopupCount   != null) s.PopupCount       = trkPopupCount.Value;
            if (trkPopupSpeed   != null) s.PopupSpeedFactor = trkPopupSpeed.Value / 10f;
            if (txtWatermark    != null) s.WatermarkText    = txtWatermark.Text.Trim();
            if (txtWatermarkSub != null) s.WatermarkSubText = txtWatermarkSub.Text.Trim();
            if (txtExtra        != null) s.ExtraWords       = txtExtra.Text.Trim();
            s.CrawlText = cur.CrawlText;  // always carry through (edited via dialog, not a text field)
            // Effects – kept in sync by SetPopupEffect()
            s.PopupEffects = cur.PopupEffects;
            // General flags – live-synced to cur, but read directly for safety
            if (chkScanlines != null) s.ShowScanlines = chkScanlines.Checked;
            if (chkWatermark != null) s.ShowWatermark = chkWatermark.Checked;
            if (chkVeeam100      != null) s.ShowVeeam100   = chkVeeam100.Checked;
            if (chkOrderedTerms  != null) s.OrderedTerms   = chkOrderedTerms.Checked;
            if (chkCrawlHideRain != null) { s.CrawlHideRain = s.PopupHideRain = chkCrawlHideRain.Checked; }
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
                cur.RainColor=cur.WordColor=cur.PopupColor=p.RainColor;
                cur.HeadColor=cur.WordHeadColor=p.HeadColor;
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
            if (btnRainColor != null) SetBtn(btnRainColor, cur.RainColor);
            if (btnHeadColor != null) SetBtn(btnHeadColor, cur.HeadColor);
        }

        private void Pick(ref Color field, Button btn)
        {
            using (ColorDialog dlg = new ColorDialog { Color=field, FullOpen=true })
                if (dlg.ShowDialog(this)==DialogResult.OK) { field=dlg.Color; SetBtn(btn,dlg.Color); MarkDirty(); }
        }

        // Opens the term catalog editor — shows built-in or custom terms.txt one per line,
        // saves to %APPDATA%\VeeaMatrix\terms.txt on OK.
        // ── Crawl Text Editor ──────────────────────────────────────────────────
        private void ShowCrawlTextEditor()
        {
            bool dm   = cur.DarkMode;
            Color bg  = dm ? Color.FromArgb(20,26,20) : Color.FromArgb(240,244,240);
            Color fg  = dm ? Color.FromArgb(0,210,60) : Color.FromArgb(0,120,35);
            Color ibg = dm ? Color.FromArgb(4,10,4)   : Color.FromArgb(235,247,235);
            Color ifg = dm ? Color.FromArgb(0,200,55) : Color.FromArgb(0,100,30);

            var dlg = new Form {
                Text            = T("Crawl Text Editor  –  like Star Wars Intro",
                                    "Crawl Text Editor  –  wie Star Wars Intro"),
                Size            = new Size(560, 640),
                MinimumSize     = new Size(420, 480),
                FormBorderStyle = FormBorderStyle.Sizable,
                StartPosition   = FormStartPosition.CenterParent,
                MaximizeBox     = false, MinimizeBox = false,
                BackColor       = bg, ForeColor = fg
            };

            int px = 12, py = 10, pw = 528;

            // ── Info ────────────────────────────────────────────────────────────
            var lblInfo = new Label {
                Text      = T("One phrase per line — each line scrolls as its own word in the Crawl effect.",
                              "Eine Phrase pro Zeile — jede Zeile scrollt als eigenes Wort im Crawl-Effekt."),
                Location  = new Point(px, py), Size = new Size(pw, 32), ForeColor = fg
            };
            dlg.Controls.Add(lblInfo); py += 34;

            // ── Templates ───────────────────────────────────────────────────────
            var lblT = new Label { Text = T("Templates:","Vorlagen:"), Location = new Point(px, py+4), AutoSize=true, ForeColor=fg };
            dlg.Controls.Add(lblT);

            string[] tmplNames = { "Episode IV", "Spaceballs", "Veeam" };
            // Episode IV — intro line first, then body with paragraph breaks
            string ep4 =
                "A LONG TIME AGO IN A GALAXY FAR, FAR AWAY....\r\n" +
                "\r\n" +
                "EPISODE IV\r\n" +
                "A NEW HOPE\r\n" +
                "\r\n" +
                "IT IS A PERIOD OF CIVIL WAR.\r\n" +
                "REBEL SPACESHIPS, STRIKING FROM A HIDDEN BASE,\r\n" +
                "HAVE WON THEIR FIRST VICTORY AGAINST\r\n" +
                "THE EVIL GALACTIC EMPIRE.\r\n" +
                "\r\n" +
                "DURING THE BATTLE, REBEL SPIES MANAGED\r\n" +
                "TO STEAL SECRET PLANS TO THE EMPIRE'S ULTIMATE WEAPON,\r\n" +
                "THE DEATH STAR,\r\n" +
                "AN ARMORED SPACE STATION WITH ENOUGH POWER\r\n" +
                "TO DESTROY AN ENTIRE PLANET.\r\n" +
                "\r\n" +
                "PURSUED BY THE EMPIRE'S SINISTER AGENTS,\r\n" +
                "PRINCESS LEIA RACES HOME ABOARD HER STARSHIP,\r\n" +
                "CUSTODIAN OF THE STOLEN PLANS\r\n" +
                "THAT CAN SAVE HER PEOPLE\r\n" +
                "AND RESTORE FREEDOM TO THE GALAXY....";
            string sp =
                // ── INTRO (static, centered) — one line before first blank ──
                "ONCE UPON A TIME WARP, IN A GALAXY VERY, VERY, VERY, VERY, FAR AWAY...\r\n" +
                // ── BODY (perspective crawl) ──
                "\r\n" +
                "THERE LIVED A RUTHLESS RACE OF BEINGS KNOWN AS\r\n" +
                "\r\n" +
                "SPACEBALLS\r\n" +
                "\r\n" +
                "CHAPTER ELEVEN\r\n" +
                "\r\n" +
                "THE EVIL LEADERS OF PLANET SPACEBALL,\r\n" +
                "HAVING FOOLISHLY SQUANDERED THEIR PRECIOUS ATMOSPHERE,\r\n" +
                "HAVE DEVISED A SECRET PLAN TO TAKE EVERY BREATH OF AIR\r\n" +
                "AWAY FROM THEIR PEACE-LOVING NEIGHBOR, PLANET DRUIDIA.\r\n" +
                "\r\n" +
                "TODAY IS PRINCESS VESPA'S WEDDING DAY.\r\n" +
                "UNBEKNOWNEST TO THE PRINCESS, BUT KNOWEST TO US,\r\n" +
                "DANGER LURKS IN THE STARS ABOVE. . .\r\n" +
                "\r\n" +
                "IF YOU CAN READ THIS, YOU DON'T NEED GLASSES.";
            string veeam = _crawlDefaultVeeam;
            string[] tmplTexts = { ep4, sp, veeam };

            // Current text → restore blank lines (paragraph breaks)
            string raw = cur.CrawlText ?? "";
            var initLines = raw.Split('|');  // keep empty entries (= blank lines)
            string initText = string.Join("\r\n", initLines);

            var txtCrawl = new TextBox {
                Location    = new Point(px, py + 30),
                Size        = new Size(pw, 380),
                Multiline   = true, ScrollBars = ScrollBars.Vertical, AcceptsReturn = true,
                Text        = initText,
                Font        = new Font("Consolas", 10f),
                BackColor   = ibg, ForeColor = ifg,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Dynamic template buttons — (pw - 70 - (n-1)*gap) / n per button
            {
                int tbGap = 4;
                int tbW   = (pw - 70 - (tmplNames.Length - 1) * tbGap) / tmplNames.Length;
                for (int ti = 0; ti < tmplNames.Length; ti++)
                {
                    int cap = ti;
                    var btnT = new Button {
                        Text      = tmplNames[cap],
                        Location  = new Point(px + 70 + cap * (tbW + tbGap), py - 1),
                        Size      = new Size(tbW, 24),
                        BackColor = Color.FromArgb(0,75,22), ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font      = new Font("Segoe UI", 7.5f)
                    };
                    btnT.FlatAppearance.BorderColor = Color.FromArgb(0,150,45);
                    btnT.Click += delegate { txtCrawl.Text = tmplTexts[cap]; };
                    dlg.Controls.Add(btnT);
                }
            }
            dlg.Controls.Add(txtCrawl);
            py += 30 + 380 + 8;

            // ── Save / Load templates ────────────────────────────────────────────
            string tmplDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "VeeaMatrix", "templates");

            var btnSave = new Button {
                Text      = T("Save as Template…","Als Vorlage speichern…"),
                Location  = new Point(px, py), Size = new Size(160, 26),
                BackColor = Color.FromArgb(0,55,18), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderColor = Color.FromArgb(0,120,36);
            btnSave.Click += delegate {
                // Ask name via tiny inline dialog
                var fName = new Form {
                    Size=new Size(320,110), FormBorderStyle=FormBorderStyle.FixedDialog,
                    Text="Save Template", StartPosition=FormStartPosition.CenterParent,
                    MaximizeBox=false, MinimizeBox=false, BackColor=bg, ForeColor=fg
                };
                var tName = new TextBox { Location=new Point(12,12), Size=new Size(280,24),
                    Text="My Crawl", BackColor=ibg, ForeColor=ifg, BorderStyle=BorderStyle.FixedSingle };
                var bOk = new Button { Text="Save", Location=new Point(12,44), Size=new Size(80,26),
                    DialogResult=DialogResult.OK, BackColor=Color.FromArgb(0,100,30), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
                var bCl = new Button { Text="Cancel", Location=new Point(100,44), Size=new Size(80,26),
                    DialogResult=DialogResult.Cancel, BackColor=Color.FromArgb(50,15,15), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
                fName.Controls.AddRange(new Control[]{tName,bOk,bCl});
                fName.AcceptButton=bOk; fName.CancelButton=bCl;
                if (fName.ShowDialog(dlg) == DialogResult.OK && tName.Text.Trim().Length > 0)
                {
                    try {
                        Directory.CreateDirectory(tmplDir);
                        string safe = string.Concat(tName.Text.Trim().Split(Path.GetInvalidFileNameChars()));
                        File.WriteAllText(Path.Combine(tmplDir, safe + ".txt"), txtCrawl.Text, Encoding.UTF8);
                    } catch { }
                }
            };
            dlg.Controls.Add(btnSave);

            var btnLoad = new Button {
                Text      = T("Load Template…","Vorlage laden…"),
                Location  = new Point(px + 168, py), Size = new Size(140, 26),
                BackColor = Color.FromArgb(0,55,18), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnLoad.FlatAppearance.BorderColor = Color.FromArgb(0,120,36);
            btnLoad.Click += delegate {
                if (!Directory.Exists(tmplDir)) { MessageBox.Show(T("No saved templates yet.","Noch keine gespeicherten Vorlagen.")); return; }
                var files = Directory.GetFiles(tmplDir, "*.txt");
                if (files.Length == 0) { MessageBox.Show(T("No saved templates yet.","Noch keine gespeicherten Vorlagen.")); return; }
                var fPick = new Form {
                    Size=new Size(300,260), FormBorderStyle=FormBorderStyle.FixedDialog,
                    Text="Load Template", StartPosition=FormStartPosition.CenterParent,
                    MaximizeBox=false, MinimizeBox=false, BackColor=bg, ForeColor=fg
                };
                var lst = new ListBox { Location=new Point(8,8), Size=new Size(270,170), BackColor=ibg, ForeColor=ifg };
                foreach (var f in files) lst.Items.Add(Path.GetFileNameWithoutExtension(f));
                if (lst.Items.Count > 0) lst.SelectedIndex = 0;
                var bOkL = new Button { Text="Load", Location=new Point(8,186), Size=new Size(80,26),
                    DialogResult=DialogResult.OK, BackColor=Color.FromArgb(0,100,30), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
                var bClL = new Button { Text="Cancel", Location=new Point(96,186), Size=new Size(80,26),
                    DialogResult=DialogResult.Cancel, BackColor=Color.FromArgb(50,15,15), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
                fPick.Controls.AddRange(new Control[]{lst,bOkL,bClL});
                fPick.AcceptButton=bOkL; fPick.CancelButton=bClL;
                if (fPick.ShowDialog(dlg) == DialogResult.OK && lst.SelectedItem != null)
                {
                    string path = Path.Combine(tmplDir, lst.SelectedItem + ".txt");
                    if (File.Exists(path)) txtCrawl.Text = File.ReadAllText(path, Encoding.UTF8);
                }
            };
            dlg.Controls.Add(btnLoad);
            py += 34;

            // ── OK / Cancel ──────────────────────────────────────────────────────
            var btnOK = new Button {
                Text            = "OK", Size = new Size(108, 30),
                Location        = new Point(px + pw - 220, py),
                DialogResult    = DialogResult.OK,
                BackColor       = Color.FromArgb(0,118,34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(0,200,55);
            var btnCancelDlg = new Button {
                Text            = T("Cancel","Abbrechen"), Size = new Size(108, 30),
                Location        = new Point(px + pw - 108, py),
                DialogResult    = DialogResult.Cancel,
                BackColor       = Color.FromArgb(50,15,15), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
            };
            btnCancelDlg.FlatAppearance.BorderColor = Color.FromArgb(130,36,36);
            dlg.Controls.AddRange(new Control[]{btnOK, btnCancelDlg});
            dlg.AcceptButton = btnOK; dlg.CancelButton = btnCancelDlg;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                // Preserve single blank lines (paragraph breaks); collapse multiples; save as |-separated
                var rawLines = txtCrawl.Text.Split(new char[]{'\n','\r'});
                var saved    = new System.Collections.Generic.List<string>();
                bool prevBlank = false;
                foreach (string line in rawLines)
                {
                    string t = line.Trim().ToUpper();
                    if (t.Length == 0)
                    {
                        if (!prevBlank) saved.Add("");   // one blank line = paragraph break
                        prevBlank = true;
                    }
                    else { saved.Add(t); prevBlank = false; }
                }
                // Trim leading/trailing blank lines
                while (saved.Count > 0 && saved[0]  == "") saved.RemoveAt(0);
                while (saved.Count > 0 && saved[saved.Count-1] == "") saved.RemoveAt(saved.Count-1);
                cur.CrawlText = string.Join("|", saved);
                MarkDirty();
            }
        }

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
                Multiline = true, ScrollBars = ScrollBars.Vertical, AcceptsReturn = true,
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
