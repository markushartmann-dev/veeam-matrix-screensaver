// VeeaMatrix.cs  -  Windows Screensaver v1.81
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

        internal static string SettingsProfilesDir
        { get { return Path.Combine(ConfigDir, "settings_profiles"); } }

        public void SaveToFile(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
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
            sb.AppendLine("PopupEffects="     + PopupEffects);
            sb.AppendLine("PopupCount="       + PopupCount);
            sb.AppendLine("PopupFontSize="    + PopupFontSize);
            sb.AppendLine("PopupColor="       + ToHex(PopupColor));
            sb.AppendLine("PopupSpeedFactor=" + PopupSpeedFactor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            sb.AppendLine("Orientation="      + Orientation);
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
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        public static Settings LoadFromFile(string filePath)
        {
            var s = new Settings();
            if (!File.Exists(filePath)) return s;
            var ic = System.Globalization.CultureInfo.InvariantCulture;
            foreach (string raw in File.ReadAllLines(filePath, Encoding.UTF8))
            {
                string ln = raw.Trim();
                if (ln.StartsWith("#") || ln.StartsWith("[") || !ln.Contains("=")) continue;
                int eq = ln.IndexOf('=');
                string k = ln.Substring(0, eq).Trim();
                string v = ln.Substring(eq + 1).Trim();
                try { switch (k) {
                    case "RainColor":       s.RainColor       = FromHex(v); break;
                    case "HeadColor":       s.HeadColor       = FromHex(v); break;
                    case "FadeAlpha":       s.FadeAlpha       = int.Parse(v); break;
                    case "FontSize":        s.FontSize        = int.Parse(v); break;
                    case "SpeedFactor":     s.SpeedFactor     = float.Parse(v, ic); break;
                    case "ShowScanlines":   s.ShowScanlines   = bool.Parse(v); break;
                    case "ShowWatermark":   s.ShowWatermark   = bool.Parse(v); break;
                    case "WordMode":        s.WordMode        = v; break;
                    case "WordCount":       s.WordCount       = int.Parse(v); break;
                    case "WordFontSize":    s.WordFontSize    = int.Parse(v); break;
                    case "WordColor":       s.WordColor       = FromHex(v); break;
                    case "WordHeadColor":   s.WordHeadColor   = FromHex(v); break;
                    case "GlowChance":      s.GlowChance      = float.Parse(v, ic); break;
                    case "PopupEffects":    s.PopupEffects    = v; break;
                    case "PopupCount":      s.PopupCount      = int.Parse(v); break;
                    case "PopupFontSize":   s.PopupFontSize   = int.Parse(v); break;
                    case "PopupColor":      s.PopupColor      = FromHex(v); break;
                    case "PopupSpeedFactor": s.PopupSpeedFactor = float.Parse(v, ic); break;
                    case "Orientation":     s.Orientation     = v; break;
                    case "WordOrientation": s.WordOrientation = v; break;
                    case "WordStyle":       s.WordStyle       = v; break;
                    case "WordSpeedFactor": s.WordSpeedFactor = float.Parse(v, ic); break;
                    case "CrawlHideRain":   s.CrawlHideRain   = bool.Parse(v); break;
                    case "PopupHideRain":   s.PopupHideRain   = bool.Parse(v); break;
                    case "CrawlStarfield":  s.CrawlStarfield  = bool.Parse(v); break;
                    case "OrderedTerms":    s.OrderedTerms    = bool.Parse(v); break;
                    case "CrawlText":       s.CrawlText       = v; break;
                    case "ShowVeeam100":    s.ShowVeeam100    = bool.Parse(v); break;
                    case "UseBuiltinTerms": s.UseBuiltinTerms = bool.Parse(v); break;
                    case "WatermarkText":   s.WatermarkText   = v; break;
                    case "WatermarkSubText": s.WatermarkSubText = v; break;
                    case "ExtraWords":      s.ExtraWords      = v; break;
                    case "WordFontName":    s.WordFontName    = v; break;
                    case "WordFontBold":    s.WordFontBold    = bool.Parse(v); break;
                    case "WordFontItalic":  s.WordFontItalic  = bool.Parse(v); break;
                    case "Language":        s.Language        = v; break;
                    case "DarkMode":        s.DarkMode        = bool.Parse(v); break;
                } } catch { }
            }
            return s;
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
            const int div2  = 445, div3  = 878;       // vertical divider x positions (1px lines)
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
            string[] _wmLabels = new string[]{ T("* MATRIX RAIN *","* MATRIX REGEN *"), T("* STAR WARS INTRO *","* STAR WARS INTRO *") };
            string[] _wmKeys   = new string[]{ "Rain",                "Crawl" };
            int[]    _wmWidths = new int[]   { 164,                                           164                                         };  // 80+4+164+4+164 = 416 <= cW1(420)
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
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            _btnCrawlText.FlatAppearance.BorderColor = Color.FromArgb(115, 115, 115);
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

            // ── Settings Profiles — Save / Load full settings as named profiles ──
            HSep(yR, div3, fw-div3-14); yR += 12;
            Section(T("SETTINGS PROFILES","EINSTELLUNGSPROFILE"), c3, yR, cW3); yR += 26;
            {
                string profDir = Settings.SettingsProfilesDir;
                // Profile dropdown
                var cboProf = new ComboBox {
                    Location = new Point(c3, yR), Size = new Size(cW3 - 176, 24),
                    FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = _dark ? Color.FromArgb(18,30,18) : Color.FromArgb(240,250,240),
                    ForeColor = ForeColor,  Font = new Font("Segoe UI", 8.5f)
                };
                Action refreshProfiles = delegate {
                    cboProf.Items.Clear();
                    if (Directory.Exists(profDir))
                        foreach (string f in Directory.GetFiles(profDir, "*.ini"))
                            cboProf.Items.Add(Path.GetFileNameWithoutExtension(f));
                };
                refreshProfiles();
                Controls.Add(cboProf);
                int bpx = c3 + cW3 - 168;  // x for profile buttons
                // Save As button
                var btnPSave = new Button {
                    Text = T("Save As","Speichern"),
                    Location = new Point(bpx, yR - 1), Size = new Size(80, 26),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0,80,22), ForeColor = Color.White
                };
                btnPSave.FlatAppearance.BorderColor = Color.FromArgb(0,170,55);
                btnPSave.Click += delegate {
                    string name = Microsoft.VisualBasic.Interaction.InputBox(
                        T("Profile name:","Profilname:"),
                        T("Save Settings Profile","Einstellungsprofil speichern"), "");
                    name = name.Trim();
                    if (name.Length == 0) return;
                    foreach (char ch in Path.GetInvalidFileNameChars()) name = name.Replace(ch.ToString(), "");
                    if (name.Length == 0) return;
                    try {
                        Directory.CreateDirectory(profDir);
                        cur.SaveToFile(Path.Combine(profDir, name + ".ini"));
                        refreshProfiles();
                        cboProf.SelectedItem = name;
                    } catch (Exception ex) { MessageBox.Show(ex.Message); }
                };
                Controls.Add(btnPSave);
                // Load button
                var btnPLoad = new Button {
                    Text = T("Load","Laden"),
                    Location = new Point(bpx + 84, yR - 1), Size = new Size(40, 26),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0,60,90), ForeColor = Color.White
                };
                btnPLoad.FlatAppearance.BorderColor = Color.FromArgb(0,140,200);
                btnPLoad.Click += delegate {
                    if (cboProf.SelectedItem == null) return;
                    string fp = Path.Combine(profDir, cboProf.SelectedItem.ToString() + ".ini");
                    try {
                        var loaded = Settings.LoadFromFile(fp);
                        loaded.Language = cur.Language;  // keep UI language
                        cur = loaded;
                        RebuildUI();
                    } catch (Exception ex) { MessageBox.Show(ex.Message); }
                };
                Controls.Add(btnPLoad);
                // Delete button
                var btnPDel = new Button {
                    Text = T("Del","Löschen"),
                    Location = new Point(bpx + 128, yR - 1), Size = new Size(40, 26),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(60,10,10), ForeColor = Color.White
                };
                btnPDel.FlatAppearance.BorderColor = Color.FromArgb(140,30,30);
                btnPDel.Click += delegate {
                    if (cboProf.SelectedItem == null) return;
                    string fp = Path.Combine(profDir, cboProf.SelectedItem.ToString() + ".ini");
                    try { File.Delete(fp); refreshProfiles(); } catch { }
                };
                Controls.Add(btnPDel);
                yR += 32;
            }

            // ── CHANGE LOG — right column ─────────────────────────────────────
            HSep(yR, div3, fw-div3-14); yR += 12;
            Section(T("CHANGE LOG","ÄNDERUNGSPROTOKOLL"), c3, yR, cW3); yR += 26;
            {
                string changelog =
                    "v1.80  Narrower cinema bars (86%, was 72%); Credits white text; Crawl button grey/white\r\n" +
                    "v1.81  BACKUP OPERATIONS: Settings Profiles - save/load full settings as named .ini files\r\n" +
                    "v1.79  Cinema letterbox: both banners fill-width, 72% height, equal bars top+bottom\r\n" +
                    "v1.78  Credits popup respects Light/Dark mode; Matrix banner fills full width, bars on top\r\n" +
                    "v1.77  Credits popup: OK button; Term Catalog smaller (560x540) + Light/Dark mode\r\n" +
                    "v1.76  Credits button (LinkedIn + markushartmann.blog links); dual-banner black bars\r\n" +
                    "v1.75  Button labels: * MATRIX RAIN * / * STAR WARS INTRO * (DE+EN)\r\n" +
                    "v1.74  Column alignment fixed (div2/div3 corrected); button widths adjusted\r\n" +
                    "v1.73  Jedi banner recompressed 1200px/q67 (87 KB); .scr shrinks 1.9 MB -> 758 KB\r\n" +
                    "v1.72  Left column narrower (420 px, fw=1556); fill-width banner rendering\r\n" +
                    "v1.71  Dual banner: Jedi image in CRAWL mode, Matrix image in Word Stream mode\r\n" +
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

            // Credits button
            var btnCredits = new Button {
                Text = T("Credits","Credits"),
                Location = new Point(c1, yBot), Size = new Size(80, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 40, 20), ForeColor = Color.White
            };
            btnCredits.FlatAppearance.BorderColor = Color.FromArgb(0, 160, 60);
            btnCredits.Click += delegate {
                bool dmC = cur.DarkMode;
                var dC = new Form { Text = T("Credits","Credits"), Size = new Size(420,210),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false, MinimizeBox = false,
                    BackColor = dmC ? Color.FromArgb(10,18,10) : Color.FromArgb(240,248,240) };
                var lC = new LinkLabel {
                    Text = T("a tribute to the veeam community\nby Markus Hartmann\nmarkushartmann.blog",
                             "Ein Dank an die Veeam Community\nvon Markus Hartmann\nmarkushartmann.blog"),
                    Location = new Point(20,20), Size = new Size(370,90),
                    ForeColor = dmC ? Color.FromArgb(160,220,160) : Color.FromArgb(20,70,20),
                    Font = new Font("Segoe UI", 10f),
                    LinkColor = dmC ? Color.FromArgb(0,210,90) : Color.FromArgb(0,140,50),
                    ActiveLinkColor = Color.FromArgb(0,255,110),
                    BackColor = Color.Transparent, AutoSize = false };
                int p1 = lC.Text.IndexOf("Markus Hartmann");
                int p2 = lC.Text.LastIndexOf("markushartmann.blog");
                lC.Links.Add(new LinkLabel.Link(p1, "Markus Hartmann".Length,    "https://www.linkedin.com/in/markus-hartmann-28311232/"));
                lC.Links.Add(new LinkLabel.Link(p2, "markushartmann.blog".Length, "https://markushartmann.blog"));
                lC.LinkClicked += delegate(object s2, LinkLabelLinkClickedEventArgs e2) {
                    try { System.Diagnostics.Process.Start((string)e2.Link.LinkData); } catch {}
                };
                var bOkC = new Button { Text = "OK", DialogResult = DialogResult.OK,
                    Location = new Point(270,120), Size = new Size(80,28),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0,100,28), ForeColor = Color.White };
                bOkC.FlatAppearance.BorderColor = Color.FromArgb(0,185,55);
                dC.Controls.Add(lC); dC.Controls.Add(bOkC);
                dC.AcceptButton = bOkC; dC.ShowDialog(this);
            };
            Controls.Add(btnCredits);
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
                        int dw = pb.Width;
                        int dh = (int)(pb.Height * 0.86);  // cinema letterbox: ~7% bars top + bottom
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

            bool dmT = cur.DarkMode;
            Color tcBg   = dmT ? Color.FromArgb(22, 28, 22)  : Color.FromArgb(245, 250, 245);
            Color tcTxt  = dmT ? Color.FromArgb(0, 210, 60)  : Color.FromArgb(20, 80, 20);
            Color tcEdit = dmT ? Color.FromArgb(32, 40, 32)  : Color.White;
            Color tcMute = dmT ? Color.FromArgb(140,140,140) : Color.FromArgb(80,80,80);
            var dlg = new Form {
                Text = T("Term Catalog  –  one term per line", "Wort-Katalog  –  ein Begriff pro Zeile"),
                Size = new Size(560, 540),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false,
                BackColor = tcBg, ForeColor = tcTxt
            };
            var lblInfo = new Label {
                Text = T("Edit the term list. Each line = one term. Lines starting with # are ignored.",
                         "Begriffsliste bearbeiten. Jede Zeile = ein Begriff. Zeilen mit # werden ignoriert."),
                Location = new Point(10, 8), Size = new Size(530, 18),
                ForeColor = tcMute, Font = new Font("Segoe UI", 8.5f)
            };
            var txt = new TextBox {
                Multiline = true, ScrollBars = ScrollBars.Vertical, AcceptsReturn = true,
                Location = new Point(10, 30), Size = new Size(528, 420),
                Text = initial,
                BackColor = tcEdit, ForeColor = tcTxt,
                Font = new Font("Courier New", 8.5f),
                BorderStyle = BorderStyle.FixedSingle,
                WordWrap = false
            };
            var btnReset = new Button {
                Text = T("Reset to defaults","Auf Standard zurücksetzen"),
                Location = new Point(10, 460), Size = new Size(186, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 35, 0), ForeColor = Color.White
            };
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(140, 90, 0);
            btnReset.Click += delegate { txt.Text = string.Join(Environment.NewLine, MatrixEngine.TERMS); };

            var btnOK = new Button {
                Text = "OK", Location = new Point(362, 460), Size = new Size(80, 30),
                DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 28), ForeColor = Color.White
            };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(0, 185, 55);
            var btnCancel = new Button {
                Text = T("Cancel","Abbrechen"), Location = new Point(452, 460), Size = new Size(86, 30),
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
