using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otter;
using Thaum.Scenes;

//----------------
// Author: J. Brown (DrMelon)
// Part of the [Thaum] Project.
// Date: 09/05/2016
//----------------
// Purpose: Player movement class; has two states, stable (walking) and physics-driven.

namespace Thaum.Components
{
    class PlayerMovement : Movement
    {
        public bool AllowControl = false;
        public bool Stable;
        public Speed WalkSpeed;

        public Speed PhysAccel;
        public Speed PhysVeloc;
        public int PhysRadius;
        public float PhysBounce;
        public float PhysFriction;
        public int Facing;
        public bool Underwater = false;

        public Entities.PixelTerrain TheTerrain;
        
        
        public PlayerMovement(Entities.PixelTerrain terrain, int physradius)
        {
            TheTerrain = terrain;
            Stable = false;

            PhysRadius = physradius;

            WalkSpeed = new Speed(50);
            PhysAccel = new Speed(600);
            PhysVeloc = new Speed(3200);
            Facing = 1;

            PhysBounce = 0.2f;
            PhysFriction = 0.65f;

            PhysAccel.Y = 45.0f; // gravity

            OnMove = MovePixelTerrain;
        }

        public override void MoveX(int speed, Collider collider = null)
        {
            base.MoveX(speed, collider);

            OnMove();
        }

        public override void MoveY(int speed, Collider collider = null)
        {
            base.MoveY(speed, collider);

            OnMove();
        }

        public static Vector2 Acceleration(RK4.State state, PlayerMovement myMovement, float dt)
        {
            return (new Vector2(myMovement.PhysAccel.X / 100, myMovement.PhysAccel.Y / 100));
        }

        public override void Update()
        {
            base.Update();

            if(Stable)
            {
                MovePixelTerrain();

                Entity.X += WalkSpeed.X / 100;
                Entity.Y += WalkSpeed.Y / 100;

            }
            else
            {

                //////////////////////////////////////////////////////////////////////////
                /// OLD LOGIC
                /// 

                //PhysVeloc.X += PhysAccel.X;
                //PhysVeloc.Y += PhysAccel.Y;

                // check for collision
                //MovePixelTerrain();

                // go
                //Entity.X += PhysVeloc.X / 100;
                //Entity.Y += PhysVeloc.Y / 100;

                //////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////



                // Move by physics
                RK4.State currentState = new RK4.State();
                currentState.Velocity = new Vector2(PhysVeloc.X / 100, PhysVeloc.Y / 100);
                currentState.Position = new Vector2(Entity.X, Entity.Y);
                RK4.RK4.IntegrateStep(ref currentState, Game.Instance.Timer, BattleScene.TimeScale, this);


                Entity.X = currentState.Position.X;
                Entity.Y = currentState.Position.Y;
                PhysVeloc.X = currentState.Velocity.X * 100;
                PhysVeloc.Y = currentState.Velocity.Y * 100;

                MovePixelTerrain();


                if (PhysVeloc.X < 0)
                {
                    Facing = -1;
                }
                if(PhysVeloc.X > 0)
                {
                    Facing = 1;
                }


            }

            if(Entity.Y > TheTerrain.Graphic.Height)
            {
                //Underwater!
                PhysVeloc.Y *= 0.05f;
                PhysVeloc.X *= 0.05f;
                Stable = false;
                if(!Underwater)
                {
                    Entity.Timer = 0;
                    Entity.LifeSpan = 90.0f;
                    Underwater = true;
                }
               
            }



        }

        public void DrawDebugStuff()
        {
            Vector2 physVec = new Vector2(PhysVeloc.X, PhysVeloc.Y);
            physVec.Normalize();

            physVec *= Math.Max(PhysRadius + 1, PhysVeloc.Length / 100);
            Draw.Line(Entity.X, Entity.Y, physVec.X + Entity.X, physVec.Y + Entity.Y, Color.Red, 3);
        }

        public override void Render()
        {
            base.Render();
            //DrawDebugStuff();
        }

        public void MovePixelTerrain()
        {
            // Test against pix terrain, using surf. normals & bresenham cast
            if(TheTerrain != null && this.Entity != null)
            {
                // If in walk-mode, check for open air and shift to phys-mode.
                if(Stable)
                {
                    PhysVeloc.Y = 0;
                    PhysVeloc.X = 0;

                    // Prevent clipping through walls
                    if(WalkSpeed.X != 0)
                    {
                        Vector4 ray = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X - PhysRadius / 2, Entity.Y), new Vector2(Entity.X + PhysRadius / 2, Entity.Y));
                        if (ray.X > -1)
                        {
                            Vector4 ray2 = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X, Entity.Y), new Vector2(Entity.X, Entity.Y + PhysRadius + 1));
                            if (ray2.X <= -1)
                            {
                                // Ah! We are fallin'.
                                Stable = false;

                            }
                            if (ray.X < Entity.X)
                            {
                                Entity.X = ray.X + PhysRadius / 2;
                                if (WalkSpeed.X < 0)
                                {
                                    WalkSpeed.X = 0;
                                }
                            }
                            if (ray.Z > Entity.X)
                            {
                                Entity.X = ray.Z - PhysRadius / 2;
                                if(WalkSpeed.X > 0)
                                {
                                    WalkSpeed.X = 0;
                                }
                            }
                            
                        }
                        else
                        {
                            Vector4 ray2 = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X, Entity.Y), new Vector2(Entity.X, Entity.Y + PhysRadius + 1));
                            if (ray2.X <= -1)
                            {
                                // Ah! We are fallin'.
                                Stable = false;

                            }
                        }
                    }


                    Vector4 rayResult = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X, Entity.Y), new Vector2(Entity.X, Entity.Y + PhysRadius + 1));
                    if (rayResult.X <= -1)
                    {
                        // Ah! We are fallin'.
                        Stable = false;
                        
                    }
                    else
                    {
                        // Stay on top of terrain
                        Entity.Y = rayResult.W - PhysRadius;
                    }



                }
                else
                {
                    // If in phys-mode, check the terrain & do collision response based on speed etc.
                    Vector2 physVec = new Vector2(PhysVeloc.X, PhysVeloc.Y);
                    physVec.Normalize();

                    physVec *= Math.Max(PhysRadius + 1, PhysVeloc.Length / 100);

                    Vector4 rayResult = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X, Entity.Y), new Vector2(Entity.X + physVec.X, Entity.Y + physVec.Y));
                    Vector4 wallCheck = TheTerrain.GenericBresenhamCircleCast(new Vector2(Entity.X - PhysRadius / 2, Entity.Y), new Vector2(Entity.X + PhysRadius / 2, Entity.Y), PhysRadius);

                    if (wallCheck.X > -1)
                    {

                        if (wallCheck.X < Entity.X)
                        {
                            Entity.X = wallCheck.X + PhysRadius / 2;
                        }
                        if (wallCheck.Z > Entity.X)
                        {
                            Entity.X = wallCheck.Z - PhysRadius / 2;
                        }

                    }

                    if (rayResult.X != -1)
                    {
                        // Ray actually hit terrain!

                       

                        // Reflect velocity through surface normal
                        Vector2 SurfaceNormal = TheTerrain.GetSurfaceNormal(new Vector2(rayResult.X, rayResult.Y), 3);
                        Vector2 SurfaceTangent = new Vector2(-SurfaceNormal.Y, -SurfaceNormal.X);
                        SurfaceTangent.Normalize();
                        Vector2 NewVeloc = new Vector2(PhysVeloc.X, PhysVeloc.Y);

                       

                        SurfaceNormal.Normalize();

                        // Reflect r = d - 2(d . n)n
                        
                        NewVeloc = NewVeloc - 2 * Vector2.Dot(NewVeloc, SurfaceNormal) * SurfaceNormal;

                        PhysVeloc.X = (int)NewVeloc.X;
                        PhysVeloc.Y = (int)NewVeloc.Y;

                        // Push-out of terrain

                        // Must find min. axis of seperation.
                        Vector2 PopVec = new Vector2(PhysVeloc.X, PhysVeloc.Y);
                        PopVec.Normalize();
                        PopVec *= (PhysRadius) - (new Vector2(Entity.X - rayResult.Z, Entity.Y - rayResult.W).Length);

                        Entity.X += PopVec.X;
                        Entity.Y += PopVec.Y;

                        // Mult Velocities by bounce amt
                        PhysVeloc.X *= PhysBounce;
                        PhysVeloc.Y *= PhysBounce;

                        PhysVeloc.X -= (SurfaceTangent.X * PhysVeloc.X) * (1.0f - PhysFriction);
                        PhysVeloc.Y -= (SurfaceTangent.Y * PhysVeloc.Y) * (1.0f - PhysFriction);

                        // Check to see if the parent entity is a projectile. If so, take away the number of remaining bounces & flag for instant death.
                        if(Entity.GetType() == Type.GetType("Thaum.Entities.Projectile"))
                        {
                            Entities.Projectile myProjectile = (Entities.Projectile)Entity;

                            if(myProjectile.Type == (int)Entities.Projectile.ExplosionType.Bounces)
                            {
                                myProjectile.MaxBounces--;
                                if(myProjectile.MaxBounces < 0)
                                {
                                    myProjectile.Detonate();
                                }
                            }

                            if(myProjectile.Type == (int)Entities.Projectile.ExplosionType.Instant)
                            {
                                myProjectile.Detonate();
                            }
                            
                            

                        }


                        // If velocity is too low, they are standing.
                        if (Math.Abs(PhysVeloc.Y) < 100 && Math.Abs(PhysVeloc.X) < 100)
                        {
                            Stable = true;
                            
                        }


                    }


                }
            }
        }


    }
}
