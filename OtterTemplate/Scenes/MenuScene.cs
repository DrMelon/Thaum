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
// Purpose: Ultra-basic menu scene. Tool/level select.

namespace Thaum.Scenes
{
    class MenuScene : Scene
    {
        // Storage
        Music MenuMusic;
        Image DebugMenuImage;
        BitmapFont SystemFont;
        RichText MenuTitle;
        RichText MenuPlayDebugLevel;
        RichText MenuSoundTest;
        RichText MenuQuit;

        ControllerXbox360 Player1Controller;

        int CurrentSelection = 0;
        int MaxSelection = 2;

        public override void Begin()
        {
            // Load Music
            MenuMusic = new Music(Assets.MUSIC_MENU, true);
            //MenuMusic.Play();

            // Load BG
            DebugMenuImage = new Image(Assets.GFX_DEBUGMENU);
            DebugMenuImage.Repeat = true;

            // Load font & text
            SystemFont = new BitmapFont(new Texture(Assets.FNT_SYSTEM), 8, 8, 65);

            MenuTitle = new RichText("{waveAmpY:8}{waveRateY:2}DEBUG MENU", SystemFont, 8, 100, 100);
            MenuTitle.MonospaceWidth = 8;
            MenuTitle.SetPosition(200, 100);
            MenuTitle.CenterOrigin();

            MenuPlayDebugLevel = new RichText("{color:FF0}PLAY GAME", SystemFont, 8, 100, 100);
            MenuPlayDebugLevel.MonospaceWidth = 8;
            MenuPlayDebugLevel.SetPosition(100, 120);

            MenuSoundTest = new RichText("SOUND TEST", SystemFont, 8, 100, 100);
            MenuSoundTest.MonospaceWidth = 8;
            MenuSoundTest.SetPosition(100, 130);

            MenuQuit = new RichText("EXIT", SystemFont, 8, 100, 100);
            MenuQuit.MonospaceWidth = 8;
            MenuQuit.SetPosition(100, 140);

            // Fetch controller
            Player1Controller = Game.Session("Player1").GetController<ControllerXbox360>();



            AddGraphic(DebugMenuImage);
            AddGraphic(MenuTitle);
            AddGraphic(MenuPlayDebugLevel);
            AddGraphic(MenuSoundTest);
            AddGraphic(MenuQuit);
        }

        public override void Update()
        {
            base.Update();

            // Scroll BG
            DebugMenuImage.X += 1;
            DebugMenuImage.Y -= 1;


            // Menu Controls

            if(Player1Controller.DPad.Up.Pressed)
            {
                if(CurrentSelection == 0)
                {
                    CurrentSelection = MaxSelection;
                }
                else
                {
                    CurrentSelection--;
                }

                CheckSelection();

            }



            if(Player1Controller.DPad.Down.Pressed)
            {
                if (CurrentSelection == MaxSelection)
                {
                    CurrentSelection = 0;
                }
                else
                {
                    CurrentSelection++;
                }

                CheckSelection();

            }

            if(Player1Controller.Start.Pressed)
            {
                DoSelection();
            }

        }

        public void CheckSelection()
        {
            switch (CurrentSelection)
            {
                case 0:
                    MenuPlayDebugLevel.String = "{color:FF0}PLAY GAME";
                    MenuSoundTest.String = "SOUND TEST";
                    MenuQuit.String = "EXIT";
                    break;
                case 1:
                    MenuPlayDebugLevel.String = "PLAY GAME";
                    MenuSoundTest.String = "{color:FF0}SOUND TEST";
                    MenuQuit.String = "EXIT";
                    break;
                case 2:
                    MenuPlayDebugLevel.String = "PLAY GAME";
                    MenuSoundTest.String = "SOUND TEST";
                    MenuQuit.String = "{color:FF0}EXIT";
                    break;
            }
        }

        public void DoSelection()
        {
            switch (CurrentSelection)
            {
                case 0:
                    // Load Game
                    Game.SwitchScene(new BattleScene());
                    break;
                case 1:
                    // Load Sound Test Screen
                    //
                    break;
                case 2:
                    // Quit
                    Game.Close();
                    break;
            }
        }

    }
}
