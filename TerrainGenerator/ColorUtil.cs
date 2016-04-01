using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TerrainGenerator
{
    static class ColorUtil
    {
        public static Color Blend(this Color color, Color underColor, double amount)
        {
            byte r = (byte)((color.R * amount) + underColor.R * (1 - amount));
            byte g = (byte)((color.G * amount) + underColor.G * (1 - amount)); ;
            byte b = (byte)((color.B * amount) + underColor.B * (1 - amount)); ;
            return Color.FromArgb(r, g, b);
        }
    }
}
