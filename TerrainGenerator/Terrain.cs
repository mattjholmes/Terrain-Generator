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
        class WaterParticle
        {
            private double sedimentCapacity;
            private double sedimentCarried;
            public WaterParticle(double sedimentCapacity, double sedimentCarried)
            {
                this.sedimentCapacity = sedimentCapacity;
                this.sedimentCarried = sedimentCarried;
            }

            public double getCapacity()
            {
                return sedimentCapacity;
            }

            public double getCarried()
            {
                return sedimentCarried;
            }

            public void setCapacity(double capacity)
            {
                sedimentCapacity = capacity;
            }

            public void setCarried(double carried)
            {
                sedimentCarried = carried;
            }

            public double calculateLoad()
            {
                double output = 0;

                output = sedimentCarried - sedimentCapacity;
                sedimentCarried = sedimentCapacity;

                return output;
            }
        }
        private class OutflowFlux
        {
            public double left = 0;
            public double right = 0;
            public double up = 0;
            public double down = 0;
        }

        // 2d array to hold the height information
        private double[,] terrain;

        // 2d array of Vector3d to hold normal information
        private Vector3D[,] normalMap;

        // 2d array to hold water information
        private Stack<WaterParticle>[,] water;
        private double[,] waterB;
        private double[,] waterMap;
        private double[,] sediment;
        private OutflowFlux[,] oFlux;
        private Vector[,] waterVel;
        private WaterParticleSystem waterp;

        // 2d array to hold erosion map - used in generating texture control maps 
        private double[,] erosion;
        private double[,] deposition;

        // Maximum altitude of the map, used in slope calculations
        private float maxAltitude;

        // X and Y size of the overall map in meters
        private float xActualSize;
        private float yActualSize;

        // Depth of a single water particle
        private double waterSize;

        // x and y size of terrain map
        private int xSize;
        private int ySize;

        // object variables for water calculations
        double grav;
        double pipeArea;
        double pipeLength;

        // gradient sample map for generating terrain texture
        Bitmap texSample = new Bitmap(1024, 1024);

        // create a noise generator instance for use in terrain generation
        private NoiseGenerator generator = new NoiseGenerator();

        // Terrain class constructer, requires x and y size - this should be square for unity .raw terrain maps, or 2:1 rect for spherical terrain maps
        // xMapSize and yMapSize are the overall map size in meters, this is independent of the underlying bitmap whose size is determined by the x and y parameters
        // Also takes a maximum altitude, in meters
        public Terrain(int x, int y, float xMapSize, float yMapSize, float maxAlt)
        {
            maxAltitude = maxAlt;
            waterSize = (1.0 / 32.0) / 10.0;
            xSize = x;
            ySize = y;
            xActualSize = xMapSize;
            yActualSize = yMapSize;
            terrain = new double[x, y];
            normalMap = new Vector3D[x, y];
            waterB = new double[x, y];
            sediment = new double[x, y];
            water = new Stack<WaterParticle>[x, y];
            erosion = new double[x, y];
            deposition = new double[x, y];
            waterMap = new double[x, y];
            waterVel = new Vector[x, y];
            oFlux = new OutflowFlux[x, y];
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    waterVel[i, j] = new Vector(0, 0);
                    oFlux[i, j] = new OutflowFlux();
                }
            }
            for (x = 0; x < xSize; x++)
            {
                for (y = 0; y < ySize; y++)
                {
                    water[x, y] = new Stack<WaterParticle>();
                }
            }
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

        public float getXMapSize()
        {
            return xActualSize;
        }

        public float getYMapSize()
        {
            return yActualSize;
        }

        public int getXRes()
        {
            return xSize;
        }

        public int getYRes()
        {
            return ySize;
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

        public void setMapSize(float x, float y)
        {
            xActualSize = x;
            yActualSize = y;
        }

        public void setResolution(int x, int y)
        {
            xSize = x;
            ySize = y;
            terrain = new Double[x, y];
            normalMap = new Vector3D[x, y];
        }

        public void setTextureSample()
        {
            Graphics g = Graphics.FromImage(texSample);
            Rectangle targetRect = new Rectangle(new System.Drawing.Point(0, 0), texSample.Size);
            
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
            // divide the frequency by 10 km - this results in macro features ~ 10 km across at a frequency input of 1
            generator.setFrequency(frequency / 10000);
            generator.setLacunarity(lacunarity);
            generator.setMu(mu);
            generator.setOctaves(octaves);
            generator.setPersistance(persistance);
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

        public void generateTerrain(Bitmap input, double weight, double xOffset, double yOffset, double frequency, int octaves, double persistance, double lacunarity, double mu)
        {
            // weight input is expected to be a percentage, IE 0.0...1.0
            if (weight > 1.0 || weight < 0.0)
            {
                throw new ArgumentOutOfRangeException("Weight must be less than 1.0, greater than or equal to 0.0");
            }
            
            Random rand = new Random();
            double[,] inTerrain = new double[input.Width,input.Height];

            generator.setXOffset(xOffset);
            generator.setYOffset(yOffset);
            generator.setFrequency(frequency / 10000);
            generator.setLacunarity(lacunarity);
            generator.setMu(mu);
            generator.setOctaves(octaves);
            generator.setPersistance(persistance);

            double xScale = xActualSize / xSize;
            double yScale = yActualSize / ySize;

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
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    double baseHeight = 0;
                    if ( i < input.Width && j < input.Height)
                    {
                        baseHeight = inTerrain[i, j];
                    }
                    else
                    {
                        Console.WriteLine("Input bitmap too small");
                    }
                    terrain[i, j] = (generator.OctaveExpPerlin2d(i * xScale, j * yScale) * weight) + (baseHeight * (1 - weight));
                }
            }

            normalizeTerrain();
            calculateNormals();
        }

        public void generateTerrain(String path, double weight, double xOffset, double yOffset, double frequency, int octaves, double persistance, double lacunarity, double mu)
        {
            // weight input is expected to be a percentage, IE 0.0...1.0
            if (weight > 1.0 || weight < 0.0)
            {
                throw new ArgumentOutOfRangeException("Weight must be less than 1.0, greater than or equal to 0.0");
            }

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

            generator.setXOffset(xOffset);
            generator.setYOffset(yOffset);
            generator.setFrequency(frequency / 10000);
            generator.setLacunarity(lacunarity);
            generator.setMu(mu);
            generator.setOctaves(octaves);
            generator.setPersistance(persistance);

            double xScale = xActualSize / xSize;
            double yScale = yActualSize / ySize;
            
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    double baseHeight = 0;
                    if (i < width && j < height)
                    {
                        baseHeight = inTerrain[i, j];
                    }
                    else
                    {
                        Console.WriteLine("Input bitmap too small");
                    }
                    terrain[i, j] = (generator.OctaveExpPerlin2d(i * xScale, j * yScale) * weight) + (baseHeight * (1 - weight));
                }
            }

            normalizeTerrain();
            calculateNormals();
        }

        // Normalized the terrain, making the lowest point = 0 and the highest point = 1
        // This is useful to make the color map generation easier, as well as to use the full range of color resolution in the output heightmap
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

        public void thermalErosion(float talusAngle, int passes)
        {
            // maximum difference between neighboring locations
            double maxDiff = ((xActualSize / xSize) * Math.Tan(talusAngle * (Math.PI / 180))) / maxAltitude;
            // amount to move to the neighbor - higher values will lead to stairstepping in the output height map, but require less passes for the same effect
            double hChange = maxDiff / 16;

            for (int i = 0; i < passes; i++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y ++)
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
                         // special case - if current location is the lowest, skip this step
                        for (int nx = -1; nx <= 1; nx++)
                        {
                            for (int ny = -1; ny <= 1; ny++)
                            {
                                // make sure we don't go out of bounds
                                if (x + nx >= 0 && x + nx < xSize && y + ny >= 0 && y + ny < ySize)
                                {
                                    // if we previously marked one of these squares as lower than maxDiff, we will now move material to it
                                    // also add the amount removed to the erosion map, and the amount added to neighbors to the deposition map
                                    if (lowNeighbors[nx +1, ny +1])
                                    {
                                        terrain[x, y] -= amountToMove;
                                        terrain[x + nx, y + ny] += amountToMove;
                                        erosion[x, y] += amountToMove;
                                        deposition[x + nx, y + ny] += amountToMove;
                                    }
                                }
                            }
                        }
                    }// for y
                }// for x
            }// for passes
            calculateNormals();
        }

        public void waterSystem(int numParticles)
        {
            Bitmap waterMap;
            Bitmap heightMap;
            var form = new Form1();
            waterp = new WaterParticleSystem(numParticles);
            Random rand = new Random();

            form.Show();

            for (int p = 0; p < numParticles; p++)
            {
                Vector pos = new Vector(rand.NextDouble() * xActualSize, rand.NextDouble() * yActualSize);
                WaterParticleSystem.Particle particle = new WaterParticleSystem.Particle(pos);
                waterp.addParticle(particle);
            }
            for (int p = 0; p < 500; p++)
            {
                
                waterp.runStep(normalMap, xActualSize / xSize, yActualSize / ySize, xSize, ySize);

                waterMap = getWaterParticleMap();
                form.textBox1.Text = p.ToString();
                form.pictureBox1.Image = waterMap;
                heightMap = getHeightBitmap();
                form.pictureBox2.Image = heightMap;
                form.Update();
            }
        }

        public void hydraulicErosion(double solubility, double rainChance, double evapChance, int passes)
        {
            // random generator for rain, evap chances
            Random rand = new Random();
            Bitmap waterMap = getWaterMap();
            Bitmap heightMap = getHeightBitmap();
            var form = new Form1();
            // solubility represents the fraction of the water particle size that can be filled with sediment
            double maxCapacity = solubility * waterSize * 20;

            form.Show();
            form.pictureBox1.Image = waterMap;
            form.pictureBox2.Image = heightMap;
            form.Update();

            for (int p = 0; p < passes; p++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        // decide if we are going to rain on this square on this pass
                        if (rand.NextDouble() < rainChance)
                        {
                            water[x, y].Push(new WaterParticle(0, 0));
                            
                        }
                        
                        //calculate the number of particles to remove via evaporation in this location
                        int waterToRemove = 0;
                        foreach (WaterParticle w in water[x, y])
                        {
                            if (rand.NextDouble() < evapChance)
                            {
                                waterToRemove++;
                            }
                        }

                        //actually remove the particles, drop their sediment load in place
                        for (int i = 0; i < waterToRemove; i++)
                        {
                            WaterParticle removed = water[x, y].Pop();
                            terrain[x, y] += removed.getCarried();
                        }

                        //iterate through the stack, reduce the capacities of each, and drop some sediment. This means water
                        //which hasn't moved for a while will lose capacity
                        foreach (WaterParticle w in water[x,y])
                        {
                            w.setCapacity(w.getCapacity() * .3);
                            double materialMoved = w.calculateLoad();
                            terrain[x, y] += materialMoved;
                            erosion[x, y] += Math.Abs(materialMoved);
                        }

                        //figure out if the top particle should drain to a neighbor
                        bool cont = true;

                        while (cont)
                        {
                            int minx = 0;
                            int miny = 0;
                            double minHeight = double.MaxValue;
                            for (int nx = -1; nx <= 1; nx++)
                            {
                                for (int ny = -1; ny <= 1; ny++)
                                {
                                    // make sure we don't go out of bounds
                                    if (x + nx >= 0 && x + nx < xSize && y + ny >= 0 && y + ny < ySize)
                                    {
                                        if (terrain[x + nx, y + ny] + water[x + nx, y + ny].Count * waterSize < minHeight)
                                        {
                                            minHeight = terrain[x + nx, y + ny] + water[x + nx, y + ny].Count * waterSize;
                                            minx = x + nx;
                                            miny = y + ny;
                                        }
                                    }
                                }
                            }
                            if (!(x == minx && y == miny) && water[x, y].Count != 0 && terrain[x, y] + water[x, y].Count * waterSize - minHeight > waterSize)
                            {
                                // calculate the slope in radians
                                double slope = Math.Atan(terrain[x, y] + water[x, y].Count * waterSize - minHeight);
                                // capacity = slope / (Pi / 2) * maxCapacity
                                double capacity = (slope / (Math.PI / 2)) * maxCapacity;
                                // set the new capacity based on the move the particle is about to make
                                water[x, y].Peek().setCapacity(capacity);
                                // calculateLoad() returns the amount of material the particle is taking or leaving, and sets the new carried amount
                                double materialMoved = water[x, y].Peek().calculateLoad();
                                terrain[x, y] += materialMoved;
                                erosion[x, y] += Math.Abs(materialMoved);
                                // move the particle to its lowest neighbor
                                water[minx, miny].Push(water[x, y].Pop());
                            }
                            else
                            {
                                cont = false;
                            }
                        }
                    }
                }
                waterMap = getWaterMap();
                form.textBox1.Text = p.ToString();
                form.pictureBox1.Image = waterMap;
                heightMap = getHeightBitmap();
                form.pictureBox2.Image = heightMap;
                form.Update();
            }
            //settleWater();
            normalizeTerrain();
            normalizeErosion();
        }

        public void altHydraulicErosion(double solubility, double rainChance, double evapChance, int passes)
        {
            // random generator for rain, evap chances
            Random rand = new Random();
            Bitmap waterMap = getWaterMapB();
            Bitmap heightMap = getHeightBitmap();
            var form = new Form1();

            form.Show();
            form.pictureBox1.Image = waterMap;
            form.pictureBox2.Image = heightMap;
            form.Update();

            for (int p = 0; p < passes; p++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        // add water to this square, based on rain amount
                        waterB[x, y] += waterSize * rainChance;

                        //figure out where the current cell's water should flow
                        bool[,] lowerNeighbors = new bool[3, 3];
                        int lowNeighborCount = 0;
                        double totalLowerNeighborHeight = 0;
                        double totalDifference = 0;
                        double totalNeighborhoodHeight = terrain[x, y] + waterB[x, y];

                        for (int nx = -1; nx <= 1; nx++)
                        {
                            for (int ny = -1; ny <= 1; ny++)
                            {
                                // make sure we don't go out of bounds
                                if (x + nx >= 0 && x + nx < xSize && y + ny >= 0 && y + ny < ySize)
                                {
                                    if (nx == 0 && ny == 0)
                                    {
                                        lowerNeighbors[nx + 1, ny + 1] = false;
                                    }
                                    else if ((terrain[x, y] + waterB[x, y]) - (terrain[x + nx, y + ny] + waterB[x + nx, y + ny]) > 0)
                                    {
                                        lowerNeighbors[nx + 1, ny + 1] = true;
                                        lowNeighborCount++;
                                        totalLowerNeighborHeight += terrain[x + nx, y + ny] + waterB[x + nx, y + ny];
                                        totalDifference += (terrain[x, y] + waterB[x, y]) - (terrain[x + nx, y + ny] + waterB[x + nx, y + ny]);
                                        totalNeighborhoodHeight += terrain[x + nx, y + ny] + waterB[x + nx, y + ny];
                                    }
                                    else
                                    {
                                        lowerNeighbors[nx + 1, ny + 1] = false;
                                    }
                                }
                            }
                        }

                        double avgNeighborAlt = totalLowerNeighborHeight / lowNeighborCount;
                        double avgAltitude = (totalLowerNeighborHeight + (terrain[x, y] + waterB[x, y])) / (lowNeighborCount + 1) ;
                        
                        double totalWaterMoved = 0;

                        if ((terrain[x, y] + waterB[x, y]) - avgNeighborAlt > 0.00001)
                        {
                            sediment[x, y] += waterB[x, y] * solubility * Math.Min(0.1, ((terrain[x, y] + waterB[x, y]) - avgNeighborAlt));
                            terrain[x, y] -= waterB[x, y] * solubility * Math.Min(0.1, ((terrain[x, y] + waterB[x, y]) - avgNeighborAlt));
                        }

                        double currentSediment = sediment[x, y];
                        for (int nx = -1; nx <= 1; nx++)
                        {
                            for (int ny = -1; ny <= 1; ny++)
                            {
                                // make sure we don't go out of bounds
                                if (x + nx >= 0 && x + nx < xSize + 1 && y + ny >= 0 && y + ny < ySize + 1)
                                {
                                    if (lowerNeighbors[nx + 1, ny + 1])
                                    {
                                        double difference = (waterB[x, y] + terrain[x, y]) - (waterB[x + nx, y + ny] + terrain[x + nx, y + ny]);
                                        double waterMoved = Math.Min(waterB[x,y], terrain[x, y] + waterB[x, y] - avgAltitude) * (difference / totalDifference);
                                        waterB[x + nx, y + ny] += waterMoved;
                                        waterB[x, y] -= waterMoved;
                                        sediment[x + nx, y + ny] += waterMoved * currentSediment;
                                        totalWaterMoved += waterMoved;
                                    }
                                }
                            }
                        }
                        
                        sediment[x, y] -= totalWaterMoved * currentSediment;

                        waterB[x, y] *= (1 - evapChance);

                        double maxSediment = waterB[x, y] * solubility * .0001;

                        if (sediment[x,y] > maxSediment)
                        {
                            terrain[x, y] += sediment[x, y] - maxSediment;
                            sediment[x, y] -= sediment[x, y] - maxSediment; 
                        }
                    }
                }
                waterMap = getWaterMapB();
                form.textBox1.Text = p.ToString();
                form.pictureBox1.Image = waterMap;
                heightMap = getHeightBitmap();
                form.pictureBox2.Image = heightMap;
                form.Update();
            }
            //normalizeTerrain();
            //normalizeErosion();
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    terrain[x, y] += sediment[x, y];
                }
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
            Random rand = new Random();
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
                //waterMap[xSize / 2, ySize / 2] += (1/maxAltitude) * timeStep;

                // second step - simulate flow, update waterMap and waterVel

                // update the outflowFlux map
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        // calculate the flow in each direction, edge cases = 0
                        if (x - 1 >= 0)
                        {
                            oFlux[x, y].left = Math.Max(0, oFlux[x, y].left + timeStep * pipeArea * ((grav * (terrain[x, y] + waterMap[x, y] - terrain[x - 1, y] + waterMap[x - 1, y])) / pipeLength));
                        }
                        else
                        {
                            oFlux[x, y].left = 0;
                        }

                        if (x + 1 < xSize)
                        {
                            oFlux[x, y].right = Math.Max(0, oFlux[x, y].right + timeStep * pipeArea * ((grav * (terrain[x, y] + waterMap[x,y] - terrain[x + 1, y] + waterMap[x + 1, y])) / pipeLength));
                        }
                        else
                        {
                            oFlux[x, y].right = 0;
                        }

                        if (y - 1 >= 0)
                        {
                            oFlux[x, y].up = Math.Max(0, oFlux[x, y].up + timeStep * pipeArea * ((grav * (terrain[x, y] + waterMap[x,y] - terrain[x, y - 1] + waterMap[x, y - 1])) / pipeLength));
                        }
                        else
                        {
                            oFlux[x, y].up = 0;
                        }

                        if (y + 1 < ySize)
                        {
                            oFlux[x, y].down = Math.Max(0, oFlux[x, y].down + timeStep * pipeArea * ((grav * (terrain[x, y] + waterMap[x, y] - terrain[x, y + 1] + waterMap[x, y + 1])) / pipeLength));
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

                // drop all remaining sediment back on the terrain, and remove the water
                /*for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        waterMap[x, y] = 0;
                        terrain[x, y] += sediment[x, y];
                        deposition[x, y] += sediment[x, y];
                        sediment[x, y] = 0;
                    }
                }*/

                /*waterBmp = getWaterMap();
                form.textBox1.Text = n.ToString();
                form.pictureBox1.Image = waterBmp;
                heightMap = getHeightBitmap();
                form.pictureBox2.Image = heightMap;
                sedimentMap = getSedimentmap();
                form.pictureBox3.Image = sedimentMap;
                form.Update();*/
            }
        }

        private void settleWater()
        {
            int passes = 10;
            
            for (int p = 0; p < passes; p++)
            {
                int[] xOrder = generateRandomOrder(xSize);
                int[] yOrder = generateRandomOrder(ySize);
                foreach (int x in xOrder)
                {
                    foreach (int y in yOrder)
                    {
                        bool cont = true;

                        while (cont)
                        {
                            int minx = 0;
                            int miny = 0;
                            double minHeight = double.MaxValue;
                            for (int nx = -1; nx <= 1; nx++)
                            {
                                for (int ny = -1; ny <= 1; ny++)
                                {
                                    // make sure we don't go out of bounds
                                    if (x + nx >= 0 && x + nx < xSize && y + ny >= 0 && y + ny < ySize)
                                    {
                                        if (terrain[x + nx, y + ny] + water[x + nx, y + ny].Count * waterSize < minHeight)
                                        {
                                            minHeight = terrain[x + nx, y + ny] + water[x + nx, y + ny].Count * waterSize;
                                            minx = x + nx;
                                            miny = y + ny;
                                        }
                                    }
                                }
                            }
                            if (!(x == minx && y == miny) && water[x, y].Count != 0 && terrain[x, y] + water[x, y].Count * waterSize - minHeight > waterSize)
                            {
                                WaterParticle moved = water[x, y].Pop();
                                water[minx, miny].Push(moved);
                            }
                            else
                            {
                                cont = false;
                            }
                        }
                    }
                }
            }
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

            Bitmap bmp = new Bitmap(xSize + 2, ySize + 2);
            // bmp channel values are 8 bits
            int output8;

            for (int x = 0; x < xSize + 2; x++)
            {
                for (int y = 0; y < ySize + 2; y++)
                {
                    output8 = (int)(workTerrain[x, y] * 255);
                    if (output8 < 0) output8 = 0;
                    if (output8 > 255) output8 = 255;
                    bmp.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                }
            }
            bmp.Save("normalWorkTerrain.bmp");
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

        // generate grayscale watermap
        public Bitmap getWaterParticleMap(int threshold)
        {
            Bitmap output = new Bitmap(xSize, ySize);
            int output8;

            /*
            int max = 0;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    max = Math.Max(max, water[x, y].Count);
                }
            }
            */

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    if (water[x, y].Count > threshold)
                    {
                        output8 = (int)(water[x, y].Count * waterSize * 255 * 10);
                        if (output8 < 0) output8 = 0;
                        if (output8 > 255) output8 = 255;
                        output.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                    }
                    else
                    {
                        output.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0));
                    }
                }
            }

            return output;
        }

        public Bitmap getWaterMapB()
        {
            Bitmap bmp = new Bitmap(xSize, ySize);
            // bmp channel values are 8 bits
            int output8;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    output8 = (int)(waterB[x, y] * 255 * 30);
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
                    output8 = (int)(waterMap[x, y] * 255 *4000);
                    if (output8 < 0) output8 = 0;
                    if (output8 > 255) output8 = 255;
                    bmp.SetPixel(x, y, Color.FromArgb(255, output8, output8, output8));
                }
            }
            return bmp;
        }

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
                UInt16 output16;

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

        public Bitmap getWaterParticleMap()
        {
            Bitmap output = new Bitmap(xSize, ySize);

            /*for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    output.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                }
            }*/

            output = getHeightBitmap();

            for (int i = 0; i < waterp.getNumParticles(); i++)
            {
                Vector position = waterp.getParticleAt(i).getPosition();
                int xPos = (int)(position.X / (xActualSize / xSize));
                int yPos = (int)(position.Y / (yActualSize / ySize));
                if (xPos >= 0 && yPos >= 0 && xPos < xSize && yPos < ySize)
                {
                    output.SetPixel(xPos, yPos, Color.Red);
                }
            }

            return output;
        }

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
        public Bitmap getSplatMap(double minAlt, double maxAlt, double altFuzz, double minSlope, double maxSlope, double slopeFuzz, double noiseAmount)
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
                    
                    if (altitude >= minAlt && altitude <= minAlt + altFuzz)
                    {
                        value = value * ((altitude - minAlt) / altFuzz);
                    }
                    if (altitude <= maxAlt && altitude >= maxAlt - altFuzz)
                    {
                        value = value * (1 - ((altitude - (maxAlt - altFuzz)) / altFuzz));
                    }
                    if ( slope >= minSlope && slope <= minSlope + slopeFuzz)
                    {
                        value = value * ((slope - minSlope) / slopeFuzz);
                    }
                    if (slope <= maxSlope && slope >= maxSlope - slopeFuzz)
                    {
                        value = value * (1 -((slope - (maxSlope - slopeFuzz)) / slopeFuzz));
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

        // generate color texture bitmap
        public Bitmap getTexture()
        {
            Bitmap output = new Bitmap(xSize, ySize);
            Color slopeColor = Color.FromArgb(100, 100, 89);
            //Color slopeColor = Color.Red;
            double slope;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    // calculate the slope in radians
                    slope = Math.Asin(new Vector(normalMap[x, y].X, normalMap[x, y].Y).Length);
                    // reduce the range back to 0..1
                    slope /= Math.PI / 2;

                    slope = Fade(slope);

                    int altitude = (int)(terrain[x, y] * texSample.Width);
                    if (altitude < 0) altitude = 0;
                    if (altitude >= texSample.Width) altitude = texSample.Width - 1;
                    Color color = texSample.GetPixel(altitude, 0);
                    //color = color.Blend(slopeColor, 1 - slope);
                    output.SetPixel(x, y, color);
                }
            }

            return output;
        }
    }
}
