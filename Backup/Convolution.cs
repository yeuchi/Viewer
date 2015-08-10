// ============================================================================
// Module:		Convolution
//
// Description:	for blur, edge detection and parent class of unsharpmask
//
// Purpose:		An image processing application
//
// Input:		A raster image
// Output:		A raster image
//
// Author:		Chi Toung Yeung			cty
//
// History:	
// 12Feb07		Start														cty
// 13Feb07		All but custom working, UI needs work						cty
// 14Feb07		1st all features working, no error trapping					cty 
// 14Feb07		Some fixes for custom initialization						cty
// 18Feb07		Added color to the Kernel UI								cty
// 19Feb07		Preview image is made the same size as the original image	cty
// 19Feb07		Preview start position is 1/2 offset from original			cty
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
	enum Kernel:int{Custom, Identity, Lowpass, Highpass, Laplacian, Sobel_X, Sobel_Y, Roberts45, Roberts135, Prewitt_X, Prewitt_Y };
	/// <summary>
	/// Summary description for Convolution.
	/// </summary>
	public class Convolution : System.Windows.Forms.Form
	{			
		/* Variables declaration */
		public ImageView m_Orig;									// point to the original
		public ImageView m_prv;										// a preview image window
	
		double   m_dKernTtl;
		double[] m_pKern;											// array to hold kernel
		int		 m_iKernWid;										// kernel width

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox textBoxR1C1;
		private System.Windows.Forms.TextBox textBoxR1C2;
		private System.Windows.Forms.TextBox textBoxR1C3;
		private System.Windows.Forms.TextBox textBoxR1C4;
		private System.Windows.Forms.TextBox textBoxR1C5;
		private System.Windows.Forms.TextBox textBoxR1C6;
		private System.Windows.Forms.TextBox textBoxR1C7;
		private System.Windows.Forms.TextBox textBoxR2C7;
		private System.Windows.Forms.TextBox textBoxR2C6;
		private System.Windows.Forms.TextBox textBoxR2C5;
		private System.Windows.Forms.TextBox textBoxR2C4;
		private System.Windows.Forms.TextBox textBoxR2C3;
		private System.Windows.Forms.TextBox textBoxR2C2;
		private System.Windows.Forms.TextBox textBoxR2C1;
		private System.Windows.Forms.TextBox textBoxR3C7;
		private System.Windows.Forms.TextBox textBoxR3C6;
		private System.Windows.Forms.TextBox textBoxR3C5;
		private System.Windows.Forms.TextBox textBoxR3C4;
		private System.Windows.Forms.TextBox textBoxR3C3;
		private System.Windows.Forms.TextBox textBoxR3C2;
		private System.Windows.Forms.TextBox textBoxR3C1;
		private System.Windows.Forms.TextBox textBoxR4C3;
		private System.Windows.Forms.TextBox textBoxR4C2;
		private System.Windows.Forms.TextBox textBoxR4C1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.TextBox textBoxR5C1;
		private System.Windows.Forms.TextBox textBoxR6C1;
		private System.Windows.Forms.TextBox textBoxR7C1;
		private System.Windows.Forms.TextBox textBoxR4C7;
		private System.Windows.Forms.TextBox textBoxR4C6;
		private System.Windows.Forms.TextBox textBoxR4C5;
		private System.Windows.Forms.TextBox textBoxR4C4;
		private System.Windows.Forms.TextBox textBoxR5C7;
		private System.Windows.Forms.TextBox textBoxR5C6;
		private System.Windows.Forms.TextBox textBoxR5C5;
		private System.Windows.Forms.TextBox textBoxR5C4;
		private System.Windows.Forms.TextBox textBoxR5C3;
		private System.Windows.Forms.TextBox textBoxR5C2;
		private System.Windows.Forms.TextBox textBoxR6C7;
		private System.Windows.Forms.TextBox textBoxR6C6;
		private System.Windows.Forms.TextBox textBoxR6C5;
		private System.Windows.Forms.TextBox textBoxR6C4;
		private System.Windows.Forms.TextBox textBoxR6C3;
		private System.Windows.Forms.TextBox textBoxR6C2;
		private System.Windows.Forms.TextBox textBoxR7C7;
		private System.Windows.Forms.TextBox textBoxR7C6;
		private System.Windows.Forms.TextBox textBoxR7C5;
		private System.Windows.Forms.TextBox textBoxR7C4;
		private System.Windows.Forms.TextBox textBoxR7C3;
		private System.Windows.Forms.TextBox textBoxR7C2;
		private System.Windows.Forms.TextBox textBoxDivider;
		private System.Windows.Forms.ComboBox comboBoxType;
		private System.Windows.Forms.Button button_Apply;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// ///////////////////////////////////////////////////////////////////////////////////////
		// Public - UI

		// ====================================================================
		// Description: Constructor - initialize the object
		public Convolution()
		// ====================================================================
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			comboBoxType.SelectedIndexChanged += new EventHandler ( OnComboClick );
			button_Apply.Click += new EventHandler ( OnApply );
			button_OK.Click += new EventHandler ( OnOK );
			button_Cancel.Click += new EventHandler ( OnCancel );
			
			comboBoxType.Items.Clear();
			comboBoxType.Items.Add ("Custom");
			comboBoxType.Items.Add ("Identity");
			comboBoxType.Items.Add ("Low-pass");
			comboBoxType.Items.Add ("High-pass");
			comboBoxType.Items.Add ("Laplacian");
			comboBoxType.Items.Add ("Sobel X");
			comboBoxType.Items.Add ("Sobel Y");
			comboBoxType.Items.Add ("Roberts 45");
			comboBoxType.Items.Add ("Roberts 135");
			comboBoxType.Items.Add ("Prewitt X");
			comboBoxType.Items.Add ("Prewitt Y");
		}

		// ====================================================================
		// Description:	Initialized the object
		// Return:		void
		public void OnInitForm ( ImageView fm,  // [in] image object
								 int iMethod )	// [in] kernel type
		// ====================================================================
		{
			m_Orig = fm;
			this.Text = fm.Text;

			/* Show a preview image */
			if ( m_prv != null)
				m_prv.Dispose();

			m_prv = new ImageView();
			m_prv.MdiParent = this.ParentForm;
			m_prv.OnInitForm ( "Preview", (Bitmap)fm.m_Img );
			m_prv.OnFormSize ( m_Orig.Width-m_Orig.XPADD, m_Orig.Height-m_Orig.YPADD );
			m_prv.Show();
	
			comboBoxType.SelectedIndex = iMethod;
		}

		// ====================================================================
		// Description:	apply convolution on preview image
		// Return:		void
		public void OnApply(object sender
							, EventArgs e)
		// ====================================================================
		{		
			Apply();
		}

		// ====================================================================
		// Description:	Commit to the convolution change on original image
		// Return:		void
		public void OnOK (object sender
							, EventArgs e)
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

		// ====================================================================
		// Description:	Convolution method changed by combo box selection
		// Return:		void
		public void OnComboClick(object sender
								, EventArgs e)
		// ====================================================================
		{
			switch ( (int)comboBoxType.SelectedIndex )
			{
				case (int)Kernel.Custom:
					if ( ( m_pKern == null ) || ( m_iKernWid <= 0 ) || ( m_iKernWid % 2 == 0 ) )
						SetKernel ( (int)comboBoxType.SelectedIndex );
					else
						SetKernel ( m_iKernWid, m_pKern );
					break;

				default:
					SetKernel ( (int)comboBoxType.SelectedIndex );
					break;
			};

			Apply();
		}

		/// ///////////////////////////////////////////////////////////////////////////////////////
		// Public 

		// ====================================================================
		// Description:	Set kernel
		// Return:		true if success
		//				false if failed
		public bool SetKernel ( int iMethod )
		// ====================================================================
		{
			m_iKernWid = 3;
			m_pKern = new double[m_iKernWid*m_iKernWid];
			int i;
			for ( i = 0; i < m_iKernWid*m_iKernWid; i ++ )
				m_pKern[i] = 0;

			switch ( iMethod )
			{
				case (int)Kernel.Custom:
				case (int)Kernel.Identity:
					m_iKernWid = 1;
					m_pKern = new double[1];
					m_pKern[0] = 1;
					break;

				case (int)Kernel.Highpass:
					for ( i = 0; i < m_iKernWid*m_iKernWid; i ++ )
						m_pKern[i] = -1;
					m_pKern[4] = 8;
					break;

				case (int)Kernel.Laplacian:
					m_pKern[1] = -1;	m_pKern[3] = -1;
					m_pKern[5] = -1;	m_pKern[7] = -1;
					m_pKern[4] = 4;
					break;

				case (int)Kernel.Lowpass:
					m_iKernWid = 5;
					m_pKern = new double[m_iKernWid*m_iKernWid];
					for ( i = 0; i < m_iKernWid*m_iKernWid; i ++ )
						m_pKern[i] = 1;
					break;

				case (int)Kernel.Prewitt_X:
					m_pKern[0] = -1;	m_pKern[2] = 1;
					m_pKern[3] = -1;	m_pKern[5] = 1;
					m_pKern[6] = -1;	m_pKern[8] = 1;
					break;

				case (int)Kernel.Prewitt_Y:
					m_pKern[0] = -1;	m_pKern[1] = -1;
					m_pKern[2] = -1;	m_pKern[6] =  1;
					m_pKern[7] =  1;	m_pKern[8] =  1;
					break;

				case (int)Kernel.Roberts135:
					m_pKern[2] =  1;	m_pKern[4] = -1;
					break;

				case (int)Kernel.Roberts45:
					m_pKern[0] =  1;	m_pKern[4] = -1;
					break;

				case (int)Kernel.Sobel_X:
					m_pKern[0] = -1;	m_pKern[2] = 1;
					m_pKern[3] = -2;	m_pKern[5] = 2;
					m_pKern[6] = -1;	m_pKern[8] = 1;
					break;

				case (int)Kernel.Sobel_Y:
					m_pKern[0] = -1;	m_pKern[1] = -2;
					m_pKern[2] = -1;	m_pKern[6] = 1;
					m_pKern[7] =  2;	m_pKern[8] = 1;
					break;
			};

			m_dKernTtl = 0;

			for ( int p = 0; p < m_iKernWid*m_iKernWid; p ++ )
				m_dKernTtl += m_pKern[p];

			if ( m_dKernTtl == 0 )
				m_dKernTtl = 1;

			textBoxDivider.Text = Convert.ToString ( m_dKernTtl );
			SetKernUI();
			return true;
		}

		// ====================================================================
		// Description:	Set kernel
		// Return:		true if success
		//				false if failed
		public bool SetKernel ( int iLen,			// [in] kernel array length
								double[] pdKern )	// [in] kernel array
		// ====================================================================
		{
			if ( ( iLen % 2 == 0 ) || ( iLen < 3 ) )
				return false;

			m_pKern = new double[iLen];
			for ( int i = 0; i < iLen; i ++ )
				m_pKern[i] = pdKern[i];

			m_dKernTtl = 0;

			for ( int p = 0; p < m_iKernWid*m_iKernWid; p ++ )
				m_dKernTtl += m_pKern[p];

			if ( m_dKernTtl == 0 )
				m_dKernTtl = 1;

			textBoxDivider.Text = Convert.ToString ( m_dKernTtl );
			SetKernUI();
			return true;
		}

		/// ///////////////////////////////////////////////////////////////////////////////////////
		// Private 

		// ====================================================================
		// Description:	Apply convolution
		// Return:		void
		private void Apply ()
		// ====================================================================
		{
			this.Cursor = Cursors.WaitCursor;
			m_prv.OnInitForm ( "Preview", (Bitmap)m_Orig.m_Img );
			m_prv.Location = new Point ( m_Orig.Location.X + m_Orig.Width/2, m_Orig.Location.Y+m_Orig.Height/2);

			Bitmap bmp = (Bitmap)m_prv.m_Img;
			if ( bmp.PixelFormat != PixelFormat.Format24bppRgb )
				return;

			BitmapData Bmpdata = bmp.LockBits ( new Rectangle (0,0, bmp.Width, bmp.Height ), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb );

			Bitmap tmp = (Bitmap)bmp.Clone();
			BitmapData Tmpdata = tmp.LockBits ( new Rectangle (0,0, tmp.Width, tmp.Height ), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb );

			if ( ( m_pKern == null ) || ( bmp == null ) )
				return;

			if ( comboBoxType.SelectedIndex == (int)Kernel.Custom )
				GetKernUI();
	

			int iPad = m_iKernWid / 2;

			m_dKernTtl = Convert.ToDouble(textBoxDivider.Text);
			
			if ( m_dKernTtl == 0 )
				m_dKernTtl = 1;

			unsafe
			{
				/*set pointer to first pixel */
				byte *Bmpptr = (byte*)(Bmpdata.Scan0) + (Bmpdata.Stride * (iPad+1)) + (3*(iPad+1));
				byte *Tmpptr = (byte*)(Tmpdata.Scan0);

				for ( int y = 0; y < bmp.Height-m_iKernWid; y ++ )
				{
					for ( int x = 0; x < bmp.Width-m_iKernWid; x ++ )
					{
						double dSumR = 0, dSumG = 0, dSumB = 0;
						for ( int j = 0; j < m_iKernWid; j ++ )
						{
							for ( int i = 0; i < m_iKernWid; i ++ )
							{
								dSumB += *Tmpptr * m_pKern[j*m_iKernWid+i]; Tmpptr ++;
								dSumG += *Tmpptr * m_pKern[j*m_iKernWid+i]; Tmpptr ++;
								dSumR += *Tmpptr * m_pKern[j*m_iKernWid+i]; Tmpptr ++;
							}
							Tmpptr = (byte*)(Tmpdata.Scan0) + (Bmpdata.Stride * (j+y)) + (3*x);
						}
						*Bmpptr = Cap(dSumB/m_dKernTtl); Bmpptr ++;
						*Bmpptr = Cap(dSumG/m_dKernTtl); Bmpptr ++;
						*Bmpptr = Cap(dSumR/m_dKernTtl); Bmpptr ++;
					}
					Bmpptr = (byte*)(Bmpdata.Scan0) + (Bmpdata.Stride * (iPad+1+y)) + (3*(iPad+1));
				}
			}
			bmp.UnlockBits(Bmpdata);
			tmp.UnlockBits(Tmpdata);

			this.Cursor = Cursors.Default;
			m_prv.Repaint();
		}

		// ====================================================================
		// Description:	Get Kernel from User interface
		// Return:		void
		private bool GetKernUI()
		// ====================================================================
		{
			/* determine kernel size */
			if ( ( textBoxR1C1.Text != "0" ) || ( textBoxR2C1.Text != "0" ) || ( textBoxR3C1.Text != "0" ) || ( textBoxR4C1.Text != "0" ) || ( textBoxR5C1.Text != "0" ) || ( textBoxR6C1.Text != "0" ) || ( textBoxR7C1.Text != "0" ) ||
				( textBoxR1C7.Text != "0" ) || ( textBoxR2C7.Text != "0" ) || ( textBoxR3C7.Text != "0" ) || ( textBoxR4C7.Text != "0" ) || ( textBoxR5C7.Text != "0" ) || ( textBoxR6C7.Text != "0" ) || ( textBoxR7C7.Text != "0" ) ||
				( textBoxR1C2.Text != "0" ) || ( textBoxR1C3.Text != "0" ) || ( textBoxR1C4.Text != "0" ) || ( textBoxR1C5.Text != "0" ) || ( textBoxR1C6.Text != "0" ) ||                            
				( textBoxR7C2.Text != "0" ) || ( textBoxR7C3.Text != "0" ) || ( textBoxR7C4.Text != "0" ) || ( textBoxR7C5.Text != "0" ) || ( textBoxR7C6.Text != "0" )  )
			{
				m_iKernWid = 7;
				m_pKern = new double[m_iKernWid*m_iKernWid];
				m_pKern[0] = Convert.ToDouble(textBoxR1C1.Text);
				m_pKern[1] = Convert.ToDouble(textBoxR1C2.Text);
				m_pKern[2] = Convert.ToDouble(textBoxR1C3.Text);
				m_pKern[3] = Convert.ToDouble(textBoxR1C4.Text);
				m_pKern[4] = Convert.ToDouble(textBoxR1C5.Text);
				m_pKern[5] = Convert.ToDouble(textBoxR1C6.Text);
				m_pKern[6] = Convert.ToDouble(textBoxR1C7.Text);
				m_pKern[7] = Convert.ToDouble(textBoxR2C1.Text);
				m_pKern[8] = Convert.ToDouble(textBoxR2C2.Text);
				m_pKern[9] = Convert.ToDouble(textBoxR2C3.Text);
				m_pKern[10] = Convert.ToDouble(textBoxR2C4.Text);
				m_pKern[11] = Convert.ToDouble(textBoxR2C5.Text);
				m_pKern[12] = Convert.ToDouble(textBoxR2C6.Text);
				m_pKern[13] = Convert.ToDouble(textBoxR2C7.Text);
				m_pKern[14] = Convert.ToDouble(textBoxR3C1.Text);
				m_pKern[15] = Convert.ToDouble(textBoxR3C2.Text);
				m_pKern[16] = Convert.ToDouble(textBoxR3C3.Text);
				m_pKern[17] = Convert.ToDouble(textBoxR3C4.Text);
				m_pKern[18] = Convert.ToDouble(textBoxR3C5.Text);
				m_pKern[19] = Convert.ToDouble(textBoxR3C6.Text);
				m_pKern[20] = Convert.ToDouble(textBoxR3C7.Text);
				m_pKern[21] = Convert.ToDouble(textBoxR4C1.Text);
				m_pKern[22] = Convert.ToDouble(textBoxR4C2.Text);
				m_pKern[23] = Convert.ToDouble(textBoxR4C3.Text);
				m_pKern[24] = Convert.ToDouble(textBoxR4C4.Text);
				m_pKern[25] = Convert.ToDouble(textBoxR4C5.Text);
				m_pKern[26] = Convert.ToDouble(textBoxR4C6.Text);
				m_pKern[27] = Convert.ToDouble(textBoxR4C7.Text);
				m_pKern[28] = Convert.ToDouble(textBoxR5C1.Text);
				m_pKern[29] = Convert.ToDouble(textBoxR5C2.Text);
				m_pKern[30] = Convert.ToDouble(textBoxR5C3.Text);
				m_pKern[31] = Convert.ToDouble(textBoxR5C4.Text);
				m_pKern[32] = Convert.ToDouble(textBoxR5C5.Text);
				m_pKern[33] = Convert.ToDouble(textBoxR5C6.Text);
				m_pKern[34] = Convert.ToDouble(textBoxR5C7.Text);
				m_pKern[35] = Convert.ToDouble(textBoxR6C1.Text);
				m_pKern[36] = Convert.ToDouble(textBoxR6C2.Text);
				m_pKern[37] = Convert.ToDouble(textBoxR6C3.Text);
				m_pKern[38] = Convert.ToDouble(textBoxR6C4.Text);
				m_pKern[39] = Convert.ToDouble(textBoxR6C5.Text);
				m_pKern[40] = Convert.ToDouble(textBoxR6C6.Text);
				m_pKern[41] = Convert.ToDouble(textBoxR6C7.Text);
				m_pKern[42] = Convert.ToDouble(textBoxR7C1.Text);
				m_pKern[43] = Convert.ToDouble(textBoxR7C2.Text);
				m_pKern[44] = Convert.ToDouble(textBoxR7C3.Text);
				m_pKern[45] = Convert.ToDouble(textBoxR7C4.Text);
				m_pKern[46] = Convert.ToDouble(textBoxR7C5.Text);
				m_pKern[47] = Convert.ToDouble(textBoxR7C6.Text);
				m_pKern[48] = Convert.ToDouble(textBoxR7C7.Text);
				
			}
			else if ( ( textBoxR2C2.Text != "0" ) || ( textBoxR2C3.Text != "0" ) || ( textBoxR2C4.Text != "0" ) || ( textBoxR2C5.Text != "0" ) || ( textBoxR2C6.Text != "0" ) || 
				( textBoxR6C2.Text != "0" ) || ( textBoxR6C3.Text != "0" ) || ( textBoxR6C4.Text != "0" ) || ( textBoxR6C5.Text != "0" ) || ( textBoxR6C6.Text != "0" ) ||
				( textBoxR2C2.Text != "0" ) || ( textBoxR2C3.Text != "0" ) || ( textBoxR2C4.Text != "0" ) ||
				( textBoxR6C2.Text != "0" ) || ( textBoxR6C3.Text != "0" ) || ( textBoxR6C4.Text != "0" ) )
			{
				m_iKernWid = 5;
				m_pKern = new double[m_iKernWid*m_iKernWid];
				m_pKern[0] = Convert.ToDouble(textBoxR2C2.Text);
				m_pKern[1] = Convert.ToDouble(textBoxR2C3.Text);
				m_pKern[2] = Convert.ToDouble(textBoxR2C4.Text);
				m_pKern[3] = Convert.ToDouble(textBoxR2C5.Text);
				m_pKern[4] = Convert.ToDouble(textBoxR2C6.Text);
				m_pKern[5] = Convert.ToDouble(textBoxR3C2.Text);
				m_pKern[6] = Convert.ToDouble(textBoxR3C3.Text);
				m_pKern[7] = Convert.ToDouble(textBoxR3C4.Text);
				m_pKern[8] = Convert.ToDouble(textBoxR3C5.Text);
				m_pKern[9] = Convert.ToDouble(textBoxR3C6.Text);
				m_pKern[10] = Convert.ToDouble(textBoxR4C2.Text);
				m_pKern[11] = Convert.ToDouble(textBoxR4C3.Text);
				m_pKern[12] = Convert.ToDouble(textBoxR4C4.Text);
				m_pKern[13] = Convert.ToDouble(textBoxR4C5.Text);
				m_pKern[14] = Convert.ToDouble(textBoxR4C6.Text);
				m_pKern[15] = Convert.ToDouble(textBoxR5C2.Text);
				m_pKern[16] = Convert.ToDouble(textBoxR5C3.Text);
				m_pKern[17] = Convert.ToDouble(textBoxR5C4.Text);
				m_pKern[18] = Convert.ToDouble(textBoxR5C5.Text);
				m_pKern[19] = Convert.ToDouble(textBoxR5C6.Text);
				m_pKern[20] = Convert.ToDouble(textBoxR6C2.Text);
				m_pKern[21] = Convert.ToDouble(textBoxR6C3.Text);
				m_pKern[22] = Convert.ToDouble(textBoxR6C4.Text);
				m_pKern[23] = Convert.ToDouble(textBoxR6C5.Text);
				m_pKern[24] = Convert.ToDouble(textBoxR6C6.Text);
			}
			else if ( ( textBoxR3C3.Text != "0" ) || ( textBoxR3C4.Text != "0" ) || ( textBoxR3C5.Text != "0" ) ||
				( textBoxR5C3.Text != "0" ) || ( textBoxR5C4.Text != "0" ) || ( textBoxR5C5.Text != "0" ) ||
				( textBoxR4C3.Text != "0" ) ||
				( textBoxR4C5.Text != "0" ) )
			{
				m_iKernWid = 3;
				m_pKern = new double[m_iKernWid*m_iKernWid];
				m_pKern[0] = Convert.ToDouble(textBoxR3C3.Text);
				m_pKern[1] = Convert.ToDouble(textBoxR3C4.Text);
				m_pKern[2] = Convert.ToDouble(textBoxR3C5.Text);
				m_pKern[3] = Convert.ToDouble(textBoxR4C3.Text);
				m_pKern[4] = Convert.ToDouble(textBoxR4C4.Text);
				m_pKern[5] = Convert.ToDouble(textBoxR4C5.Text);
				m_pKern[6] = Convert.ToDouble(textBoxR5C3.Text);
				m_pKern[7] = Convert.ToDouble(textBoxR5C4.Text);
				m_pKern[8] = Convert.ToDouble(textBoxR5C5.Text);
			}
			else if ( textBoxR4C4.Text != "0" )
				m_iKernWid = 1;

			return true;
		}

		// ====================================================================
		// Description:	Set Kernel User interface
		// Return:		void
		private void SetKernUI()
		// ====================================================================
		{
			/* Initialize the text boxes */
			textBoxR1C1.Text = Convert.ToString(0);
			textBoxR1C2.Text = Convert.ToString(0);
			textBoxR1C3.Text = Convert.ToString(0);
			textBoxR1C4.Text = Convert.ToString(0);
			textBoxR1C5.Text = Convert.ToString(0);
			textBoxR1C6.Text = Convert.ToString(0);
			textBoxR1C7.Text = Convert.ToString(0);

			textBoxR2C1.Text = Convert.ToString(0);
			textBoxR2C2.Text = Convert.ToString(0);
			textBoxR2C3.Text = Convert.ToString(0);
			textBoxR2C4.Text = Convert.ToString(0);
			textBoxR2C5.Text = Convert.ToString(0);
			textBoxR2C6.Text = Convert.ToString(0);
			textBoxR2C7.Text = Convert.ToString(0);

			textBoxR3C1.Text = Convert.ToString(0);
			textBoxR3C2.Text = Convert.ToString(0);
			textBoxR3C3.Text = Convert.ToString(0);
			textBoxR3C4.Text = Convert.ToString(0);
			textBoxR3C5.Text = Convert.ToString(0);
			textBoxR3C6.Text = Convert.ToString(0);
			textBoxR3C7.Text = Convert.ToString(0);

			textBoxR4C1.Text = Convert.ToString(0);
			textBoxR4C2.Text = Convert.ToString(0);
			textBoxR4C3.Text = Convert.ToString(0);
			textBoxR4C4.Text = Convert.ToString(0);
			textBoxR4C5.Text = Convert.ToString(0);
			textBoxR4C6.Text = Convert.ToString(0);
			textBoxR4C7.Text = Convert.ToString(0);

			textBoxR5C1.Text = Convert.ToString(0);
			textBoxR5C2.Text = Convert.ToString(0);
			textBoxR5C3.Text = Convert.ToString(0);
			textBoxR5C4.Text = Convert.ToString(0);
			textBoxR5C5.Text = Convert.ToString(0);
			textBoxR5C6.Text = Convert.ToString(0);
			textBoxR5C7.Text = Convert.ToString(0);

			textBoxR6C1.Text = Convert.ToString(0);
			textBoxR6C2.Text = Convert.ToString(0);
			textBoxR6C3.Text = Convert.ToString(0);
			textBoxR6C4.Text = Convert.ToString(0);
			textBoxR6C5.Text = Convert.ToString(0);
			textBoxR6C6.Text = Convert.ToString(0);
			textBoxR6C7.Text = Convert.ToString(0);

			textBoxR7C1.Text = Convert.ToString(0);
			textBoxR7C2.Text = Convert.ToString(0);
			textBoxR7C3.Text = Convert.ToString(0);
			textBoxR7C4.Text = Convert.ToString(0);
			textBoxR7C5.Text = Convert.ToString(0);
			textBoxR7C6.Text = Convert.ToString(0);
			textBoxR7C7.Text = Convert.ToString(0);

			textBoxDivider.Text = Convert.ToString (m_dKernTtl);

			/* Set kernel with kernel (if exist) */
			if ( m_iKernWid == 1 )
				textBoxR4C4.Text = Convert.ToString(m_pKern[0]);

			else if ( m_iKernWid == 3 )
			{
				textBoxR3C3.Text = Convert.ToString(m_pKern[0]);
				textBoxR3C4.Text = Convert.ToString(m_pKern[1]);
				textBoxR3C5.Text = Convert.ToString(m_pKern[2]);

				textBoxR4C3.Text = Convert.ToString(m_pKern[3]);
				textBoxR4C4.Text = Convert.ToString(m_pKern[4]);
				textBoxR4C5.Text = Convert.ToString(m_pKern[5]);

				textBoxR5C3.Text = Convert.ToString(m_pKern[6]);
				textBoxR5C4.Text = Convert.ToString(m_pKern[7]);
				textBoxR5C5.Text = Convert.ToString(m_pKern[8]);
			}
			else if ( m_iKernWid == 5 )
			{
				textBoxR2C2.Text = Convert.ToString(m_pKern[0]);
				textBoxR2C3.Text = Convert.ToString(m_pKern[1]);
				textBoxR2C4.Text = Convert.ToString(m_pKern[2]);
				textBoxR2C5.Text = Convert.ToString(m_pKern[3]);
				textBoxR2C6.Text = Convert.ToString(m_pKern[4]);

				textBoxR3C2.Text = Convert.ToString(m_pKern[5]);
				textBoxR3C3.Text = Convert.ToString(m_pKern[6]);
				textBoxR3C4.Text = Convert.ToString(m_pKern[7]);
				textBoxR3C5.Text = Convert.ToString(m_pKern[8]);
				textBoxR3C6.Text = Convert.ToString(m_pKern[9]);

				textBoxR4C2.Text = Convert.ToString(m_pKern[10]);
				textBoxR4C3.Text = Convert.ToString(m_pKern[11]);
				textBoxR4C4.Text = Convert.ToString(m_pKern[12]);
				textBoxR4C5.Text = Convert.ToString(m_pKern[13]);
				textBoxR4C6.Text = Convert.ToString(m_pKern[14]);

				textBoxR5C2.Text = Convert.ToString(m_pKern[15]);
				textBoxR5C3.Text = Convert.ToString(m_pKern[16]);
				textBoxR5C4.Text = Convert.ToString(m_pKern[17]);
				textBoxR5C5.Text = Convert.ToString(m_pKern[18]);
				textBoxR5C6.Text = Convert.ToString(m_pKern[19]);

				textBoxR6C2.Text = Convert.ToString(m_pKern[20]);
				textBoxR6C3.Text = Convert.ToString(m_pKern[21]);
				textBoxR6C4.Text = Convert.ToString(m_pKern[22]);
				textBoxR6C5.Text = Convert.ToString(m_pKern[23]);
				textBoxR6C6.Text = Convert.ToString(m_pKern[24]);
			}
			else if ( m_iKernWid == 7 )
			{
				textBoxR1C1.Text = Convert.ToString(m_pKern[0]);
				textBoxR1C2.Text = Convert.ToString(m_pKern[1]);
				textBoxR1C3.Text = Convert.ToString(m_pKern[2]);
				textBoxR1C4.Text = Convert.ToString(m_pKern[3]);
				textBoxR1C5.Text = Convert.ToString(m_pKern[4]);
				textBoxR1C6.Text = Convert.ToString(m_pKern[5]);
				textBoxR1C7.Text = Convert.ToString(m_pKern[6]);

				textBoxR2C1.Text = Convert.ToString(m_pKern[7]);
				textBoxR2C2.Text = Convert.ToString(m_pKern[8]);
				textBoxR2C3.Text = Convert.ToString(m_pKern[9]);
				textBoxR2C4.Text = Convert.ToString(m_pKern[10]);
				textBoxR2C5.Text = Convert.ToString(m_pKern[11]);
				textBoxR2C6.Text = Convert.ToString(m_pKern[12]);
				textBoxR2C7.Text = Convert.ToString(m_pKern[13]);

				textBoxR3C1.Text = Convert.ToString(m_pKern[14]);
				textBoxR3C2.Text = Convert.ToString(m_pKern[15]);
				textBoxR3C3.Text = Convert.ToString(m_pKern[16]);
				textBoxR3C4.Text = Convert.ToString(m_pKern[17]);
				textBoxR3C5.Text = Convert.ToString(m_pKern[18]);
				textBoxR3C6.Text = Convert.ToString(m_pKern[19]);
				textBoxR3C7.Text = Convert.ToString(m_pKern[20]);

				textBoxR4C1.Text = Convert.ToString(m_pKern[21]);
				textBoxR4C2.Text = Convert.ToString(m_pKern[22]);
				textBoxR4C3.Text = Convert.ToString(m_pKern[23]);
				textBoxR4C4.Text = Convert.ToString(m_pKern[24]);
				textBoxR4C5.Text = Convert.ToString(m_pKern[25]);
				textBoxR4C6.Text = Convert.ToString(m_pKern[26]);
				textBoxR4C7.Text = Convert.ToString(m_pKern[27]);

				textBoxR5C1.Text = Convert.ToString(m_pKern[28]);
				textBoxR5C2.Text = Convert.ToString(m_pKern[29]);
				textBoxR5C3.Text = Convert.ToString(m_pKern[30]);
				textBoxR5C4.Text = Convert.ToString(m_pKern[31]);
				textBoxR5C5.Text = Convert.ToString(m_pKern[32]);
				textBoxR5C6.Text = Convert.ToString(m_pKern[33]);
				textBoxR5C7.Text = Convert.ToString(m_pKern[34]);

				textBoxR6C1.Text = Convert.ToString(m_pKern[35]);
				textBoxR6C2.Text = Convert.ToString(m_pKern[36]);
				textBoxR6C3.Text = Convert.ToString(m_pKern[37]);
				textBoxR6C4.Text = Convert.ToString(m_pKern[38]);
				textBoxR6C5.Text = Convert.ToString(m_pKern[39]);
				textBoxR6C6.Text = Convert.ToString(m_pKern[40]);
				textBoxR6C7.Text = Convert.ToString(m_pKern[41]);

				textBoxR7C1.Text = Convert.ToString(m_pKern[42]);
				textBoxR7C2.Text = Convert.ToString(m_pKern[43]);
				textBoxR7C3.Text = Convert.ToString(m_pKern[44]);
				textBoxR7C4.Text = Convert.ToString(m_pKern[45]);
				textBoxR7C5.Text = Convert.ToString(m_pKern[46]);
				textBoxR7C6.Text = Convert.ToString(m_pKern[47]);
				textBoxR7C7.Text = Convert.ToString(m_pKern[48]);

			}

		}

		// ====================================================================
		// Description:	Cap
		// Return:		integer
		private byte Cap ( double value )
		// ====================================================================
		{
			if ( value < 0 )
				return 0;

			if ( value > 255 )
				return 255;

			return (byte)value;
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textBoxR7C7 = new System.Windows.Forms.TextBox();
			this.textBoxR7C6 = new System.Windows.Forms.TextBox();
			this.textBoxR7C5 = new System.Windows.Forms.TextBox();
			this.textBoxR7C4 = new System.Windows.Forms.TextBox();
			this.textBoxR7C3 = new System.Windows.Forms.TextBox();
			this.textBoxR7C2 = new System.Windows.Forms.TextBox();
			this.textBoxR7C1 = new System.Windows.Forms.TextBox();
			this.textBoxR6C7 = new System.Windows.Forms.TextBox();
			this.textBoxR6C6 = new System.Windows.Forms.TextBox();
			this.textBoxR6C5 = new System.Windows.Forms.TextBox();
			this.textBoxR6C4 = new System.Windows.Forms.TextBox();
			this.textBoxR6C3 = new System.Windows.Forms.TextBox();
			this.textBoxR6C2 = new System.Windows.Forms.TextBox();
			this.textBoxR6C1 = new System.Windows.Forms.TextBox();
			this.textBoxR5C7 = new System.Windows.Forms.TextBox();
			this.textBoxR5C6 = new System.Windows.Forms.TextBox();
			this.textBoxR5C5 = new System.Windows.Forms.TextBox();
			this.textBoxR5C4 = new System.Windows.Forms.TextBox();
			this.textBoxR5C3 = new System.Windows.Forms.TextBox();
			this.textBoxR5C2 = new System.Windows.Forms.TextBox();
			this.textBoxR5C1 = new System.Windows.Forms.TextBox();
			this.textBoxR4C7 = new System.Windows.Forms.TextBox();
			this.textBoxR4C6 = new System.Windows.Forms.TextBox();
			this.textBoxR4C5 = new System.Windows.Forms.TextBox();
			this.textBoxR4C4 = new System.Windows.Forms.TextBox();
			this.textBoxR4C3 = new System.Windows.Forms.TextBox();
			this.textBoxR4C2 = new System.Windows.Forms.TextBox();
			this.textBoxR4C1 = new System.Windows.Forms.TextBox();
			this.textBoxR3C7 = new System.Windows.Forms.TextBox();
			this.textBoxR3C6 = new System.Windows.Forms.TextBox();
			this.textBoxR3C5 = new System.Windows.Forms.TextBox();
			this.textBoxR3C4 = new System.Windows.Forms.TextBox();
			this.textBoxR3C3 = new System.Windows.Forms.TextBox();
			this.textBoxR3C2 = new System.Windows.Forms.TextBox();
			this.textBoxR3C1 = new System.Windows.Forms.TextBox();
			this.textBoxR2C7 = new System.Windows.Forms.TextBox();
			this.textBoxR2C6 = new System.Windows.Forms.TextBox();
			this.textBoxR2C5 = new System.Windows.Forms.TextBox();
			this.textBoxR2C4 = new System.Windows.Forms.TextBox();
			this.textBoxR2C3 = new System.Windows.Forms.TextBox();
			this.textBoxR2C2 = new System.Windows.Forms.TextBox();
			this.textBoxR2C1 = new System.Windows.Forms.TextBox();
			this.textBoxR1C7 = new System.Windows.Forms.TextBox();
			this.textBoxR1C6 = new System.Windows.Forms.TextBox();
			this.textBoxR1C5 = new System.Windows.Forms.TextBox();
			this.textBoxR1C4 = new System.Windows.Forms.TextBox();
			this.textBoxR1C3 = new System.Windows.Forms.TextBox();
			this.textBoxR1C2 = new System.Windows.Forms.TextBox();
			this.textBoxR1C1 = new System.Windows.Forms.TextBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.textBoxDivider = new System.Windows.Forms.TextBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.comboBoxType = new System.Windows.Forms.ComboBox();
			this.button_Apply = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.button_Cancel = new System.Windows.Forms.Button();
			this.button_OK = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.textBoxR7C7);
			this.groupBox1.Controls.Add(this.textBoxR7C6);
			this.groupBox1.Controls.Add(this.textBoxR7C5);
			this.groupBox1.Controls.Add(this.textBoxR7C4);
			this.groupBox1.Controls.Add(this.textBoxR7C3);
			this.groupBox1.Controls.Add(this.textBoxR7C2);
			this.groupBox1.Controls.Add(this.textBoxR7C1);
			this.groupBox1.Controls.Add(this.textBoxR6C7);
			this.groupBox1.Controls.Add(this.textBoxR6C6);
			this.groupBox1.Controls.Add(this.textBoxR6C5);
			this.groupBox1.Controls.Add(this.textBoxR6C4);
			this.groupBox1.Controls.Add(this.textBoxR6C3);
			this.groupBox1.Controls.Add(this.textBoxR6C2);
			this.groupBox1.Controls.Add(this.textBoxR6C1);
			this.groupBox1.Controls.Add(this.textBoxR5C7);
			this.groupBox1.Controls.Add(this.textBoxR5C6);
			this.groupBox1.Controls.Add(this.textBoxR5C5);
			this.groupBox1.Controls.Add(this.textBoxR5C4);
			this.groupBox1.Controls.Add(this.textBoxR5C3);
			this.groupBox1.Controls.Add(this.textBoxR5C2);
			this.groupBox1.Controls.Add(this.textBoxR5C1);
			this.groupBox1.Controls.Add(this.textBoxR4C7);
			this.groupBox1.Controls.Add(this.textBoxR4C6);
			this.groupBox1.Controls.Add(this.textBoxR4C5);
			this.groupBox1.Controls.Add(this.textBoxR4C4);
			this.groupBox1.Controls.Add(this.textBoxR4C3);
			this.groupBox1.Controls.Add(this.textBoxR4C2);
			this.groupBox1.Controls.Add(this.textBoxR4C1);
			this.groupBox1.Controls.Add(this.textBoxR3C7);
			this.groupBox1.Controls.Add(this.textBoxR3C6);
			this.groupBox1.Controls.Add(this.textBoxR3C5);
			this.groupBox1.Controls.Add(this.textBoxR3C4);
			this.groupBox1.Controls.Add(this.textBoxR3C3);
			this.groupBox1.Controls.Add(this.textBoxR3C2);
			this.groupBox1.Controls.Add(this.textBoxR3C1);
			this.groupBox1.Controls.Add(this.textBoxR2C7);
			this.groupBox1.Controls.Add(this.textBoxR2C6);
			this.groupBox1.Controls.Add(this.textBoxR2C5);
			this.groupBox1.Controls.Add(this.textBoxR2C4);
			this.groupBox1.Controls.Add(this.textBoxR2C3);
			this.groupBox1.Controls.Add(this.textBoxR2C2);
			this.groupBox1.Controls.Add(this.textBoxR2C1);
			this.groupBox1.Controls.Add(this.textBoxR1C7);
			this.groupBox1.Controls.Add(this.textBoxR1C6);
			this.groupBox1.Controls.Add(this.textBoxR1C5);
			this.groupBox1.Controls.Add(this.textBoxR1C4);
			this.groupBox1.Controls.Add(this.textBoxR1C3);
			this.groupBox1.Controls.Add(this.textBoxR1C2);
			this.groupBox1.Controls.Add(this.textBoxR1C1);
			this.groupBox1.Location = new System.Drawing.Point(16, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(344, 192);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Kernel";
			// 
			// textBoxR7C7
			// 
			this.textBoxR7C7.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR7C7.Location = new System.Drawing.Point(296, 160);
			this.textBoxR7C7.Name = "textBoxR7C7";
			this.textBoxR7C7.Size = new System.Drawing.Size(40, 20);
			this.textBoxR7C7.TabIndex = 48;
			this.textBoxR7C7.Text = "textBox43";
			// 
			// textBoxR7C6
			// 
			this.textBoxR7C6.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR7C6.Location = new System.Drawing.Point(248, 160);
			this.textBoxR7C6.Name = "textBoxR7C6";
			this.textBoxR7C6.Size = new System.Drawing.Size(40, 20);
			this.textBoxR7C6.TabIndex = 47;
			this.textBoxR7C6.Text = "textBox44";
			// 
			// textBoxR7C5
			// 
			this.textBoxR7C5.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR7C5.Location = new System.Drawing.Point(200, 160);
			this.textBoxR7C5.Name = "textBoxR7C5";
			this.textBoxR7C5.Size = new System.Drawing.Size(40, 20);
			this.textBoxR7C5.TabIndex = 46;
			this.textBoxR7C5.Text = "textBox45";
			// 
			// textBoxR7C4
			// 
			this.textBoxR7C4.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR7C4.Location = new System.Drawing.Point(152, 160);
			this.textBoxR7C4.Name = "textBoxR7C4";
			this.textBoxR7C4.Size = new System.Drawing.Size(40, 20);
			this.textBoxR7C4.TabIndex = 45;
			this.textBoxR7C4.Text = "textBox46";
			// 
			// textBoxR7C3
			// 
			this.textBoxR7C3.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR7C3.Location = new System.Drawing.Point(104, 160);
			this.textBoxR7C3.Name = "textBoxR7C3";
			this.textBoxR7C3.Size = new System.Drawing.Size(40, 20);
			this.textBoxR7C3.TabIndex = 44;
			this.textBoxR7C3.Text = "textBox47";
			// 
			// textBoxR7C2
			// 
			this.textBoxR7C2.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR7C2.Location = new System.Drawing.Point(56, 160);
			this.textBoxR7C2.Name = "textBoxR7C2";
			this.textBoxR7C2.Size = new System.Drawing.Size(40, 20);
			this.textBoxR7C2.TabIndex = 43;
			this.textBoxR7C2.Text = "textBox48";
			// 
			// textBoxR7C1
			// 
			this.textBoxR7C1.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR7C1.Location = new System.Drawing.Point(8, 160);
			this.textBoxR7C1.Name = "textBoxR7C1";
			this.textBoxR7C1.Size = new System.Drawing.Size(40, 20);
			this.textBoxR7C1.TabIndex = 42;
			this.textBoxR7C1.Text = "textBox49";
			// 
			// textBoxR6C7
			// 
			this.textBoxR6C7.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR6C7.Location = new System.Drawing.Point(296, 136);
			this.textBoxR6C7.Name = "textBoxR6C7";
			this.textBoxR6C7.Size = new System.Drawing.Size(40, 20);
			this.textBoxR6C7.TabIndex = 41;
			this.textBoxR6C7.Text = "textBox36";
			// 
			// textBoxR6C6
			// 
			this.textBoxR6C6.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR6C6.Location = new System.Drawing.Point(248, 136);
			this.textBoxR6C6.Name = "textBoxR6C6";
			this.textBoxR6C6.Size = new System.Drawing.Size(40, 20);
			this.textBoxR6C6.TabIndex = 40;
			this.textBoxR6C6.Text = "textBox37";
			// 
			// textBoxR6C5
			// 
			this.textBoxR6C5.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR6C5.Location = new System.Drawing.Point(200, 136);
			this.textBoxR6C5.Name = "textBoxR6C5";
			this.textBoxR6C5.Size = new System.Drawing.Size(40, 20);
			this.textBoxR6C5.TabIndex = 39;
			this.textBoxR6C5.Text = "textBox38";
			// 
			// textBoxR6C4
			// 
			this.textBoxR6C4.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR6C4.Location = new System.Drawing.Point(152, 136);
			this.textBoxR6C4.Name = "textBoxR6C4";
			this.textBoxR6C4.Size = new System.Drawing.Size(40, 20);
			this.textBoxR6C4.TabIndex = 38;
			this.textBoxR6C4.Text = "textBox39";
			// 
			// textBoxR6C3
			// 
			this.textBoxR6C3.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR6C3.Location = new System.Drawing.Point(104, 136);
			this.textBoxR6C3.Name = "textBoxR6C3";
			this.textBoxR6C3.Size = new System.Drawing.Size(40, 20);
			this.textBoxR6C3.TabIndex = 37;
			this.textBoxR6C3.Text = "textBox40";
			// 
			// textBoxR6C2
			// 
			this.textBoxR6C2.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR6C2.Location = new System.Drawing.Point(56, 136);
			this.textBoxR6C2.Name = "textBoxR6C2";
			this.textBoxR6C2.Size = new System.Drawing.Size(40, 20);
			this.textBoxR6C2.TabIndex = 36;
			this.textBoxR6C2.Text = "textBox41";
			// 
			// textBoxR6C1
			// 
			this.textBoxR6C1.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR6C1.Location = new System.Drawing.Point(8, 136);
			this.textBoxR6C1.Name = "textBoxR6C1";
			this.textBoxR6C1.Size = new System.Drawing.Size(40, 20);
			this.textBoxR6C1.TabIndex = 35;
			this.textBoxR6C1.Text = "textBox42";
			// 
			// textBoxR5C7
			// 
			this.textBoxR5C7.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR5C7.Location = new System.Drawing.Point(296, 112);
			this.textBoxR5C7.Name = "textBoxR5C7";
			this.textBoxR5C7.Size = new System.Drawing.Size(40, 20);
			this.textBoxR5C7.TabIndex = 34;
			this.textBoxR5C7.Text = "textBox29";
			// 
			// textBoxR5C6
			// 
			this.textBoxR5C6.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR5C6.Location = new System.Drawing.Point(248, 112);
			this.textBoxR5C6.Name = "textBoxR5C6";
			this.textBoxR5C6.Size = new System.Drawing.Size(40, 20);
			this.textBoxR5C6.TabIndex = 33;
			this.textBoxR5C6.Text = "textBox30";
			// 
			// textBoxR5C5
			// 
			this.textBoxR5C5.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.textBoxR5C5.Location = new System.Drawing.Point(200, 112);
			this.textBoxR5C5.Name = "textBoxR5C5";
			this.textBoxR5C5.Size = new System.Drawing.Size(40, 20);
			this.textBoxR5C5.TabIndex = 32;
			this.textBoxR5C5.Text = "textBox31";
			// 
			// textBoxR5C4
			// 
			this.textBoxR5C4.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.textBoxR5C4.Location = new System.Drawing.Point(152, 112);
			this.textBoxR5C4.Name = "textBoxR5C4";
			this.textBoxR5C4.Size = new System.Drawing.Size(40, 20);
			this.textBoxR5C4.TabIndex = 31;
			this.textBoxR5C4.Text = "textBox32";
			// 
			// textBoxR5C3
			// 
			this.textBoxR5C3.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.textBoxR5C3.Location = new System.Drawing.Point(104, 112);
			this.textBoxR5C3.Name = "textBoxR5C3";
			this.textBoxR5C3.Size = new System.Drawing.Size(40, 20);
			this.textBoxR5C3.TabIndex = 30;
			this.textBoxR5C3.Text = "textBox33";
			// 
			// textBoxR5C2
			// 
			this.textBoxR5C2.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR5C2.Location = new System.Drawing.Point(56, 112);
			this.textBoxR5C2.Name = "textBoxR5C2";
			this.textBoxR5C2.Size = new System.Drawing.Size(40, 20);
			this.textBoxR5C2.TabIndex = 29;
			this.textBoxR5C2.Text = "textBox34";
			// 
			// textBoxR5C1
			// 
			this.textBoxR5C1.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR5C1.Location = new System.Drawing.Point(8, 112);
			this.textBoxR5C1.Name = "textBoxR5C1";
			this.textBoxR5C1.Size = new System.Drawing.Size(40, 20);
			this.textBoxR5C1.TabIndex = 28;
			this.textBoxR5C1.Text = "textBox35";
			// 
			// textBoxR4C7
			// 
			this.textBoxR4C7.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR4C7.Location = new System.Drawing.Point(296, 88);
			this.textBoxR4C7.Name = "textBoxR4C7";
			this.textBoxR4C7.Size = new System.Drawing.Size(40, 20);
			this.textBoxR4C7.TabIndex = 27;
			this.textBoxR4C7.Text = "textBox22";
			// 
			// textBoxR4C6
			// 
			this.textBoxR4C6.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR4C6.Location = new System.Drawing.Point(248, 88);
			this.textBoxR4C6.Name = "textBoxR4C6";
			this.textBoxR4C6.Size = new System.Drawing.Size(40, 20);
			this.textBoxR4C6.TabIndex = 26;
			this.textBoxR4C6.Text = "textBox23";
			// 
			// textBoxR4C5
			// 
			this.textBoxR4C5.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.textBoxR4C5.Location = new System.Drawing.Point(200, 88);
			this.textBoxR4C5.Name = "textBoxR4C5";
			this.textBoxR4C5.Size = new System.Drawing.Size(40, 20);
			this.textBoxR4C5.TabIndex = 25;
			this.textBoxR4C5.Text = "textBox24";
			// 
			// textBoxR4C4
			// 
			this.textBoxR4C4.Location = new System.Drawing.Point(152, 88);
			this.textBoxR4C4.Name = "textBoxR4C4";
			this.textBoxR4C4.Size = new System.Drawing.Size(40, 20);
			this.textBoxR4C4.TabIndex = 24;
			this.textBoxR4C4.Text = "textBoxR4C4";
			this.textBoxR4C4.TextChanged += new System.EventHandler(this.textBox25_TextChanged);
			// 
			// textBoxR4C3
			// 
			this.textBoxR4C3.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.textBoxR4C3.Location = new System.Drawing.Point(104, 88);
			this.textBoxR4C3.Name = "textBoxR4C3";
			this.textBoxR4C3.Size = new System.Drawing.Size(40, 20);
			this.textBoxR4C3.TabIndex = 23;
			this.textBoxR4C3.Text = "textBox26";
			// 
			// textBoxR4C2
			// 
			this.textBoxR4C2.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR4C2.Location = new System.Drawing.Point(56, 88);
			this.textBoxR4C2.Name = "textBoxR4C2";
			this.textBoxR4C2.Size = new System.Drawing.Size(40, 20);
			this.textBoxR4C2.TabIndex = 22;
			this.textBoxR4C2.Text = "textBox27";
			// 
			// textBoxR4C1
			// 
			this.textBoxR4C1.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR4C1.Location = new System.Drawing.Point(8, 88);
			this.textBoxR4C1.Name = "textBoxR4C1";
			this.textBoxR4C1.Size = new System.Drawing.Size(40, 20);
			this.textBoxR4C1.TabIndex = 21;
			this.textBoxR4C1.Text = "textBox28";
			// 
			// textBoxR3C7
			// 
			this.textBoxR3C7.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR3C7.Location = new System.Drawing.Point(296, 64);
			this.textBoxR3C7.Name = "textBoxR3C7";
			this.textBoxR3C7.Size = new System.Drawing.Size(40, 20);
			this.textBoxR3C7.TabIndex = 20;
			this.textBoxR3C7.Text = "textBox15";
			// 
			// textBoxR3C6
			// 
			this.textBoxR3C6.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR3C6.Location = new System.Drawing.Point(248, 64);
			this.textBoxR3C6.Name = "textBoxR3C6";
			this.textBoxR3C6.Size = new System.Drawing.Size(40, 20);
			this.textBoxR3C6.TabIndex = 19;
			this.textBoxR3C6.Text = "textBox16";
			// 
			// textBoxR3C5
			// 
			this.textBoxR3C5.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.textBoxR3C5.Location = new System.Drawing.Point(200, 64);
			this.textBoxR3C5.Name = "textBoxR3C5";
			this.textBoxR3C5.Size = new System.Drawing.Size(40, 20);
			this.textBoxR3C5.TabIndex = 18;
			this.textBoxR3C5.Text = "textBox17";
			// 
			// textBoxR3C4
			// 
			this.textBoxR3C4.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.textBoxR3C4.Location = new System.Drawing.Point(152, 64);
			this.textBoxR3C4.Name = "textBoxR3C4";
			this.textBoxR3C4.Size = new System.Drawing.Size(40, 20);
			this.textBoxR3C4.TabIndex = 17;
			this.textBoxR3C4.Text = "textBox18";
			// 
			// textBoxR3C3
			// 
			this.textBoxR3C3.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(224)), ((System.Byte)(224)), ((System.Byte)(224)));
			this.textBoxR3C3.Location = new System.Drawing.Point(104, 64);
			this.textBoxR3C3.Name = "textBoxR3C3";
			this.textBoxR3C3.Size = new System.Drawing.Size(40, 20);
			this.textBoxR3C3.TabIndex = 16;
			this.textBoxR3C3.Text = "textBox19";
			// 
			// textBoxR3C2
			// 
			this.textBoxR3C2.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR3C2.Location = new System.Drawing.Point(56, 64);
			this.textBoxR3C2.Name = "textBoxR3C2";
			this.textBoxR3C2.Size = new System.Drawing.Size(40, 20);
			this.textBoxR3C2.TabIndex = 15;
			this.textBoxR3C2.Text = "textBox20";
			// 
			// textBoxR3C1
			// 
			this.textBoxR3C1.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR3C1.Location = new System.Drawing.Point(8, 64);
			this.textBoxR3C1.Name = "textBoxR3C1";
			this.textBoxR3C1.Size = new System.Drawing.Size(40, 20);
			this.textBoxR3C1.TabIndex = 14;
			this.textBoxR3C1.Text = "textBox21";
			// 
			// textBoxR2C7
			// 
			this.textBoxR2C7.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR2C7.Location = new System.Drawing.Point(296, 40);
			this.textBoxR2C7.Name = "textBoxR2C7";
			this.textBoxR2C7.Size = new System.Drawing.Size(40, 20);
			this.textBoxR2C7.TabIndex = 13;
			this.textBoxR2C7.Text = "textBox8";
			// 
			// textBoxR2C6
			// 
			this.textBoxR2C6.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR2C6.Location = new System.Drawing.Point(248, 40);
			this.textBoxR2C6.Name = "textBoxR2C6";
			this.textBoxR2C6.Size = new System.Drawing.Size(40, 20);
			this.textBoxR2C6.TabIndex = 12;
			this.textBoxR2C6.Text = "textBox9";
			// 
			// textBoxR2C5
			// 
			this.textBoxR2C5.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR2C5.Location = new System.Drawing.Point(200, 40);
			this.textBoxR2C5.Name = "textBoxR2C5";
			this.textBoxR2C5.Size = new System.Drawing.Size(40, 20);
			this.textBoxR2C5.TabIndex = 11;
			this.textBoxR2C5.Text = "textBox10";
			// 
			// textBoxR2C4
			// 
			this.textBoxR2C4.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR2C4.Location = new System.Drawing.Point(152, 40);
			this.textBoxR2C4.Name = "textBoxR2C4";
			this.textBoxR2C4.Size = new System.Drawing.Size(40, 20);
			this.textBoxR2C4.TabIndex = 10;
			this.textBoxR2C4.Text = "textBox11";
			// 
			// textBoxR2C3
			// 
			this.textBoxR2C3.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR2C3.Location = new System.Drawing.Point(104, 40);
			this.textBoxR2C3.Name = "textBoxR2C3";
			this.textBoxR2C3.Size = new System.Drawing.Size(40, 20);
			this.textBoxR2C3.TabIndex = 9;
			this.textBoxR2C3.Text = "textBox12";
			// 
			// textBoxR2C2
			// 
			this.textBoxR2C2.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(192)));
			this.textBoxR2C2.Location = new System.Drawing.Point(56, 40);
			this.textBoxR2C2.Name = "textBoxR2C2";
			this.textBoxR2C2.Size = new System.Drawing.Size(40, 20);
			this.textBoxR2C2.TabIndex = 8;
			this.textBoxR2C2.Text = "textBox13";
			// 
			// textBoxR2C1
			// 
			this.textBoxR2C1.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR2C1.Location = new System.Drawing.Point(8, 40);
			this.textBoxR2C1.Name = "textBoxR2C1";
			this.textBoxR2C1.Size = new System.Drawing.Size(40, 20);
			this.textBoxR2C1.TabIndex = 7;
			this.textBoxR2C1.Text = "textBox14";
			// 
			// textBoxR1C7
			// 
			this.textBoxR1C7.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR1C7.Location = new System.Drawing.Point(296, 16);
			this.textBoxR1C7.Name = "textBoxR1C7";
			this.textBoxR1C7.Size = new System.Drawing.Size(40, 20);
			this.textBoxR1C7.TabIndex = 6;
			this.textBoxR1C7.Text = "textBox7";
			// 
			// textBoxR1C6
			// 
			this.textBoxR1C6.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR1C6.Location = new System.Drawing.Point(248, 16);
			this.textBoxR1C6.Name = "textBoxR1C6";
			this.textBoxR1C6.Size = new System.Drawing.Size(40, 20);
			this.textBoxR1C6.TabIndex = 5;
			this.textBoxR1C6.Text = "textBox6";
			// 
			// textBoxR1C5
			// 
			this.textBoxR1C5.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR1C5.Location = new System.Drawing.Point(200, 16);
			this.textBoxR1C5.Name = "textBoxR1C5";
			this.textBoxR1C5.Size = new System.Drawing.Size(40, 20);
			this.textBoxR1C5.TabIndex = 4;
			this.textBoxR1C5.Text = "textBox5";
			// 
			// textBoxR1C4
			// 
			this.textBoxR1C4.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR1C4.Location = new System.Drawing.Point(152, 16);
			this.textBoxR1C4.Name = "textBoxR1C4";
			this.textBoxR1C4.Size = new System.Drawing.Size(40, 20);
			this.textBoxR1C4.TabIndex = 3;
			this.textBoxR1C4.Text = "textBox4";
			// 
			// textBoxR1C3
			// 
			this.textBoxR1C3.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR1C3.Location = new System.Drawing.Point(104, 16);
			this.textBoxR1C3.Name = "textBoxR1C3";
			this.textBoxR1C3.Size = new System.Drawing.Size(40, 20);
			this.textBoxR1C3.TabIndex = 2;
			this.textBoxR1C3.Text = "textBox3";
			// 
			// textBoxR1C2
			// 
			this.textBoxR1C2.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR1C2.Location = new System.Drawing.Point(56, 16);
			this.textBoxR1C2.Name = "textBoxR1C2";
			this.textBoxR1C2.Size = new System.Drawing.Size(40, 20);
			this.textBoxR1C2.TabIndex = 1;
			this.textBoxR1C2.Text = "textBox2";
			// 
			// textBoxR1C1
			// 
			this.textBoxR1C1.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(192)), ((System.Byte)(255)), ((System.Byte)(255)));
			this.textBoxR1C1.Location = new System.Drawing.Point(8, 16);
			this.textBoxR1C1.Name = "textBoxR1C1";
			this.textBoxR1C1.Size = new System.Drawing.Size(40, 20);
			this.textBoxR1C1.TabIndex = 0;
			this.textBoxR1C1.Text = "textBoxR1C1";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.textBoxDivider);
			this.groupBox2.Location = new System.Drawing.Point(16, 224);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(72, 48);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Divider";
			// 
			// textBoxDivider
			// 
			this.textBoxDivider.Location = new System.Drawing.Point(8, 16);
			this.textBoxDivider.Name = "textBoxDivider";
			this.textBoxDivider.Size = new System.Drawing.Size(48, 20);
			this.textBoxDivider.TabIndex = 0;
			this.textBoxDivider.Text = "textBox50";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.comboBoxType);
			this.groupBox3.Location = new System.Drawing.Point(96, 224);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(136, 48);
			this.groupBox3.TabIndex = 2;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Type";
			// 
			// comboBoxType
			// 
			this.comboBoxType.Location = new System.Drawing.Point(8, 16);
			this.comboBoxType.Name = "comboBoxType";
			this.comboBoxType.Size = new System.Drawing.Size(121, 21);
			this.comboBoxType.TabIndex = 0;
			this.comboBoxType.Text = "comboBox1";
			// 
			// button_Apply
			// 
			this.button_Apply.Location = new System.Drawing.Point(24, 16);
			this.button_Apply.Name = "button_Apply";
			this.button_Apply.TabIndex = 3;
			this.button_Apply.Text = "Apply";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.button_Cancel);
			this.groupBox4.Controls.Add(this.button_OK);
			this.groupBox4.Controls.Add(this.button_Apply);
			this.groupBox4.Location = new System.Drawing.Point(240, 224);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(120, 120);
			this.groupBox4.TabIndex = 4;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Action";
			// 
			// button_Cancel
			// 
			this.button_Cancel.Location = new System.Drawing.Point(23, 80);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.TabIndex = 5;
			this.button_Cancel.Text = "Cancel";
			// 
			// button_OK
			// 
			this.button_OK.Location = new System.Drawing.Point(24, 48);
			this.button_OK.Name = "button_OK";
			this.button_OK.TabIndex = 4;
			this.button_OK.Text = "OK";
			// 
			// Convolution
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(376, 358);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "Convolution";
			this.Text = "Convolution";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void textBox25_TextChanged(object sender, System.EventArgs e)
		{
		
		}
	}
}
