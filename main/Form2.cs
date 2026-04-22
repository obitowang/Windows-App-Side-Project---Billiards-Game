using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace remake
{
    public partial class Form2 : Form
    {
        int b0id, b1id;   // 用來記錄 需 拉回的球 的號碼
        Pen penRed, penGreen, penBlue;   // 宣告 3 支筆
        static Graphics g;    // 繪圖裝置
        static int r = 10, r2 = 20;    // 半徑，直徑
        static double fr = 0;

        // ★ 控制畫線的開關與備份變數
        bool showLines = false;
        double redSpd, redCos, redSin; // 用來備份紅線(碰撞前)的數據

        static int width = 0, height = 0;  // 球桌 寬，高

        class ball
        {
            public int id;                 // 球編號
            public double x = 0, y = 0;    // 球心 坐標
            Color c;                       // 球顏色
            SolidBrush br;                 // 刷子

            // public 權限，確保 hit_speed 可讀取
            public double ang = 0;

            public double cosA, sinA;      // coSine, Sine
            public double spd = 0;

            public void rebound()
            {  // 球碰邊反彈
                if (x < r || x > width - r)
                {
                    setAng(Math.PI - ang);
                    if (x < r) x = r;
                    else x = width - r;
                }
                else if (y < r || y > height - r)
                {
                    setAng(-ang);
                    if (y < r) y = r;
                    else y = height - r;
                }
            }
            public void move()
            {  // 移動球
                if (spd > 0)
                {
                    x += spd * cosA;
                    y += spd * sinA;
                    spd -= fr;
                }
                else spd = 0;
            }
            public ball(int bx, int by, Color cc, int i)
            {
                x = bx; y = by; c = cc; br = new SolidBrush(cc); id = i;
            }
            public void draw()
            {
                g.FillEllipse(br, (int)(x - r), (int)(y - r), r2, r2);
            }
            public void setAng(double _ang)
            {
                ang = _ang;
                cosA = Math.Cos(ang);
                sinA = Math.Sin(ang);
            }
            public void drawStick()
            {
                double r12 = 12 * r;
                Pen skyBluePen = new Pen(Brushes.DeepSkyBlue);
                skyBluePen.Width = 3.0F;
                g.DrawLine(skyBluePen,
                     (float)(x - r12 * cosA), (float)(y - r12 * sinA),
                     (float)(x - r * cosA), (float)(y - r * sinA)
                );
            }
        }

        ball[] balls = new ball[10];

        // ★★★ 修正後的 hit_speed (加入防呆檢查) ★★★
        private void hit_speed(ball b0, ball b1)
        {
            // 1. V0: B0 原方向向量
            double v0x = b0.spd * b0.cosA;
            double v0y = b0.spd * b0.sinA;

            // 2. 計算兩球中心連線方向 (angLine)
            double dx = b1.x - b0.x;
            double dy = b1.y - b0.y;
            double angLine = Math.Atan2(dy, dx);

            // 3. V1 投影長度計算
            double v1_len = b0.spd * Math.Cos(b0.ang - angLine);

            // ★★★ 關鍵修正：如果球正在遠離 (投影<=0)，則不進行物理碰撞計算 ★★★
            if (v1_len <= 0) return;

            // 4. 設定 B1 的新角度與速度
            b1.setAng(angLine);
            b1.spd = Math.Abs(v1_len);

            // 5. V1 的向量分量
            double v1x = b1.spd * b1.cosA;
            double v1y = b1.spd * b1.sinA;

            // 6. V2 = V0 - V1 (向量減法)
            double v2x = v0x - v1x;
            double v2y = v0y - v1y;

            // 7. 設定 B0 的新速度與角度
            b0.spd = Math.Sqrt(v2x * v2x + v2y * v2y); // V2 長度
            b0.setAng(Math.Atan2(v2y, v2x));           // V2 方向
        }

        private void hit(ball b0, ball b1)
        {
            if (b0.spd < b1.spd)
            {   // 交換球，確保 b0 是主動撞擊者
                ball t = b0; b0 = b1; b1 = t;
            }

            double dx = b1.x - b0.x, dy = b1.y - b0.y;

            // 碰撞偵測
            if ((dx * dx + dy * dy) <= (r2 * r2))
            {
                if (checkBox1.Checked)
                {
                    // 暫停模式：停止計時，記錄球號，重繪
                    timer1.Stop();
                    b0id = b0.id; b1id = b1.id;
                    panel2.Refresh();
                }
                else
                {
                    // 無暫停模式：拉回 -> 計算物理 -> 繼續跑
                    pullBack(b0, b1);
                    hit_speed(b0, b1);
                }
            }
        }

        public Form2()
        {
            InitializeComponent();

            // 啟用雙緩衝
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty
              | System.Reflection.BindingFlags.Instance
              | System.Reflection.BindingFlags.NonPublic,
                null, panel2, new object[] { true });

            this.Text = DateTime.Now.ToString("'今天是' M月d日");
            width = panel2.Width;
            height = panel2.Height;

            for (int i = 1; i < 10; i++)
                balls[i] = new ball(150, i * (r2 + 8) + 5, Color.FromArgb(255, (i * 100) % 256, (i * 50) % 256, (i * 25) % 256), i);

            balls[0] = new ball(350, 150, Color.FromArgb(255, 255, 255, 255), 0);
            balls[0].setAng(Math.PI / 4);

            penRed = new Pen(Color.Red, 3);
            penGreen = new Pen(Color.Green, 3);
            penBlue = new Pen(Color.Blue, 3);
            penRed.EndCap = penGreen.EndCap = penBlue.EndCap = LineCap.ArrowAnchor;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            label1.Text = "●歡迎━  " + ((Form1)Owner).accountobx.Text + " 你好●";
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Owner.Show();
        }

        int clickX = -1, clickY = -1;
        bool hasClick = false;

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Owner.Show();
        }

        // ★★★ 拉回按鈕 ★★★
        private void button3_Click(object sender, EventArgs e)
        {
            // 1. 拉回
            pullBack(balls[b0id], balls[b1id]);

            // 2. 備份 B0 原始數據 (畫紅線用)
            redSpd = balls[b0id].spd;
            redCos = balls[b0id].cosA;
            redSin = balls[b0id].sinA;

            // 3. 計算物理碰撞
            hit_speed(balls[b0id], balls[b1id]);

            // 4. 開啟畫線開關，觸發重繪
            showLines = true;
            panel2.Invalidate();
        }

        private void pullBack(ball b0, ball b1)
        {
            int r2r2 = r2 * r2;
            int r4 = 2 * r2;
            for (int px = 0; px < r4; px++)
                if (((b0.x - b1.x) * (b0.x - b1.x) + (b0.y - b1.y) * (b0.y - b1.y)) <= r2r2)
                {
                    b0.x -= b0.cosA; b0.y -= b0.sinA;
                }
                else break;
        }

        // ★★★ 繪圖事件 ★★★
        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. 畫球
            for (int i = 0; i < 10; i++)
            {
                balls[i].draw();
            }

            // 2. 畫碰撞輔助線
            if (showLines == true)
            {
                float m = 3.0f; // 向量長度放大倍率

                // 統一從 B0 球心畫出
                float x0 = (float)balls[b0id].x;
                float y0 = (float)balls[b0id].y;

                // (A) 紅線 V0 (碰撞前 b0)
                float rx = x0 + m * (float)(redSpd * redCos);
                float ry = y0 + m * (float)(redSpd * redSin);

                // (B) 綠線 V1 (碰撞後 b1)
                float gx = x0 + m * (float)(balls[b1id].spd * balls[b1id].cosA);
                float gy = y0 + m * (float)(balls[b1id].spd * balls[b1id].sinA);

                // (C) 藍線 V2 (碰撞後 b0)
                float bx = x0 + m * (float)(balls[b0id].spd * balls[b0id].cosA);
                float by = y0 + m * (float)(balls[b0id].spd * balls[b0id].sinA);

                // --- 繪製向量 ---
                g.DrawLine(penRed, x0, y0, rx, ry);
                g.DrawLine(penGreen, x0, y0, gx, gy);
                g.DrawLine(penBlue, x0, y0, bx, by);
            }

            // 3. 畫球桿
            if (balls[0].spd < 0.0001) balls[0].drawStick();

            // 4. 畫點擊框
            if (hasClick)
            {
                g.DrawRectangle(Pens.HotPink, clickX - 2, clickY - 2, 4, 4);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                timer1.Start();
                showLines = false; // 取消暫停時關閉畫線
            }
        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (balls[0].spd < 0.0001)
            {
                double a = Math.Atan2(e.Y - balls[0].y, e.X - balls[0].x);
                balls[0].setAng(a);
                clickX = e.X;
                clickY = e.Y;
                hasClick = true;
                panel2.Invalidate();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            balls[0].spd = vScrollBar1.Maximum - vScrollBar1.Value;
            fr = (vScrollBar2.Maximum - vScrollBar2.Value) / 50.0;
            timer1.Enabled = true;
            showLines = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            double sum_spd = 0;
            panel2.Invalidate();
            for (int i = 0; i < 10; i++)
            {
                if (balls[i].spd > 0)
                {
                    balls[i].move();
                    balls[i].rebound();
                    sum_spd += balls[i].spd;
                }
                for (int j = i + 1; j < 10; j++)
                {
                    hit(balls[i], balls[j]);
                }
            }

            if (sum_spd <= 0.001)
            {
                timer1.Stop();
                panel2.Refresh();
            }
        }
    }
}