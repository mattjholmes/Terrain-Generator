using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TerrainGenerator
{
    public partial class TerrainGeneratorMainForm : Form
    {
        private Terrain terrain;
        int xSize = 1024;
        int ySize = 1024;
        float xMapSize = 10000;
        float yMapSize = 10000;
        float maxAlt = 1000;
        int octaves = 8;
        double frequency = 1;
        double persistance = .45;
        double lacunarity = 1.95;
        double mu = 1.02; // useful range - 1.0 - about 1.01
        double xOffset = 8.9;
        double yOffset = 12.2;

        ColorBlend cb;

        public TerrainGeneratorMainForm()
        {
            InitializeComponent();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void quitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox box = new AboutBox())
            {
                box.ShowDialog(this);
            }
        }

        private void TerrainGeneratorMainForm_Load(object sender, EventArgs e)
        {

        }

        private void generateTerrainButton_Click(object sender, EventArgs e)
        {
            xOffset = Double.Parse(xOffsetTextBox.Text);
            yOffset = Double.Parse(yOffsetTextBox.Text);
            cb = new ColorBlend();
            cb.Positions = new[] { 0, 1 / 3f, 1 / 2f, 3 / 4f, 7 / 8f, 1 };
            cb.Colors = new[] { Color.FromArgb(61, 84, 51), Color.FromArgb(35, 50, 32), Color.FromArgb(35, 50, 32), Color.FromArgb(160, 153, 147), Color.FromArgb(247, 247, 251), Color.FromArgb(247, 247, 251) };

            terrain = new Terrain(xSize, ySize, xMapSize, yMapSize, maxAlt);
            terrain.generateTerrain(xOffset, yOffset, frequency, octaves, persistance, lacunarity, mu);
            terrain.setTextureSample(cb);

            Bitmap map = terrain.getHeightBitmap();

            heightMapPicture.Image = map;

            map = terrain.getTexture();

            colorMapPicture.Image = map;

            waterMapPicture.Image = map;
            customMap1Picture.Image = map;
            customMap2Picture.Image = map;

            Update();
        }

        private void customMap1Tab_Click(object sender, EventArgs e)
        {

        }

        private void importMapButton_Click(object sender, EventArgs e)
        {
            // show the file open dialog, and get the path from it
            DialogResult result = importMapDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (importMapDialog.FileName.EndsWith(".tif"))
                {
                    try
                    {
                        terrain = new Terrain(xSize, ySize, xMapSize, yMapSize, maxAlt);
                        terrain.terrainFromTIFF(importMapDialog.FileName);

                        Bitmap map = terrain.getHeightBitmap();

                        heightMapPicture.Image = map;
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else if (importMapDialog.FileName.EndsWith(".bmp"))
                {
                    terrain = new Terrain(xSize, ySize, xMapSize, yMapSize, maxAlt);
                    Bitmap inputBmp = new Bitmap(importMapDialog.FileName);
                    terrain.terrainFromBmp(inputBmp);
                    Bitmap map = terrain.getHeightBitmap();

                    heightMapPicture.Image = map;
                }
                else
                {
                    MessageBox.Show("Could not import selected map");
                }
                
            }


            Console.WriteLine(result);
            Console.WriteLine(importMapDialog.FileName);
        }

        private void textBoxFilterNonNumeric(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            if (ch == 46 && xOffsetTextBox.Text.IndexOf('.') != -1)
            {
                e.Handled = true;
                return;
            }

            if (!Char.IsDigit(ch) && ch != 8 && ch != 46)
            {
                e.Handled = true;
            }
        }

        private void textBoxFilterNonInteger(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            if (!Char.IsDigit(ch) && ch != 8)
            {
                e.Handled = true;
            }
        }
    }
}
