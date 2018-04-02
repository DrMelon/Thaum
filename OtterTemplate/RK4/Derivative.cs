using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otter;
using Thaum.Components;

namespace Thaum.RK4
{
    class Derivative
    {
        public Vector2 Velocity;
        public Vector2 Acceleration;

        public static Derivative Evaluate(State initial, float t, float dt, BallisticMovement myMovement, Derivative derivative)
        {
            State newstate = new State();
            newstate.Position = initial.Position + derivative.Velocity * dt;
            newstate.Velocity = initial.Velocity + derivative.Acceleration * dt;

            Derivative output = new Derivative();
            output.Velocity = newstate.Velocity;
            output.Acceleration = BallisticMovement.Acceleration(newstate, myMovement, dt);

            return output;
        }
    }
}
