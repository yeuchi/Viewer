// ============================================================================
// Module:		Form1
//
// Description:	Base window class
//
// Purpose:		An image processing application
//
// Input:		A raster image
// Output:		A raster image
//
// Author:		Chi Toung Yeung			cty
//
// History:		
// 10Jan07		Added comments											cty
// 11Jan07		Added Action Crop										cty
// 11Jan07		Add Channel histogram									cty
// 18Jan07		Add Cummulative histogram								cty
// 14Feb07		Add Convolution with an array of kernel types			cty
// 14Feb07		Add Unsharpmask											cty
// 18Feb07		Fixed save as(), added switch statement 
//				for file format save type								cty
// 18Feb07		Added location setting for forms						cty
// 27Feb07		Split Channels()										cty
// 28Feb07		Convert2Gray()											cty
// 01Mar07		Histogram, CumHistogram, VolHistogram, Convolution,
//				Unsharpmask, all need 24bpp images for now				cty
// 26Oct07		A work around for Image.FromFile()	see ImageView		cty
// ============================================================================

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Configuration;

namespace Viewer
{
	enum Flip:int{Vertical, Horizontal};
	enum FileType:int{OFFSET, JPG, PNG, TIF, BMP, GIF};

	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		ArrayList	m_list;		// all form2 currently opened
		const int PAD = 20;

		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem_Open;
		private System.Windows.Forms.MenuItem menuItem_Save;
		private System.Windows.Forms.MenuItem menuItem_SaveAs;
		private System.Windows.Forms.MenuItem menuItem_Close;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem_Rotate_Right;
		private System.Windows.Forms.MenuItem menuItem_Rotate_180;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem_Flip_Vertical;
		private System.Windows.Forms.MenuItem menuItem_Flip_Horizontal;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem_Rotate_Left;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem_Crop;
		private System.Windows.Forms.MenuItem menuItem_Copy;
		private System.Windows.Forms.MenuItem menuItem_ChannelHistogram;
		private System.Windows.Forms.MenuItem menuItem_CummulativeHistogram;
		private System.Windows.Forms.MenuItem menuItem_VolumeHistogram;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuItem_Convolve_Custom;
		private System.Windows.Forms.MenuItem menuItem_Convolve_Highpass;
		private System.Windows.Forms.MenuItem menuItem_Convolve_Lowpass;
		private System.Windows.Forms.MenuItem menuItem_UnSharpMask;
		private System.Windows.Forms.MenuItem menuItem_Convolve_SobelX;
		private System.Windows.Forms.MenuItem menuItem_Convolve_SobelY;
		private System.Windows.Forms.MenuItem menuItem_Convolve_Roberts45;
		private System.Windows.Forms.MenuItem menuItem_Convolve_Roberts135;
		private System.Windows.Forms.MenuItem menuItem_Convolve_PrewittX;
		private System.Windows.Forms.MenuItem menuItem_Convolve_PrewittY;
		private System.Windows.Forms.MenuItem menuItem_Resize;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.MenuItem menuItem_Color_Split_RGB;
		private System.Windows.Forms.MenuItem menuItem_Color_Convert2Gray;
		private System.Windows.Forms.MenuItem menuItem_Color_Depth_4bpp;
		private System.Windows.Forms.MenuItem menuItem_Color_Depth_8bpp;
		private System.Windows.Forms.MenuItem menuItem_Color_Depth_24bpp;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.MenuItem menuItem_Screen;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.Windows.Forms.MenuItem menuItem_FocusStacking;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			
			menuItem_Open.Click += new EventHandler ( File_Open );
			menuItem_Save.Click += new EventHandler ( File_Save );
			menuItem_SaveAs.Click += new EventHandler ( File_SaveAs );
			menuItem_Close.Click += new EventHandler ( File_Close );

			menuItem_Rotate_Left.Click += new EventHandler ( Action_Rotate_Left );
			menuItem_Rotate_Right.Click += new EventHandler ( Action_Rotate_Right );
			menuItem_Rotate_180.Click += new EventHandler ( Action_Rotate_180 );

			menuItem_Flip_Vertical.Click += new EventHandler ( Action_Flip_Vertical );
			menuItem_Flip_Horizontal.Click += new EventHandler ( Action_Flip_Horizontal );

			menuItem_Crop.Click += new EventHandler ( Action_Crop );
			menuItem_Copy.Click += new EventHandler ( Action_Copy );

			menuItem_Resize.Click += new EventHandler ( Action_Resize );


			menuItem_ChannelHistogram.Click += new EventHandler ( Histogram_Channel );
			menuItem_CummulativeHistogram.Click += new EventHandler ( Histogram_Cummulative);
			menuItem_VolumeHistogram.Click += new EventHandler ( Histogram_Volume);
			
			menuItem_Convolve_Custom.Click += new EventHandler ( Convolution_Custom );
			menuItem_Convolve_Highpass.Click += new EventHandler ( Convolution_Highpass );
			menuItem_Convolve_Lowpass.Click += new EventHandler ( Convolution_Lowpass );
			menuItem_UnSharpMask.Click += new EventHandler ( Convolution_Unsharpmask );

			menuItem_Convolve_SobelX.Click += new EventHandler ( Convolution_SobelX );
			menuItem_Convolve_SobelY.Click += new EventHandler ( Convolution_SobelY );
			menuItem_Convolve_Roberts45.Click += new EventHandler ( Convolution_Roberts45 );
			menuItem_Convolve_Roberts135.Click += new EventHandler ( Convolution_Roberts135 );
			menuItem_Convolve_PrewittX.Click += new EventHandler ( Convolution_PrewittX );
			menuItem_Convolve_PrewittY.Click += new EventHandler ( Convolution_PrewittY );
						
			menuItem_Color_Split_RGB.Click += new EventHandler ( Color_Split_RGB );
			menuItem_Color_Convert2Gray.Click += new EventHandler ( Color_2_Gray );

			menuItem_Screen.Click += new EventHandler (Dither);

			// Initialize application's foot print
			int ScrnWid = Screen.PrimaryScreen.WorkingArea.Width;
			int ScrnLen = Screen.PrimaryScreen.WorkingArea.Height;

			m_list = new ArrayList();
			this.Bounds = new Rectangle ( 0, 0, ScrnWid / 2, ScrnLen / 2 );
		}

		// ====================================================================
		// Description:	Display an error box
		void DisplayError(string sError)
		// ====================================================================
		{
			MessageBox.Show( sError, 
				"Caption", MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
		}

		/// ///////////////////////////////////////////////////////////////////////////////////////
		// File

		// ====================================================================
		// Description:	1) Open File Dialog
		//				2) Create a new form
		//				3) Open file and bitblit onto form
		// Return:		void
		void File_Open (object sender, EventArgs evv)
		// ====================================================================
		{
			string sPath;
			try
			{
				sPath = ConfigurationSettings.AppSettings.Get("OPEN_PATH");
				openFileDialog1.InitialDirectory = sPath;
				openFileDialog1.Filter = "All files (*.*)|*.*|JPEG files (*.jpg)|*.jpg|PNG files (*.png)|*.png |TIFF files (*.tif)|*.tif|Bitmap files (*.bmp)|*.bmp|GIF files (*.gif)|*.gif" ;
				openFileDialog1.FilterIndex = 1 ;

				/* I create a temp file because the image is "Locked"
				* during the life of Image.  Temp file is deleted during
				* destruction of the object ImageView.
				*/
				if ( openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					ImageView fm = new ImageView();
					fm.MdiParent = this;
					
		
					fm.OnInitForm ( openFileDialog1.FileName, this.Width, this.Height );
					fm.Show();	
					m_list.Add (fm);

					sPath = openFileDialog1.FileName;
					int pos = sPath.LastIndexOf("\\");
					sPath = sPath.Substring(0, pos+1);
					ConfigurationSettings.AppSettings.Set("OPEN_PATH", sPath);
				}
			}
			catch(Exception e)
			{
				string str = e.ToString();
				if (!str.StartsWith("System.NotSupportedException: Collection is read-only."))
					DisplayError("File_Open() failed " + e.ToString() );
			}
		}
		// ====================================================================
		// Description:	1) Open File Dialog
		//				2) Save image to file
		// Return:		void
		void File_Save (object sender, EventArgs ev)
		// ====================================================================
		{
			try
			{
				ImageView fm = (ImageView)this.ActiveMdiChild;				// get current active form
				
				if ( fm != null )											// make sure there is an image opened
				{
					if ( fm.Text.Length > 0 )
					{					
						this.Cursor = Cursors.WaitCursor;
						fm.m_Img.Save (fm.Text );				 
						this.Cursor = Cursors.Default;			
					}
					else
						File_SaveAs (sender, ev);
				}
			}
			catch(Exception e)
			{
				DisplayError("File_Save() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	1) Open File Dialog
		//				2) Save image to file
		// Return:		void
		void File_SaveAs (object sender, EventArgs ev)
		// ====================================================================
		{
			// set up dialog box
			ImageView fm = (ImageView)this.ActiveMdiChild;				// get current active form
			string sName, sPath;
			int pos;

			try
			{
				if ( fm != null )											// make sure there is an image opened
				{
					if ( fm.Text.Length == 0 )
					{
						saveFileDialog1.InitialDirectory = "c:\\";
						saveFileDialog1.FileName = "Default";
					}
					else
					{
						/* extract name from path */
						pos = fm.Text.LastIndexOf ('\\');
						sName = fm.Text.Remove( 0, pos+1 );
						sPath = fm.Text.Remove (pos, fm.Text.Length - pos );
						saveFileDialog1.InitialDirectory = sPath;
						saveFileDialog1.FileName = sName;

						/* remove file extension */
						sName = saveFileDialog1.FileName;
						pos = sName.LastIndexOf(".");
						saveFileDialog1.FileName = sName.Remove (pos, sName.Length-pos);
					}
					
					saveFileDialog1.Filter = "JPEG files (*.jpg)|*.jpg|PNG files (*.png)|*.png |TIFF files (*.tif)|*.tif|Bitmap files (*.bmp)|*.bmp|GIF files (*.gif)|*.gif" ;
					saveFileDialog1.FilterIndex = 1;
					
					// get image and write to disk
					if ( saveFileDialog1.ShowDialog() == DialogResult.OK)
					{
						this.Cursor = Cursors.WaitCursor;
						switch ( saveFileDialog1.FilterIndex )
						{
							case (int)FileType.JPG:
								fm.m_Img.Save (saveFileDialog1.FileName, ImageFormat.Jpeg );
								break;

							case (int)FileType.PNG:
								fm.m_Img.Save (saveFileDialog1.FileName, ImageFormat.Png );
								break;

							case (int)FileType.TIF:
								fm.m_Img.Save (saveFileDialog1.FileName, ImageFormat.Tiff );
								break;

							case (int)FileType.BMP:
								fm.m_Img.Save (saveFileDialog1.FileName, ImageFormat.Bmp );
								break;

							case (int)FileType.GIF:
								fm.m_Img.Save (saveFileDialog1.FileName, ImageFormat.Gif );
								break;
						};
						this.Cursor = Cursors.Default;			
						fm.Text = saveFileDialog1.FileName;
					}
				}
			}
			catch(Exception e)
			{
				DisplayError("File_SaveAs() failed " + e.ToString() );
			}
		}


		// ====================================================================
		// Description:	Close the form
		// Return		void
		void File_Close (object sender, EventArgs ev)
		// ====================================================================
		{
			try
			{

				ImageView fm = (ImageView)this.ActiveMdiChild;				// get current active form

				if ( fm != null )											// make sure there is an image opened
				{
					for ( int i = 0; i < m_list.Count; i ++ )
					{
						if ( m_list[i] == fm )
						{
							m_list.RemoveAt(i);
							break;
						}
					}
					fm.Close();
				}
			}
			catch(Exception e)
			{
				DisplayError("File_Close() failed " + e.ToString() );
			}
		}
		/// ///////////////////////////////////////////////////////////////////////////////////////
		// Action
		
		// ====================================================================
		// Description:	Rotate image Left
		// Return		void
		void Action_Rotate_Left ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{

			ImageView fm = (ImageView)this.ActiveMdiChild;				// get current active form
			
			if ( fm != null )											// make sure there is an image opened
				fm.OnRotate(270);
			}
			catch(Exception e)
			{
				DisplayError("Action_Rotate_Left() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Rotate image Right
		// Return		void
		void Action_Rotate_Right ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				ImageView fm = (ImageView)this.ActiveMdiChild;				// get current active form
				
				if ( fm != null )											// make sure there is an image opened
					fm.OnRotate(90);
			}
			catch(Exception e)
			{
				DisplayError("Action_Rotate_Right() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Rotate image Right
		// Return		void
		void Action_Rotate_180 ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				ImageView fm = (ImageView)this.ActiveMdiChild;				// get current active form
				
				if ( fm != null )											// make sure there is an image opened
					fm.OnRotate( 180.0 );
			}
			catch(Exception e)
			{
				DisplayError("Action_Rotate_180() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Flip image vertical
		// Return		void
		void Action_Flip_Vertical ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				ImageView fm = (ImageView)this.ActiveMdiChild;				// get current active form
				
				if ( fm != null )											// make sure there is an image opened
					fm.OnFlip( (int)Flip.Vertical );
			}
			catch(Exception e)
			{
				DisplayError("Action_Flip_Vertical() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Flip image horizontal
		// Return		void
		void Action_Flip_Horizontal ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				ImageView fm = (ImageView)this.ActiveMdiChild;				// get current active form
				
				if ( fm != null )											// make sure there is an image opened
					fm.OnFlip( (int)Flip.Horizontal );
			}
			catch(Exception e)
			{
				DisplayError("Action_Flip_Horizontal() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Crop
		// Return		void
		void Action_Crop ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				ImageView fm = (ImageView)this.ActiveMdiChild;
				fm.OnCrop();
			}
			catch(Exception e)
			{
				DisplayError("Action_Crop() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Copy
		// Return		void
		void Action_Copy ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
			}
			catch(Exception e)
			{
				DisplayError("Action_Copy() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Resize
		// Return		void
		void Action_Resize ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
					Resize dlg = new Resize(fm);
					dlg.MdiParent = this;	

					dlg.Show();
				}
			}
			catch(Exception e)
			{
				DisplayError("Action_Resize() failed " + e.ToString() );
			}
		}

		/// ///////////////////////////////////////////////////////////////////////////////////////
		// Histogram

		// ====================================================================
		// Description:	Image histogram
		// Return		void
		void Histogram_Channel ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{		
					ImageView fm = (ImageView)this.ActiveMdiChild;
					
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}

					this.Cursor = Cursors.WaitCursor;
					ChannelHistogram dlg = new ChannelHistogram();
					dlg.MdiParent = this;				
					
					dlg.OnInitForm(fm);
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );

					this.Cursor = Cursors.Default;
				}
			}
			catch(Exception e)
			{
				DisplayError("Histogram_Channel() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Cummulative histogram
		// Return		void
		void Histogram_Cummulative ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;

					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}

					this.Cursor = Cursors.WaitCursor;
					CummulativeHistogram dlg = new CummulativeHistogram();
					dlg.MdiParent = this;				

					dlg.OnInitForm( fm );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );

					this.Cursor = Cursors.Default;
				}
			}
			catch(Exception e)
			{
				DisplayError("Histogram_Cummulative() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Volumetric histogram
		// Return		void
		void Histogram_Volume ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
			
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}

					this.Cursor = Cursors.WaitCursor;
					VolumeHistogram dlg = new VolumeHistogram();
					dlg.MdiParent = this;				

					dlg.OnInitForm( fm );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );

					this.Cursor = Cursors.Default;
				}
			}
			catch(Exception e)
			{
				DisplayError("Histogram_Volume() failed " + e.ToString() );
			}
		}

		/// ///////////////////////////////////////////////////////////////////////////////////////
		// Convolution - blur, sharpen, edge detection

		// ====================================================================
		// Description:	Convolution - Custom
		// Return		void
		void Convolution_Custom ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
			
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}
					Convolution dlg = new Convolution();
					dlg.MdiParent = this;
					dlg.OnInitForm( fm, (int)Kernel.Custom );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );
				}
			}
			catch(Exception e)
			{
				DisplayError("Convolution_Custom() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Convolution - Highpass
		// Return		void
		void Convolution_Highpass ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
			
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}

					Convolution dlg = new Convolution();
					dlg.MdiParent = this;

					dlg.OnInitForm( (ImageView)fm, (int)Kernel.Highpass );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );

				}
			}
			catch(Exception e)
			{
				DisplayError("Convolution_Highpass() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Convolution - Lowpass
		// Return		void
		void Convolution_Lowpass ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
			
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}

					Convolution dlg = new Convolution();
					dlg.MdiParent = this;

					dlg.OnInitForm( (ImageView)fm, (int)Kernel.Lowpass );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );

				}
			}
			catch(Exception e)
			{
				DisplayError("Convolution_Lowpass() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Convolution - Unsharp mask ( sharpening )
		// Return		void
		void Convolution_Unsharpmask ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
			
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}

					UnSharpMask dlg = new UnSharpMask();
					dlg.MdiParent = this;

					dlg.OnInitForm( (ImageView)fm );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );

				}
			}
			catch(Exception e)
			{
				DisplayError("Convolution_Unsharpmask() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Convolution - Sobel operator X
		// Return		void
		void Convolution_SobelX ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
			
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}
					Convolution dlg = new Convolution();
					dlg.MdiParent = this;

					dlg.OnInitForm( (ImageView)fm, (int)Kernel.Sobel_X );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );

				}
			}
			catch(Exception e)
			{
				DisplayError("Convolution_SobelX() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Convolution - Sobel operator Y axis
		// Return		void
		void Convolution_SobelY ( object sender, EventArgs ev )
			// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
			
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}
					Convolution dlg = new Convolution();
					dlg.MdiParent = this;

					dlg.OnInitForm( (ImageView)fm, (int)Kernel.Sobel_Y );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );
				}
			}
			catch(Exception e)
			{
				DisplayError("Convolution_SobelY() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Convolution - Roberts 45 degrees
		// Return		void
		void Convolution_Roberts45 ( object sender, EventArgs ev )
			// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
			
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}
					Convolution dlg = new Convolution();
					dlg.MdiParent = this;

					dlg.OnInitForm( (ImageView)fm, (int)Kernel.Roberts45 );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );

				}
			}
			catch(Exception e)
			{
				DisplayError("Convolution_Roberts45() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Convolution - Roberts 135 degrees
		// Return		void
		void Convolution_Roberts135 ( object sender, EventArgs ev )
			// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
			
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}
					Convolution dlg = new Convolution();
					dlg.MdiParent = this;

					dlg.OnInitForm( (ImageView)fm, (int)Kernel.Roberts135 );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );

				}
			}
			catch(Exception e)
			{
				DisplayError("Convolution_Roberts135() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Convolution - Prewitt X
		// Return		void
		void Convolution_PrewittX ( object sender, EventArgs ev )
			// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
			
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}
					Convolution dlg = new Convolution();
					dlg.MdiParent = this;

					dlg.OnInitForm( fm, (int)Kernel.Prewitt_X );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );

				}
			}
			catch(Exception e)
			{
				DisplayError("Convolution_PrewittX() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Convolution - Prewitt Y
		// Return		void
		void Convolution_PrewittY ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
			
					if ( fm.m_Img.PixelFormat != PixelFormat.Format24bppRgb )
					{
						MessageBoxButtons buttons = MessageBoxButtons.OK;
						MessageBox.Show(this, "Image Depth != 24bpp", null, buttons,
							MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
							MessageBoxOptions.RightAlign);
						return;
					}
					Convolution dlg = new Convolution();
					dlg.MdiParent = this;

					dlg.OnInitForm( fm, (int)Kernel.Prewitt_Y );
					dlg.Show();
					dlg.Location = new Point (this.Width-dlg.Width-PAD, 0 );
				}
			}
			catch(Exception e)
			{
				DisplayError("Convolution_PrewittY() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Color - split channels
		// Return		void
		void Color_Split_RGB ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView img = (ImageView)this.ActiveMdiChild;
					ColorRGB clr = new ColorRGB(img);
					
					string[] sName = new string[3];
					sName[(int)ClrChannel.Blue] = "_Blue";
					sName[(int)ClrChannel.Green] = "_Green";
					sName[(int)ClrChannel.Red] = "_Red";

					this.Cursor = Cursors.WaitCursor;
					
					for ( int i = 0; i < 3; i ++ )
					{
						Bitmap bmp = clr.GetChannel ( i );
						ImageView fm = new ImageView();
						fm.MdiParent = this;
						fm.OnInitForm ( img.Text + sName[i], bmp );
						fm.OnFormSize ( img.Width - img.XPADD, img.Height - img.YPADD );
						fm.Show();
						m_list.Add (fm);
					}

					this.Cursor = Cursors.Default;
				}
			}
			catch(Exception e)
			{
				DisplayError("Color_Split_RGB() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Color to gray
		// Return		void
		void Color_2_Gray ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if ( m_list.Count > 0 )
				{
					ImageView img = (ImageView)this.ActiveMdiChild;
					ColorRGB clr = new ColorRGB(img);
					
					string sName = "_Gray";

					this.Cursor = Cursors.WaitCursor;			
					
					Bitmap bmp = clr.GetChannel ( (int)ClrChannel.Gray );
					ImageView fm = new ImageView();
					fm.MdiParent = this;
					fm.OnInitForm ( img.Text + sName, bmp );
					fm.OnFormSize ( img.Width - img.XPADD, img.Height - img.YPADD );
					fm.Show();
					m_list.Add (fm);

					this.Cursor = Cursors.Default;
				}
			}
			catch(Exception e)
			{
				DisplayError("Color_2_Gray() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Halftone-Amplitude Modulation (AM)
		// Return		void
		void Dither ( object sender, EventArgs ev )
		// ====================================================================
		{
			try
			{
				if (m_list.Count > 0)
				{
					ImageView fm = (ImageView)this.ActiveMdiChild;
					this.Cursor = Cursors.WaitCursor;

					DitherDlg dlg = new DitherDlg();
					dlg.MdiParent = this;
					dlg.OnInitForm(fm);
					dlg.Show();
					dlg.Location = new Point(this.Width - dlg.Width - PAD, 0);

					this.Cursor = Cursors.Default;
				}
			}
			catch(Exception e)
			{
				DisplayError("Dither() failed " + e.ToString() );
			}
		}

		// ====================================================================
		// Description:	Merge multiple images of varying depth of field
		// Return:		void
		private void menuItem_FocusStacking_Click(object sender, System.EventArgs e)
		// ====================================================================
		{
			FocusStackDlg dlg;
			try
			{
				if ( this.m_list.Count > 1 )
				{
					dlg = new FocusStackDlg(m_list);
					dlg.MdiParent = this;
					dlg.Init();
					dlg.Show();
					dlg.Location = new Point(this.Width - dlg.Width - PAD, 0);
				}
				else
					DisplayError("Please open at least 2 images");
			}
			catch(Exception ex)
			{
				DisplayError("Focus Stacking() failed " + ex.ToString() );
			}
		}

		/// ///////////////////////////////////////////////////////////////////////////////////////
		// End

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
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem_Open = new System.Windows.Forms.MenuItem();
			this.menuItem_Save = new System.Windows.Forms.MenuItem();
			this.menuItem_SaveAs = new System.Windows.Forms.MenuItem();
			this.menuItem_Close = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItem_Rotate_Left = new System.Windows.Forms.MenuItem();
			this.menuItem_Rotate_Right = new System.Windows.Forms.MenuItem();
			this.menuItem_Rotate_180 = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuItem_Flip_Vertical = new System.Windows.Forms.MenuItem();
			this.menuItem_Flip_Horizontal = new System.Windows.Forms.MenuItem();
			this.menuItem_Crop = new System.Windows.Forms.MenuItem();
			this.menuItem_Copy = new System.Windows.Forms.MenuItem();
			this.menuItem_Resize = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem_ChannelHistogram = new System.Windows.Forms.MenuItem();
			this.menuItem_CummulativeHistogram = new System.Windows.Forms.MenuItem();
			this.menuItem_VolumeHistogram = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuItem_Convolve_Custom = new System.Windows.Forms.MenuItem();
			this.menuItem_Convolve_Lowpass = new System.Windows.Forms.MenuItem();
			this.menuItem_Convolve_Highpass = new System.Windows.Forms.MenuItem();
			this.menuItem_UnSharpMask = new System.Windows.Forms.MenuItem();
			this.menuItem_Convolve_SobelX = new System.Windows.Forms.MenuItem();
			this.menuItem_Convolve_SobelY = new System.Windows.Forms.MenuItem();
			this.menuItem_Convolve_Roberts45 = new System.Windows.Forms.MenuItem();
			this.menuItem_Convolve_Roberts135 = new System.Windows.Forms.MenuItem();
			this.menuItem_Convolve_PrewittX = new System.Windows.Forms.MenuItem();
			this.menuItem_Convolve_PrewittY = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItem_Color_Split_RGB = new System.Windows.Forms.MenuItem();
			this.menuItem_Color_Convert2Gray = new System.Windows.Forms.MenuItem();
			this.menuItem8 = new System.Windows.Forms.MenuItem();
			this.menuItem_Color_Depth_4bpp = new System.Windows.Forms.MenuItem();
			this.menuItem_Color_Depth_8bpp = new System.Windows.Forms.MenuItem();
			this.menuItem_Color_Depth_24bpp = new System.Windows.Forms.MenuItem();
			this.menuItem9 = new System.Windows.Forms.MenuItem();
			this.menuItem_Screen = new System.Windows.Forms.MenuItem();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.menuItem10 = new System.Windows.Forms.MenuItem();
			this.menuItem_FocusStacking = new System.Windows.Forms.MenuItem();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1,
																					  this.menuItem2,
																					  this.menuItem3,
																					  this.menuItem5,
																					  this.menuItem6,
																					  this.menuItem9,
																					  this.menuItem10});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem_Open,
																					  this.menuItem_Save,
																					  this.menuItem_SaveAs,
																					  this.menuItem_Close});
			this.menuItem1.Text = "File";
			// 
			// menuItem_Open
			// 
			this.menuItem_Open.Index = 0;
			this.menuItem_Open.Text = "Open";
			// 
			// menuItem_Save
			// 
			this.menuItem_Save.Index = 1;
			this.menuItem_Save.Text = "Save";
			// 
			// menuItem_SaveAs
			// 
			this.menuItem_SaveAs.Index = 2;
			this.menuItem_SaveAs.Text = "Save As...";
			// 
			// menuItem_Close
			// 
			this.menuItem_Close.Index = 3;
			this.menuItem_Close.Text = "Close";
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem4,
																					  this.menuItem7,
																					  this.menuItem_Crop,
																					  this.menuItem_Copy,
																					  this.menuItem_Resize});
			this.menuItem2.Text = "Action";
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 0;
			this.menuItem4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem_Rotate_Left,
																					  this.menuItem_Rotate_Right,
																					  this.menuItem_Rotate_180});
			this.menuItem4.Text = "Rotate";
			// 
			// menuItem_Rotate_Left
			// 
			this.menuItem_Rotate_Left.Index = 0;
			this.menuItem_Rotate_Left.Text = "Left";
			// 
			// menuItem_Rotate_Right
			// 
			this.menuItem_Rotate_Right.Index = 1;
			this.menuItem_Rotate_Right.Text = "Right";
			// 
			// menuItem_Rotate_180
			// 
			this.menuItem_Rotate_180.Index = 2;
			this.menuItem_Rotate_180.Text = "180 degrees";
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 1;
			this.menuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem_Flip_Vertical,
																					  this.menuItem_Flip_Horizontal});
			this.menuItem7.Text = "Flip";
			// 
			// menuItem_Flip_Vertical
			// 
			this.menuItem_Flip_Vertical.Index = 0;
			this.menuItem_Flip_Vertical.Text = "Vertical";
			// 
			// menuItem_Flip_Horizontal
			// 
			this.menuItem_Flip_Horizontal.Index = 1;
			this.menuItem_Flip_Horizontal.Text = "Horizontal";
			// 
			// menuItem_Crop
			// 
			this.menuItem_Crop.Index = 2;
			this.menuItem_Crop.Text = "Crop";
			// 
			// menuItem_Copy
			// 
			this.menuItem_Copy.Enabled = false;
			this.menuItem_Copy.Index = 3;
			this.menuItem_Copy.Text = "Copy";
			// 
			// menuItem_Resize
			// 
			this.menuItem_Resize.Index = 4;
			this.menuItem_Resize.Text = "Resize";
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 2;
			this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem_ChannelHistogram,
																					  this.menuItem_CummulativeHistogram,
																					  this.menuItem_VolumeHistogram});
			this.menuItem3.Text = "Histogram";
			// 
			// menuItem_ChannelHistogram
			// 
			this.menuItem_ChannelHistogram.Index = 0;
			this.menuItem_ChannelHistogram.Text = "Channels ";
			// 
			// menuItem_CummulativeHistogram
			// 
			this.menuItem_CummulativeHistogram.Index = 1;
			this.menuItem_CummulativeHistogram.Text = "Cummulative";
			// 
			// menuItem_VolumeHistogram
			// 
			this.menuItem_VolumeHistogram.Index = 2;
			this.menuItem_VolumeHistogram.Text = "Volume";
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 3;
			this.menuItem5.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem_Convolve_Custom,
																					  this.menuItem_Convolve_Lowpass,
																					  this.menuItem_Convolve_Highpass,
																					  this.menuItem_UnSharpMask,
																					  this.menuItem_Convolve_SobelX,
																					  this.menuItem_Convolve_SobelY,
																					  this.menuItem_Convolve_Roberts45,
																					  this.menuItem_Convolve_Roberts135,
																					  this.menuItem_Convolve_PrewittX,
																					  this.menuItem_Convolve_PrewittY});
			this.menuItem5.Text = "Convolution";
			// 
			// menuItem_Convolve_Custom
			// 
			this.menuItem_Convolve_Custom.Index = 0;
			this.menuItem_Convolve_Custom.Text = "Custom";
			// 
			// menuItem_Convolve_Lowpass
			// 
			this.menuItem_Convolve_Lowpass.Index = 1;
			this.menuItem_Convolve_Lowpass.Text = "Low Pass";
			// 
			// menuItem_Convolve_Highpass
			// 
			this.menuItem_Convolve_Highpass.Index = 2;
			this.menuItem_Convolve_Highpass.Text = "High Pass";
			// 
			// menuItem_UnSharpMask
			// 
			this.menuItem_UnSharpMask.Index = 3;
			this.menuItem_UnSharpMask.Text = "Unsharp-mask";
			// 
			// menuItem_Convolve_SobelX
			// 
			this.menuItem_Convolve_SobelX.Index = 4;
			this.menuItem_Convolve_SobelX.Text = "Sobel X";
			// 
			// menuItem_Convolve_SobelY
			// 
			this.menuItem_Convolve_SobelY.Index = 5;
			this.menuItem_Convolve_SobelY.Text = "Sobel Y";
			// 
			// menuItem_Convolve_Roberts45
			// 
			this.menuItem_Convolve_Roberts45.Index = 6;
			this.menuItem_Convolve_Roberts45.Text = "Roberts 45";
			// 
			// menuItem_Convolve_Roberts135
			// 
			this.menuItem_Convolve_Roberts135.Index = 7;
			this.menuItem_Convolve_Roberts135.Text = "Roberts 135";
			// 
			// menuItem_Convolve_PrewittX
			// 
			this.menuItem_Convolve_PrewittX.Index = 8;
			this.menuItem_Convolve_PrewittX.Text = "Prewitt X";
			// 
			// menuItem_Convolve_PrewittY
			// 
			this.menuItem_Convolve_PrewittY.Index = 9;
			this.menuItem_Convolve_PrewittY.Text = "Prewitt W";
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 4;
			this.menuItem6.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem_Color_Split_RGB,
																					  this.menuItem_Color_Convert2Gray,
																					  this.menuItem8});
			this.menuItem6.Text = "Color";
			// 
			// menuItem_Color_Split_RGB
			// 
			this.menuItem_Color_Split_RGB.Index = 0;
			this.menuItem_Color_Split_RGB.Text = "Split RGB";
			// 
			// menuItem_Color_Convert2Gray
			// 
			this.menuItem_Color_Convert2Gray.Index = 1;
			this.menuItem_Color_Convert2Gray.Text = " to Gray";
			// 
			// menuItem8
			// 
			this.menuItem8.Enabled = false;
			this.menuItem8.Index = 2;
			this.menuItem8.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem_Color_Depth_4bpp,
																					  this.menuItem_Color_Depth_8bpp,
																					  this.menuItem_Color_Depth_24bpp});
			this.menuItem8.Text = "Depth";
			// 
			// menuItem_Color_Depth_4bpp
			// 
			this.menuItem_Color_Depth_4bpp.Index = 0;
			this.menuItem_Color_Depth_4bpp.Text = "4 bpp ";
			// 
			// menuItem_Color_Depth_8bpp
			// 
			this.menuItem_Color_Depth_8bpp.Index = 1;
			this.menuItem_Color_Depth_8bpp.Text = "8 bpp";
			// 
			// menuItem_Color_Depth_24bpp
			// 
			this.menuItem_Color_Depth_24bpp.Index = 2;
			this.menuItem_Color_Depth_24bpp.Text = "24 bpp";
			// 
			// menuItem9
			// 
			this.menuItem9.Index = 5;
			this.menuItem9.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem_Screen});
			this.menuItem9.Text = "Dither";
			// 
			// menuItem_Screen
			// 
			this.menuItem_Screen.Index = 0;
			this.menuItem_Screen.Text = "Screen";
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 6;
			this.menuItem10.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuItem_FocusStacking});
			this.menuItem10.Text = "Multi-Images";
			// 
			// menuItem_FocusStacking
			// 
			this.menuItem_FocusStacking.Index = 0;
			this.menuItem_FocusStacking.Text = "Focus Stacking";
			this.menuItem_FocusStacking.Click += new System.EventHandler(this.menuItem_FocusStacking_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(688, 409);
			this.IsMdiContainer = true;
			this.Menu = this.mainMenu1;
			this.Name = "Form1";
			this.Text = "Viewer";

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		

	}
}
