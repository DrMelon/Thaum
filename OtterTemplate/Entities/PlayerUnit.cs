using System;
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

        public int Health;
        public int HealthDelta;

        public PlayerUnit(float x, float y, PixelTerrain terrain)
        {
            // Set up defaults
            X = x;
            Y = y;
            Health = 100;
            PlayerMovement myMovement = new PlayerMovement(terrain, 8);
            myMovement.AllowControl = true;
            AddComponent(myMovement);

            // Add GFX
            AddGraphic(Image.CreateCircle(8, Color.Magenta));
            Graphic.CenterOrigin();
        }

        public void TakeHealth(int HP)
        {
            HealthDelta -= HP;
        }
    }
}
