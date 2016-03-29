using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;

namespace TerrainGenerator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        static void Main()
        {
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/

            int xSize = 2048;
            int ySize = xSize;
            float maxAlt = 512;
            int octaves = 7;
            double frequency = 4;
            double persistance = .45;
            double lacunarity = 1.95;
            double mu = 1.01; // useful range - 1.0 - about 1.01
            double xOffset = 4;
            double yOffset = 9;
            
            string filename = "terrain.raw";
            string bmpFile = "terrain.bmp";
            string texFile = "texture.bmp";
            string tifFile = "terrain.tif";
            string inBmpFile = "input.bmp";
            string erosionMap = "erosionmap.bmp";
            string waterMap = "watermap.bmp";
            string waterRaw = "water.raw";
            Bitmap inBmp = new Bitmap(inBmpFile);
            Bitmap bmp = new Bitmap(xSize, ySize);
            Terrain terrain = new Terrain(xSize, ySize, maxAlt);

            //terrain.generateTerrain(inBmp, 0.4, xOffset, yOffset, frequency, octaves, persistance, lacunarity, mu);
            terrain.generateTerrain(xOffset, yOffset, frequency, octaves, persistance, lacunarity, mu);
            terrain.setTextureSample();
            bmp = terrain.getHeightBitmap();
            terrain.saveHeightRaw("beforeErosion.raw");
            bmp.Save("terrainBeforeErosion.bmp");
            terrain.thermalErosion(45, 30);
            terrain.altHydraulicErosion(.1, 10, .7, 400);
            terrain.saveHeightRaw(filename);
            terrain.saveTIFF(tifFile);
            bmp = terrain.getHeightBitmap();
            bmp.Save(bmpFile);
            bmp = terrain.getTexture();
            bmp.Save(texFile);
            bmp = terrain.getErosionMap();
            bmp.Save(erosionMap);
            bmp = terrain.getWaterMapB();
            bmp.Save(waterMap);
            terrain.saveWaterRaw(waterRaw,5);
        }
    }
}