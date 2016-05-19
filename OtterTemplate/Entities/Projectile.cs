using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Otter;

//----------------
// Author: J. Brown (DrMelon)
// Part of the [Thaum] Project.
// Date: 09/05/2016
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

        public string Name;
        public float PhysRadius;
        public int Type;
        public int Effect;
        public int Homing;
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
        public int NumSpawns;
        public string ToSpawn;


        float CurrentTimer;
        Entities.PixelTerrain TheTerrain;

        // Load from XML/JSON
        public Projectile(string XMLFile, Entities.PixelTerrain terrain)
        {
            TheTerrain = terrain;
            SetFromXML(XMLFile);
        }

        // Normal constructor
        public Projectile(string GFXString, float phys, Entities.PixelTerrain terrain)
        {
            // Default
            Type = (int)ExplosionType.Timed;
            Effect = (int)ExplosionEffect.Normal;
            Homing = (int)HomingType.None;
            Yield = 16.0f;
            Force = 8.0f;
            Damage = 50.0f;
            Bias = 4.0f;
            Bounciness = 0.65f;
            Friction = 0.85f;
            WindAffect = 0.0f;
            Wobble = 0.0f;
            FuseLength = 3.0f;
            MaxBounces = 1;
            NumSpawns = 5;
            
            // create ents based on xml stuff
            
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

        // Set projectile's settings from XML file.
        public void SetFromXML(string XMLFile)
        {
            XmlDocument ProjectileXML;
            // First checking to see if it already exists in the projectile database.
            if (Assets.LoadedProjectiles.ContainsKey(XMLFile))
            {
                Assets.LoadedProjectiles.TryGetValue(XMLFile, out ProjectileXML);
            }
            else // Otherwise load and add to database.
            {             
                ProjectileXML = new XmlDocument();
                ProjectileXML.Load(XMLFile);
                Assets.LoadedProjectiles.Add(XMLFile, ProjectileXML);
            }

            // Now build projectile from XML document.
            var parentNode = ProjectileXML.DocumentElement;
            
            if(parentNode.Name != "projectile")
            {
                // Raise exception..! This is not a projectile file.
                Util.Log("Projectile XML file load failure! File: " + XMLFile);
            }
            else
            {
                // Start loading file!
                Name = parentNode.Attributes["name"].ToString();
                foreach(XmlNode childnode in parentNode.ChildNodes)
                {
                    // Walk through file.
                    if(childnode.Name == "explosion_type")
                    {
                        if(childnode.InnerText == "Timed")
                        {
                            Type = (int)ExplosionType.Timed;
                        }
                        if(childnode.InnerText == "Bounces")
                        {
                            Type = (int)ExplosionType.Bounces;
                        }
                        if(childnode.InnerText == "AtRest")
                        {
                            Type = (int)ExplosionType.AtRest;
                        }
                        if(childnode.InnerText == "Instant")
                        {
                            Type = (int)ExplosionType.Instant;
                        }
                    }
                    if(childnode.Name == "explosion_effect")
                    {
                        if(childnode.InnerText == "Normal")
                        {
                            Effect = (int)ExplosionEffect.Normal;
                        }
                        if(childnode.InnerText == "Spawn")
                        {
                            Effect = (int)ExplosionEffect.Spawn;
                            ToSpawn = childnode.AttributeString("projectile_to_spawn");
                        }
                    }
                    if(childnode.Name == "homing_type")
                    {
                        if(childnode.InnerText == "None")
                        {
                            Homing = (int)HomingType.None;
                        }
                        if(childnode.InnerText == "Dumb")
                        {
                            Homing = (int)HomingType.Dumb;
                        }
                        if(childnode.InnerText == "Smart")
                        {
                            Homing = (int)HomingType.Smart;
                        }
                    }
                    if(childnode.Name == "sprite")
                    {
                        // Load sprite info.
                        string imgString = "";
                        foreach (XmlNode spritenode in childnode.ChildNodes)
                        {
                            
                            int numFrames = 0;
                            int imgw = 1;
                            int imgh = 1;

                            if (spritenode.Name == "image")
                            {
                                imgString = Assets.ASSET_BASE_PATH + spritenode.InnerText;
                                imgw = spritenode.AttributeInt("w");
                                imgh = spritenode.AttributeInt("h");
                            }
                            if(spritenode.Name == "frames")
                            {
                                numFrames = spritenode.InnerInt();
                            }
                            if(spritenode.Name == "rotation_style")
                            {
                                if(spritenode.InnerText == "None")
                                {
                                    RotStyle = (int)RotationStyle.None;
                                }
                                if (spritenode.InnerText == "RotateToFace")
                                {
                                    RotStyle = (int)RotationStyle.RotateToFace;
                                }
                                if (spritenode.InnerText == "SpinXVelocity")
                                {
                                    RotStyle = (int)RotationStyle.SpinXVelocity;
                                }
                            }
                            if (spritenode.Name == "anim_style")
                            {
                                if (spritenode.InnerText == "Static")
                                {
                                    AniStyle = (int)AnimStyle.Static;
                                }
                                if (spritenode.InnerText == "Frames")
                                {
                                    AniStyle = (int)AnimStyle.Frames;
                                }
                                if (spritenode.InnerText == "FrameOnVelocity")
                                {
                                    AniStyle = (int)AnimStyle.FrameOnVelocity;
                                }
                            }

                            // Create gfx based on type.
                            if(AniStyle == (int)AnimStyle.Static || numFrames < 2)
                            {
                                Image theImage = new Image(imgString);
                                theImage.CenterOrigin();
                                Graphic = theImage;
                            }
                            else
                            {
                                Spritemap<string> animSprite = new Spritemap<string>(imgString, imgw, imgh);
                                AddGraphic(animSprite);
                                Graphic.CenterOrigin();
                            }

                        }
                    }
                    if(childnode.Name == "physics_radius")
                    {
                        PhysRadius = childnode.InnerFloat();
                    }
                    if (childnode.Name == "physics_bounciness")
                    {
                        Bounciness = childnode.InnerFloat();
                    }
                    if (childnode.Name == "physics_friction")
                    {
                        Friction = childnode.InnerFloat();
                    }
                    if (childnode.Name == "physics_wind")
                    {
                        WindAffect = childnode.InnerFloat();
                    }
                    if (childnode.Name == "physics_wobble")
                    {
                        Wobble = childnode.InnerFloat();
                    }
                    if (childnode.Name == "explosion_fuse")
                    {
                        FuseLength = childnode.InnerFloat();
                    }
                    if (childnode.Name == "explosion_maxbounces")
                    {
                        MaxBounces = childnode.InnerInt();
                    }
                    if (childnode.Name == "explosion_numspawns")
                    {
                        NumSpawns = childnode.InnerInt();
                    }
                    if (childnode.Name == "explosion_yield")
                    {
                        Yield = childnode.InnerFloat();
                    }
                    if (childnode.Name == "explosion_force")
                    {
                        Force = childnode.InnerFloat();
                    }
                    if (childnode.Name == "explosion_damage")
                    {
                        Damage = childnode.InnerFloat();
                    }
                    if (childnode.Name == "explosion_bias")
                    {
                        Bias = childnode.InnerFloat();
                    }
                }
            }



            AddComponent(new Components.PlayerMovement(TheTerrain, (int)PhysRadius));
            Components.PlayerMovement myMovement = GetComponent<Components.PlayerMovement>();
            myMovement.PhysFriction = Friction;
            myMovement.PhysBounce = Bounciness;
            CurrentTimer = FuseLength;


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
            if(RotStyle == (int)RotationStyle.RotateToFace)
            {
                Components.PlayerMovement myMovement = GetComponent<Components.PlayerMovement>();
                Graphic.Angle = (float)Math.Atan2(myMovement.PhysVeloc.Y, myMovement.PhysVeloc.X);
            }
            if (RotStyle == (int)RotationStyle.SpinXVelocity)
            {
                Components.PlayerMovement myMovement = GetComponent<Components.PlayerMovement>();
                Graphic.Smooth = true;
                Graphic.Angle -= (float)myMovement.PhysVeloc.X / 10.0f;
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
            if (Effect == (int)ExplosionEffect.Spawn)
            {
                // Make an explosion with the relevant settings, here.
                Scene.Add(new Explosion(TheTerrain, new Vector2(X, Y + Bias), Yield, Force, Damage));
                // Create new objects
                for(int i = 0; i < NumSpawns; i++)
                {

                }
            }


            // get rid of self!
            RemoveSelf();
        }

    }
}
