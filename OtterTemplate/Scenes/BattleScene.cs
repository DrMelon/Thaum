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

        public BattleScene()
        {
            TheTerrain = new Entities.PixelTerrain(Assets.GFX_TERRAIN);
            Add(TheTerrain);
        }

        public override void Update()
        {

            if (Game.Session("Player1").GetController<ControllerXbox360>().B.Pressed)
            {
                Entities.Explosion newExplosion = new Entities.Explosion(TheTerrain, new Vector2(130, 125), 20);
                Add(newExplosion);
            }



            base.Update();
        }

        public override void Render()
        {
            base.Render();

        }
    }
}
