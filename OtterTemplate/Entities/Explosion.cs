using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otter;

//----------------
// Author: J. Brown (DrMelon)
// Part of the [Thaum] Project.
// Date: 09/05/2016
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
            Scene.GetEntity<Entities.CameraShaker>().ShakeCamera();
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
                explosionParticle.SpeedX = Rand.Float(-3 - (Force / 30.0f), 3 + (Force / 30.0f));
                explosionParticle.SpeedY = Rand.Float(-3 - (Force / 30.0f), 3 + (Force / 30.0f));
                explosionParticle.FinalSpeedY = -4.0f;
                explosionParticle.FinalScaleX = 0.1f;
                explosionParticle.FinalScaleY = 0.1f;
                //explosionParticle.Image.Scroll = Rand.Float(1.0f, 1.2f);
                this.Scene.Add(explosionParticle);
            }

            // Get objects in scene around me
            List<PlayerUnit> plys = this.Scene.GetEntities<PlayerUnit>();
            // radius check
            Vector2 myPos = new Vector2(X, Y);
            foreach (PlayerUnit ply in plys)
            {
                Vector2 plyPos = new Vector2(ply.X, ply.Y);

                Vector2 ExplosionVector = (plyPos - myPos);


                if (ExplosionVector.Length <= 2)
                {
                    ExplosionVector.Length = 2; //prevent exponential explosions
                }

                if ((ExplosionVector).Length <= Radius)
                {
                    Components.PlayerMovement plyMove = ply.GetComponent<Components.PlayerMovement>();
                    if (plyMove.Stable)
                    {
                        plyMove.Stable = false;
                        plyMove.PhysVeloc.X = (ExplosionVector).Normalized().X * (1.0f / (ExplosionVector).Length) * Force * 100;
                        plyMove.PhysVeloc.Y = (ExplosionVector).Normalized().Y * (1.0f / (ExplosionVector).Length) * Force * 100;
                    }
                    else
                    {
                        plyMove.PhysVeloc.X += (ExplosionVector).Normalized().X * (1.0f / (ExplosionVector).Length) * Force * 100;
                        plyMove.PhysVeloc.Y += (ExplosionVector).Normalized().Y * (1.0f / (ExplosionVector).Length) * Force * 100;
                    }

                    //plyMove.AllowControl = false; 
                    // 5px radius deadzone for max dam
                    if(ExplosionVector.Length <= 5)
                    {
                        ply.TakeHealth((int)Damage);
                    }
                    else
                    {
                        ply.TakeHealth((int)((1.0f / (ExplosionVector).Length) * Damage));
                    }
                    
                }
            }

            List<Projectile> projs = this.Scene.GetEntities<Projectile>();
            // radius check
            foreach (Projectile proj in projs)
            {
                Vector2 projPos = new Vector2(proj.X, proj.Y);

                Vector2 ExplosionVector = (projPos - myPos);

                if(ExplosionVector.Length <= 2)
                {
                    ExplosionVector.Length = 2; //prevent exponential explosions
                }

                if ((myPos - projPos).Length <= Radius)
                {
                    Components.PlayerMovement projMove = proj.GetComponent<Components.PlayerMovement>();
                    if(projMove.Stable)
                    {
                        projMove.Stable = false;
                        projMove.PhysVeloc.X = (ExplosionVector).Normalized().X * (1.0f / (ExplosionVector).Length) * Force * 100;
                        projMove.PhysVeloc.Y = (ExplosionVector).Normalized().Y * (1.0f / (ExplosionVector).Length) * Force * 100;
                    }
                    else
                    {
                        projMove.PhysVeloc.X += (ExplosionVector).Normalized().X * (1.0f / (ExplosionVector).Length) * Force * 100;
                        projMove.PhysVeloc.Y += (ExplosionVector).Normalized().Y * (1.0f / (ExplosionVector).Length) * Force * 100;
                    }
                    
                    
                }
            }

        }
    }
}
