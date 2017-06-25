﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otter;
using Thaum.Components;

//----------------
// Author: J. Brown (DrMelon)
// Part of the [Thaum] Project.
// Date: 12/05/2016
//----------------
// Purpose: This will be the little units the player moves around the map.



namespace Thaum.Entities
{
    class PlayerUnit : Entity
    {

        public static int SMALL_JUMP = 500;
        public static int MAX_JUMP = 950;

        public static int SMALL_FIRE = 50;
        public static int MAX_FIRE = 2000;

        public string Name;
        public int Health;
        public int HealthDelta;
        public int AimAngle;
        public PlayerMovement myMovement;
        public float JumpCharge;
        public float FireCharge;
        public bool IsFiring;
        public float Refire;
        public Spritemap<string> mySpritemap;

        Image Crosshair;

        public PlayerUnit(float x, float y, PixelTerrain terrain)
        {
            // Set up defaults
            X = x;
            Y = y;
            Health = 100;
            AimAngle = 45;
            Name = "Unnamed Wizard";
            myMovement = new PlayerMovement(terrain, 8);
            myMovement.AllowControl = true;
            AddComponent(myMovement);

            // Add GFX
            mySpritemap = new Spritemap<string>(Assets.GFX_WIZ, 16, 16);

            mySpritemap.Add("idle", new Anim(new int[] { 0 }, new float[] { 6f }));
            mySpritemap.Add("firing", new Anim(new int[] { 1 }, new float[] { 6f }));
            mySpritemap.Play("idle");

            AddGraphic(mySpritemap);
            Graphic.CenterOrigin();

            Crosshair = Image.CreateCircle(4, Color.Red);
            Crosshair.CenterOrigin();
        }

        public void TakeHealth(int HP)
        {
            HealthDelta -= HP;
            // Update HP display

            if(HP <= 0)
            {
                DoDeath();
            }
        }

        public void DoDeath()
        {
            // Die!
            // Make explosion
        }

        public override void Update()
        {
            base.Update();

            if (Refire > 0)
            {
                Refire--;
            }
            

            if(myMovement.AllowControl)
            {
                if (Game.Session("Player1").GetController<ControllerXbox360>().A.Pressed && myMovement.Stable)
                {
                    JumpCharge = SMALL_JUMP;
                }
                else if (Game.Session("Player1").GetController<ControllerXbox360>().A.Down && myMovement.Stable)
                {
                    //Charge jump
                    JumpCharge += 25;
                    if(JumpCharge >= MAX_JUMP)
                    {
                        JumpCharge = MAX_JUMP;
                    }
                }
                if (Game.Session("Player1").GetController<ControllerXbox360>().A.Released && myMovement.Stable)
                {
                    Jump();
                }

                if (Game.Session("Player1").GetController<ControllerXbox360>().X.Pressed)
                {
                    IsFiring = true;
                    FireCharge = SMALL_FIRE;
                    
                }
                else if(Game.Session("Player1").GetController<ControllerXbox360>().X.Down)
                {
                    if (Refire <= 0)
                    {
                        FireCharge += 25;
                        if (FireCharge >= MAX_FIRE)
                        {
                            FireCharge = MAX_FIRE;
                            // force fire
                            Game.Session("Player1").GetController<ControllerXbox360>().X.ForceState(false);
                        }
                    }                   


                }
                else if(Game.Session("Player1").GetController<ControllerXbox360>().X.Released)
                {
                    // use weap. 
                    Entities.Projectile newProjectile = new Entities.Projectile(Assets.PROJ_TEST, myMovement.TheTerrain);
                    newProjectile.X = X;
                    newProjectile.Y = Y;
                    Vector2 LaunchVector = new Vector2();

                    LaunchVector.X = (float)Math.Cos(AimAngle * (Math.PI / 180.0f)) * myMovement.Facing;
                    LaunchVector.Y = -(float)Math.Sin(AimAngle * (Math.PI / 180.0f));

                    newProjectile.Launch(LaunchVector * FireCharge);
                    Scene.Add(newProjectile);
                    IsFiring = false;
                    FireCharge = 0;
                    Refire = 2.0f;
                    Game.Session("Player1").GetController<ControllerXbox360>().X.ReleaseState();
                }

                if(Game.Session("Player1").GetController<ControllerXbox360>().DPad.Y < -0.2)
                {
                    AimAngle += 1;
                }
                if (Game.Session("Player1").GetController<ControllerXbox360>().DPad.Y > 0.2)
                {
                    AimAngle -= 1;
                }

                AimAngle = (int)Util.Clamp(AimAngle, -90.0f, 90.0f);
                if(myMovement.Facing > 0)
                {
                    Graphic.ScaleX = 1.0f;
                }
                if(myMovement.Facing < 0)
                {
                    Graphic.ScaleX = -1.0f;
                }
            }

        }

        public void Jump()
        {
            // Hop forwards
            myMovement.Stable = false;
            myMovement.PhysVeloc.X += 150 * myMovement.Facing;
            myMovement.PhysVeloc.Y -= JumpCharge;
            JumpCharge = 0;
        }

        public override void Render()
        {
            base.Render();

            // Render crosshair
            if(myMovement.AllowControl)
            {
                Crosshair.X = X + (((float)Math.Cos(AimAngle * (Math.PI / 180.0f)) * myMovement.Facing) * 32.0f);
                Crosshair.Y = Y + ((-(float)Math.Sin(AimAngle * (Math.PI / 180.0f))) * 32.0f);
                Crosshair.Render();
            }
            if(IsFiring)
            {
                // Show charge
                Draw.RoundedLine(X, Y, Util.Lerp(X, Crosshair.X, FireCharge/MAX_FIRE), Util.Lerp(Y, Crosshair.Y, FireCharge / MAX_FIRE), new Color(1.0f - FireCharge / MAX_FIRE, FireCharge / MAX_FIRE, 0.0f, 1.0f), 2);

                mySpritemap.Play("firing", false);
            }
            else
            {
                mySpritemap.Play("idle", false);

            }

        }
    }
}
