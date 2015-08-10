using System;
using System.Drawing;
using System.Drawing.Imaging;					// bitmap data

namespace Viewer
{
	//enum Kernel:int{Custom, Identity, Lowpass, Highpass, Laplacian, Sobel_X, Sobel_Y, Roberts45, Roberts135, Prewitt_X, Prewitt_Y };
	
	public class Convolve
	{
		Image	 img;
		double   m_dKernTtl;
		double[] m_pKern;											// array to hold kernel
		int		 m_iKernWid;										// kernel width
		string	 m_sRc;

		/// /////////////////////////////////////////////////////////////////////////////
		/// Constructor
		
		// --------------------------------------------------------------------
		// Description:	Constructor
		public Convolve()
		// --------------------------------------------------------------------
		{
			img = null;
		}

		/// /////////////////////////////////////////////////////////////////////////////
		/// Empty / IsEmpty
		
		// --------------------------------------------------------------------
		// Description:	Empty the object
		public void Empty()
		// --------------------------------------------------------------------
		{
			if ( img != null)
				img.Dispose();
			img = null;
		}

		// --------------------------------------------------------------------
		// Description:	Is the object empty?
		// Return:		true if success
		//				false if failed
		public bool IsEmpty()
		// --------------------------------------------------------------------
		{
			if ( img == null )
				return true;
			return false;
		}

		/// /////////////////////////////////////////////////////////////////////////////
		/// Set Kernel
		
		// --------------------------------------------------------------------
		// Description:	Set kernel
		// Return:		true if success
		//				false if failed
		public bool SetKernel ( int iMethod )
		// --------------------------------------------------------------------
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

			return true;
		}

		// --------------------------------------------------------------------
		// Description:	Set kernel
		// Return:		true if success
		//				false if failed
		public bool SetKernel ( int iLen,			// [in] kernel array length
								double[] pdKern )	// [in] kernel array
		// --------------------------------------------------------------------
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

			return true;
		}

		/// /////////////////////////////////////////////////////////////////////////////
		/// Set / Get image
		
		// --------------------------------------------------------------------
		// Description:	Get image from this object
		// Return:		image if success
		//				null if empty
		public Image GetImage()
		// --------------------------------------------------------------------
		{
			return img;
		}

		// --------------------------------------------------------------------
		// Description:	Set image into object
		// Return:		true if success
		//				false if failed
		public bool SetImage(Image img)
		// --------------------------------------------------------------------
		{
			this.img = img;
			if ( img == null )
				return false;
			return true;
		}

		/// /////////////////////////////////////////////////////////////////////////////
		/// Apply convolution
		
		// --------------------------------------------------------------------
		// Description:	Apply convolution to 8 bpp gray image
		// Return:		true if success
		//				false if failed
		public bool Apply_toGray()
		// --------------------------------------------------------------------
		{
			// Palette is presumed to be gray already
			if ( img.PixelFormat != PixelFormat.Format8bppIndexed )
				return false;
			
			try
			{

				// temp destination bitmap
				Bitmap bmpDes = new Bitmap(img.Width, img.Height, PixelFormat.Format8bppIndexed );
				Bitmap bmpSrc = (Bitmap)img;

				BitmapData srcData = bmpSrc.LockBits(new Rectangle(0,0,bmpSrc.Width, bmpSrc.Height), 
					ImageLockMode.ReadOnly, bmpSrc.PixelFormat);
				BitmapData desData = bmpDes.LockBits(new Rectangle(0,0,bmpDes.Width, bmpDes.Height), 
					ImageLockMode.ReadWrite, bmpDes.PixelFormat);

				unsafe
				{
					byte *srcPtr = (byte*)(srcData.Scan0);
					byte *desPtr = (byte*)(desData.Scan0);

					for ( int y = 0; y < bmpSrc.Height; y ++)
					{
						for ( int x = 0; x < bmpSrc.Width; x ++ )
						{
						}
					}
				}
				bmpDes.UnlockBits(desData);
				bmpSrc.UnlockBits(srcData);
			
				img.Dispose();
				img = (Image)bmpDes;
				
				return true;
			}
			catch (Exception e )
			{
				m_sRc = "Apply_toGray " + e.ToString();
			}
			return false;
		}
	}
}
