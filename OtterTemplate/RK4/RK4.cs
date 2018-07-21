using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otter;

//----------------
// Author: J. Brown (DrMelon)
// Part of the [Thaum] Project.
// Date: 19/05/2016
//----------------
// Purpose: Runge-Kutta 4th Order Integrator

namespace Thaum.RK4
{
    class RK4
    {
        public static void IntegrateStep(ref State state, float t, float dt, Components.BallisticMovement myMovement)
        {
            // fix timestep
            Derivative a, b, c, d;
            a = new Derivative();
            b = new Derivative();
            c = new Derivative();
            d = new Derivative();

            // 4 derivs
            a = Derivative.Evaluate(state, t, 0.0f, myMovement, new Derivative());
            b = Derivative.Evaluate(state, t, dt * 0.25f, myMovement, a);
            c = Derivative.Evaluate(state, t, dt * 0.5f, myMovement, b);
            d = Derivative.Evaluate(state, t, dt, myMovement, c);

            // Taylor series expansion
            Vector2 VelocChange = 1.0f / 6.0f * (a.Velocity + 2.0f * (b.Velocity + c.Velocity) + d.Velocity);
            Vector2 AccelChange = 1.0f / 6.0f * (a.Acceleration + 2.0f * (b.Acceleration + c.Acceleration) + d.Acceleration);

            myMovement.Entity.X = state.Position.X;
            myMovement.Entity.Y = state.Position.Y;
            myMovement.PhysVeloc.X = state.Velocity.X * 100;
            myMovement.PhysVeloc.Y = state.Velocity.Y * 100;

            //myMovement.MovePixelTerrain();

            state.Position = new Vector2(myMovement.Entity.X, myMovement.Entity.Y);
            state.Velocity = new Vector2(myMovement.PhysVeloc.X / 100, myMovement.PhysVeloc.Y / 100);

            state.Position = state.Position + (VelocChange);// * dt);
            state.Velocity = state.Velocity + (AccelChange);// * dt);

        }
    }
}
