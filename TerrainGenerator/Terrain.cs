using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrainGenerator
{
    // Terrain class holds the terrain information, and provides methods to generate and modify the terrain
    class Terrain
    {
        // 2d array to hold the height information
        private double[,] terrain;

        // x and y size of terrain map
        private int xSize;
        private int ySize;

        // gradient sample map for generating terrain texture
        Bitmap texSample = new Bitmap(1024, 1024);

        // create a noise generator instance for use in terrain generation
        private NoiseGenerator generator = new NoiseGenerator();

        // Terrain class constructer, requires x and y size - this should be square for unity .raw terrain maps, or 2:1 rect for spherical terrain maps
        public Terrain(int x, int y)
        {
            xSize = x;
            ySize = y;
            terrain = new double[x,y];
        }

        // get height from x, y
        public double getHeight(int x, int y)
        {
            return terrain[x, y];
        }

        // set height at x, y
        public void setHeight(int x, int y, double height)
        {
            terrain[x, y] = height;
        }

        public void setTextureSample()
        {
            Graphics g = Graphics.FromImage(texSample);
            Rectangle targetRect = new Rectangle(new Point(0, 0), texSample.Size);
            
            LinearGradientBrush grBrush = new LinearGradientBrush(targetRect, Color.Black, Color.Black, 0, false);
            ColorBlend cb = new ColorBlend();
            cb.Positions = new[] { 0, 1/3f, 1/2f, 3/4f, 7/8f,  1 };
            cb.Colors = new[] { Color.FromArgb(61, 84, 51), Color.FromArgb(35, 50, 32), Color.FromArgb(35, 50, 32), Color.FromArgb(160, 153, 147), Color.FromArgb(247, 247, 251), Color.FromArgb(247, 247, 251) };
            grBrush.InterpolationColors = cb;
            g.FillRectangle(grBrush, 0, 0, 1024, 1024);
            texSample.Save("texSample.bmp");
        }

        // generate purely pseudorandom terrain - frequency is the initial size of the noise, octaves sets the number of layers of noise,
        // persistance is the amplitude of each successive noise layer, lacunarity is the frequency multiplier per layer
        // mu is the exponential noise distribution decay amount
        public void generateTerrain(double frequency, int octaves, double persistance, double lacunarity, double mu)
        {
            generator.InitExpMagTable(mu);
            double scale = 1.0 / Math.Max(xSize, ySize); 
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    terrain [i, j] = generator.OctaveExpPerlin2d(i * scale, j * scale, frequency, octaves, persistance, lacunarity);
                    
                }
            }
        }

        // Normalized the terrain, making the lowest point = 0 and the highest point = 1
        // This is useful to make the color map generation easier, as well as to use the full range of color resolution in the output heightmap
        public void normalizeTerrain()
        {
            double min = 1;
            double max = 0;
            // loop through the array once and find the min/max values
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    min = Math.Min(min, terrain[x, y]);
                    max = Math.Max(max, terrain[x, y]);
                }
            }
            double scale = 1 / (max - min);
            // loop through the array again and normalize each value
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    terrain[x, y] = (terrain[x, y] - min) * scale;
                }
            }
        }

        // write a file to "filename", in unity compatible .raw heightmap format
        public void saveHeightRaw(string filename)
        {
            if (xSize != ySize)
            {
                throw new IOException(".raw heightmap must be square");
            }
            using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {
                // two bytes for each x, y coordinate when outputing unity .raw heightmaps
                UInt16 output16;

                for (int y = ySize - 1; y >= 0; y--)
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        output16 = (ushort)(terrain[x, y] * 65536);
                        writer.Write(output16);
                    }
                }
            }
        }

        // get a grayscale bitmap representing the terrain heightmap
        public Bitmap getHeightBitmap()
        {
            Bitmap bmp = new Bitmap(xSize, ySize);
            // bmp channel values are 8 bits
            int output8;

            for (int x = 0; x < xSize; x++)
                {
                for (int y = 0; y < ySize; y++)
                {
                    output8 = (int)(terrain[x, y] * 256);
                    if (output8 < 0) output8 = 0;
                    if (output8 > 255) output8 = 255;
                    bmp.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                }
            }
            return bmp;
        }

        // generate color texture bitmap
        public Bitmap getTexture()
        {
            Bitmap output = new Bitmap(xSize, ySize);

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    int altitude = (int)(terrain[x, y] * texSample.Width);
                    if (altitude < 0) altitude = 0;
                    if (altitude >= texSample.Width) altitude = texSample.Width - 1;
                    Color color = texSample.GetPixel(altitude, 0);
                    output.SetPixel(x, y, color);
                }
            }

            return output;
        }
    }
}
