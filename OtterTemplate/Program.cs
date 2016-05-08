using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otter;

//----------------
// Author: J. Brown (DrMelon)
// Part of the [OtterTemplate] Project.
// Date: 09/05/2016
//----------------
// Purpose: Main Program Entry Point. Initializes Otter Window.



namespace OtterTemplate
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize Otter2D Framework, set Internal res to 400x240 (16:9 retro res) & window scale to 2x.
            var theGame = new Otter.Game("Otter Template", 400, 240, 60, false);
            theGame.SetWindowScale(2);


            // Initialize Player Sessions
            var playerOneSession = theGame.AddSession("Player1");

            // Import any extant save files
            playerOneSession.Data.Import();

            // Load Save Data or Set Defaults
            playerOneSession.Data.GetIntOrDefault("CurrentPlaythrough", 0);
            playerOneSession.Data.GetIntOrDefault("CurrentPlaythroughZone", 0);
            playerOneSession.Data.GetIntOrDefault("CurrentBestScore", 0);
            playerOneSession.Data.GetStringOrDefault("SaveName", "Player");

            // Export to semi-encrypted file
            playerOneSession.Data.Export();

            // Configure Controller as Xbox Controller
            playerOneSession.Controller = new ControllerXbox360(0);

            // Add keyboard bindings
            playerOneSession.GetController<ControllerXbox360>().Start.AddKey(Key.Return);
            playerOneSession.GetController<ControllerXbox360>().DPad.AddKeys(new Key[] { Key.Up, Key.Right, Key.Down, Key.Left });

            // Using X, D, A, W for A, B, X, Y buttons: roughly corresponds to shape.
            playerOneSession.GetController<ControllerXbox360>().A.AddKey(Key.X);
            playerOneSession.GetController<ControllerXbox360>().B.AddKey(Key.D);
            playerOneSession.GetController<ControllerXbox360>().X.AddKey(Key.A);
            playerOneSession.GetController<ControllerXbox360>().Y.AddKey(Key.W);

            // Shoulder buttons and triggers
            playerOneSession.GetController<ControllerXbox360>().LB.AddKey(Key.Q);
            playerOneSession.GetController<ControllerXbox360>().LT.AddKey(Key.Num1);
            playerOneSession.GetController<ControllerXbox360>().RB.AddKey(Key.E);
            playerOneSession.GetController<ControllerXbox360>().RT.AddKey(Key.Num3);

            // Link Left Stick to D-Pad control.
            playerOneSession.GetController<ControllerXbox360>().DPad.AddAxis(playerOneSession.GetController<ControllerXbox360>().LeftStick);


            // Create menu scene
            theGame.AddScene(new Scenes.MenuScene());

            // Start game
            theGame.Start();

        }
    }
}
