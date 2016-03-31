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

        // 2d array to hold the height information
        private double[,] terrain;

        // 2d array of Vector3d to hold normal information
        private Vector3D[,] normalMap;

        // 2d array to hold water information
        private Stack<WaterParticle>[,] water;
        private double[,] waterB;
        private double[,] sediment;
        private WaterParticleSystem waterp;

        // 2d array to hold erosion map - used in generating texture control maps 
        private double[,] erosion;

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

        public void thermalErosion(float talusAngle, int passes)
        {
            // maximum difference between neighboring locations
            double maxDiff = ((xActualSize / xSize) * Math.Tan(talusAngle * (Math.PI / 180))) / maxAltitude;
            // amount to move to the neighbor - higher values will lead to stairstepping in the output height map, but require less passes for the same effect
            double hChange = maxDiff / 4;

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
                                    if (lowNeighbors[nx +1, ny +1])
                                    {
                                        terrain[x, y] -= amountToMove;
                                        terrain[x + nx, y + ny] += amountToMove;
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
            Bitmap waterMap = getWaterMap(0);
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
                waterMap = getWaterMap(0);
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
                    double scale = maxAltitude / Math.Min(xActualSize, yActualSize);
                    Vector3D norm = new Vector3D();
                    // calculate the normal using a sobel filter - scale determined by ratio of height/mapsize
                    norm.X = scale * -(sample[6] - sample[0] + 2 * (sample[7] - sample[1]) + sample[8] - sample[2]);
                    norm.Y = scale * (sample[2] - sample[0] + 2 * (sample[5] - sample[3]) + sample[8] - sample[6]);
                    norm.Z = 1.0;
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

        // generate grayscale watermap
        public Bitmap getWaterMap(int threshold)
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

        //save a copy of the terrain + water heightmap - this generates a second terrain to overlay and produce water
        public void saveWaterRaw(string filename, int threshold)
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
                        if (water[x, y].Count > threshold)
                        {
                            double output = terrain[x, y] + (water[x, y].Count * waterSize);
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
