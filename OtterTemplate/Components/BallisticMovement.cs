﻿using System;
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
    class BallisticMovement : Movement
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
        public bool RespondsToExplosiveForces = false;

        public Entities.PixelTerrain TheTerrain;

        public Action<Vector2, Vector2, Vector2> BounceCallback;
        
        public BallisticMovement(Entities.PixelTerrain terrain, int physradius)
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

        public static Vector2 Acceleration(RK4.State state, BallisticMovement myMovement, float dt)
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

        public void RegisterOnBounceCallback(Action<Vector2, Vector2, Vector2> callback)
        {
            BounceCallback += callback;
        }

        public void SpawnDot(float sX, float sY, float sSize, float sTime, Color sCol, String sText)
        {
            return;
            Entity dot = new Entity(sX, sY, Image.CreateCircle((int)sSize, sCol));
            dot.Graphic.CenterOrigin();
            dot.LifeSpan = sTime;
            Entity.Scene.Add(dot);

            Text tempText = new Text(sText, 16);
            //tempText.X = X;
            tempText.Y = - 17;
            dot.AddGraphic(tempText);
        }

        public void DrawDebugStuff()
        {
            return;
            Draw.Circle(Entity.X - (PhysRadius), Entity.Y - (PhysRadius), PhysRadius, Color.None, Color.Cyan, 1);

            Vector2 physVec = new Vector2(PhysVeloc.X, PhysVeloc.Y);
            physVec.Normalize();

            physVec *= Math.Max(PhysRadius + 1, PhysVeloc.Length / 100);
            //Draw.Line(Entity.X, Entity.Y, physVec.X + Entity.X, physVec.Y + Entity.Y, Color.Red, 3);

            // draw rays
            if (Stable)
            {
                Draw.Line(Entity.X - (PhysRadius), Entity.Y, Entity.X + (PhysRadius), Entity.Y, Color.Magenta, 1);
                Draw.Line(Entity.X, Entity.Y, Entity.X, Entity.Y + (PhysRadius) + 1, Color.Magenta, 1);
            }
            else
            {
                Draw.Line(Entity.X - (PhysRadius), Entity.Y, Entity.X + (PhysRadius), Entity.Y, Color.Magenta, 1);
                Draw.Line(Entity.X, Entity.Y, Entity.X + physVec.X, Entity.Y + physVec.Y, Color.Magenta, 1);
            }
            
        }

        public override void Render()
        {
            base.Render();
            DrawDebugStuff();
        }

        public void AttemptHomingBehaviour(bool smart, float TargetX, float TargetY)
        {
            // Get vector to target
            PhysAccel.Y = 0;
            Vector2 vecToTarget = (new Vector2(TargetX, TargetY)) - (new Vector2(Entity.X, Entity.Y));
            // Normalize
            Vector2 normToTarget = vecToTarget.Normalized();

            // Apply thrust
            PhysVeloc.X += normToTarget.X * 100;
            PhysVeloc.Y += normToTarget.Y * 100;
        }

        public static void MovePixelTerrain(ref BallisticMovement movement, ref RK4.State newstate)
        {
            // Test against pix terrain, using surf. normals & bresenham cast
            if (movement.TheTerrain != null && movement.Entity != null)
            {
                // If in walk-mode, check for open air and shift to phys-mode.
                if (movement.Stable)
                {
                    newstate.Velocity.Y = 0;
                    newstate.Velocity.X = 0;

                    // Prevent clipping through walls
                    if (movement.WalkSpeed.X != 0)
                    {
                        Vector4 ray = movement.TheTerrain.GenericBresenhamRaycast(new Vector2(newstate.Position.X - movement.PhysRadius, newstate.Position.Y), new Vector2(newstate.Position.X + movement.PhysRadius, newstate.Position.Y));
                        if (ray.X != -1)
                        {
                            Vector4 ray2 = movement.TheTerrain.GenericBresenhamRaycast(new Vector2(newstate.Position.X, newstate.Position.Y), new Vector2(newstate.Position.X, newstate.Position.Y + (movement.PhysRadius) + 1));
                            if (ray2.X <= -1)
                            {
                                // Ah! We are fallin'.
                                movement.Stable = false;
                                //SpawnDot(ray2.Z, ray2.W, 2, 3.0f * 60.0f, Color.Magenta, "FALL");
                            }
                            if (ray.X < newstate.Position.X && movement.Stable)
                            {
                                newstate.Position.X = ray.X + (movement.PhysRadius);
                                movement.SpawnDot(ray.X, ray.Y, 2, 3.0f * 60.0f, Color.Magenta, "RAYX < ENT X");
                                if (movement.WalkSpeed.X < 0)
                                {
                                    movement.WalkSpeed.X = 0;
                                }
                            }
                            if (ray.Z > newstate.Position.X && movement.Stable)
                            {
                                newstate.Position.X = ray.Z - (movement.PhysRadius);
                                movement.SpawnDot(ray.X, ray.Y, 2, 3.0f * 60.0f, Color.Magenta, "RAYZ > ENT X");
                                if (movement.WalkSpeed.X > 0)
                                {
                                    movement.WalkSpeed.X = 0;
                                }
                            }

                        }
                        else
                        {
                            Vector4 ray2 = movement.TheTerrain.GenericBresenhamRaycast(new Vector2(newstate.Position.X, newstate.Position.Y), new Vector2(newstate.Position.X, newstate.Position.Y + (movement.PhysRadius) + 1));
                            if (ray2.X <= -1)
                            {
                                // Ah! We are fallin'.
                                movement.Stable = false;
                                //SpawnDot(Entity.X, Entity.Y, 2, 3.0f * 60.0f, Color.Cyan, "FALL");
                            }
                        }
                    }

                    // Generic Ground Check
                    Vector4 rayResult = movement.TheTerrain.GenericBresenhamRaycast(new Vector2(newstate.Position.X, newstate.Position.Y), new Vector2(newstate.Position.X, newstate.Position.Y + (movement.PhysRadius) + 1));
                    if (rayResult.X <= -1)
                    {
                        // Ah! We are fallin'.
                        movement.Stable = false;
                        //SpawnDot(Entity.X, Entity.Y, 2, 3.0f * 60.0f, Color.Yellow, "FALL");
                    }
                    else
                    {
                        // Stay on top of terrain
                        newstate.Position.Y = rayResult.W - (movement.PhysRadius);
                        // SpawnDot(rayResult.X, rayResult.Y, 2, 0.1f * 60.0f, Color.Yellow, "GROUNDLEVEL");
                    }



                }
                else
                {
                    // If in phys-mode, check the terrain & do collision response based on speed etc.
                    Vector2 physVec = new Vector2(newstate.Velocity.X, newstate.Velocity.Y);
                    physVec.Normalize();

                    physVec *= Math.Max((movement.PhysRadius) + 1, newstate.Velocity.Length / 100);

                    Vector4 rayResult = movement.TheTerrain.GenericBresenhamRaycast(new Vector2(newstate.Position.X, newstate.Position.Y), new Vector2(newstate.Position.X + physVec.X, newstate.Position.Y + physVec.Y));




                    if (rayResult.X != -1)
                    {
                        // Ray actually hit terrain!



                        // Reflect velocity through surface normal
                        Vector2 SurfaceNormal = movement.TheTerrain.GetSurfaceNormal(new Vector2(rayResult.X, rayResult.Y), 3);
                        Vector2 SurfaceTangent = new Vector2(-SurfaceNormal.Y, -SurfaceNormal.X);
                        SurfaceTangent.Normalize();
                        Vector2 NewVeloc = new Vector2(newstate.Velocity.X, newstate.Velocity.Y);
                        SurfaceNormal.Normalize();


                        // We bounce! Make sure any parent stuff knows about this.
                        movement.BounceCallback?.Invoke(new Vector2(rayResult.X, rayResult.Y), new Vector2(newstate.Velocity.X, newstate.Velocity.Y), new Vector2(SurfaceNormal.X, SurfaceNormal.Y));



                        // Reflect r = d - 2(d . n)n

                        NewVeloc = NewVeloc - 2 * Vector2.Dot(NewVeloc, SurfaceNormal) * SurfaceNormal;

                        newstate.Velocity.X = (int)NewVeloc.X;
                        newstate.Velocity.Y = (int)NewVeloc.Y;

                        // Push-out of terrain

                        // Must find min. axis of seperation.
                        //Vector2 PopVec = new Vector2(newstate.Velocity.X, newstate.Velocity.Y);
                        Vector2 PopVec = new Vector2(SurfaceNormal.X, SurfaceNormal.Y);
                        PopVec.Normalize();
                        PopVec *= (movement.PhysRadius) - (new Vector2(newstate.Position.X - rayResult.Z, newstate.Position.Y - rayResult.W).Length);
                        movement.SpawnDot(rayResult.Z, rayResult.W, 2, 3.0f * 60.0f, Color.Magenta, "POP");

                        newstate.Position.X += PopVec.X;
                        newstate.Position.Y += PopVec.Y;

                        // Mult Velocities by bounce amt
                        newstate.Velocity.X *= movement.PhysBounce;
                        newstate.Velocity.Y *= movement.PhysBounce;

                        newstate.Velocity.X -= (SurfaceTangent.X * newstate.Velocity.X) * (1.0f - movement.PhysFriction);
                        newstate.Velocity.Y -= (SurfaceTangent.Y * newstate.Velocity.Y) * (1.0f - movement.PhysFriction);


                        // If velocity is too low, they are standing.
                        if (Math.Abs(newstate.Velocity.Y) < 100 && Math.Abs(newstate.Velocity.X) < 100)
                        {
                            movement.Stable = true;

                        }


                    }

                    // Wall seperation checker
                    Vector4 wallCheckA = movement.TheTerrain.GenericBresenhamRaycast(new Vector2(newstate.Position.X, newstate.Position.Y), new Vector2(newstate.Position.X + movement.PhysRadius, newstate.Position.Y));
                    Vector4 wallCheckB = movement.TheTerrain.GenericBresenhamRaycast(new Vector2(newstate.Position.X, newstate.Position.Y), new Vector2(newstate.Position.X - movement.PhysRadius, newstate.Position.Y));
                    if (wallCheckA.X != -1 && !movement.Stable)
                    {

                        if (wallCheckA.Z < newstate.Position.X)
                        {
                            newstate.Position.X = wallCheckA.Z + movement.PhysRadius;
                            movement.SpawnDot(wallCheckA.Z, wallCheckA.W, 2, 3.0f * 60.0f, Color.Cyan, "PIP");
                        }
                        if (wallCheckA.Z > newstate.Position.X)
                        {
                            newstate.Position.X = wallCheckA.Z - movement.PhysRadius;
                            movement.SpawnDot(wallCheckA.Z, wallCheckA.W, 2, 3.0f * 60.0f, Color.Yellow, "PAP");
                        }

                    }

                    if (wallCheckB.X != -1 && !movement.Stable)
                    {

                        if (wallCheckB.X < newstate.Position.X)
                        {
                            newstate.Position.X = wallCheckB.X + movement.PhysRadius;
                            movement.SpawnDot(wallCheckB.Z, wallCheckB.W, 2, 3.0f * 60.0f, Color.Cyan, "PIP");
                        }
                        if (wallCheckB.X > newstate.Position.X)
                        {
                            newstate.Position.X = wallCheckB.X - movement.PhysRadius;
                            movement.SpawnDot(wallCheckB.Z, wallCheckB.W, 2, 3.0f * 60.0f, Color.Yellow, "PAP");
                        }

                    }


                }
            }
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
                        Vector4 ray = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X - PhysRadius, Entity.Y), new Vector2(Entity.X + PhysRadius, Entity.Y));
                        if (ray.X != -1)
                        {
                            Vector4 ray2 = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X, Entity.Y), new Vector2(Entity.X, Entity.Y + (PhysRadius) + 1));
                            if (ray2.X <= -1)
                            {
                                // Ah! We are fallin'.
                                Stable = false;
                                //SpawnDot(ray2.Z, ray2.W, 2, 3.0f * 60.0f, Color.Magenta, "FALL");
                            }
                            if (ray.X < Entity.X && Stable)
                            {
                                Entity.X = ray.X + (PhysRadius);
                                SpawnDot(ray.X, ray.Y, 2, 3.0f * 60.0f, Color.Magenta, "RAYX < ENT X");
                                if (WalkSpeed.X < 0)
                                {
                                    WalkSpeed.X = 0;
                                }
                            }
                            if (ray.Z > Entity.X && Stable)
                            {
                                Entity.X = ray.Z - (PhysRadius);
                                SpawnDot(ray.X, ray.Y, 2, 3.0f * 60.0f, Color.Magenta, "RAYZ > ENT X");
                                if (WalkSpeed.X > 0)
                                {
                                    WalkSpeed.X = 0;
                                }
                            }
                            
                        }
                        else
                        {
                            Vector4 ray2 = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X, Entity.Y), new Vector2(Entity.X, Entity.Y + (PhysRadius) + 1));
                            if (ray2.X <= -1)
                            {
                                // Ah! We are fallin'.
                                Stable = false;
                                //SpawnDot(Entity.X, Entity.Y, 2, 3.0f * 60.0f, Color.Cyan, "FALL");
                            }
                        }
                    }

                    // Generic Ground Check
                    Vector4 rayResult = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X, Entity.Y), new Vector2(Entity.X, Entity.Y + (PhysRadius) + 1));
                    if (rayResult.X <= -1)
                    {
                        // Ah! We are fallin'.
                        Stable = false;
                        //SpawnDot(Entity.X, Entity.Y, 2, 3.0f * 60.0f, Color.Yellow, "FALL");
                    }
                    else
                    {
                        // Stay on top of terrain
                        Entity.Y = rayResult.W - (PhysRadius);
                       // SpawnDot(rayResult.X, rayResult.Y, 2, 0.1f * 60.0f, Color.Yellow, "GROUNDLEVEL");
                    }



                }
                else
                {
                    // If in phys-mode, check the terrain & do collision response based on speed etc.
                    Vector2 physVec = new Vector2(PhysVeloc.X, PhysVeloc.Y);
                    physVec.Normalize();

                    physVec *= Math.Max((PhysRadius) + 1, PhysVeloc.Length / 100);

                    Vector4 rayResult = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X, Entity.Y), new Vector2(Entity.X + physVec.X, Entity.Y + physVec.Y));
                    



                    if (rayResult.X != -1)
                    {
                        // Ray actually hit terrain!

                       

                        // Reflect velocity through surface normal
                        Vector2 SurfaceNormal = TheTerrain.GetSurfaceNormal(new Vector2(rayResult.X, rayResult.Y), 3);
                        Vector2 SurfaceTangent = new Vector2(-SurfaceNormal.Y, -SurfaceNormal.X);
                        SurfaceTangent.Normalize();
                        Vector2 NewVeloc = new Vector2(PhysVeloc.X, PhysVeloc.Y);
                        SurfaceNormal.Normalize();


                        // We bounce! Make sure any parent stuff knows about this.
                        BounceCallback?.Invoke(new Vector2(rayResult.X, rayResult.Y), new Vector2(PhysVeloc.X, PhysVeloc.Y), new Vector2(SurfaceNormal.X, SurfaceNormal.Y));

                        

                        // Reflect r = d - 2(d . n)n
                        
                        NewVeloc = NewVeloc - 2 * Vector2.Dot(NewVeloc, SurfaceNormal) * SurfaceNormal;

                        PhysVeloc.X = (int)NewVeloc.X;
                        PhysVeloc.Y = (int)NewVeloc.Y;

                        // Push-out of terrain

                        // Must find min. axis of seperation.
                        //Vector2 PopVec = new Vector2(PhysVeloc.X, PhysVeloc.Y);
                        Vector2 PopVec = new Vector2(SurfaceNormal.X, SurfaceNormal.Y);
                        PopVec.Normalize();
                        PopVec *= (PhysRadius) - (new Vector2(Entity.X - rayResult.Z, Entity.Y - rayResult.W).Length);
                        SpawnDot(rayResult.Z, rayResult.W, 2, 3.0f * 60.0f, Color.Magenta, "POP");

                        Entity.X += PopVec.X;
                        Entity.Y += PopVec.Y;

                        // Mult Velocities by bounce amt
                        PhysVeloc.X *= PhysBounce;
                        PhysVeloc.Y *= PhysBounce;

                        PhysVeloc.X -= (SurfaceTangent.X * PhysVeloc.X) * (1.0f - PhysFriction);
                        PhysVeloc.Y -= (SurfaceTangent.Y * PhysVeloc.Y) * (1.0f - PhysFriction);


                        // If velocity is too low, they are standing.
                        if (Math.Abs(PhysVeloc.Y) < 100 && Math.Abs(PhysVeloc.X) < 100)
                        {
                            Stable = true;
                            
                        }


                    }

                    // Wall seperation checker
                    Vector4 wallCheckA = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X, Entity.Y), new Vector2(Entity.X + PhysRadius, Entity.Y));
                    Vector4 wallCheckB = TheTerrain.GenericBresenhamRaycast(new Vector2(Entity.X, Entity.Y), new Vector2(Entity.X - PhysRadius, Entity.Y));
                    if (wallCheckA.X != -1 && !Stable)
                    {

                        if (wallCheckA.Z < Entity.X)
                        {
                            Entity.X = wallCheckA.Z + PhysRadius;
                            SpawnDot(wallCheckA.Z, wallCheckA.W, 2, 3.0f * 60.0f, Color.Cyan, "PIP");
                        }
                        if (wallCheckA.Z > Entity.X)
                        {
                            Entity.X = wallCheckA.Z - PhysRadius;
                            SpawnDot(wallCheckA.Z, wallCheckA.W, 2, 3.0f * 60.0f, Color.Yellow, "PAP");
                        }

                    }

                    if (wallCheckB.X != -1 && !Stable)
                    {

                        if (wallCheckB.X < Entity.X)
                        {
                            Entity.X = wallCheckB.X + PhysRadius;
                            SpawnDot(wallCheckB.Z, wallCheckB.W, 2, 3.0f * 60.0f, Color.Cyan, "PIP");
                        }
                        if (wallCheckB.X > Entity.X)
                        {
                            Entity.X = wallCheckB.X - PhysRadius;
                            SpawnDot(wallCheckB.Z, wallCheckB.W, 2, 3.0f * 60.0f, Color.Yellow, "PAP");
                        }

                    }


                }
            }
        }


    }
}
