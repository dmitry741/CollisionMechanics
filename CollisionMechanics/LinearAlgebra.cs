using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionMechanics
{
    class MyMatrix2x2
    {
        double _a11, _a12, _a21, _a22;

        #region === Constructors ===

        public MyMatrix2x2()
        {
            _a11 = _a12 = _a21 = _a22 = 0;
        }

        public MyMatrix2x2(double a11, double a12, double a21, double a22)
        {
            _a11 = a11;
            _a12 = a12;
            _a21 = a21;
            _a22 = a22;
        }

        #endregion

        #region === Properties ===

        public double A11 { get { return _a11; } set { _a11 = value; } }
        public double A12 { get { return _a12; } set { _a12 = value; } }
        public double A21 { get { return _a21; } set { _a21 = value; } }
        public double A22 { get { return _a22; } set { _a22 = value; } }
        public bool IsValid
        {
            get
            {
                return !_a11.Equals(double.NaN) && !_a12.Equals(double.NaN) && !_a21.Equals(double.NaN) && !_a22.Equals(double.NaN);
            }
        }

        public double Det { get { return _a11 * _a22 - _a12 * _a21; } }

        #endregion

        #region === Methods ===

        public bool Invert()
        {
            double det = Det;

            if (Math.Abs(det) < 0.001)
                return false;

            double a11 = _a22 / det;
            double a12 = -_a12 / det;
            double a21 = -_a21 / det;
            double a22 = _a11 / det;

            _a11 = a11;
            _a12 = a12;
            _a21 = a21;
            _a22 = a22;

            return true;
        }

        public static MyMatrix2x2 operator *(MyMatrix2x2 matrix1, MyMatrix2x2 matrix2)
        {
            double a11 = matrix1.A11 * matrix2.A11 + matrix1.A12 * matrix2.A21;
            double a12 = matrix1.A11 * matrix2.A12 + matrix1.A12 * matrix2.A22;
            double a21 = matrix1.A21 * matrix2.A11 + matrix1.A22 * matrix2.A21;
            double a22 = matrix1.A21 * matrix2.A12 + matrix1.A22 * matrix2.A22;

            return new MyMatrix2x2(a11, a12, a21, a22);
        }

        #endregion
    }

    class LinearSolver
    {
        static public Vector Solve2x2(double A11, double A12, double A21, double A22, double B1, double B2, out bool result)
        {
            MyMatrix2x2 A = new MyMatrix2x2(A11, A12, A21, A22);
            double det = A.Det;
            result = false;

            if (Math.Abs(det) < 0.001)
                return new Vector(0, 0);

            MyMatrix2x2 DX = new MyMatrix2x2(B1, A12, B2, A22);
            double detX = DX.Det;

            MyMatrix2x2 DY = new MyMatrix2x2(A11, B1, A21, B2);
            double detY = DY.Det;
            result = true;

            return new Vector(detX / det, detY / det);
        }

        static public void SolveLinear(double X11, double Y11, double X12, double Y12, double X21, double Y21, double X22, double Y22, out bool result, out double X, out double Y)
        {
            X = Y = 0;

            double A11 = Y12 - Y11;
            double A12 = X11 - X12;
            double A21 = Y22 - Y21;
            double A22 = X21 - X22;

            double B1 = A11 * X11 + A12 * Y11;
            double B2 = A21 * X21 + A22 * Y21;

            Vector solution = Solve2x2(A11, A12, A21, A22, B1, B2, out result);

            if (!result)
                return;

            X = solution.X;
            Y = solution.Y;

            double t, s;

            if (X12 - X11 != 0)
            {
                t = (X - X11) / (X12 - X11);
            }
            else
            {
                t = (Y - Y11) / (Y12 - Y11);
            }

            if (X22 - X21 != 0)
            {
                s = (X - X21) / (X22 - X21);
            }
            else
            {
                s = (Y - Y21) / (Y22 - Y21);
            }

            result = (0 <= t && t <= 1) && (0 <= s && s <= 1);
        }
    }
}
