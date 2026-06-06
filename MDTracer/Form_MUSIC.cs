namespace MDTracer
{
    public partial class Form_MUSIC : Form
    {
        private enum MusicViewMode
        {
            Scroll,
            PianoRoll3D
        }

        private const int PianoRollHistoryLength = 180;
        private const int PianoRollStarCount = 360;
        private const int MusicViewDefaultWidth = 674;
        private const int MusicViewDefaultHeight = 360;
        private int PianoKeyboardHeight => Math.Max(34, Math.Min(72, g_orgBitmap.Height / 6));
        private int PianoRollBottom => Math.Max(1, g_orgBitmap.Height - PianoKeyboardHeight);

        public float[] KEY_SCALE_LIST = {
            34, 36, 38, 40, 42, 45, 48, 50, 53, 57,
            60, 64, 67, 71, 76, 80, 85, 90, 95, 101,
            107, 113, 120, 127, 135, 143, 151, 160, 170, 180,
            190, 202, 214, 227, 240, 254, 269, 285, 302, 320,
            339, 360, 381, 404, 428, 453, 480, 509, 539, 571,
            605, 641, 679, 719, 762, 807, 855, 906, 960, 1017,
            1078, 1142, 1210, 1282, 1358, 1438, 1524, 1615, 1711, 1812,
            1920, 2034, 2155, 2283, 2419, 2563, 2715, 2877, 3048, 3229,
            3421, 3625, 3840, 4069, 4310, 4567, 4838, 5126, 5431, 5754,
            6096, 6458, 6842, 7249, 7680 };
        public int[] KEY_WIDTH = { 9, 6, 6, 6, 9, 9, 6, 6, 6, 6, 6, 9 };
        public int[] KEY_POS = { 9, 15, 21, 27, 36, 45, 51, 57, 63, 69, 75, 84 };
        public int[] KEY_COLOR = { 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0 };
        public Bitmap g_orgBitmap;
        public Bitmap g_cpyBitmap;
        public int[] g_freq_out;
        public int g_screen_xpos;
        public int g_screen_ypos;

        private int[,] g_piano_roll_history;
        private int g_piano_roll_phase;
        private int g_piano_roll_style;
        private MusicViewMode g_view_mode = MusicViewMode.Scroll;
        private int[] g_active_key_channel;
        private readonly Color[] g_channel_colors = {
            Color.Salmon,
            Color.Gold,
            Color.Lime,
            Color.Aqua,
            Color.SteelBlue,
            Color.DarkKhaki,
            Color.Orchid,
            Color.DeepPink,
            Color.Pink,
            Color.DarkViolet
        };
        //----------------------------------------------------------------
        //form
        //----------------------------------------------------------------
        public Form_MUSIC()
        {
            InitializeComponent();
            this.MinimumSize = new Size(520, 300);
            g_orgBitmap = new Bitmap(MusicViewDefaultWidth, MusicViewDefaultHeight);
            g_cpyBitmap = new Bitmap(MusicViewDefaultWidth, PianoRollBottom);
            g_freq_out = new int[11];
            g_piano_roll_history = new int[PianoRollHistoryLength, 10];
            g_active_key_channel = new int[KEY_SCALE_LIST.Length];
            pictureBox_view.Image = g_orgBitmap;
            this.SizeChanged += Form_MUSIC_SizeChanged;
            comboBox_viewMode.SelectedIndex = 0;
            LayoutMusicControls();
            ResetScrollView();
        }
        //----------------------------------------------------------------
        //initialize
        //----------------------------------------------------------------
        public void initialize()
        {
            hScrollBar_Fm1.Value = md_main.g_md_music.g_master_vol[0];
            hScrollBar_Fm2.Value = md_main.g_md_music.g_master_vol[1];
            hScrollBar_Fm3.Value = md_main.g_md_music.g_master_vol[2];
            hScrollBar_Fm4.Value = md_main.g_md_music.g_master_vol[3];
            hScrollBar_Fm5.Value = md_main.g_md_music.g_master_vol[4];
            hScrollBar_Fm6.Value = md_main.g_md_music.g_master_vol[5];
            hScrollBar_Psg1.Value = md_main.g_md_music.g_master_vol[6];
            hScrollBar_Psg2.Value = md_main.g_md_music.g_master_vol[7];
            hScrollBar_Psg3.Value = md_main.g_md_music.g_master_vol[8];
            hScrollBar_Psg4.Value = md_main.g_md_music.g_master_vol[9];
            hScrollBar_Master.Value = md_main.g_md_music.g_master_vol[10];
            checkBox_Fm1.Checked = md_main.g_md_music.g_master_chk[0];
            checkBox_Fm2.Checked = md_main.g_md_music.g_master_chk[1];
            checkBox_Fm3.Checked = md_main.g_md_music.g_master_chk[2];
            checkBox_Fm4.Checked = md_main.g_md_music.g_master_chk[3];
            checkBox_Fm5.Checked = md_main.g_md_music.g_master_chk[4];
            checkBox_Fm6.Checked = md_main.g_md_music.g_master_chk[5];
            checkBox_Psg1.Checked = md_main.g_md_music.g_master_chk[6];
            checkBox_Psg2.Checked = md_main.g_md_music.g_master_chk[7];
            checkBox_Psg3.Checked = md_main.g_md_music.g_master_chk[8];
            checkBox_Psg4.Checked = md_main.g_md_music.g_master_chk[9];
            checkBox_Master.Checked = md_main.g_md_music.g_master_chk[10];
        }

        //----------------------------------------------------------------
        //Event Handling: Painting
        //----------------------------------------------------------------
        private void Form_MUSIC_Paint(object sender, PaintEventArgs e)
        {
            pictureBox_view.Invalidate();
        }
        private void pictureBox_view_Paint(object sender, PaintEventArgs e)
        {
            EnsureMusicBitmapSize();
            UpdateActiveKeys();
            if (g_view_mode == MusicViewMode.PianoRoll3D)
            {
                PushPianoRollHistory();
                DrawPianoRoll3D();
                return;
            }

            DrawScrollView();
        }

        private void DrawScrollView()
        {
            using (Graphics g_cpy = Graphics.FromImage(g_cpyBitmap))
            {
                Rectangle srcRect = new Rectangle(0, 1, g_orgBitmap.Width, PianoRollBottom - 1);
                Rectangle desRect = new Rectangle(0, 0, srcRect.Width, srcRect.Height);
                g_cpy.DrawImage(g_orgBitmap, desRect, srcRect, GraphicsUnit.Pixel);
            }
            using (Graphics g_org = Graphics.FromImage(g_orgBitmap))
            {
                Rectangle srcRect = new Rectangle(0, 0, g_orgBitmap.Width, PianoRollBottom - 1);
                Rectangle desRect = new Rectangle(0, 0, srcRect.Width, srcRect.Height);
                g_org.DrawImage(g_cpyBitmap, desRect, srcRect, GraphicsUnit.Pixel);
                using (Brush w_backBrush = new SolidBrush(Color.FromArgb(28, 232, 250, 255)))
                {
                    g_org.FillRectangle(w_backBrush, new Rectangle(0, PianoRollBottom - 2, g_orgBitmap.Width, 2));
                }
            }
            using (Graphics g_org = Graphics.FromImage(g_orgBitmap))
            {
                for (int i = 0; i <= 9; i++)
                {
                    int w_freq = GetVisibleFrequency(i);
                    if (0 < w_freq)
                    {
                        int w_ley_number = key_number_chk(w_freq);
                        int dx = (int)PitchToPianoRollX(w_ley_number);
                        Rectangle rect = new Rectangle(dx, PianoRollBottom - 9, Math.Max(3, (int)KeyToPianoRollWidth(w_ley_number)), 7);
                        using (Brush brush = new SolidBrush(GetChannelColor(i)))
                        {
                            g_org.FillRectangle(brush, rect);
                        }
                    }
                }
                DrawHorizontalKeyboard(g_org);
            }
        }

        private void PushPianoRollHistory()
        {
            for (int history = PianoRollHistoryLength - 1; 0 < history; history--)
            {
                for (int channel = 0; channel < 10; channel++)
                {
                    g_piano_roll_history[history, channel] = g_piano_roll_history[history - 1, channel];
                }
            }

            for (int channel = 0; channel < 10; channel++)
            {
                g_piano_roll_history[0, channel] = GetVisibleFrequency(channel);
            }
            g_piano_roll_phase++;
        }

        private void EnsureMusicBitmapSize()
        {
            int w_width = Math.Max(160, pictureBox_view.ClientSize.Width);
            int w_height = Math.Max(120, pictureBox_view.ClientSize.Height);
            if ((g_orgBitmap.Width == w_width) && (g_orgBitmap.Height == w_height)
                && (g_cpyBitmap.Width == w_width) && (g_cpyBitmap.Height == PianoRollBottom))
            {
                return;
            }

            int w_keyboardHeight = Math.Max(34, Math.Min(72, w_height / 6));
            int w_rollBottom = Math.Max(1, w_height - w_keyboardHeight);
            Bitmap w_oldOrgBitmap = g_orgBitmap;
            Bitmap w_oldCpyBitmap = g_cpyBitmap;
            g_orgBitmap = new Bitmap(w_width, w_height);
            g_cpyBitmap = new Bitmap(w_width, w_rollBottom);
            pictureBox_view.Image = g_orgBitmap;
            w_oldOrgBitmap?.Dispose();
            w_oldCpyBitmap?.Dispose();
            ResetScrollView();
        }

        private void UpdateActiveKeys()
        {
            Array.Fill(g_active_key_channel, -1);
            for (int channel = 0; channel < 10; channel++)
            {
                int w_freq = GetVisibleFrequency(channel);
                if (w_freq <= 0) continue;

                int w_keyNumber = key_number_chk(w_freq);
                if (w_keyNumber < g_active_key_channel.Length)
                {
                    g_active_key_channel[w_keyNumber] = channel;
                }
            }
        }

        private int GetVisibleFrequency(int in_channel)
        {
            if (IsChannelDisplayEnabled(in_channel) == false) return 0;
            return g_freq_out[in_channel];
        }

        private bool IsChannelDisplayEnabled(int in_channel)
        {
            if ((in_channel < 0) || (10 <= in_channel)) return false;
            if (md_main.g_md_music.g_master_chk[in_channel] == false) return false;
            return md_main.g_md_music.g_out_vol[in_channel] > 0.0f;
        }

        private Color GetChannelColor(int in_channel)
        {
            return g_channel_colors[Math.Max(0, Math.Min(g_channel_colors.Length - 1, in_channel))];
        }

        private void DrawPianoRoll3D()
        {
            using (Graphics g = Graphics.FromImage(g_orgBitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Black);

                Rectangle w_view = new Rectangle(0, 0, g_orgBitmap.Width, PianoRollBottom);
                using (System.Drawing.Drawing2D.LinearGradientBrush w_brush = new System.Drawing.Drawing2D.LinearGradientBrush(w_view, GetPianoRollBackColorFar(), GetPianoRollBackColorNear(), 90.0f))
                {
                    g.FillRectangle(w_brush, w_view);
                }

                DrawPianoRollStarfield(g);
                DrawPianoRollAtmosphere(g);

                DrawPianoRollGrid3D(g);
                DrawPianoRollKeyboard3D(g);
                DrawPianoRollNotes3D(g);
                pictureBox_view.Image = g_orgBitmap;
            }
        }

        private void DrawPianoRollGrid3D(Graphics in_graphics)
        {
            using (Pen w_pen = new Pen(GetPianoRollGridColor(), 1.0f))
            {
                for (int key = 0; key <= KEY_SCALE_LIST.Length; key += 12)
                {
                    float w_x = PitchToPianoRollX(key);
                    PointF w_bottom = ProjectPianoRollPoint(w_x, 0.0f);
                    PointF w_top = ProjectPianoRollPoint(w_x, 1.0f);
                    in_graphics.DrawLine(w_pen, w_bottom, w_top);
                }

                for (int i = 0; i <= 14; i++)
                {
                    float w_depth = i / 14.0f;
                    PointF w_left = ProjectPianoRollPoint(0.0f, w_depth);
                    PointF w_right = ProjectPianoRollPoint(g_orgBitmap.Width, w_depth);
                    in_graphics.DrawLine(w_pen, w_left, w_right);
                }
            }
        }

        private void DrawPianoRollStarfield(Graphics in_graphics)
        {
            float w_vanishX = (g_orgBitmap.Width / 2.0f) + GetPianoRollVanishOffsetX();
            float w_vanishY = GetPianoRollVanishY();
            int w_bottom = PianoRollBottom;

            for (int i = 0; i < PianoRollStarCount; i++)
            {
                int w_seed = GetPianoRollStarSeed(i);
                float w_speedClass = GetPianoRollStarUnit(w_seed, 20);
                float w_speed = 1.4f + (w_speedClass * w_speedClass * 6.6f) + ((g_piano_roll_style % 4) * 0.26f);
                float w_progress = ((w_seed % 4096) + (g_piano_roll_phase * w_speed)) % 4096.0f / 4096.0f;
                DrawPianoRollStar(in_graphics, w_seed, w_progress, w_vanishX, w_vanishY, w_bottom);
            }
        }

        private void DrawPianoRollStar(Graphics in_graphics, int in_seed, float in_progress, float in_vanishX, float in_vanishY, int in_bottom)
        {
            float w_side = GetPianoRollStarUnit(in_seed, 8) - 0.5f;
            float w_vertical = 0.08f + (GetPianoRollStarUnit(in_seed, 16) * 0.96f);
            float w_sizeClass = GetPianoRollStarUnit(in_seed, 4);
            float w_blinkClass = GetPianoRollStarUnit(in_seed, 24);
            float w_near = in_progress * in_progress;
            float w_spread = 0.05f + (w_near * 1.72f);
            float w_x = in_vanishX + (w_side * g_orgBitmap.Width * 2.25f * w_spread);
            float w_y = in_vanishY + ((in_bottom - in_vanishY) * w_vertical * w_spread);
            if (w_x < -20.0f || g_orgBitmap.Width + 20.0f < w_x || w_y < -20.0f || in_bottom + 20.0f < w_y)
            {
                return;
            }

            float w_blink = 0.72f + (0.28f * (float)Math.Sin((g_piano_roll_phase * (0.035f + (w_blinkClass * 0.055f))) + (in_seed & 0xff)));
            int w_alpha = Math.Max(14, Math.Min(245, (int)((20 + (w_near * 218.0f) + (w_sizeClass * 38.0f)) * w_blink)));
            Color w_color = GetPianoRollStarColor(in_seed, w_near, w_blink);

            using (Brush w_brush = new SolidBrush(Color.FromArgb(w_alpha, w_color)))
            {
                in_graphics.FillRectangle(w_brush, (int)w_x, (int)w_y, 1, 1);
            }
        }

        private int GetPianoRollStarSeed(int in_index)
        {
            unchecked
            {
                int w_seed = (in_index + 1) * 1103515245;
                w_seed ^= (g_piano_roll_style + 17) * 12345;
                w_seed ^= w_seed >> 16;
                return w_seed & 0x7fffffff;
            }
        }

        private float GetPianoRollStarUnit(int in_seed, int in_shift)
        {
            int w_value = (in_seed >> in_shift) & 0xff;
            return w_value / 255.0f;
        }

        private Color GetPianoRollStarColor(int in_seed, float in_near, float in_blink)
        {
            Color[] w_starColors = {
                Color.FromArgb(210, 230, 255),
                Color.FromArgb(255, 245, 210),
                Color.FromArgb(190, 255, 235),
                Color.FromArgb(255, 210, 235),
                Color.FromArgb(220, 210, 255),
                GetPianoRollSparkColor(in_seed & 0x7f)
            };
            Color w_spark = w_starColors[(in_seed >> 3) % w_starColors.Length];
            float w_white = 0.32f + (in_near * 0.5f) + (in_blink * 0.14f);
            return Color.FromArgb(
                Math.Min(255, (int)((w_spark.R * (1.0f - w_white)) + (255 * w_white))),
                Math.Min(255, (int)((w_spark.G * (1.0f - w_white)) + (255 * w_white))),
                Math.Min(255, (int)((w_spark.B * (1.0f - w_white)) + (255 * w_white))));
        }

        private void DrawPianoRollKeyboard3D(Graphics in_graphics)
        {
            DrawHorizontalKeyboard(in_graphics);
        }

        private void DrawPianoRollNotes3D(Graphics in_graphics)
        {
            for (int history = PianoRollHistoryLength - 1; 0 <= history; history--)
            {
                float w_depth = history / (float)(PianoRollHistoryLength - 1);
                float w_scale = Math.Max(0.12f, 1.0f - (w_depth * 0.82f));
                int w_alpha = Math.Max(0, 255 - (int)(w_depth * 255));
                int w_edgeAlpha = Math.Min(255, (int)(w_alpha * 1.15f));

                for (int channel = 0; channel < 10; channel++)
                {
                    if (IsChannelDisplayEnabled(channel) == false) continue;

                    int w_freq = g_piano_roll_history[history, channel];
                    if (w_freq <= 0) continue;

                    int w_keyNumber = key_number_chk(w_freq);
                    if (KEY_SCALE_LIST.Length <= w_keyNumber) continue;

                    float w_x = PitchToPianoRollX(w_keyNumber);
                    float w_w = Math.Max(2.0f, KeyToPianoRollWidth(w_keyNumber) * w_scale);
                    float w_len = 0.020f + ((g_piano_roll_style % 4) * 0.004f);
                    PointF w_p1 = ProjectPianoRollPoint(w_x, w_depth);
                    PointF w_p2 = ProjectPianoRollPoint(w_x + w_w, w_depth);
                    PointF w_p3 = ProjectPianoRollPoint(w_x + w_w, Math.Min(1.0f, w_depth + w_len));
                    PointF w_p4 = ProjectPianoRollPoint(w_x, Math.Min(1.0f, w_depth + w_len));
                    Color w_color = GetPianoRollDepthColor(GetPianoRollNoteColor(channel, history), w_depth);

                    using (Brush w_brush = new SolidBrush(Color.FromArgb(w_alpha, w_color)))
                    using (Pen w_pen = new Pen(Color.FromArgb(w_edgeAlpha, GetPianoRollEdgeColor()), Math.Max(1.0f, w_scale)))
                    {
                        PointF[] w_poly = { w_p1, w_p2, w_p3, w_p4 };
                        DrawPianoRollNoteShape(in_graphics, w_poly, w_brush, w_pen, w_scale);
                    }
                }
            }
        }

        private PointF ProjectPianoRollPoint(float in_x, float in_depth)
        {
            float w_curve = GetPianoRollDepthCurve(in_depth);
            float w_vanishX = (g_orgBitmap.Width / 2.0f) + GetPianoRollVanishOffsetX();
            float w_vanishY = GetPianoRollVanishY();
            float w_sway = GetPianoRollSway(in_x, in_depth);
            float w_x = in_x + ((w_vanishX - in_x) * w_curve);
            float w_y = (PianoRollBottom - 2.0f) + ((w_vanishY - (PianoRollBottom - 2.0f)) * w_curve);
            return new PointF(w_x + w_sway, w_y);
        }

        private float GetPianoRollDepthCurve(float in_depth)
        {
            float w_depth = Math.Max(0.0f, Math.Min(1.0f, in_depth));
            return (0.0025f * w_depth * w_depth * w_depth) - (1.0025f * w_depth * w_depth) + (2.0f * w_depth);
        }

        private Color GetPianoRollDepthColor(Color in_color, float in_depth)
        {
            float w_depth = Math.Max(0.0f, Math.Min(1.0f, in_depth));
            float w_brightness = 1.0f - (w_depth * 0.85f);
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, (int)(in_color.R * w_brightness))),
                Math.Max(0, Math.Min(255, (int)(in_color.G * w_brightness))),
                Math.Max(0, Math.Min(255, (int)(in_color.B * w_brightness))));
        }

        private void DrawPianoRollAtmosphere(Graphics in_graphics)
        {
            switch (g_piano_roll_style)
            {
                case 1:
                case 6:
                    for (int i = 0; i < 5; i++)
                    {
                        using (Pen w_pen = new Pen(Color.FromArgb(34, 160 + i * 14, 220, 255), 2.0f))
                        {
                            float w_y = PianoRollBottom - 36 - i * 28;
                            in_graphics.DrawBezier(w_pen, 0, w_y, g_orgBitmap.Width * 0.27f, w_y - 54, g_orgBitmap.Width * 0.64f, w_y + 44, g_orgBitmap.Width, w_y - 20);
                        }
                    }
                    break;
                case 2:
                case 7:
                    for (int i = 0; i < 8; i++)
                    {
                        float w_x = 40 + i * 82;
                        using (Pen w_pen = new Pen(Color.FromArgb(46, 230, 240, 255), 1.0f))
                        {
                            in_graphics.DrawLine(w_pen, w_x, PianoRollBottom, g_orgBitmap.Width / 2, 12 + i * 2);
                        }
                    }
                    break;
                case 3:
                case 9:
                    using (Brush w_brush = new SolidBrush(Color.FromArgb(36, 0, 255, 150)))
                    {
                        for (int i = 0; i < 18; i++)
                        {
                            int w_x = (i * 41 + g_piano_roll_phase * 3) % g_orgBitmap.Width;
                            in_graphics.FillRectangle(w_brush, w_x, 0, 2, PianoRollBottom);
                        }
                    }
                    break;
            }
        }

        private void DrawPianoRollNoteShape(Graphics in_graphics, PointF[] in_poly, Brush in_brush, Pen in_pen, float in_scale)
        {
            switch (g_piano_roll_style % 5)
            {
                case 1:
                    in_graphics.FillClosedCurve(in_brush, in_poly);
                    in_graphics.DrawClosedCurve(in_pen, in_poly);
                    break;
                case 2:
                    PointF w_center = new PointF((in_poly[0].X + in_poly[2].X) / 2.0f, (in_poly[0].Y + in_poly[2].Y) / 2.0f);
                    float w_rx = Math.Max(2.0f, Math.Abs(in_poly[1].X - in_poly[0].X) * 0.7f);
                    float w_ry = Math.Max(2.0f, Math.Abs(in_poly[3].Y - in_poly[0].Y) + 3.0f * in_scale);
                    in_graphics.FillEllipse(in_brush, w_center.X - w_rx, w_center.Y - w_ry, w_rx * 2.0f, w_ry * 2.0f);
                    in_graphics.DrawEllipse(in_pen, w_center.X - w_rx, w_center.Y - w_ry, w_rx * 2.0f, w_ry * 2.0f);
                    break;
                case 3:
                    PointF[] w_diamond = {
                        new PointF((in_poly[0].X + in_poly[1].X) / 2.0f, in_poly[0].Y),
                        new PointF(in_poly[1].X, (in_poly[1].Y + in_poly[2].Y) / 2.0f),
                        new PointF((in_poly[2].X + in_poly[3].X) / 2.0f, in_poly[2].Y),
                        new PointF(in_poly[3].X, (in_poly[3].Y + in_poly[0].Y) / 2.0f)
                    };
                    in_graphics.FillPolygon(in_brush, w_diamond);
                    in_graphics.DrawPolygon(in_pen, w_diamond);
                    break;
                default:
                    in_graphics.FillPolygon(in_brush, in_poly);
                    in_graphics.DrawPolygon(in_pen, in_poly);
                    break;
            }
        }

        private Color GetPianoRollBackColorNear()
        {
            Color[] w_colors = {
                Color.FromArgb(72, 190, 225, 255),
                Color.FromArgb(82, 60, 245, 210),
                Color.FromArgb(70, 230, 240, 255),
                Color.FromArgb(86, 20, 220, 150),
                Color.FromArgb(82, 255, 170, 80),
                Color.FromArgb(78, 80, 160, 255),
                Color.FromArgb(86, 255, 110, 210),
                Color.FromArgb(72, 190, 255, 130),
                Color.FromArgb(82, 255, 230, 160),
                Color.FromArgb(70, 80, 255, 120),
                Color.FromArgb(88, 180, 140, 255)
            };
            return w_colors[g_piano_roll_style % w_colors.Length];
        }

        private Color GetPianoRollBackColorFar()
        {
            Color[] w_colors = {
                Color.Black,
                Color.FromArgb(4, 0, 18),
                Color.FromArgb(8, 12, 24),
                Color.FromArgb(0, 8, 4),
                Color.FromArgb(18, 4, 0),
                Color.FromArgb(0, 2, 22),
                Color.FromArgb(22, 0, 18),
                Color.FromArgb(0, 18, 8),
                Color.FromArgb(20, 14, 0),
                Color.FromArgb(0, 10, 0),
                Color.FromArgb(10, 0, 22)
            };
            return w_colors[g_piano_roll_style % w_colors.Length];
        }

        private Color GetPianoRollGridColor()
        {
            Color[] w_colors = {
                Color.FromArgb(70, 95, 100, 120),
                Color.FromArgb(80, 70, 255, 220),
                Color.FromArgb(90, 230, 240, 255),
                Color.FromArgb(76, 0, 255, 150),
                Color.FromArgb(76, 255, 180, 90),
                Color.FromArgb(82, 110, 160, 255),
                Color.FromArgb(84, 255, 120, 230),
                Color.FromArgb(76, 160, 255, 120),
                Color.FromArgb(80, 255, 235, 180),
                Color.FromArgb(76, 80, 255, 110),
                Color.FromArgb(88, 200, 150, 255)
            };
            return w_colors[g_piano_roll_style % w_colors.Length];
        }

        private Color GetPianoRollSparkColor(int in_index)
        {
            Color w_base = GetPianoRollGridColor();
            if ((g_piano_roll_style % 3) == 0 && (in_index % 2) == 0) return Color.White;
            return Color.FromArgb(Math.Min(255, w_base.R + 70), Math.Min(255, w_base.G + 70), Math.Min(255, w_base.B + 70));
        }

        private Color GetPianoRollNoteColor(int in_channel, int in_history)
        {
            Color w_color = g_channel_colors[in_channel];
            if ((g_piano_roll_style % 2) == 1)
            {
                int w_shift = (g_piano_roll_style * 23 + in_history) % 80;
                return Color.FromArgb(Math.Min(255, w_color.R + w_shift), Math.Min(255, w_color.G + 40), Math.Min(255, w_color.B + 60));
            }
            return w_color;
        }

        private Color GetPianoRollEdgeColor()
        {
            return (g_piano_roll_style % 4) switch
            {
                1 => Color.Cyan,
                2 => Color.White,
                3 => Color.FromArgb(180, 255, 255, 160),
                _ => Color.White,
            };
        }

        private float GetPianoRollProjectionPower()
        {
            float[] w_power = { 1.35f, 1.12f, 1.65f, 1.0f, 1.42f, 1.25f, 1.58f, 1.18f, 1.48f, 1.08f, 1.72f };
            return w_power[g_piano_roll_style % w_power.Length];
        }

        private float GetPianoRollVanishOffsetX()
        {
            float[] w_offset = { 0.0f, -90.0f, 84.0f, 0.0f, -52.0f, 60.0f, 0.0f, -120.0f, 120.0f, 0.0f, -30.0f };
            return w_offset[g_piano_roll_style % w_offset.Length];
        }

        private float GetPianoRollVanishY()
        {
            float[] w_y = { 0.0f, 20.0f, -18.0f, 0.0f, 34.0f, -8.0f, 12.0f, 0.0f, 26.0f, -14.0f, 8.0f };
            return w_y[g_piano_roll_style % w_y.Length];
        }

        private float GetPianoRollSway(float in_x, float in_depth)
        {
            return (g_piano_roll_style % 4) switch
            {
                1 => (float)Math.Sin((in_depth * 8.0f) + (g_piano_roll_phase * 0.03f)) * 18.0f * in_depth,
                2 => (float)Math.Cos((in_x * 0.025f) + (in_depth * 5.0f)) * 10.0f * in_depth,
                3 => (float)Math.Sin((in_x * 0.018f) + (g_piano_roll_phase * 0.05f)) * 14.0f * in_depth,
                _ => 0.0f,
            };
        }

        private float PitchToPianoRollX(int in_keyNumber)
        {
            int w_key = in_keyNumber % 12;
            float w_whiteWidth = GetWhiteKeyWidth();
            int w_whiteIndex = GetWhiteKeyIndex(in_keyNumber);
            if (KEY_COLOR[w_key] == 0)
            {
                return 8.0f + (w_whiteIndex * w_whiteWidth);
            }

            int w_prevWhiteIndex = Math.Max(0, w_whiteIndex - 1);
            return 8.0f + ((w_prevWhiteIndex + 0.68f) * w_whiteWidth);
        }

        private float KeyToPianoRollWidth(int in_keyNumber)
        {
            float w_whiteWidth = GetWhiteKeyWidth();
            return Math.Max(3.0f, w_whiteWidth * (KEY_COLOR[in_keyNumber % 12] == 1 ? 0.58f : 0.96f));
        }

        private float GetWhiteKeyWidth()
        {
            return (g_orgBitmap.Width - 16.0f) / Math.Max(1, CountWhiteKeys());
        }

        private int CountWhiteKeys()
        {
            int w_count = 0;
            for (int key = 0; key < KEY_SCALE_LIST.Length; key++)
            {
                if (KEY_COLOR[key % 12] == 0) w_count++;
            }
            return w_count;
        }

        private int GetWhiteKeyIndex(int in_keyNumber)
        {
            int w_count = 0;
            for (int key = 0; key < in_keyNumber; key++)
            {
                if (KEY_COLOR[key % 12] == 0) w_count++;
            }
            return w_count;
        }

        private void DrawHorizontalKeyboard(Graphics in_graphics)
        {
            int w_y = PianoRollBottom;
            using (Pen w_pen = new Pen(Color.Black, 1.0f))
            {
                using (Brush w_whiteBrush = new SolidBrush(Color.White))
                {
                    in_graphics.FillRectangle(w_whiteBrush, 0, w_y, g_orgBitmap.Width, PianoKeyboardHeight);
                }
                for (int key = 0; key < KEY_SCALE_LIST.Length; key++)
                {
                    if (KEY_COLOR[key % 12] == 1) continue;

                    float w_x = PitchToPianoRollX(key);
                    float w_w = KeyToPianoRollWidth(key);
                    if (g_active_key_channel[key] >= 0)
                    {
                        using (Brush w_activeBrush = new SolidBrush(GetActiveKeyColor(g_active_key_channel[key], false)))
                        {
                            in_graphics.FillRectangle(w_activeBrush, w_x, w_y, w_w, PianoKeyboardHeight - 1);
                        }
                    }
                    in_graphics.DrawRectangle(w_pen, w_x, w_y, w_w, PianoKeyboardHeight - 1);
                }
            }

            using (Pen w_pen = new Pen(Color.FromArgb(80, 0, 0, 0), 1.0f))
            {
                for (int key = 0; key < KEY_SCALE_LIST.Length; key++)
                {
                    if (KEY_COLOR[key % 12] == 0) continue;

                    float w_x = PitchToPianoRollX(key);
                    float w_w = KeyToPianoRollWidth(key);
                    RectangleF w_rect = new RectangleF(w_x, w_y, w_w, PianoKeyboardHeight * 0.62f);
                    using (Brush w_keyBrush = new SolidBrush(g_active_key_channel[key] >= 0 ? GetActiveKeyColor(g_active_key_channel[key], true) : Color.Black))
                    {
                        in_graphics.FillRectangle(w_keyBrush, w_rect);
                    }
                    in_graphics.DrawRectangle(w_pen, w_rect.X, w_rect.Y, w_rect.Width, w_rect.Height);
                }
            }
        }

        private Color GetActiveKeyColor(int in_channel, bool in_blackKey)
        {
            Color w_color = g_channel_colors[Math.Max(0, Math.Min(g_channel_colors.Length - 1, in_channel))];
            if (in_blackKey)
            {
                return Color.FromArgb(
                    Math.Min(255, (w_color.R / 2) + 90),
                    Math.Min(255, (w_color.G / 2) + 90),
                    Math.Min(255, (w_color.B / 2) + 90));
            }
            return Color.FromArgb(
                Math.Min(255, w_color.R + 85),
                Math.Min(255, w_color.G + 85),
                Math.Min(255, w_color.B + 85));
        }
        private int key_number_chk(float freq)
        {
            int w_out = KEY_SCALE_LIST.Length;
            for (int i = 0; i < KEY_SCALE_LIST.Length; i++)
            {
                if (freq <= KEY_SCALE_LIST[i])
                {
                    w_out = i;
                    break;
                }
            }
            return w_out;
        }
        //----------------------------------------------------------------
        ///Event Handling: Screen Operations
        //----------------------------------------------------------------
        private void hScrollBar_Fm1_Scroll(object sender, ScrollEventArgs e)
        {
            md_main.g_md_music.g_master_vol[0] = hScrollBar_Fm1.Value;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void hScrollBar_Fm2_Scroll(object sender, ScrollEventArgs e)
        {
            md_main.g_md_music.g_master_vol[1] = hScrollBar_Fm2.Value;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void hScrollBar_Fm3_Scroll(object sender, ScrollEventArgs e)
        {
            md_main.g_md_music.g_master_vol[2] = hScrollBar_Fm3.Value;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void hScrollBar_Fm4_Scroll(object sender, ScrollEventArgs e)
        {
            md_main.g_md_music.g_master_vol[3] = hScrollBar_Fm4.Value;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void hScrollBar_Fm5_Scroll(object sender, ScrollEventArgs e)
        {
            md_main.g_md_music.g_master_vol[4] = hScrollBar_Fm5.Value;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void hScrollBar_Fm6_Scroll(object sender, ScrollEventArgs e)
        {
            md_main.g_md_music.g_master_vol[5] = hScrollBar_Fm6.Value;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void hScrollBar_Psg1_Scroll(object sender, ScrollEventArgs e)
        {
            md_main.g_md_music.g_master_vol[6] = hScrollBar_Psg1.Value;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void hScrollBar_Psg2_Scroll(object sender, ScrollEventArgs e)
        {
            md_main.g_md_music.g_master_vol[7] = hScrollBar_Psg2.Value;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void hScrollBar_Psg3_Scroll(object sender, ScrollEventArgs e)
        {
            md_main.g_md_music.g_master_vol[8] = hScrollBar_Psg3.Value;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void hScrollBar_Psg4_Scroll(object sender, ScrollEventArgs e)
        {
            md_main.g_md_music.g_master_vol[9] = hScrollBar_Psg4.Value;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void hScrollBar_Master_Scroll(object sender, ScrollEventArgs e)
        {
            md_main.g_md_music.g_master_vol[10] = hScrollBar_Master.Value;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void checkBox_Fm1_CheckedChanged(object sender, EventArgs e)
        {
            md_main.g_md_music.g_master_chk[0] = checkBox_Fm1.Checked;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void checkBox_Fm2_CheckedChanged(object sender, EventArgs e)
        {
            md_main.g_md_music.g_master_chk[1] = checkBox_Fm2.Checked;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void checkBox_Fm3_CheckedChanged(object sender, EventArgs e)
        {
            md_main.g_md_music.g_master_chk[2] = checkBox_Fm3.Checked;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void checkBox_Fm4_CheckedChanged(object sender, EventArgs e)
        {
            md_main.g_md_music.g_master_chk[3] = checkBox_Fm4.Checked;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void checkBox_Fm5_CheckedChanged(object sender, EventArgs e)
        {
            md_main.g_md_music.g_master_chk[4] = checkBox_Fm5.Checked;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void checkBox_Fm6_CheckedChanged(object sender, EventArgs e)
        {
            md_main.g_md_music.g_master_chk[5] = checkBox_Fm6.Checked;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void checkBox_Psg1_CheckedChanged(object sender, EventArgs e)
        {
            md_main.g_md_music.g_master_chk[6] = checkBox_Psg1.Checked;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void checkBox_Psg2_CheckedChanged(object sender, EventArgs e)
        {
            md_main.g_md_music.g_master_chk[7] = checkBox_Psg2.Checked;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void checkBox_Psg3_CheckedChanged(object sender, EventArgs e)
        {
            md_main.g_md_music.g_master_chk[8] = checkBox_Psg3.Checked;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void checkBox_Psg4_CheckedChanged(object sender, EventArgs e)
        {
            md_main.g_md_music.g_master_chk[9] = checkBox_Psg4.Checked;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void checkBox_Master_CheckedChanged(object sender, EventArgs e)
        {
            md_main.g_md_music.g_master_chk[10] = checkBox_Master.Checked;
            md_main.g_md_music.setting();
            md_main.write_setting();
        }
        private void comboBox_viewMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            g_view_mode = (comboBox_viewMode.SelectedIndex == 0) ? MusicViewMode.Scroll : MusicViewMode.PianoRoll3D;
            g_piano_roll_style = Math.Max(0, comboBox_viewMode.SelectedIndex - 1);
            Array.Clear(g_piano_roll_history, 0, g_piano_roll_history.Length);
            g_piano_roll_phase = 0;
            ResetScrollView();
            pictureBox_view.Invalidate();
        }
        private void Form_MUSIC_FormClosing(object sender, FormClosingEventArgs e)
        {
            md_main.g_debugView.music_enable = false;
            WinFormsDebugTools.g_form_setting.update();
            md_main.write_setting();
            e.Cancel = true;
        }
        private void Form_MUSIC_ResizeEnd(object sender, EventArgs e)
        {
            var currentPosition = this.Location;
            g_screen_xpos = currentPosition.X;
            g_screen_ypos = currentPosition.Y;
            md_main.write_setting();
        }
        private void Form_MUSIC_SizeChanged(object? sender, EventArgs e)
        {
            LayoutMusicControls();
            EnsureMusicBitmapSize();
            pictureBox_view.Invalidate();
        }
        private void Form_MUSIC_Shown(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(g_screen_xpos, g_screen_ypos);
        }

        private void LayoutMusicControls()
        {
            int w_margin = 12;
            int w_sideWidth = Math.Max(179, groupBox1.Width);
            int w_viewWidth = Math.Max(160, ClientSize.Width - w_sideWidth - (w_margin * 2));
            int w_viewHeight = Math.Max(120, ClientSize.Height);

            pictureBox_view.Location = new Point(0, 0);
            pictureBox_view.Size = new Size(w_viewWidth, w_viewHeight);
            groupBox1.Location = new Point(w_viewWidth + w_margin, 9);
            comboBox_viewMode.Location = new Point(w_viewWidth + w_margin, groupBox1.Bottom + 6);
            comboBox_viewMode.Width = w_sideWidth;
        }

        private void ResetScrollView()
        {
            using (Graphics g = Graphics.FromImage(g_orgBitmap))
            {
                g.Clear(Color.Black);
                Rectangle w_roll = new Rectangle(0, 0, g_orgBitmap.Width, PianoRollBottom);
                using (System.Drawing.Drawing2D.LinearGradientBrush w_brush = new System.Drawing.Drawing2D.LinearGradientBrush(w_roll, Color.FromArgb(8, 8, 10), Color.FromArgb(52, 205, 238, 255), 90.0f))
                {
                    g.FillRectangle(w_brush, w_roll);
                }
                DrawHorizontalKeyboard(g);
            }
        }
    }
}
