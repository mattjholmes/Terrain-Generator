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
    }
}
