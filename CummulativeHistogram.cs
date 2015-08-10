// ============================================================================
// Module:		Cummulative Histogram
//
// Description:	A window to display Cummulative histogram of image 
//				( a channel at a time )
//
// Purpose:		Display/Adjust Cummulative histogram
//
// Input:		A raster image
// Output:		Adjusted image and histogram
//
// Author:		Chi Toung Yeung			cty
//
// History:
// 21Jan07		UI working, combo boxes, knots								cty
// 21Jan07		Need to tie intensity channel with R,G,B channels	
//				Need to update Cummulative histogram dynamically.			cty
// 19Feb07		Preview image is made the same size as the original image	cty
// 19Feb07		Preview start position is 1/2 offset from original			cty
// 20Feb07		Convert all image pixel arithemtics to unsafe code.
//				- using pointers to directly access pixel values in mem.	cty
// 23Feb07		Allow 3 knots for initialization							cty
// 25Feb07		Fixed OnComboKnotsClick() to get a new copy of m_Orig.m_Img
//				- so as to not re-adjusting the same image					cty
// ============================================================================
	
using System;
using System.Drawing;
using System.Drawing.Imaging;					// bitmap data
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Viewer
{
	/// <summary>
	/// Summary description for CummulativeHistogram.
	/// </summary>
	public class CummulativeHistogram : System.Windows.Forms.Form
	{
		/* Constants */
		const int CHANNEL_DEPTH = 256;
		const int NUM_CHANNEL = 4;
		const int INIT_NUM_KNOT = 5;
		const int MIN_NUM_KNOT = 3;
		const int INIT_CHANNEL = 3;
		
		/* Variables declaration */
		public ImageView m_Orig;									// point to the original
		public ImageView m_prv;										// a preview image window
		public int[] m_RGBKHist;									// 4 histograms for R, G, B, and Intensity
		public int[] m_LUT;											// 4 look up tables

		private Point[] m_Pts;										// knot points
		private int m_NKnots;										// number of knots
		private int m_KnotIndex;									// position of knots move					

		private bool m_bKnotMove = false;							// a knot moved ?
		private Point m_OrigPos = Point.Empty;						// original position of a knot 

		/* Event declaration */
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.PictureBox pictureBox_CumHist;
		private System.Windows.Forms.ComboBox comboBox_Channel;
		private System.Windows.Forms.TextBox textBox_InSelect;
		private System.Windows.Forms.Label label_OutSelect;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.ComboBox comboBox_Knots;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label_OutLUT;
		private System.Windows.Forms.TextBox textBox_InLUT;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		///////////////////////////////////////////////////////////////////////////////////////////
		// Public

		// ====================================================================
		//	Description:	Constructor
		//	Return:			void
		public CummulativeHistogram()
		// ====================================================================
		{			
			InitializeComponent();

			/* Initial event handlers */
			buttonOK.Click += new EventHandler ( OnOK );
			buttonCancel.Click += new EventHandler ( OnCancel );
			comboBox_Channel.SelectedIndexChanged += new EventHandler ( OnComboChannelClick );
			comboBox_Knots.SelectedIndexChanged += new EventHandler ( OnComboKnotsClick );

			pictureBox_CumHist.MouseDown += new MouseEventHandler ( OnPicMouseDown );
			pictureBox_CumHist.MouseUp += new MouseEventHandler ( OnPicMouseUp );
			pictureBox_CumHist.MouseMove += new MouseEventHandler ( OnPicMouseMove );
						
		}

		// ====================================================================
		//	Description:	Initialize this form / class
		//	Return:			void
		public void OnInitForm ( ImageView fm )
		// ====================================================================
		{
			/* allocation */
			m_Orig = fm;
			m_RGBKHist = new int[CHANNEL_DEPTH*NUM_CHANNEL];
			BuiltHist((Bitmap)fm.m_Img);				// create histograms R,G,B,Intensity		
			InitKnots();								// create knot points

			/* Show a preview image */
			if ( m_prv != null)
				m_prv.Dispose();

			m_prv = new ImageView();
			m_prv.MdiParent = this.ParentForm;
			m_prv.OnInitForm ( "Preview", (Bitmap)fm.m_Img );
			m_prv.OnFormSize ( m_Orig.Width-m_Orig.XPADD, m_Orig.Height-m_Orig.YPADD );
			m_prv.Show();
			m_prv.Location = new Point ( m_Orig.Location.X + m_Orig.Width/2, m_Orig.Location.Y+m_Orig.Height/2);
	

			/* combo boxes */
			comboBox_Knots.Items.Clear();	
			comboBox_Knots.Items.Add("3");			
			comboBox_Knots.Items.Add("5");			
			comboBox_Knots.Items.Add("9");			
			comboBox_Knots.Items.Add("17");			
			comboBox_Knots.Items.Add("33");			
			comboBox_Knots.Items.Add("65");
			comboBox_Knots.Items.Add("129");
			comboBox_Knots.SelectedIndex = INIT_NUM_KNOT;

			comboBox_Channel.Items.Clear();
			comboBox_Channel.Items.Add("Red");			// Initialize combo box
			comboBox_Channel.Items.Add("Green");
			comboBox_Channel.Items.Add("Blue");
			comboBox_Channel.Items.Add("Intensity");
			comboBox_Channel.SelectedIndex = INIT_CHANNEL;

			this.Text = fm.Text;
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
		// UI - Combo box clicked

		// ====================================================================
		//	Description:	Combo box for Channel clicked
		//	Return:			void
		private void OnComboChannelClick ( object sender,
										EventArgs e )
		// ====================================================================
		{
			if ( ( comboBox_Channel.SelectedIndex > -1 ) && 
				( comboBox_Knots.SelectedIndex > -1 ) )
			{
				Point f = new Point(1);
				f.X = -1;
				InterpolateKnots(m_NKnots, comboBox_Channel.SelectedIndex);
				DrawHist ( (int)comboBox_Channel.SelectedIndex, f);
			}
		}

		// ====================================================================
		//	Description:	Combo box for Knots clicked
		//	Return:			void
		private void OnComboKnotsClick ( object sender,
										EventArgs e )
		// ====================================================================
		{
			if ( ( comboBox_Channel.SelectedIndex > -1 ) && 
				( comboBox_Knots.SelectedIndex > -1 ) )
			{
				int numKnots = MIN_NUM_KNOT;

				for ( int i = 0; i < comboBox_Knots.SelectedIndex; i ++ )
					numKnots = ( numKnots - 1 ) * 2 + 1;

				Point f = new Point(1);
				f.X = -1;
				InterpolateKnots(numKnots, comboBox_Channel.SelectedIndex);
				DrawHist ( (int)comboBox_Channel.SelectedIndex, f );
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// UI - Picture Box Cummulative Histogram

		// ====================================================================
		//	Description:	Mouse down on Histogram
		//	Return:			void
		private void OnPicMouseDown ( object sender,
									MouseEventArgs e )
		// ====================================================================
		{
			const int PAD = 2;

			for ( int i = 0; i < m_NKnots; i ++ )
			{
				if ( ( ( m_Pts[i].X + PAD ) > e.X ) &&
					( ( m_Pts[i].X - PAD ) < e.X ) )
				{
					if ( ( ( m_Pts[i].Y + PAD ) > e.Y ) &&
						( ( m_Pts[i].Y - PAD ) < e.Y ) )
					{
						m_bKnotMove = true;
						m_OrigPos.X = e.X;
						m_OrigPos.Y = e.Y;
						m_KnotIndex = i;
						return;
					}
				}
			}
		}

		// ====================================================================
		//	Description:	Mouse up on Histogram
		//	Return:			void
		private void OnPicMouseUp ( object sender,
									MouseEventArgs e )
		// ====================================================================
		{
			if ( m_bKnotMove == true )
			{
				this.Cursor = Cursors.WaitCursor;
				UpdateLUT(comboBox_Channel.SelectedIndex); 

				m_prv.m_Img.Dispose();
				m_prv.m_Img = (Image)m_Orig.m_Img.Clone();

				for ( int i = 0; i < NUM_CHANNEL; i ++ )
					UpdateImage(i);

				this.Cursor = Cursors.Default;
				m_prv.Repaint();
			}

			m_OrigPos = Point.Empty;
			m_bKnotMove = false;
		}

		// ====================================================================
		//	Description:	Mouse move on Histogram
		//	Return:			void
		private void OnPicMouseMove ( object sender,
									MouseEventArgs e )
		// ====================================================================
		{
			Point p = new Point (1);
			p.X = e.X;
			p.Y = e.Y;

			if ( m_bKnotMove )
				UpdateKnots ( p );
						
			DrawHist ( (int)comboBox_Channel.SelectedIndex, p );
			
			int i = comboBox_Channel.SelectedIndex;
			textBox_InSelect.Text = Convert.ToString(e.X);
			label_OutSelect.Text = Convert.ToString(m_RGBKHist[i*CHANNEL_DEPTH+e.X]);
			
			textBox_InLUT.Text = Convert.ToString(e.X);
			label_OutLUT.Text = Convert.ToString(255-m_LUT[i*CHANNEL_DEPTH+e.X]);

		}

		// ====================================================================
		//	Description:	Draw histogram image
		//	Return:			void
		private void DrawHist( int channel, 
								Point e )
			// ====================================================================
		{
			Bitmap bmp = new Bitmap ( pictureBox_CumHist.Width, pictureBox_CumHist.Height );
			Graphics g = Graphics.FromImage( bmp );
			Rectangle rect = new Rectangle(0, 0, pictureBox_CumHist.Width, pictureBox_CumHist.Height);
			
			g.FillRectangle(Brushes.White, 0,0, bmp.Width, bmp.Height);

			/* draw histogram */
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

			Pen wp = new Pen ( Color.LightGray, 1 );
			for ( int i = 0; i < CHANNEL_DEPTH; i ++ )
			{
				int pos;
				double h = ( (double)m_RGBKHist[channel*CHANNEL_DEPTH+i] * (double)pictureBox_CumHist.Height / ((double)m_prv.m_Img.Height*(double)m_prv.m_Img.Width) );
				
				if ( h > 1.0) pos = pictureBox_CumHist.Height - (int)h;
				else if ( h == 0 ) pos = pictureBox_CumHist.Height;
				else pos = pictureBox_CumHist.Height - 1;

				if ( i == e.X )
					g.DrawLine (wp, i, pictureBox_CumHist.Height, i, pos);
				else
					g.DrawLine(p, i, pictureBox_CumHist.Height, i, pos);
			}

			/* draw grid */
			Pen Pen = new Pen ( Color.DarkGray, 1 );
			g.DrawLine (Pen, pictureBox_CumHist.Width/2, 0, pictureBox_CumHist.Width/2, pictureBox_CumHist.Height );
			g.DrawLine (Pen, 0, pictureBox_CumHist.Height/2, pictureBox_CumHist.Width, pictureBox_CumHist.Height/2 );

			g.DrawLine (Pen, (float)pictureBox_CumHist.Width/(float)4, 0, (float)pictureBox_CumHist.Width/(float)4, pictureBox_CumHist.Height );
			g.DrawLine (Pen, 0, (float)pictureBox_CumHist.Height/(float)4, pictureBox_CumHist.Width, (float)pictureBox_CumHist.Height/(float)4 );
			
			g.DrawLine (Pen, (float)pictureBox_CumHist.Width*(float)(3.0/4.0), 0, (float)pictureBox_CumHist.Width*(float)(3.0/4.0), pictureBox_CumHist.Height );
			g.DrawLine (Pen, 0, (float)pictureBox_CumHist.Height*(float)(3.0/4.0), pictureBox_CumHist.Width, (float)pictureBox_CumHist.Height*(float)(3.0/4.0) );
			
			/* draw curve */
			for (int i = 0; i < m_NKnots; i ++ )
				g.FillEllipse(Brushes.Cyan, m_Pts[i].X-3, m_Pts[i].Y-3, 5, 5 );

			Pen blkPen = new Pen ( Color.FromArgb(0,0,0), 1 );		
			g.DrawCurve(blkPen, m_Pts );
			g.Dispose();
			pictureBox_CumHist.Image = (Image)bmp;
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Private

		// ====================================================================
		//	Description:	Initialize Knots
		//	Return:			void
		private void InitKnots ()
		// ====================================================================
		{
			m_NKnots = 5;
			m_Pts = null;
			m_Pts = new Point[m_NKnots];
			m_LUT = new int[CHANNEL_DEPTH*NUM_CHANNEL];
			
			for ( int i = 0; i < NUM_CHANNEL; i ++ )
			{
				m_Pts[0].X = 0;
				m_Pts[0].Y = pictureBox_CumHist.Height-1;

				m_Pts[1].X = (pictureBox_CumHist.Width-1) / 4;
				m_Pts[1].Y = (pictureBox_CumHist.Height-1) / 4 * 3;

				m_Pts[2].X = (pictureBox_CumHist.Width-1) / 2;
				m_Pts[2].Y = (pictureBox_CumHist.Height-1) / 2;
	
				m_Pts[3].X = (pictureBox_CumHist.Width-1) / 4 * 3;
				m_Pts[3].Y = (pictureBox_CumHist.Height-1) / 4;

				m_Pts[4].X = (pictureBox_CumHist.Width-1);
				m_Pts[4].Y = 0;

				/* initialize linear look up table */
				for ( int j = 0; j < CHANNEL_DEPTH; j ++ )
					m_LUT[i*CHANNEL_DEPTH+j] = 255-j;
			}
		}

		// ====================================================================
		//	Description:	Interpolate Knots
		//	Return:			void
		private void InterpolateKnots (int NxNumKnot,	// [in] new count of knots
										int channel )	// [in] color channel selected
		// ====================================================================
		{
			m_Pts = null;
			m_Pts = new Point[NxNumKnot];

			for ( int k = 0; k < NxNumKnot; k ++ )
			{
				m_Pts[k].X = (int)( 255.0 / (double)( NxNumKnot - 1 ) * (double)k );
				m_Pts[k].Y = m_LUT[channel*CHANNEL_DEPTH+m_Pts[k].X];
			}
			
			m_NKnots = NxNumKnot;
		}

		// ====================================================================
		//	Description:	Update knot point moved by mouse click
		//	Return:			void
		private void UpdateKnots ( Point e )	// [in] new knot position
		// ====================================================================
		{
			m_Pts[m_KnotIndex].X = e.X;
			m_Pts[m_KnotIndex].Y = e.Y;
		}

		// ====================================================================
		//	Description:	Update Knots in Look up table
		//	Return:			void
		private void UpdateLUT (int channel)	// [in] channel to update
		// ====================================================================
		{
			Bitmap bmp = (Bitmap)pictureBox_CumHist.Image;
			
			for ( int x = 0; x < CHANNEL_DEPTH; x ++ )
			{
				int y = 0;
				bool bFound = false;
				while ( ( y < 256 ) && ( bFound == false ) )
				{
					Color clr = bmp.GetPixel(x,y);
				//	if ( clr == Color.Black )
					if ( (clr.R == 0 ) && ( clr.G == 0 ) && ( clr.B == 0 ) )
						bFound = true;
					else
						y ++;
				}
				m_LUT[x+channel*CHANNEL_DEPTH] = y;
			}
		}

		// ====================================================================
		//	Description:	Built a histogram from image
		//	Return:			void
		private void BuiltHist( Bitmap bmp )
			// ====================================================================
		{
			/* Build Histogram 
			 * Histogram array is aligned with 4 channels: R, G, B, K */
			m_RGBKHist.Initialize();
			
			BitmapData data = bmp.LockBits( new Rectangle( 0 , 0 , bmp.Width , bmp.Height ) , 
				ImageLockMode.ReadWrite  , PixelFormat.Format24bppRgb  );

			unsafe
			{ 
				byte* ptr = ( byte* )( data.Scan0 ); 
				for ( int y = 0; y < bmp.Height; y ++ )
				{
					for ( int x = 0; x < bmp.Width; x ++ )
					{
						byte b = *ptr; ptr ++;
						byte g = *ptr; ptr ++;
						byte r = *ptr; ptr ++;

						m_RGBKHist[r] ++;
						m_RGBKHist[CHANNEL_DEPTH+g] ++;
						m_RGBKHist[CHANNEL_DEPTH*2+b] ++;
						
						double dMean = ( (double)r + (double)g + (double)b ) / 3.0;
						m_RGBKHist[CHANNEL_DEPTH*3+(int)dMean] ++;
					}
					ptr += data.Stride - data.Width * 3;
				}
				
				for ( int i = 1; i < CHANNEL_DEPTH; i ++ )
					for ( int k = 0; k < NUM_CHANNEL; k++ )
						m_RGBKHist[i+k*CHANNEL_DEPTH] += m_RGBKHist[i-1+k*CHANNEL_DEPTH];
			}
			bmp.UnlockBits(data);
		}

		// ====================================================================
		//	Description:	Update image through look up table
		//	Return:			void
		private void UpdateImage ( int iChannel )
		// ====================================================================
		{
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
						int b = *ptr; ptr ++;
						int g = *ptr; ptr ++;
						int r = *ptr;

						switch ( k )
						{
							case (int)Channel.Red:
								r = 255-m_LUT[r];
								break;

							case (int)Channel.Green:
								g = 255-m_LUT[CHANNEL_DEPTH+g];
								break;

							case (int)Channel.Blue:
								b = 255-m_LUT[2*CHANNEL_DEPTH+b];
								break;

							case (int)Channel.Neutral:
								r = 255-m_LUT[3*CHANNEL_DEPTH+r];
								g = 255-m_LUT[3*CHANNEL_DEPTH+g];
								b = 255-m_LUT[3*CHANNEL_DEPTH+b];
								break;
						};
						ptr -= 2;
						*ptr = (byte)b; ptr ++;
						*ptr = (byte)g; ptr ++;
						*ptr = (byte)r; ptr ++;
					}
					ptr += data.Stride - data.Width * 3;
				}
				bmp.UnlockBits(data);
			}
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
			this.pictureBox_CumHist = new System.Windows.Forms.PictureBox();
			this.comboBox_Channel = new System.Windows.Forms.ComboBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label_OutSelect = new System.Windows.Forms.Label();
			this.textBox_InSelect = new System.Windows.Forms.TextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.comboBox_Knots = new System.Windows.Forms.ComboBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.label_OutLUT = new System.Windows.Forms.Label();
			this.textBox_InLUT = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox_CumHist
			// 
			this.pictureBox_CumHist.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox_CumHist.Location = new System.Drawing.Point(24, 64);
			this.pictureBox_CumHist.Name = "pictureBox_CumHist";
			this.pictureBox_CumHist.Size = new System.Drawing.Size(256, 256);
			this.pictureBox_CumHist.TabIndex = 0;
			this.pictureBox_CumHist.TabStop = false;
			// 
			// comboBox_Channel
			// 
			this.comboBox_Channel.Location = new System.Drawing.Point(24, 24);
			this.comboBox_Channel.Name = "comboBox_Channel";
			this.comboBox_Channel.Size = new System.Drawing.Size(121, 21);
			this.comboBox_Channel.TabIndex = 1;
			this.comboBox_Channel.Text = "comboBox1";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label_OutSelect);
			this.groupBox1.Controls.Add(this.textBox_InSelect);
			this.groupBox1.Location = new System.Drawing.Point(296, 24);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(120, 56);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Selection";
			// 
			// label_OutSelect
			// 
			this.label_OutSelect.Location = new System.Drawing.Point(64, 24);
			this.label_OutSelect.Name = "label_OutSelect";
			this.label_OutSelect.Size = new System.Drawing.Size(48, 16);
			this.label_OutSelect.TabIndex = 1;
			this.label_OutSelect.Text = "label1";
			// 
			// textBox_InSelect
			// 
			this.textBox_InSelect.Location = new System.Drawing.Point(8, 24);
			this.textBox_InSelect.Name = "textBox_InSelect";
			this.textBox_InSelect.Size = new System.Drawing.Size(48, 20);
			this.textBox_InSelect.TabIndex = 0;
			this.textBox_InSelect.Text = "textBox1";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.comboBox_Knots);
			this.groupBox2.Location = new System.Drawing.Point(296, 152);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(120, 56);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Knot Points";
			// 
			// comboBox_Knots
			// 
			this.comboBox_Knots.Location = new System.Drawing.Point(8, 24);
			this.comboBox_Knots.Name = "comboBox_Knots";
			this.comboBox_Knots.Size = new System.Drawing.Size(64, 21);
			this.comboBox_Knots.TabIndex = 0;
			this.comboBox_Knots.Text = "comboBox1";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.buttonCancel);
			this.groupBox3.Controls.Add(this.buttonOK);
			this.groupBox3.Location = new System.Drawing.Point(296, 224);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(120, 96);
			this.groupBox3.TabIndex = 4;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Action";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Location = new System.Drawing.Point(24, 56);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "Cancel";
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(24, 24);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Text = "OK";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.label_OutLUT);
			this.groupBox4.Controls.Add(this.textBox_InLUT);
			this.groupBox4.Location = new System.Drawing.Point(296, 88);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(120, 56);
			this.groupBox4.TabIndex = 3;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Look up table";
			// 
			// label_OutLUT
			// 
			this.label_OutLUT.Location = new System.Drawing.Point(64, 24);
			this.label_OutLUT.Name = "label_OutLUT";
			this.label_OutLUT.Size = new System.Drawing.Size(48, 16);
			this.label_OutLUT.TabIndex = 1;
			this.label_OutLUT.Text = "label1";
			// 
			// textBox_InLUT
			// 
			this.textBox_InLUT.Location = new System.Drawing.Point(8, 24);
			this.textBox_InLUT.Name = "textBox_InLUT";
			this.textBox_InLUT.Size = new System.Drawing.Size(48, 20);
			this.textBox_InLUT.TabIndex = 0;
			this.textBox_InLUT.Text = "textBox1";
			// 
			// CummulativeHistogram
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(440, 350);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.comboBox_Channel);
			this.Controls.Add(this.pictureBox_CumHist);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox4);
			this.Name = "CummulativeHistogram";
			this.Text = "CummulativeHistogram";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
