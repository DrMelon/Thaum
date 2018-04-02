using Otter;
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
// Purpose: Turn-based artillery worms-style battle.


namespace Thaum.Scenes
{
    class BattleScene : Scene
    {
        Entities.PixelTerrain TheTerrain;
        public Entities.CameraShaker CamShake;

        Entities.PlayerUnit ActivePlayer;
        List<Entities.PlayerUnit> AllUnits;

        Entity WaterEnt;
        Entity WaterBeneath;

        Entity BG_Parallax1;
        Entity BG_Parallax2;
        Entity BG_Parallax3;
        Entity BG_Parallax4;
        Entity BG_Parallax5;

        Entity Sky;

        float CamSwitchRest;
        Entity CamFocus;
        Entity CamTarget;
        Entity FollowTarget;
        Entity NextFollowTarget;

        public enum SubState
        {
            MATCH_BEGIN,
            PLAYER_TAKING_TURN,
            SWITCHING_TURN,
            CHECK_TEAM_STATUSES,
            MID_TURN_EVENT,
            MATCH_END
        }


        public static float TimeScale = 1.0f;

        public BattleScene()
        {
            TheTerrain = new Entities.PixelTerrain(Assets.GFX_TERRAIN);
            Add(TheTerrain);

            CamShake = new Entities.CameraShaker();
            Add(CamShake);

            AllUnits = new List<Entities.PlayerUnit>();
            ActivePlayer = new Entities.PlayerUnit(200, 50, TheTerrain);
            ActivePlayer.MyTurn = true;
            ActivePlayer.TeamAffiliation = 0;
            Add(ActivePlayer);
            AllUnits.Add(ActivePlayer);
            // Make other units


            // Do BG stuff
            SetupSky();
            SetupParallax();
            SetupWater();
            

            CamFocus = new Entity(ActivePlayer.X, ActivePlayer.Y);
            CamTarget = new Entity(ActivePlayer.X, ActivePlayer.Y);
            

            CameraFocus = CamFocus;
            FollowTarget = ActivePlayer;

            // Set up debug console stuff.
            Otter.Debugger.Instance.RegisterCommands();
        }

        public void SetupSky()
        {
            Sky = new Entity(0, TheTerrain.Graphic.Height + 100, new Image(Assets.GFX_SKY));
            Sky.Graphic.ScaleX = 1080;
            Sky.Graphic.OriginY = Sky.Graphic.Height;
            Sky.Graphic.RepeatX = true;
            Sky.Layer = 100;
            Add(Sky);
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
            WaterBeneath.Layer = 1;
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

        public void SetupParallax()
        {
            // Closest -> Furthest
            BG_Parallax1 = new Entity();
            BG_Parallax1.AddGraphic(new Image(Assets.GFX_PARA1));
            BG_Parallax1.X = 0;
            BG_Parallax1.Y = TheTerrain.Graphic.Height - 32;
            BG_Parallax1.Layer = 10;
            BG_Parallax1.Graphic.OriginY = BG_Parallax1.Graphic.Height - 16;
            BG_Parallax1.Graphic.ScrollX = 0.95f;
            BG_Parallax1.Graphic.ScrollY = 0.975f;
            BG_Parallax1.Graphic.RepeatX = true;
            Add(BG_Parallax1);

            BG_Parallax2 = new Entity();
            BG_Parallax2.AddGraphic(new Image(Assets.GFX_PARA2));
            BG_Parallax2.X = 0;
            BG_Parallax2.Y = TheTerrain.Graphic.Height - 48;
            BG_Parallax2.Layer = 11;
            BG_Parallax2.Graphic.OriginY = BG_Parallax2.Graphic.Height;
            BG_Parallax2.Graphic.ScrollX = 0.8f;
            BG_Parallax2.Graphic.ScrollY = 0.85f;
            BG_Parallax2.Graphic.RepeatX = true;
            Add(BG_Parallax2);

            BG_Parallax3 = new Entity();
            BG_Parallax3.AddGraphic(new Image(Assets.GFX_PARA3));
            BG_Parallax3.X = 0;
            BG_Parallax3.Y = TheTerrain.Graphic.Height - (64.0f*2.0f);
            BG_Parallax3.Layer = 12;
            BG_Parallax3.Graphic.OriginY = BG_Parallax3.Graphic.Height;
            BG_Parallax3.Graphic.ScrollX = 0.55f;
            BG_Parallax3.Graphic.ScrollY = 0.725f;
            BG_Parallax3.Graphic.RepeatX = true;
            Add(BG_Parallax3);

            BG_Parallax4 = new Entity();
            BG_Parallax4.AddGraphic(new Image(Assets.GFX_PARA4));
            BG_Parallax4.X = 0;
            BG_Parallax4.Y = TheTerrain.Graphic.Height - 160.0f;
            BG_Parallax4.Layer = 13;
            BG_Parallax4.Graphic.OriginY = BG_Parallax4.Graphic.Height;
            BG_Parallax4.Graphic.ScrollX = 0.45f;
            BG_Parallax4.Graphic.ScrollY = 0.6f;
            BG_Parallax4.Graphic.RepeatX = true;
            Add(BG_Parallax4);

            BG_Parallax5 = new Entity();
            BG_Parallax5.AddGraphic(new Image(Assets.GFX_PARA5));
            BG_Parallax5.X = 0;
            BG_Parallax5.Y = TheTerrain.Graphic.Height - 280.0f;
            BG_Parallax5.Layer = 14;
            BG_Parallax5.Graphic.OriginY = BG_Parallax5.Graphic.Height;
            BG_Parallax5.Graphic.ScrollX = 0.35f;
            BG_Parallax5.Graphic.ScrollY = 0.405f;
            BG_Parallax5.Graphic.RepeatX = true;
            Add(BG_Parallax5);

        }

        [OtterCommand(helpText: "Reset XML-based objects.", group: "game")]
        public static void ResetXML()
        {
            // Reset all xml defined objs.
            Assets.LoadedProjectiles.Clear();
        }

        [OtterCommand(helpText: "Set deltatime modifier.", group: "game")]
        public static void SetDeltaTimeModifier(float mod = 1.0f)
        {
            TimeScale = mod;
        }
       

        public override void Update()
        {
            



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
            base.UpdateFirst();
            // Follow player if there's nothing moving around the terrain. 
            if(CamSwitchRest > 0)
            {
                CamSwitchRest--;
            }
            if(CamSwitchRest < 1 && NextFollowTarget != null)
            {
                FollowTarget = NextFollowTarget;
                NextFollowTarget = null;
            }
            
            bool somethingMoving = false;
            Entity movingThing = null;

            foreach(Entities.Projectile proj in GetEntities<Entities.Projectile>())
            {
                somethingMoving = true;
                movingThing = proj;
                
                break;
            }

            foreach (Entities.PlayerUnit proj in AllUnits)
            {
                if ((proj.GetComponent<Components.BallisticMovement>().Stable == false && proj.GetComponent<Components.BallisticMovement>().PhysVeloc.Length > 150) || (proj.GetComponent<Components.BallisticMovement>().WalkSpeed.Length > 0))
                {
                    somethingMoving = true;
                    movingThing = proj;
                    
                    break;
                }
            }

            if (somethingMoving == false)
            {
                CamSwitchTarget(ActivePlayer);
            }
            else
            {
                FollowTarget = movingThing;
            }

            if(FollowTarget != null)
            {

                CamTarget.X = FollowTarget.X;
                CamTarget.Y = FollowTarget.Y;
            }
           


            // Update camera action
            CamFocus.X = Util.Lerp(CamFocus.X, CamTarget.X, 0.1f);
            CamFocus.Y = Util.Lerp(CamFocus.Y, CamTarget.Y, 0.1f);
        }

        public void CamSwitchTarget(Entity newTarget)
        {
            if(CamSwitchRest < 1)
            {
                NextFollowTarget = newTarget;
                CamSwitchRest = 30.0f;
            }
           
        }

        public override void Render()
        {
            base.Render();


        }
    }
}
