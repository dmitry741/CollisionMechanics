using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollisionMechanics
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region members

        Bitmap _bitmap = null;
        readonly Random _rnd = new Random();
        Point _startPoint = new Point();

        readonly Timer _timer = new Timer { Enabled = false };

        // коллекция стенок
        readonly List<Wall> _walls = new List<Wall>();

        // коллекция шаров
        readonly List<Ball> _balls = new List<Ball>();

        // константы
        const int cDs = 16;
        const double cMaxSpeed = 2.5;
        const int cMinRadius = 9;
        const int cMinWeight = 1;
        const int cTimeInterval = 25;

        Wall _wall = null;

        #endregion

        #region private methods

        void Render()
        {
            if (_bitmap == null)
                return;

            Graphics g = Graphics.FromImage(_bitmap);
            g.Clear(Color.White);

            Pen penWall = new Pen(Color.Black, 2.0f);

            // отрисовка стен
            foreach (Wall wall in _walls)
            {
                g.DrawLine(penWall, (float)wall.X1, (float)wall.Y1, (float)wall.X2, (float)wall.Y2);
            }

            // отрисвока шариков
            foreach (Ball ball in _balls)
            {
                double R = ball.Radius;
                SolidBrush bubleBrush = new SolidBrush(ball.Color);
                g.FillEllipse(bubleBrush, new Rectangle((int)(ball.X - R), (int)(ball.Y - R), (int)(2 * R), (int)(2 * R)));
            }

            if (comboBox1.SelectedIndex > 0)
            {
                if (_wall != null)
                {
                    g.DrawLine(penWall, (float)_wall.X1, (float)_wall.Y1, (float)_wall.X2, (float)_wall.Y2);
                }

                const int cS = 2;

                for (int x = 2 * cDs; ; x += cDs)
                {
                    if (x >= pictureBox1.Width - cDs)
                        break;

                    for (int y = 2 * cDs; ; y += cDs)
                    {
                        if (y >= pictureBox1.Height - cDs)
                            break;

                        g.DrawLine(Pens.Gray, x, y - cS, x, y + cS);
                        g.DrawLine(Pens.Gray, x - cS, y, x + cS, y);
                    }
                }
            }

            pictureBox1.Image = _bitmap;
        }

        Point GetNearestPoint(int X, int Y, out bool result)
        {
            Rectangle r = new Rectangle(cDs, cDs, pictureBox1.Width - 2 * cDs, pictureBox1.Height - 2 * cDs);

            if (!r.Contains(X, Y))
            {
                result = false;
                return new Point(0, 0);
            }

            int xmin = 0;
            int ymin = 0;

            // X
            double xc = cDs;

            while (xc < X)
            {
                xc += cDs;
            }

            // Y
            double yc = cDs;

            while (yc < Y)
            {
                yc += cDs;
            }

            PointF[] points = new PointF[4];

            points[0].X = Convert.ToSingle(xc - cDs);
            points[0].Y = Convert.ToSingle(yc - cDs);

            points[1].X = Convert.ToSingle(xc);
            points[1].Y = Convert.ToSingle(yc - cDs);

            points[2].X = Convert.ToSingle(xc);
            points[2].Y = Convert.ToSingle(yc);

            points[3].X = Convert.ToSingle(xc - cDs);
            points[3].Y = Convert.ToSingle(yc);

            double dMin = double.MaxValue;
            double D;

            foreach (PointF point in points)
            {
                D = (X - point.X) * (X - point.X) + (Y - point.Y) * (Y - point.Y);

                if (D < dMin)
                {
                    xmin = Convert.ToInt32(point.X);
                    ymin = Convert.ToInt32(point.Y);
                    dMin = D;
                }
            }

            result = true;

            return new Point(xmin, ymin);
        }

        bool CheckForPlace(int X, int Y, double R)
        {
            bool result = true;
            double D;

            foreach (Ball ball in _balls)
            {
                D = Math.Sqrt((X - ball.X) * (X - ball.X) + (Y - ball.Y) * (Y - ball.Y));

                if (D < R + ball.Radius)
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        void Tick()
        {
            foreach (Ball ball in _balls)
            {
                // сдвигаемся согласно траектории
                ball.Go();

                foreach (Wall wall in _walls)
                {
                    // взаимодействие со стенками
                    MechanicsSystemSolver.Solve(ball, wall); 
                }
            }

            foreach (Ball ball in _balls)
            {
                foreach (Wall wall1 in _walls)
                {
                    // взаимодействие с  углами
                    MechanicsSystemSolver.Solve(ball, wall1.X1, wall1.Y1);
                    MechanicsSystemSolver.Solve(ball, wall1.X2, wall1.Y2);
                }
            }

            for (int i = 0; i < _balls.Count; i++)
            {
                for (int j = i + 1; j < _balls.Count; j++)
                {
                    // взаимодействие шаров между собой
                    MechanicsSystemSolver.Solve(_balls[i], _balls[j]);
                }
            }
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            _timer.Tick += TimerTick;

            // добавляем внешнюю рамку чтобы шарики не улетали
            _walls.Add(new Wall(cDs, cDs, pictureBox1.Width - cDs, cDs));
            _walls.Add(new Wall(pictureBox1.Width - cDs, cDs, pictureBox1.Width - cDs, pictureBox1.Height - cDs));
            _walls.Add(new Wall(pictureBox1.Width - cDs, pictureBox1.Height - cDs, cDs, pictureBox1.Height - cDs));
            _walls.Add(new Wall(cDs, pictureBox1.Height - cDs, cDs, cDs));

            comboBox1.BeginUpdate();
            comboBox1.Items.Add("Просмотр");
            comboBox1.Items.Add("Добавить стенку");
            comboBox1.Items.Add("Добавить шар");
            comboBox1.SelectedIndex = 0;
            comboBox1.EndUpdate();

            comboBox2.BeginUpdate();
            comboBox3.BeginUpdate();

            for (int i = 0; i < 20; i++)
            {
                comboBox2.Items.Add(i + cMinRadius);
                comboBox3.Items.Add(i + cMinWeight);
            }

            comboBox2.SelectedIndex = 2;
            comboBox3.SelectedIndex = 0;

            comboBox2.EndUpdate();
            comboBox3.EndUpdate();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Tick();
            Render();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            Tick();
            Render();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!_timer.Enabled)
            {
                _timer.Interval = cTimeInterval;
                _timer.Start();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
            }
        }

        private void btnClearMap_Click(object sender, EventArgs e)
        {
            _balls.Clear();
            int wallCount = _walls.Count;

            if (wallCount > 4)
            {
                _walls.RemoveRange(4, wallCount - 4);
            }

            if (_timer.Enabled)
            {
                _timer.Stop();
            }

            label5.Text = "0";
            Render();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Point point = GetNearestPoint(e.X, e.Y, out bool result);

            if (!result)
                return;

            if (comboBox1.SelectedIndex == 2) // Добавляем шар
            {
                double R = cMinRadius + comboBox2.SelectedIndex;

                if (!CheckForPlace(Convert.ToInt32(point.X), Convert.ToInt32(point.Y), R))
                    return;

                double xV = cMaxSpeed * _rnd.NextDouble();
                double yV = cMaxSpeed * _rnd.NextDouble();

                if (_rnd.NextDouble() > 0.5)
                {
                    xV *= -1;
                }

                if (_rnd.NextDouble() > 0.5)
                {
                    yV *= -1;
                }

                Ball ball = new Ball
                {
                    Radius = R,
                    Weight = cMinWeight + comboBox3.SelectedIndex,
                    X = point.X,
                    Y = point.Y,
                    Color = panel1.BackColor,
                    Velocity = new Vector(xV, yV)
                };

                _balls .Add(ball);
                label5.Text = _balls.Count.ToString();

                Render();
            }
            else if (comboBox1.SelectedIndex == 1) // добавляем стенку
            {
                _startPoint = point;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _wall = null;
            _startPoint.X = _startPoint.Y = -1;

            bool enable = comboBox1.SelectedIndex == 0;

            btnClearMap.Enabled = enable;
            btnStart.Enabled = enable;
            btnStep.Enabled = enable;
            btnStop.Enabled = enable;

            if (comboBox1.SelectedIndex != 0)
            {
                _timer.Stop();
            }

            Render();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (comboBox1.SelectedIndex == 1) // добавляем стенку
            {
                _ = GetNearestPoint(e.X, e.Y, out bool result);

                if (result)
                {
                    if (_startPoint.X > 0 && _startPoint.Y > 0)
                    {
                        if (_wall != null)
                        {
                            if (_wall.Length > 0)
                            {
                                _walls.Add(_wall);
                            }
                        }
                    }
                }

                _startPoint.X = _startPoint.Y = -1;
                _wall = null;

                Render();
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (comboBox1.SelectedIndex == 1) // добавляем стенку
            {
                if (e.Button == MouseButtons.Left)
                {
                    Point point = GetNearestPoint(e.X, e.Y, out bool result);

                    if (result)
                    {
                        if (_startPoint.X > 0 && _startPoint.Y > 0)
                        {
                            _wall = new Wall(_startPoint.X, _startPoint.Y, point.X, point.Y);
                            Render();
                        }
                    }
                }
            }
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                panel1.BackColor = dlg.Color;
            }
        }
    }
}
