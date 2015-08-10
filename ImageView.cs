// ============================================================================
// Module:		ImageView
//
// Description:	An image window
//
// Purpose:		Display an image
//
// Input:		A raster image
// Output:		A raster image
//
// Author:		Chi Toung Yeung			cty
//
// History:		
// 10Jan07		Added comments												cty
// 11Jan07		Added OnCrop(), OnMouseDown(), OnMouseUp(), OnMouseMove()
//				Improved OnInitForm()										cty
// 11Jan07		Magic numbers added to OnFormSize() to create a white 
//				border around image.										cty
// 17Jan07		Repaint														cty
// 19Feb07		OnformSize() made public, 
//				start size is 1/3 smaller									cty
// 02Apr07		A work around for Image.FromFile() - Init					cty
// ============================================================================
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;

namespace Viewer
{
	/// <summary>
	/// Summary description for ImageView.
	/// </summary>
	public class ImageView : System.Windows.Forms.Form
	{
		public int XPADD = 18;
		public int YPADD = 45;

		public Image m_Img;
		private System.Windows.Forms.PictureBox pictureBox1;

		private Point lastPoint = Point.Empty;
		private Point origPoint = Point.Empty;
		private Rectangle rectSel;

		string m_sRc;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ImageView()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
	
			
			this.MouseWheel += new MouseEventHandler ( OnMouseWheel );
			pictureBox1.MouseUp += new MouseEventHandler ( OnMouseUp );
			pictureBox1.MouseMove += new MouseEventHandler ( OnMouseMove );
			pictureBox1.MouseDown += new MouseEventHandler ( OnMouseDown );
		}
		///////////////////////////////////////////////////////////////////////////////////////////
		// Public /////////////////////////////////////////////////////////////////////////////////

		void OnClosing()
		{
			Dispose();
		}

		// ====================================================================
		//	Description:	Initialize form
		//					Populate form with background image
		//					** always 1/3 of screen width
		public void OnInitForm ( string str		// [in] image title
								, Bitmap img )	// [in] image buffer
			// ====================================================================
		{
			double iWid, iLen;
			m_Img = (Image)img.Clone();
			this.Text = str;

			Form1 fm = (Form1)this.ParentForm;
			int ScrnWid = ( fm.Width > fm.Height ) ? fm.Width : fm.Height;


			if ( m_Img.Width > m_Img.Height )
			{
				iWid = ((double)ScrnWid / 3.0);
				iLen = ((double)iWid / (double)m_Img.Width * (double)m_Img.Height );
			}
			else
			{
				iLen = ((double)ScrnWid / 3.0);
				iWid = ((double)iLen / (double)m_Img.Height * (double)m_Img.Width );
			}

			OnFormSize ( iWid, iLen );
		}

		// ====================================================================
		//	Description:	Initialize form
		//					Populate form with background image
		//					** always 1/3 of screen width
		public void OnInitForm ( String sFileName	// [in] file name, path
								, int ScrnWid		// [in] client width
								, int ScrnLen )		// [in] client height
			// ====================================================================
		{
			Image img = null;
			
			FileStream fs = File.OpenRead(sFileName);
			byte[] b = new byte[fs.Length];				
			fs.Read(b, 0, (int)fs.Length);
			MemoryStream ms = new MemoryStream(b);
			img = Image.FromStream(ms);

			OnInitForm ( sFileName, (Bitmap)img );
		}

		// ====================================================================
		// Description:	Rotate image 
		// Return:		void
		public void OnRotate ( double dDegrees )
			// ====================================================================
		{
			if ( dDegrees == 90.0 )
			{
				m_Img.RotateFlip (RotateFlipType.Rotate90FlipNone);
				OnFormSize ( pictureBox1.Height, pictureBox1.Width );
			}
			else if ( dDegrees == 180.0 )
			{
				m_Img.RotateFlip (RotateFlipType.Rotate180FlipNone);
				OnFormSize ( pictureBox1.Width, pictureBox1.Height );
			}
			else if ( dDegrees == 270.0 )
			{
				m_Img.RotateFlip (RotateFlipType.Rotate270FlipNone);
				OnFormSize ( pictureBox1.Height, pictureBox1.Width );
			}
			else if ( dDegrees > 0 )
			{
			}
		}

		// ====================================================================
		// Description:	Flip image X or Y plane
		// Return:		void
		public void OnFlip ( int iDirection )
			// ====================================================================
		{
			switch ( iDirection )
			{
				case (int)Flip.Horizontal:
					m_Img.RotateFlip (RotateFlipType.RotateNoneFlipX);
					break;

				case (int)Flip.Vertical:
					m_Img.RotateFlip (RotateFlipType.RotateNoneFlipY);
					break;
			};
			OnFormSize ( pictureBox1.Width, pictureBox1.Height );
		}

		// ====================================================================
		// Description:	Crop image
		// Return:		void
		public void OnCrop ()
			// ====================================================================
		{
			if ( ( rectSel.Width != 0 ) && ( rectSel.Height != 0 ) )
			{
				Bitmap bmp = new Bitmap(rectSel.Width, rectSel.Height);
				Rectangle rd = new Rectangle ( 0, 0, rectSel.Width, rectSel.Height);
				Graphics g = Graphics.FromImage ( bmp );
				g.DrawImage ( pictureBox1.Image, rd, rectSel, GraphicsUnit.Pixel );

				m_Img.Dispose();
				OnInitForm ( "Crop", bmp );
			}
		}
		
		// ====================================================================
		// Description:	Repaint
		// Return:		void
		public void Repaint()
		// ====================================================================
		{
			pictureBox1.Image = m_Img;
			pictureBox1.Invalidate();
		}

		
		// ====================================================================
		// Description:	Set form dimension
		// Return:		void
		public void OnFormSize ( double dWid
								, double dLen )
			// ====================================================================
		{
			
			this.Bounds = new Rectangle (0,0, (int)dWid+ XPADD, (int)dLen+ YPADD );
			pictureBox1.Bounds = new Rectangle ( 5, 5, (int)dWid, (int)dLen );
			pictureBox1.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
			pictureBox1.Image = m_Img;
			pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			pictureBox1.Refresh();
			this.AutoScroll = true;
			
		}

		/// ///////////////////////////////////////////////////////////////////////////
		/// Private

		// ====================================================================
		// Description:	Magnification of image
		//				** Always maintain aspect ratio
		void OnMouseWheel ( object sender			// [in] ??
							, MouseEventArgs e )	// [in] mouse input
		// ====================================================================
		{
			int iRight = this.Width;
			int iBottom = this.Height;
			double dMag = e.Delta;

			// zoom in
			dMag = (( dMag > 0 ) ? 1.5 : 0.5);

			iRight = (int)( dMag * (double)iRight);
			iBottom = (int)((double)iRight / (double)m_Img.Width * (double)m_Img.Height);
		
			OnFormSize ( iRight, iBottom );

		}

		// ====================================================================
		// Description:	Mouse click on form
		// Return:		void
		private void OnMouseDown ( object sender
								, MouseEventArgs e )
		// ====================================================================
		{
			// A selection box already exist
			if ( lastPoint != Point.Empty)
			{
				double wR, hR;

				wR = ((double)m_Img.Width / (double)pictureBox1.Width);
				hR = ((double)m_Img.Height / (double)pictureBox1.Height);

				// User clicked inside the selection box
				if ( (((double)e.X * wR) > rectSel.X ) && (((double)e.Y * hR) > rectSel.Y) 
					&& (((double)e.X * wR) < rectSel.Width+rectSel.X ) && (((double)e.Y * hR) < rectSel.Height+rectSel.Y) )
					OnCrop();

					// User clicked outside the selection box
				else		
				{
					pictureBox1.Invalidate();
					this.Update();
					origPoint = lastPoint = Point.Empty;
				}	
			}
				// Create a selection box
			else
			{
				lastPoint.X = e.X;
				lastPoint.Y = e.Y;
				origPoint = lastPoint;
			}
		}

		// ====================================================================
		// Description:	Mouse Up
		private void OnMouseUp ( object sender
								, MouseEventArgs e )
		// ====================================================================
		{
			/* Actual image pixel location is relational from pictureBox1's image pixels.
			 * Yes, there are really 2 images here, pictureBox and the actual image. 
			 * Need to calculate the actual selection dimension and positioning */
			rectSel.X = (int)((double)origPoint.X * (double)m_Img.Width / (double)pictureBox1.Width);
			rectSel.Y = (int)((double)origPoint.Y * (double)m_Img.Height / (double)pictureBox1.Height);

			rectSel.Width = (int)((double)Math.Abs (e.X - origPoint.X) * (double)m_Img.Width / (double)pictureBox1.Width);
			rectSel.Height = (int)((double)Math.Abs (e.Y - origPoint.Y) * (double)m_Img.Height / (double)pictureBox1.Height);
			origPoint = Point.Empty;
		}

		// ====================================================================
		// Description:	Mouse Move
		private void OnMouseMove ( object sender
			, MouseEventArgs e )
			// ====================================================================
		{
			// Draw selection box if mouse down
			if ( origPoint != Point.Empty )
			{
				Rectangle rect;
				int w = Magnitude(e.X - origPoint.X);
				int h = Magnitude(e.Y - origPoint.Y);
				rect = new Rectangle(origPoint.X, origPoint.Y, w, h );
				Graphics g = pictureBox1.CreateGraphics();
				Pen p1 = new Pen (Color.Red, 1 );
				p1.DashStyle = DashStyle.Dash;

				g.DrawImage(m_Img, 0, 0, pictureBox1.Width, pictureBox1.Height);
				g.DrawRectangle(p1, rect );
				g.Dispose();

				lastPoint.X = e.X;
				lastPoint.Y = e.Y;
			}
		}

		// ====================================================================
		// Description:	return the magnitude and not the direction
		// Return:		void
		private int Magnitude ( int value )
		// ====================================================================
		{
			if ( value < 0 )
				return ( value * -1 );

			return value;
		}

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
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(16, 16);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(264, 224);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// ImageView
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.pictureBox1);
			this.Name = "ImageView";
			this.Text = "ImageView";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
