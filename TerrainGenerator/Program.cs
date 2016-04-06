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

            int xSize = 1024;
            int ySize = xSize;
            float xMapSize = 10000;
            float yMapSize = xMapSize;
            float maxAlt = 5000;
            int octaves = 7;
            double frequency = 3;
            double persistance = .45;
            double lacunarity = 1.95;
            double mu = 1.015; // useful range - 1.0 - about 1.01
            double xOffset = 8.4;
            double yOffset = 9.3;
            
            string filename = "terrain.raw";
            string bmpFile = "terrain.bmp";
            string texFile = "texture.bmp";
            string tifFile = "terrain.tif";
            string inBmpFile = "input.bmp";
            string erosionMap = "erosionmap.bmp";
            string depositionMap = "depositionMap.bmp";
            string waterMap = "watermap.bmp";
            string waterRaw = "water.raw";
            string normalMap = "normalmap.bmp";
            string slopeMap = "slope.bmp";
            string inTif = "input.tif";
            Bitmap inBmp = new Bitmap(inBmpFile);
            Bitmap bmp = new Bitmap(xSize, ySize);
            Terrain terrain = new Terrain(xSize, ySize, xMapSize, yMapSize, maxAlt);

            //terrain.generateTerrain(inBmp, 0, xOffset, yOffset, frequency, octaves, persistance, lacunarity, mu);
            //terrain.generateTerrain(xOffset, yOffset, frequency, octaves, persistance, lacunarity, mu);
            terrain.generateTerrain(inTif, 0.7, xOffset, yOffset, frequency, octaves, persistance, lacunarity, mu);
            terrain.setTextureSample();
            bmp = terrain.getHeightBitmap();
            terrain.saveHeightRaw("beforeErosion.raw");
            bmp.Save("terrainBeforeErosion.bmp");
            terrain.thermalErosion(45, 75);
            terrain.vFieldHydroErosion(.05, .1, .6, .001, .5, .005, 1, 400);
            terrain.thermalErosion(45, 10);
            terrain.vFieldHydroErosion(.05, .1, .6, .001, .5, .005, 1, 200);
            terrain.thermalErosion(45, 10);
            //terrain.waterSystem(1000);
            //terrain.altHydraulicErosion(15, 20, .95, 350);
            terrain.saveHeightRaw(filename);
            terrain.saveTIFF(tifFile);
            bmp = terrain.getNormalMap();
            bmp.Save(normalMap);
            bmp = terrain.getHeightBitmap();
            bmp.Save(bmpFile);
            bmp = terrain.getTexture();
            bmp.Save(texFile);
            bmp = terrain.getErosionMap();
            bmp.Save(erosionMap);
            bmp = terrain.getDepositionMap();
            bmp.Save(depositionMap);
            bmp = terrain.getWaterMap();
            bmp.Save(waterMap);
            terrain.saveWaterRaw(waterRaw,5);
            bmp = terrain.getSlopeMap();
            bmp.Save(slopeMap);
        }
    }
}