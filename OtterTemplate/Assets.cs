using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
        public static string GFX_TERRAIN = ASSET_BASE_PATH + "Graphics/map/terrain2.png";
        public static string GFX_PARTICLE_EXPLODE1 = ASSET_BASE_PATH + "Graphics/explode.png";
        public static string GFX_BALL = ASSET_BASE_PATH + "Graphics/ball.png";
        public static string GFX_WATER = ASSET_BASE_PATH + "Graphics/water.png";
        public static string GFX_PARA1 = ASSET_BASE_PATH + "Graphics/para1.png";
        public static string GFX_PARA2 = ASSET_BASE_PATH + "Graphics/para2.png";
        public static string GFX_PARA3 = ASSET_BASE_PATH + "Graphics/para3.png";
        public static string GFX_PARA4 = ASSET_BASE_PATH + "Graphics/para4.png";
        public static string GFX_PARA5 = ASSET_BASE_PATH + "Graphics/para5.png";
        public static string GFX_SKY = ASSET_BASE_PATH + "Graphics/sky.png";
        public static string GFX_WIZ = ASSET_BASE_PATH + "Graphics/wizard.png";

        // XML File Loading
        public static string PROJECTILES_DEFINE_FOLDER = ASSET_BASE_PATH + "Scripts/Projectiles/";
        public static Dictionary<string, XmlDocument> LoadedProjectiles;

        public static string PROJ_TEST = PROJECTILES_DEFINE_FOLDER + "test_projectile.xml";

    }
}
