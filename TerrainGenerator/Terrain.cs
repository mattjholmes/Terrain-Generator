using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;
using static TerrainGenerator.MathUtility;
using System.Windows.Media.Media3D;
using System.Windows;

namespace TerrainGenerator
{
    // Terrain class holds the terrain information, and provides methods to generate and modify the terrain
    class Terrain
    {
        // Maximum altitude of the map, used in slope calculations
        public float maxAltitude { get; set; }

        // X and Y size of the overall map in meters
        public float xActualSize { get; set; }
        public float yActualSize { get; set; }

        // x and y size of terrain map array
        public int xSize { get; private set; }
        public int ySize { get; private set; }
        
        // 2d array to hold the height information
        private double[,] terrain;

        // 2d array of Vector3d to hold normal information
        private Vector3D[,] normalMap;

        // custom data type to hold water flow information
        // each direction holds a water flow amount out of the current square
        private class OutflowFlux
        {
            public double left { get; set; }
            public double right { get; set; }
            public double up { get; set; }
            public double down { get; set; }

            public OutflowFlux()
            {
                left = 0;
                right = 0;
                up = 0;
                down = 0;
            }
        }

        // 2d arrays to hold water and sediment information
        private double[,] waterMap;
        private double[,] sediment;
        private OutflowFlux[,] oFlux;
        private Vector[,] waterVel;

        // 2d array to hold erosion map - used in generating texture control maps 
        private double[,] erosion;
        private double[,] deposition;
        private double[,] tErosion;
        private double[,] talus;

        // variables for water calculations
        private double grav;
        private double pipeArea;
        private double pipeLength;

        // gradient sample map for generating terrain texture
        private Bitmap texSample;

        // create a noise generator instance for use in terrain generation
        private NoiseGenerator generator = new NoiseGenerator();

        // create a random number generator, used in rainfall chance calculation
        Random rand;

        // Terrain class constructer, requires x and y size - this should be square for unity .raw terrain maps, or 2:1 rect for spherical terrain maps
        // xMapSize and yMapSize are the overall map size in meters, this is independent of the underlying bitmap whose size is determined by the x and y parameters
        // Also takes a maximum altitude, in meters
        public Terrain(int x, int y, float xMapSize, float yMapSize, float maxAlt)
        {
            maxAltitude = maxAlt;
            xSize = x;
            ySize = y;
            xActualSize = xMapSize;
            yActualSize = yMapSize;
            terrain = new double[x, y];
            normalMap = new Vector3D[x, y];
            sediment = new double[x, y];
            erosion = new double[x, y];
            deposition = new double[x, y];
            waterMap = new double[x, y];
            waterVel = new Vector[x, y];
            oFlux = new OutflowFlux[x, y];
            tErosion = new double[x, y];
            talus = new double[x, y];
            rand = new Random();
            texSample = new Bitmap(1024, 1024);

            // initialize the components of the water velocity and flux arrays
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    waterVel[i, j] = new Vector(0, 0);
                    oFlux[i, j] = new OutflowFlux();
                }
            }
        }

        // get height from x, y
        public double getHeight(int x, int y)
        {
            return terrain[x, y] * maxAltitude;
        }
        
        public void setResolution(int x, int y)
        {
            xSize = x;
            ySize = y;
            terrain = new double[x, y];
            normalMap = new Vector3D[x, y];
            sediment = new double[x, y];
            erosion = new double[x, y];
            deposition = new double[x, y];
            waterMap = new double[x, y];
            waterVel = new Vector[x, y];
            oFlux = new OutflowFlux[x, y];

            // initialize the components of the water velocity and flux arrays
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    waterVel[i, j] = new Vector(0, 0);
                    oFlux[i, j] = new OutflowFlux();
                }
            }
        }

        // takes a color blend object defining a multi color gradient and creates a sample map for use in altitude based coloring
        // texture sample is square, to allow future addition of altitude + latitude color mapping
        public void setTextureSample(ColorBlend cb)
        {
            // set up the sample to be drawn to
            Graphics g = Graphics.FromImage(texSample);
            Rectangle targetRect = new Rectangle(new System.Drawing.Point(0, 0), texSample.Size);
            LinearGradientBrush grBrush = new LinearGradientBrush(targetRect, Color.Black, Color.Black, 0, false);
            
            // set the gradient to the incoming ColorBlend, and fill the textureSample with it
            grBrush.InterpolationColors = cb;
            g.FillRectangle(grBrush, 0, 0, 1024, 1024);

            //texSample.Save("texSample.bmp"); // debug output
        }

        // generate pseudorandom terrain - frequency is the initial size of the noise, octaves sets the number of layers of noise,
        // persistance is the amplitude of each successive noise layer, lacunarity is the frequency multiplier per layer
        // mu is the exponential noise distribution decay amount
        public void generateTerrain(double xOffset, double yOffset, double frequency, int octaves, double persistence, double lacunarity, double mu)
        {
            generator.setXOffset(xOffset);
            generator.setYOffset(yOffset);
            // divide the frequency by 10 km - this results in macro features ~ 10 km across at a frequency input of 1
            generator.setFrequency(frequency / 10000);
            generator.setLacunarity(lacunarity);
            generator.setMu(mu);
            generator.setOctaves(octaves);
            generator.setPersistance(persistence);
            double xScale = xActualSize / xSize;
            double yScale = yActualSize / ySize;
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    terrain [i, j] = generator.OctaveExpPerlin2d(i * xScale, j * yScale);
                    
                }
            }

            normalizeTerrain();
            calculateNormals();
        }

        // adds noise to an existing terrain - intended to be used with input maps
        public void addTerrainNoise(double weight, double xOffset, double yOffset, double frequency, int octaves, double persistence, double lacunarity, double mu)
        {
            // weight input is expected to be a percentage, IE 0.0...1.0
            if (weight > 1.0 || weight < 0.0)
            {
                throw new ArgumentOutOfRangeException("Weight must be less than 1.0, greater than or equal to 0.0");
            }

            generator.setXOffset(xOffset);
            generator.setYOffset(yOffset);
            generator.setFrequency(frequency / 10000);
            generator.setLacunarity(lacunarity);
            generator.setMu(mu);
            generator.setOctaves(octaves);
            generator.setPersistance(persistence);

            double xScale = xActualSize / xSize;
            double yScale = yActualSize / ySize;

            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    double baseHeight = 0;
                    baseHeight = terrain[i, j];
                    terrain[i, j] = (generator.OctaveExpPerlin2d(i * xScale, j * yScale) * weight) + (baseHeight * (1 - weight));
                }
            }
            normalizeTerrain();
            calculateNormals();
        }

        // generates a new terrain from a bitmap input
        public void terrainFromBmp(Bitmap input)
        {
            Random rand = new Random();
            xSize = input.Width;
            ySize = input.Height;
            terrain = new double[xSize, ySize];
            normalMap = new Vector3D[xSize, ySize];
            sediment = new double[xSize, ySize];
            erosion = new double[xSize, ySize];
            deposition = new double[xSize, ySize];
            waterMap = new double[xSize, ySize];
            waterVel = new Vector[xSize, ySize];
            oFlux = new OutflowFlux[xSize, ySize];
            tErosion = new double[xSize, ySize];
            talus = new double[xSize, ySize];

            // initialize the components of the water velocity and flux arrays
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    waterVel[i, j] = new Vector(0, 0);
                    oFlux[i, j] = new OutflowFlux();
                }
            }

            double[,] inTerrain = new double[input.Width,input.Height];
            
            // read the input image into an array for modification, add a little noise, this will help with smoothing the limited input height resolution (255 levels for an 8-bit grayscale bmp)
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y ++)
                {
                    inTerrain[x,y] = input.GetPixel(x, y).GetBrightness();
                    inTerrain[x,y] += rand.NextDouble() * (1 / maxAltitude) * 2;
                }
            }

            // Smooth the input terrain using a slightly modified Laplacian smoothing algorithm
            for (int x = 2; x < input.Width - 2; x++)
            {
                for (int y = 2; y < input.Height - 2; y++)
                {
                    double total = 0;
                    int count = 0;
                    for (int i = -2; i <= 2; i++)
                    {
                        for (int j = -2; j <= 2; j++)
                        {
                            total += inTerrain[x + i, y + j];
                            count++;
                        }
                    }
                    inTerrain[x, y] = total/count;
                }
            }

            terrain = inTerrain;
            normalizeTerrain();
            calculateNormals();
        }

        // generate terrain from a 16-bit grayscale TIFF
        public void terrainFromTIFF(String path)
        {
            double[,] inTerrain;
            int height, width;

            using (Tiff tif = Tiff.Open(path, "r"))
            {
                FieldValue[] res = tif.GetField(TiffTag.IMAGELENGTH);
                height = res[0].ToInt();

                res = tif.GetField(TiffTag.IMAGEWIDTH);
                width = res[0].ToInt();

                inTerrain = new double[width, height];

                res = tif.GetField(TiffTag.BITSPERSAMPLE);
                short bpp = res[0].ToShort();

                res = tif.GetField(TiffTag.SAMPLESPERPIXEL);
                short spp = res[0].ToShort();

                if (bpp != 16 || spp != 1)
                {
                    throw new IOException("This is not a 16-bit grayscale tiff");
                }

                res = tif.GetField(TiffTag.PHOTOMETRIC);
                Photometric photo = (Photometric)res[0].ToInt();

                int stride = tif.ScanlineSize();
                byte[] scanline = new byte[stride];
                ushort[] scanline16bit = new ushort[stride / 2];

                for (int i = 0; i < height; i++)
                {
                    tif.ReadScanline(scanline, i);
                    Buffer.BlockCopy(scanline, 0, scanline16bit, 0, scanline.Length);
                    for (int j = 0; j < width; j++)
                    {
                        inTerrain[j, i] = (double)scanline16bit[j] / ushort.MaxValue;
                    }
                }
            }

            xSize = width;
            ySize = height;
            terrain = new double[xSize, ySize];
            normalMap = new Vector3D[xSize, ySize];
            sediment = new double[xSize, ySize];
            erosion = new double[xSize, ySize];
            deposition = new double[xSize, ySize];
            waterMap = new double[xSize, ySize];
            waterVel = new Vector[xSize, ySize];
            oFlux = new OutflowFlux[xSize, ySize];
            tErosion = new double[xSize, ySize];
            talus = new double[xSize, ySize];

            // initialize the components of the water velocity and flux arrays
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    waterVel[i, j] = new Vector(0, 0);
                    oFlux[i, j] = new OutflowFlux();
                }
            }

            terrain = inTerrain;
            normalizeTerrain();
            calculateNormals();
        }

        // Normalized the terrain, making the lowest point = 0 and the highest point = 1
        // This is useful to make the color map generation easier, as well as to use the full range of bit depth resolution in the output heightmap
        private void normalizeTerrain()
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

        // normalize the erosion map, this makes the output much more readable
        private void normalizeErosion()
        {
            double min = 1;
            double max = 0;
            // loop through the array once and find the min/max values
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    min = Math.Min(min, erosion[x, y]);
                    max = Math.Max(max, erosion[x, y]);
                }
            }
            double scale = 1 / (max - min);
            // loop through the array again and normalize each value
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    erosion[x, y] = (erosion[x, y] - min) * scale;
                }
            }
        }

        // normalize the deposition map, this makes the output much more readable
        private void normalizeDeposition()
        {
            double min = 1;
            double max = 0;
            // loop through the array once and find the min/max values
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    min = Math.Min(min, deposition[x, y]);
                    max = Math.Max(max, deposition[x, y]);
                }
            }
            double scale = 1 / (max - min);
            // loop through the array again and normalize each value
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    deposition[x, y] = (deposition[x, y] - min) * scale;
                }
            }
        }

        // normalize the deposition map, this makes the output much more readable
        private void normalizeThermalErosion()
        {
            double min = 1;
            double max = 0;
            // loop through the array once and find the min/max values
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    min = Math.Min(min, tErosion[x, y]);
                    max = Math.Max(max, tErosion[x, y]);
                }
            }
            double scale = 1 / (max - min);
            // loop through the array again and normalize each value
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    tErosion[x, y] = (tErosion[x, y] - min) * scale;
                }
            }
        }

        // normalize the talus map, this makes the output much more readable
        private void normalizeTalus()
        {
            double min = 1;
            double max = 0;
            // loop through the array once and find the min/max values
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    min = Math.Min(min, talus[x, y]);
                    max = Math.Max(max, talus[x, y]);
                }
            }
            double scale = 1 / (max - min);
            // loop through the array again and normalize each value
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    talus[x, y] = (talus[x, y] - min) * scale;
                }
            }
        }

        // Thermal erosion is the process of material breaking apart due to thermal expansion and contraction
        // and falling down a slope if it is too steep
        // input parameters are the maximum stable slope angle, and the number of passes to execute
        public void thermalErosion(float talusAngle)
        {
            // maximum difference between neighboring locations
            double maxDiff = ((xActualSize / xSize) * Math.Tan(talusAngle * (Math.PI / 180))) / maxAltitude;
            // amount to move to the neighbors - higher values will lead to stairstepping in the output height map, but require less passes to move the same amount of material
            double hChange = maxDiff / 16;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    // find the neighbors below the maxDiff
                    bool[,] lowNeighbors = new bool[3, 3];
                    int numNeighbors = 0;
                    for (int nx = -1; nx <= 1; nx++)
                    {
                        for (int ny = -1; ny <= 1; ny++)
                        {
                            // make sure we don't go out of bounds
                            if (x + nx >= 0 && x + nx < xSize && y + ny >= 0 && y + ny < ySize)
                            {
                                // if we are looking at the current cell, mark the lowNeighbor point as false
                                if (nx == 0 && ny == 0)
                                {
                                    lowNeighbors[nx + 1, ny + 1] = false;
                                }
                                // if the difference between the current square and the current neighbor is greater than maxDiff, we will move material there later
                                else if (terrain[x, y] - terrain[x + nx, y + ny] > maxDiff)
                                {
                                    lowNeighbors[nx + 1, ny + 1] = true;
                                    numNeighbors++;
                                }
                                // otherwise, we will ignore that neighbor
                                else
                                {
                                    lowNeighbors[nx + 1, ny + 1] = false;
                                }
                            }
                        } // for neighboring y
                    }// for neighboring x
                    double amountToMove = hChange / numNeighbors;

                    // if any of the neighbors are lower than the max difference, swap the height change amount with the lowest neighbor
                    for (int nx = -1; nx <= 1; nx++)
                    {
                        for (int ny = -1; ny <= 1; ny++)
                        {
                            // make sure we don't go out of bounds
                            if (x + nx >= 0 && x + nx < xSize && y + ny >= 0 && y + ny < ySize)
                            {
                                // if we previously marked one of these squares as lower than maxDiff, we will now move material to it
                                // also add the amount removed to the erosion map, and the amount added to neighbors to the deposition map
                                if (lowNeighbors[nx + 1, ny + 1])
                                {
                                    terrain[x, y] -= amountToMove;
                                    terrain[x + nx, y + ny] += amountToMove;
                                    tErosion[x, y] += amountToMove;
                                    talus[x + nx, y + ny] += amountToMove;
                                }
                            }
                        }
                    }
                }// for y
            }// for x
            calculateNormals();
        }

        // version of thermal erosion that does the looping for you
        public void thermalErosion(float talusAngle, int passes)
        {
            for (int i = 0; i < passes; i++)
            {
                thermalErosion(talusAngle);
            }
        }

        // Hydraulic Erosion based on velocity field, derived from the normal map
        // input parameters, solubility: 0..1 represents the solubility of the soil as a percentage of carrying water, IE 1 solubility means 1 meter of water can dissolve 1 meter of soil in 1 timestep
        // water capacity: 0..1 represents the maximum amount of sediment the water can carry, this drops off as water velocity drops. 1 means 1 meter of water can carry 1 meter of soil
        // rainChance: 0..1 the chance per square that a "rain drop" will fall, rainAmount: the max depth in meters of the "rain drop"
        // evapConstant: 0..1 the percentage of water that will evaporate after each time step
        // timeStep: time to calculate for each step, in seconds, steps: total number of steps to calculate
        public void vFieldHydroErosion(double sol, double depRate, double wCap, double rainChance, double rainAmt, double evapConstant, double timeStep, int steps)
        {
            double solubility = sol;
            double depositRate = depRate;
            double waterCapacity = wCap;
            double rainAmount = rainAmt / maxAltitude;
            double scale = xActualSize / xSize;
            
            double[,] waterNew = new double[xSize, ySize];
            double[,] sedimentNew = new double[xSize, ySize];

            /*Bitmap waterBmp = getWaterMap();
            Bitmap heightMap = getHeightBitmap();
            Bitmap sedimentMap = getSedimentmap();
            var form = new Form1();*/

            grav = 9.8;
            pipeLength = Math.Min(xActualSize / xSize, yActualSize / ySize);
            pipeArea = Math.PI * Math.Pow(pipeLength / 2, 2);
            double k;

            /*form.Show();
            form.pictureBox1.Image = waterBmp;
            form.pictureBox2.Image = heightMap;
            form.pictureBox3.Image = sedimentMap;
            form.Update();*/

            // iterate through all 5 steps for the number of steps specified
            for (int n = 0; n < steps; n++)
            {
                // first step - add new water to waterMap based on rain chance, rain amount, rain size
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        if (rand.NextDouble() < rainChance)
                        {
                            waterMap[x, y] += rainAmount * timeStep;
                        }
                    }
                }
                waterMap[xSize / 2, ySize / 2] += (1/maxAltitude) * timeStep;

                // second step - simulate flow, update waterMap and waterVel

                // update the outflowFlux map
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        // calculate the flow in each direction, edge cases = 0
                        if (x - 1 >= 0)
                        {
                            oFlux[x, y].left = Math.Max(0, (oFlux[x, y].left + timeStep * pipeArea * ((grav  * maxAltitude * ((terrain[x, y] + waterMap[x, y]) - (terrain[x - 1, y] + waterMap[x - 1, y]))) / pipeLength))/ maxAltitude);
                        }
                        else
                        {
                            oFlux[x, y].left = 0;
                        }

                        if (x + 1 < xSize)
                        {
                            oFlux[x, y].right = Math.Max(0, (oFlux[x, y].right + timeStep * pipeArea * ((grav  * maxAltitude * ((terrain[x, y] + waterMap[x,y]) - (terrain[x + 1, y] + waterMap[x + 1, y]))) / pipeLength)) / maxAltitude);
                        }
                        else
                        {
                            oFlux[x, y].right = 0;
                        }

                        if (y - 1 >= 0)
                        {
                            oFlux[x, y].up = Math.Max(0, (oFlux[x, y].up + timeStep * pipeArea * ((grav  * maxAltitude * ((terrain[x, y] + waterMap[x,y]) - (terrain[x, y - 1] + waterMap[x, y - 1]))) / pipeLength)) / maxAltitude);
                        }
                        else
                        {
                            oFlux[x, y].up = 0;
                        }

                        if (y + 1 < ySize)
                        {
                            oFlux[x, y].down = Math.Max(0, (oFlux[x, y].down + timeStep * pipeArea * ((grav  * maxAltitude * ((terrain[x, y] + waterMap[x, y]) - (terrain[x, y + 1] + waterMap[x, y + 1]))) / pipeLength)) / maxAltitude);
                        }
                        else
                        {
                            oFlux[x, y].down = 0;
                        }

                        // k scales the flow down if the depth of the water in the square is less than the total outflow
                        k = Math.Min(1, (waterMap[x, y] * pipeLength * pipeLength) / ((oFlux[x, y].left + oFlux[x, y].right + oFlux[x, y].up + oFlux[x, y].down) * timeStep));
                        // make sure K hasn't become NaN due to divide by zero
                        if (double.IsNaN(k))
                            k = 0;
                        oFlux[x, y].left *= k;
                        oFlux[x, y].right *= k;
                        oFlux[x, y].up *= k;
                        oFlux[x, y].down *= k;
                    }
                }

                // actually move the water based on the outflow we just calculated, and update the velocity field
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        double sumIn = 0;
                        double sumOut = 0;
                        double deltaWater = 0;
                        double waterVelX = 0;
                        double waterVelY = 0;
                        double waterBefore, waterAfter, averageWater;
                        double u, v;

                        // calculate how much water to move to/from this square based on the flux map
                        if (x - 1 >= 0)
                            sumIn += oFlux[x - 1, y].right;
                        if (x + 1 <= xSize - 1)
                            sumIn += oFlux[x + 1, y].left;
                        if (y - 1 >= 0)
                            sumIn += oFlux[x, y - 1].down;
                        if (y + 1 <= ySize - 1)
                            sumIn += oFlux[x, y + 1].up;

                        sumOut = oFlux[x, y].left + oFlux[x, y].right + oFlux[x, y].up + oFlux[x, y].down;

                        deltaWater = timeStep * (sumIn - sumOut);

                        // move the water in or out of the square
                        waterBefore = waterMap[x, y];
                        waterNew[x, y] = waterMap[x, y] + (deltaWater / (pipeLength * pipeLength));
                        if (waterNew[x, y] < 0)
                            waterNew[x, y] = 0;
                        waterAfter = waterNew[x, y];
                        
                        // calculate the average water velocity in the x and y directions
                        if (x - 1 >= 0)
                            waterVelX += oFlux[x - 1, y].right - oFlux[x, y].left;
                        else
                            waterVelX -= oFlux[x, y].left;
                        if (x + 1 <= xSize - 1)
                            waterVelX += oFlux[x, y].right - oFlux[x + 1, y].left;
                        else
                            waterVelX += oFlux[x, y].right;
                        if (y - 1 >= 0)
                            waterVelY += oFlux[x, y - 1].down - oFlux[x, y].up;
                        else
                            waterVelY -= oFlux[x,y].up;
                        if (y + 1 <= ySize - 1)
                            waterVelY += oFlux[x, y].down - oFlux[x, y + 1].up;
                        else
                            waterVelY += oFlux[x, y].down;
                        waterVelX /= 2;
                        waterVelY /= 2;

                        averageWater = (waterBefore + waterAfter) / 2;

                        // create the u, v vector directions for updating the water vector field - careful to look out for NaN fp errors
                        u = waterVelX / (pipeLength * averageWater);
                        if (double.IsNaN(u))
                            u = 0;
                        v = waterVelY / (pipeLength * averageWater);
                        if (double.IsNaN(v))
                            v = 0;

                        waterVel[x, y].X = u;
                        waterVel[x, y].Y = v;
                    }
                }
                waterMap = waterNew;

                // third step - calculate erosion and deposition
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        // calculate the slope in radians from the normal map
                        double slope = Math.Asin(new Vector(normalMap[x, y].X, normalMap[x, y].Y).Length);
                        // calculate the sediment capacity of this cell
                        double sedCap = waterMap[x,y] * timeStep * waterCapacity * Math.Sin(slope) * Math.Abs(waterVel[x, y].Length);
                        //double sedCap = waterCapacity * waterVel[x, y].Length * waterVel[x, y].Length;

                        if (sedCap > sediment[x,y] && terrain[x,y] - solubility * (sedCap - sediment[x,y]) > 0)
                        {
                            terrain[x, y] -= solubility * (sedCap - sediment[x, y]);
                            sediment[x, y] += solubility * (sedCap - sediment[x, y]);
                            erosion[x, y] += solubility * (sedCap - sediment[x, y]);
                        }
                        else if (sedCap <= sediment[x,y])
                        {
                            terrain[x, y] += depositRate * (sediment[x, y] - sedCap);
                            sediment[x, y] -= depositRate * (sediment[x, y] - sedCap);
                            deposition[x, y] += depositRate * (sediment[x, y] - sedCap);
                        }
                    }
                }
                // fourth step - transport suspended sediment - calculate from waterVel
                
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        
                        int xSource, ySource;
                        xSource = x - (int)(waterVel[x, y].X * scale * timeStep);
                        ySource = y - (int)(waterVel[x, y].Y * scale * timeStep);
                        if (xSource >= 0 && xSource < xSize && ySource >= 0 && ySource < ySize)
                        {
                            sedimentNew[x, y] = sediment[xSource, ySource];
                            if (double.IsNaN(sedimentNew[x, y]))
                                sedimentNew[x, y] = 0;
                        }
                        else if (x - 1 >= 0 && x + 1 < xSize && y - 1 >= 0 && y + 1 < ySize )
                        {
                            sedimentNew[x, y] = (sediment[x + 1, y] + sediment[x - 1, y] + sediment[x, y + 1] + sediment[x, y - 1]) / 4;
                        }
                    }
                }
                sediment = sedimentNew;

                // last step - remove water from waterMap via evaporation
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        waterMap[x, y] *= 1 - evapConstant * timeStep;
                        if (double.IsNaN(waterMap[x, y]))
                            waterMap[x, y] = 0;
                    }
                }

                /*waterBmp = getWaterMap();
                form.textBox1.Text = n.ToString();
                form.pictureBox1.Image = waterBmp;
                heightMap = getHeightBitmap();
                form.pictureBox2.Image = heightMap;
                sedimentMap = getSedimentmap();
                form.pictureBox3.Image = sedimentMap;
                form.Update();*/

                // we need to update the normals on each pass, we depend on them for some of the erosion equations
                calculateNormals();
            }
        }

        //execute a single pass of hydraulic erosion
        public void vFieldHydroErosion(double sol, double depRate, double wCap, double rainChance, double rainAmt, double evapConstant, double timeStep)
        {
            vFieldHydroErosion(sol, depRate, wCap, rainChance, rainAmt, evapConstant, timeStep, 1);
        }


        public void calculateNormals()
        {
            // copy the real terrain into a working version, give it a border that we will dump later
            // this simplifies the normal calculation algorithm
            double[,] workTerrain = new double[xSize + 2, ySize + 2];
            // copy the top and bottom edges
            for (int x = 1; x < xSize + 1; x++)
            {
                workTerrain[x, 0] = terrain[x - 1, 0];
                workTerrain[x, ySize + 1] = terrain[x - 1, ySize-1];
            }
            // copy the left and right edges
            for (int y = 1; y < ySize + 1; y++)
            {
                workTerrain[0, y] = terrain[0, y - 1];
                workTerrain[xSize + 1, y] = terrain[xSize - 1, y - 1];
            }
            // copy the real terrain into the middle of the working terrain
            for (int x = 1; x < xSize + 1; x++)
            {
                for (int y = 1; y < ySize + 1; y++)
                {
                    workTerrain[x, y] = terrain[x - 1, y - 1];
                }
            }
            // handle the corners
            workTerrain[0, 0] = workTerrain[1,1];
            workTerrain[0, ySize + 1] = workTerrain[1, ySize];
            workTerrain[xSize + 1, 0] = workTerrain[xSize, 1];
            workTerrain[xSize + 1, ySize + 1] = workTerrain[xSize, ySize];

            // calculate the normals for each real terrain location
            for (int x = 1; x < xSize + 1; x++)
            {
                for (int y = 1; y < ySize + 1; y++)
                {
                    int i = 0;
                    double[] sample = new double[9];
                    // sample the 9 square grid centered on the working point
                    for (int nx = -1; nx <= 1; nx++)
                    {
                        for (int ny = -1; ny <= 1; ny++)
                        {
                            sample[i] = workTerrain[x + nx, y + ny];
                            i++;
                        }
                    }
                    double scale = 32 * maxAltitude / Math.Min(xActualSize, yActualSize);
                    Vector3D norm = new Vector3D();
                    // calculate the normal using a sobel filter - scale determined by ratio of height/mapsize
                    norm.X = -(sample[6] - sample[0] + 2 * (sample[7] - sample[1]) + sample[8] - sample[2]);
                    norm.Y = (sample[2] - sample[0] + 2 * (sample[5] - sample[3]) + sample[8] - sample[6]);
                    norm.Z = 1.0 / scale;
                    norm.Normalize();
                    // assign the resulting vector to the normal map - remember the working terrain is offset by 1, 1
                    normalMap[x - 1, y - 1] = norm;
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
                ushort output16;

                for (int y = ySize - 1; y >= 0; y--)
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        output16 = (ushort)(terrain[x, y] * 65535);
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
                        if (samples[x] > ushort.MaxValue)
                        {
                            samples[x] = ushort.MaxValue;
                        }
                        else if (samples[x] < 0)
                        {
                            samples[x] = 0;
                        }
                    }
                    byte[] buffer = new byte[samples.Length * sizeof(ushort)];
                    Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
                    output.WriteScanline(buffer, y);
                }
            }
        }

        // save 16-bit grayscale TIFF watermap
        public void saveWaterTIFF(string filename)
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
                        samples[x] = (ushort)(waterMap[x, y] * ushort.MaxValue);
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
                    output8 = (int)(terrain[x, y] * 255);
                    if (output8 < 0) output8 = 0;
                    if (output8 > 255) output8 = 255;
                    bmp.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                }
            }
            return bmp;
        }

        // get a normal map in standard encoding: X = red, Y = green, Z = blue
        public Bitmap getNormalMap()
        {
            Bitmap output = new Bitmap(xSize, ySize);

            int r, g, b;
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    r = ((int)(normalMap[x, y].X * 255) / 2) + 127;
                    g = ((int)(normalMap[x, y].Y * 255) / 2) + 127;
                    b = ((int)(normalMap[x, y].Z * 255) / 2) + 127;
                    output.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return output;
        }

        // return a bitmap of the parts of the terrain where erosion has occured
        public Bitmap getErosionMap()
        {
            Bitmap output = new Bitmap(xSize, ySize);
            int output8;

            normalizeErosion();

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    output8 = (int)(erosion[x, y] * 255);
                    if (output8 < 0) output8 = 0;
                    if (output8 > 255) output8 = 255;
                    output.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                }
            }

            return output;
        }

        // return a bitmap of the parts of the terrain where erosion has occured
        public Bitmap getThermalErosionMap()
        {
            Bitmap output = new Bitmap(xSize, ySize);
            int output8;

            normalizeThermalErosion();

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    output8 = (int)(tErosion[x, y] * 255);
                    if (output8 < 0) output8 = 0;
                    if (output8 > 255) output8 = 255;
                    output.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                }
            }

            return output;
        }

        // return a bitmap of the parts of the terrain where sediment deposition has occured
        public Bitmap getDepositionMap()
        {
            Bitmap output = new Bitmap(xSize, ySize);
            int output8;

            normalizeDeposition();

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    output8 = (int)(deposition[x, y] * 255);
                    if (output8 < 0) output8 = 0;
                    if (output8 > 255) output8 = 255;
                    output.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                }
            }

            return output;
        }

        // return a bitmap of the parts of the terrain where thermal erosion has dropped material
        public Bitmap getTalusMap()
        {
            Bitmap output = new Bitmap(xSize, ySize);
            int output8;

            normalizeTalus();

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    output8 = (int)(talus[x, y] * 255);
                    if (output8 < 0) output8 = 0;
                    if (output8 > 255) output8 = 255;
                    output.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                }
            }

            return output;
        }

        // return a bitmap of the current water levels on the map
        // parameter threshold is the minimum depth in meters to draw to the map
        public Bitmap getWaterMap(float threshold)
        {
            Bitmap bmp = new Bitmap(xSize, ySize);
            // bmp channel values are 8 bits
            int output8;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (waterMap[x, y] * maxAltitude > threshold)
                    {
                        output8 = (int)(waterMap[x, y] * 255);
                    }
                    else
                    {
                        output8 = 0;
                    }
                    if (output8 < 0) output8 = 0;
                    if (output8 > 255) output8 = 255;
                    bmp.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                }
            }
            return bmp;
        }

        public Bitmap getWaterMap()
        {
            Bitmap bmp = new Bitmap(xSize, ySize);
            // bmp channel values are 8 bits
            int output8;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    output8 = (int)(waterMap[x, y] * 255 * 50);                    
                    if (output8 < 0) output8 = 0;
                    if (output8 > 255) output8 = 255;
                    bmp.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                }
            }
            return bmp;
        }

        // return a bitmap of the current dissolved sediment levels
        public Bitmap getSedimentmap()
        {
            Bitmap bmp = new Bitmap(xSize, ySize);
            // bmp channel values are 8 bits
            int output8;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    output8 = (int)(sediment[x, y] * 255 * 1000);
                    if (output8 < 0) output8 = 0;
                    if (output8 > 255) output8 = 255;
                    bmp.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                }
            }
            return bmp;
        }

        //save a copy of the terrain + water heightmap - this generates a second terrain to overlay and produce water
        public void saveWaterRaw(string filename, double thresh)
        {
            double threshold = thresh / maxAltitude;
            if (xSize != ySize)
            {
                throw new IOException(".raw heightmap must be square");
            }
            using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {
                // two bytes for each x, y coordinate when outputing unity .raw heightmaps
                ushort output16;

                for (int y = ySize - 1; y >= 0; y--)
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        if (waterMap[x, y] > threshold)
                        {
                            double output = terrain[x, y] + waterMap[x, y];
                            if (output > 1.0)
                            {
                                output = 1.0;
                            }
                            output16 = (ushort)(output * 65535);
                            writer.Write(output16);
                        }
                        else
                        {
                            output16 = (ushort)(terrain[x, y] * 0.99 * 65535);
                            writer.Write(output16);
                        }
                    }
                }
            }
        }

        // return a bitmap of the slopes on the map: 0 = flat (0 degrees) 1 = vertical (90 degrees)
        public Bitmap getSlopeMap()
        {
            Bitmap output = new Bitmap(xSize, ySize);

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    double slope;
                    // calculate the slope in radians
                    slope = Math.Asin(new Vector(normalMap[x, y].X, normalMap[x, y].Y).Length);
                    // reduce the range back to 0..1
                    slope /= Math.PI / 2;
                    int val = (int)((slope) * 255);
                    Color color = Color.FromArgb(val, val, val);
                    output.SetPixel(x, y, color);
                }
            }

            return output;
        }

        // generate a splat map (control texture) based on slope, altitude, and optional noise
        public Bitmap getSplatMap(double minAlt, double maxAlt, double minAltFuzz, double maxAltFuzz, double minSlope, double maxSlope, double minSlopeFuzz, double maxSlopeFuzz, double noiseAmount)
        {
            Bitmap output = new Bitmap(xSize, ySize);
            double xScale = xActualSize / xSize;
            double yScale = yActualSize / ySize;
            generator.setFrequency(1.0/2000);
            generator.setLacunarity(2.1);
            generator.setMu(1);
            generator.setOctaves(5);
            generator.setPersistance(.5);

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    double value = 1;
                    int value8 = 0;
                    // calculate slope in radians
                    double slope = Math.Asin(new Vector(normalMap[x, y].X, normalMap[x, y].Y).Length);
                    // convert to degrees
                    slope *= 180 / Math.PI;
                    // get altitude
                    double altitude = terrain[x, y] * maxAltitude;
                    
                    if (altitude >= minAlt && altitude <= minAlt + minAltFuzz)
                    {
                        value = value * ((altitude - minAlt) / minAltFuzz);
                    }
                    if (altitude <= maxAlt && altitude >= maxAlt - maxAltFuzz)
                    {
                        value = value * (1 - ((altitude - (maxAlt - maxAltFuzz)) / maxAltFuzz));
                    }
                    if ( slope >= minSlope && slope <= minSlope + minSlopeFuzz)
                    {
                        value = value * ((slope - minSlope) / minSlopeFuzz);
                    }
                    if (slope <= maxSlope && slope >= maxSlope - maxSlopeFuzz)
                    {
                        value = value * (1 -((slope - (maxSlope - maxSlopeFuzz)) / maxSlopeFuzz));
                    }
                    if (altitude < minAlt)
                    {
                        value = 0;
                    }
                    if (altitude > maxAlt)
                    {
                        value = 0;
                    }
                    if (slope > maxSlope)
                    {
                        value = 0;
                    }
                    if (slope < minSlope)
                    {
                        value = 0;
                    }
                    double noise = generator.OctaveExpPerlin2d(x * xScale, y * yScale);
                    value =  value * (noise * noiseAmount + (1 - noiseAmount));
                    value8 = (byte)(value * byte.MaxValue);
                    value8 = Math.Min(byte.MaxValue, value8);
                    value8 = Math.Max(0, value8);
                    output.SetPixel(x, y, Color.FromArgb(value8, value8, value8));
                }
            }

            return output;
        }

        // generate color texture bitmap based on altitude
        public Bitmap getTexture()
        {
            Bitmap output = new Bitmap(xSize, ySize);

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    int latitude = y * texSample.Height / ySize;
                    if (latitude < 0) latitude = 0;
                    if (latitude >= texSample.Height) latitude = texSample.Height - 1;
                    int altitude = (int)(terrain[x, y] * texSample.Width);
                    if (altitude < 0) altitude = 0;
                    if (altitude >= texSample.Width) altitude = texSample.Width - 1;
                    Color color = texSample.GetPixel(altitude, latitude);
                    output.SetPixel(x, y, color);
                }
            }
            return output;
        }

        public Bitmap getTextureSample()
        {
            return texSample;
        }

        public void setTextureSample(Bitmap input)
        {
            if (input.Width == 1024 && input.Height == 1024)
            {
                texSample = input;
            }
        }
    }
}
