using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static TerrainGenerator.MathUtility;

namespace TerrainGenerator
{
    class NoiseGenerator
    {
        static NoiseGenerator() // static constructor to normalize the gradient array
        {
            for (int i = 0; i < grad2d.Length; i ++)
            {
                grad2d[i].Normalize();
            }
        }

        // precalculate square root of 2, for use in 2d noise function
        private static readonly double sqr2 = Math.Sqrt(2);

        // size of repetition, if 0, does not repeat
        private int repeat = 0;

        // Frequency determines the feature size of the first iteration of noise, smaller numbers make larger features
        private double frequency = 1;
        // Octaves are the number of iterations of noise to layer
        private int octaves = 5;
        // Persistence is the amplitude multiplier in each successive octave of noise
        private double persistence = .5;
        // Lacunarity is the frequency multiplier in each successive octave of noise
        private double lacunarity = 2;
        // Mu is the exponential decay rate for the noise distribution table
        private double mu = 1;
        // offset "moves" the window into the noise field around.. effectively changes the random generation seed
        private double xOffset = 0;
        private double yOffset = 0;

        public void setFrequency(double freq)
        {
            frequency = freq;
        }

        public void setOctaves(int oct)
        {
            octaves = oct;
        }

        public void setPersistance(double pers)
        {
            persistence = pers;
        }

        public void setLacunarity(double lac)
        {
            lacunarity = lac;
        }

        public void setMu(double m)
        {
            mu = m;
            InitExpMagTable();
        }

        public void setXOffset(double xoff)
        {
            xOffset = xoff;
        }

        public void setYOffset(double yoff)
        {
            yOffset = yoff;
        }

        public double getFrequency()
        {
            return frequency;
        }

        public int getOctaves()
        {
            return octaves;
        }

        public double getPersistence()
        {
            return persistence;
        }

        public double getLacunarity()
        {
            return lacunarity;
        }

        public double getMu()
        {
            return mu;
        }

        public double getXOffset()
        {
            return xOffset;
        }

        public double getYOffset()
        {
            return yOffset;
        }


        // permutation table for generating psuedorandom numbers
        private static int[] p =
        {
            120, 227, 122, 223, 150, 210, 83, 69, 231, 131, 225, 55, 160, 121, 86, 232, 1, 93, 13, 188, 41, 117, 134, 113, 90, 23, 3, 9, 108,
            72, 197, 79, 255, 163, 214, 155, 208, 220, 56, 224, 202, 248, 42, 34, 171, 6, 145, 174, 172, 20, 57, 181, 111, 203, 62, 156, 43,
            25, 61, 40, 239, 60, 196, 44, 29, 75, 116, 103, 119, 254, 22, 195, 182, 33, 76, 26, 235, 153, 7, 178, 173, 102, 133, 18, 114, 17,
            249, 193, 159, 130, 12, 157, 14, 65, 87, 221, 28, 88, 67, 50, 180, 169, 152, 206, 199, 198, 24, 70, 141, 252, 215, 85, 185, 187,
            247, 89, 91, 99, 8, 233, 158, 71, 184, 175, 58, 46, 228, 226, 183, 109, 253, 54, 124, 162, 48, 179, 177, 74, 77, 59, 142, 96, 242,
            10, 143, 146, 204, 132, 201, 27, 209, 95, 222, 139, 151, 38, 82, 135, 148, 241, 63, 19, 37, 49, 238, 31, 251, 234, 0, 246, 168,
            106, 107, 128, 45, 68, 165, 30, 21, 176, 94, 2, 211, 125, 217, 189, 229, 127, 98, 147, 64, 194, 186, 51, 123, 149, 110, 237, 4,
            161, 129, 15, 166, 218, 144, 154, 104, 190, 118, 138, 97, 5, 137, 240, 81, 244, 105, 167, 115, 84, 243, 92, 192, 250, 205, 140,
            112, 100, 245, 213, 52, 16, 207, 78, 212, 35, 73, 47, 32, 53, 136, 80, 236, 230, 11, 170, 164, 39, 219, 216, 66, 101, 191, 126,
            200, 36, 120, 227, 122, 223, 150, 210, 83, 69, 231, 131, 225, 55, 160, 121, 86, 232, 1, 93, 13, 188, 41, 117, 134, 113, 90, 23, 3, 9, 108,
            72, 197, 79, 255, 163, 214, 155, 208, 220, 56, 224, 202, 248, 42, 34, 171, 6, 145, 174, 172, 20, 57, 181, 111, 203, 62, 156, 43,
            25, 61, 40, 239, 60, 196, 44, 29, 75, 116, 103, 119, 254, 22, 195, 182, 33, 76, 26, 235, 153, 7, 178, 173, 102, 133, 18, 114, 17,
            249, 193, 159, 130, 12, 157, 14, 65, 87, 221, 28, 88, 67, 50, 180, 169, 152, 206, 199, 198, 24, 70, 141, 252, 215, 85, 185, 187,
            247, 89, 91, 99, 8, 233, 158, 71, 184, 175, 58, 46, 228, 226, 183, 109, 253, 54, 124, 162, 48, 179, 177, 74, 77, 59, 142, 96, 242,
            10, 143, 146, 204, 132, 201, 27, 209, 95, 222, 139, 151, 38, 82, 135, 148, 241, 63, 19, 37, 49, 238, 31, 251, 234, 0, 246, 168,
            106, 107, 128, 45, 68, 165, 30, 21, 176, 94, 2, 211, 125, 217, 189, 229, 127, 98, 147, 64, 194, 186, 51, 123, 149, 110, 237, 4,
            161, 129, 15, 166, 218, 144, 154, 104, 190, 118, 138, 97, 5, 137, 240, 81, 244, 105, 167, 115, 84, 243, 92, 192, 250, 205, 140,
            112, 100, 245, 213, 52, 16, 207, 78, 212, 35, 73, 47, 32, 53, 136, 80, 236, 230, 11, 170, 164, 39, 219, 216, 66, 101, 191, 126,
            200, 36
        };

        // bitmask for the permutation table
        private const int hashMask = 255;

        // table to hold the exponential noise distribution
        private double[] m = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        };

        // table containing the possible vectors to be used in generating the gradients
        private static Vector[] grad2d = {
            new Vector(1, 0),
            new Vector(-1, 0),
            new Vector(0, 1),
            new Vector(0, -1),
            new Vector(1, 1),
            new Vector(-1, 1),
            new Vector(1, -1),
            new Vector(-1, -1)
        };

        // bitmask for the vector table
        private const int grad2dmask = 7;

        // function to initialized the exponential noise distribution table
        private void InitExpMagTable()
        {
            double s = 1.0; // initial magnitude
            for (int i = 0; i < m.Length; i++)
            {
                m[i] = s;
                s /= mu;
            }
        }

        // increment function - to allow tiling to wrap properly
        private int Inc(int num)
        {
            num++;
            if (repeat > 0) num %= repeat;
            return num;
        }

        public double Perlin2dNoise(double x, double y)
        {
            if (repeat > 0)
            {
                x %= repeat;
                y %= repeat;
            }
            int ix0 = (int)Math.Floor(x);
            int iy0 = (int)Math.Floor(y);
            double tx0 = x - ix0;
            double ty0 = y - iy0;
            double tx1 = tx0 - 1;
            double ty1 = ty0 - 1;
            ix0 &= hashMask;
            iy0 &= hashMask;
            int ix1 = ix0 + 1;
            int iy1 = iy0 + 1;

            int h0 = p[ix0];
            int h1 = p[ix1];

            Vector g00 = grad2d[p[h0 + iy0] & grad2dmask];
            Vector g10 = grad2d[p[h1 + iy0] & grad2dmask];
            Vector g01 = grad2d[p[h0 + iy1] & grad2dmask];
            Vector g11 = grad2d[p[h1 + iy1] & grad2dmask];

            double v00 = Dot(g00, tx0, ty0);
            double v10 = Dot(g10, tx1, ty0);
            double v01 = Dot(g01, tx0, ty1);
            double v11 = Dot(g11, tx1, ty1);

            double tx = Fade(tx0);
            double ty = Fade(ty0);

            double a = v00;
            double b = v10 - v00;
            double c = v01 - v00;
            double d = v11 - v01 - v10 + v00;

            double output = (a + b * tx + (c + d * tx) * ty) * sqr2;

            return (output + 1) / 2;
        }

        public double ExpPerlin2dNoise(double x, double y)
        {
            if (repeat > 0)
            {
                x %= repeat;
                y %= repeat;
            }
            int ix0 = (int)Math.Floor(x);
            int iy0 = (int)Math.Floor(y);
            double tx0 = x - ix0;
            double ty0 = y - iy0;
            double tx1 = tx0 - 1;
            double ty1 = ty0 - 1;
            ix0 &= hashMask;
            iy0 &= hashMask;
            int ix1 = ix0 + 1;
            int iy1 = iy0 + 1;

            int h0 = p[ix0];
            int h1 = p[ix1];

            Vector g00 = grad2d[p[h0 + iy0] & grad2dmask];
            Vector g10 = grad2d[p[h1 + iy0] & grad2dmask];
            Vector g01 = grad2d[p[h0 + iy1] & grad2dmask];
            Vector g11 = grad2d[p[h1 + iy1] & grad2dmask];

            int m00 = p[(p[ix0] + iy0) & hashMask];
            int m10 = p[(p[ix1] + iy0) & hashMask];
            int m01 = p[(p[ix0] + iy1) & hashMask];
            int m11 = p[(p[ix1] + iy1) & hashMask];

            double v00 = Dot(g00 , tx0, ty0) * m[m00];
            double v10 = Dot(g10, tx1, ty0) * m[m10];
            double v01 = Dot(g01, tx0, ty1) * m[m01];
            double v11 = Dot(g11, tx1, ty1) * m[m11];

            double tx = Fade(tx0);
            double ty = Fade(ty0);

            double a = v00;
            double b = v10 - v00;
            double c = v01 - v00;
            double d = v11 - v01 - v10 + v00;

            double output = (a + b * tx + (c + d * tx) * ty) * sqr2;

            return (output + 1) / 2;
        }

        public double OctavePerlin2d(double x, double y)
        {
            // make a copy of the frequency, we will be modifying it for just a single execution of this function
            double freq = frequency;
            double total = 0;
            double amplitude = 1;
            double maxValue = 0;
            for (int i = 0; i < octaves; i++)
            {
                total += Perlin2dNoise(x * freq + xOffset, y * freq + yOffset) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                freq *= lacunarity;
            }
            return total / maxValue;
        }

        public double OctaveExpPerlin2d(double x, double y)
        {
            // make a copy of the frequency, we will be modifying it for just a single execution of this function
            double freq = frequency;
            double total = 0;
            double amplitude = 1;
            double maxValue = 0;
            for (int i = 0; i < octaves; i++)
            {
                total += ExpPerlin2dNoise(x * freq + xOffset, y * freq + yOffset) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                freq *= lacunarity;
            }
            return total / maxValue;
        }
    }
}

