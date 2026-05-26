// VeeamMatrix.cs  –  Windows Screensaver
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
        public Color  RainColor     = Color.FromArgb(0, 255, 65);
        public Color  HeadColor     = Color.White;
        public int    FadeAlpha     = 12;
        public int    FontSize      = 14;
        public float  SpeedFactor   = 1.0f;
        public bool   ShowWords     = true;
        public int    WordCount     = 14;
        public int    WordFontSize  = 16;
        public string Orientation   = "TopDown";
        public bool   ShowScanlines = true;
        public bool   ShowWatermark = true;
        public float  GlowChance    = 0.22f;
        public string ExtraWords    = "";

        private static string ConfigDir
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "VeeamMatrix");
            }
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
            sb.AppendLine("ShowWords="     + ShowWords);
            sb.AppendLine("WordCount="     + WordCount);
            sb.AppendLine("WordFontSize="  + WordFontSize);
            sb.AppendLine("Orientation="   + Orientation);
            sb.AppendLine("ShowScanlines=" + ShowScanlines);
            sb.AppendLine("ShowWatermark=" + ShowWatermark);
            sb.AppendLine("GlowChance="    + GlowChance.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
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
                        case "ShowWords":     s.ShowWords     = bool.Parse(v); break;
                        case "WordCount":     s.WordCount     = int.Parse(v); break;
                        case "WordFontSize":  s.WordFontSize  = int.Parse(v); break;
                        case "Orientation":   s.Orientation   = v; break;
                        case "ShowScanlines": s.ShowScanlines = bool.Parse(v); break;
                        case "ShowWatermark": s.ShowWatermark = bool.Parse(v); break;
                        case "GlowChance":    s.GlowChance    = float.Parse(v, ic); break;
                        case "ExtraWords":    s.ExtraWords    = v; break;
                    }
                }
                catch { }
            }
            return s;
        }

        private static string ToHex(Color c)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }
        private static Color FromHex(string h)
        {
            h = h.TrimStart('#');
            return Color.FromArgb(
                Convert.ToInt32(h.Substring(0, 2), 16),
                Convert.ToInt32(h.Substring(2, 2), 16),
                Convert.ToInt32(h.Substring(4, 2), 16));
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

        private readonly Settings s;
        private readonly int W, H;
        private readonly Random rng = new Random();
        private readonly string[] allTerms;

        private Bitmap    buf;
        private Graphics  bg;
        private Font      rainFont;
        private Font      wordFont;
        private SolidBrush fadeBrush;
        private SolidBrush rainBrush;
        private SolidBrush brightBrush;
        private SolidBrush tmpBrush;
        private Bitmap    scanBmp;

        private float[] lanePos;
        private float[] laneSpeed;
        private bool[]  laneBright;
        private int     laneCount;

        private class WDrop
        {
            public char[] Chars;
            public float X, Y, V;
            public bool Glow;
        }
        private readonly List<WDrop> wdrops = new List<WDrop>();

        private bool IsVertical  { get { return s.Orientation == "TopDown"   || s.Orientation == "BottomUp";  } }
        private bool IsForward   { get { return s.Orientation == "TopDown"   || s.Orientation == "LeftRight"; } }

        public MatrixEngine(Settings settings, int w, int h)
        {
            s = settings; W = w; H = h;
            var list = new List<string>(TERMS);
            if (!string.IsNullOrEmpty(s.ExtraWords))
            {
                foreach (string part in s.ExtraWords.Split(','))
                {
                    string t = part.Trim().ToUpper();
                    if (t.Length > 0) list.Add(t);
                }
            }
            allTerms = list.ToArray();
            Build();
        }

        private void Build()
        {
            DisposeBrushes();
            if (buf != null)   { bg.Dispose(); buf.Dispose(); }
            if (rainFont != null) rainFont.Dispose();
            if (wordFont != null) wordFont.Dispose();
            if (scanBmp  != null) scanBmp.Dispose();

            buf  = new Bitmap(W, H, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bg   = Graphics.FromImage(buf);
            bg.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            bg.SmoothingMode     = System.Drawing.Drawing2D.SmoothingMode.None;
            bg.Clear(Color.Black);

            int rfs = Math.Max(6, s.FontSize - 1);
            int wfs = Math.Max(6, s.WordFontSize - 1);
            rainFont   = new Font("Courier New", rfs, FontStyle.Bold, GraphicsUnit.Pixel);
            wordFont   = new Font("Courier New", wfs, FontStyle.Bold, GraphicsUnit.Pixel);
            fadeBrush  = new SolidBrush(Color.FromArgb(Math.Max(2, Math.Min(60, s.FadeAlpha)), 0, 0, 0));
            rainBrush  = new SolidBrush(s.RainColor);
            brightBrush= new SolidBrush(s.HeadColor);
            tmpBrush   = new SolidBrush(Color.White);

            int fs = s.FontSize;
            laneCount = IsVertical ? Math.Max(1, W / fs) : Math.Max(1, H / fs);
            int limit  = IsVertical ? H / fs : W / fs;
            lanePos    = new float[laneCount];
            laneSpeed  = new float[laneCount];
            laneBright = new bool[laneCount];
            for (int i = 0; i < laneCount; i++)
            {
                lanePos[i]    = (float)(rng.NextDouble() * limit);
                laneSpeed[i]  = (float)((0.2 + rng.NextDouble() * 0.9) * s.SpeedFactor);
                laneBright[i] = rng.NextDouble() < 0.1;
            }

            wdrops.Clear();
            if (s.ShowWords)
                for (int i = 0; i < s.WordCount; i++)
                    wdrops.Add(SpawnWord(true));

            BuildScanlines();
        }

        private WDrop SpawnWord(bool scatter)
        {
            string term  = allTerms[rng.Next(allTerms.Length)];
            int    fs    = s.WordFontSize;
            char[] chars = term.ToCharArray();
            float  len   = chars.Length * fs;
            float  v     = (float)((0.6 + rng.NextDouble() * 1.6) * s.SpeedFactor);
            float  x, y;

            if (IsVertical)
            {
                x = fs * 0.5f + (float)(rng.NextDouble() * Math.Max(1, W - fs));
                y = scatter ? (float)(rng.NextDouble() * (H + len) - len) : (IsForward ? -(len + 5) : H + 5);
            }
            else
            {
                y = fs * 0.5f + (float)(rng.NextDouble() * Math.Max(1, H - fs));
                x = scatter ? (float)(rng.NextDouble() * (W + len) - len) : (IsForward ? -(len + 5) : W + 5);
            }
            return new WDrop
            {
                Chars = chars, X = x, Y = y,
                V     = IsForward ? v : -v,
                Glow  = rng.NextDouble() < s.GlowChance
            };
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
                {
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                        for (int y = 0; y < H; y += 2)
                            g.FillRectangle(sb, 0, y, W, 1);
                }
                if (s.ShowWatermark)
                {
                    int logoSize = Math.Max(24, (int)(W * 0.08));
                    using (Font lf = new Font("Courier New", logoSize, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (SolidBrush lb = new SolidBrush(Color.FromArgb(10, s.RainColor)))
                    {
                        SizeF ls = g.MeasureString("VEEAM", lf);
                        g.DrawString("VEEAM", lf, lb, (W - ls.Width) / 2f, (H - ls.Height) / 2f);
                    }
                    int subSize = Math.Max(7, (int)(W * 0.015));
                    using (Font sf = new Font("Courier New", subSize, FontStyle.Regular, GraphicsUnit.Pixel))
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(7, s.RainColor)))
                    {
                        const string sub = "DATA PROTECTION  *  CYBER RESILIENCE  *  ALWAYS-ON";
                        SizeF ss = g.MeasureString(sub, sf);
                        g.DrawString(sub, sf, sb,
                            (W - ss.Width) / 2f,
                            (H / 2f) + (int)(W * 0.04f) + subSize * 2);
                    }
                }
            }
        }

        public void Tick()
        {
            bg.FillRectangle(fadeBrush, 0, 0, W, H);
            DrawRain();
            if (s.ShowWords) DrawWords();
            if (scanBmp != null) bg.DrawImage(scanBmp, 0, 0);
        }

        private void DrawRain()
        {
            int fs = s.FontSize;
            for (int i = 0; i < laneCount; i++)
            {
                char ch  = RAIN_CHARS[rng.Next(RAIN_CHARS.Length)];
                float px, py;
                if (IsVertical)
                {
                    px = i * fs;
                    py = IsForward ? lanePos[i] * fs : H - lanePos[i] * fs;
                }
                else
                {
                    py = i * fs;
                    px = IsForward ? lanePos[i] * fs : W - lanePos[i] * fs;
                }

                SolidBrush br = laneBright[i] ? brightBrush : rainBrush;
                bg.DrawString(ch.ToString(), rainFont, br, px, py);

                lanePos[i] += laneSpeed[i];
                float maxPos = IsVertical ? (float)H / fs : (float)W / fs;
                if (lanePos[i] > maxPos && rng.NextDouble() > 0.975)
                {
                    lanePos[i]    = 0;
                    laneSpeed[i]  = (float)((0.2 + rng.NextDouble() * 0.9) * s.SpeedFactor);
                    laneBright[i] = rng.NextDouble() < 0.1;
                }
            }
        }

        private void DrawWords()
        {
            int fs = s.WordFontSize;
            for (int i = wdrops.Count - 1; i >= 0; i--)
            {
                WDrop w = wdrops[i];
                int   n = w.Chars.Length;

                for (int j = 0; j < n; j++)
                {
                    float px = IsVertical ? w.X           : w.X + j * fs;
                    float py = IsVertical ? w.Y + j * fs  : w.Y;

                    if (px < -fs || px > W + fs || py < -fs || py > H + fs) continue;

                    float fade  = n > 1 ? (float)j / (n - 1) : 0f;
                    int   alpha = Clamp((int)(255 * (1f - fade * 0.55f)));

                    Color col;
                    if (j == 0)
                    {
                        col = s.HeadColor;
                    }
                    else if (w.Glow)
                    {
                        col = Color.FromArgb(alpha,
                            Clamp((int)(s.RainColor.R + 80 * (1 - fade))),
                            Clamp((int)(s.RainColor.G)),
                            Clamp((int)(s.RainColor.B + 40 * (1 - fade))));
                    }
                    else
                    {
                        col = Color.FromArgb(alpha,
                            Clamp((int)(s.RainColor.R * (1 - fade * 0.5f))),
                            Clamp((int)(s.RainColor.G * (1 - fade * 0.3f) + 30f * (1f - fade))),
                            Clamp((int)(s.RainColor.B * (1 - fade * 0.5f))));
                    }

                    tmpBrush.Color = col;
                    bg.DrawString(w.Chars[j].ToString(), wordFont, tmpBrush, px, py);
                }

                if (IsVertical) w.Y += w.V;
                else            w.X += w.V;

                bool gone = IsVertical
                    ? (IsForward ? w.Y > H + 5 : w.Y + n * fs < -5)
                    : (IsForward ? w.X > W + 5 : w.X + n * fs < -5);

                if (gone) { wdrops.RemoveAt(i); wdrops.Add(SpawnWord(false)); }
            }
        }

        private static int Clamp(int v) { return v < 0 ? 0 : v > 255 ? 255 : v; }

        public void Render(Graphics g) { g.DrawImage(buf, 0, 0); }

        private void DisposeBrushes()
        {
            if (fadeBrush   != null) { fadeBrush.Dispose();   fadeBrush   = null; }
            if (rainBrush   != null) { rainBrush.Dispose();   rainBrush   = null; }
            if (brightBrush != null) { brightBrush.Dispose(); brightBrush = null; }
            if (tmpBrush    != null) { tmpBrush.Dispose();    tmpBrush    = null; }
        }

        public void Dispose()
        {
            DisposeBrushes();
            if (bg       != null) bg.Dispose();
            if (buf      != null) buf.Dispose();
            if (rainFont != null) rainFont.Dispose();
            if (wordFont != null) wordFont.Dispose();
            if (scanBmp  != null) scanBmp.Dispose();
        }
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
            // OptimizedDoubleBuffer: WinForms buffers WM_PAINT before blitting to screen.
            // AllPaintingInWmPaint: no separate WM_ERASEBKGND → no black flash between frames.
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);

            engine = new MatrixEngine(s, bounds.Width, bounds.Height);
            timer  = new Timer();
            timer.Interval = 25;
            timer.Tick += OnTick;
            timer.Start(); // start on ALL screens

            if (isPrimary)
            {
                ShowCursor(false);
                SetCursorPos(bounds.Left, bounds.Top);
                KeyDown   += delegate { Close(); };
                MouseDown += delegate { Close(); };
                MouseMove += OnMouseMove;
                FormClosed += delegate { ShowCursor(true); };
            }
        }

        // Suppress background erase — engine always repaints the full surface.
        protected override void OnPaintBackground(PaintEventArgs e) { }

        private void OnTick(object sender, EventArgs e) { engine.Tick(); Invalidate(false); }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (firstMove) { lastMouse = e.Location; firstMove = false; return; }
            if (Math.Abs(e.X - lastMouse.X) > 5 || Math.Abs(e.Y - lastMouse.Y) > 5)
                Close();
        }

        protected override void OnPaint(PaintEventArgs e) { engine.Render(e.Graphics); }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { timer.Dispose(); engine.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // =========================================================================
    // PREVIEW FORM  (/p hwnd  –  shown in screensaver settings panel)
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
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            engine = new MatrixEngine(s, Width, Height);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            timer  = new Timer();
            timer.Interval = 40;
            timer.Tick += delegate { engine.Tick(); Invalidate(false); };
            timer.Start();
        }
        protected override void OnPaintBackground(PaintEventArgs e) { }
        protected override void OnPaint(PaintEventArgs e) { engine.Render(e.Graphics); }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { timer.Dispose(); engine.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // =========================================================================
    // CONFIG FORM
    // =========================================================================
    class ConfigForm : Form
    {
        private Settings cur;
        private Button   btnRainColor, btnHeadColor;
        private TrackBar trkFade, trkFont, trkSpeed, trkWordCount, trkWordFont;
        private Label    lblFade, lblFont, lblSpeed, lblWCount, lblWFont;
        private ComboBox cboOrient;
        private CheckBox chkWords, chkScanlines, chkWatermark;
        private TextBox  txtExtra;

        public Settings Result { get; private set; }

        public ConfigForm(Settings s)
        {
            cur = Clone(s);
            Text            = "Veeam Matrix - Einstellungen";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ClientSize      = new Size(500, 580);
            BackColor       = Color.FromArgb(18, 18, 18);
            ForeColor       = Color.FromArgb(0, 200, 55);
            Font            = new Font("Segoe UI", 9f);
            Build();
        }

        private static Settings Clone(Settings s)
        {
            return new Settings
            {
                RainColor    = s.RainColor,    HeadColor    = s.HeadColor,
                FadeAlpha    = s.FadeAlpha,    FontSize     = s.FontSize,
                SpeedFactor  = s.SpeedFactor,  ShowWords    = s.ShowWords,
                WordCount    = s.WordCount,    WordFontSize = s.WordFontSize,
                Orientation  = s.Orientation,  ShowScanlines= s.ShowScanlines,
                ShowWatermark= s.ShowWatermark, GlowChance  = s.GlowChance,
                ExtraWords   = s.ExtraWords
            };
        }

        private Label AddLabel(string text, int x, int y)
        {
            var l = new Label { Text = text, Location = new Point(x, y), AutoSize = true,
                                ForeColor = Color.FromArgb(0, 200, 55) };
            Controls.Add(l); return l;
        }

        private void AddSep(int y)
        {
            Controls.Add(new Panel { Location = new Point(14, y), Size = new Size(472, 1),
                                     BackColor = Color.FromArgb(0, 80, 25) });
        }

        private TrackBar AddTrack(int x, int y, int w, int min, int max, int val)
        {
            var t = new TrackBar { Location = new Point(x, y), Size = new Size(w, 36),
                                   Minimum = min, Maximum = max, Value = val,
                                   TickFrequency = Math.Max(1, (max - min) / 10),
                                   SmallChange = 1, BackColor = Color.FromArgb(18, 18, 18) };
            Controls.Add(t); return t;
        }

        private Button AddColorBtn(string text, Color col, int x, int y)
        {
            var b = new Button { Text = text, Location = new Point(x, y), Size = new Size(140, 28),
                                 BackColor = col,
                                 ForeColor = col.GetBrightness() > 0.45f ? Color.Black : Color.White,
                                 FlatStyle = FlatStyle.Flat };
            b.FlatAppearance.BorderColor = Color.FromArgb(0, 160, 45);
            Controls.Add(b); return b;
        }

        private void Build()
        {
            int y = 16;

            // Farben
            AddLabel("Farben", 14, y); y += 22;
            btnRainColor = AddColorBtn("Regen-Farbe",  cur.RainColor, 14, y);
            btnHeadColor = AddColorBtn("Kopf-Farbe",   cur.HeadColor, 162, y);
            btnRainColor.Click += delegate { PickColor(ref cur.RainColor, btnRainColor); };
            btnHeadColor.Click += delegate { PickColor(ref cur.HeadColor, btnHeadColor); };
            y += 36; AddSep(y); y += 10;

            // Schriftgroesse Regen
            lblFont = AddLabel("Schriftgroesse Regen:  " + cur.FontSize + " px", 14, y); y += 18;
            trkFont = AddTrack(14, y, 340, 8, 36, cur.FontSize);
            trkFont.ValueChanged += delegate { cur.FontSize = trkFont.Value; lblFont.Text = "Schriftgroesse Regen:  " + cur.FontSize + " px"; };
            y += 44; AddSep(y); y += 10;

            // Schriftgroesse Woerter
            lblWFont = AddLabel("Schriftgroesse Woerter:  " + cur.WordFontSize + " px", 14, y); y += 18;
            trkWordFont = AddTrack(14, y, 340, 8, 36, cur.WordFontSize);
            trkWordFont.ValueChanged += delegate { cur.WordFontSize = trkWordFont.Value; lblWFont.Text = "Schriftgroesse Woerter:  " + cur.WordFontSize + " px"; };
            y += 44; AddSep(y); y += 10;

            // Geschwindigkeit
            lblSpeed = AddLabel("Geschwindigkeit:  " + cur.SpeedFactor.ToString("F1") + "x", 14, y); y += 18;
            trkSpeed = AddTrack(14, y, 340, 1, 30, (int)(cur.SpeedFactor * 10));
            trkSpeed.ValueChanged += delegate { cur.SpeedFactor = trkSpeed.Value / 10f; lblSpeed.Text = "Geschwindigkeit:  " + cur.SpeedFactor.ToString("F1") + "x"; };
            y += 44; AddSep(y); y += 10;

            // Spur-Laenge
            lblFade = AddLabel("Spur-Laenge (niedrig = laenger):  " + cur.FadeAlpha, 14, y); y += 18;
            trkFade = AddTrack(14, y, 340, 2, 60, cur.FadeAlpha);
            trkFade.ValueChanged += delegate { cur.FadeAlpha = trkFade.Value; lblFade.Text = "Spur-Laenge (niedrig = laenger):  " + cur.FadeAlpha; };
            y += 44; AddSep(y); y += 10;

            // Richtung
            AddLabel("Richtung:", 14, y);
            cboOrient = new ComboBox { Location = new Point(130, y - 2), Size = new Size(160, 24),
                                       DropDownStyle = ComboBoxStyle.DropDownList,
                                       BackColor = Color.FromArgb(30, 30, 30),
                                       ForeColor = Color.FromArgb(0, 200, 55) };
            cboOrient.Items.AddRange(new object[] { "TopDown", "BottomUp", "LeftRight", "RightLeft" });
            cboOrient.Text = cur.Orientation;
            Controls.Add(cboOrient);
            y += 32; AddSep(y); y += 10;

            // Woerter
            chkWords = new CheckBox { Text = "Veeam-Begriffe anzeigen", Location = new Point(14, y),
                                      Checked = cur.ShowWords, AutoSize = true,
                                      ForeColor = Color.FromArgb(0, 200, 55) };
            Controls.Add(chkWords); y += 26;

            lblWCount = AddLabel("Gleichzeitige Woerter:  " + cur.WordCount, 14, y); y += 18;
            trkWordCount = AddTrack(14, y, 340, 1, 30, cur.WordCount);
            trkWordCount.ValueChanged += delegate { cur.WordCount = trkWordCount.Value; lblWCount.Text = "Gleichzeitige Woerter:  " + cur.WordCount; };
            y += 44; AddSep(y); y += 10;

            // Extras
            chkScanlines = new CheckBox { Text = "CRT-Scanlines",         Location = new Point(14,  y), Checked = cur.ShowScanlines, AutoSize = true, ForeColor = Color.FromArgb(0, 200, 55) };
            chkWatermark = new CheckBox { Text = "VEEAM-Wasserzeichen",   Location = new Point(185, y), Checked = cur.ShowWatermark, AutoSize = true, ForeColor = Color.FromArgb(0, 200, 55) };
            Controls.Add(chkScanlines); Controls.Add(chkWatermark);
            y += 28; AddSep(y); y += 10;

            // Eigene Begriffe
            AddLabel("Eigene Begriffe (kommagetrennt):", 14, y); y += 20;
            txtExtra = new TextBox { Location = new Point(14, y), Size = new Size(472, 22),
                                     Text = cur.ExtraWords, BackColor = Color.FromArgb(28, 28, 28),
                                     ForeColor = Color.FromArgb(0, 200, 55), BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(txtExtra);
            y += 36; AddSep(y); y += 14;

            // Buttons
            int bx = ClientSize.Width - 220;
            var btnOK = new Button { Text = "OK", Location = new Point(bx, y), Size = new Size(90, 30),
                                     DialogResult = DialogResult.OK,
                                     BackColor = Color.FromArgb(0, 130, 38), ForeColor = Color.White,
                                     FlatStyle = FlatStyle.Flat };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(0, 200, 55);
            btnOK.Click += delegate
            {
                cur.Orientation   = cboOrient.Text;
                cur.ShowWords     = chkWords.Checked;
                cur.ShowScanlines = chkScanlines.Checked;
                cur.ShowWatermark = chkWatermark.Checked;
                cur.ExtraWords    = txtExtra.Text.Trim();
                Result = cur;
                Result.Save();
            };

            var btnCancel = new Button { Text = "Abbrechen", Location = new Point(bx + 98, y), Size = new Size(100, 30),
                                         DialogResult = DialogResult.Cancel,
                                         BackColor = Color.FromArgb(55, 18, 18), ForeColor = Color.White,
                                         FlatStyle = FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(140, 40, 40);

            Controls.Add(btnOK); Controls.Add(btnCancel);
            AcceptButton = btnOK; CancelButton = btnCancel;

            // adjust form height
            ClientSize = new Size(ClientSize.Width, y + 50);
        }

        private void PickColor(ref Color field, Button btn)
        {
            using (ColorDialog dlg = new ColorDialog { Color = field, FullOpen = true })
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    field         = dlg.Color;
                    btn.BackColor = dlg.Color;
                    btn.ForeColor = dlg.Color.GetBrightness() > 0.45f ? Color.Black : Color.White;
                }
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
                using (ConfigForm cfg = new ConfigForm(s))
                    cfg.ShowDialog();
            }
            else if (mode == "/p" && args.Length > 1)
            {
                IntPtr hwnd = new IntPtr(long.Parse(args[1]));
                using (PreviewForm pf = new PreviewForm(s, hwnd))
                    Application.Run(pf);
            }
            else
            {
                var forms = new List<ScreenSaverForm>();
                ScreenSaverForm main = null;
                foreach (Screen scr in Screen.AllScreens)
                {
                    var f = new ScreenSaverForm(s, scr.Bounds, scr.Primary);
                    forms.Add(f);
                    if (scr.Primary) main = f;
                    else             f.Show();
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
