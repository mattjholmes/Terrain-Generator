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
        private int xSize = 1024;
        private int ySize = 1024;
        private float xMapSize = 10000;
        private float yMapSize = 10000;
        private float maxAlt = 1000;
        private int octaves = 8;
        private double frequency = 1;
        private double persistance = .45;
        private double lacunarity = 1.95;
        private double mu = 1.02; // useful range - 1.0 - about 1.01
        private double noiseWeight = .5;
        private double xOffset = 8.9;
        private double yOffset = 12.2;
        private float talusAngle = 30;
        private int tErodePasses = 50;
        private double solubility = .01;
        private double depositionRate = .1;
        private double waterCapacity = .01;
        private double rainChance = .001;
        private double rainAmount = 1;
        private double evaporation = .0005;
        private double timeStep = 1;
        private int hydroErodePasses = 100;


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
            xOffset = (double)xOffsetNum.Value;
            yOffset = (double)yOffsetNum.Value;
            xSize = (int)xSizeNum.Value;
            ySize = (int)ySizeNum.Value;
            maxAlt = (float)maxAltNum.Value;
            frequency = (double)frequencyNum.Value;
            octaves = (int)octavesNum.Value;
            persistance = (double)persistenceNum.Value;
            lacunarity = (double)lacunarityNum.Value;
            mu = (double)muNum.Value;

            

            terrain = new Terrain(xSize, ySize, xMapSize, yMapSize, maxAlt);
            terrain.generateTerrain(xOffset, yOffset, frequency, octaves, persistance, lacunarity, mu);

            if (texSamplePicture.Image == null)
            {
                cb = new ColorBlend();
                cb.Positions = new[] { 0, 1 / 3f, 1 / 2f, 3 / 4f, 7 / 8f, 1 };
                cb.Colors = new[] { Color.FromArgb(61, 84, 51), Color.FromArgb(35, 50, 32), Color.FromArgb(35, 50, 32), Color.FromArgb(160, 153, 147), Color.FromArgb(247, 247, 251), Color.FromArgb(247, 247, 251) };
                terrain.setTextureSample(cb);
                texSamplePicture.Image = terrain.getTextureSample();
            }

            heightMapPicture.Image = terrain.getHeightBitmap();

            colorMapPicture.Image = terrain.getTexture();
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
                }
                else
                {
                    MessageBox.Show("Could not import selected map");
                    return;
                }

                if (texSamplePicture.Image == null)
                {
                    cb = new ColorBlend();
                    cb.Positions = new[] { 0, 1 / 3f, 1 / 2f, 3 / 4f, 7 / 8f, 1 };
                    cb.Colors = new[] { Color.FromArgb(61, 84, 51), Color.FromArgb(35, 50, 32), Color.FromArgb(35, 50, 32), Color.FromArgb(160, 153, 147), Color.FromArgb(247, 247, 251), Color.FromArgb(247, 247, 251) };
                    terrain.setTextureSample(cb);
                    texSamplePicture.Image = terrain.getTextureSample();
                }
                
                colorMapPicture.Image = terrain.getTexture();
                heightMapPicture.Image = terrain.getHeightBitmap();
            }
        }

        private void tErodeRunButton_Click(object sender, EventArgs e)
        {
            // extract relevant values from the form
            talusAngle = (float)talusAngleNum.Value;
            tErodePasses = (int)tErodePassNum.Value;

            // make sure the terrain has been generated or imported
            if (terrain != null)
            {
                // create a new modal dialog that will execute the erosion process, with progress bar
                ProgressDialog progress = new ProgressDialog("Running Thermal Erosion", ThermalErosion);
                progress.ShowDialog();
                heightMapPicture.Image = terrain.getHeightBitmap();
                colorMapPicture.Image = terrain.getTexture();
            }
            else
            {
                MessageBox.Show("Please generate a terrain before running erosion.");
            }
        }

        private void ThermalErosion(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int p = 0; p < tErodePasses; p++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                worker.ReportProgress(p * 100 / tErodePasses);
                terrain.thermalErosion(talusAngle);
            }
        }

        private void hydroErodeRunButton_Click(object sender, EventArgs e)
        {
            // extract relevant values from the form
            solubility = (double)solubilityNum.Value;
            depositionRate = (double)depRateNum.Value;
            waterCapacity = (double)waterCapNum.Value;
            rainChance = (double)rainChanceNum.Value;
            rainAmount = (double)rainAmountNum.Value;
            evaporation = (double)evapConstNum.Value;
            timeStep = (double)timeStepNum.Value;
            hydroErodePasses = (int)hydroPassesNum.Value;

            // make sure the terrain has been generated or imported
            if (terrain != null)
            {
                // create a new modal dialog that will execute the erosion process, with progress bar
                ProgressDialog progress = new ProgressDialog("Running Hydraulic Erosion", HydroErosion);
                progress.ShowDialog();
                heightMapPicture.Image = terrain.getHeightBitmap();
                colorMapPicture.Image = terrain.getTexture();
                waterMapPicture.Image = terrain.getWaterMap(0.0f);
            }
            else
            {
                MessageBox.Show("Please generate a terrain before running erosion.");
            }
        }

        private void HydroErosion(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int p = 0; p < hydroErodePasses; p++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                worker.ReportProgress(p * 100 / hydroErodePasses);
                terrain.vFieldHydroErosion(solubility, depositionRate, waterCapacity, rainChance, rainAmount, evaporation, timeStep);
            }
        }

        private void importTexSample_Click(object sender, EventArgs e)
        {
            // show the file open dialog, and get the path from it
            DialogResult result = importTexSampleDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (importTexSampleDialog.FileName.EndsWith(".bmp"))
                {
                    Bitmap inputBmp = new Bitmap(importTexSampleDialog.FileName);

                    // make sure the size is correct
                    if (inputBmp.Height == 1024 && inputBmp.Width == 1024)
                    {
                        terrain.setTextureSample(inputBmp);
                    }
                    else
                    {
                        MessageBox.Show("Please select a 1024x1024 pixel bitmap");
                    }

                    texSamplePicture.Image = terrain.getTextureSample();
                    colorMapPicture.Image = terrain.getTexture();
                }
                else
                {
                    MessageBox.Show("Could not import selected map");
                }

                Console.WriteLine(importTexSampleDialog.FileName);
            }
        }

        private void addNoiseButton_Click(object sender, EventArgs e)
        {
            xOffset = (double)xOffsetNum.Value;
            yOffset = (double)yOffsetNum.Value;
            xSize = (int)xSizeNum.Value;
            ySize = (int)ySizeNum.Value;
            maxAlt = (float)maxAltNum.Value;
            frequency = (double)frequencyNum.Value;
            octaves = (int)octavesNum.Value;
            persistance = (double)persistenceNum.Value;
            lacunarity = (double)lacunarityNum.Value;
            mu = (double)muNum.Value;
            noiseWeight = (double)noiseWeightNum.Value;

            if (terrain != null)
            {
                terrain.addTerrainNoise(noiseWeight, xOffset, yOffset, frequency, octaves, persistance, lacunarity, mu);
                heightMapPicture.Image = terrain.getHeightBitmap();
                colorMapPicture.Image = terrain.getTexture();
            }
            else
            {
                MessageBox.Show("Please generate or import terrain before adding noise.");
            }
        }
    }
}
