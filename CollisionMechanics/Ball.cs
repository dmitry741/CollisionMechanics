using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CollisionMechanics
{
    class Ball : Point
    {
        double _radius = 1;
        double _weight = 1;
        Vector _velocity = new Vector(1, 1);
        bool _bMark = false;
        public Color _color = Color.Black;
        long _id = 0;

        public long id
        {
            get { return _id; }
            set { _id = value; }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public bool Mark
        {
            get { return _bMark; }
            set { _bMark = value; }
        }

        public double R
        {
            get { return _radius; }
            set { _radius = value; }
        }

        public double Weight
        {
            get { return _weight; }
            set { _weight = value; }
        }

        public Vector Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        public double KineticEnergy
        {
            get
            {
                double len = _velocity.Length;
                return _weight * len * len / 2;
            }
        }

        public void Go()
        {
            _X += _velocity.X;
            _Y += _velocity.Y;

            _bMark = false;
        }
    }
}
