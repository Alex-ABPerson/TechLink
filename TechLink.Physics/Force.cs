using System;

namespace TechLink.Physics
{
    public class Direction
    {

    }

    public class Force
    {
        public double Magnitude;
        public Direction Direction;
    }

    public class Velocity
    {
        public double Speed;
        public Direction Direction;
    }

    public class Acceleration
    {
        public Velocity Velocity;
        public TimeSpan Time;
    }
}
