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

        public Entities.PixelTerrain TheTerrain;
        
        
        public PlayerMovement(Entities.PixelTerrain terrain, int physradius)
        {
            TheTerrain = terrain;
            Stable = false;

            PhysRadius = physradius;

            WalkSpeed = new Speed(50);
            PhysAccel = new Speed(600);
            PhysVeloc = new Speed(1200);

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

        public override void Update()
        {
            base.Update();

            if(AllowControl)
            {
                if(Scene.Game.Session("Player1").GetController<ControllerXbox360>().DPad.X > 0)
                {
                    WalkSpeed.X = 50;
                    
                }
                if (Scene.Game.Session("Player1").GetController<ControllerXbox360>().DPad.X < 0)
                {
                    WalkSpeed.X = -50;
                }
                if (Scene.Game.Session("Player1").GetController<ControllerXbox360>().DPad.X == 0)
                {
                    WalkSpeed.X = 0;
                    WalkSpeed.Y = 0;
                }
            }

            if(Stable)
            {
                MoveXY((int)WalkSpeed.X, (int)WalkSpeed.Y);
            }
            else
            {
                // Move by physics
                PhysVeloc.X += PhysAccel.X;
                PhysVeloc.Y += PhysAccel.Y;

                MoveXY((int)PhysVeloc.X, (int)PhysVeloc.Y);

                
            }

        }

        public void DrawDebugStuff()
        {
            Vector2 physVec = new Vector2(PhysVeloc.X, PhysVeloc.Y);
            physVec.Normalize();

            physVec *= Math.Max(PhysRadius + 1, PhysVeloc.Length / 100);
            Draw.Line(Entity.X, Entity.Y, physVec.X + Entity.X, physVec.Y + Entity.Y, Color.Green);
        }

        public override void Render()
        {
            base.Render();
            DrawDebugStuff();
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
                            if (ray.X < Entity.X)
                            {
                                Entity.X = ray.X + 1 + PhysRadius / 2;
                            }
                            if (ray.X > Entity.X)
                            {
                                Entity.X = ray.X - PhysRadius / 2;
                            }
                        }
                    }


                    Vector4 rayResult = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X, Entity.Y), new Vector2(Entity.X, Entity.Y + PhysRadius));
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
                    
                    if(rayResult.X != -1)
                    {
                        // Ray actually hit terrain!

                        // Players don't have great bounciness
                        PhysVeloc.Y *= PhysBounce;

                        // But they can skid along terrain
                        PhysVeloc.X *= PhysFriction;

                        

                        // Reflect velocity through surface normal
                        Vector2 SurfaceNormal = TheTerrain.GetSurfaceNormal(new Vector2(rayResult.X, rayResult.Y), 3);
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
                        PopVec *= PhysRadius - (new Vector2(Entity.X - rayResult.X, Entity.Y - rayResult.Y).Length);

                        Entity.X += PopVec.X;
                        Entity.Y += PopVec.Y;

                        


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
