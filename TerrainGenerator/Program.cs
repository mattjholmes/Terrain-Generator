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
            int xSize = 2048;
            int ySize = xSize;
            float maxAlt = 256;
            int octaves = 5;
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
            Bitmap bmp = new Bitmap(xSize, ySize);
            Terrain terrain = new Terrain(xSize, ySize, maxAlt);

            terrain.generateTerrain(xOffset, yOffset, frequency, octaves, persistance, lacunarity, mu);
            terrain.setTextureSample();
            terrain.normalizeTerrain();
            bmp = terrain.getHeightBitmap();
            terrain.saveHeightRaw("beforeErosion.raw");
            bmp.Save("terrainBeforeErosion.bmp");
            terrain.thermalErosion(40, 75);
            terrain.saveHeightRaw(filename);
            terrain.saveTIFF(tifFile);
            bmp = terrain.getHeightBitmap();
            bmp.Save(bmpFile);
            bmp = terrain.getTexture();
            bmp.Save(texFile);
                        
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/
        }
    }
}