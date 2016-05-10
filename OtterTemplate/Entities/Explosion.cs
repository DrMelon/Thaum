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
// Purpose: Explosions!! Pretty self-explanatory.

namespace Thaum.Entities
{
    class Explosion : Entity
    {
        PixelTerrain TerrainRef;
        float Radius;
        float Force;
        float Damage;

        public Explosion(PixelTerrain terrain, Vector2 pos, float rad, float force, float dam)
        {
            TerrainRef = terrain;
            Radius = rad;
            Force = force;
            Damage = dam;
            X = pos.X;
            Y = pos.Y;
            LifeSpan = 1;
        }

        public override void Added()
        {
            base.Added();
            // Make a hole!
            if(TerrainRef != null)
            {
                TerrainRef.MakeHole(new Vector2(X, Y), Radius);
            }

            // Begin animation
            for(int i = 0; i < Radius*1.5; i++)
            {
                Particle explosionParticle = new Particle(X, Y, Assets.GFX_PARTICLE_EXPLODE1, 16, 16);
                explosionParticle.LifeSpan = 15.0f;
                explosionParticle.CenterOrigin = true;
                explosionParticle.Image.Shake = 1;
                explosionParticle.X = Rand.Float(X - Radius/3, X + Radius/3);
                explosionParticle.Y = Rand.Float(Y - Radius/3, Y + Radius/3);
                explosionParticle.SpeedX = Rand.Float(-3 - (Force / 3), 3 + (Force / 3));
                explosionParticle.SpeedY = Rand.Float(-3 - (Force / 3), 3 + (Force / 3));
                explosionParticle.FinalSpeedY = -4.0f;
                explosionParticle.FinalScaleX = 0.1f;
                explosionParticle.FinalScaleY = 0.1f;
                explosionParticle.Image.Scroll = Rand.Float(1.0f, 1.2f);
                this.Scene.Add(explosionParticle);
            }
        }
    }
}
