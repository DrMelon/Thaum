using Otter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//----------------
// Author: J. Brown (DrMelon)
// Part of the [Thaum] Project.
// Date: 09/03/2016
//----------------
// Purpose: Turn-based artillery worms-style battle.


namespace Thaum.Scenes
{
    class BattleScene : Scene
    {
        Entities.PixelTerrain TheTerrain;
        Entities.CameraShaker CamShake;

        Entities.PlayerUnit PlyTest;

        Entity WaterEnt;
        Entity WaterBeneath;

        Entity CamFocus;
        Entity CamTarget;

        public BattleScene()
        {
            TheTerrain = new Entities.PixelTerrain(Assets.GFX_TERRAIN);
            Add(TheTerrain);

            CamShake = new Entities.CameraShaker();
            Add(CamShake);

            PlyTest = new Entities.PlayerUnit(200, 50, TheTerrain);
            Add(PlyTest);

            SetupWater();

            CamFocus = new Entity(PlyTest.X, PlyTest.Y);
            CamTarget = new Entity(PlyTest.X, PlyTest.Y);

            CameraFocus = CamFocus;

            // Set up debug console stuff.
            Otter.Debugger.Instance.RegisterCommands();
        }

        public void SetupWater()
        {
            WaterEnt = new Entity(0, TheTerrain.Graphic.Height - 8, new ImageSet(Assets.GFX_WATER, 32, 16));
            WaterEnt.GetGraphic<ImageSet>().RepeatX = true;
            WaterEnt.GetGraphic<ImageSet>().CenterOrigin();

            WaterEnt.Layer = -10;
            Add(WaterEnt);


            WaterBeneath = new Entity();
            WaterBeneath.AddGraphic(Image.CreateRectangle(Color.White));
            WaterBeneath.Graphic.RepeatX = true;
            WaterBeneath.X = 0;
            WaterBeneath.Y = TheTerrain.Graphic.Height;
            WaterBeneath.Layer = 20;
            Add(WaterBeneath);

            WaterBeneath = new Entity();
            Graphic WaterBlue = Image.CreateRectangle(new Color(99.0f / 255.0f, 155.0f / 255.0f, 1.0f, 0.95f));
            WaterBlue.RepeatX = true;
            WaterBeneath.AddGraphic(WaterBlue);
            WaterBeneath.X = 0;
            WaterBeneath.Y = TheTerrain.Graphic.Height;
            WaterBeneath.Layer = -9;
            Add(WaterBeneath);

            

            
        }

        [OtterCommand(helpText: "Reset XML-based objects.", group: "game")]
        public static void ResetXML()
        {
            // Reset all xml defined objs.
            Assets.LoadedProjectiles.Clear();
        }
       

        public override void Update()
        {
            
            if (Game.Session("Player1").GetController<ControllerXbox360>().B.Pressed)
            {
                Entities.Explosion newExplosion = new Entities.Explosion(TheTerrain, new Vector2(PlyTest.X,PlyTest.Y),20,5,50);
                CamShake.ShakeCamera(10);
                Add(newExplosion);
            }

            if (Game.Session("Player1").GetController<ControllerXbox360>().A.Pressed)
            {
                Entities.Projectile newProjectile = new Entities.Projectile(Assets.PROJ_TEST, TheTerrain);
                newProjectile.X = PlyTest.X;
                newProjectile.Y = PlyTest.Y;
                newProjectile.Launch(new Vector2(Rand.Float(-500, 500), -850));
                Add(newProjectile);
            }


            if(WaterEnt.GetGraphic<ImageSet>().Frame < WaterEnt.GetGraphic<ImageSet>().Frames)
            {
                if(WaterEnt.GetGraphic<ImageSet>().Frame == WaterEnt.GetGraphic<ImageSet>().Frames - 1)
                {
                    WaterEnt.GetGraphic<ImageSet>().Frame = 0;
                    
                }
                else
                {
                    WaterEnt.GetGraphic<ImageSet>().Frame += 1;
                }
                
            }



            base.Update();
        }

        public override void UpdateFirst()
        {
            // Follow player if there's no projectiles. Otherwise follow one of them.
            if(GetEntities<Entities.Projectile>().Count < 1)
            {
                CamTarget.X = PlyTest.X;
                CamTarget.Y = PlyTest.Y;
            }
            else
            {
                Entities.Projectile theProj = GetEntities<Entities.Projectile>()[0];
                CamTarget.X = theProj.X;
                CamTarget.Y = theProj.Y;
            }



            // Update camera action
            CamFocus.X = Util.Lerp(CamFocus.X, CamTarget.X, 0.1f);
            CamFocus.Y = Util.Lerp(CamFocus.Y, CamTarget.Y, 0.1f);
        }

        public override void Render()
        {
            base.Render();


        }
    }
}
