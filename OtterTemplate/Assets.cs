using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//----------------
// Author: J. Brown (DrMelon)
// Part of the [Thaum] Project.
// Date: 09/05/2016
//----------------
// Purpose: Asset management. 

namespace Thaum
{
    class Assets
    {

        public static string ASSET_BASE_PATH = "../../Assets/";

        public static string MUSIC_MENU = ASSET_BASE_PATH + "Music/menu_test.ogg";

        public static string GFX_DEBUGMENU = ASSET_BASE_PATH + "Graphics/debug_menu.png";
        public static string FNT_SYSTEM = ASSET_BASE_PATH + "Graphics/systemfont.png";

        public static string FNT_NOODLE = ASSET_BASE_PATH + "Font/big_noodle_titling_oblique.ttf";

        // Game
        public static string GFX_TERRAIN = ASSET_BASE_PATH + "Graphics/map/terrain1.png";
        public static string GFX_PARTICLE_EXPLODE1 = ASSET_BASE_PATH + "Graphics/explode.png";

    }
}
