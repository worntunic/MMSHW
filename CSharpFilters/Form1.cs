using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using HWFilters;
using System.IO;
using HWFilters.Compression;

namespace CSharpFilters
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Drawing.Bitmap m_Bitmap;
		private System.Drawing.Bitmap m_Undo;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem FileLoad;
		private System.Windows.Forms.MenuItem FileSave;
		private System.Windows.Forms.MenuItem FileExit;
		private System.Windows.Forms.MenuItem FilterInvert;
		private System.Windows.Forms.MenuItem FilterGrayScale;
		private System.Windows.Forms.MenuItem FilterBrightness;
		private System.Windows.Forms.MenuItem FilterContrast;
		private System.Windows.Forms.MenuItem FilterGamma;
        private System.Windows.Forms.MenuItem FilterColor;
        private double Zoom = 1.0;
		private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem Undo;
		private MenuItem menuItem15;
        private MenuItem menuItem16;
        private MenuItem menuItem2;
        private MenuItem HW_Grayscale;
        private MenuItem HW_Gamma;
        private MenuItem HW_GSGamma;
        private MenuItem HW_Neighbourhood;
        private MenuItem HW_PixelChunk;
        private MenuItem HW_PixelChunk_NoGradient;
        private MenuItem HW_PixelChunk_DiagonalGradient;
        private MenuItem HW_PixelChunk_RadialGradient;
        private MenuItem HW_PixelChunk_ImageRadialGradient;
        private MenuItem menuItem3;
        private MenuItem HW_MeanRemoval;
        private MenuItem HW_Sphere;
        private MenuItem HW_SphereOffCenter;
        private MenuItem HW_MeanThenSphere;
        private MenuItem HW_MeanThenSphereOffCenter;
        private MenuItem menuItem6;
        private MenuItem HW_Fitmap_Downsample;
        private MenuItem HW_Fitmap_Load;
        private MenuItem HW_Fitmap_FullHuffman;
        private MenuItem HW_Fitmap_LoadFullHuffman;
        private MenuItem HW_Fitmap_ChannelHuffman;
        private MenuItem HW_Fitmap_LoadChannelHuffman;
        private MenuItem menuItem7;
        private MenuItem HW_Dither_BNW;
        private MenuItem HW_Dither_Gameboy;
        private MenuItem HW_Dither_C64;
        private IContainer components;

		public Form1()
		{
			InitializeComponent();

			m_Bitmap= new Bitmap(2, 2);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		        #region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.FileLoad = new System.Windows.Forms.MenuItem();
            this.FileSave = new System.Windows.Forms.MenuItem();
            this.FileExit = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.Undo = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.FilterInvert = new System.Windows.Forms.MenuItem();
            this.FilterGrayScale = new System.Windows.Forms.MenuItem();
            this.FilterBrightness = new System.Windows.Forms.MenuItem();
            this.FilterContrast = new System.Windows.Forms.MenuItem();
            this.FilterGamma = new System.Windows.Forms.MenuItem();
            this.FilterColor = new System.Windows.Forms.MenuItem();
            this.menuItem15 = new System.Windows.Forms.MenuItem();
            this.menuItem16 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.HW_Grayscale = new System.Windows.Forms.MenuItem();
            this.HW_Gamma = new System.Windows.Forms.MenuItem();
            this.HW_GSGamma = new System.Windows.Forms.MenuItem();
            this.HW_Neighbourhood = new System.Windows.Forms.MenuItem();
            this.HW_PixelChunk = new System.Windows.Forms.MenuItem();
            this.HW_PixelChunk_NoGradient = new System.Windows.Forms.MenuItem();
            this.HW_PixelChunk_DiagonalGradient = new System.Windows.Forms.MenuItem();
            this.HW_PixelChunk_RadialGradient = new System.Windows.Forms.MenuItem();
            this.HW_PixelChunk_ImageRadialGradient = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.HW_MeanRemoval = new System.Windows.Forms.MenuItem();
            this.HW_Sphere = new System.Windows.Forms.MenuItem();
            this.HW_SphereOffCenter = new System.Windows.Forms.MenuItem();
            this.HW_MeanThenSphere = new System.Windows.Forms.MenuItem();
            this.HW_MeanThenSphereOffCenter = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.HW_Fitmap_Downsample = new System.Windows.Forms.MenuItem();
            this.HW_Fitmap_Load = new System.Windows.Forms.MenuItem();
            this.HW_Fitmap_FullHuffman = new System.Windows.Forms.MenuItem();
            this.HW_Fitmap_LoadFullHuffman = new System.Windows.Forms.MenuItem();
            this.HW_Fitmap_ChannelHuffman = new System.Windows.Forms.MenuItem();
            this.HW_Fitmap_LoadChannelHuffman = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.HW_Dither_BNW = new System.Windows.Forms.MenuItem();
            this.HW_Dither_Gameboy = new System.Windows.Forms.MenuItem();
            this.HW_Dither_C64 = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem5,
            this.menuItem4,
            this.menuItem2,
            this.menuItem3,
            this.menuItem6,
            this.menuItem7});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.FileLoad,
            this.FileSave,
            this.FileExit});
            this.menuItem1.Text = "File";
            // 
            // FileLoad
            // 
            this.FileLoad.Index = 0;
            this.FileLoad.Shortcut = System.Windows.Forms.Shortcut.CtrlL;
            this.FileLoad.Text = "Load";
            this.FileLoad.Click += new System.EventHandler(this.File_Load);
            // 
            // FileSave
            // 
            this.FileSave.Index = 1;
            this.FileSave.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.FileSave.Text = "Save";
            this.FileSave.Click += new System.EventHandler(this.File_Save);
            // 
            // FileExit
            // 
            this.FileExit.Index = 2;
            this.FileExit.Text = "Exit";
            this.FileExit.Click += new System.EventHandler(this.File_Exit);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 1;
            this.menuItem5.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.Undo});
            this.menuItem5.Text = "Edit";
            // 
            // Undo
            // 
            this.Undo.Index = 0;
            this.Undo.Text = "Undo";
            this.Undo.Click += new System.EventHandler(this.OnUndo);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 2;
            this.menuItem4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.FilterInvert,
            this.FilterGrayScale,
            this.FilterBrightness,
            this.FilterContrast,
            this.FilterGamma,
            this.FilterColor,
            this.menuItem15,
            this.menuItem16});
            this.menuItem4.Text = "Filter";
            // 
            // FilterInvert
            // 
            this.FilterInvert.Index = 0;
            this.FilterInvert.Text = "Invert";
            this.FilterInvert.Click += new System.EventHandler(this.Filter_Invert);
            // 
            // FilterGrayScale
            // 
            this.FilterGrayScale.Index = 1;
            this.FilterGrayScale.Text = "GrayScale";
            this.FilterGrayScale.Click += new System.EventHandler(this.Filter_GrayScale);
            // 
            // FilterBrightness
            // 
            this.FilterBrightness.Index = 2;
            this.FilterBrightness.Text = "Brightness";
            this.FilterBrightness.Click += new System.EventHandler(this.Filter_Brightness);
            // 
            // FilterContrast
            // 
            this.FilterContrast.Index = 3;
            this.FilterContrast.Text = "Contrast";
            this.FilterContrast.Click += new System.EventHandler(this.Filter_Contrast);
            // 
            // FilterGamma
            // 
            this.FilterGamma.Index = 4;
            this.FilterGamma.Text = "Gamma";
            this.FilterGamma.Click += new System.EventHandler(this.Filter_Gamma);
            // 
            // FilterColor
            // 
            this.FilterColor.Index = 5;
            this.FilterColor.Text = "Color";
            this.FilterColor.Click += new System.EventHandler(this.Filter_Color);
            // 
            // menuItem15
            // 
            this.menuItem15.Index = 6;
            this.menuItem15.Text = "InvertUnsafe";
            this.menuItem15.Click += new System.EventHandler(this.menuItem15_Click);
            // 
            // menuItem16
            // 
            this.menuItem16.Index = 7;
            this.menuItem16.Text = "PaintY";
            this.menuItem16.Click += new System.EventHandler(this.menuItem16_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 3;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.HW_Grayscale,
            this.HW_Gamma,
            this.HW_GSGamma,
            this.HW_Neighbourhood,
            this.HW_PixelChunk});
            this.menuItem2.Text = "Homework";
            // 
            // HW_Grayscale
            // 
            this.HW_Grayscale.Index = 0;
            this.HW_Grayscale.Text = "Grayscale";
            this.HW_Grayscale.Click += new System.EventHandler(this.HW_Filter_Grayscale);
            // 
            // HW_Gamma
            // 
            this.HW_Gamma.Index = 1;
            this.HW_Gamma.Text = "Gamma";
            this.HW_Gamma.Click += new System.EventHandler(this.HW_Filter_Gamma);
            // 
            // HW_GSGamma
            // 
            this.HW_GSGamma.Index = 2;
            this.HW_GSGamma.Text = "GSGamma";
            this.HW_GSGamma.Click += new System.EventHandler(this.HW_Filter_GSGamma);
            // 
            // HW_Neighbourhood
            // 
            this.HW_Neighbourhood.Index = 3;
            this.HW_Neighbourhood.Text = "Neighbourhood";
            this.HW_Neighbourhood.Click += new System.EventHandler(this.HW_Filter_Neighbourhood);
            // 
            // HW_PixelChunk
            // 
            this.HW_PixelChunk.Index = 4;
            this.HW_PixelChunk.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.HW_PixelChunk_NoGradient,
            this.HW_PixelChunk_DiagonalGradient,
            this.HW_PixelChunk_RadialGradient,
            this.HW_PixelChunk_ImageRadialGradient});
            this.HW_PixelChunk.Text = "PixelChunk";
            // 
            // HW_PixelChunk_NoGradient
            // 
            this.HW_PixelChunk_NoGradient.Index = 0;
            this.HW_PixelChunk_NoGradient.Text = "No Gradient";
            this.HW_PixelChunk_NoGradient.Click += new System.EventHandler(this.HW_Filter_PixelChunk_NoGradient);
            // 
            // HW_PixelChunk_DiagonalGradient
            // 
            this.HW_PixelChunk_DiagonalGradient.Index = 1;
            this.HW_PixelChunk_DiagonalGradient.Text = "Diagonal Gradient";
            this.HW_PixelChunk_DiagonalGradient.Click += new System.EventHandler(this.HW_Filter_PixelChunk_DiagonalGradient);
            // 
            // HW_PixelChunk_RadialGradient
            // 
            this.HW_PixelChunk_RadialGradient.Index = 2;
            this.HW_PixelChunk_RadialGradient.Text = "Radial Gradient";
            this.HW_PixelChunk_RadialGradient.Click += new System.EventHandler(this.HW_Filter_PixelChunk_RadialGradient);
            // 
            // HW_PixelChunk_ImageRadialGradient
            // 
            this.HW_PixelChunk_ImageRadialGradient.Index = 3;
            this.HW_PixelChunk_ImageRadialGradient.Text = "Image Radial Gradient";
            this.HW_PixelChunk_ImageRadialGradient.Click += new System.EventHandler(this.HW_Filter_PixelChunk_ImageRadialGradient);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 4;
            this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.HW_MeanRemoval,
            this.HW_Sphere,
            this.HW_SphereOffCenter,
            this.HW_MeanThenSphere,
            this.HW_MeanThenSphereOffCenter});
            this.menuItem3.Text = "HW2";
            // 
            // HW_MeanRemoval
            // 
            this.HW_MeanRemoval.Index = 0;
            this.HW_MeanRemoval.Text = "Mean Removal";
            this.HW_MeanRemoval.Click += new System.EventHandler(this.HW_Filter_MeanRemoval);
            // 
            // HW_Sphere
            // 
            this.HW_Sphere.Index = 1;
            this.HW_Sphere.Text = "Sphere";
            this.HW_Sphere.Click += new System.EventHandler(this.HW_Filter_Sphere);
            // 
            // HW_SphereOffCenter
            // 
            this.HW_SphereOffCenter.Index = 2;
            this.HW_SphereOffCenter.Text = "SphereOffCenter";
            this.HW_SphereOffCenter.Click += new System.EventHandler(this.HW_Filter_SphereOffCenter);
            // 
            // HW_MeanThenSphere
            // 
            this.HW_MeanThenSphere.Index = 3;
            this.HW_MeanThenSphere.Text = "MeanThenSphere";
            this.HW_MeanThenSphere.Click += new System.EventHandler(this.HW_Filter_MeanThenSphere);
            // 
            // HW_MeanThenSphereOffCenter
            // 
            this.HW_MeanThenSphereOffCenter.Index = 4;
            this.HW_MeanThenSphereOffCenter.Text = "MeanThenSphereOffCenter";
            this.HW_MeanThenSphereOffCenter.Click += new System.EventHandler(this.HW_Filter_MeanThenSphereOffCenter);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 5;
            this.menuItem6.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.HW_Fitmap_Downsample,
            this.HW_Fitmap_Load,
            this.HW_Fitmap_FullHuffman,
            this.HW_Fitmap_LoadFullHuffman,
            this.HW_Fitmap_ChannelHuffman,
            this.HW_Fitmap_LoadChannelHuffman});
            this.menuItem6.Text = "Fitmap";
            // 
            // HW_Fitmap_Downsample
            // 
            this.HW_Fitmap_Downsample.Index = 0;
            this.HW_Fitmap_Downsample.Text = "Downsample";
            this.HW_Fitmap_Downsample.Click += new System.EventHandler(this.HW_Filter_Fitmap_Downsample);
            // 
            // HW_Fitmap_Load
            // 
            this.HW_Fitmap_Load.Index = 1;
            this.HW_Fitmap_Load.Text = "Load";
            this.HW_Fitmap_Load.Click += new System.EventHandler(this.HW_Filter_Fitmap_Load);
            // 
            // HW_Fitmap_FullHuffman
            // 
            this.HW_Fitmap_FullHuffman.Index = 2;
            this.HW_Fitmap_FullHuffman.Text = "FullHuffman";
            this.HW_Fitmap_FullHuffman.Click += new System.EventHandler(this.HW_Filter_Fitmap_FullHuffman);
            // 
            // HW_Fitmap_LoadFullHuffman
            // 
            this.HW_Fitmap_LoadFullHuffman.Index = 3;
            this.HW_Fitmap_LoadFullHuffman.Text = "LoadFullHuffman";
            this.HW_Fitmap_LoadFullHuffman.Click += new System.EventHandler(this.HW_Filter_Fitmap_LoadFullHuffman);
            // 
            // HW_Fitmap_ChannelHuffman
            // 
            this.HW_Fitmap_ChannelHuffman.Index = 4;
            this.HW_Fitmap_ChannelHuffman.Text = "ChannelHuffman";
            this.HW_Fitmap_ChannelHuffman.Click += new System.EventHandler(this.HW_Filter_Fitmap_ChannelHuffman);
            // 
            // HW_Fitmap_LoadChannelHuffman
            // 
            this.HW_Fitmap_LoadChannelHuffman.Index = 5;
            this.HW_Fitmap_LoadChannelHuffman.Text = "LoadChannelHuffman";
            this.HW_Fitmap_LoadChannelHuffman.Click += new System.EventHandler(this.HW_Filter_Fitmap_LoadChannelHuffman);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 6;
            this.menuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.HW_Dither_BNW,
            this.HW_Dither_Gameboy,
            this.HW_Dither_C64});
            this.menuItem7.Text = "Dither";
            // 
            // HW_Dither_BNW
            // 
            this.HW_Dither_BNW.Index = 0;
            this.HW_Dither_BNW.Text = "BNW";
            this.HW_Dither_BNW.Click += new System.EventHandler(this.HW_Filter_Dither_BNW);
            // 
            // HW_Dither_Gameboy
            // 
            this.HW_Dither_Gameboy.Index = 1;
            this.HW_Dither_Gameboy.Text = "Gameboy";
            this.HW_Dither_Gameboy.Click += new System.EventHandler(this.HW_Filter_Dither_Gameboy);
            // 
            // HW_Dither_C64
            // 
            this.HW_Dither_C64.Index = 2;
            this.HW_Dither_C64.Text = "C64";
            this.HW_Dither_C64.Click += new System.EventHandler(this.HW_Filter_Dither_C64);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(616, 347);
            this.MaximumSize = new System.Drawing.Size(1920, 1080);
            this.Menu = this.mainMenu1;
            this.Name = "Form1";
            this.Text = "Image Filters ";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
            byte[] bytes = new byte[]
            {
                255, 0, 20, 20, 13, 14, 18, 255, 0, 0, 0, 0, 1, 2, 1, 255, 255, 255, 255, 255, 255
            };
            HuffmanCode<byte> hCode = new HuffmanCode<byte>(bytes);

            Application.Run(new Form1());
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			g.DrawImage(m_Bitmap, new Rectangle(this.AutoScrollPosition.X, this.AutoScrollPosition.Y, (int)(m_Bitmap.Width*Zoom), (int)(m_Bitmap.Height * Zoom)));
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
		}

		private void File_Load(object sender, System.EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();

			openFileDialog.InitialDirectory = "c:\\" ;
			openFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp|Jpeg files (*.jpg)|*.jpg|GIF files(*.gif)|*.gif|PNG files(*.png)|*.png|All valid files|*.bmp/*.jpg/*.gif/*.png";
			openFileDialog.FilterIndex = 2 ;
			openFileDialog.RestoreDirectory = true ;

			if(DialogResult.OK == openFileDialog.ShowDialog())
			{
				m_Bitmap = (Bitmap)Bitmap.FromFile(openFileDialog.FileName, false);
                if (m_Bitmap.Size.Width > this.MaximumSize.Width || m_Bitmap.Size.Height > this.MaximumSize.Height)
                {
                    this.AutoScroll = true;
                    this.AutoScrollMinSize = new Size((int)(m_Bitmap.Width * Zoom), (int)(m_Bitmap.Height * Zoom));
                } else
                {
                    this.Size = m_Bitmap.Size;
                }

				this.Invalidate();
			}
		}

		private void File_Save(object sender, System.EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();

			saveFileDialog.InitialDirectory = "c:\\" ;
			saveFileDialog.Filter = "Bitmap files (*.bmp)|*.bmp|Jpeg files (*.jpg)|*.jpg|All valid files (*.bmp/*.jpg)|*.bmp/*.jpg" ;
			saveFileDialog.FilterIndex = 1 ;
			saveFileDialog.RestoreDirectory = true ;

			if(DialogResult.OK == saveFileDialog.ShowDialog())
			{
				m_Bitmap.Save(saveFileDialog.FileName);
			}
		}

		private void File_Exit(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void Filter_Invert(object sender, System.EventArgs e)
		{
            //m_Undo = m_Bitmap.Clone() as Bitmap;
            //if (m_Undo == null)
            //    return;
			m_Undo = (Bitmap)m_Bitmap.Clone();

			if(BitmapFilter.Invert(m_Bitmap))
				this.Invalidate();
		}
				

		delegate bool ImageProcessingMethod(Bitmap b);

		private void ProcessImage(ImageProcessingMethod methodToExecute, Bitmap b)
		{
			m_Undo = (Bitmap)m_Bitmap.Clone();
			if (methodToExecute(b))
				this.Invalidate();
		}

		private void menuItem15_Click(object sender, EventArgs e)
		{
			ProcessImage(BitmapFilter.InvertH, m_Bitmap);
		}

		private void Filter_GrayScale(object sender, System.EventArgs e)
		{
			ColorInput dlg = new ColorInput();
			dlg.red = dlg.green = dlg.blue = 0;

			if (DialogResult.OK == dlg.ShowDialog())
			{
				m_Undo = (Bitmap)m_Bitmap.Clone();
				if(BitmapFilter.GrayScale(m_Bitmap, dlg.red, dlg.green, dlg.blue))
					this.Invalidate();
			}
			
		}

		private void Filter_Brightness(object sender, System.EventArgs e)
		{
			Parameter dlg = new Parameter();
			dlg.nValue = 0;

			if (DialogResult.OK == dlg.ShowDialog())
			{
				m_Undo = (Bitmap)m_Bitmap.Clone();
				if(BitmapFilter.Brightness(m_Bitmap, dlg.nValue))
					this.Invalidate();
			}
		}

		private void Filter_Contrast(object sender, System.EventArgs e)
		{
			Parameter dlg = new Parameter();
			dlg.nValue = 0;

			if (DialogResult.OK == dlg.ShowDialog())
			{
				m_Undo = (Bitmap)m_Bitmap.Clone();
				if(BitmapFilter.Contrast(m_Bitmap, (sbyte)dlg.nValue))
					this.Invalidate();
			}
		
		}

		private void Filter_Gamma(object sender, System.EventArgs e)
		{
			GammaInput dlg = new GammaInput();
			dlg.red = dlg.green = dlg.blue = 1;

			if (DialogResult.OK == dlg.ShowDialog())
			{
				m_Undo = (Bitmap)m_Bitmap.Clone();
				if(BitmapFilter.Gamma(m_Bitmap, dlg.red, dlg.green, dlg.blue))
					this.Invalidate();
			}
		}

		private void Filter_Color(object sender, System.EventArgs e)
		{
			ColorInput dlg = new ColorInput();
			dlg.red = dlg.green = dlg.blue = 0;

			if (DialogResult.OK == dlg.ShowDialog())
			{
				m_Undo = (Bitmap)m_Bitmap.Clone();
				if(BitmapFilter.Color(m_Bitmap, dlg.red, dlg.green, dlg.blue))
					this.Invalidate();
			}
		
		}
        

		private void OnUndo(object sender, System.EventArgs e)
		{
			Bitmap temp = (Bitmap)m_Bitmap.Clone();
			m_Bitmap = (Bitmap)m_Undo.Clone();
			m_Undo = (Bitmap)temp.Clone();
			this.Invalidate();
		}
				

        private void menuItem16_Click(object sender, EventArgs e)
        {
            m_Undo = (Bitmap)m_Bitmap.Clone();
            BitmapFilter.PrintYBitmapUnsafe(m_Bitmap);
                this.Invalidate();	
        }

        private void HW_Filter_Grayscale(object sender, EventArgs e)
        {
            Console.WriteLine("Grayscale");
            if (HWFilter.FilterGrayscale(m_Bitmap))
            {
                this.Invalidate();
            }
        }

        private void HW_Filter_Gamma(object sender, EventArgs e)
        {
            double[] gamma = GetGammaInput();
            Console.WriteLine($"Gamma ({gamma[0]},{gamma[1]},{gamma[2]})");
            HWFilter.FilterGamma(m_Bitmap, gamma);
        }

        private void HW_Filter_GSGamma(object sender, EventArgs e)
        {
            double[] gamma = GetGammaInput();
            Console.WriteLine($"GSGamma ({gamma[0]},{gamma[1]},{gamma[2]})");
            GammaConfig gammaConfig = new GammaConfig(gamma);
            HWFilter.FilterGSGamma(m_Bitmap, gammaConfig);
        }

        private void HW_Filter_Neighbourhood(object sender, EventArgs e)
        {
            int neighbourWidth = GetIntInput();
            Console.WriteLine($"NeighbourHood ({neighbourWidth})");
            NeighbourMergeFilter.ApplyNeighbourhoodFilter(m_Bitmap, neighbourWidth);
        }
        private void HW_Filter_PixelChunk_NoGradient(object sender, EventArgs e)
        {
            int chunkWidth = GetIntInput();
            int chunkHeight = GetIntInput();
            Console.WriteLine($"PixelChunk_NoGradient ({chunkWidth}:{chunkHeight})");
            PixelateChunkFilter.ApplyPixelChunkFilter(m_Bitmap, chunkWidth, chunkHeight, PixelateChunkFilter.GradientType.No);
        }
        private void HW_Filter_PixelChunk_DiagonalGradient(object sender, EventArgs e)
        {
            int chunkWidth = GetIntInput();
            int chunkHeight = GetIntInput();
            Console.WriteLine($"PixelChunk_DiagonalGradient ({chunkWidth}:{chunkHeight})");
            PixelateChunkFilter.ApplyPixelChunkFilter(m_Bitmap, chunkWidth, chunkHeight, PixelateChunkFilter.GradientType.Diagonal);
        }
        private void HW_Filter_PixelChunk_RadialGradient(object sender, EventArgs e)
        {
            int chunkWidth = GetIntInput();
            int chunkHeight = GetIntInput();
            Console.WriteLine($"PixelChunk_RadialGradient ({chunkWidth}:{chunkHeight})");
            PixelateChunkFilter.ApplyPixelChunkFilter(m_Bitmap, chunkWidth, chunkHeight, PixelateChunkFilter.GradientType.RadialChunk);
        }
        private void HW_Filter_PixelChunk_ImageRadialGradient(object sender, EventArgs e)
        {
            int chunkWidth = GetIntInput();
            int chunkHeight = GetIntInput();
            Console.WriteLine($"PixelChunk_ImageRadialGradient ({chunkWidth}:{chunkHeight})");
            PixelateChunkFilter.ApplyPixelChunkFilter(m_Bitmap, chunkWidth, chunkHeight, PixelateChunkFilter.GradientType.RadialImage);
        }

        private void HW_Filter_MeanRemoval(object sender, EventArgs e)
        {
            int nWeight = GetIntInput();
            Console.WriteLine($"MeanRemoval({nWeight})");
            m_Bitmap = StockFilters.MeanRemoval(m_Bitmap, nWeight);
            this.Invalidate();
        }
        private void HW_Filter_Sphere(object sender, EventArgs e)
        {
            Console.WriteLine($"Sphere");
            m_Bitmap = StockFilters.Sphere(m_Bitmap);
            this.Invalidate();
        }
        private void HW_Filter_SphereOffCenter(object sender, EventArgs e)
        {
            int xMid = GetIntInput();
            int yMid = GetIntInput();
            Console.WriteLine($"SphereOffCenter");
            PixelPoint midPoint = new PixelPoint(xMid, yMid);
            m_Bitmap = StockFilters.Sphere(m_Bitmap, midPoint);
            this.Invalidate();
        }
        private void HW_Filter_MeanThenSphere(object sender, EventArgs e)
        {
            int nWeight = GetIntInput();
            Console.WriteLine($"MeanThenSphere");
            m_Bitmap = StockFilters.MeanRemovalThenSphere(m_Bitmap, nWeight);
            this.Invalidate();
        }
        private void HW_Filter_MeanThenSphereOffCenter(object sender, EventArgs e)
        {
            int nWeight = GetIntInput();
            int xMid = GetIntInput();
            int yMid = GetIntInput();
            PixelPoint midPoint = new PixelPoint(xMid, yMid);
            Console.WriteLine($"MeanThenSphereOffCenter{nWeight}:({xMid},{yMid})");
            MeanSphereConfig msConfig = new MeanSphereConfig(nWeight, PaddingType.Full, midPoint);
            m_Bitmap = StockFilters.MeanRemovalThenSphere(m_Bitmap, msConfig);
            this.Invalidate();
        }


        private void HW_Filter_Fitmap_Downsample(object sender, EventArgs e)
        {
            Console.WriteLine($"Fitmap downsample");
            Fitmap f = Fitmap.SaveBitmap(m_Bitmap, "D:/file.fmp");
            m_Bitmap = f.GetBitmap();
            m_Bitmap.Save("D:/bmpfmp1NoComp.bmp", ImageFormat.Bmp);
            this.Invalidate();
        }

        private void HW_Filter_Fitmap_Load(object sender, EventArgs e)
        {
            Console.WriteLine($"Fitmap Load");
            m_Bitmap = Fitmap.LoadBitmapFromFile("D:/file.fmp");
            this.Invalidate();
        }

        private void HW_Filter_Fitmap_FullHuffman(object sender, EventArgs e)
        {
            Console.WriteLine($"Fitmap Full Huffman");
            Fitmap f = new Fitmap(m_Bitmap);
            f.SetCompressionAlg(FitmapCompression.CompressionType.HuffmanFull);
            f.SaveToFile("D:/file.fmp1");
            m_Bitmap = f.GetBitmap();
            this.Invalidate();
        }

        private void HW_Filter_Fitmap_LoadFullHuffman(object sender, EventArgs e)
        {
            Console.WriteLine($"Fitmap Full Huffman Load");
            m_Bitmap = Fitmap.LoadBitmapFromFile("D:/file.fmp1");
            this.Invalidate();
        }

        private void HW_Filter_Fitmap_ChannelHuffman(object sender, EventArgs e)
        {
            Console.WriteLine($"Fitmap Channel Huffman");
            Fitmap f = new Fitmap(m_Bitmap);
            f.SetCompressionAlg(FitmapCompression.CompressionType.HuffmanPerChannel);
            f.SaveToFile("D:/file.fmp2");
            m_Bitmap = f.GetBitmap();
            this.Invalidate();
        }

        private void HW_Filter_Fitmap_LoadChannelHuffman(object sender, EventArgs e)
        {
            Console.WriteLine($"Load Fitmap Channel Huffman");
            m_Bitmap = Fitmap.LoadBitmapFromFile("D:/file.fmp2");
            this.Invalidate();
        }


        private void HW_Filter_Dither_BNW(object sender, EventArgs e)
        {
            Console.WriteLine($"Atkinson BNW Dither");
            m_Bitmap = HWFilters.StockDithers.ApplyBNWBillAtkinson(m_Bitmap);
            this.Invalidate();
        }
        private void HW_Filter_Dither_Gameboy(object sender, EventArgs e)
        {
            Console.WriteLine($"Atkinson Gameboy Dither");
            m_Bitmap = HWFilters.StockDithers.ApplyGameboyBillAtkinson(m_Bitmap);
            this.Invalidate();
        }
        private void HW_Filter_Dither_C64(object sender, EventArgs e)
        {
            Console.WriteLine($"Atkinson C64 Dither");
            m_Bitmap = HWFilters.StockDithers.ApplyC64BillAtkinson(m_Bitmap);
            this.Invalidate();
        }

        private double[] GetGammaInput()
        {
            GammaInput dlg = new GammaInput();
            dlg.red = dlg.green = dlg.blue = 1;
            double[] gamma = new double[3];
            if (DialogResult.OK == dlg.ShowDialog())
            {
                gamma[0] = dlg.red;
                gamma[1] = dlg.green;
                gamma[2] = dlg.blue;
                this.Invalidate();
            }
            return gamma;
        }

        private int GetIntInput()
        {
            Parameter dlg = new Parameter();
            dlg.nValue = 0;
            int retVal = 0;
            if (DialogResult.OK == dlg.ShowDialog())
            {
                retVal = dlg.nValue;
                this.Invalidate();
            }
            return retVal;
        }


    }
}

