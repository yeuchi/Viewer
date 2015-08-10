// ============================================================================
// Module:		Unsharp Mask
//
// Description:	sharpening
//
// Purpose:		An image processing application
//
// Input:		A raster image
// Output:		A raster image
//
// Author:		Chi Toung Yeung			cty
//
// History:	
// 14Feb07		1st start and completion									cty
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
	/// <summary>
	/// Summary description for UnSharpMask.
	/// </summary>
	public class UnSharpMask : System.Windows.Forms.Form
	{
		/* Variables declaration */
		public ImageView m_Orig;									// point to the original
		public ImageView m_prv;										// a preview image window
		int m_iKernWid;

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ComboBox comboBox_Radius;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// ///////////////////////////////////////////////////////////////////////////////////////
		// Public - UI

		// ====================================================================
		// Description: Constructor - initialize the object
		public UnSharpMask()
		// ====================================================================
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			button_OK.Click += new EventHandler ( OnOK );
			button_Cancel.Click += new EventHandler ( OnCancel );
			comboBox_Radius.SelectedIndexChanged += new EventHandler ( OnComboClick );
			
			comboBox_Radius.Items.Clear();
			comboBox_Radius.Items.Add ("3");
			comboBox_Radius.Items.Add ("5");
			comboBox_Radius.Items.Add ("7");
			comboBox_Radius.Items.Add ("9");
			comboBox_Radius.Items.Add ("11");
		}

		// ====================================================================
		// Description:	Initialized the object
		// Return:		void
		public void OnInitForm ( ImageView fm )  // [in] image object
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
	
			comboBox_Radius.SelectedIndex = 0;
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
			m_iKernWid = comboBox_Radius.SelectedIndex * 2 + 3;
			Apply();
		}

		/// ///////////////////////////////////////////////////////////////////////////////////////
		// Public
	
		// ====================================================================
		// Description:	Apply unsharpmask
		// Return:		void
		public void Apply ()
		// ====================================================================
		{
			this.Cursor = Cursors.WaitCursor;
			m_prv.OnInitForm ( "Preview", (Bitmap)m_Orig.m_Img );
			m_prv.Location = new Point ( m_Orig.Location.X + m_Orig.Width/2, m_Orig.Location.Y+m_Orig.Height/2);

			Bitmap bmp = (Bitmap)m_prv.m_Img;
			BitmapData Bmpdata = bmp.LockBits ( new Rectangle (0,0, bmp.Width, bmp.Height ), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb );

			Bitmap tmp = (Bitmap)bmp.Clone();
			BitmapData Tmpdata = tmp.LockBits ( new Rectangle (0,0, tmp.Width, tmp.Height ), 
				ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb );

			
			/* create kernel */
			double dKernTtl = m_iKernWid*m_iKernWid;
			int iPad = m_iKernWid / 2;
			int [] m_pKern = new int[m_iKernWid*m_iKernWid];
			for ( int i = 0; i < m_iKernWid*m_iKernWid; i ++ )
				m_pKern[i] = 1;

			/* do bluring */
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
						*Bmpptr = Cap((double)(*Bmpptr)+(double)(*Bmpptr)-dSumB/dKernTtl); Bmpptr ++;
						*Bmpptr = Cap((double)(*Bmpptr)+(double)(*Bmpptr)-dSumG/dKernTtl); Bmpptr ++;
						*Bmpptr = Cap((double)(*Bmpptr)+(double)(*Bmpptr)-dSumR/dKernTtl); Bmpptr ++;
					}
					Bmpptr = (byte*)(Bmpdata.Scan0) + (Bmpdata.Stride * (iPad+1+y)) + (3*(iPad+1));
				}
			}
			bmp.UnlockBits(Bmpdata);
			tmp.UnlockBits(Tmpdata);

			this.Cursor = Cursors.Default;
			m_prv.Repaint();
		}

		/// ///////////////////////////////////////////////////////////////////////////////////////
		// Private

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
			this.button_OK = new System.Windows.Forms.Button();
			this.button_Cancel = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.comboBox_Radius = new System.Windows.Forms.ComboBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.button_OK);
			this.groupBox1.Controls.Add(this.button_Cancel);
			this.groupBox1.Location = new System.Drawing.Point(144, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(112, 80);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Action";
			// 
			// button_OK
			// 
			this.button_OK.Location = new System.Drawing.Point(16, 16);
			this.button_OK.Name = "button_OK";
			this.button_OK.TabIndex = 0;
			this.button_OK.Text = "OK";
			// 
			// button_Cancel
			// 
			this.button_Cancel.Location = new System.Drawing.Point(16, 48);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.TabIndex = 1;
			this.button_Cancel.Text = "Cancel";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.comboBox_Radius);
			this.groupBox2.Location = new System.Drawing.Point(16, 16);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(112, 48);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Kernel Radius";
			// 
			// comboBox_Radius
			// 
			this.comboBox_Radius.Location = new System.Drawing.Point(16, 16);
			this.comboBox_Radius.Name = "comboBox_Radius";
			this.comboBox_Radius.Size = new System.Drawing.Size(72, 21);
			this.comboBox_Radius.TabIndex = 0;
			this.comboBox_Radius.Text = "comboBox1";
			// 
			// UnSharpMask
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(272, 118);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "UnSharpMask";
			this.Text = "UnSharpMask";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
