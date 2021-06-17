using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionMechanics
{
    class Point
    {
        protected double _X = 0;
        protected double _Y = 0;

        public Point()
        {
            _X = _Y = 0;
        }

        public Point(double X, double Y)
        {
            _X = X;
            _Y = Y;
        }

        public double X
        {
            get { return _X; }
            set { _X = value; }
        }

        public double Y
        {
            get { return _Y; }
            set { _Y = value; }
        }
    }

    class Vector : Point
    {
        public Vector()
        {
            _X = _Y = 0;
        }

        public Vector(double X, double Y)
        {
            _X = X;
            _Y = Y;
        }

        public void SetupVector(double X1, double Y1, double X2, double Y2)
        {
            _X = X2 - X1;
            _Y = Y2 - Y1;
        }

        public Vector GetProjection(Vector vector)
        {
            double myLen = Length;            
            double K;

            if (myLen > 0)
            {
                double vLen = vector.Length;
                double cosa = (X * vector.X + Y * vector.Y) / (myLen * vLen);
                double len = myLen * cosa;

                K = len / vLen;
            }
            else
            {
                K = 0;
            }

            return K * vector;
        }

        public double Length => Math.Sqrt(X * X + Y * Y);

        public Vector Ort
        {
            get
            {
                double K = 1 / Length;
                return K * this;
            }
        }

        public Vector Ortogonal => new Vector(_Y, -_X);

        public bool IsEqualTo(Vector v)
        {
            return Math.Abs(_X - v.X) + Math.Abs(_Y - v.Y) < 0.00001;
        }

        static public Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }

        static public Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }

        static public Vector operator *(double K, Vector a)
        {
            return new Vector(K * a.X, K * a.Y);
        }

        static public double operator *(Vector a, Vector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
    }
}
