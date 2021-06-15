using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace CollisionMechanics
{
    class Ball
    {
        public float Radius { get; set; }
        public float Weight { get; set; }
        public Vector2 Position { get; set; } = Vector2.Zero;
        public Vector2 Velocity { get; set; } = Vector2.Zero;
        public string Label { get; set; } = string.Empty;
        public Color Color { get; set; }
        public bool Mark { get; set; }

        public void Go()
        {
            Position += Velocity;
            Mark = false;
        }
    }
}
