namespace TerrainGenerator
{
    partial class TerrainGeneratorMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TerrainGeneratorMainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.quitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.heightMapPanel = new System.Windows.Forms.Panel();
            this.heightMapPicture = new System.Windows.Forms.PictureBox();
            this.generateTerrainButton = new System.Windows.Forms.Button();
            this.mapTabs = new System.Windows.Forms.TabControl();
            this.heightMapTab = new System.Windows.Forms.TabPage();
            this.colorMapTab = new System.Windows.Forms.TabPage();
            this.colorMapPanel = new System.Windows.Forms.Panel();
            this.colorMapPicture = new System.Windows.Forms.PictureBox();
            this.waterMapTab = new System.Windows.Forms.TabPage();
            this.waterMapPanel = new System.Windows.Forms.Panel();
            this.waterMapPicture = new System.Windows.Forms.PictureBox();
            this.customMap1Tab = new System.Windows.Forms.TabPage();
            this.customMap1Panel = new System.Windows.Forms.Panel();
            this.customMap1Picture = new System.Windows.Forms.PictureBox();
            this.customMap2Tab = new System.Windows.Forms.TabPage();
            this.customMap2Panel = new System.Windows.Forms.Panel();
            this.customMap2Picture = new System.Windows.Forms.PictureBox();
            this.toolTabs = new System.Windows.Forms.TabControl();
            this.generateTab = new System.Windows.Forms.TabPage();
            this.xOffsetTextBox = new System.Windows.Forms.TextBox();
            this.yOffsetLabel = new System.Windows.Forms.Label();
            this.xOffsetLabel = new System.Windows.Forms.Label();
            this.addNoiseButton = new System.Windows.Forms.Button();
            this.importMapButton = new System.Windows.Forms.Button();
            this.erosionTab = new System.Windows.Forms.TabPage();
            this.textureMapTab = new System.Windows.Forms.TabPage();
            this.importMapDialog = new System.Windows.Forms.OpenFileDialog();
            this.yOffsetTextBox = new System.Windows.Forms.TextBox();
            this.frequencyLabel = new System.Windows.Forms.Label();
            this.octaveLabel = new System.Windows.Forms.Label();
            this.lacunarityLabel = new System.Windows.Forms.Label();
            this.persistenceLabel = new System.Windows.Forms.Label();
            this.slopeDistributionLabel = new System.Windows.Forms.Label();
            this.noiseWeightLabel = new System.Windows.Forms.Label();
            this.frequencyTextBox = new System.Windows.Forms.TextBox();
            this.octavesTextBox = new System.Windows.Forms.TextBox();
            this.lacunarityTextBox = new System.Windows.Forms.TextBox();
            this.persistTextBox = new System.Windows.Forms.TextBox();
            this.muTextBox = new System.Windows.Forms.TextBox();
            this.noiseWeightTextBox = new System.Windows.Forms.TextBox();
            this.ySizeTextBox = new System.Windows.Forms.TextBox();
            this.xSizeTextBox = new System.Windows.Forms.TextBox();
            this.ySizeLabel = new System.Windows.Forms.Label();
            this.xSizeLabel = new System.Windows.Forms.Label();
            this.mapPropertiesBox = new System.Windows.Forms.GroupBox();
            this.xActualSizeLabel = new System.Windows.Forms.Label();
            this.yActualSizeTextBox = new System.Windows.Forms.TextBox();
            this.xActualSizeTextBox = new System.Windows.Forms.TextBox();
            this.yActualSizeLabel = new System.Windows.Forms.Label();
            this.maxAltLabel = new System.Windows.Forms.Label();
            this.maxAltTextBox = new System.Windows.Forms.TextBox();
            this.noisePropBox = new System.Windows.Forms.GroupBox();
            this.generalTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1.SuspendLayout();
            this.heightMapPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.heightMapPicture)).BeginInit();
            this.mapTabs.SuspendLayout();
            this.heightMapTab.SuspendLayout();
            this.colorMapTab.SuspendLayout();
            this.colorMapPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.colorMapPicture)).BeginInit();
            this.waterMapTab.SuspendLayout();
            this.waterMapPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.waterMapPicture)).BeginInit();
            this.customMap1Tab.SuspendLayout();
            this.customMap1Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.customMap1Picture)).BeginInit();
            this.customMap2Tab.SuspendLayout();
            this.customMap2Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.customMap2Picture)).BeginInit();
            this.toolTabs.SuspendLayout();
            this.generateTab.SuspendLayout();
            this.mapPropertiesBox.SuspendLayout();
            this.noisePropBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.aboutMenu});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(734, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitMenuItem});
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.fileMenu.Size = new System.Drawing.Size(37, 20);
            this.fileMenu.Text = "&File";
            this.fileMenu.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // quitMenuItem
            // 
            this.quitMenuItem.Name = "quitMenuItem";
            this.quitMenuItem.ShortcutKeyDisplayString = "";
            this.quitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.quitMenuItem.Size = new System.Drawing.Size(140, 22);
            this.quitMenuItem.Text = "&Quit";
            this.quitMenuItem.Click += new System.EventHandler(this.quitMenuItem_Click);
            // 
            // aboutMenu
            // 
            this.aboutMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutMenuItem});
            this.aboutMenu.Name = "aboutMenu";
            this.aboutMenu.Size = new System.Drawing.Size(44, 20);
            this.aboutMenu.Text = "&Help";
            // 
            // aboutMenuItem
            // 
            this.aboutMenuItem.Name = "aboutMenuItem";
            this.aboutMenuItem.Size = new System.Drawing.Size(201, 22);
            this.aboutMenuItem.Text = "About Terrain Generator";
            this.aboutMenuItem.Click += new System.EventHandler(this.aboutMenuItem_Click);
            // 
            // heightMapPanel
            // 
            this.heightMapPanel.AutoScroll = true;
            this.heightMapPanel.Controls.Add(this.heightMapPicture);
            this.heightMapPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heightMapPanel.Location = new System.Drawing.Point(0, 0);
            this.heightMapPanel.Name = "heightMapPanel";
            this.heightMapPanel.Size = new System.Drawing.Size(507, 545);
            this.heightMapPanel.TabIndex = 1;
            // 
            // heightMapPicture
            // 
            this.heightMapPicture.Location = new System.Drawing.Point(0, 0);
            this.heightMapPicture.Margin = new System.Windows.Forms.Padding(0);
            this.heightMapPicture.Name = "heightMapPicture";
            this.heightMapPicture.Size = new System.Drawing.Size(100, 50);
            this.heightMapPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.heightMapPicture.TabIndex = 3;
            this.heightMapPicture.TabStop = false;
            // 
            // generateTerrainButton
            // 
            this.generateTerrainButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.generateTerrainButton.Location = new System.Drawing.Point(37, 487);
            this.generateTerrainButton.Name = "generateTerrainButton";
            this.generateTerrainButton.Size = new System.Drawing.Size(103, 23);
            this.generateTerrainButton.TabIndex = 0;
            this.generateTerrainButton.Text = "Generate Terrain";
            this.generateTerrainButton.UseVisualStyleBackColor = true;
            this.generateTerrainButton.Click += new System.EventHandler(this.generateTerrainButton_Click);
            // 
            // mapTabs
            // 
            this.mapTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mapTabs.Controls.Add(this.heightMapTab);
            this.mapTabs.Controls.Add(this.colorMapTab);
            this.mapTabs.Controls.Add(this.waterMapTab);
            this.mapTabs.Controls.Add(this.customMap1Tab);
            this.mapTabs.Controls.Add(this.customMap2Tab);
            this.mapTabs.Location = new System.Drawing.Point(13, 28);
            this.mapTabs.Name = "mapTabs";
            this.mapTabs.SelectedIndex = 0;
            this.mapTabs.Size = new System.Drawing.Size(515, 571);
            this.mapTabs.TabIndex = 3;
            // 
            // heightMapTab
            // 
            this.heightMapTab.Controls.Add(this.heightMapPanel);
            this.heightMapTab.Location = new System.Drawing.Point(4, 22);
            this.heightMapTab.Name = "heightMapTab";
            this.heightMapTab.Size = new System.Drawing.Size(507, 545);
            this.heightMapTab.TabIndex = 0;
            this.heightMapTab.Text = "Height";
            this.heightMapTab.UseVisualStyleBackColor = true;
            // 
            // colorMapTab
            // 
            this.colorMapTab.Controls.Add(this.colorMapPanel);
            this.colorMapTab.Location = new System.Drawing.Point(4, 22);
            this.colorMapTab.Name = "colorMapTab";
            this.colorMapTab.Size = new System.Drawing.Size(507, 545);
            this.colorMapTab.TabIndex = 1;
            this.colorMapTab.Text = "Color";
            this.colorMapTab.UseVisualStyleBackColor = true;
            // 
            // colorMapPanel
            // 
            this.colorMapPanel.AutoScroll = true;
            this.colorMapPanel.Controls.Add(this.colorMapPicture);
            this.colorMapPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.colorMapPanel.Location = new System.Drawing.Point(0, 0);
            this.colorMapPanel.Name = "colorMapPanel";
            this.colorMapPanel.Size = new System.Drawing.Size(507, 545);
            this.colorMapPanel.TabIndex = 0;
            // 
            // colorMapPicture
            // 
            this.colorMapPicture.Location = new System.Drawing.Point(0, 0);
            this.colorMapPicture.Margin = new System.Windows.Forms.Padding(0);
            this.colorMapPicture.Name = "colorMapPicture";
            this.colorMapPicture.Size = new System.Drawing.Size(100, 50);
            this.colorMapPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.colorMapPicture.TabIndex = 0;
            this.colorMapPicture.TabStop = false;
            // 
            // waterMapTab
            // 
            this.waterMapTab.Controls.Add(this.waterMapPanel);
            this.waterMapTab.Location = new System.Drawing.Point(4, 22);
            this.waterMapTab.Name = "waterMapTab";
            this.waterMapTab.Size = new System.Drawing.Size(507, 545);
            this.waterMapTab.TabIndex = 2;
            this.waterMapTab.Text = "Water";
            this.waterMapTab.UseVisualStyleBackColor = true;
            // 
            // waterMapPanel
            // 
            this.waterMapPanel.AutoScroll = true;
            this.waterMapPanel.Controls.Add(this.waterMapPicture);
            this.waterMapPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.waterMapPanel.Location = new System.Drawing.Point(0, 0);
            this.waterMapPanel.Name = "waterMapPanel";
            this.waterMapPanel.Size = new System.Drawing.Size(507, 545);
            this.waterMapPanel.TabIndex = 1;
            // 
            // waterMapPicture
            // 
            this.waterMapPicture.Location = new System.Drawing.Point(0, 0);
            this.waterMapPicture.Margin = new System.Windows.Forms.Padding(0);
            this.waterMapPicture.Name = "waterMapPicture";
            this.waterMapPicture.Size = new System.Drawing.Size(100, 50);
            this.waterMapPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.waterMapPicture.TabIndex = 0;
            this.waterMapPicture.TabStop = false;
            // 
            // customMap1Tab
            // 
            this.customMap1Tab.Controls.Add(this.customMap1Panel);
            this.customMap1Tab.Location = new System.Drawing.Point(4, 22);
            this.customMap1Tab.Name = "customMap1Tab";
            this.customMap1Tab.Size = new System.Drawing.Size(507, 545);
            this.customMap1Tab.TabIndex = 3;
            this.customMap1Tab.Text = "Custom 1";
            this.customMap1Tab.UseVisualStyleBackColor = true;
            this.customMap1Tab.Click += new System.EventHandler(this.customMap1Tab_Click);
            // 
            // customMap1Panel
            // 
            this.customMap1Panel.AutoScroll = true;
            this.customMap1Panel.Controls.Add(this.customMap1Picture);
            this.customMap1Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customMap1Panel.Location = new System.Drawing.Point(0, 0);
            this.customMap1Panel.Name = "customMap1Panel";
            this.customMap1Panel.Size = new System.Drawing.Size(507, 545);
            this.customMap1Panel.TabIndex = 1;
            // 
            // customMap1Picture
            // 
            this.customMap1Picture.Location = new System.Drawing.Point(0, 0);
            this.customMap1Picture.Margin = new System.Windows.Forms.Padding(0);
            this.customMap1Picture.Name = "customMap1Picture";
            this.customMap1Picture.Size = new System.Drawing.Size(100, 50);
            this.customMap1Picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.customMap1Picture.TabIndex = 0;
            this.customMap1Picture.TabStop = false;
            // 
            // customMap2Tab
            // 
            this.customMap2Tab.Controls.Add(this.customMap2Panel);
            this.customMap2Tab.Location = new System.Drawing.Point(4, 22);
            this.customMap2Tab.Name = "customMap2Tab";
            this.customMap2Tab.Size = new System.Drawing.Size(507, 545);
            this.customMap2Tab.TabIndex = 4;
            this.customMap2Tab.Text = "Custom 2";
            this.customMap2Tab.UseVisualStyleBackColor = true;
            // 
            // customMap2Panel
            // 
            this.customMap2Panel.AutoScroll = true;
            this.customMap2Panel.Controls.Add(this.customMap2Picture);
            this.customMap2Panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customMap2Panel.Location = new System.Drawing.Point(0, 0);
            this.customMap2Panel.Name = "customMap2Panel";
            this.customMap2Panel.Size = new System.Drawing.Size(507, 545);
            this.customMap2Panel.TabIndex = 1;
            // 
            // customMap2Picture
            // 
            this.customMap2Picture.Location = new System.Drawing.Point(0, 0);
            this.customMap2Picture.Margin = new System.Windows.Forms.Padding(0);
            this.customMap2Picture.Name = "customMap2Picture";
            this.customMap2Picture.Size = new System.Drawing.Size(100, 50);
            this.customMap2Picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.customMap2Picture.TabIndex = 0;
            this.customMap2Picture.TabStop = false;
            // 
            // toolTabs
            // 
            this.toolTabs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolTabs.Controls.Add(this.generateTab);
            this.toolTabs.Controls.Add(this.erosionTab);
            this.toolTabs.Controls.Add(this.textureMapTab);
            this.toolTabs.Location = new System.Drawing.Point(534, 28);
            this.toolTabs.Multiline = true;
            this.toolTabs.Name = "toolTabs";
            this.toolTabs.SelectedIndex = 0;
            this.toolTabs.Size = new System.Drawing.Size(188, 571);
            this.toolTabs.TabIndex = 4;
            // 
            // generateTab
            // 
            this.generateTab.Controls.Add(this.noisePropBox);
            this.generateTab.Controls.Add(this.mapPropertiesBox);
            this.generateTab.Controls.Add(this.addNoiseButton);
            this.generateTab.Controls.Add(this.importMapButton);
            this.generateTab.Controls.Add(this.generateTerrainButton);
            this.generateTab.Location = new System.Drawing.Point(4, 22);
            this.generateTab.Name = "generateTab";
            this.generateTab.Padding = new System.Windows.Forms.Padding(3);
            this.generateTab.Size = new System.Drawing.Size(180, 545);
            this.generateTab.TabIndex = 0;
            this.generateTab.Text = "Generate";
            this.generateTab.UseVisualStyleBackColor = true;
            // 
            // xOffsetTextBox
            // 
            this.xOffsetTextBox.Location = new System.Drawing.Point(9, 32);
            this.xOffsetTextBox.Name = "xOffsetTextBox";
            this.xOffsetTextBox.Size = new System.Drawing.Size(56, 20);
            this.xOffsetTextBox.TabIndex = 5;
            this.xOffsetTextBox.Text = "0.0";
            this.generalTooltip.SetToolTip(this.xOffsetTextBox, "X & Y Offset act as the seed for the random noise generation.");
            this.xOffsetTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonNumeric);
            // 
            // yOffsetLabel
            // 
            this.yOffsetLabel.AutoSize = true;
            this.yOffsetLabel.Location = new System.Drawing.Point(97, 16);
            this.yOffsetLabel.Name = "yOffsetLabel";
            this.yOffsetLabel.Size = new System.Drawing.Size(45, 13);
            this.yOffsetLabel.TabIndex = 4;
            this.yOffsetLabel.Text = "Y Offset";
            this.generalTooltip.SetToolTip(this.yOffsetLabel, "X & Y Offset act as the seed for the random noise generation.");
            // 
            // xOffsetLabel
            // 
            this.xOffsetLabel.AutoSize = true;
            this.xOffsetLabel.Location = new System.Drawing.Point(6, 16);
            this.xOffsetLabel.Name = "xOffsetLabel";
            this.xOffsetLabel.Size = new System.Drawing.Size(45, 13);
            this.xOffsetLabel.TabIndex = 3;
            this.xOffsetLabel.Text = "X Offset";
            this.generalTooltip.SetToolTip(this.xOffsetLabel, "X & Y Offset act as the seed for the random noise generation.");
            // 
            // addNoiseButton
            // 
            this.addNoiseButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.addNoiseButton.Location = new System.Drawing.Point(37, 516);
            this.addNoiseButton.Name = "addNoiseButton";
            this.addNoiseButton.Size = new System.Drawing.Size(103, 23);
            this.addNoiseButton.TabIndex = 2;
            this.addNoiseButton.Text = "Add Noise";
            this.addNoiseButton.UseVisualStyleBackColor = true;
            // 
            // importMapButton
            // 
            this.importMapButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.importMapButton.Location = new System.Drawing.Point(37, 458);
            this.importMapButton.Name = "importMapButton";
            this.importMapButton.Size = new System.Drawing.Size(103, 23);
            this.importMapButton.TabIndex = 1;
            this.importMapButton.Text = "Import Map";
            this.importMapButton.UseVisualStyleBackColor = true;
            this.importMapButton.Click += new System.EventHandler(this.importMapButton_Click);
            // 
            // erosionTab
            // 
            this.erosionTab.Location = new System.Drawing.Point(4, 22);
            this.erosionTab.Name = "erosionTab";
            this.erosionTab.Padding = new System.Windows.Forms.Padding(3);
            this.erosionTab.Size = new System.Drawing.Size(180, 545);
            this.erosionTab.TabIndex = 1;
            this.erosionTab.Text = "Erosion";
            this.erosionTab.UseVisualStyleBackColor = true;
            // 
            // textureMapTab
            // 
            this.textureMapTab.Location = new System.Drawing.Point(4, 22);
            this.textureMapTab.Name = "textureMapTab";
            this.textureMapTab.Size = new System.Drawing.Size(180, 545);
            this.textureMapTab.TabIndex = 2;
            this.textureMapTab.Text = "Maps";
            this.textureMapTab.UseVisualStyleBackColor = true;
            // 
            // importMapDialog
            // 
            this.importMapDialog.Filter = "Windows Bitmap|*.bmp|TIFF Images|*.tif";
            this.importMapDialog.FilterIndex = 2;
            // 
            // yOffsetTextBox
            // 
            this.yOffsetTextBox.Location = new System.Drawing.Point(100, 32);
            this.yOffsetTextBox.Name = "yOffsetTextBox";
            this.yOffsetTextBox.Size = new System.Drawing.Size(56, 20);
            this.yOffsetTextBox.TabIndex = 6;
            this.yOffsetTextBox.Text = "0.0";
            this.generalTooltip.SetToolTip(this.yOffsetTextBox, "X & Y Offset act as the seed for the random noise generation.");
            this.yOffsetTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonNumeric);
            // 
            // frequencyLabel
            // 
            this.frequencyLabel.AutoSize = true;
            this.frequencyLabel.Location = new System.Drawing.Point(6, 68);
            this.frequencyLabel.Margin = new System.Windows.Forms.Padding(3);
            this.frequencyLabel.Name = "frequencyLabel";
            this.frequencyLabel.Size = new System.Drawing.Size(57, 13);
            this.frequencyLabel.TabIndex = 7;
            this.frequencyLabel.Text = "Frequency";
            this.generalTooltip.SetToolTip(this.frequencyLabel, resources.GetString("frequencyLabel.ToolTip"));
            // 
            // octaveLabel
            // 
            this.octaveLabel.AutoSize = true;
            this.octaveLabel.Location = new System.Drawing.Point(6, 94);
            this.octaveLabel.Margin = new System.Windows.Forms.Padding(3);
            this.octaveLabel.Name = "octaveLabel";
            this.octaveLabel.Size = new System.Drawing.Size(47, 13);
            this.octaveLabel.TabIndex = 8;
            this.octaveLabel.Text = "Octaves";
            this.generalTooltip.SetToolTip(this.octaveLabel, "Octaves are the number of noise passes to layer when generating terrain.");
            // 
            // lacunarityLabel
            // 
            this.lacunarityLabel.AutoSize = true;
            this.lacunarityLabel.Location = new System.Drawing.Point(6, 120);
            this.lacunarityLabel.Margin = new System.Windows.Forms.Padding(3);
            this.lacunarityLabel.Name = "lacunarityLabel";
            this.lacunarityLabel.Size = new System.Drawing.Size(56, 13);
            this.lacunarityLabel.TabIndex = 9;
            this.lacunarityLabel.Text = "Lacunarity";
            this.generalTooltip.SetToolTip(this.lacunarityLabel, "Lacunarity multiplies the frequency of each successive octave of noise.");
            // 
            // persistenceLabel
            // 
            this.persistenceLabel.AutoSize = true;
            this.persistenceLabel.Location = new System.Drawing.Point(6, 146);
            this.persistenceLabel.Margin = new System.Windows.Forms.Padding(3);
            this.persistenceLabel.Name = "persistenceLabel";
            this.persistenceLabel.Size = new System.Drawing.Size(62, 13);
            this.persistenceLabel.TabIndex = 10;
            this.persistenceLabel.Text = "Persistence";
            this.generalTooltip.SetToolTip(this.persistenceLabel, "Persistence multiplies the amplitude of each successive octave of noise.");
            // 
            // slopeDistributionLabel
            // 
            this.slopeDistributionLabel.AutoSize = true;
            this.slopeDistributionLabel.Location = new System.Drawing.Point(6, 172);
            this.slopeDistributionLabel.Margin = new System.Windows.Forms.Padding(3);
            this.slopeDistributionLabel.Name = "slopeDistributionLabel";
            this.slopeDistributionLabel.Size = new System.Drawing.Size(89, 13);
            this.slopeDistributionLabel.TabIndex = 11;
            this.slopeDistributionLabel.Text = "Slope Distribution";
            this.generalTooltip.SetToolTip(this.slopeDistributionLabel, "Slope distribution determines the variation in the landscape. 1.0 is an even dist" +
        "ribution, higher numbers result in more varied landscape. Values higher than ~ 1" +
        ".015 result in odd terrains.");
            // 
            // noiseWeightLabel
            // 
            this.noiseWeightLabel.AutoSize = true;
            this.noiseWeightLabel.Location = new System.Drawing.Point(6, 198);
            this.noiseWeightLabel.Margin = new System.Windows.Forms.Padding(3);
            this.noiseWeightLabel.Name = "noiseWeightLabel";
            this.noiseWeightLabel.Size = new System.Drawing.Size(71, 13);
            this.noiseWeightLabel.TabIndex = 12;
            this.noiseWeightLabel.Text = "Noise Weight";
            this.generalTooltip.SetToolTip(this.noiseWeightLabel, "Noise weight determines the amount of noise applied by the Add Noise command.");
            // 
            // frequencyTextBox
            // 
            this.frequencyTextBox.Location = new System.Drawing.Point(100, 65);
            this.frequencyTextBox.Name = "frequencyTextBox";
            this.frequencyTextBox.Size = new System.Drawing.Size(56, 20);
            this.frequencyTextBox.TabIndex = 13;
            this.frequencyTextBox.Text = "1.0";
            this.generalTooltip.SetToolTip(this.frequencyTextBox, resources.GetString("frequencyTextBox.ToolTip"));
            this.frequencyTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonNumeric);
            // 
            // octavesTextBox
            // 
            this.octavesTextBox.Location = new System.Drawing.Point(100, 91);
            this.octavesTextBox.Name = "octavesTextBox";
            this.octavesTextBox.Size = new System.Drawing.Size(56, 20);
            this.octavesTextBox.TabIndex = 14;
            this.octavesTextBox.Text = "6";
            this.generalTooltip.SetToolTip(this.octavesTextBox, "Octaves are the number of noise passes to layer when generating terrain.");
            this.octavesTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonInteger);
            // 
            // lacunarityTextBox
            // 
            this.lacunarityTextBox.Location = new System.Drawing.Point(100, 117);
            this.lacunarityTextBox.Name = "lacunarityTextBox";
            this.lacunarityTextBox.Size = new System.Drawing.Size(56, 20);
            this.lacunarityTextBox.TabIndex = 15;
            this.lacunarityTextBox.Text = "2.0";
            this.generalTooltip.SetToolTip(this.lacunarityTextBox, "Lacunarity multiplies the frequency of each successive octave of noise.");
            this.lacunarityTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonNumeric);
            // 
            // persistTextBox
            // 
            this.persistTextBox.Location = new System.Drawing.Point(100, 143);
            this.persistTextBox.Name = "persistTextBox";
            this.persistTextBox.Size = new System.Drawing.Size(56, 20);
            this.persistTextBox.TabIndex = 16;
            this.persistTextBox.Text = "0.5";
            this.generalTooltip.SetToolTip(this.persistTextBox, "Persistence multiplies the amplitude of each successive octave of noise.");
            this.persistTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonNumeric);
            // 
            // muTextBox
            // 
            this.muTextBox.Location = new System.Drawing.Point(100, 169);
            this.muTextBox.Name = "muTextBox";
            this.muTextBox.Size = new System.Drawing.Size(56, 20);
            this.muTextBox.TabIndex = 17;
            this.muTextBox.Text = "1.0";
            this.generalTooltip.SetToolTip(this.muTextBox, "Slope distribution determines the variation in the landscape. 1.0 is an even dist" +
        "ribution, higher numbers result in more varied landscape. Values higher than ~ 1" +
        ".015 result in odd terrains.");
            this.muTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonNumeric);
            // 
            // noiseWeightTextBox
            // 
            this.noiseWeightTextBox.Location = new System.Drawing.Point(100, 195);
            this.noiseWeightTextBox.Name = "noiseWeightTextBox";
            this.noiseWeightTextBox.Size = new System.Drawing.Size(56, 20);
            this.noiseWeightTextBox.TabIndex = 18;
            this.noiseWeightTextBox.Text = "0.5";
            this.generalTooltip.SetToolTip(this.noiseWeightTextBox, "Noise weight determines the amount of noise applied by the Add Noise command.");
            this.noiseWeightTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonNumeric);
            // 
            // ySizeTextBox
            // 
            this.ySizeTextBox.Location = new System.Drawing.Point(100, 32);
            this.ySizeTextBox.Name = "ySizeTextBox";
            this.ySizeTextBox.Size = new System.Drawing.Size(56, 20);
            this.ySizeTextBox.TabIndex = 22;
            this.ySizeTextBox.Text = "512";
            this.generalTooltip.SetToolTip(this.ySizeTextBox, "X & Y Resolution determine the size, in pixels, of the generated heightmap.");
            this.ySizeTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonInteger);
            // 
            // xSizeTextBox
            // 
            this.xSizeTextBox.Location = new System.Drawing.Point(9, 32);
            this.xSizeTextBox.Name = "xSizeTextBox";
            this.xSizeTextBox.Size = new System.Drawing.Size(56, 20);
            this.xSizeTextBox.TabIndex = 21;
            this.xSizeTextBox.Text = "512";
            this.generalTooltip.SetToolTip(this.xSizeTextBox, "X & Y Resolution determine the size, in pixels, of the generated heightmap.");
            this.xSizeTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonInteger);
            // 
            // ySizeLabel
            // 
            this.ySizeLabel.AutoSize = true;
            this.ySizeLabel.Location = new System.Drawing.Point(97, 16);
            this.ySizeLabel.Name = "ySizeLabel";
            this.ySizeLabel.Size = new System.Drawing.Size(67, 13);
            this.ySizeLabel.TabIndex = 20;
            this.ySizeLabel.Text = "Y Resolution";
            this.generalTooltip.SetToolTip(this.ySizeLabel, "X & Y Resolution determine the size, in pixels, of the generated heightmap.");
            // 
            // xSizeLabel
            // 
            this.xSizeLabel.AutoSize = true;
            this.xSizeLabel.Location = new System.Drawing.Point(6, 16);
            this.xSizeLabel.Name = "xSizeLabel";
            this.xSizeLabel.Size = new System.Drawing.Size(67, 13);
            this.xSizeLabel.TabIndex = 19;
            this.xSizeLabel.Text = "X Resolution";
            this.generalTooltip.SetToolTip(this.xSizeLabel, "X & Y Resolution determine the size, in pixels, of the generated heightmap.");
            // 
            // mapPropertiesBox
            // 
            this.mapPropertiesBox.Controls.Add(this.maxAltTextBox);
            this.mapPropertiesBox.Controls.Add(this.maxAltLabel);
            this.mapPropertiesBox.Controls.Add(this.xActualSizeLabel);
            this.mapPropertiesBox.Controls.Add(this.yActualSizeTextBox);
            this.mapPropertiesBox.Controls.Add(this.xActualSizeTextBox);
            this.mapPropertiesBox.Controls.Add(this.yActualSizeLabel);
            this.mapPropertiesBox.Controls.Add(this.xSizeLabel);
            this.mapPropertiesBox.Controls.Add(this.ySizeTextBox);
            this.mapPropertiesBox.Controls.Add(this.xSizeTextBox);
            this.mapPropertiesBox.Controls.Add(this.ySizeLabel);
            this.mapPropertiesBox.Location = new System.Drawing.Point(6, 6);
            this.mapPropertiesBox.Name = "mapPropertiesBox";
            this.mapPropertiesBox.Size = new System.Drawing.Size(168, 132);
            this.mapPropertiesBox.TabIndex = 23;
            this.mapPropertiesBox.TabStop = false;
            this.mapPropertiesBox.Text = "Map Properties";
            // 
            // xActualSizeLabel
            // 
            this.xActualSizeLabel.AutoSize = true;
            this.xActualSizeLabel.Location = new System.Drawing.Point(6, 55);
            this.xActualSizeLabel.Name = "xActualSizeLabel";
            this.xActualSizeLabel.Size = new System.Drawing.Size(61, 13);
            this.xActualSizeLabel.TabIndex = 23;
            this.xActualSizeLabel.Text = "X Map Size";
            this.generalTooltip.SetToolTip(this.xActualSizeLabel, "X & Y map size determine the real world size of the generated map, in meters.");
            // 
            // yActualSizeTextBox
            // 
            this.yActualSizeTextBox.Location = new System.Drawing.Point(100, 71);
            this.yActualSizeTextBox.Name = "yActualSizeTextBox";
            this.yActualSizeTextBox.Size = new System.Drawing.Size(56, 20);
            this.yActualSizeTextBox.TabIndex = 26;
            this.yActualSizeTextBox.Text = "10000";
            this.generalTooltip.SetToolTip(this.yActualSizeTextBox, "X & Y map size determine the real world size of the generated map, in meters.");
            this.yActualSizeTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonNumeric);
            // 
            // xActualSizeTextBox
            // 
            this.xActualSizeTextBox.Location = new System.Drawing.Point(9, 71);
            this.xActualSizeTextBox.Name = "xActualSizeTextBox";
            this.xActualSizeTextBox.Size = new System.Drawing.Size(56, 20);
            this.xActualSizeTextBox.TabIndex = 25;
            this.xActualSizeTextBox.Text = "10000";
            this.generalTooltip.SetToolTip(this.xActualSizeTextBox, "X & Y map size determine the real world size of the generated map, in meters.");
            this.xActualSizeTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonNumeric);
            // 
            // yActualSizeLabel
            // 
            this.yActualSizeLabel.AutoSize = true;
            this.yActualSizeLabel.Location = new System.Drawing.Point(97, 55);
            this.yActualSizeLabel.Name = "yActualSizeLabel";
            this.yActualSizeLabel.Size = new System.Drawing.Size(61, 13);
            this.yActualSizeLabel.TabIndex = 24;
            this.yActualSizeLabel.Text = "Y Map Size";
            this.generalTooltip.SetToolTip(this.yActualSizeLabel, "X & Y map size determine the real world size of the generated map, in meters.");
            // 
            // maxAltLabel
            // 
            this.maxAltLabel.AutoSize = true;
            this.maxAltLabel.Location = new System.Drawing.Point(6, 105);
            this.maxAltLabel.Name = "maxAltLabel";
            this.maxAltLabel.Size = new System.Drawing.Size(65, 13);
            this.maxAltLabel.TabIndex = 27;
            this.maxAltLabel.Text = "Max Altitude";
            this.generalTooltip.SetToolTip(this.maxAltLabel, "Max Altitude determines the maximum altitude, in meters, of the generated heightm" +
        "ap.");
            // 
            // maxAltTextBox
            // 
            this.maxAltTextBox.Location = new System.Drawing.Point(100, 102);
            this.maxAltTextBox.Name = "maxAltTextBox";
            this.maxAltTextBox.Size = new System.Drawing.Size(56, 20);
            this.maxAltTextBox.TabIndex = 28;
            this.maxAltTextBox.Text = "2500";
            this.generalTooltip.SetToolTip(this.maxAltTextBox, "Max Altitude determines the maximum altitude, in meters, of the generated heightm" +
        "ap.");
            this.maxAltTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilterNonNumeric);
            // 
            // noisePropBox
            // 
            this.noisePropBox.Controls.Add(this.xOffsetLabel);
            this.noisePropBox.Controls.Add(this.noiseWeightTextBox);
            this.noisePropBox.Controls.Add(this.persistenceLabel);
            this.noisePropBox.Controls.Add(this.yOffsetLabel);
            this.noisePropBox.Controls.Add(this.muTextBox);
            this.noisePropBox.Controls.Add(this.slopeDistributionLabel);
            this.noisePropBox.Controls.Add(this.persistTextBox);
            this.noisePropBox.Controls.Add(this.lacunarityLabel);
            this.noisePropBox.Controls.Add(this.lacunarityTextBox);
            this.noisePropBox.Controls.Add(this.noiseWeightLabel);
            this.noisePropBox.Controls.Add(this.yOffsetTextBox);
            this.noisePropBox.Controls.Add(this.octaveLabel);
            this.noisePropBox.Controls.Add(this.octavesTextBox);
            this.noisePropBox.Controls.Add(this.xOffsetTextBox);
            this.noisePropBox.Controls.Add(this.frequencyTextBox);
            this.noisePropBox.Controls.Add(this.frequencyLabel);
            this.noisePropBox.Location = new System.Drawing.Point(6, 144);
            this.noisePropBox.Name = "noisePropBox";
            this.noisePropBox.Size = new System.Drawing.Size(168, 228);
            this.noisePropBox.TabIndex = 24;
            this.noisePropBox.TabStop = false;
            this.noisePropBox.Text = "Noise Generator Properties";
            // 
            // TerrainGeneratorMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 611);
            this.Controls.Add(this.toolTabs);
            this.Controls.Add(this.mapTabs);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(750, 650);
            this.Name = "TerrainGeneratorMainForm";
            this.Text = "Procedural Terrain Generator";
            this.Load += new System.EventHandler(this.TerrainGeneratorMainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.heightMapPanel.ResumeLayout(false);
            this.heightMapPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.heightMapPicture)).EndInit();
            this.mapTabs.ResumeLayout(false);
            this.heightMapTab.ResumeLayout(false);
            this.colorMapTab.ResumeLayout(false);
            this.colorMapPanel.ResumeLayout(false);
            this.colorMapPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.colorMapPicture)).EndInit();
            this.waterMapTab.ResumeLayout(false);
            this.waterMapPanel.ResumeLayout(false);
            this.waterMapPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.waterMapPicture)).EndInit();
            this.customMap1Tab.ResumeLayout(false);
            this.customMap1Panel.ResumeLayout(false);
            this.customMap1Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.customMap1Picture)).EndInit();
            this.customMap2Tab.ResumeLayout(false);
            this.customMap2Panel.ResumeLayout(false);
            this.customMap2Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.customMap2Picture)).EndInit();
            this.toolTabs.ResumeLayout(false);
            this.generateTab.ResumeLayout(false);
            this.mapPropertiesBox.ResumeLayout(false);
            this.mapPropertiesBox.PerformLayout();
            this.noisePropBox.ResumeLayout(false);
            this.noisePropBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem quitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutMenu;
        private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
        private System.Windows.Forms.Panel heightMapPanel;
        private System.Windows.Forms.PictureBox heightMapPicture;
        private System.Windows.Forms.Button generateTerrainButton;
        private System.Windows.Forms.TabControl mapTabs;
        private System.Windows.Forms.TabPage heightMapTab;
        private System.Windows.Forms.TabPage colorMapTab;
        private System.Windows.Forms.TabControl toolTabs;
        private System.Windows.Forms.TabPage generateTab;
        private System.Windows.Forms.TabPage erosionTab;
        private System.Windows.Forms.Panel colorMapPanel;
        private System.Windows.Forms.PictureBox colorMapPicture;
        private System.Windows.Forms.TabPage textureMapTab;
        private System.Windows.Forms.TabPage waterMapTab;
        private System.Windows.Forms.TabPage customMap1Tab;
        private System.Windows.Forms.TabPage customMap2Tab;
        private System.Windows.Forms.Panel waterMapPanel;
        private System.Windows.Forms.PictureBox waterMapPicture;
        private System.Windows.Forms.Panel customMap1Panel;
        private System.Windows.Forms.PictureBox customMap1Picture;
        private System.Windows.Forms.Panel customMap2Panel;
        private System.Windows.Forms.PictureBox customMap2Picture;
        private System.Windows.Forms.Button importMapButton;
        private System.Windows.Forms.OpenFileDialog importMapDialog;
        private System.Windows.Forms.Button addNoiseButton;
        private System.Windows.Forms.TextBox xOffsetTextBox;
        private System.Windows.Forms.Label yOffsetLabel;
        private System.Windows.Forms.Label xOffsetLabel;
        private System.Windows.Forms.TextBox yOffsetTextBox;
        private System.Windows.Forms.Label noiseWeightLabel;
        private System.Windows.Forms.Label slopeDistributionLabel;
        private System.Windows.Forms.Label persistenceLabel;
        private System.Windows.Forms.Label lacunarityLabel;
        private System.Windows.Forms.Label octaveLabel;
        private System.Windows.Forms.Label frequencyLabel;
        private System.Windows.Forms.TextBox noiseWeightTextBox;
        private System.Windows.Forms.TextBox muTextBox;
        private System.Windows.Forms.TextBox persistTextBox;
        private System.Windows.Forms.TextBox lacunarityTextBox;
        private System.Windows.Forms.TextBox octavesTextBox;
        private System.Windows.Forms.TextBox frequencyTextBox;
        private System.Windows.Forms.TextBox ySizeTextBox;
        private System.Windows.Forms.TextBox xSizeTextBox;
        private System.Windows.Forms.Label ySizeLabel;
        private System.Windows.Forms.Label xSizeLabel;
        private System.Windows.Forms.GroupBox mapPropertiesBox;
        private System.Windows.Forms.Label xActualSizeLabel;
        private System.Windows.Forms.TextBox yActualSizeTextBox;
        private System.Windows.Forms.TextBox xActualSizeTextBox;
        private System.Windows.Forms.Label yActualSizeLabel;
        private System.Windows.Forms.TextBox maxAltTextBox;
        private System.Windows.Forms.Label maxAltLabel;
        private System.Windows.Forms.GroupBox noisePropBox;
        private System.Windows.Forms.ToolTip generalTooltip;
    }
}