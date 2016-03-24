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
            int resolution = 1024;
            int octaves = 8;
            double frequency = 5;
            double persistance = .5;
            double lacunarity = 2.12;
            double mu = 1.005;
            NoiseGenerator generator = new NoiseGenerator();
            string filename = "terrain.raw";
            string bmpFile = "terrain.bmp";
            Bitmap bmp = new Bitmap(resolution, resolution);

            using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {

                for (int i = 0; i < resolution; i++)
                {
                    for (int j = 0; j < resolution; j++)
                    {
                        double output = generator.OctaveExpPerlin2d(i * .001 + 9, j * .001 + 9, frequency, octaves, persistance, lacunarity, mu);
                        UInt16 output16 = (ushort)(output * 65536);
                        int output8 = (int)(output * 256);
                        //Console.WriteLine(output);
                        writer.Write(output16);
                        bmp.SetPixel(i, j, Color.FromArgb(255, output8, output8, output8));
                    }
                }
            }

            bmp.Save(bmpFile);
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());*/
        }
    }
}