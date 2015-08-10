// ============================================================================
// Module:		Volume Histogram
//
// Description:	A window to display Volume histogram of image 
//				( 3-D projection )
//
// Purpose:		Display/Volumetric histogram
//
// Input:		A raster image
//
// Author:		Chi Toung Yeung			cty
//
// History:
// 23Jan07		Start today													cty		
// 24Jan07		Created binarytree and buildhist(), needs test				cty
// 25Jan07		Added code for pictureBoxSpin control						cty
// 11Feb07		Fixed Binarytree and MouseMove()							cty
// 11Feb07		1st working													cty
// 20Feb07		Convert all image pixel arithemtics to unsafe code.
//				- using pointers to directly access pixel values in mem.	cty
// ============================================================================

using System;
using System.Drawing;
using System.Drawing.Imaging;					// bitmap data
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Viewer
{
	enum PUSH:int{EMPTY, UP, DOWN, LEFT, RIGHT};

	/// <summary>
	/// Summary description for VolumeHistogram.
	/// </summary>
	public class VolumeHistogram : System.Windows.Forms.Form
	{
		const int SPIN_WIDTH = 75;
		const int SPIN_HEIGHT = 75;
		const int VOL_WIDTH = 300;
		const int PICBOXOFFSET = 181;								// picture box offset (to center)

		/* Variables declaration */
		public ImageView m_Orig;									// point to the original
		public int[] m_RGBVol;
		public double[] m_XYZCVol;									// Histogram in projection mode
		ulong m_lSize;												// number of entries

		private System.Windows.Forms.PictureBox pictureBox_Volume;
		private System.Windows.Forms.PictureBox pictureBox_Spin;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox textBox_R;
		private System.Windows.Forms.TextBox textBox_G;
		private System.Windows.Forms.TextBox textBox_B;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBox_Count;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		///////////////////////////////////////////////////////////////////////////////////////////
		// Public

		// ====================================================================
		//	Description:	Constructor
		//	Return:			void
		public VolumeHistogram()
		// ====================================================================
		{
			InitializeComponent();

			pictureBox_Spin.MouseUp += new MouseEventHandler ( OnPBSpinMouseUp );
			pictureBox_Spin.MouseDown += new MouseEventHandler ( OnPBSpinMouseDown );

			pictureBox_Volume.MouseMove += new MouseEventHandler ( OnPBVolMouseMove );
		}

		// ====================================================================
		//	Description:	Initialize this form / class
		//	Return:			void
		public void OnInitForm ( ImageView fm )
		// ====================================================================
		{
			/* allocation */
			m_Orig = fm;
			this.Text = fm.Text;

			BuiltHist();
			RotateZ(45);
			RotateY(45);
			DrawPBVolume();
			DrawPBSpin((int)PUSH.EMPTY);
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Picture box Spin - UI
		
		// ====================================================================
		//	Description:	Mouse Down - Rotate volume and redraw image
		//	Return:			void
		private void OnPBSpinMouseDown ( object sender
									, MouseEventArgs e )
		// ====================================================================
		{
			if ( ( e.X > ( SPIN_WIDTH/2-15 ) )&& ( e.X < ( SPIN_WIDTH/2+15 ) ) )
			{				
				/* click on up button */
				if ( ( e.Y > 0 ) && ( e.Y < ( SPIN_HEIGHT-18) ) )
				{
					DrawPBSpin((int)PUSH.UP);
					RotateX(10);
					DrawPBVolume();
				}
				/* click on down button */
				else if ( ( e.Y > ( SPIN_HEIGHT/2+18 ) ) && ( e.Y < ( SPIN_HEIGHT ) ) )
				{
					DrawPBSpin((int)PUSH.DOWN);
					RotateX(10);
					DrawPBVolume();
				}
			}

			/* click on left or right button */
			else if ( ( e.Y > SPIN_HEIGHT/2-15 )&& ( e.Y < SPIN_HEIGHT/2+15 ) )
			{
				/* click on left button */
				if ( ( e.X > 0 ) && ( e.X < ( SPIN_WIDTH-18) ) )
				{
					DrawPBSpin((int)PUSH.LEFT);
					RotateY(10);
					DrawPBVolume();
				}
				/* click on right button */
				else if ( ( e.X > ( SPIN_WIDTH/2+18 ) ) && ( e.X < ( SPIN_WIDTH) ) ) 
				{
					DrawPBSpin((int)PUSH.RIGHT);
					RotateY(10);
					DrawPBVolume();
				}
			}
		}

		// ====================================================================
		//	Description:	Mouse up
		//	Return:			void
		private void OnPBSpinMouseUp ( object sender
									, MouseEventArgs e )
		// ====================================================================
		{
			DrawPBSpin((int)PUSH.EMPTY);
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Picture box Volume - UI

		// ====================================================================
		//	Description:	Mouse move on picture box volume
		//	Return:			void
		private void OnPBVolMouseMove ( object sender
									, MouseEventArgs e )
		// ====================================================================
		{
			/* Improvement can be made here by with an efficient search method, tree or hash */
			for ( ulong i = 0; i < m_lSize; i ++ )
			{
				if ( ( e.X > (int)m_XYZCVol[i*4]-2+PICBOXOFFSET ) && ( e.X < (int)m_XYZCVol[i*4]+2+PICBOXOFFSET ) )
				{
					if ( ( e.Y > (int)m_XYZCVol[i*4+1]-2+PICBOXOFFSET ) && ( e.Y < (int)m_XYZCVol[i*4+1]+2+PICBOXOFFSET ) )
					{
						textBox_R.Text = Convert.ToString (m_RGBVol[i*4]);
						textBox_G.Text = Convert.ToString (m_RGBVol[i*4+1]);
						textBox_B.Text = Convert.ToString (m_RGBVol[i*4+2]);
						textBox_Count.Text = Convert.ToString (m_RGBVol[i*4+3]);
						break;
					}
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Private

		// ====================================================================
		//	Description:	built a histogram
		//	Return:			void
		private void BuiltHist ()
		// ====================================================================
		{
			BinaryTree tree = new BinaryTree();
			Bitmap bmp = (Bitmap)m_Orig.m_Img;
			BitmapData data = bmp.LockBits( new Rectangle( 0 , 0 , bmp.Width , bmp.Height ) , 
				ImageLockMode.ReadWrite  , PixelFormat.Format24bppRgb  );
			
			ulong item = 0;

			MessageBoxButtons buttons = MessageBoxButtons.YesNo;
			string message = "Errored";

			/* built binary tree */
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

						item = ((ulong)r) << 24;
						item += ((ulong)g) << 16;
						item += ((ulong)b) << 8;

						if ( tree.Insert(item) == false )
						{
							MessageBox.Show (this, message, null, buttons,
								MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 
								MessageBoxOptions.RightAlign);
							break;
						}
					}
					ptr += data.Stride - data.Width * 3;
				}
			}
			bmp.UnlockBits(data);

	
			/* Find size of histogram */
			m_lSize = tree.NumEntries();
			m_RGBVol = new int[m_lSize * 4];
			m_XYZCVol = new double[m_lSize * 4];

			/* Get the histogram */
			tree.Print ( m_RGBVol, 0 ); 
			
			for ( ulong i = 0; i < m_lSize; i ++ )
			{
				for ( ulong j = 0; j < 3; j ++ )
				{
					m_XYZCVol[i*4+j] = m_RGBVol[i*4+j] - 128.0;
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Draw pictureBoxes

		// ====================================================================
		//	Description:	Draw User interface, picturebox volume
		//	Return:			void
		private void DrawPBVolume()
		// ====================================================================
		{
			Bitmap bmp = new Bitmap ( pictureBox_Volume.Width, pictureBox_Volume.Height );
			Graphics g = Graphics.FromImage ( bmp );
			Rectangle rect = new Rectangle (0,0, pictureBox_Volume.Width, pictureBox_Volume.Height );
			g.FillRectangle(Brushes.LightGray,0,0,pictureBox_Volume.Width, pictureBox_Volume.Height );
		
			for ( ulong i = 0; i < m_lSize; i ++ )
			{
				int R, G, B;
				R = m_RGBVol[i*4];
				G = m_RGBVol[i*4+1];
				B = m_RGBVol[i*4+2];
				Pen p = new Pen (Color.FromArgb(R,G,B), 1);
				g.DrawEllipse(p, VOL_WIDTH/2+(int)m_XYZCVol[i*4]-1, VOL_WIDTH/2+(int)m_XYZCVol[i*4+1]-1,2,2); 
				//g.FillEllipse(Color.FromArgb(R,G,B), VOL_WIDTH/2+(int)m_XYZCVol[i*4]-1, VOL_WIDTH/2+(int)m_XYZCVol[i*4+1]-1,2,2); 
			}
			g.Dispose();
			pictureBox_Volume.Image = (Image)bmp;
		}

		// ====================================================================
		//	Description:	Draw User interface, picturebox spin
		//	Return:			void
		private void DrawPBSpin( int iPush )
		// ====================================================================
		{
			Bitmap bmp = new Bitmap ( SPIN_WIDTH, SPIN_HEIGHT );
			Graphics g = Graphics.FromImage ( bmp );
			Rectangle rect = new Rectangle (0,0, SPIN_WIDTH, SPIN_HEIGHT );
			g.FillRectangle(Brushes.LightGray,0,0,SPIN_WIDTH, SPIN_HEIGHT );
/*
			Pen p = new Pen ( Color.Gray, 1 );
			g.DrawLine ( p, 0,0,SPIN_WIDTH, SPIN_HEIGHT );
			g.DrawLine ( p, 0,SPIN_HEIGHT ,SPIN_WIDTH, 0 );
			g.DrawLine ( p, SPIN_WIDTH/2,0,SPIN_WIDTH/2, SPIN_HEIGHT );
			g.DrawLine ( p, 0,SPIN_HEIGHT/2,SPIN_WIDTH, SPIN_HEIGHT/2 );
*/			
			Pen P = new Pen ( Color.Black );
			Point up1 = new Point (SPIN_WIDTH/2, 0);
			Point up2 = new Point (SPIN_WIDTH/2-15, SPIN_HEIGHT/2-18);
			Point up3 = new Point (SPIN_WIDTH/2+15, SPIN_HEIGHT/2-18);
			Point[] upPts = {up1, up2, up3};
			//g.DrawPolygon(P, upPts);
			g.FillPolygon(Brushes.Olive, upPts);

			Point dw1 = new Point (SPIN_WIDTH/2, SPIN_HEIGHT);
			Point dw2 = new Point (SPIN_WIDTH/2-15, SPIN_HEIGHT/2+18);
			Point dw3 = new Point (SPIN_WIDTH/2+15, SPIN_HEIGHT/2+18);
			Point[] dwPts = {dw1, dw2, dw3};
			//g.DrawPolygon(P, dwPts);
			g.FillPolygon(Brushes.Olive, dwPts);

			Point lf1 = new Point (0, SPIN_HEIGHT/2);
			Point lf2 = new Point (SPIN_WIDTH/2-18, SPIN_HEIGHT/2-15);
			Point lf3 = new Point (SPIN_WIDTH/2-18, SPIN_HEIGHT/2+15);
			Point[] lfPts = {lf1, lf2, lf3};
			//g.DrawPolygon(P, lfPts);
			g.FillPolygon(Brushes.Olive, lfPts);

			Point rg1 = new Point (SPIN_WIDTH, SPIN_HEIGHT/2);
			Point rg2 = new Point (SPIN_WIDTH/2+18, SPIN_WIDTH/2-15);
			Point rg3 = new Point (SPIN_WIDTH/2+18, SPIN_WIDTH/2+15);
			Point[] rgPts = {rg1, rg2, rg3};
			//g.DrawPolygon(P, rgPts);
			g.FillPolygon(Brushes.Olive, rgPts);

			switch ( iPush )
			{
				case (int)PUSH.UP:
					g.FillPolygon(Brushes.Black, upPts);
					break;

				case (int)PUSH.DOWN:
					g.FillPolygon(Brushes.Black, dwPts);
					break;

				case (int)PUSH.LEFT:
					g.FillPolygon(Brushes.Black, lfPts);
					break;

				case (int)PUSH.RIGHT:
					g.FillPolygon(Brushes.Black, rgPts);
					break;
			};

			g.Dispose();
			pictureBox_Spin.Image = (Image)bmp;
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Rotation

		// ====================================================================
		//	Description:	rotate image
		//	Return:			void
		private void RotateX ( double angle )
		// ====================================================================
		{
			double y, z;
			angle = Degree2Radian(angle);
			for ( ulong i = 0; i < m_lSize; i ++ )
			{
				//m_XYZCVol[i*4] = 1*m_XYZCVol[i*4];
				y = Math.Cos(angle)*m_XYZCVol[i*4+1]-Math.Sin(angle)*m_XYZCVol[i*4+2];
				z = Math.Sin(angle)*m_XYZCVol[i*4+1]+Math.Cos(angle)*m_XYZCVol[i*4+2];
				
				m_XYZCVol[i*4+1] = y;
				m_XYZCVol[i*4+2] = z;
			}
		}

		// ====================================================================
		//	Description:	rotate image
		//	Return:			void
		private void RotateY ( double angle )
		// ====================================================================
		{
			double x, z;
			angle = Degree2Radian(angle);
			for ( ulong i = 0; i < m_lSize; i ++ )
			{
				x = Math.Cos(angle)*m_XYZCVol[i*4]+Math.Sin(angle)*m_XYZCVol[i*4+2];
				//m_XYZCVol[i*4+1] = m_XYZCVol[i*4+1];
				z = Math.Cos(angle)*m_XYZCVol[i*4+2]-Math.Sin(angle)*m_XYZCVol[i*4];
			
				m_XYZCVol[i*4] = x;
				m_XYZCVol[i*4+2] = z;
			}
		}

		// ====================================================================
		//	Description:	rotate image
		//	Return:			void
		private void RotateZ ( double angle )
		// ====================================================================
		{
			double x, y;
			angle = Degree2Radian(angle);
			for ( ulong i = 0; i < m_lSize; i ++ )
			{
				x = Math.Cos(angle)*m_XYZCVol[i*4]-Math.Sin(angle)*m_XYZCVol[i*4+1];
				y = Math.Sin(angle)*m_XYZCVol[i*4]+Math.Cos(angle)*m_XYZCVol[i*4+1];
				//m_XYZCVol[i*4+2] = m_XYZCVol[i*4+2];

				m_XYZCVol[i*4] = x;
				m_XYZCVol[i*4+1] = y;
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Radian / Degree conversion 

		// ====================================================================
		//	Description:	degree to radian conversion
		//	Return:			angle in radian
		private double Degree2Radian(double angle)
		// ====================================================================
		{
			return ( angle * Math.PI / 180.0 );
		}

		// ====================================================================
		//	Description:	radian to degree conversion
		//	Return:			angle in degrees
		private double Radian2Degree(double angle)
		// ====================================================================
		{
			return ( angle * 180.0 / Math.PI );
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		//
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
			this.pictureBox_Volume = new System.Windows.Forms.PictureBox();
			this.pictureBox_Spin = new System.Windows.Forms.PictureBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textBox_Count = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox_B = new System.Windows.Forms.TextBox();
			this.textBox_G = new System.Windows.Forms.TextBox();
			this.textBox_R = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox_Volume
			// 
			this.pictureBox_Volume.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox_Volume.Location = new System.Drawing.Point(16, 24);
			this.pictureBox_Volume.Name = "pictureBox_Volume";
			this.pictureBox_Volume.Size = new System.Drawing.Size(362, 362);
			this.pictureBox_Volume.TabIndex = 0;
			this.pictureBox_Volume.TabStop = false;
			// 
			// pictureBox_Spin
			// 
			this.pictureBox_Spin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox_Spin.Location = new System.Drawing.Point(392, 24);
			this.pictureBox_Spin.Name = "pictureBox_Spin";
			this.pictureBox_Spin.Size = new System.Drawing.Size(75, 75);
			this.pictureBox_Spin.TabIndex = 1;
			this.pictureBox_Spin.TabStop = false;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.textBox_Count);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.textBox_B);
			this.groupBox1.Controls.Add(this.textBox_G);
			this.groupBox1.Controls.Add(this.textBox_R);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Location = new System.Drawing.Point(392, 144);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(88, 176);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Selection";
			// 
			// textBox_Count
			// 
			this.textBox_Count.Location = new System.Drawing.Point(8, 136);
			this.textBox_Count.Name = "textBox_Count";
			this.textBox_Count.Size = new System.Drawing.Size(64, 20);
			this.textBox_Count.TabIndex = 6;
			this.textBox_Count.Text = "textBox4";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(16, 23);
			this.label3.TabIndex = 5;
			this.label3.Text = "B";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(16, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "G";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(16, 23);
			this.label1.TabIndex = 3;
			this.label1.Text = "R";
			// 
			// textBox_B
			// 
			this.textBox_B.Location = new System.Drawing.Point(32, 88);
			this.textBox_B.Name = "textBox_B";
			this.textBox_B.Size = new System.Drawing.Size(40, 20);
			this.textBox_B.TabIndex = 2;
			this.textBox_B.Text = "textBox3";
			// 
			// textBox_G
			// 
			this.textBox_G.Location = new System.Drawing.Point(32, 56);
			this.textBox_G.Name = "textBox_G";
			this.textBox_G.Size = new System.Drawing.Size(40, 20);
			this.textBox_G.TabIndex = 1;
			this.textBox_G.Text = "textBox2";
			// 
			// textBox_R
			// 
			this.textBox_R.Location = new System.Drawing.Point(32, 24);
			this.textBox_R.Name = "textBox_R";
			this.textBox_R.Size = new System.Drawing.Size(40, 20);
			this.textBox_R.TabIndex = 0;
			this.textBox_R.Text = "textBox_R";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 120);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 23);
			this.label4.TabIndex = 3;
			this.label4.Text = "Count";
			// 
			// VolumeHistogram
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(496, 414);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.pictureBox_Spin);
			this.Controls.Add(this.pictureBox_Volume);
			this.Name = "VolumeHistogram";
			this.Text = "VolumeHistogram";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
