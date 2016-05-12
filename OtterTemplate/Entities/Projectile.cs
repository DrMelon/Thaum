using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otter;

//----------------
// Author: J. Brown (DrMelon)
// Part of the [Thaum] Project.
// Date: 09/03/2016
//----------------
// Purpose: Projectiles are the meat and potatoes of any artillery-strategy game.
//  All spells/weapons will launch projectile-based attacks, with sub-classes handling any specifics.
//  Projectiles have a few options:
//      Explosion Type: Timed, # Bounces, Instantaneous, At Rest Timed
//      Explosion Yield: px Radius
//      Explosion Force: px/s Impulse
//      Explosion Damage: hp at epicenter
//      Explosion Bias: Offset on Y-axis to allow for deeper trenches.
//      Explosion Effect: Normal, Spawn Object(s)
//      Bounciness: Restitution, 0.0f - 1.0f
//      Friction: Friction, 0.0f - 1.0f;
//      Wind Affectation: Degree of wind's influence (0.0f - 1.0f)
//      Homing Type: None, Dumb, Smart
//      Wobble: Degree of wobbling during flight (0.0f - ?.0f)
//      FuseLength: Time in seconds of fuse.
//      MaxBounces: Number of bounces allowed
//      RotationStyle: Rotate-To-Face, Spin On X Velocity, None
//      AnimStyle: Static, Frames, Frames on Velocity

namespace Thaum.Entities
{
    class Projectile : Entity
    {
        public enum ExplosionType { Timed, Bounces, Instant, AtRest };
        public enum ExplosionEffect { Normal, Spawn };
        public enum HomingType { None, Dumb, Smart };
        public enum RotationStyle { RotateToFace, SpinXVelocity, None };
        public enum AnimStyle { Static, Frames, FrameOnVelocity };

        public float PhysRadius;
        public int Type;
        public int Effect;
        public float Yield;
        public float Force;
        public float Damage;
        public float Bias;
        public float Bounciness;
        public float Friction;
        public float WindAffect;
        public float Wobble;
        public float FuseLength;
        public int MaxBounces;
        public int RotStyle;
        public int AniStyle;

        float CurrentTimer;
        Entities.PixelTerrain TheTerrain;

        public Projectile(string GFXString, float phys, Entities.PixelTerrain terrain)
        {
            // Default
            Type = (int)ExplosionType.Timed;
            Effect = (int)ExplosionEffect.Normal;
            Yield = 16.0f;
            Force = 5.0f;
            Damage = 50.0f;
            Bias = 4.0f;
            Bounciness = 0.65f;
            Friction = 0.85f;
            WindAffect = 0.0f;
            Wobble = 0.0f;
            FuseLength = 3.0f;
            MaxBounces = 1;
            AniStyle = (int)AnimStyle.Static;
            RotStyle = (int)RotationStyle.SpinXVelocity;
            PhysRadius = phys;
            CurrentTimer = FuseLength;

            TheTerrain = terrain;

            AddGraphic(new Image(GFXString));
            Graphic.CenterOrigin();
            AddComponent(new Components.PlayerMovement(TheTerrain, (int)PhysRadius));
            Components.PlayerMovement myMovement = GetComponent<Components.PlayerMovement>();
            myMovement.PhysFriction = Friction;
            myMovement.PhysBounce = Bounciness;
        }

        public override void Update()
        {
            base.Update();

            // Check Timed Explosion
            if(Type == (int)ExplosionType.Timed)
            {
                if (Game.FixedFramerate)
                {
                    CurrentTimer -= 1.0f / Game.Framerate;
                }
                else
                {
                    CurrentTimer -= Game.DeltaTime;
                }
                if(CurrentTimer <= 0)
                {
                    Detonate();
                }
            }

        }

        public void Launch(Vector2 launchVector)
        {
            // Hurls the projectile
            Components.PlayerMovement myMovement = GetComponent<Components.PlayerMovement>();
            myMovement.Stable = false;
            myMovement.PhysVeloc.X = launchVector.X;
            myMovement.PhysVeloc.Y = launchVector.Y;
        }

        public void Detonate()
        {
            // Explode!!
            if(Effect == (int)ExplosionEffect.Normal)
            {
                // Make an explosion with the relevant settings, here.
                Scene.Add(new Explosion(TheTerrain, new Vector2(X, Y + Bias), Yield, Force, Damage));
            }


            // get rid of self!
            RemoveSelf();
        }

    }
}
