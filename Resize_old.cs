// ============================================================================
// Module:		Resize
//
// Description:	adjust image size, larger or smaller
//
// Purpose:		An image processing application
//
// Input:		A raster image
// Output:		A raster image
//
// Author:		Chi Toung Yeung			cty
//
// History:
// 17Feb07		Started work - mostly working except zero conditions for x,y	cty
// 18Feb07		Adding OnChange Methods()										cty	
// 18Feb07		All UI complete, need to fix bilinear algorithm					cty
// 18Feb07		All working but not efficient									cty
// 20Feb07		Convert all image pixel arithemtics to unsafe code.
//				- using pointers to directly access pixel values in mem.		cty
// 23Feb07		Fixed x, y boundary calculation in resizing algorithm			cty
// 11Mar07		Fixed Apply24bpp() the pixel format was incorrect at initial	cty
// ============================================================================
using System;
using System.Drawing;
using System.Drawing.Imaging;					// bitmap data

using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Viewer
{
	/// <summary>
	/// Summary description for Resize.
	/// </summary>
	public class Resize : System.Windows.Forms.Form
	{
		/* Variables declaration */
		public ImageView m_Orig;									// point to the original
		public Bitmap m_Nbmp;										// new resized image
		private double m_dWid, m_dLen;								
		
		private System.Windows.Forms.GroupBox Original;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.TextBox textBox_OrigLen;
		private System.Windows.Forms.TextBox textBox_OrigWid;
		private System.Windows.Forms.TextBox textBox_NewLen;
		private System.Windows.Forms.TextBox textBox_NewWid;
		private System.Windows.Forms.CheckBox checkBox_AspectRatio;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// ///////////////////////////////////////////////////////////////////////////////////////
		// Public - UI

		// ====================================================================
		// Description: Constructor - initialize the object
		public Resize( ImageView fm )
		// ====================================================================
		{
			InitializeComponent();

			m_Orig = fm;
			this.Text = fm.Text;

			m_dWid = m_Orig.m_Img.Width;
			m_dLen = m_Orig.m_Img.Height;
			
			checkBox_AspectRatio.Checked = true;

			button_OK.Click += new EventHandler ( OnOK );
			button_Cancel.Click += new EventHandler ( OnCancel );

			textBox_OrigLen.Text = Convert.ToString ( m_Orig.m_Img.Height );
			textBox_OrigWid.Text = Convert.ToString ( m_Orig.m_Img.Width );

			textBox_NewLen.Text = Convert.ToString ( m_Orig.m_Img.Height );
			textBox_NewWid.Text = Convert.ToString ( m_Orig.m_Img.Width );
		
			textBox_NewWid.TextChanged += new EventHandler ( OnNewWidChanged );
			textBox_NewLen.TextChanged += new EventHandler ( OnNewLenChanged );

			checkBox_AspectRatio.Click += new EventHandler ( OnAspectRatioClick );
		}

		// ====================================================================
		// Description:	Commit to the convolution change on original image
		// Return:		void
		public void OnOK (object sender
							, EventArgs e)
		// ====================================================================
		{
			if ( ( (int)m_dWid == m_Orig.m_Img.Width ) && ( (int)m_dLen == m_Orig.m_Img.Height ) )
			{
				string msg = "No resizing done";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				MessageBox.Show (this, msg, null, buttons,
					MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
					MessageBoxOptions.RightAlign);
				return;
			}

			if ( ( m_dWid < 1 ) || ( m_dLen < 1 ) )
			{
				string msg = "Bad size";
				MessageBoxButtons buttons = MessageBoxButtons.OK;
				MessageBox.Show (this, msg, null, buttons,
					MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 
					MessageBoxOptions.RightAlign);
				return;
			}

			switch ( m_Orig.m_Img.PixelFormat )
			{
				case PixelFormat.Format24bppRgb:
					Apply_24bpp();
					break;

				default:
					Apply_default();
					break;
			};
			m_Orig.OnInitForm ( m_Orig.Text, m_Nbmp );
			OnCancel (sender, e);
		}

		// ====================================================================
		//	Description:	Cancel depressed
		//	Return:			void
		public void OnCancel (object sender
								, EventArgs e)
		// ====================================================================
		{
			this.Dispose();
		}

		// ====================================================================
		//	Description:	Aspect Ratio
		//	Return:			void
		public void OnNewWidChanged (object sender
									, EventArgs e)
		// ====================================================================
		{
			double dLen;
			if ( textBox_NewWid.Text.Length != 0 )
			{
				m_dWid = double.Parse ( textBox_NewWid.Text );

				if ( checkBox_AspectRatio.Checked )
				{
					dLen = ((double)m_dWid / (double)m_Orig.m_Img.Width * (double)m_Orig.m_Img.Height);
					if ( dLen != m_dLen )
						textBox_NewLen.Text = Convert.ToString( dLen );
				}
			}
		}
		
		// ====================================================================
		//	Description:	Aspect Ratio
		//	Return:			void
		public void OnNewLenChanged (object sender
									, EventArgs e)
		// ====================================================================
		{
			double dWid;
			if ( textBox_NewLen.Text.Length != 0 )
			{
				m_dLen = double.Parse ( textBox_NewLen.Text );

				if ( checkBox_AspectRatio.Checked )
				{
					dWid = ((double)m_dLen / (double)m_Orig.m_Img.Height * (double)m_Orig.m_Img.Width);
					if ( dWid != m_dWid )
						textBox_NewWid.Text = Convert.ToString( dWid );
				}
			}
		}

		// ====================================================================
		//	Description:	Aspect Ratio clicked
		//	Return:			void
		public void OnAspectRatioClick (object sender
										, EventArgs e)
		// ====================================================================
		{
			if ( checkBox_AspectRatio.Checked == true )
			{
				double dLen = m_dWid / m_Orig.m_Img.Width * m_Orig.m_Img.Height;
				if ( dLen != m_dLen )
					textBox_NewLen.Text = Convert.ToString( dLen );		
			}
		}

		// ====================================================================
		// Description:		Apply bilinear resizing
		//
		// Return:			true if successful
		//					false if failed													
		public bool Apply_24bpp()
		// ====================================================================		
		{	
			/* This method should be optimized !!!
			 * I should be able to walk through all pixels in a single pass.
			 * Also, my method of generating the bilinear matrix should be optimized */
			

			this.Cursor = Cursors.WaitCursor;

			double [] matrix = new double[4];
			double [] p = new double[3];								

			Bitmap Obmp = (Bitmap)m_Orig.m_Img;
			m_Nbmp = new Bitmap ( (int)m_dWid, (int)m_dLen, PixelFormat.Format24bppRgb );

			BitmapData Odata = Obmp.LockBits ( new Rectangle (0,0, Obmp.Width, Obmp.Height ), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb );

			BitmapData Ndata = m_Nbmp.LockBits ( new Rectangle (0,0, m_Nbmp.Width, m_Nbmp.Height ), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb );

			/* keep us from walking out of bound */
			int xlim = (int)(m_dWid / (double)m_Orig.m_Img.Width);
			int ylim = (int)(m_dLen / (double)m_Orig.m_Img.Height);
			
			/* resize image - main body*/
			int x, y;
			double R, G, B;
			double[] pClr1 = new double[3];
			double[] pClr2 = new double[3];
			double[] pClr3 = new double[3];
			double[] pClr4 = new double[3];

 
			unsafe
			{
				byte *Nptr;
				byte *Optr;
				
				/* resize the majority pixels
				 * - inside boundaries */
				for ( y = 0; y < (int)m_dLen-ylim; y ++ )
				{
					Nptr = (byte*)(Ndata.Scan0) + Ndata.Stride * y;
					for ( x = 0; x < (int)m_dWid-xlim; x ++ )
					{
						/* Point on the original image */
						p[0] = (double)x * (double)m_Orig.m_Img.Width / m_dWid;
						p[1] = (double)y * (double)m_Orig.m_Img.Height / m_dLen;
						
						/* x axis points */
						matrix[0] = 1.0 - (double)( p[0] % 1.0 );
						matrix[1] = (double)( p[0] % 1.0 );
						matrix[2] = 1.0 - (double)( p[0] % 1.0 );
						matrix[3] = (double)( p[0] % 1.0 );
						
						/* y axis points */
						matrix[0] = matrix[0] * ( 1.0 - (double)( p[1] % 1.0 ) );
						matrix[1] = matrix[1] * ( 1.0 - (double)( p[1] % 1.0 ) );
						matrix[2] = matrix[2] * (double)( p[1] % 1.0 );
						matrix[3] = matrix[3] * (double)( p[1] % 1.0 );

						/* Get the neighborhood pixels */	
						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (int)p[1] + (int)p[0]*3;
						pClr1[2] = *Optr; Optr ++;
						pClr1[1] = *Optr; Optr ++;
						pClr1[0] = *Optr; Optr ++;
						pClr2[2] = *Optr; Optr ++;
						pClr2[1] = *Optr; Optr ++;
						pClr2[0] = *Optr; 

						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (int)(p[1]+1) + (int)p[0]*3;
						pClr3[2] = *Optr; Optr ++;
						pClr3[1] = *Optr; Optr ++;
						pClr3[0] = *Optr; Optr ++;
						pClr4[2] = *Optr; Optr ++;
						pClr4[1] = *Optr; Optr ++;
						pClr4[0] = *Optr; 

						/* Calculate the pixel's color */
						R = matrix[0] * pClr1[0] + matrix[1] * pClr2[0] + matrix[2] * pClr3[0] + matrix[3] * pClr4[0];
						G = matrix[0] * pClr1[1] + matrix[1] * pClr2[1] + matrix[2] * pClr3[1] + matrix[3] * pClr4[1];
						B = matrix[0] * pClr1[2] + matrix[1] * pClr2[2] + matrix[2] * pClr3[2] + matrix[3] * pClr4[2];

						*Nptr = (byte)B; Nptr ++;
						*Nptr = (byte)G; Nptr ++;
						*Nptr = (byte)R; Nptr ++;
					}
				}

				/* resize image x axis border */
				for ( y = (int)m_dLen-ylim; y < (int)m_dLen; y ++ )
				{
					Nptr = (byte*)(Ndata.Scan0) + Ndata.Stride * y;
					for ( x = 0; x < (int)m_dWid-xlim; x ++ )
					{
						p[0] = (double)x * (double)m_Orig.m_Img.Width / m_dWid;
						
						matrix[0] = 1.0 - (double)( p[0] % 1.0 );
						matrix[1] = (double)( p[0] % 1.0 );

						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (m_Orig.m_Img.Height-1) + (int)p[0]*3;
						pClr1[2] = *Optr; Optr ++;
						pClr1[1] = *Optr; Optr ++;
						pClr1[0] = *Optr; Optr ++;
						pClr2[2] = *Optr; Optr ++;
						pClr2[1] = *Optr; Optr ++;
						pClr2[0] = *Optr; 

						R = matrix[0] * pClr1[0] + matrix[1] * pClr2[0];
						G = matrix[0] * pClr1[1] + matrix[1] * pClr2[1];
						B = matrix[0] * pClr1[2] + matrix[1] * pClr2[2];

						*Nptr = (byte)B; Nptr ++;
						*Nptr = (byte)G; Nptr ++;
						*Nptr = (byte)R; Nptr ++;
					}
				}

				/* resize image y axis border */
				for ( y = 0; y < (int)m_dLen-ylim; y ++ )
				{
					Nptr = (byte*)(Ndata.Scan0) + Ndata.Stride * y + ( (int)m_dWid-xlim ) * 3;
					for ( x = (int)m_dWid-xlim; x < (int)m_dWid; x ++ )
					{
						p[1] = (double)y * (double)m_Orig.m_Img.Height / m_dLen;
						
						matrix[1] = ( 1.0 - (double)( p[1] % 1.0 ) );
						matrix[2] = (double)( p[1] % 1.0 );

						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (int)p[1] + (m_Orig.m_Img.Width-1)*3;
						pClr1[2] = *Optr; Optr ++;
						pClr1[1] = *Optr; Optr ++;
						pClr1[0] = *Optr; 

						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (int)(p[1]+1) + (m_Orig.m_Img.Width-1)*3;
						pClr3[2] = *Optr; Optr ++;
						pClr3[1] = *Optr; Optr ++;
						pClr3[0] = *Optr; 

						/* Calculate the pixel's color */
						R = matrix[1] * pClr1[0] + matrix[2] * pClr3[0];
						G = matrix[1] * pClr1[1] + matrix[2] * pClr3[1];
						B = matrix[1] * pClr1[2] + matrix[2] * pClr3[2];

						*Nptr = (byte)B; Nptr ++;
						*Nptr = (byte)G; Nptr ++;
						*Nptr = (byte)R; Nptr ++;
					}
				}

				/* resize remainder */
				for ( y = (int)m_dLen-ylim; y < (int)m_dLen;  y ++ )
				{
					Nptr = (byte*)(Ndata.Scan0) + y * Ndata.Stride + ((int)m_dWid-xlim) * 3;
					for ( x = (int)m_dWid-xlim; x < (int)m_dWid; x ++ )
					{
						p[0] = (double)x * (double)m_Orig.m_Img.Width / m_dWid;
						p[1] = (double)y * (double)m_Orig.m_Img.Height / m_dLen;

						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (int)p[1] + (int)p[0]*3;

						*Nptr = *Optr;	Optr ++;
						*Nptr = *Optr;	Optr ++;
						*Nptr = *Optr;	Optr ++;
					}
				}
			}
			Obmp.UnlockBits(Odata);
			m_Nbmp.UnlockBits(Ndata);

			this.Cursor = Cursors.Default;			
			return true;
		}

		// ====================================================================
		// Description:		Apply bilinear resizing
		//
		// Return:			true if successful
		//					false if failed													
		public bool Apply_default()
			// ====================================================================		
		{	
			/* This method should be optimized !!!
			 * I should be able to walk through all pixels in a single pass.
			 * Also, my method of generating the bilinear matrix should be optimized */

			this.Cursor = Cursors.WaitCursor;

			double [] matrix = new double[4];
			double [] p = new double[3];								

			Bitmap Obmp = (Bitmap)m_Orig.m_Img;
			m_Nbmp = new Bitmap ( (int)m_dWid, (int)m_dLen, Obmp.PixelFormat );

			BitmapData Odata = Obmp.LockBits ( new Rectangle (0,0, Obmp.Width, Obmp.Height ), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb );

			BitmapData Ndata = m_Nbmp.LockBits ( new Rectangle (0,0, m_Nbmp.Width, m_Nbmp.Height ), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb );

			/* keep us from walking out of bound */
			int xlim = (int)(m_dWid / (double)m_Orig.m_Img.Width);
			int ylim = (int)(m_dLen / (double)m_Orig.m_Img.Height);
			
			/* resize image - main body*/
			int x, y;
			double R, G, B;
			double[] pClr1 = new double[3];
			double[] pClr2 = new double[3];
			double[] pClr3 = new double[3];
			double[] pClr4 = new double[3];

 
			unsafe
			{
				byte *Nptr;
				byte *Optr;
				
				/* resize the majority pixels
				 * - inside boundaries */
				for ( y = 0; y < (int)m_dLen-ylim; y ++ )
				{
					Nptr = (byte*)(Ndata.Scan0) + Ndata.Stride * y;
					for ( x = 0; x < (int)m_dWid-xlim; x ++ )
					{
						/* Point on the original image */
						p[0] = (double)x * (double)m_Orig.m_Img.Width / m_dWid;
						p[1] = (double)y * (double)m_Orig.m_Img.Height / m_dLen;
						
						/* x axis points */
						matrix[0] = 1.0 - (double)( p[0] % 1.0 );
						matrix[1] = (double)( p[0] % 1.0 );
						matrix[2] = 1.0 - (double)( p[0] % 1.0 );
						matrix[3] = (double)( p[0] % 1.0 );
						
						/* y axis points */
						matrix[0] = matrix[0] * ( 1.0 - (double)( p[1] % 1.0 ) );
						matrix[1] = matrix[1] * ( 1.0 - (double)( p[1] % 1.0 ) );
						matrix[2] = matrix[2] * (double)( p[1] % 1.0 );
						matrix[3] = matrix[3] * (double)( p[1] % 1.0 );

						/* Get the neighborhood pixels */	
						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (int)p[1] + (int)p[0]*3;
						pClr1[2] = *Optr; Optr ++;
						pClr1[1] = *Optr; Optr ++;
						pClr1[0] = *Optr; Optr ++;
						pClr2[2] = *Optr; Optr ++;
						pClr2[1] = *Optr; Optr ++;
						pClr2[0] = *Optr; 

						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (int)(p[1]+1) + (int)p[0]*3;
						pClr3[2] = *Optr; Optr ++;
						pClr3[1] = *Optr; Optr ++;
						pClr3[0] = *Optr; Optr ++;
						pClr4[2] = *Optr; Optr ++;
						pClr4[1] = *Optr; Optr ++;
						pClr4[0] = *Optr; 

						/* Calculate the pixel's color */
						R = matrix[0] * pClr1[0] + matrix[1] * pClr2[0] + matrix[2] * pClr3[0] + matrix[3] * pClr4[0];
						G = matrix[0] * pClr1[1] + matrix[1] * pClr2[1] + matrix[2] * pClr3[1] + matrix[3] * pClr4[1];
						B = matrix[0] * pClr1[2] + matrix[1] * pClr2[2] + matrix[2] * pClr3[2] + matrix[3] * pClr4[2];

						*Nptr = (byte)B; Nptr ++;
						*Nptr = (byte)G; Nptr ++;
						*Nptr = (byte)R; Nptr ++;
					}
				}

				/* resize image x axis border */
				for ( y = (int)m_dLen-ylim; y < (int)m_dLen; y ++ )
				{
					Nptr = (byte*)(Ndata.Scan0) + Ndata.Stride * y;
					for ( x = 0; x < (int)m_dWid-xlim; x ++ )
					{
						p[0] = (double)x * (double)m_Orig.m_Img.Width / m_dWid;
						
						matrix[0] = 1.0 - (double)( p[0] % 1.0 );
						matrix[1] = (double)( p[0] % 1.0 );

						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (m_Orig.m_Img.Height-1) + (int)p[0]*3;
						pClr1[2] = *Optr; Optr ++;
						pClr1[1] = *Optr; Optr ++;
						pClr1[0] = *Optr; Optr ++;
						pClr2[2] = *Optr; Optr ++;
						pClr2[1] = *Optr; Optr ++;
						pClr2[0] = *Optr; 

						R = matrix[0] * pClr1[0] + matrix[1] * pClr2[0];
						G = matrix[0] * pClr1[1] + matrix[1] * pClr2[1];
						B = matrix[0] * pClr1[2] + matrix[1] * pClr2[2];

						*Nptr = (byte)B; Nptr ++;
						*Nptr = (byte)G; Nptr ++;
						*Nptr = (byte)R; Nptr ++;
					}
				}

				/* resize image y axis border */
				for ( y = 0; y < (int)m_dLen-ylim; y ++ )
				{
					Nptr = (byte*)(Ndata.Scan0) + Ndata.Stride * y + ( (int)m_dWid-xlim ) * 3;
					for ( x = (int)m_dWid-xlim; x < (int)m_dWid; x ++ )
					{
						p[1] = (double)y * (double)m_Orig.m_Img.Height / m_dLen;
						
						matrix[1] = ( 1.0 - (double)( p[1] % 1.0 ) );
						matrix[2] = (double)( p[1] % 1.0 );

						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (int)p[1] + (m_Orig.m_Img.Width-1)*3;
						pClr1[2] = *Optr; Optr ++;
						pClr1[1] = *Optr; Optr ++;
						pClr1[0] = *Optr; 

						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (int)(p[1]+1) + (m_Orig.m_Img.Width-1)*3;
						pClr3[2] = *Optr; Optr ++;
						pClr3[1] = *Optr; Optr ++;
						pClr3[0] = *Optr; 

						/* Calculate the pixel's color */
						R = matrix[1] * pClr1[0] + matrix[2] * pClr3[0];
						G = matrix[1] * pClr1[1] + matrix[2] * pClr3[1];
						B = matrix[1] * pClr1[2] + matrix[2] * pClr3[2];

						*Nptr = (byte)B; Nptr ++;
						*Nptr = (byte)G; Nptr ++;
						*Nptr = (byte)R; Nptr ++;
					}
				}

				/* resize remainder */
				for ( y = (int)m_dLen-ylim; y < (int)m_dLen;  y ++ )
				{
					Nptr = (byte*)(Ndata.Scan0) + y * Ndata.Stride + ((int)m_dWid-xlim) * 3;
					for ( x = (int)m_dWid-xlim; x < (int)m_dWid; x ++ )
					{
						p[0] = (double)x * (double)m_Orig.m_Img.Width / m_dWid;
						p[1] = (double)y * (double)m_Orig.m_Img.Height / m_dLen;

						Optr = (byte*)(Odata.Scan0) + Odata.Stride * (int)p[1] + (int)p[0]*3;

						*Nptr = *Optr;	Optr ++;
						*Nptr = *Optr;	Optr ++;
						*Nptr = *Optr;	Optr ++;
					}
				}
			}
			Obmp.UnlockBits(Odata);
			m_Nbmp.UnlockBits(Ndata);

			this.Cursor = Cursors.Default;			
			return true;
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
			this.Original = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox_OrigLen = new System.Windows.Forms.TextBox();
			this.textBox_OrigWid = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.textBox_NewLen = new System.Windows.Forms.TextBox();
			this.textBox_NewWid = new System.Windows.Forms.TextBox();
			this.button_OK = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.button_Cancel = new System.Windows.Forms.Button();
			this.checkBox_AspectRatio = new System.Windows.Forms.CheckBox();
			this.Original.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// Original
			// 
			this.Original.Controls.Add(this.label4);
			this.Original.Controls.Add(this.label3);
			this.Original.Controls.Add(this.label2);
			this.Original.Controls.Add(this.label1);
			this.Original.Controls.Add(this.textBox_OrigLen);
			this.Original.Controls.Add(this.textBox_OrigWid);
			this.Original.Location = new System.Drawing.Point(16, 16);
			this.Original.Name = "Original";
			this.Original.TabIndex = 0;
			this.Original.TabStop = false;
			this.Original.Text = "Original Image";
			this.Original.Enter += new System.EventHandler(this.Original_Enter);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(152, 64);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 23);
			this.label4.TabIndex = 5;
			this.label4.Text = "pixels";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(152, 32);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(40, 23);
			this.label3.TabIndex = 4;
			this.label3.Text = "pixels";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 23);
			this.label2.TabIndex = 3;
			this.label2.Text = "Height";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 23);
			this.label1.TabIndex = 2;
			this.label1.Text = "Width";
			// 
			// textBox_OrigLen
			// 
			this.textBox_OrigLen.Location = new System.Drawing.Point(72, 56);
			this.textBox_OrigLen.Name = "textBox_OrigLen";
			this.textBox_OrigLen.ReadOnly = true;
			this.textBox_OrigLen.Size = new System.Drawing.Size(72, 20);
			this.textBox_OrigLen.TabIndex = 1;
			this.textBox_OrigLen.Text = "textBox2";
			// 
			// textBox_OrigWid
			// 
			this.textBox_OrigWid.Location = new System.Drawing.Point(72, 24);
			this.textBox_OrigWid.Name = "textBox_OrigWid";
			this.textBox_OrigWid.ReadOnly = true;
			this.textBox_OrigWid.Size = new System.Drawing.Size(72, 20);
			this.textBox_OrigWid.TabIndex = 0;
			this.textBox_OrigWid.Text = "textBox1";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.textBox_NewLen);
			this.groupBox1.Controls.Add(this.textBox_NewWid);
			this.groupBox1.Location = new System.Drawing.Point(16, 128);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "New Image";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(152, 64);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(40, 23);
			this.label5.TabIndex = 5;
			this.label5.Text = "pixels";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(152, 32);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 23);
			this.label6.TabIndex = 4;
			this.label6.Text = "pixels";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(24, 64);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(40, 23);
			this.label7.TabIndex = 3;
			this.label7.Text = "Height";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(24, 32);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(40, 23);
			this.label8.TabIndex = 2;
			this.label8.Text = "Width";
			// 
			// textBox_NewLen
			// 
			this.textBox_NewLen.Location = new System.Drawing.Point(72, 56);
			this.textBox_NewLen.Name = "textBox_NewLen";
			this.textBox_NewLen.Size = new System.Drawing.Size(72, 20);
			this.textBox_NewLen.TabIndex = 1;
			this.textBox_NewLen.Text = "textBox2";
			// 
			// textBox_NewWid
			// 
			this.textBox_NewWid.Location = new System.Drawing.Point(72, 24);
			this.textBox_NewWid.Name = "textBox_NewWid";
			this.textBox_NewWid.Size = new System.Drawing.Size(72, 20);
			this.textBox_NewWid.TabIndex = 0;
			this.textBox_NewWid.Text = "textBox1";
			// 
			// button_OK
			// 
			this.button_OK.Location = new System.Drawing.Point(16, 24);
			this.button_OK.Name = "button_OK";
			this.button_OK.TabIndex = 7;
			this.button_OK.Text = "OK";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.button_OK);
			this.groupBox2.Controls.Add(this.button_Cancel);
			this.groupBox2.Location = new System.Drawing.Point(232, 16);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(112, 100);
			this.groupBox2.TabIndex = 8;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Action";
			// 
			// button_Cancel
			// 
			this.button_Cancel.Location = new System.Drawing.Point(16, 56);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.TabIndex = 9;
			this.button_Cancel.Text = "Cancel";
			// 
			// checkBox_AspectRatio
			// 
			this.checkBox_AspectRatio.Location = new System.Drawing.Point(240, 136);
			this.checkBox_AspectRatio.Name = "checkBox_AspectRatio";
			this.checkBox_AspectRatio.TabIndex = 0;
			this.checkBox_AspectRatio.Text = "Aspect Ratio";
			// 
			// Resize
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(368, 254);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.Original);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.checkBox_AspectRatio);
			this.Name = "Resize";
			this.Text = "Resize";
			this.Original.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void Original_Enter(object sender, System.EventArgs e)
		{
		
		}
	}
}
