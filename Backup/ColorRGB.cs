// ============================================================================
// Module:		ColorRGB
//
// Description:	Processing for RGB color image
//
// Purpose:		various conversions for RGB color image
//
// Input:		A raster image
// Output:		Adjusted image 
//
// Author:		Chi Toung Yeung			cty
//
// History:
// 26Feb07		Started work, need to set palette							cty
// 27Feb07		GetChannel() completed										cty
// 01Mar07		Added splitRGB_default() and splitRGB_24bpp() to cover all
//				bit depth situations.										cty
// ============================================================================
using System;
using System.Drawing;
using System.Drawing.Imaging;					// bitmap data

namespace Viewer
{
	/// <summary>
	/// Summary description for ColorRGB.
	/// </summary>
	///
	enum ClrChannel:int{Blue, Green, Red, Gray};
	public class ColorRGB
	{
		Image m_Img;

		///////////////////////////////////////////////////////////////////////////////////////////
		// Public

		// ====================================================================
		//	Description:	Constructor
		//	Return:			void
		public ColorRGB(ImageView fm)
		// ====================================================================
		{
			m_Img = fm.m_Img;
		}

		// ====================================================================
		// Description:	Get Red channel as an image
		// Return:		Bitmap of red channel image
		public Bitmap GetChannel ( int iColor )	// [in] color plane to get
		// ====================================================================
		{
			Bitmap Obmp = (Bitmap)m_Img;
			Bitmap Nbmp = new Bitmap ( Obmp.Width, Obmp.Height, PixelFormat.Format8bppIndexed );

			/* need to set palette */
			ColorPalette pal = Nbmp.Palette; 
			for ( int i = 0; i < 256; i ++ )
				pal.Entries[i] = Color.FromArgb(i, i, i);;
		
			Nbmp.Palette = pal; // The crucial statement 

			switch ( m_Img.PixelFormat )
			{
				case PixelFormat.Format24bppRgb:
					SplitRGB_24bpp ( Obmp, Nbmp, iColor );
					break;

				default:
					SplitRGB_default ( Obmp, Nbmp, iColor );
					break;
			};
			return Nbmp;
		}

		// ====================================================================
		// Description:	Is the image a gray scale photo ?
		// Return:		true if it is gray
		//				false if not
		public bool IsGray ()
		// ====================================================================
		{
			//if ( m_Img.PixelFormat == PixelFormat.Format24bppRgb ) ||
			/* if there is a palette, that is all I have to evaluate ! */
			return true;
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// private

		// ====================================================================
		// Description:	walk through image and split 24bpp RGB into 3 8bpp images
		private void SplitRGB_24bpp (Bitmap Obmp,
									 Bitmap Nbmp,
									 int iColor )
		// ====================================================================
		{
			BitmapData Odata = Obmp.LockBits( new Rectangle( 0 , 0 , Obmp.Width , Obmp.Height ) , 
				ImageLockMode.ReadWrite  , PixelFormat.Format24bppRgb  );

			BitmapData Ndata = Nbmp.LockBits( new Rectangle( 0 , 0 , Nbmp.Width , Nbmp.Height ) , 
				ImageLockMode.ReadWrite  , PixelFormat.Format8bppIndexed  );

			unsafe
			{ 
				byte* Optr = ( byte* )( Odata.Scan0 );
				byte* Nptr = ( byte* )( Ndata.Scan0 ); 
 
				for ( int y = 0; y < Obmp.Height; y ++ )
				{
					for ( int x = 0; x < Obmp.Width; x ++ )
					{
						byte b = *Optr; Optr ++;
						byte g = *Optr; Optr ++;
						byte r = *Optr; Optr ++;

						switch ( iColor )
						{
							case (int)ClrChannel.Blue:
								*Nptr = b; Nptr ++;
								break;

							case (int)ClrChannel.Green:
								*Nptr = g; Nptr ++;
								break;

							case (int)ClrChannel.Red:
								*Nptr = r; Nptr ++;
								break;

							case (int)ClrChannel.Gray:
								*Nptr = (byte)(( (double)b + (double)g + (double)r ) / 3.0); Nptr ++;
								break;
						};
					}
					Optr += Odata.Stride - Odata.Width*3;
					Nptr += Ndata.Stride - Ndata.Width;
				}
				Obmp.UnlockBits(Odata);
				Nbmp.UnlockBits(Ndata);
			}
		}

		// ====================================================================
		//	Description:	Split whatever bitdepth RGB image into 3 8bpp images
		private void SplitRGB_default (Bitmap Obmp,
									   Bitmap Nbmp,
									   int iColor )
		// ====================================================================
		{
			BitmapData Ndata = Nbmp.LockBits( new Rectangle( 0 , 0 , Nbmp.Width , Nbmp.Height ) , 
				ImageLockMode.ReadWrite  , PixelFormat.Format8bppIndexed  );

			unsafe
			{ 
				byte* Nptr = ( byte* )( Ndata.Scan0 ); 
 
				for ( int y = 0; y < Obmp.Height; y ++ )
				{
					for ( int x = 0; x < Obmp.Width; x ++ )
					{
						Color clr = Obmp.GetPixel(x,y);

						switch ( iColor )
						{
							case (int)ClrChannel.Blue:
								*Nptr = clr.B; Nptr ++;
								break;

							case (int)ClrChannel.Green:
								*Nptr = clr.G; Nptr ++;
								break;

							case (int)ClrChannel.Red:
								*Nptr = clr.R; Nptr ++;
								break;

							case (int)ClrChannel.Gray:
								*Nptr = (byte)(( (double)clr.B + (double)clr.G + (double)clr.R ) / 3.0); Nptr ++;
								break;
						};
					}
					Nptr += Ndata.Stride - Ndata.Width;
				}
				Nbmp.UnlockBits(Ndata);
			}
		}
	}
	
}
