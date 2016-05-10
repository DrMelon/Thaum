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

        Entity PlyTest;

        public BattleScene()
        {
            TheTerrain = new Entities.PixelTerrain(Assets.GFX_TERRAIN);
            Add(TheTerrain);

            CamShake = new Entities.CameraShaker();
            Add(CamShake);

            PlyTest = new Entity(200, 50, Image.CreateCircle(8));
            PlyTest.Graphic.CenterOrigin();
            PlyTest.Graphic.Color = Color.Red;
            PlyTest.AddComponent<Components.PlayerMovement>(new Components.PlayerMovement(TheTerrain, 8));
            PlyTest.GetComponent<Components.PlayerMovement>().AllowControl = true;
            Add(PlyTest);
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
                Entities.Projectile newProjectile = new Entities.Projectile(Assets.GFX_BALL, 5.0f, TheTerrain);
                newProjectile.X = PlyTest.X;
                newProjectile.Y = PlyTest.Y;
                newProjectile.Launch(new Vector2(Rand.Float(-500, 500), -850));
                Add(newProjectile);
            }





            base.Update();
        }

        public override void Render()
        {
            base.Render();


        }
    }
}
