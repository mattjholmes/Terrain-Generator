using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TerrainGenerator
{
    static class MathUtility
    {
        // linear interpolation function
        public static double Lerp(double input1, double input2, double weight)
        {
            return (1 - weight) * input1 + weight * input2;

        }

        // Fade function used to ease the coordinate values and smooth the final output
        public static double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        // vector dot product function
        public static double Dot(Vector g, double x, double y)
        {
            return g.X * x + g.Y * y;
        }

        // generate a random list including all ints ONCE from 0...Max
        public static int[] generateRandomOrder(int max)
        {
            List<int> source = new List<int>();
            List<int> output = new List<int>();
            Random rand = new Random();

            for (int i = 0; i < max; i++)
            {
                source.Add(i);
            }
            for (int i = source.Count; i > 0; i--)
            {
                int r = rand.Next(i);
                output.Add(source.ElementAt(r));
                source.RemoveAt(r);
            }

            return output.ToArray();
        }

        // generate a random list including all ints ONCE from Min...Max
        public static int[] generateRandomOrder(int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException("Min may not be greater than max");
            }
            List<int> source = new List<int>();
            List<int> output = new List<int>();
            Random rand = new Random();

            for (int i = min; i < max; i++)
            {
                source.Add(i);
            }
            for (int i = source.Count; i > 0; i--)
            {
                int r = rand.Next(i);
                output.Add(source.ElementAt(r));
                source.RemoveAt(r);
            }

            return output.ToArray();
        }
    }
}
