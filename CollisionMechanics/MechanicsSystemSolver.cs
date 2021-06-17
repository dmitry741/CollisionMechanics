using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionMechanics
{
    class MechanicsSystemSolver
    {
        static long s_id = 0;

        // решение квадратного уравнения
        static public bool SolveSquareEquation(double A, double B, double C, out double X1, out double X2)
        {
            X1 = X2 = 0;
            double D = B * B - 4 * A * C;

            if (D < 0)
                return false;

            D = Math.Sqrt(D);

            X1 = (-B + D) / (2 * A);
            X2 = (-B - D) / (2 * A);

            return true;
        }

        // решение задачи с центральным ударом
        static private bool Solve(double m1, double m2, double C1, double C2, out double w1, out double w2, bool bFirst)
        {
            w1 = w2 = 0;

            double A = m2 * (m1 + m2); // m1 * m2 + m2 * m2;
            double B = -2 * C1 * m2;
            double C = C1 * C1 - C2 * m1;

            double x1, x2;

            if (!SolveSquareEquation(A, B, C, out x1, out x2))
                return false;

            if (bFirst)
            {
                w2 = x1;
            }
            else
            {
                w2 = x2;
            }

            w1 = (C1 - m2 * w2) / m1;

            return true;
        }

        static private bool IntersectInPreviousStep(Ball ball, Wall wall)
        {
            double X11 = ball.X - ball.Velocity.X;
            double Y11 = ball.Y - ball.Velocity.Y;
            double X12 = ball.X;
            double Y12 = ball.Y;

            double X21 = wall.X1;
            double Y21 = wall.Y1;
            double X22 = wall.X2;
            double Y22 = wall.Y2;

            LinearSolver.SolveLinear(X11, Y11, X12, Y12, X21, Y21, X22, Y22, out bool result, out _, out _);

            return result;
        }

        static private bool IntersectInPreviousStep(Ball ball, double X, double Y)
        {
            double Xs, Ys;
            bool result = false;
            double r, D;
            const int c_step = 8;
            double t = -1;
            double tStep = 1.0 / (double)c_step;

            for (int i = 0; i < c_step; i++)
            {
                Xs = ball.X + ball.Velocity.X * t;
                Ys = ball.Y + ball.Velocity.Y * t;

                D = Math.Sqrt((Xs - X) * (Xs - X) + (Ys - Y) * (Ys - Y));
                r = Math.Abs(D - ball.R);

                if (r < 0.01)
                {
                    result = true;
                    break;
                }

                t += tStep;
            }

            return result;
        }

        // расстояние между 2-мя точками
        static public double Distance(Point p1, Point p2)
        {
            Vector vecor = new Vector();
            vecor.SetupVector(p1.X, p1.Y, p2.X, p2.Y);
            return vecor.Length;
        }

        // расстояние от точки до прямой
        static public void Distance(Point point, double X1, double Y1, double X2, double Y2, out double D, out bool Result)
        {
            D = 0;
            Result = false;

            double A11 = Y2 - Y1;
            double A12 = X1 - X2;
            double B1 = -A11 * X1 - A12 * Y1;
            double len = Math.Sqrt(A11 * A11 + A12 * A12);

            if (len < 0.001)
                return;

            A11 /= len;
            A12 /= len;
            B1 /= len;

            D = Math.Abs(A11 * point.X + A12 * point.Y + B1);

            double A21 = A12;
            double A22 = -A11;
            double B2 = -A21 * point.X - A22 * point.Y;

            B1 = -B1;
            B2 = -B2;

            Vector solution2x2 = LinearSolver.Solve2x2(A11, A12, A21, A22, B1, B2, out bool result);

            if (!result)
            {
                Result = false;
                return;
            }

            double t;

            if (X2 - X1 != 0)
            {
                t = (solution2x2.X - X1) / (X2 - X1);
            }
            else
            {
                t = (solution2x2.Y - Y1) / (Y2 - Y1);
            }

            Result = (0 <= t && t <= 1);
        }

        // скалярное произведение векторов
        static public double Scalar(Vector v1, Vector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        static private void CorrectPosition(Ball ball, Wall wall)
        {
            double x0 = ball.X;
            double y0 = ball.Y;

            double t = -1;
            double c_dt = 0.0001;
            double f, fn, D1, D, r;

            do
            {
                ball.X = x0 + (ball.Velocity.X * t);
                ball.Y = y0 + (ball.Velocity.Y * t);

                Distance(ball, wall.X1, wall.Y1, wall.X2, wall.Y2, out D, out _);
                r = Math.Abs(D - ball.R);

                if (r < 0.01)
                    break;

                ball.X = x0 + (ball.Velocity.X * (t + c_dt));
                ball.Y = y0 + (ball.Velocity.Y * (t + c_dt));

                Distance(ball, wall.X1, wall.Y1, wall.X2, wall.Y2, out D1, out _);

                f = D - ball.R;
                fn = (D1 - D) / c_dt;

                t -= (f / fn);

            } while (true);
        }

        static private void CorrectPosition(Ball ball1, Ball ball2)
        {
            double x10 = ball1.X;
            double y10 = ball1.Y;
            double x20 = ball2.X;
            double y20 = ball2.Y;

            double t = -1;
            double c_dt = 0.0001;
            double f, fn, D1, D, r;

            do
            {
                ball1.X = x10 + (ball1.Velocity.X * t);
                ball1.Y = y10 + (ball1.Velocity.Y * t);
                ball2.X = x20 + (ball2.Velocity.X * t);
                ball2.Y = y20 + (ball2.Velocity.Y * t);

                D = Distance(ball1, ball2);
                r = Math.Abs(D - ball1.R - ball2.R);

                if (r < 0.01)
                    break;

                ball1.X = x10 + (ball1.Velocity.X * (t + c_dt));
                ball1.Y = y10 + (ball1.Velocity.Y * (t + c_dt));
                ball2.X = x20 + (ball2.Velocity.X * (t + c_dt));
                ball2.Y = y20 + (ball2.Velocity.Y * (t + c_dt));

                D1 = Distance(ball1, ball2);

                f = D - ball1.R - ball2.R;
                fn = (D1 - D) / c_dt;

                t -= (f / fn);

            } while (true);

            if (t > 0 || t < -1)
            {
                t = 0;

                ball1.X = x10 + (ball1.Velocity.X * t);
                ball1.Y = y10 + (ball1.Velocity.Y * t);
                ball2.X = x20 + (ball2.Velocity.X * t);
                ball2.Y = y20 + (ball2.Velocity.Y * t);
            }
        }

        static private void CorrectPosition(Ball ball, double X, double Y)
        {
            double x0 = ball.X;
            double y0 = ball.Y;

            double t = -1;
            double c_dt = 0.0001;
            double f, fn, D1, D, r;

            do
            {
                ball.X = x0 + (ball.Velocity.X * t);
                ball.Y = y0 + (ball.Velocity.Y * t);

                D = Math.Sqrt((ball.X - X) * (ball.X - X) + (ball.Y - Y) * (ball.Y - Y));
                r = Math.Abs(D - ball.R);

                if (r < 0.01)
                    break;

                ball.X = x0 + (ball.Velocity.X * (t + c_dt));
                ball.Y = y0 + (ball.Velocity.Y * (t + c_dt));

                D1 = Math.Sqrt((ball.X - X) * (ball.X - X) + (ball.Y - Y) * (ball.Y - Y));

                f = D - ball.R;
                fn = (D1 - D) / c_dt;

                t -= (f / fn);

            } while (true);
        }

        static private bool CheckCollision(Ball ball1, Ball ball2)
        {
            Vector axe = new Vector(ball2.X - ball1.X, ball2.Y - ball1.Y);
            return Scalar(ball1.Velocity, axe) > 0 || Scalar(ball2.Velocity, axe) < 0;
        }

        static private bool CheckIDs(Ball ball1, Ball ball2)
        {
            bool result = false;

            do
            {
                if ((ball1.id * ball2.id) == 0)
                    break;

                if (ball1.id != ball2.id)
                    break;

                result = true;

            } while (false);

            return result;
        }

        // столкновение шара с углом стенки
        static public void Solve(Ball ball, double X, double Y)
        {
            if (ball.Mark)
                return;

            double D = Math.Sqrt((ball.X - X) * (ball.X - X) + (ball.Y - Y) * (ball.Y - Y));

            if (D > ball.R)
            {
                if (!IntersectInPreviousStep(ball, X, Y))
                    return;
            }

            CorrectPosition(ball, X, Y);

            // вектор соединяющий точку и центр шара
            Vector vAxe = new Vector(ball.X - X, ball.Y - Y);

            double Xn = X + vAxe.Ort.X * ball.R / 16;
            double Yn = Y + vAxe.Ort.Y * ball.R / 16;

            Vector vOrt = vAxe.Ortogonal.Ort;

            double X1 = Xn + vOrt.X * 2 * ball.R;
            double Y1 = Yn + vOrt.Y * 2 * ball.R;
            double X2 = Xn - vOrt.X * 2 * ball.R;
            double Y2 = Yn - vOrt.Y * 2 * ball.R;

            Wall wall = new Wall(X1, Y1, X2, Y2);

            Solve(ball, wall);
        }

        // столкновение шара со стенкой
        static public void Solve(Ball ball, Wall wall)
        {
            if (ball.Mark)
                return;

            // случай 1, расстояние от центра шара до стены меньше радиуса шара
            Distance(ball, wall.X1, wall.Y1, wall.X2, wall.Y2, out double D, out bool result);

            if (D > ball.R || !result)
            {
                if (!IntersectInPreviousStep(ball, wall))
                    return;
            }

            // === correct position ===
            CorrectPosition(ball, wall);
            // ========================

            Vector xAxe = new Vector(wall.X2 - wall.X1, wall.Y2 - wall.Y1);
            Vector yAxe = xAxe.Ortogonal;

            Vector xPro = ball.Velocity.GetProjection(xAxe);
            Vector yPro = ball.Velocity.GetProjection(yAxe);

            ball.Velocity = xPro - yPro;

            ball.Mark = true;
            ball.id = 0;
        }

        // столкновение двух шаров
        static public void Solve(Ball ball1, Ball ball2)
        {
            if (ball1.Mark || ball2.Mark)
                return;

            if (CheckIDs(ball1, ball2))
                return;

            if (!CheckCollision(ball1, ball2))
                return;

            // проверяем расстояние между шарами
            double D = Distance(ball1, ball2);

            if (D > ball1.R + ball2.R)
                return;

            Vector vAxeX = new Vector();
            Vector vAxeY = new Vector();

            // создаем вектор соединяющий центры шаров, на него будем строить проекции
            vAxeX.SetupVector(ball1.X, ball1.Y, ball2.X, ball2.Y);

            // создаем вектор ортогональный предыдущему, вместе они образуют ортогональную систему координат
            vAxeY.X = vAxeX.Y;
            vAxeY.Y = -vAxeX.X;

            // получаем проекции скоростей шаров на вектор вдоль оси Х
            Vector vProectionX1 = ball1.Velocity.GetProjection(vAxeX);
            Vector vProectionX2 = ball2.Velocity.GetProjection(vAxeX);

            // получаем проекции скоростей шаров на вектор вдоль оси Y, эти проекиции не изменятся в результате столкновения
            Vector vProectionY1 = ball1.Velocity.GetProjection(vAxeY);
            Vector vProectionY2 = ball2.Velocity.GetProjection(vAxeY);

            double m1 = ball1.Weight;
            double v1 = vProectionX1.Length;
            if (vProectionX1 * vAxeX < 0) v1 *= (-1);

            double m2 = ball2.Weight;
            double v2 = vProectionX2.Length;
            if ((vProectionX2 * vAxeX) < 0) v2 *= (-1);

            double C1 = m1 * v1 + m2 * v2;
            double C2 = m1 * v1 * v1 + m2 * v2 * v2;

            // В результате решения системы получаем два решения
            // одно решение описывает состояние системы до столкновения
            // другое после столкновения

            if (!Solve(m1, m2, C1, C2, out double w1, out double w2, false))
                return;

            Vector ortX = vAxeX.Ort;

            Vector newProX1 = w1 * ortX;
            Vector newProX2 = w2 * ortX;

            // если получили предыдущее состояние то берем другое решение
            if (vProectionX1.IsEqualTo(newProX1) && vProectionX2.IsEqualTo(newProX2))
            {
                if (!Solve(m1, m2, C1, C2, out w1, out w2, true))
                    return;

                newProX1 = w1 * ortX;
                newProX2 = w2 * ortX;
            }

            // === correct position ===
            CorrectPosition(ball1, ball2);
            // ========================

            ball1.Velocity = newProX1 + vProectionY1;
            ball2.Velocity = newProX2 + vProectionY2;

            ball1.Mark = ball2.Mark = true;

            s_id++;
            ball1.id = ball2.id = s_id;
        }
    }
}
