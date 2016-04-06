using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
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

            int xSize = 512;
            int ySize = xSize;
            float xMapSize = 10000;
            float yMapSize = xMapSize;
            float maxAlt = 2500;
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
            ColorBlend cb = new ColorBlend();
            cb.Positions = new[] { 0, 1 / 3f, 1 / 2f, 3 / 4f, 7 / 8f, 1 };
            cb.Colors = new[] { Color.FromArgb(61, 84, 51), Color.FromArgb(35, 50, 32), Color.FromArgb(35, 50, 32), Color.FromArgb(160, 153, 147), Color.FromArgb(247, 247, 251), Color.FromArgb(247, 247, 251) };

            //terrain.terrainFromBmp(inBmp);
            //terrain.terrainFromTIFF(inTif);
            //terrain.addTerrainNoise(0.5, xOffset, yOffset, frequency, octaves, persistance, lacunarity, mu);
            terrain.generateTerrain(xOffset, yOffset, frequency, octaves, persistance, lacunarity, mu);
            terrain.setTextureSample(cb);
            bmp = terrain.getHeightBitmap();
            terrain.saveHeightRaw("beforeErosion.raw");
            bmp.Save("terrainBeforeErosion.bmp");
            terrain.thermalErosion(45, 25);
            terrain.vFieldHydroErosion(.1, .5, .01, .5, .1, .025, 1, 400);
            terrain.thermalErosion(45, 5);
            terrain.vFieldHydroErosion(.1, .5, .01, .5, .1, .025, 1, 200);
            terrain.thermalErosion(45, 5);
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
            bmp = terrain.getThermalErosionMap();
            bmp.Save("thermalErosion.bmp");
            bmp = terrain.getTalusMap();
            bmp.Save("talus.bmp");
            bmp = terrain.getWaterMap();
            bmp.Save(waterMap);
            terrain.saveWaterRaw(waterRaw,5);
            bmp = terrain.getSlopeMap();
            bmp.Save(slopeMap);
            bmp = terrain.getSplatMap(1000, 5000, 500, 0, 0, 40, 0, 15, .1);
            bmp.Save("snow.bmp");
            bmp = terrain.getSplatMap(0, 1500, 0, 1000, 0, 45, 0, 20, .3);
            bmp.Save("trees.bmp");
            bmp = terrain.getSplatMap(0, 1500, 0, 1000, 0, 65, 0, 20, .4);
            bmp.Save("grass.bmp");
        }
    }
}