// ============================================================================
// Module:		Channel Histogram
//
// Description:	A window to display histogram of image ( a channel at a time )
//
// Purpose:		Display Adjust histogram
//
// Input:		A raster image
// Output:		Adjusted image and histogram
//
// Author:		Chi Toung Yeung			cty
//
// History:		
// 11Jan07		Created User interface										cty
// 12Jan07		Added 2 sliders - pictureBoxMin & pictureBoxMax				cty
// 13Jan07		Added FindMode()											cty
// 14Jan07		Histogram display works!
//				Combo box works!
//				Selection box and line draw on histogram works !			cty
//				Strange: e.X is different between different MouseMove methods
// 15Jan07		Added preview image, call ImageView to create copy			cty
// 16Jan07		Added wait cursor... next, I need invalidate() method for 
//				preview image.  
//				Started work on Gamma										cty
// 17Jan07		Repaint for Image preview added								cty
//				Intensity works for gamma and equalization					cty
//				Added record settings between channel selections			cty
//				Added code for OnOK(), OnCancel()							cty
// 19Jan07		Changed min & max picturebox shape to triangle				cty
// 21Jan07		Remove redundant code for DrawHist()						cty
// 19Feb07		Preview image is made the same size as the original image	cty
// 19Feb07		Preview start position is 1/2 offset from original			cty
// 20Feb07		Convert all image pixel arithemtics to unsafe code.
//				- using pointers to directly access pixel values in mem.	cty
// 25Feb07		Added OnImageChanged(), now it works as should. 
//				- before, I would continue to adjust the m_prv.m_Img.
//				- now, I get a fresh copy.									cty
// ============================================================================
using System;
using System.Drawing;
using System.Drawing.Imaging;					// bitmap data
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Viewer
{
	enum Channel:int{Red, Green, Blue, Neutral};
	/// <summary>
	/// Summary description for ChannelHistogram.
	/// </summary>
	public class ChannelHistogram : System.Windows.Forms.Form
	{
		/* Constants */
		const int CHANNEL_DEPTH = 256;
		const int NUM_CHANNEL = 4;
		
		private int m_MinXPos = 15;									// Slider bar minimum position on form
		private int m_MaxXPos = 270;								// Slider bar maximum position on form
		private int m_YPos = 215;									// Slider bar heigth on form

		/* Variables declaration */
		public ImageView m_Orig;									// point to the original
		public ImageView m_prv;										// a preview image window
		public int[] m_RGBKHist;									// 4 histograms for R, G, B, and Intensity
		public long[] m_HistMode;									// 4 histogram mode - determine max height of values

		private int[] m_MinLoc;										// position of minimum slider bar per channel
		private int[] m_MaxLoc;										// position of maximum slider bar per channel
		private int[] m_GammaLoc;									// position of gamma slider bar per channel
		
		private Point m_MinPoint = Point.Empty;						// Slider bar minimum - histogram equalization 
		private Point m_MaxPoint = Point.Empty;						// Slider bar maximum - histogram equalization 
		private Point m_GammaPoint = Point.Empty;					// Slider bar - gamma adjustment
		private Point m_HistPoint = Point.Empty;					// Info - on histogram, cursor position and amount 

		/* Event declaration */
		private System.Windows.Forms.PictureBox pictureBoxHist;
		private System.Windows.Forms.PictureBox pictureBoxMin;
		private System.Windows.Forms.PictureBox pictureBoxMax;
		private System.Windows.Forms.ComboBox comboBox_Channel;
		private System.Windows.Forms.GroupBox groupBox_InSelect;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox_InMin;
		private System.Windows.Forms.GroupBox groupBox_InMax;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.TextBox textBox_Gamma;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.PictureBox pictureBox_Gamma;
		private System.Windows.Forms.TextBox textBox_InMin;
		private System.Windows.Forms.TextBox textBox_InSelect;
		private System.Windows.Forms.TextBox textBox_InMax;
		private System.Windows.Forms.Label label_OutSelect;
		private System.Windows.Forms.Label label_OutMin;
		private System.Windows.Forms.Label label_OutMax;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		///////////////////////////////////////////////////////////////////////////////////////////
		// Public

		// ====================================================================
		//	Description:	Constructor
		//	Return:			void
		public ChannelHistogram()
		// ====================================================================
		{
			InitializeComponent();
			
			/* Allocation & Initialization */		
			DrawUI();
			
			/* Initial event handlers */
			pictureBoxMin.MouseUp += new MouseEventHandler ( OnPBMinMouseUp );
			pictureBoxMin.MouseMove += new MouseEventHandler ( OnPBMinMouseMove );
			pictureBoxMin.MouseDown += new MouseEventHandler ( OnPBMinMouseDown );

			pictureBoxMax.MouseUp += new MouseEventHandler ( OnPBMaxMouseUp );
			pictureBoxMax.MouseMove += new MouseEventHandler ( OnPBMaxMouseMove );
			pictureBoxMax.MouseDown += new MouseEventHandler ( OnPBMaxMouseDown );

			pictureBox_Gamma.MouseUp += new MouseEventHandler ( OnPBGmMouseUp );
			pictureBox_Gamma.MouseMove += new MouseEventHandler ( OnPBGmMouseMove );
			pictureBox_Gamma.MouseDown += new MouseEventHandler ( OnPBGmMouseDown );

			pictureBoxHist.MouseMove += new MouseEventHandler ( OnHistMouseMove );
			comboBox_Channel.SelectedIndexChanged += new EventHandler ( OnComboClick );

			buttonOK.Click += new EventHandler ( OnOK );
			buttonCancel.Click += new EventHandler ( OnCancel );
		}

		// ====================================================================
		//	Description:	Initialize this form / class
		//	Return:			void
		public void OnInitForm ( ImageView fm )	// [in] image object
		// ====================================================================
		{
			/* allocation */
			m_Orig = fm;
			m_RGBKHist = new int[CHANNEL_DEPTH*NUM_CHANNEL];
			m_HistMode = new long[NUM_CHANNEL];

			m_MinLoc = new int[NUM_CHANNEL];
			m_MaxLoc = new int[NUM_CHANNEL];
			m_GammaLoc = new int[NUM_CHANNEL];

			/* initialize slider settings */
			for ( int i = 0; i < NUM_CHANNEL; i ++ )
			{
				m_MinLoc[i] = m_MinXPos;
				m_MaxLoc[i] = m_MaxXPos;
				m_GammaLoc[i] = m_MaxXPos-CHANNEL_DEPTH/2;
			}

			BuiltHist((Bitmap)fm.m_Img);				// create histograms R,G,B,Intensity
			FindStat();									// Find Mode of all 4 histograms
			
			comboBox_Channel.Items.Clear();
			comboBox_Channel.Items.Add("Red");			// Initialize combo box
			comboBox_Channel.Items.Add("Green");
			comboBox_Channel.Items.Add("Blue");
			comboBox_Channel.Items.Add("Intensity");
			comboBox_Channel.SelectedIndex = 3;

			this.Text = fm.Text;

			/* Show a preview image */
			if ( m_prv != null)
				m_prv.Dispose();

			m_prv = new ImageView();
			m_prv.MdiParent = this.ParentForm;
			m_prv.OnInitForm ( "Preview", (Bitmap)fm.m_Img );
			m_prv.OnFormSize ( m_Orig.Width-m_Orig.XPADD, m_Orig.Height-m_Orig.YPADD );
			m_prv.Show();
			m_prv.Location = new Point ( m_Orig.Location.X + m_Orig.Width/2, m_Orig.Location.Y+m_Orig.Height/2);
			
		}

		// ====================================================================
		//	Description:	Okay depressed
		//	Return:			void
		public void OnOK (object sender
						, EventArgs e )
		// ====================================================================
		{
			m_Orig.OnInitForm ( m_Orig.Text, (Bitmap)m_prv.m_Img );
			OnCancel (sender, e);
		}

		// ====================================================================
		//	Description:	Cancel depressed
		//	Return:			void
		public void OnCancel (object sender
							, EventArgs e)
		// ====================================================================
		{
			m_prv.Dispose();
			this.Dispose();
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Private

		// ====================================================================
		//	Description:	Built a histogram from image
		//	Return:			void
		private void BuiltHist( Bitmap bmp )
			// ====================================================================
		{
			m_RGBKHist.Initialize();

			BitmapData data = bmp.LockBits ( new Rectangle (0,0, bmp.Width, bmp.Height ), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb );

			unsafe
			{
				byte r,g,b;
				byte *ptr = (byte*)(data.Scan0);
				for ( int y = 0; y < bmp.Height; y ++ )
				{
					for ( int x = 0; x < bmp.Width; x ++ )
					{
						b = *ptr; ptr++;
						g = *ptr; ptr++;
						r = *ptr; ptr++;

						m_RGBKHist[r]++;	
						m_RGBKHist[CHANNEL_DEPTH+g]++;	
						m_RGBKHist[CHANNEL_DEPTH*2+b]++;
						double dMean = ((double)r+(double)g+(double)b)/3.0;
						m_RGBKHist[CHANNEL_DEPTH*3+(int)dMean] ++;
					}
					ptr += data.Stride - data.Width*3;
				}
			}
			bmp.UnlockBits(data);
		}

		// ====================================================================
		//	Description:	Find Histogram mode
		//	Return:			void
		private void FindStat ()
		// ====================================================================
		{
			m_HistMode[0] = m_HistMode[1] = m_HistMode[2] = m_HistMode[3] = 0;

			for ( int i = 0; i < CHANNEL_DEPTH; i ++ )
			{
				for ( int j = 0; j < NUM_CHANNEL; j ++ )
				{
					if ( m_RGBKHist[i+j*CHANNEL_DEPTH] > m_HistMode[j] ) 
						m_HistMode[j] = m_RGBKHist[i+j*CHANNEL_DEPTH];
				}
			}
		}
		
		// ====================================================================
		//	Description:	Initialize the statistics
		//	Return:			void
		private void InitStat ()
		// ====================================================================
		{
			int k = comboBox_Channel.SelectedIndex;
			textBox_InMin.Text = Convert.ToString ( m_MinLoc[k]-m_MinXPos );
			label_OutMin.Text = Convert.ToString ( m_RGBKHist[(m_MinLoc[k]-m_MinXPos)] );
			pictureBoxMin.Bounds = new Rectangle ( m_MinLoc[k], m_YPos, 11, 11 );
			
			textBox_InMax.Text = Convert.ToString ( m_MaxLoc[k]-m_MinXPos );
			label_OutMax.Text = Convert.ToString ( m_RGBKHist[(m_MaxLoc[k]-m_MinXPos)] );
			pictureBoxMax.Bounds = new Rectangle ( m_MaxLoc[k], m_YPos, 11, 11 );
			
			label_OutSelect.Text = Convert.ToString(m_RGBKHist[k*CHANNEL_DEPTH+126]);
			textBox_InSelect.Text = Convert.ToString( 126 );

			textBox_Gamma.Text = Convert.ToString ( (double)(m_GammaLoc[k]-m_MinXPos)/255.0*2.0 );
			pictureBox_Gamma.Bounds = new Rectangle ( m_GammaLoc[k], m_YPos, 11, 11 );
		}

		// ====================================================================
		//	Description:	Draw histogram image
		//	Return:			void
		private void DrawHist( int channel, Point e )
		// ====================================================================
		{
			Bitmap bmp = new Bitmap ( pictureBoxHist.Width, pictureBoxHist.Height );
			Graphics g = Graphics.FromImage( bmp );
			Rectangle rect = new Rectangle(0, 0, pictureBoxHist.Width, pictureBoxHist.Height);
			Pen p; 
			
			switch ( channel )
			{
				case (int)Channel.Red:
					p = new Pen(Color.Red,1);
					break;

				case (int)Channel.Green:
					p = new Pen(Color.Green,1);
					break;
					
				case (int)Channel.Blue:
					p = new Pen(Color.Blue,1);
					break;

				default:
					p = new Pen(Color.Gray,1);
					break;
			};	

			Pen blkpen = new Pen(Color.Black, 1);
			for ( int i = 0; i < CHANNEL_DEPTH; i ++ )
			{
				int pos;
				double h = ( (double)m_RGBKHist[channel*CHANNEL_DEPTH+i] * (double)pictureBoxHist.Height / (double)m_HistMode[channel]);
				
				if ( h > 1.0) pos = pictureBoxHist.Height - (int)h;
				else if ( h == 0 ) pos = pictureBoxHist.Height;
				else pos = pictureBoxHist.Height - 1;
				
				if ( i == e.X )
					g.DrawLine(blkpen, i, pictureBoxHist.Height, i, pos);
				else
				g.DrawLine(p, i, pictureBoxHist.Height, i, pos);
			}
			g.Dispose();
			pictureBoxHist.Image = (Image)bmp;
		}

		// ====================================================================
		//	Description:
		//	Return:			void
		private void DrawUI()
		// ====================================================================
		{
			Bitmap bmp = new Bitmap (11, 11);
			Pen blkPen = new Pen(Brushes.Black,1);
			Graphics g = Graphics.FromImage( bmp );
			Rectangle rect = new Rectangle ( 0, 0, 9, 9 );
			Point pt1 = new Point (0,0);
			Point pt2 = new Point (0,9);
			Point pt3 = new Point (9,9);
			Point[] myPoints = {pt1, pt2, pt3};
			g.DrawPolygon(blkPen, myPoints);
			g.FillPolygon(Brushes.Olive, myPoints);
		
		
			pictureBoxMin.Image = (Image) bmp;
			pictureBoxMax.Image = (Image) bmp;

			Bitmap bmp2 = new Bitmap (11, 11);
			Graphics h = Graphics.FromImage( bmp2 );
			Rectangle r = new Rectangle ( 0, 0, 9, 9 );
			h.DrawEllipse(blkPen, rect);
			h.FillEllipse (Brushes.DarkOrchid, rect);
			pictureBox_Gamma.Image = (Image)bmp2;

			h.Dispose();
			g.Dispose();
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// UI - Combo box clicked

		// ====================================================================
		//	Description:
		//	Return:			void
		private void OnComboClick ( object sender,
								 EventArgs e )
		// ====================================================================
		{
			Point f = new Point(1);
			f.X = -1;

			DrawHist ( (int)comboBox_Channel.SelectedIndex, f );
			InitStat();
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// PictureBox Histogram

		// ====================================================================
		// Description:	Mouse over histogram
		// Return:		void
		private void OnHistMouseMove ( object sender
			, MouseEventArgs e )
		// ====================================================================
		{
			int i = comboBox_Channel.SelectedIndex;
			Point f = new Point(1);
			f.X = e.X;
			f.Y = e.Y;
			DrawHist ( i, f );
	
			if ( ( e.X < 256 )&& ( e.X >= 0 ) )
			{
				label_OutSelect.Text = Convert.ToString(m_RGBKHist[i*CHANNEL_DEPTH+e.X]);
				textBox_InSelect.Text = Convert.ToString( e.X );
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// PictureBox Gamma - slider

		// ====================================================================
		// Description:	Mouse click on form
		// Return:		void
		private void OnPBGmMouseDown ( object sender
									, MouseEventArgs e )
		// ====================================================================
		{
			if ( m_GammaPoint == Point.Empty)
				m_GammaPoint.X = e.X;
		}
		// ====================================================================
		// Description:	Mouse Up
		// Return:		void
		private void OnPBGmMouseUp ( object sender
									, MouseEventArgs e )
		// ====================================================================
		{
			OnImageChanged();
			m_GammaPoint = Point.Empty;
		}

		// ====================================================================
		// Description:	Mouse Move
		// Return:		void
		private void OnPBGmMouseMove ( object sender
									, MouseEventArgs e )
		// ====================================================================
		{
			if ( m_GammaPoint != Point.Empty )
			{
				m_GammaPoint.X+=e.X;
				m_GammaPoint.X = ( m_GammaPoint.X < m_MinXPos ) ? m_MinXPos : m_GammaPoint.X;
				m_GammaPoint.X = ( m_GammaPoint.X > m_MaxXPos ) ? m_MaxXPos : m_GammaPoint.X;

				pictureBox_Gamma.Bounds = new Rectangle ( m_GammaPoint.X, m_YPos, 11, 11 );		
				pictureBox_Gamma.Invalidate();

				double dGamma = (((double)m_GammaPoint.X - 15.0 ) / 255.0 * 2.0 );
				textBox_Gamma.Text = Convert.ToString ( dGamma );
				int k = comboBox_Channel.SelectedIndex;
				m_GammaLoc[k] = m_GammaPoint.X;
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// PictureBox Min - slider

		// ====================================================================
		// Description:	Mouse click on form
		// Return:		void
		private void OnPBMinMouseDown ( object sender
								, MouseEventArgs e )
		// ====================================================================
		{
			if ( m_MinPoint == Point.Empty)
				m_MinPoint.X = e.X;
		}
		// ====================================================================
		// Description:	Mouse Up
		// Return:		void
		private void OnPBMinMouseUp ( object sender
								, MouseEventArgs e )
		// ====================================================================
		{
			OnImageChanged();		
			m_MinPoint = Point.Empty;
		}

		// ====================================================================
		// Description:	Mouse Move
		// Return:		void
		private void OnPBMinMouseMove ( object sender
								, MouseEventArgs e )
		// ====================================================================
		{
			if ( m_MinPoint != Point.Empty )
			{
				m_MinPoint.X+=e.X;
				m_MinPoint.X = ( m_MinPoint.X < m_MinXPos ) ? m_MinXPos : m_MinPoint.X;
				m_MinPoint.X = ( m_MinPoint.X > m_MaxXPos ) ? m_MaxXPos : m_MinPoint.X;

				pictureBoxMin.Bounds = new Rectangle ( m_MinPoint.X, m_YPos, 11, 11 );		
				pictureBoxMin.Invalidate();

				int k = comboBox_Channel.SelectedIndex;
				textBox_InMin.Text = Convert.ToString ( m_MinPoint.X-m_MinXPos );
				label_OutMin.Text = Convert.ToString ( m_RGBKHist[m_MinPoint.X-m_MinXPos+k*CHANNEL_DEPTH] );
				m_MinLoc[k] = m_MinPoint.X;
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Picture Box Max - slider

		// ====================================================================
		// Description:	Mouse click on form
		// Return:		void
		private void OnPBMaxMouseDown ( object sender
									, MouseEventArgs e )
		// ====================================================================
		{
			if ( m_MaxPoint == Point.Empty)
				m_MaxPoint.X = e.X;
		}
		// ====================================================================
		// Description:	Mouse Up
		// Return:		void
		private void OnPBMaxMouseUp ( object sender
									, MouseEventArgs e )
		// ====================================================================
		{
			OnImageChanged();
			m_MaxPoint = Point.Empty;
		}

		// ====================================================================
		// Description:	Mouse Move
		// Return:		void
		private void OnPBMaxMouseMove ( object sender
									, MouseEventArgs e )
		// ====================================================================
		{
			if ( m_MaxPoint != Point.Empty )
			{
				m_MaxPoint.X+=e.X;
				m_MaxPoint.X = ( m_MaxPoint.X < m_MinXPos ) ? m_MinXPos : m_MaxPoint.X;
				m_MaxPoint.X = ( m_MaxPoint.X > m_MaxXPos ) ? m_MaxXPos : m_MaxPoint.X;
				

				pictureBoxMax.Bounds = new Rectangle ( m_MaxPoint.X, m_YPos, 11, 11 );		
				pictureBoxMax.Invalidate();

				int k = comboBox_Channel.SelectedIndex;
				textBox_InMax.Text = Convert.ToString ( m_MaxPoint.X -m_MinXPos);
				label_OutMax.Text = Convert.ToString ( m_RGBKHist[m_MaxPoint.X-m_MinXPos+k*CHANNEL_DEPTH] );
				m_MaxLoc[k] = m_MaxPoint.X;
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Image processing

		// ====================================================================
		// Description:	Gamma, min or max point has changed by user
		// Return:		void
		private void OnImageChanged ()
		// ====================================================================
		{
			this.Cursor = Cursors.WaitCursor;

			m_prv.m_Img.Dispose();
			m_prv.m_Img = (Image)m_Orig.m_Img.Clone();

			for ( int i = 0; i < NUM_CHANNEL; i ++ )
			{
				if ( ( m_MinLoc[i] != m_MinXPos ) && ( m_MaxLoc[i] != m_MaxXPos ) )
					HistEqualize ( m_MinLoc[i], m_MaxLoc[i], i );

				else if ( m_MinLoc[i] != m_MinXPos )
					HistEqualize ( m_MinLoc[i], 255, i );

				else if ( m_MaxLoc[i] != m_MaxXPos )
					HistEqualize ( 0, m_MaxLoc[i], i );

				if ( m_GammaLoc[i] != m_MaxXPos-CHANNEL_DEPTH/2 )
					GammaChanged ( m_GammaLoc[i], i );
			}
			this.Cursor = Cursors.Default;
			m_prv.Repaint();
		}


		// ====================================================================
		// Description:	Gamma has changed
		//					gamma range of 0 to 2
		//
		// Return:		void
		private void GammaChanged (int GammaPoint,
								   int iChannel )
		// ====================================================================
		{
			if ( GammaPoint != m_MaxXPos-CHANNEL_DEPTH/2 )
			{
				int[] iLUT = new int[256];
				double dGamma = (( (double)GammaPoint - 15.0 ) / 255.0 * 2.0 );
				double denom = Math.Pow ( (double)(CHANNEL_DEPTH-1), dGamma );

				for ( int i = 0; i < CHANNEL_DEPTH; i ++ )
					iLUT[i] = (int)(Math.Pow( (double)i, dGamma ) / denom * (double)(CHANNEL_DEPTH - 1));

				Bitmap bmp = (Bitmap)m_prv.m_Img;
				BitmapData data = bmp.LockBits( new Rectangle( 0 , 0 , bmp.Width , bmp.Height ) , 
					ImageLockMode.ReadWrite  , PixelFormat.Format24bppRgb  );

				int k = iChannel;

				unsafe
				{ 
					byte* ptr = ( byte* )( data.Scan0 ); 
					for ( int y = 0; y < bmp.Height; y ++ )
					{
						for ( int x = 0; x < bmp.Width; x ++ )
						{
							byte b = *ptr; ptr ++;
							byte g = *ptr; ptr ++;
							byte r = *ptr; 

							switch ( k )
							{
								case (int)Channel.Red:
									r = (byte)iLUT[ r ];
									break;

								case (int)Channel.Green:
									g = (byte)iLUT[ g ];
									break;

								case (int)Channel.Blue:
									b = (byte)iLUT[ b ];
									break;

								case (int)Channel.Neutral:
									r = (byte)iLUT[ r ];
									g = (byte)iLUT[ g ];
									b = (byte)iLUT[ b ];
									break;
							};
							ptr -=2;
							*ptr = b; ptr ++;
							*ptr = g; ptr ++;
							*ptr = r; ptr ++;
						}
						ptr += data.Stride - data.Width * 3;
					}
				}
				bmp.UnlockBits(data);
			}
		}

		// ====================================================================
		// Description:	Histogram equalization 
		// Return:		void
		private void HistEqualize ( int iNMin,	// [in] new minimum
									int iNMax, 	// [in] new maximum
									int iChannel )
		// ====================================================================
		{
			if ( ( iNMin != m_MinXPos ) || ( iNMax != m_MaxXPos ) )
			{
				int iMax = ( iNMax == m_MaxXPos )? 255:iNMax-15;
				int iMin = ( iNMax < iNMin )? 0:iNMin-15;
					
				Bitmap bmp = (Bitmap)m_prv.m_Img;		
				BitmapData data = bmp.LockBits( new Rectangle( 0 , 0 , bmp.Width , bmp.Height ) , 
					ImageLockMode.ReadWrite  , PixelFormat.Format24bppRgb  );

				double d = ( 255.0 / (double)(iMax - iMin) );
				int k = iChannel;

				unsafe
				{ 
					byte* ptr = ( byte* )( data.Scan0 ); 
					for ( int y = 0; y < bmp.Height; y ++ )
					{
						for ( int x = 0; x < bmp.Width; x ++ )
						{
							byte b = *ptr; ptr ++;
							byte g = *ptr; ptr ++;
							byte r = *ptr;

							switch ( k )
							{
								case (int)Channel.Red:
									r = Cap(((double)( r - iMin )) * d);
									break;

								case (int)Channel.Green:
									g = Cap(((double)( g - iMin )) * d);
									break;

								case (int)Channel.Blue:
									b = Cap(((double)( b - iMin )) * d);
									break;

								case (int)Channel.Neutral:
									r = Cap(((double)( r - iMin )) * d);
									g = Cap(((double)( g - iMin )) * d);
									b = Cap(((double)( b - iMin )) * d);
									
									break;
							};
							ptr -= 2;
							*ptr = b; ptr ++;
							*ptr = g; ptr ++;
							*ptr = r; ptr ++;
						}
						ptr += data.Stride - data.Width * 3;
					}
				}
				bmp.UnlockBits(data);
			}
		}

		private byte Cap ( double value )
		{
			if ( value < 0 )
				return 0;

			if ( value > 255 )
				return 255;

			return (byte)value;
		}


		///////////////////////////////////////////////////////////////////////////////////////////
		// Ends
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
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
			this.pictureBoxHist = new System.Windows.Forms.PictureBox();
			this.pictureBoxMin = new System.Windows.Forms.PictureBox();
			this.pictureBoxMax = new System.Windows.Forms.PictureBox();
			this.comboBox_Channel = new System.Windows.Forms.ComboBox();
			this.textBox_InMin = new System.Windows.Forms.TextBox();
			this.textBox_InSelect = new System.Windows.Forms.TextBox();
			this.groupBox_InSelect = new System.Windows.Forms.GroupBox();
			this.label_OutSelect = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox_InMin = new System.Windows.Forms.GroupBox();
			this.label_OutMin = new System.Windows.Forms.Label();
			this.groupBox_InMax = new System.Windows.Forms.GroupBox();
			this.textBox_InMax = new System.Windows.Forms.TextBox();
			this.label_OutMax = new System.Windows.Forms.Label();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.textBox_Gamma = new System.Windows.Forms.TextBox();
			this.pictureBox_Gamma = new System.Windows.Forms.PictureBox();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox_InSelect.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox_InMin.SuspendLayout();
			this.groupBox_InMax.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBoxHist
			// 
			this.pictureBoxHist.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.pictureBoxHist.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBoxHist.Location = new System.Drawing.Point(15, 64);
			this.pictureBoxHist.Name = "pictureBoxHist";
			this.pictureBoxHist.Size = new System.Drawing.Size(256, 150);
			this.pictureBoxHist.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBoxHist.TabIndex = 0;
			this.pictureBoxHist.TabStop = false;
			// 
			// pictureBoxMin
			// 
			this.pictureBoxMin.Location = new System.Drawing.Point(15, 215);
			this.pictureBoxMin.Name = "pictureBoxMin";
			this.pictureBoxMin.Size = new System.Drawing.Size(16, 24);
			this.pictureBoxMin.TabIndex = 2;
			this.pictureBoxMin.TabStop = false;
			// 
			// pictureBoxMax
			// 
			this.pictureBoxMax.Location = new System.Drawing.Point(256, 215);
			this.pictureBoxMax.Name = "pictureBoxMax";
			this.pictureBoxMax.Size = new System.Drawing.Size(16, 24);
			this.pictureBoxMax.TabIndex = 3;
			this.pictureBoxMax.TabStop = false;
			// 
			// comboBox_Channel
			// 
			this.comboBox_Channel.Location = new System.Drawing.Point(16, 24);
			this.comboBox_Channel.Name = "comboBox_Channel";
			this.comboBox_Channel.Size = new System.Drawing.Size(121, 21);
			this.comboBox_Channel.TabIndex = 4;
			this.comboBox_Channel.Text = "comboBox1";
			// 
			// textBox_InMin
			// 
			this.textBox_InMin.Location = new System.Drawing.Point(8, 16);
			this.textBox_InMin.Name = "textBox_InMin";
			this.textBox_InMin.Size = new System.Drawing.Size(40, 20);
			this.textBox_InMin.TabIndex = 5;
			this.textBox_InMin.Text = "textBox1";
			// 
			// textBox_InSelect
			// 
			this.textBox_InSelect.Location = new System.Drawing.Point(16, 16);
			this.textBox_InSelect.Name = "textBox_InSelect";
			this.textBox_InSelect.Size = new System.Drawing.Size(40, 20);
			this.textBox_InSelect.TabIndex = 7;
			this.textBox_InSelect.Text = "textBox3";
			// 
			// groupBox_InSelect
			// 
			this.groupBox_InSelect.Controls.Add(this.textBox_InSelect);
			this.groupBox_InSelect.Controls.Add(this.label_OutSelect);
			this.groupBox_InSelect.Location = new System.Drawing.Point(288, 8);
			this.groupBox_InSelect.Name = "groupBox_InSelect";
			this.groupBox_InSelect.Size = new System.Drawing.Size(128, 48);
			this.groupBox_InSelect.TabIndex = 8;
			this.groupBox_InSelect.TabStop = false;
			this.groupBox_InSelect.Text = "Selection";
			// 
			// label_OutSelect
			// 
			this.label_OutSelect.Location = new System.Drawing.Point(64, 24);
			this.label_OutSelect.Name = "label_OutSelect";
			this.label_OutSelect.Size = new System.Drawing.Size(56, 16);
			this.label_OutSelect.TabIndex = 8;
			this.label_OutSelect.Text = "label";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.groupBox_InMin);
			this.groupBox2.Controls.Add(this.groupBox_InMax);
			this.groupBox2.Location = new System.Drawing.Point(288, 64);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(128, 136);
			this.groupBox2.TabIndex = 9;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Boundary";
			// 
			// groupBox_InMin
			// 
			this.groupBox_InMin.Controls.Add(this.textBox_InMin);
			this.groupBox_InMin.Controls.Add(this.label_OutMin);
			this.groupBox_InMin.Location = new System.Drawing.Point(8, 24);
			this.groupBox_InMin.Name = "groupBox_InMin";
			this.groupBox_InMin.Size = new System.Drawing.Size(112, 48);
			this.groupBox_InMin.TabIndex = 10;
			this.groupBox_InMin.TabStop = false;
			this.groupBox_InMin.Text = "Min";
			// 
			// label_OutMin
			// 
			this.label_OutMin.Location = new System.Drawing.Point(56, 24);
			this.label_OutMin.Name = "label_OutMin";
			this.label_OutMin.Size = new System.Drawing.Size(48, 16);
			this.label_OutMin.TabIndex = 6;
			this.label_OutMin.Text = "label_InMin";
			// 
			// groupBox_InMax
			// 
			this.groupBox_InMax.Controls.Add(this.textBox_InMax);
			this.groupBox_InMax.Controls.Add(this.label_OutMax);
			this.groupBox_InMax.Location = new System.Drawing.Point(8, 80);
			this.groupBox_InMax.Name = "groupBox_InMax";
			this.groupBox_InMax.Size = new System.Drawing.Size(112, 48);
			this.groupBox_InMax.TabIndex = 11;
			this.groupBox_InMax.TabStop = false;
			this.groupBox_InMax.Text = "Max";
			// 
			// textBox_InMax
			// 
			this.textBox_InMax.Location = new System.Drawing.Point(8, 16);
			this.textBox_InMax.Name = "textBox_InMax";
			this.textBox_InMax.Size = new System.Drawing.Size(40, 20);
			this.textBox_InMax.TabIndex = 5;
			this.textBox_InMax.Text = "textBox1";
			// 
			// label_OutMax
			// 
			this.label_OutMax.Location = new System.Drawing.Point(56, 24);
			this.label_OutMax.Name = "label_OutMax";
			this.label_OutMax.Size = new System.Drawing.Size(48, 16);
			this.label_OutMax.TabIndex = 7;
			this.label_OutMax.Text = "label_InMax";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.textBox_Gamma);
			this.groupBox5.Location = new System.Drawing.Point(288, 208);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(128, 48);
			this.groupBox5.TabIndex = 9;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Gamma";
			// 
			// textBox_Gamma
			// 
			this.textBox_Gamma.Location = new System.Drawing.Point(16, 16);
			this.textBox_Gamma.Name = "textBox_Gamma";
			this.textBox_Gamma.Size = new System.Drawing.Size(40, 20);
			this.textBox_Gamma.TabIndex = 7;
			this.textBox_Gamma.Text = "textBox3";
			// 
			// pictureBox_Gamma
			// 
			this.pictureBox_Gamma.Location = new System.Drawing.Point(142, 215);
			this.pictureBox_Gamma.Name = "pictureBox_Gamma";
			this.pictureBox_Gamma.Size = new System.Drawing.Size(16, 24);
			this.pictureBox_Gamma.TabIndex = 10;
			this.pictureBox_Gamma.TabStop = false;
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(8, 24);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.TabIndex = 11;
			this.buttonOK.Text = "OK";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(96, 24);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.TabIndex = 12;
			this.buttonCancel.Text = "Cancel";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.buttonCancel);
			this.groupBox1.Controls.Add(this.buttonOK);
			this.groupBox1.Location = new System.Drawing.Point(16, 256);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(184, 64);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Action";
			// 
			// ChannelHistogram
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(432, 334);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.pictureBox_Gamma);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox_InSelect);
			this.Controls.Add(this.comboBox_Channel);
			this.Controls.Add(this.pictureBoxMax);
			this.Controls.Add(this.pictureBoxMin);
			this.Controls.Add(this.pictureBoxHist);
			this.Controls.Add(this.groupBox5);
			this.Name = "ChannelHistogram";
			this.Text = "Channel Histogram";
			this.groupBox_InSelect.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox_InMin.ResumeLayout(false);
			this.groupBox_InMax.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
