// VeeamMatrix.cs  –  Windows Screensaver v1.8
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
        // DM Sans Bold embedded as TTF bytes — Veeam's geometric sans-serif (ES Build equivalent).
        // Source: Google Fonts open-source distribution (OFL licence).
        private static readonly byte[] DMSANS_BOLD = Convert.FromBase64String(
            "AAEAAAASAQAABAAgRFNJRwAAAAEAAR7kAAAACEdERUYVPhJfAADR0AAAAMZHUE9Tg6n1ngAA0pgAAEfCR1NVQl8Na0IAARpcAAAEhk9TLzKCqT6WAAABqAAAAGBjbWFwfMtoWQAACKgAAASgY3Z0IBXcBjUAABwUAAAAYGZwZ21iLv99AAANSAAADgxnYXNwAAAAEAAA0cgAAAAIZ2x5ZkvTNA4AAB/IAAChAGhlYWQWdCFvAAABLAAAADZoaGVhCF8D2wAAAWQAAAAkaG10eHtPP9YAAAIIAAAGoGxvY2Fm4T8wAAAcdAAAA1JtYXhwAxMPEQAAAYgAAAAgbmFtZW7mj40AAMDIAAAEjnBvc3QNZvpvAADFWAAADHBwcmVwzKRWIQAAG1QAAAC9AAEAAAABMzPY8BGFXw889QAPA+gAAAAA2QgNjAAAAADZy50Q/8b+7gS2A88AAQAGAAIAAAAAAAAAAQAAA+D+ygAABN7/xv6KBLYAAQAAAAAAAAAAAAAAAAAAAagAAQAAAagAWAAHAFIABAACACoAVwCNAAAApQ4MAAMAAgAEAj0CvAAFAAACigJYAAAASwKKAlgAAAFeADIBKgAAAAAAAAAAAAAAAIAAAC9QACBbAAAAAAAAAABHT09HACAADfsCA+D+ygAAA+ABNiAAAJMAAAAAAfACvAAAACAAAwIZAEQCvQAXAr0AFwK9ABcCvQAXAr0AFwK9ABcCvQAXAsMAFwK9ABcCvQAXA6sAFwOrABcCdwBEAuEALQLhAC0C4QAtAuEALQLhAC0CxwBEAtQABgLHAEQC1AAGAkIARAJCAEQCQgBEAkIARAJCAEQCQgBEAkIARAJCAEQCQgBEAkIARAJCAEQCJwBEAwUALQMFAC0DBQAtAwUALQLCAEQBCABEAQgARAEI/+kBCP/yAQj/7AEIAEQBCP/UAQj/yQEI/+0BCP/ZAhQAGgKAAEQCgABEAicARAInAEQCJwBEAicARAInAEQCQAAUA3MARALRAEQC0QBEAtEARALRAEQC0QBEAxcALQMXAC0DFwAtAxcALQMXAC0DFwAtAxcALQMXAC0DEAAfAxcALQRPAC0CXwBEAl8ARAMUAC0CbwBEAm8ARAJvAEQCbwBEAlUAKwJVACsCVQArAlUAKwJVACsC/gAtAk8AGwJPABsCTwAbAk8AGwKpAEACqQBAAqkAQAKpAEACqQBAAqkAQAKpAEACqQBAAqkAQAKpAEACqQBAArsADwPyABkD8gAZA/IAGQPyABkD8gAZApMAHwJxAA4CcQAOAnEADgJxAA4CcQAOAnEADgI2AC8CNgAvAjYALwI2AC8CQQAyAkEAMgJBADICQQAyAkEAMgJBADICQQAyAkEAMgJBADICQQAyA60AMgOtADICjABBAlgAMQJYADECWAAxAlgAMQJYADECjAAxAoUAMQMgADECqwAxAlYAMQJWADECVgAxAlYAMQJWADECVgAxAlYAMQJWADECVgAxAlYAMQJWADECVgArAWcAFAI5ACYCOQAmAjkAJgI5ACYCYABBAQsANgEDAEEBAwBBAQP/5wED//ABA//qAQP/0gED/8YBC//vAQP/0gEM/+MCOABBAjgAQQEDAEEBAwBBAZcAQQELAEEBigBGASEACwOpAEECYABBAmAAQQJgAEECYABBAmAAQQJeADECXgAxAl4AMQJeADECXgAxAl4AMQJeADECXgAxAkwAHgJeADEDzwAxAowAQQKMAEECjAAxAYwAQQGMAEEBjABBAYwAQQINACYCDQAmAg0AJgINACYCDQAmAsMAQgGlAB0BpQAdAaUAHQGlAB0CYAA5AmAAOQJgADkCYAA5AmAAOQJgADkCYAA5AmAAOQJgADkCYAA5AmAAOQIpABEDIgARAyIAEQMiABEDIgARAyIAEQIxABICVQATAlUAEwJVABMCVQATAlUAEwJVABMB3wAhAd8AIQHfACEB3wAhAowAMQKMADECjAAxAowAMQKMADECjAAxAowAMQKMADECjAAxAowAMQP1ADED9QAxApcAMQKXADEClwAxApcAMQJeABQCagAUAcsAHQG+AB0ChwAhAsYALQFZAB0COwAzAlQALgKBACsCbAA/AnoAMQIWABsCdQA6AnoAOQJdADECXgAxAl4AMQDpABgBZQAiAWAAFQFaABoB9f/5AuwAJwLCACcDKQAtAPAAKADnAA4A8gAoAQoAGAKZACgBKABEASgARAIPAB4CDwAnAPAAKAF8ADYB+gBEA1wAIQGlABcBpQAXANAAKQGPAC0BjwAgAc4AWQHOAD4BaABEAWgANgIPADYCWAA2AogANgNkADYCqQA2APAADgGwAA4ByAAhAcgAKQEIACEBCAApAfkAKgH5ADIBLwAqAS8AMgFcACcAvQAnANAAKQFxACkBZgAjAWUAJADFACMAxQAkAO4AAADuAAACWAAAAlgAMQIrAD8CVQArAxYAFQFO/8oCiAAdBHUARgJtACgCgQAVAaUAFwJVADgCEwA4AgIAOAIcADcCHgA7Ah4AOwIJAEYCCQBCAfQAOQH0ADgCaQA+Aj0AIAI9ACACsQAuApkAIQLkACEBE//UAxQAJwLHABcC4AAdAe0AKwMOABMCaQBBAk8AJQNoACgE3gAoAi8AHQQXAEoDBAAmAoEAHwJhACsDEgAsAf0ALQM8ABsBaAAmAQgARAEIAEQCUQAuAQgARAJSAC4C8wApAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAECABABVwAQAUQAEAElABABRAAQAVkAFQCfABIBAgAQAYkAEAGWABABBQAUAQAADwF6AA0AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAmoALQM5ABUAAAACAAAAAwAAABQAAwABAAAAFAAEBIwAAAB2AEAABQA2AA0ALwA5AH4BBwEbASMBMQE3AUgBWwFlAX4BjwGSAf0CGwJZAscC3QMEAwgDDAMoA8AehR69HvMe+SAUIBogHiAiICYgMCA6IEQgdCCoIKwgvSETISIhJiEuIgIiBiIPIhIiFSIaIh4iKyJIImAiZSXK+wL//wAAAA0AIAAwADoAoAEKAR4BKAE2ATkBTAFeAWgBjwGSAfwCGAJZAsYC2AMAAwYDCgMnA8AegB68HvIe+CATIBggHCAgICYgMCA5IEQgdCCoIKwgvSETISIhJiEuIgIiBiIPIhEiFSIaIh4iKyJIImAiZCXK+wH//wE+AAAA1wAAAAAAAAAAAAAAAAAAAAAAAAAA/sn/vgAAAAD+QQAAAAAAAAAAAAD+Y/1GAAAAAAAAAADhIQAAAAAAAOD64T/hBuDU4KPgquCj4JTgaeBV4EHgUN9r32LfWgAA30DfUd9H3zvfGd77AADbpgYBAAEAAAB0AAAAkAEYAeYCCAISAiQCJgJEAmICcAAAAAACmAKaAAACngKgAqoCsgK2AAAAAAK2AsACwgLEAAACxALIAswAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACsgAAAAAAAAAAAAAAAAKoAAAAAAAAAUkBIQFBASgBTgFuAXIBQgEsAS0BJwFWAR0BMgEcASkBHgEfAV0BWgFcASMBcQABAA0ADgATABcAIgAjACcAKAAyADMANQA7ADwAQQBMAE4ATwBTAFkAXQBoAGkAbgBvAHUBMAEqATEBZAE2AZMAeQCFAIYAiwCPAJsAnACgAKEAqwCsAK4AtAC1ALoAxQDHAMgAzADSANYA4QDiAOcA6ADuAS4BeQEvAWIBSgEiAUwBUwFNAVQBegF0AZEBdQEEAT0BYwEzAXYBlQF4AWABFQEWAYwBbAFzASUBjwEUAQUBPgEaARkBGwEkAAYAAgAEAAoABQAJAAsAEQAeABgAGwAcAC4AKQArACwAFABAAEYAQgBEAEoARQFYAEkAYgBeAGAAYQBwAE0A0QB+AHoAfACCAH0AgQCDAIkAlgCQAJMAlACnAKMApQCmAIwAuQC/ALsAvQDDAL4BWQDCANsA1wDZANoA6QDGAOsABwB/AAMAewAIAIAADwCHABIAigAQAIgAFQCNABYAjgAfAJcAGQCRAB0AlQAgAJgAGgCSACQAnQAmAJ8AJQCeADEAqgAvAKgAKgCkADAAqQAtAKIANACtADYArwA4ALEANwCwADkAsgA6ALMAPQC2AD8AuAA+ALcASADBAEMAvABHAMAASwDEAFAAyQBSAMsAUQDKAFQAzQBWAM8AVQDOAFsA1ABaANMAZwDgAGQA3QBfANgAZgDfAGMA3ABlAN4AawDkAHEA6gByAHYA7wB4APEAdwDwAAwAhABXANAAXADVAZABjgGNAZIBlwGWAZgBlAGBAYIBhAGIAYkBhgGAAX8BhwGDAYUAbQDmAGoA4wBsAOUAIQCZAHMA7AB0AO0BOwE8ATcBOQE6ATgBewF9ASYBagFXAV8BXrAALCCwAFVYRVkgIEu4AA5RS7AGU1pYsDQbsChZYGYgilVYsAIlYbkIAAgAY2MjYhshIbAAWbAAQyNEsgABAENgQi2wASywIGBmLbACLCMhIyEtsAMsIGSzAxQVAEJDsBNDIGBgQrECFENCsSUDQ7ACQ1R4ILAMI7ACQ0NhZLAEUHiyAgICQ2BCsCFlHCGwAkNDsg4VAUIcILACQyNCshMBE0NgQiOwAFBYZVmyFgECQ2BCLbAELLADK7AVQ1gjISMhsBZDQyOw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ApgAAwABBAkADgA0A7gAQwBvAHAAeQByAGkAZwBoAHQAIAAyADAAMQA0AC0AMgAwADEANwAgAEkAbgBkAGkAYQBuACAAVAB5AHAAZQAgAEYAbwB1AG4AZAByAHkAIAAoAGkAbgBmAG8AQABpAG4AZABpAGEAbgB0AHkAcABlAGYAbwB1AG4AZAByAHkALgBjAG8AbQApACAAdwBpAHQAaAAgAFIAZQBzAGUAcgB2AGUAZAAgAEYAbwBuAHQAIABOAGEAbQBlACAAJwBQAG8AcABwAGkAbgBzACcALgAgAEMAbwBwAHkAcgBpAGcAaAB0ACAAMgAwADEAOQAgAEcAbwBvAGcAbABlACAATABMAEMALgBEAE0AIABTAGEAbgBzAEIAbwBsAGQAMQAuADIAMAAwADsARwBPAE8ARwA7AEQATQBTAGEAbgBzAC0AQgBvAGwAZABEAE0AIABTAGEAbgBzACAAQgBvAGwAZABWAGUAcgBzAGkAbwBuACAAMQAuADIAMAAwADsAIAB0AHQAZgBhAHUAdABvAGgAaQBuAHQAIAAoAHYAMQAuADgALgAzACkARABNAFMAYQBuAHMALQBCAG8AbABkAEMAbwBsAG8AcABoAG8AbgAgAEYAbwB1AG4AZAByAHkAQwBvAGwAbwBwAGgAbwBuACAARgBvAHUAbgBkAHIAeQAsACAASgBvAG4AbgB5ACAAUABpAG4AaABvAHIAbgBoAHQAdABwADoALwAvAHcAdwB3AC4AYwBvAGwAbwBwAGgAbwBuAC0AZgBvAHUAbgBkAHIAeQAuAG8AcgBnAGgAdAB0AHAAcwA6AC8ALwB3AHcAdwAuAGkAbgBkAGkAYQBuAHQAeQBwAGUAZgBvAHUAbgBkAHIAeQAuAGMAbwBtAFQAaABpAHMAIABGAG8AbgB0ACAAUwBvAGYAdAB3AGEAcgBlACAAaQBzACAAbABpAGMAZQBuAHMAZQBkACAAdQBuAGQAZQByACAAdABoAGUAIABTAEkATAAgAE8AcABlAG4AIABGAG8AbgB0ACAATABpAGMAZQBuAHMAZQAsACAAVgBlAHIAcwBpAG8AbgAgADEALgAxAC4AIABUAGgAaQBzACAAbABpAGMAZQBuAHMAZQAgAGkAcwAgAGEAdgBhAGkAbABhAGIAbABlACAAdwBpAHQAaAAgAGEAIABGAEEAUQAgAGEAdAA6ACAAaAB0AHQAcAA6AC8ALwBzAGMAcgBpAHAAdABzAC4AcwBpAGwALgBvAHIAZwAvAE8ARgBMAGgAdAB0AHAAOgAvAC8AcwBjAHIAaQBwAHQAcwAuAHMAaQBsAC4AbwByAGcALwBPAEYATAAAAAIAAAAAAAD/nAAyAAAAAAAAAAAAAAAAAAAAAAAAAAABqAAAACQAyQECAMcAYgCtAQMBBABjAK4AkAEFACUAJgD9AP8AZAEGACcA6QEHAQgAKABlAQkBCgDIAMoBCwDLAQwBDQGzACkAKgD4AbQBEAArACwAzAERAM0AzgD6AM8BEgETARQALQAuAbUALwEWARcBtgEZAOIAMAAxARoBGwG3AGYAMgDQAR0A0QBnANMBHgEfAJEArwCwADMA7QA0ADUBIAEhAbgANgEjAOQA+wG5AboANwEmAbsBvAA4ANQBKQDVAGgA1gEqASsBLAEtAS4AOQA6AS8BMAExATIAOwA8AOsBMwC7ATQBvQA9ATYA5gE3AEQAaQE4AGsAbABqATkBOgBuAG0AoAE7AEUARgD+AQAAbwE8AEcA6gE9AQEASABwAT4BPwByAHMBQABxAUEBQgG+Ab8ASQBKAPkBwAFGAEsATADXAHQBSAB2AHcAdQFJAUoBSwBNAE4BwQBPAU0BTgHCAVAA4wBQAFEBUQFSAcMAeABSAHkBVAB7AHwAegFVAVYAoQB9ALEAUwDuAFQAVQFXAVgBxABWAVoA5QD8AcUAiQBXAVwBxgHHAFgAfgFfAIAAgQB/AWABYQFiAWMBZABZAFoBZQFmAWcBaABbAFwA7AFpALoBagHIAF0BbADnAW0BbgFvAXABcQFyAXMBdAF1AXYBdwF4AXkBegF7AckBfQDAAMEAnQCeAJsAEwAUABUAFgAXABgAGQAaABsAHAF+AX8BgAHKAcsBzAHNALwA9AD1APYAEQAPAB0AHgCrAAQAowAiAKIAwwCHAA0ABgASAD8BggALAAwAXgBgAD4AQAAQAc4AsgCzAEIAxADFALQAtQC2ALcAqQCqAL4AvwAFAAoBhgGHAYgBiQGKAYsAAwHPAY0AhAC9AAcB0ACmAdEB0gCFAJYB0wAOAO8A8AC4ACAAjwAhAB8AlQCUAJMApwBhAKQAQQCSAJwB1AHVAJoAmQClAdYAmAAIAMYAuQAjAAkAiACGAIsAigCMAIMAXwDoAIIB1wDCAZYB2AHZAZkBmgHaAdsB3AHdAd4BoAHfAeAB4QCNANsA4QDeANgAjgDcAEMA3wDaAOAA3QDZAeIB4wGmAacB5AHlAeYB5wHoAa0B6QHqAesBsQGyBkFicmV2ZQdBbWFjcm9uB0FvZ29uZWsHQUVhY3V0ZQpDZG90YWNjZW50BkRjYXJvbgZEY3JvYXQGRWJyZXZlBkVjYXJvbgpFZG90YWNjZW50B0VtYWNyb24HRW9nb25lawZFdGlsZGUMR2NvbW1hYWNjZW50Ckdkb3RhY2NlbnQGSWJyZXZlB0ltYWNyb24HSW9nb25lawZJdGlsZGUMS2NvbW1hYWNjZW50BkxhY3V0ZQZMY2Fyb24MTGNvbW1hYWNjZW50BExkb3QGTmFjdXRlBk5jYXJvbgxOY29tbWFhY2NlbnQGT2JyZXZlDU9odW5nYXJ1bWxhdXQHT21hY3JvbgZSYWN1dGUGUmNhcm9uDFJjb21tYWFjY2VudAZTYWN1dGUMU2NvbW1hYWNjZW50BVNjaHdhBlRjYXJvbghUY2VkaWxsYQxUY29tbWFhY2NlbnQGVWJyZXZlDVVodW5nYXJ1bWxhdXQHVW1hY3JvbgdVb2dvbmVrBVVyaW5nBlV0aWxkZQZXYWN1dGULV2NpcmN1bWZsZXgJV2RpZXJlc2lzBldncmF2ZQtZY2lyY3VtZmxleAZZZ3JhdmUGWXRpbGRlBlphY3V0ZQpaZG90YWNjZW50BmFicmV2ZQdhbWFjcm9uB2FvZ29uZWsHYWVhY3V0ZQpjZG90YWNjZW50BmRjYXJvbgZlYnJldmUGZWNhcm9uCmVkb3RhY2NlbnQHZW1hY3Jvbgdlb2dvbmVrBmV0aWxkZQVzY2h3YQxnY29tbWFhY2NlbnQKZ2RvdGFjY2VudAhpZG90bGVzcwZpYnJldmUHaW1hY3Jvbgdpb2dvbmVrBml0aWxkZQxrY29tbWFhY2NlbnQGbGFjdXRlBmxjYXJvbgxsY29tbWFhY2NlbnQEbGRvdAZuYWN1dGUGbmNhcm9uDG5jb21tYWFjY2VudAZvYnJldmUNb2h1bmdhcnVtbGF1dAdvbWFjcm9uBnJhY3V0ZQZyY2Fyb24McmNvbW1hYWNjZW50BnNhY3V0ZQxzY29tbWFhY2NlbnQGdGNhcm9uCHRjZWRpbGxhDHRjb21tYWFjY2VudAZ1YnJldmUNdWh1bmdhcnVtbGF1dAd1bWFjcm9uB3VvZ29uZWsFdXJpbmcGdXRpbGRlBndhY3V0ZQt3Y2lyY3VtZmxleAl3ZGllcmVzaXMGd2dyYXZlC3ljaXJjdW1mbGV4BnlncmF2ZQZ5dGlsZGUGemFjdXRlCnpkb3RhY2NlbnQGYS5zczAyC2FhY3V0ZS5zczAyC2FicmV2ZS5zczAyEGFjaXJjdW1mbGV4LnNzMDIOYWRpZXJlc2lzLnNzMDILYWdyYXZlLnNzMDIMYW1hY3Jvbi5zczAyDGFvZ29uZWsuc3MwMgphcmluZy5zczAyC2F0aWxkZS5zczAyB2FlLnNzMDIMYWVhY3V0ZS5zczAyBmcuc3MwMwtnYnJldmUuc3MwMxFnY29tbWFhY2NlbnQuc3MwMw9nZG90YWNjZW50LnNzMDMKdGhyZWUuc3MwNAhzaXguc3MwNAluaW5lLnNzMDQMZm91cnN1cGVyaW9yCmNvbW1hLnNzMDEKc29mdGh5cGhlbg1ndWlsbGVtZXRsZWZ0Dmd1aWxsZW1ldHJpZ2h0E3F1b3Rlc2luZ2xiYXNlLnNzMDERcXVvdGVkYmxiYXNlLnNzMDERcXVvdGVkYmxsZWZ0LnNzMDEScXVvdGVkYmxyaWdodC5zczAxDnF1b3RlbGVmdC5zczAxD3F1b3RlcmlnaHQuc3MwMQduYnNwYWNlAkNSBGV1cm8FcnVibGUFcnVwZWUNZGl2aXNpb25zbGFzaANPaG0JaW5jcmVtZW50BW1pY3JvCWxpdGVyU2lnbgllc3RpbWF0ZWQMZGllcmVzaXNjb21iDWRvdGFjY2VudGNvbWIJZ3JhdmVjb21iCWFjdXRlY29tYhBodW5nYXJ1bWxhdXRjb21iDmNpcmN1bWZsZXhjb21iCWNhcm9uY29tYglicmV2ZWNvbWIIcmluZ2NvbWIJdGlsZGVjb21iCm1hY3JvbmNvbWILY2VkaWxsYWNvbWIKb2dvbmVrY29tYhBkaWVyZXNpc2NvbWIuY2FwEWRvdGFjY2VudGNvbWIuY2FwDWdyYXZlY29tYi5jYXANYWN1dGVjb21iLmNhcBRodW5nYXJ1bWxhdXRjb21iLmNhcBJjaXJjdW1mbGV4Y29tYi5jYXANY2Fyb25jb21iLmNhcA1icmV2ZWNvbWIuY2FwDHJpbmdjb21iLmNhcA10aWxkZWNvbWIuY2FwDm1hY3JvbmNvbWIuY2FwD2NlZGlsbGFjb21iLmNhcA5vZ29uZWtjb21iLmNhcAtpbmRpYW5ydXBlZQt0dXJraXNobGlyYQd1bmkxRUJDB3VuaTAxMjIHdW5pMDEzNgd1bmkwMTNCB3VuaTAxNDUHdW5pMDE1Ngd1bmkwMjE4B3VuaTAxOEYHdW5pMDE2Mgd1bmkwMjFBB3VuaTFFRjgHdW5pMUVCRAd1bmkwMjU5B3VuaTAxMjMHdW5pMDEzNwd1bmkwMTNDB3VuaTAxNDYHdW5pMDE1Nwd1bmkwMjE5B3VuaTAxNjMHdW5pMDIxQgd1bmkxRUY5DHVuaTAxMjMuc3MwMwd1bmkwMEI5B3VuaTAwQjIHdW5pMDBCMwd1bmkyMDc0B3VuaTAwQUQHdW5pMDBBMARFdXJvB3VuaTIwQkQHdW5pMjBBOAd1bmkyMjE1B3VuaTIxMjYHdW5pMjIwNgd1bmkwMEI1B3VuaTIxMTMHdW5pMDMwOAd1bmkwMzA3B3VuaTAzMEIHdW5pMDMwMgd1bmkwMzBDB3VuaTAzMDYHdW5pMDMwQQd1bmkwMzA0B3VuaTAzMjcHdW5pMDMyOAt1bmkwMzA4LmNhcAt1bmkwMzA3LmNhcAt1bmkwMzBCLmNhcAt1bmkwMzAyLmNhcAt1bmkwMzBDLmNhcAt1bmkwMzA2LmNhcAt1bmkwMzBBLmNhcAt1bmkwMzA0LmNhcAt1bmkwMzI3LmNhcAt1bmkwMzI4LmNhcAABAAH//wAPAAEAAAAMAAAApgAAAAIAGQABAAwAAQAOACEAAQAjACYAAQAoADEAAQAzADoAAQA8AEoAAQBOAGcAAQBpAG0AAQBvAIQAAQCGAIsAAQCNAJoAAQCcAKoAAQCsALEAAQC1AMEAAQDDAMMAAQDIANAAAQDSANQAAQDWAOAAAQDiAOYAAQDoAQEAAQECAQMAAgFMAUwAAQFOAU8AAQF/AYsAAwGZAaUAAwAIAAIAEAAYAAEAAgECAQMAAQAEAAEBEgABAAQAAQEMAAAAAQAAAAoAPABWAAJERkxUAA5sYXRuABIAGgAAABYAA0NBVCAAFk1PTCAAFlJPTSAAFgAA//8AAgAAAAEAAmtlcm4ADm1hcmsAFAAAAAEAAAAAAAEAAQACAAY+aAACAAgAAgAKM+QAAQJcAAQAAAEpAuoC6gLqAuoC6gLqAuoC6gLqAuoGOgY6A0QwDDAMMAwwDDAMCTwJPAk8CTwGOgY6BjoGOgY6BjoGOgY6BjoGOgY6A8INbA1sDWwNbDPQM9Az0DPQM9Az0DPQM9Az0DPQM9AFMAVqBWoF5gXmBagF5gXmBeYz0DPQM9Az0DPQM9AJPAk8CTwJPAk8CTwJPAk8BiAJPAY6BkgH8ghsCR4JHgkeCR4wAjACMAIwAjACCTwJYgliCWIJYgnQCdAJ0AnQCdAJ0AnQCdAJ0AnQCdAJ4gpMCkwKTApMCkwKvgywDLAMsAywDLAMsA0mDSYNJg0mDTgNOA04DTgNOA04DTgNOA04DTgVFhUWET4NTg1ODU4NTg1OFToNbBEeDXIVFhUWFRYVFhUWFRYVFhUWFRYVFhUWET4NeA6uDq4Org6uESoOuA8+D1AQthEYFToVOhEeFToRJBEqESoRKhEqESoRKhE+ET4RPhE+ET4RPhE+ET4ROBE+FRYRPhE+EXARehF6EXoRejBOME4wTjBOME4RlBJ2EnYSdhJ2EoAT4hPiE+IT4hPiE/wU3hTeFN4U3hTeFN4VDBUMFQwVDBUWFRYVMBUwFTAVMBU6FUAz0BXiF2AYAhh0GV4aKBy+HYAeKh7QH7oiuiGQInIikCK6Iugi9iM4I7YkzCYuKVgvxCsKLKwtri4oLi4w8DDwMPAw8C44LzYvNi9AL24vQC9uL5wvnC+iL6IvxC/EL94v+C/eL/gwAjAMMB4wQDBOMHQwejDwMP4xkDGaMbgxyjHQMs4z0DPQAAIAFwABAKAAAACjAKQAoACoAKgAogCqAKsAowCuALEApQCzANUAqQDhAPEAzAD8AQEA3QEDAQMA4wEHARMA5AEcASQA8QEnAScA+gEpASwA+wEuAT0A/wE/AT8BDwFBAUgBEAFOAVIBGAFUAVQBHQFWAVkBHgFcAV0BIgFrAWsBJAFxAXIBJQF5AXoBJwAWAJv/9wDh/8gBB//YAQj/4gEK/+wBC//xAQz/3gEN/+IBDv/zAQ//5wEQ/90BEf/oARL/6AET/74BI//eASf/oAEq/8QBLf/vAS7/zwFW/+MBWP/mAXH/9wAfAAH/+gAC//oAA//6AAT/+gAF//oABv/6AAf/+gAI//oACf/6AAr/+gAL/+MADP/jAFn/9wBa//cAW//3AFz/9wBo/+sAaf/sAGr/7ABr/+wAbP/sAG3/7ABu/+oAb//iAHD/4gBx/+IAcv/iAHP/4gB0/+IAqAAoARD/9gBbAAH/tAAC/7QAA/+0AAT/tAAF/7QABv+0AAf/tAAI/7QACf+0AAr/tAAy/88Aef/WAHr/1gB7/9YAfP/WAH3/1gB+/9YAf//WAID/1gCB/9YAgv/WAIP/1gCE/9YAhv/mAIf/5gCI/+YAif/mAIr/5gCL/+YAjP/mAI3/5gCO/+YAj//mAJD/5gCR/+YAkv/mAJP/5gCU/+YAlf/mAJb/5gCX/+YAmP/mAJn/5gCa/+YApAAsAKgARgC6/+YAu//mALz/5gC9/+YAvv/mAL//5gDA/+YAwf/mAMP/5gDE/+YAx//mAMz/7ADN/+wAzv/sAM//7ADQ/+wA8v/mAPP/5gD0/+YA9f/mAPb/5gD3/+YA+P/mAPn/5gD6/+YA+//mAPz/5gD9/+YA/v/sAP//7AEA/+wBAf/sAQ//9wER//MBEv/iARz/xAEd/8QBIP/EASv/0gE2/9gBQ//SAUT/0gFY/+wBcf/YAXL/7AAOAAH/4gAC/+IAA//iAAT/4gAF/+IABv/iAAf/4gAI/+IACf/iAAr/4gCnACgAqABCARz/7AEg/+wADwCkADYApwA8AKgAPADh/+IBB//lAQr/7AEL/+YBDf/eAQ//4gEQ/+gBEv/YARP/4QEj//EBVv/OAXL/8gAPAFj/4gBo/9EA4f/FAQf/3gEI/9wBC//vAQz/6QEN/+UBDv/UARD/zwET/88BI//oASf/mwEq/9gBVv/PAA4AWP/iAOH/xQEH/94BCP/cAQv/7wEM/+kBDf/lAQ7/1AEQ/88BE//PASP/6AEn/5sBKv/YAVb/zwAGAKcANgCoAD0BHP/vASD/7wEp/9IBNv/YAAMApAAiAKgAOACqACkAagAB/7UAAv+1AAP/tQAE/7UABf+1AAb/tQAH/7UACP+1AAn/tQAK/7UAC/+LAAz/iwAy/7sAbv/iAG//+gBw//oAcf/6AHL/+gBz//oAdP/6AHX/7AB2/+wAd//sAHj/7AB5/+EAev/hAHv/4QB8/+EAff/hAH7/4QB//+EAgP/hAIH/4QCC/+EAg//hAIT/4QCG/+cAh//nAIj/5wCJ/+cAiv/nAIv/5wCM/+cAjf/nAI7/5wCP/+cAkP/nAJH/5wCS/+cAk//nAJT/5wCV/+cAlv/nAJf/5wCY/+cAmf/nAJr/5wCc/9kAnf/ZAJ7/2QCf/9kAqgAoALr/5wC7/+cAvP/nAL3/5wC+/+cAv//nAMD/5wDB/+cAw//nAMT/5wDH/+cA8v/nAPP/5wD0/+cA9f/nAPb/5wD3/+cA+P/nAPn/5wD6/+cA+//nAPz/5wD9/+cA/v/iAP//4gEA/+IBAf/iAQv/zwEM//cBD//6ARH/4wES/+cBHP+6AR3/pQEg/7oBKf+2ASv/xAEt/+IBNv+7AUP/xAFE/8QBVv/3AXH/2QFy/94AHgAB/9QAAv/UAAP/1AAE/9QABf/UAAb/1AAH/9QACP/UAAn/1AAK/9QAWf/HAFr/xwBb/8cAXP/HAGj/1ABp/9gAav/YAGv/2ABs/9gAbf/YAG7/ugBv/8AAcP/AAHH/wABy/8AAc//AAHT/wAEc/88BIP/PAS3/3gAsAAH/8wAC//MAA//zAAT/8wAF//MABv/zAAf/8wAI//MACf/zAAr/8wBZ/+8AWv/vAFv/7wBc/+8AaP/pAGn/7wBq/+8Aa//vAGz/7wBt/+8Abv/vAG//2QBw/9kAcf/ZAHL/2QBz/9kAdP/ZANYACwDXAAsA2AALANkACwDaAAsA2wALANwACwDdAAsA3gALAN8ACwDgAAsA4gALAOMACwDkAAsA5QALAOYACwFx//MABwCoACIAqgAUAQv/6AEM/+wBD//3ARH/+wFW//cACQBu/80BCf/3AQ7/4gEd/9gBKf/SASr/8wEt/8UBL//cATb/6wAbADL/pQCoAEYAwv+mAMX/0wDh/8QA5//ZAQf/4wEJ//cBCv/1AQv/xAEP/+8BEP/3ARH/9QES/84BHf+wAR//yAEi/9kBIwAUAST/pwEp/74BKgARAS7/4gE2/8QBVv/KAVj/xAFx/6cBcv/OAAQAqAA4AKoAFAEp/+IBNv/YABoAMv/EAKQAMgCoAEIA4f/3AQf/3gEJ//MBCv/3AQv/twEM//MBDf/vAQ4AFAEP/98BEP/qARH/9wES/8UBE//3AR3/tAEe/+IBIv/cASn/tgEqABcBNv+iAVb/zwFY/8ABcf+gAXL/xAAcADL/2ABJ/+wApAAoAKgATACqACgAwv/LAOH/9wEH/9gBCf/zAQr/6wEL/84BDP/vAQ3/2AEP/9gBEP/lARH/8wES/84BE//sAR3/xAEf/9wBIv/iASn/xAEu/+IBNv+6AVb/zwFY/+sBcf+wAXL/zgB8AA7/zQAP/80AEP/NABH/zQAS/80AI//NACT/zQAl/80AJv/NAEH/zQBC/80AQ//NAET/zQBF/80ARv/NAEf/zQBI/80ASv/NAEv/zQBO/80Aef/mAHr/5gB7/+YAfP/mAH3/5gB+/+YAf//mAID/5gCB/+YAgv/mAIP/5gCE/+YAhv/cAIf/3ACI/9wAif/cAIr/3ACL/9wAjP/cAI3/3ACO/9wAj//cAJD/3ACR/9wAkv/cAJP/3ACU/9wAlf/cAJb/3ACX/9wAmP/cAJn/3ACa/9wApAAeAKgAMgCqABQAuv/cALv/3AC8/9wAvf/cAL7/3AC//9wAwP/cAMH/3ADD/9wAxP/cAMf/3ADM/+4Azf/uAM7/7gDP/+4A0P/uANb/4QDX/+EA2P/hANn/4QDa/+EA2//hANz/4QDd/+EA3v/hAN//4QDg/+EA4f/iAOL/4gDj/+IA5P/iAOX/4gDm/+IA6P/mAOn/5gDq/+YA6//mAOz/5gDt/+YA8v/cAPP/3AD0/9wA9f/cAPb/3AD3/9wA+P/cAPn/3AD6/9wA+//cAPz/3AD9/9wA/v/iAP//4gEA/+IBAf/iAQf/0gEK/+wBC//OAQz/0gEN/94BD//YARD/2AER/+IBEv/cARP/2QEj/+YBWP/eAXH/8wAdADL/qgBJ/9MApAAoAKgARwCqABQA4f/cAQf/2AEJ//MBCv/ZAQv/qQEM/9kBDf/UAQ//1AEQ/94BEf/3ARL/rwET/+gBHf+zAR7/3AEf/8sBIv/cAST/rAEp/74BLv/OATb/ugFW/7QBWP/EAXH/kgFy/7oABACkACIAqABCAKoAJAEL//MABQDh//EBDv/zARP/9QEn/9cBKv/SAAcA4f/vAQ7/9wET/+YBI//iASf/4QEq/80BLf/vAAEAqAAeAAEAqABWAE0Aef/sAHr/7AB7/+wAfP/sAH3/7AB+/+wAf//sAID/7ACB/+wAgv/sAIP/7ACE/+wAhv/sAIf/7ACI/+wAif/sAIr/7ACL/+wAjP/sAI3/7ACO/+wAj//sAJD/7ACR/+wAkv/sAJP/7ACU/+wAlf/sAJb/7ACX/+wAmP/sAJn/7ACa/+wAnP/YAJ3/2ACe/9gAn//YAKQAHgCnACgAqAA8AKoAKQC6/+wAu//sALz/7AC9/+wAvv/sAL//7ADA/+wAwf/sAMP/7ADE/+wAx//sAMz/7wDN/+8Azv/vAM//7wDQ/+8A8v/sAPP/7AD0/+wA9f/sAPb/7AD3/+wA+P/sAPn/7AD6/+wA+//sAPz/7AD9/+wA/v/xAP//8QEA//EBAf/xAQv/7AEc/+IBIP/iAXH/4wACAKsAFADhAAsAIQCFADwAmwAoAKAAPACmAFAAqAA1AKoALgCsADwArQA8AK4APACvADwAsAA8ALEAPACyADwAswAxAREAOAEhACwBIwA7AScAGgEqAEwBLQAiAS8AEQEwADwBMQAiATkAJwE6ADkBOwAnATwAOQFBAAkBQgAJAUUAUwFGAFMBRwBTAUgAUwAEAQgAKAEOADYBIwAkASoAPABZAA0AKAATACgAFQAoABcAKAAYACgAGQAoABoAKAAbACgAHAAoAB0AKAAeACgAHwAoACAAKAAhACgAIgAoACcAKAAoACgAKQAoACoAKAArACgALAAoAC0AKAAuACgALwAoADEAKAAzACgANAAoADUAKAA2ACgANwAoADgAKAA5ACgAOwAoADwAKAA9ACgAPgAoAD8AKABAACgATAAoAE0AKABPACgAUAAoAFEAKABSACgAhQA8AJsAEQCgADwArAA8AK0APACuADwArwA8ALAAPACxADwAsgA8ALMAPADGADwA0QA8AO4ALgDvAC4A8AAuAPEALgEHACIBCABWAQkAIgEKACQBDAAUAQ8AHgEQACkBEQAoARMANgEhADwBIwBHAScARQEqAFEBMAA8ATEAIgE5AEABOgAxATsAQAE8ADEBQQBkAUIAZAFFAGQBRgBkAUcAZAFIAGQBUgAoAXkAKAF6ACgAGACFABcAmwAeAKAAFwCrACQArAAXAK0AFwCuABcArwAXALAAFwCxABcAsgAXASMAKAEnABEBMAAfATkACQE6AB0BOwAJATwAHQFBAEIBQgBCAUUAPAFGADwBRwA8AUgAPAABAKoAJAABAST/xwABAKgAPAADAOH/+wEn/+EBKv/YAAEA4f/1AAwA4f/oAOf/5gEI/9gBDv/AARD/9wET/+wBI//iASf/0QEp//ABKv+2AS3/0gEv/84AAgCrAB8BKv/eAAYAwv/3AQkAHQEKAAsBEwAeAR3/3AEeAAkAOACG//oAh//6AIj/+gCJ//oAiv/6AIv/+gCM//oAjf/6AI7/+gCP//oAkP/6AJH/+gCS//oAk//6AJT/+gCV//oAlv/6AJf/+gCY//oAmf/6AJr/+gC6//oAu//6ALz/+gC9//oAvv/6AL//+gDA//oAwf/6AMP/+gDE//oAx//6AOH/1ADo/98A6f/fAOr/3wDr/98A7P/fAO3/3wDy//oA8//6APT/+gD1//oA9v/6APf/+gD4//oA+f/6APr/+gD7//oA/P/6AP3/+gEO/94BE//UASf/xAFF/84BR//OAAIBCQAeAR0AGgBYAHn/4gB6/+IAe//iAHz/4gB9/+IAfv/iAH//4gCA/+IAgf/iAIL/4gCD/+IAhP/iAIb/6ACH/+gAiP/oAIn/6ACK/+gAi//oAIz/6ACN/+gAjv/oAI//6ACQ/+gAkf/oAJL/6ACT/+gAlP/oAJX/6ACW/+gAl//oAJj/6ACZ/+gAmv/oAJsAFACc/+YAnf/mAJ7/5gCf/+YAuv/oALv/6AC8/+gAvf/oAL7/6AC//+gAwP/oAMH/6ADC//UAw//oAMT/6ADH/+gAzP/tAM3/7QDO/+0Az//tAND/7QDSAAsA0wALANQACwDVAAsA8v/oAPP/6AD0/+gA9f/oAPb/6AD3/+gA+P/oAPn/6AD6/+gA+//oAPz/6AD9/+gA/v/iAP//4gEA/+IBAf/iAQkACwEL/+wBEf/1ARL/7AEc/9gBHf/OASD/2AEk/9kBK//XAUP/1wFE/9cBcf/UAXL/7wAGAQv/9wEP//cBEf/3AR3/4gEk//MBcf/YADgAef/1AHr/9QB7//UAfP/1AH3/9QB+//UAf//1AID/9QCB//UAgv/1AIP/9QCE//UAhv/mAIf/5gCI/+YAif/mAIr/5gCL/+YAjP/mAI3/5gCO/+YAj//mAJD/5gCR/+YAkv/mAJP/5gCU/+YAlf/mAJb/5gCX/+YAmP/mAJn/5gCa/+YAuv/mALv/5gC8/+YAvf/mAL7/5gC//+YAwP/mAMH/5gDD/+YAxP/mAMf/5gDy/+YA8//mAPT/5gD1/+YA9v/mAPf/5gD4/+YA+f/mAPr/5gD7/+YA/P/mAP3/5gALAML/7AEL/+IBDgARARH/7wES/+wBHf++AST/ygEp/9gBWP/3AXH/ygFy//UAAgCoACgBC//zAAYA4f/sAOf/3gEO/94BI//sASf/4QEq/8gAAgEO//MBKv/YAAEAqAAsACgAAf/YAAL/2AAD/9gABP/YAAX/2AAG/9gAB//YAAj/2AAJ/9gACv/YAAv/3wAM/98AWf/jAFr/4wBb/+MAXP/jAGj/3gBp/9gAav/YAGv/2ABs/9gAbf/YAG7/0gBv/9gAcP/YAHH/2ABy/9gAc//YAHT/2ACoACIBCf/7AQ7/+QER//cBHP/YAR3/4gEg/9gBKf/YASr/3gEt/94BNv/EAF8ADv/7AA//+wAQ//sAEf/7ABL/+wAj//sAJP/7ACX/+wAm//sAQf/7AEL/+wBD//sARP/7AEX/+wBG//sAR//7AEj/+wBK//sAS//7AE7/+wBZ//UAWv/1AFv/9QBc//UAaP/vAGn/8wBq//MAa//zAGz/8wBt//MAb//rAHD/6wBx/+sAcv/rAHP/6wB0/+sAhv/3AIf/9wCI//cAif/3AIr/9wCL//cAjP/3AI3/9wCO//cAj//3AJD/9wCR//cAkv/3AJP/9wCU//cAlf/3AJb/9wCX//cAmP/3AJn/9wCa//cAmwALAKgAHQC6//cAu//3ALz/9wC9//cAvv/3AL//9wDA//cAwf/3AMP/9wDE//cAx//3AOEACwDnAAsA8v/3APP/9wD0//cA9f/3APb/9wD3//cA+P/3APn/9wD6//cA+//3APz/9wD9//cBB//7AQv/9wEO//cBD//3ARD/+wES//MBKAARAVAAHwFW//MBWP/3AVn/8wAoAAH/3QAC/90AA//dAAT/3QAF/90ABv/dAAf/3QAI/90ACf/dAAr/3QAL/+gADP/oAFn/7wBa/+8AW//vAFz/7wBo/+MAaf/dAGr/3QBr/90AbP/dAG3/3QBu/9gAb//UAHD/1ABx/9QAcv/UAHP/1AB0/9QAqAAaAQj/9wEO//cBEP/oARP/8wEn/+wBKf/ZASr/3gFu//cBb//3AXH/6AAcAFn/2ABa/9gAW//YAFz/2ABo/+MAaf/mAGr/5gBr/+YAbP/mAG3/5gBv/+IAcP/iAHH/4gBy/+IAc//iAHT/4gDo/+YA6f/mAOr/5gDr/+YA7P/mAO3/5gET/+YBJ//NAWsAHgFu/+8Bb//vAXIAIgA6AAH/1wAC/9cAA//XAAT/1wAF/9cABv/XAAf/1wAI/9cACf/XAAr/1wAL/+8ADP/vAFn/9wBa//cAW//3AFz/9wBo/+8Aaf/vAGr/7wBr/+8AbP/vAG3/7wBu/9gAb//fAHD/3wBx/98Acv/fAHP/3wB0/98AnP/1AJ3/9QCe//UAn//1AKgAFADi//cA4//3AOT/9wDl//cA5v/3AOj/9wDp//cA6v/3AOv/9wDs//cA7f/3AQn/9wEO//cBEP/oARP/8wEn/9wBKAARASn/4gE2/9QBOv/mATz/5gFB//cBQv/3AXH/8wAyAAH/3gAC/94AA//eAAT/3gAF/94ABv/eAAf/3gAI/94ACf/eAAr/3gAL/9kADP/ZAFn/7wBa/+8AW//vAFz/7wBo/+8Aaf/YAGr/2ABr/9gAbP/YAG3/2ABu/9UAb//KAHD/ygBx/8oAcv/KAHP/ygB0/8oAnP/3AJ3/9wCe//cAn//3AKgAFAEJ//MBDP/sAQ7/8wEQ/+wBEf/3ARP/7wEc/94BIP/eASf/3AEp/94BKv/tAS3/4gE2/9gBbv/3AW//9wFx//MApQAB/7EAAv+xAAP/sQAE/7EABf+xAAb/sQAH/7EACP+xAAn/sQAK/7EADv/zAA//8wAQ//MAEf/zABL/8wAj//MAJP/zACX/8wAm//MAMv/YAEH/8wBC//MAQ//zAET/8wBF//MARv/zAEf/8wBI//MASf/tAEr/8wBL//MATv/zAGgAFABpAAsAagALAGsACwBsAAsAbQALAHn/7wB6/+8Ae//vAHz/7wB9/+8Afv/vAH//7wCA/+8Agf/vAIL/7wCD/+8AhP/vAIb/zwCH/88AiP/PAIn/zwCK/88Ai//PAIz/zwCN/88Ajv/PAI//zwCQ/88Akf/PAJL/zwCT/88AlP/PAJX/zwCW/88Al//PAJj/zwCZ/88Amv/PAJsACwCc/8cAnf/HAJ7/xwCf/8cAov/zAKP/8wCkACkApf/zAKgATACp//MAqgAiALT/8wC1//MAtv/zALf/8wC4//MAuf/zALr/zwC7/88AvP/PAL3/zwC+/88Av//PAMD/zwDB/88Aw//PAMT/zwDF/+8Ax//PAMj/8wDJ//MAyv/zAMv/8wDM/9cAzf/XAM7/1wDP/9cA0P/XAPL/zwDz/88A9P/PAPX/zwD2/88A9//PAPj/zwD5/88A+v/PAPv/zwD8/88A/f/PAP7/xQD//8UBAP/FAQH/xQEH//kBCAARAQn/8wEK//MBC//KAQz/6AEN//MBD//3ARD/8wER//YBEv/dARz/xAEd/84BH//jASD/xAEk/8sBKf+xASv/wQEy/+8BM//vATT/7wE1/+8BNv/PATf/xAE4/8QBPf/UAT//1AFBAB4BQgAeAUP/wQFE/8EBTP/eAVb/8wFX/+8BWP/zAVn/7wFd/+MBcf+vAXL/2QAwAAH/5wAC/+cAA//nAAT/5wAF/+cABv/nAAf/5wAI/+cACf/nAAr/5wAL/+gADP/oAFn/7wBa/+8AW//vAFz/7wBo/98Aaf/YAGr/2ABr/9gAbP/YAG3/2ABu/9gAb//UAHD/1ABx/9QAcv/UAHP/1AB0/9QAqAAeAOL/9wDj//cA5P/3AOX/9wDm//cBCP/sAQn/9wEO//MBEP/oARP/4wEn/+IBKf/vASr/3gEt/9IBNv/NATr/7AE8/+wBcf/zACoAAf/RAAL/0QAD/9EABP/RAAX/0QAG/9EAB//RAAj/0QAJ/9EACv/RAAv/1AAM/9QAOgAfAFn/7wBa/+8AW//vAFz/7wBo/+IAaf/bAGr/2wBr/9sAbP/bAG3/2wBu/8gAb//OAHD/zgBx/84Acv/OAHP/zgB0/84AqAAeAQj/9wEJ//cBCv/3AQ7/8wEP//cBHP/iASD/4gEp/84BKv/iAS3/1wE2/84AKQAB/94AAv/eAAP/3gAE/94ABf/eAAb/3gAH/94ACP/eAAn/3gAK/94AC//cAAz/3ABo//cAaf/3AGr/9wBr//cAbP/3AG3/9wBu/9gApwAiAKgAMgDh/+gA4v/oAOP/6ADk/+gA5f/oAOb/6ADo/+8A6f/vAOr/7wDr/+8A7P/vAO3/7wEJ//cBE//iASf/3AEp/+8BNv/iAVz/9QFd/+8Bcf/eADoAAf/zAAL/8wAD//MABP/zAAX/8wAG//MAB//zAAj/8wAJ//MACv/zADoAGABZ/+MAWv/jAFv/4wBc/+MAaP/PAGn/ygBq/8oAa//KAGz/ygBt/8oAbv/UAG//wABw/8AAcf/AAHL/wABz/8AAdP/AAOH/3gDi//cA4//3AOT/9wDl//cA5v/3AOj/2ADp/9gA6v/YAOv/2ADs/9gA7f/YAQn/7wEO/+8BEP/3ARP/3wEj/+IBJ/+3ASn/7wEq/84BLf/vATb/3AE5/+wBOv/eATv/7AE8/94BQf/EAUL/xAFu/9gBb//YAHUAAf+4AAL/uAAD/7gABP+4AAX/uAAG/7gAB/+4AAj/uAAJ/7gACv+4AAv/pQAM/6UAMv/UAGj/9wBp/+wAav/sAGv/7ABs/+wAbf/sAG7/4gBv/+8AcP/vAHH/7wBy/+8Ac//vAHT/7wB1/+YAdv/mAHf/5gB4/+YAef/mAHr/5gB7/+YAfP/mAH3/5gB+/+YAf//mAID/5gCB/+YAgv/mAIP/5gCE/+YAhv/cAIf/3ACI/9wAif/cAIr/3ACL/9wAjP/cAI3/3ACO/9wAj//cAJD/3ACR/9wAkv/cAJP/3ACU/9wAlf/cAJb/3ACX/9wAmP/cAJn/3ACa/9wAnP/hAJ3/4QCe/+EAn//hAKgAHgC6/9wAu//cALz/3AC9/9wAvv/cAL//3ADA/9wAwf/cAMP/3ADE/9wAx//cAMz//ADN//wAzv/8AM///ADQ//wA8v/cAPP/3AD0/9wA9f/cAPb/3AD3/9wA+P/cAPn/3AD6/9wA+//cAPz/3AD9/9wBCf/5AQr/7AEL/+gBDP/sAQ//7AER/9gBEv/dARz/sQEd/8MBIP+xAST/uwEp/7QBKv/mASv/xAEt/80BNv+bATf/2AE4/9gBQ//EAUT/xAFx/8QAOAAO/+wAD//sABD/7AAR/+wAEv/sACP/7AAk/+wAJf/sACb/7ABB/+wAQv/sAEP/7ABE/+wARf/sAEb/7ABH/+wASP/sAEr/7ABL/+wATv/sAFn/sABa/7AAW/+wAFz/sABo/7QAaf/IAGr/yABr/8gAbP/IAG3/yABv/6kAcP+pAHH/qQBy/6kAc/+pAHT/qQDS/+IA0//iANT/4gDV/+IA4f/UAOL/4gDj/+IA5P/iAOX/4gDm/+IA6P/IAOn/yADq/8gA6//IAOz/yADt/8gBE//EASr/wwFu/8cBb//HAAcAaP/iAG//3ABw/9wAcf/cAHL/3ABz/9wAdP/cAAoAWf/UAFr/1ABb/9QAXP/UAG//9QBw//UAcf/1AHL/9QBz//UAdP/1AAsA4f/YAQf/2AEI/+EBDf/iAQ7/4gEQ/+IBE/+5ASP/3gEq/8QBLv/OAV3/xAADAKcALACoADwBJ//pABAAWf/ZAFr/2QBb/9kAXP/ZAGj/3ABp/+IAav/iAGv/4gBs/+IAbf/iAG//3ABw/9wAcf/cAHL/3ABz/9wAdP/cAB8AAf+6AAL/ugAD/7oABP+6AAX/ugAG/7oAB/+6AAj/ugAJ/7oACv+6AAv/ogAM/6IAMv/UAG7/7ACkACQApwAoAKgAPACqAC4BHP+gAR3/ugEg/6ABJP+SASn/tAEr/9IBLf+9ATb/pgE3/9gBOP/YAUP/0gFE/9IBcf/PAEUAWf/KAFr/ygBb/8oAXP/KAG//ygBw/8oAcf/KAHL/ygBz/8oAdP/KAIb/7ACH/+wAiP/sAIn/7ACK/+wAi//sAIz/7ACN/+wAjv/sAI//7ACQ/+wAkf/sAJL/7ACT/+wAlP/sAJX/7ACW/+wAl//sAJj/7ACZ/+wAmv/sALr/7AC7/+wAvP/sAL3/7AC+/+wAv//sAMD/7ADB/+wAw//sAMT/7ADH/+wA4f/zAO4AFADvABQA8AAUAPEAFADy/+wA8//sAPT/7AD1/+wA9v/sAPf/7AD4/+wA+f/sAPr/7AD7/+wA/P/sAP3/7AEN/94BDv/zARD/8wET/8oBI//eASr/3AE3ACIBOAAiAW7/5wFv/+cAWAAB/6AAAv+gAAP/oAAE/6AABf+gAAb/oAAH/6AACP+gAAn/oAAK/6AAC/+HAAz/hwAy/8AAef/lAHr/5QB7/+UAfP/lAH3/5QB+/+UAf//lAID/5QCB/+UAgv/lAIP/5QCE/+UAhv/hAIf/4QCI/+EAif/hAIr/4QCL/+EAjP/hAI3/4QCO/+EAj//hAJD/4QCR/+EAkv/hAJP/4QCU/+EAlf/hAJb/4QCX/+EAmP/hAJn/4QCa/+EAqABFAKoAFwC6/+EAu//hALz/4QC9/+EAvv/hAL//4QDA/+EAwf/hAMP/4QDE/+EAx//hAMz/6ADN/+gAzv/oAM//6ADQ/+gA8v/hAPP/4QD0/+EA9f/hAPb/4QD3/+EA+P/hAPn/4QD6/+EA+//hAPz/4QD9/+EBC/+cAQz/1wEN/9wBD//iARH/wAES/8EBIf/pAST/pgEp/4sBNv+tAXH/yAFy/9IAygANADEADv/iAA//4gAQ/+IAEf/iABL/4gATADEAFQAxABcAMQAYADEAGQAxABoAMQAbADEAHAAxAB0AMQAeADEAHwAxACAAMQAhADEAIgAxACP/4gAk/+IAJf/iACb/4gAnADEAKAAxACkAMQAqADEAKwA8ACwAMwAtADEALgAxAC8ARwAxADEAMv/YADMAMQA0ADEANQAxADYAMQA3ADEAOAAxADkAMQA7ADEAPAAxAD0AMQA+ADEAPwAxAEAAMQBB/+IAQv/iAEP/4gBE/+IARf/iAEb/4gBH/+IASP/iAEn/4gBK/+IAS//iAEwAMQBNADEATv/iAE8AMQBQADEAUQAxAFIAMQBZABQAWgAUAFsAFABcABQAaAAeAGkACwBqAAsAawALAGwACwBtAAsAbwAaAHAAGgBxABoAcgAaAHMAGgB0ABoAef/IAHr/yAB7/8gAfP/IAH3/yAB+/8gAf//IAID/yACB/8gAgv/IAIP/yACE/8gAhv/PAIf/zwCI/88Aif/PAIr/zwCL/88AjP/PAI3/zwCO/88Aj//PAJD/zwCR/88Akv/PAJP/zwCU/88Alf/PAJb/zwCX/88AmP/PAJn/zwCa/88AnP/XAJ3/1wCe/9cAn//XAKL/7QCj/+0ApAAoAKUAIwCmACUApwBGAKgAUQCp/+0AtP/tALX/7QC2/+0At//tALj/7QC5/+0Auv/PALv/zwC8/88Avf/PAL7/zwC//88AwP/PAMH/zwDD/88AxP/PAMf/zwDI/+0Ayf/tAMr/7QDL/+0AzP/iAM3/4gDO/+IAz//iAND/4gDy/88A8//PAPT/zwD1/88A9v/PAPf/zwD4/88A+f/PAPr/zwD7/88A/P/PAP3/zwD+/8gA///IAQD/yAEB/8gBB//sAQr/3gEL/8ABDP/iAQ3/6AEP/+8BEP/eARH/8wES/+YBE//oARz/iQEd/8QBIP+JAST/vgEn/+kBKf+IASv/pwEy/8cBM//HATT/xwE1/8cBNv9wATf/rAE4/6wBQ/+nAUT/pwFSADEBVv/PAVf/xwFx/60Bcv/OAXkAMQF6ADEAbAAO//cAD//3ABD/9wAR//cAEv/3ACP/9wAk//cAJf/3ACb/9wBB//cAQv/3AEP/9wBE//cARf/3AEb/9wBH//cASP/3AEr/9wBL//cATv/3AFn/4wBa/+MAW//jAFz/4wBo/8QAaf/PAGr/zwBr/88AbP/PAG3/zwBv/7oAcP+6AHH/ugBy/7oAc/+6AHT/ugCG/+UAh//lAIj/5QCJ/+UAiv/lAIv/5QCM/+UAjf/lAI7/5QCP/+UAkP/lAJH/5QCS/+UAk//lAJT/5QCV/+UAlv/lAJf/5QCY/+UAmf/lAJr/5QC6/+UAu//lALz/5QC9/+UAvv/lAL//5QDA/+UAwf/lAMP/5QDE/+UAx//lAPL/5QDz/+UA9P/lAPX/5QD2/+UA9//lAPj/5QD5/+UA+v/lAPv/5QD8/+UA/f/lAQf/6AEN/9wBDv/zAQ//3gEQ/+IBEv/1ARP/xQEdADEBI//iASr/fwEy/+IBM//iATT/4gE1/+IBNgAuATcAJwE4ACcBOf+7ATr/ugE7/7sBPP+6AUH/oAFC/6ABRf+6AUb/ugFH/7oBSP+6AVf/4gBoAAH/7wAC/+8AA//vAAT/7wAF/+8ABv/vAAf/7wAI/+8ACf/vAAr/7wAO/8UAD//FABD/xQAR/8UAEv/FACP/xQAk/8UAJf/FACb/xQBB/8UAQv/FAEP/xQBE/8UARf/FAEb/xQBH/8UASP/FAEr/xQBL/8UATv/FAHn/0gB6/9IAe//SAHz/0gB9/9IAfv/SAH//0gCA/9IAgf/SAIL/0gCD/9IAhP/SAIb/0gCH/9IAiP/SAIn/0gCK/9IAi//SAIz/0gCN/9IAjv/SAI//0gCQ/9IAkf/SAJL/0gCT/9IAlP/SAJX/0gCW/9IAl//SAJj/0gCZ/9IAmv/SALr/0gC7/9IAvP/SAL3/0gC+/9IAv//SAMD/0gDB/9IAw//SAMT/0gDH/9IAzP/sAM3/7ADO/+wAz//sAND/7ADS//cA0//3ANT/9wDV//cA8v/SAPP/0gD0/9IA9f/SAPb/0gD3/9IA+P/SAPn/0gD6/9IA+//SAPz/0gD9/9IBB//eAQv/3gEN/+IBD//SARD/3AES/9gBE//IAXH/3gFy/9gAQAAO/9wAD//cABD/3AAR/9wAEv/cACP/3AAk/9wAJf/cACb/3ABB/9wAQv/cAEP/3ABE/9wARf/cAEb/3ABH/9wASP/cAEr/3ABL/9wATv/cAIb/zgCH/84AiP/OAIn/zgCK/84Ai//OAIz/zgCN/84Ajv/OAI//zgCQ/84Akf/OAJL/zgCT/84AlP/OAJX/zgCW/84Al//OAJj/zgCZ/84Amv/OALr/zgC7/84AvP/OAL3/zgC+/84Av//OAMD/zgDB/84Aw//OAMT/zgDH/84A8v/OAPP/zgD0/84A9f/OAPb/zgD3/84A+P/OAPn/zgD6/84A+//O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AAEAAAAEAAEAAgGKAYsAAgABAX8BiQAAAAEAAAABAAgAAQAGAAEAAQABAKEAAQAAAAEACAABAAYAAQABAAQAVgBbAM8A1AAGAAAAAgAKAB4AAwAAAAIAPgAoAAEAPgABAAAACAADAAAAAgBKABQAAQBKAAEAAAAJAAEAAQElAAQAAAABAAgAAQAIAAEADgABAAEArgABAAQAsgACASUABAAAAAEACAABAAgAAQAOAAEAAQA1AAEABAA5AAIBJQABAAAAAQAIAAEABgAMAAIAAQEIAQsAAAAEAAAAAQAIAAEALAACAAoAIAACAAYADgEaAAMBKQELARkAAwEpAQkAAQAEARsAAwEpAQsAAQACAQgBCgAGAAAAAgAKACQAAwABACwAAQASAAAAAQAAAA0AAQACAAEAeQADAAEAEgABABwAAAABAAAADQACAAEBBwEQAAAAAQACAEEAugABAAAAAQAIAAIADgAEAQQBBQEEAQUAAQAEAAEAQQB5ALoABAAAAAEACAABABoAAQAIAAIABgAMAQIAAgChAQMAAgCuAAEAAQCbAAEAAAABAAgAAgAUAAcBKwFDAUQBRQFGAUcBSAACAAIBHQEdAAABNwE8AAEAAQAAAAEACAABAAYAeQACAAEAeQCEAAAAAQAAAAEACAABAAYAYgACAAEAnACfAAAAAQAAAAEACAACAAwAAwERARIBEwABAAMBCgENARAAAAAAAAEAAAAA");

        private PrivateFontCollection _pfc;
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
            // Load DM Sans Bold (Veeam geometric sans) for word drops and popups
            if (_pfc != null) { _pfc.Dispose(); _pfc = null; }
            _pfc = new PrivateFontCollection();
            GCHandle fh = GCHandle.Alloc(DMSANS_BOLD, GCHandleType.Pinned);
            try   { _pfc.AddMemoryFont(fh.AddrOfPinnedObject(), DMSANS_BOLD.Length); }
            finally { fh.Free(); }
            FontFamily wff = _pfc.Families.Length > 0 ? _pfc.Families[0] : FontFamily.GenericSansSerif;
            wordFont    = new Font(wff, Math.Max(6, s.WordFontSize - 1), FontStyle.Bold, GraphicsUnit.Pixel);
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
                    FontFamily wmff = _pfc != null && _pfc.Families.Length > 0 ? _pfc.Families[0] : FontFamily.GenericSansSerif;
                    using (Font lf = new Font(wmff, lsz, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (SolidBrush lb = new SolidBrush(Color.FromArgb(10, s.RainColor)))
                    { SizeF ls=g.MeasureString(wmText,lf); g.DrawString(wmText,lf,lb,(W-ls.Width)/2f,(H-ls.Height)/2f); }
                    if (wmSub.Length > 0)
                    {
                        int ssz = Math.Max(7,(int)(W*0.015));
                        using (Font sf = new Font(wmff, ssz, FontStyle.Regular, GraphicsUnit.Pixel))
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
            if(_pfc       !=null){_pfc.Dispose();       _pfc       =null;}
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
        private Settings cur;
        private Button   btnRainColor, btnHeadColor, btnWordColor, btnWordHeadColor, btnPopupColor;
        private TrackBar trkFade, trkFont, trkSpeed, trkWordCount, trkWordFont, trkPopupCount, trkPopupFont;
        private Label    lblFade, lblFont, lblSpeed, lblWCount, lblWFont, lblPCount, lblPFont;
        private ComboBox cboOrient, cboWordOrient, cboWordMode, cboWordStyle;
        private TrackBar trkWordSpeed;
        private Label    lblWordSpeed;
        private TextBox  txtWatermark, txtWatermarkSub;
        private CheckBox chkFade, chkFlash, chkGlitch, chkScan, chkZoom;
        private CheckBox chkScanlines, chkWatermark, chkVeeam100;
        private TextBox  txtExtra;
        private ComboBox cboProfiles;
        private List<ColorProfile> profiles;

        public Settings Result { get; private set; }

        public ConfigForm(Settings s)
        {
            cur=Clone(s); Text="Veeam Matrix – Einstellungen"; FormBorderStyle=FormBorderStyle.FixedDialog;
            MaximizeBox=false; MinimizeBox=false; ClientSize=new Size(540,100);
            BackColor=Color.FromArgb(18,18,18); ForeColor=Color.FromArgb(0,200,55);
            Font=new Font("Segoe UI",9f);
            profiles=ColorProfile.LoadAll();
            Build();
        }

        private static Settings Clone(Settings s)
        {
            return new Settings { RainColor=s.RainColor, HeadColor=s.HeadColor, FadeAlpha=s.FadeAlpha, FontSize=s.FontSize,
                SpeedFactor=s.SpeedFactor, ShowScanlines=s.ShowScanlines, ShowWatermark=s.ShowWatermark,
                WordMode=s.WordMode, WordCount=s.WordCount, WordFontSize=s.WordFontSize,
                WordColor=s.WordColor, WordHeadColor=s.WordHeadColor, GlowChance=s.GlowChance,
                PopupEffects=s.PopupEffects, PopupCount=s.PopupCount, PopupFontSize=s.PopupFontSize,
                PopupColor=s.PopupColor, Orientation=s.Orientation, WordOrientation=s.WordOrientation,
                WordStyle=s.WordStyle, WordSpeedFactor=s.WordSpeedFactor, ShowVeeam100=s.ShowVeeam100,
                WatermarkText=s.WatermarkText, WatermarkSubText=s.WatermarkSubText, ExtraWords=s.ExtraWords };
        }

        private Label    Lbl(string t,int x,int y){var l=new Label{Text=t,Location=new Point(x,y),AutoSize=true,ForeColor=Color.FromArgb(0,200,55)};Controls.Add(l);return l;}
        private void     Sep(int y){Controls.Add(new Panel{Location=new Point(14,y),Size=new Size(512,1),BackColor=Color.FromArgb(0,80,25)});}
        private TrackBar Trk(int x,int y,int w,int min,int max,int val){var t=new TrackBar{Location=new Point(x,y),Size=new Size(w,36),Minimum=min,Maximum=max,Value=Clamp(val,min,max),TickFrequency=Math.Max(1,(max-min)/10),SmallChange=1,BackColor=Color.FromArgb(18,18,18)};Controls.Add(t);return t;}
        private Button   ColBtn(string text,Color col,int x,int y){var b=new Button{Text=text,Location=new Point(x,y),Size=new Size(120,26),BackColor=col,ForeColor=col.GetBrightness()>.45f?Color.Black:Color.White,FlatStyle=FlatStyle.Flat};b.FlatAppearance.BorderColor=Color.FromArgb(0,160,45);Controls.Add(b);return b;}
        private ComboBox Combo(int x,int y,int w,string[] items,string sel){var c=new ComboBox{Location=new Point(x,y),Size=new Size(w,24),DropDownStyle=ComboBoxStyle.DropDownList,BackColor=Color.FromArgb(30,30,30),ForeColor=Color.FromArgb(0,200,55)};c.Items.AddRange(items);c.Text=sel;Controls.Add(c);return c;}
        private CheckBox Chk(string text,bool val,int x,int y){var c=new CheckBox{Text=text,Checked=val,Location=new Point(x,y),AutoSize=true,ForeColor=Color.FromArgb(0,200,55)};Controls.Add(c);return c;}
        private static int Clamp(int v,int min,int max){return v<min?min:v>max?max:v;}

        private void Build()
        {
            int y=14;

            // ── Farbprofile ───────────────────────────────────────────────────
            Lbl("Farbprofil laden:", 14, y);
            var profileNames = new List<string>();
            foreach (var p in profiles) profileNames.Add(p.Name);
            cboProfiles = Combo(155, y-2, 180, profileNames.ToArray(), "");
            var btnLoad = new Button { Text="Laden",  Location=new Point(343,y-1), Size=new Size(70,24), BackColor=Color.FromArgb(0,100,30), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            var btnSave = new Button { Text="Speichern als...", Location=new Point(421,y-1), Size=new Size(110,24), BackColor=Color.FromArgb(40,40,0), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            btnLoad.Click += delegate { LoadSelectedProfile(); };
            btnSave.Click += delegate { SaveCurrentAsProfile(); };
            Controls.Add(btnLoad); Controls.Add(btnSave);
            y += 32; Sep(y); y += 10;

            // ── Farben ────────────────────────────────────────────────────────
            Lbl("Regen-Zeichen:", 14, y); y += 20;
            btnRainColor = ColBtn("Zeichen",     cur.RainColor,     14,  y);
            btnHeadColor = ColBtn("Kopf (hell)", cur.HeadColor,     142, y);
            btnRainColor.Click += delegate { Pick(ref cur.RainColor,     btnRainColor); };
            btnHeadColor.Click += delegate { Pick(ref cur.HeadColor,     btnHeadColor); };
            y += 34;
            Lbl("Fallende Woerter:", 14, y); y += 20;
            btnWordColor     = ColBtn("Woerter",     cur.WordColor,     14,  y);
            btnWordHeadColor = ColBtn("Kopf (hell)", cur.WordHeadColor, 142, y);
            btnWordColor.Click     += delegate { Pick(ref cur.WordColor,     btnWordColor); };
            btnWordHeadColor.Click += delegate { Pick(ref cur.WordHeadColor, btnWordHeadColor); };
            y += 34;
            Lbl("Popup-Woerter:", 14, y); y += 20;
            btnPopupColor = ColBtn("Popup-Farbe", cur.PopupColor, 14, y);
            btnPopupColor.Click += delegate { Pick(ref cur.PopupColor, btnPopupColor); };
            y += 36; Sep(y); y += 10;

            // ── Regen ─────────────────────────────────────────────────────────
            lblFont  = Lbl("Schriftgroesse Regen:  "+cur.FontSize+" px",14,y); y+=18;
            trkFont  = Trk(14,y,370,8,36,cur.FontSize);
            trkFont.ValueChanged  += delegate{cur.FontSize=trkFont.Value;lblFont.Text="Schriftgroesse Regen:  "+cur.FontSize+" px";};
            y+=42;
            lblSpeed = Lbl("Geschwindigkeit:  "+cur.SpeedFactor.ToString("F1")+"x",14,y); y+=18;
            trkSpeed = Trk(14,y,370,1,30,(int)(cur.SpeedFactor*10));
            trkSpeed.ValueChanged += delegate{cur.SpeedFactor=trkSpeed.Value/10f;lblSpeed.Text="Geschwindigkeit:  "+cur.SpeedFactor.ToString("F1")+"x";};
            y+=42;
            lblFade  = Lbl("Spur-Laenge (niedrig = laenger):  "+cur.FadeAlpha,14,y); y+=18;
            trkFade  = Trk(14,y,370,2,60,cur.FadeAlpha);
            trkFade.ValueChanged  += delegate{cur.FadeAlpha=trkFade.Value;lblFade.Text="Spur-Laenge (niedrig = laenger):  "+cur.FadeAlpha;};
            y+=42; Sep(y); y+=10;

            // ── Orientierung + Wortmodus ──────────────────────────────────────
            Lbl("Regen-Richtung:",14,y);
            cboOrient=Combo(120,y-2,140,new string[]{"TopDown","BottomUp","LeftRight","RightLeft"},cur.Orientation);
            Lbl("Wortmodus:",274,y);
            cboWordMode=Combo(360,y-2,130,new string[]{"Rain","Popup","Both"},cur.WordMode);
            y+=32;
            Lbl("Wort-Richtung:",14,y);
            cboWordOrient=Combo(120,y-2,180,new string[]{"Same","TopDown","BottomUp","LeftRight","RightLeft"},
                string.IsNullOrEmpty(cur.WordOrientation)?"Same":cur.WordOrientation);
            y+=32; Sep(y); y+=10;

            // ── Fallende Woerter ──────────────────────────────────────────────
            Lbl("── Fallende Woerter ───────────────────────", 14, y); y+=22;
            Lbl("Wort-Stil:", 14, y);
            cboWordStyle = Combo(100, y-2, 180, new string[]{"Scroll","Fade","Build"},
                string.IsNullOrEmpty(cur.WordStyle) ? "Scroll" : cur.WordStyle);
            y += 32;
            lblWFont  = Lbl("Schriftgroesse:  "+cur.WordFontSize+" px",14,y); y+=18;
            trkWordFont=Trk(14,y,370,8,36,cur.WordFontSize);
            trkWordFont.ValueChanged+=delegate{cur.WordFontSize=trkWordFont.Value;lblWFont.Text="Schriftgroesse:  "+cur.WordFontSize+" px";};
            y+=42;
            lblWCount=Lbl("Gleichzeitige Woerter:  "+cur.WordCount,14,y); y+=18;
            trkWordCount=Trk(14,y,370,1,30,cur.WordCount);
            trkWordCount.ValueChanged+=delegate{cur.WordCount=trkWordCount.Value;lblWCount.Text="Gleichzeitige Woerter:  "+cur.WordCount;};
            y+=42;
            lblWordSpeed=Lbl("Wort-Geschwindigkeit:  "+cur.WordSpeedFactor.ToString("F1")+"x",14,y); y+=18;
            trkWordSpeed=Trk(14,y,370,1,30,(int)(cur.WordSpeedFactor*10));
            trkWordSpeed.ValueChanged+=delegate{cur.WordSpeedFactor=trkWordSpeed.Value/10f;lblWordSpeed.Text="Wort-Geschwindigkeit:  "+cur.WordSpeedFactor.ToString("F1")+"x";};
            y+=42; Sep(y); y+=10;

            // ── Popup Effekte ─────────────────────────────────────────────────
            Lbl("── Popup-Effekte (aktive Stile) ───────────", 14, y); y+=22;
            bool hasFade  = cur.PopupEffects.Contains("Fade");
            bool hasFlash = cur.PopupEffects.Contains("Flash");
            bool hasGlitch= cur.PopupEffects.Contains("Glitch");
            bool hasScan  = cur.PopupEffects.Contains("Scan");
            bool hasZoom  = cur.PopupEffects.Contains("Zoom");
            chkFade  = Chk("Fade",  hasFade,  14,  y);
            chkFlash = Chk("Flash", hasFlash, 100, y);
            chkGlitch= Chk("Glitch",hasGlitch,190, y);
            chkScan  = Chk("Scan",  hasScan,  280, y);
            chkZoom  = Chk("Zoom",  hasZoom,  360, y);
            y+=30;
            lblPFont=Lbl("Schriftgroesse Popups:  "+cur.PopupFontSize+" px",14,y); y+=18;
            trkPopupFont=Trk(14,y,370,10,72,cur.PopupFontSize);
            trkPopupFont.ValueChanged+=delegate{cur.PopupFontSize=trkPopupFont.Value;lblPFont.Text="Schriftgroesse Popups:  "+cur.PopupFontSize+" px";};
            y+=42;
            lblPCount=Lbl("Gleichzeitige Popups:  "+cur.PopupCount,14,y); y+=18;
            trkPopupCount=Trk(14,y,370,1,16,cur.PopupCount);
            trkPopupCount.ValueChanged+=delegate{cur.PopupCount=trkPopupCount.Value;lblPCount.Text="Gleichzeitige Popups:  "+cur.PopupCount;};
            y+=42; Sep(y); y+=10;

            // ── Extras ────────────────────────────────────────────────────────
            chkScanlines=Chk("CRT-Scanlines",       cur.ShowScanlines, 14,  y);
            chkWatermark=Chk("Wasserzeichen",        cur.ShowWatermark, 160, y);
            chkVeeam100 =Chk("Veeam 100 Namen",     cur.ShowVeeam100,  300, y);
            y+=30;
            Lbl("Wasserzeichen-Text:",14,y);
            txtWatermark=new TextBox{Location=new Point(160,y),Size=new Size(200,22),Text=cur.WatermarkText,
                BackColor=Color.FromArgb(28,28,28),ForeColor=Color.FromArgb(0,200,55),BorderStyle=BorderStyle.FixedSingle};
            Controls.Add(txtWatermark);
            Lbl("Untertitel:",372,y);
            txtWatermarkSub=new TextBox{Location=new Point(440,y),Size=new Size(86,22),Text=cur.WatermarkSubText,
                BackColor=Color.FromArgb(28,28,28),ForeColor=Color.FromArgb(0,200,55),BorderStyle=BorderStyle.FixedSingle};
            Controls.Add(txtWatermarkSub);
            y+=30; Sep(y); y+=10;
            Lbl("Eigene Begriffe (kommagetrennt):",14,y); y+=20;
            txtExtra=new TextBox{Location=new Point(14,y),Size=new Size(512,22),Text=cur.ExtraWords,
                                 BackColor=Color.FromArgb(28,28,28),ForeColor=Color.FromArgb(0,200,55),BorderStyle=BorderStyle.FixedSingle};
            Controls.Add(txtExtra);
            y+=36; Sep(y); y+=14;

            // ── Buttons ───────────────────────────────────────────────────────
            int bx=ClientSize.Width-228;
            var btnOK=new Button{Text="OK",Location=new Point(bx,y),Size=new Size(90,30),DialogResult=DialogResult.OK,
                                  BackColor=Color.FromArgb(0,130,38),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};
            btnOK.FlatAppearance.BorderColor=Color.FromArgb(0,200,55);
            btnOK.Click+=delegate
            {
                cur.Orientation     =cboOrient.Text;
                cur.WordOrientation =cboWordOrient.Text;
                cur.WordMode        =cboWordMode.Text;
                cur.WordStyle       =cboWordStyle.Text;
                cur.ShowScanlines   =chkScanlines.Checked;
                cur.ShowWatermark   =chkWatermark.Checked;
                cur.ShowVeeam100    =chkVeeam100.Checked;
                cur.WatermarkText   =txtWatermark.Text.Trim();
                cur.WatermarkSubText=txtWatermarkSub.Text.Trim();
                cur.ExtraWords      =txtExtra.Text.Trim();
                // Build PopupEffects from checkboxes
                var effects=new List<string>();
                if(chkFade.Checked)   effects.Add("Fade");
                if(chkFlash.Checked)  effects.Add("Flash");
                if(chkGlitch.Checked) effects.Add("Glitch");
                if(chkScan.Checked)   effects.Add("Scan");
                if(chkZoom.Checked)   effects.Add("Zoom");
                cur.PopupEffects=effects.Count>0?string.Join(",",effects.ToArray()):"Fade";
                Result=cur; Result.Save();
            };
            var btnCancel=new Button{Text="Abbrechen",Location=new Point(bx+98,y),Size=new Size(108,30),DialogResult=DialogResult.Cancel,
                                      BackColor=Color.FromArgb(55,18,18),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};
            btnCancel.FlatAppearance.BorderColor=Color.FromArgb(140,40,40);
            Controls.Add(btnOK); Controls.Add(btnCancel);
            AcceptButton=btnOK; CancelButton=btnCancel;
            ClientSize=new Size(ClientSize.Width,y+50);
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
        private static void SetBtn(Button b,Color c){b.BackColor=c;b.ForeColor=c.GetBrightness()>.45f?Color.Black:Color.White;}

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
