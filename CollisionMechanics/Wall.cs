using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionMechanics
{
    class Wall
    {
        double _X1 = 0;
        double _Y1 = 0;
        double _X2 = 0;
        double _Y2 = 0;

        public Wall()
        {
            _X1 = 0;
            _Y1 = 0;
            _X2 = 0;
            _Y2 = 0;
        }

        public Wall(double X1, double Y1, double X2, double Y2)
        {
            _X1 = X1;
            _Y1 = Y1;
            _X2 = X2;
            _Y2 = Y2;
        }

        public double Length => Math.Sqrt((_X1 - _X2) * (_X1 - _X2) + (_Y1 - _Y2) * (_Y1 - _Y2));

        public double X1
        {
            get { return _X1; }
            set { _X1 = value; }
        }

        public double Y1
        {
            get { return _Y1; }
            set { _Y1 = value; }
        }

        public double X2
        {
            get { return _X2; }
            set { _X2 = value; }
        }

        public double Y2
        {
            get { return _Y2; }
            set { _Y2 = value; }
        }
    }
}
