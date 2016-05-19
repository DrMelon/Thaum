using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otter;

//----------------
// Author: J. Brown (DrMelon)
// Part of the [Thaum] Project.
// Date: 09/05/2016
//----------------
// Purpose: Classic Worms-Like pixel terrain.
// Should be able to load images as maps.
// Bitmap mode images can be generated, and have textures overlaid during generation.

namespace Thaum.Entities
{
    class PixelTerrain : Entity
    {
        public Texture TerrainTexture;
        public Image TerrainDisplay;
        bool GFXDirty;

        public PixelTerrain(String filename)
        {
            // Load image.
            TerrainTexture = new Texture(filename);
            TerrainDisplay = new Image(filename);
            TerrainDisplay.SetTexture(TerrainTexture);
            AddGraphic(TerrainDisplay);
            GFXDirty = true;
        }

        public override void Update()
        {
            CheckDirty();

            base.Update();
        }

        public void CheckDirty()
        {
            // Update terrain visuals
            if (GFXDirty)
            {
                TerrainTexture.Update();
                

                GFXDirty = false;
            }
        }

        // Make a hole in the terrain!
        public void MakeHole(Vector2 HolePosition, float Radius)
        {
            // Simple radial removal of pixels.
            for(float xPosition = HolePosition.X - Radius; xPosition <= HolePosition.X + Radius; xPosition++)
            {
                for (float yPosition = HolePosition.Y - Radius; yPosition <= HolePosition.Y + Radius; yPosition++)
                {
                    if(Math.Pow(xPosition - HolePosition.X, 2) + Math.Pow(yPosition - HolePosition.Y, 2) < Math.Pow(Radius, 2))
                    {
                        // Sanity check; don't run over texture memory!
                        if(xPosition < 0 || xPosition >= TerrainTexture.Width || yPosition < 0 || yPosition >= TerrainTexture.Height)
                        {
                            continue;
                        }
                        // Remove pixel.
                        TerrainTexture.SetPixel((int)xPosition, (int)yPosition, Color.None);


                        // (Do dynamic physics pixels?)

                        // Set dirty flag
                        GFXDirty = true;
                    }
                }
            }


            
        }

        // Find surface normals.
        public Vector2 GetSurfaceNormal(Vector2 PointOnTerrain, int ScanRadius)
        {
            Vector2 result = new Vector2();

            // Check area around scanpoint, get average displacement.
            for(int x = -ScanRadius; x <= ScanRadius; x++)
            {
                for(int y = -ScanRadius; y <= ScanRadius; y++)
                {
                    if(TerrainTexture.GetPixel(x + (int)PointOnTerrain.X, y + (int)PointOnTerrain.Y).A > 0)
                    {
                        // Modify displacement when solid pixels are encountered.
                        result -= new Vector2(x, y);
                    }
                }
            }

            // Normalize results
            //result.Normalize();



            return result;
        }


        // This generic bresenham-based raycast function is courtesy of 
        // http://www.gamedev.net/page/resources/_/reference/programming/sweet-snippets/line-drawing-algorithm-explained-r1275

        public Vector4 GenericBresenhamRaycast(Vector2 StartPos, Vector2 EndPos)
        {
            // Result
            Vector4 result = new Vector4();

            // Get Direction Vector
            Vector2 DirectionVec = EndPos - StartPos;
            DirectionVec.X = Math.Abs(DirectionVec.X);
            DirectionVec.Y = Math.Abs(DirectionVec.Y);

            // Incremental values, for step direction.
            Vector2 Incremental1 = new Vector2();
            Vector2 Incremental2 = new Vector2();

            if(EndPos.X >= StartPos.X)
            {
                // X is increasing
                Incremental1.X = 1;
                Incremental2.X = 1;
            }
            else
            {
                // Decreasing X
                Incremental1.X = -1;
                Incremental2.X = -1;
            }
            if(EndPos.Y >= StartPos.Y)
            {
                // Y is increasing
                Incremental1.Y = 1;
                Incremental2.Y = 1;
            }
            else
            {
                // Decreasing Y
                Incremental1.Y = -1;
                Incremental2.Y = -1;
            }

            // Take into account slope directions
            float Denominator, Numerator, NumToAdd, NumPixels;

            // This means there is at least one X for every Y
            if(DirectionVec.X >= DirectionVec.Y)
            {
                // Nullify quadrant
                Incremental1.X = 0;
                Incremental2.Y = 0;

                Denominator = DirectionVec.X;
                Numerator = DirectionVec.X / 2.0f;
                NumToAdd = DirectionVec.Y;
                NumPixels = DirectionVec.X;
            }
            else // Vice-versa
            {
                // Nullify quadrant
                Incremental2.X = 0;
                Incremental1.Y = 0;

                Denominator = DirectionVec.Y;
                Numerator = DirectionVec.Y / 2.0f;
                NumToAdd = DirectionVec.X;
                NumPixels = DirectionVec.Y;
            }

            // Set previous position to start position
            Vector2 PrevPos = StartPos;
            int CheckX = (int)StartPos.X;
            int CheckY = (int)StartPos.Y;


            // Now iterating over the line
            for(int CurrentPixel = 0; CurrentPixel <= NumPixels; CurrentPixel++)
            {
                if(TerrainTexture.GetPixel(CheckX, CheckY).A > 0)
                {
                    result.X = PrevPos.X;
                    result.Y = PrevPos.Y;
                    result.Z = CheckX;
                    result.W = CheckY;
                    return result;
                }

                PrevPos.X = CheckX;
                PrevPos.Y = CheckY;

                // Increase numerator
                Numerator += NumToAdd;

                // If we've exceeded the denominator, we can shift pixels by the incrementals.
                if(Numerator >= Denominator)
                {
                    Numerator -= Denominator;
                    CheckX += (int)Incremental1.X;
                    CheckY += (int)Incremental1.Y;
                }

                CheckX += (int)Incremental2.X;
                CheckY += (int)Incremental2.Y;

            }



            return new Vector4(-1,-1,-1,-1);
        }

    }
}
