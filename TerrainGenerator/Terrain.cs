using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;

namespace TerrainGenerator
{
    // Terrain class holds the terrain information, and provides methods to generate and modify the terrain
    class Terrain
    {
        // 2d array to hold the height information
        private double[,] terrain;

        // Maximum altitude of the map, used in slope calculations
        private float maxAltitude;

        // x and y size of terrain map
        private int xSize;
        private int ySize;

        // gradient sample map for generating terrain texture
        Bitmap texSample = new Bitmap(1024, 1024);

        // create a noise generator instance for use in terrain generation
        private NoiseGenerator generator = new NoiseGenerator();

        // Terrain class constructer, requires x and y size - this should be square for unity .raw terrain maps, or 2:1 rect for spherical terrain maps
        // Also takes a maximum altitude, this is in the same units as x, y
        public Terrain(int x, int y, float maxAlt)
        {
            maxAltitude = maxAlt;
            xSize = x;
            ySize = y;
            terrain = new double[x,y];
        }

        // get height from x, y
        public double getHeight(int x, int y)
        {
            return terrain[x, y];
        }

        public float getMaxAlt()
        {
            return maxAltitude;
        }

        // set height at x, y
        public void setHeight(int x, int y, double height)
        {
            terrain[x, y] = height;
        }

        public void setMaxAlt(float maxAlt)
        {
            maxAltitude = maxAlt;
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
        public void generateTerrain(double xOffset, double yOffset, double frequency, int octaves, double persistance, double lacunarity, double mu)
        {
            generator.setXOffset(xOffset);
            generator.setYOffset(yOffset);
            generator.setFrequency(frequency);
            generator.setLacunarity(lacunarity);
            generator.setMu(mu);
            generator.setOctaves(octaves);
            generator.setPersistance(persistance);
            double scale = 1.0 / Math.Max(xSize, ySize); 
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    terrain [i, j] = generator.OctaveExpPerlin2d(i * scale, j * scale);
                    
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

        public void thermalErosion(float talusAngle, int passes)
        {
            // maximum difference between neighboring locations
            double maxDiff = (Math.Tan(talusAngle * (Math.PI / 180))) / maxAltitude;
            double hChange = maxDiff / 2;
            for (int i = 0; i < passes; i++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        // find the minimum height neighbor
                        int minx = 0, miny = 0;
                        double minHeight = 1;
                        for (int nx = -1; nx <= 1; nx++)
                        {
                            for (int ny = -1; ny <= 1; ny++)
                            {
                                // make sure we don't go out of bounds
                                if ( x + nx >= 0 && x + nx < xSize && y + ny >=0 && y + ny < ySize)
                                {
                                    if (terrain[x + nx, y + ny] < minHeight)
                                    {
                                        minHeight = terrain[x + nx, y + ny];
                                        minx = x + nx;
                                        miny = y + ny;
                                    }
                                }
                            } // for neighboring y
                        }// for neighboring x
                        // if any of the neighbors are lower than the max difference, swap the height change amount with the lowest neighbor
                        // special case - if current location is the lowest, skip this step
                        if (terrain[x, y] - minHeight > maxDiff && terrain[x,y] != terrain[minx, miny])
                        {
                            terrain[x, y] -= hChange;
                            terrain[minx, miny] += hChange;
                        }
                    }// for y
                }// for x
            }// for passes

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

        // save 16-bit grayscale TIFF heightmap
        public void saveTIFF(string filename)
        {
            using (Tiff output = Tiff.Open(filename, "w"))
            {
                output.SetField(TiffTag.IMAGEWIDTH, xSize);
                output.SetField(TiffTag.IMAGELENGTH, ySize);
                output.SetField(TiffTag.SAMPLESPERPIXEL, 1); // 1 color channel
                output.SetField(TiffTag.BITSPERSAMPLE, 16); // 16 bits per pixel
                output.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                output.SetField(TiffTag.ROWSPERSTRIP, ySize);
                output.SetField(TiffTag.XRESOLUTION, 88.0);
                output.SetField(TiffTag.YRESOLUTION, 88.0);
                output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);

                for (int y = 0; y < ySize; y++)
                {
                    ushort[] samples = new ushort[xSize];
                    for (int x = 0; x < xSize; x++)
                    {
                        samples[x] = (ushort)(terrain[x, y] * ushort.MaxValue);
                    }
                    byte[] buffer = new byte[samples.Length * sizeof(ushort)];
                    Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                    output.WriteScanline(buffer, y);
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
