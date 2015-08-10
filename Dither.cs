// ============================================================================
//	Company:		Center for Geographic Information Science (CGIS)
//					All rights reserved.	Copyright 2007
//
//	Module:			Dither.cs
//
//	Description:	Convert image from 4, 8, 24 bpp to 1 bpp.
//					Also has Color to gray method
//
//	Input:			Image 1, 4 ,8, 24 bpp
//	Output:			1 bpp for halftone dither
//					8 bpp for ToGray() method
//
//	Author:			Chi Toung (CT) Yeung
//
//	History:	
//	01Jun07			1st start and completion								cty
// ============================================================================

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Viewer
{
    enum DScreen : int { Round, Square, Diamond, Poisson, Random };

    public class Dither
    {
        Log log;
        Image img;
        int[] iScrn;
        int iScrnWid;
		Random rdm;

        /////////////////////////////////////////////////////////////////////////////////
        // Construction

        // ----------------------------------------------------------
        // Description: Construction
        public Dither(string sLogFile)
        // ----------------------------------------------------------
        {
            log = new Log(sLogFile);
            img = null;
			DateTime dt = new DateTime();
			rdm = new Random((int)dt.Millisecond);
        }

        /////////////////////////////////////////////////////////////////////////////////
        // Empty / Is Empty

        // ----------------------------------------------------------
        // Description: Empty object
        public void Empty()
        // ----------------------------------------------------------
        {
            if (img != null)
                img.Dispose();

            img = null;
        }

        // ----------------------------------------------------------
        // Description: Is object empty ?
        // Return:      true if is empty
        //              false if is not empty
        public bool IsEmpty()
        // ----------------------------------------------------------
        {
            if ( img == null )
                return true;
            return false;
        }

        /// ////////////////////////////////////////////////////////////////////////////
        // Set / get image

        // ----------------------------------------------------------
        // Description: Set image into object
        public void SetImage(Image img)
        // ----------------------------------------------------------
        {
            this.img = (Image)img.Clone();
        }

        // ----------------------------------------------------------
        // Description: Get image from object
        public Image GetImage()
        // ----------------------------------------------------------
        {
            return img;
        }

		/// ////////////////////////////////////////////////////////////////////////////
		// Set AM Screens

		// ----------------------------------------------------------
		// Description: Set default screen patterns
		// Return:      true if success
		//              false if failed
		public bool SetScreen(int iType)
		// ----------------------------------------------------------
		{
			iScrnWid = 6;
			iScrn = new int[iScrnWid * iScrnWid];

			int[] SQ = { 7,  5,  4, 13, 11,  9, 
						 0,  2,  6, 16, 12, 10,
						 3,  1,  8, 17, 15, 14,
						13, 11,  9,  7,  5,  4,
						15, 12, 10,  0,  2,  6,
						17, 16, 14,  3,  1,  8};
			int i, c, iNum, iPos;
			bool bFill = false;
			

			switch (iType)
			{
				case (int)DScreen.Round:
					return false;

				case (int)DScreen.Square:
					for (i = 0; i < iScrnWid * iScrnWid; i++)
						iScrn[i] = SQ[i]*15;
					break;

				case (int)DScreen.Diamond:
					return false;

				case (int)DScreen.Poisson:
					break;

				case (int)DScreen.Random:
					for ( i = 0; i < 36; i ++ )
						iScrn[i] = -1;

					for ( i = 0; i < 18; i ++ )
					{
						iPos = rdm.Next();	iPos = (iPos < 18)? iPos:iPos%18;
						iNum = rdm.Next();	iNum = (iNum < 18)? iNum:iNum%18;

						if ( iScrn[iPos] == -1 )
							iScrn[iPos] = i*15;
						else
						{
							bFill = false;
							while ( !bFill )
							{
								c = (( iPos+iNum ) < 18 )? iPos+iNum:(iPos+iNum)%18;
								if ( iScrn[c] == -1 )
								{
									iScrn[c] = i*15;
									bFill = true;
								}
								else
								{
									while ( iScrn[c] > -1 )
									{
										c++;
										c = (c < 18)? c:c%18;
										if ( iScrn[c] == -1 )
											bFill = true;
									}
									iScrn[c] = i*15;
								}
							}
						}
					}
					for ( i = 1; i < 19; i ++ )
						iScrn[iScrnWid*iScrnWid-i] = iScrn[i-1];
					break;

				default:
					return false;
			};
			return true;
		}

		// ----------------------------------------------------------
		// Description: Set screening pattern
		// Return:      true if success
		//              false if failed
		public bool SetScreen(int[] iScreen,    // [in] screen
							  int iWid)			// [in] screen width (height = width)
		// ----------------------------------------------------------
		{
			try
			{
				iScrn = new int[iWid];

				for (int i = 0; i < iWid; i++)
					iScrn[i] = iScreen[i];

				iScrnWid = iWid;
				return true;
			}
			catch (Exception e)
			{
				log.Write("Set Screen " + e.ToString());
			}
			return false;
		}

        /////////////////////////////////////////////////////////////////////////////////
        // Color to 8 bit palette gray

        // ----------------------------------------------------------
        // Description: convert image from color to gray
        // Return:      true if success
        //              false if failed
        public bool ToGray()
        // ----------------------------------------------------------
        {
            try
            {
                switch (img.PixelFormat)
                {
                    case PixelFormat.Format1bppIndexed:
                        PaltoGray(1);
                        break;

                    case PixelFormat.Format4bppIndexed:
                        PaltoGray(4);
                        break;

                    case PixelFormat.Format8bppIndexed:
                        PaltoGray(8);
                        break;

                    case PixelFormat.Format24bppRgb:
                        RGBtoGray();
                        break;

                    default:
                        return false;
                };
                return true;
            }
            catch (Exception e)
            {
                log.Write("Gray " + e.ToString());
            }
            return false;
        }

        // ----------------------------------------------------------
        // Description: convert the palette to gray
        // Return:      true if success
        //              false if failed
        private bool PaltoGray(int bpp )
        // ----------------------------------------------------------
        {
            // palette image, I don't have to actually convert the color pixels.
            // just have to change the palette.
            try
            {
				/* need to set palette */
				Bitmap bmpDes = (Bitmap)img;
				ColorPalette pal = bmpDes.Palette; 
				for ( int i = 0; i < Math.Pow(2, bpp); i ++ )
				{
					Color clr = pal.Entries[i];
					double dClr = (double)clr.R * 0.3; 
					dClr += ((double)clr.G * 0.59);
					dClr += ((double)clr.B * 0.11);
					pal.Entries[i] = Color.FromArgb((int)dClr,(int)dClr,(int)dClr);
				}
				bmpDes.Palette = pal; // The crucial statement 
            }
            catch (Exception e)
            {
                log.Write("PGray " + e.ToString());
            }
            return false;
        }

        // ----------------------------------------------------------
        // Description: convert the rgb image into paletted gray
        // Return:      true if success
        //              false if failed
        private bool RGBtoGray()
        // ----------------------------------------------------------
        {
			if ( img == null )
				return false;

			Color sClr;
			int G;
			Bitmap bmpSrc = (Bitmap)img;

			try
			{
				// create a new 8 bpp image
				Bitmap bmpDes = new Bitmap(img.Width, img.Height, PixelFormat.Format8bppIndexed);

				/* need to set palette */
				ColorPalette pal = bmpDes.Palette; 
				for ( int i = 0; i < 256; i ++ )
					pal.Entries[i] = Color.FromArgb(i, i, i);
		
				bmpDes.Palette = pal; // The crucial statement 

				// walk through image
				BitmapData desData = bmpDes.LockBits(new Rectangle(0,0,bmpDes.Width, bmpDes.Height),
									ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
				unsafe
				{
					byte *ptr = (byte*)(desData.Scan0);
					for ( int y = 0; y < img.Height; y ++)
					{
						for (int x = 0; x < img.Width; x ++ )
						{
							// NTSC formula for RGB to Y
							sClr = bmpSrc.GetPixel(x,y);
							G = (int)((double)sClr.R * 0.3 + (double)sClr.G * 0.59 + (double)sClr.B * 0.11);
							*ptr = (byte)Requant(G);
							ptr ++;
						}
						ptr = (byte*)(desData.Scan0)+(desData.Stride*y);
					}
				}
				bmpDes.UnlockBits(desData);
				img.Dispose();
				img = (Image)bmpDes;
				return true;
            }
            catch (Exception e)
            {
                log.Write("RGBtoGray " + e.ToString());
            }
            return false;
        }

		/// ////////////////////////////////////////////////////////////////////////////
		// Dithering algorithms

        // ----------------------------------------------------------
        // Description: Perform halftone
        // Return:      true if success
        //              false if failed
        public bool Halftone()
        // ----------------------------------------------------------
        {
			if ((img == null)||(iScrn == null)||(iScrnWid<=0)||
				(img.PixelFormat != PixelFormat.Format8bppIndexed))
				return false;

			Color sClr;
			int X, Y;
			Bitmap bmpSrc = (Bitmap)img;

            try
            {
				// create a new 1 bpp image
				Bitmap bmpDes = new Bitmap(img.Width, img.Height, PixelFormat.Format1bppIndexed);

				/* need to set palette */
				ColorPalette pal = bmpDes.Palette; 
				pal.Entries[0] = Color.FromArgb(0, 0, 0);
				pal.Entries[1] = Color.FromArgb(255, 255, 255);
				bmpDes.Palette = pal; // The crucial statement 

				// walk through image
				BitmapData desData = bmpDes.LockBits(new Rectangle(0,0,bmpDes.Width, bmpDes.Height),
					ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
				unsafe
				{
					byte *ptr = (byte*)(desData.Scan0);
					*ptr = 0;
					// walk through image
					Y = 0;
					byte bMask;
					int ipos = 0;

					for ( int y = 0; y < img.Height; y ++)
					{
						X = 0;
						for (int x = 0; x < img.Width; x ++ )
						{
							// screen the image
							sClr = bmpSrc.GetPixel(x,y);
							if ( sClr.G > iScrn[Y*iScrnWid+X] )
							{
								bMask =(byte)( 0x80 >> ipos );
								*ptr = (byte)(*ptr | bMask);
							}
							if ( ipos == 7 )
							{
								ptr ++;
								ipos = 0;
							}
							else
								ipos ++;

							if ( X < iScrnWid-1 )
								X++;
							else
								X = 0;
						}
						if ( Y < iScrnWid-1 )
							Y ++;
						else
							Y = 0;

						ptr = (byte*)(desData.Scan0)+(desData.Stride*y);
					}
				}
				bmpDes.UnlockBits(desData);
				// swap image
				img.Dispose();
				img = (Image)bmpDes;
				return true;
            }
            catch (Exception e)
            {
                log.Write("Halftone " + e.ToString());
            }
            return false;
        }

		// ----------------------------------------------------------
		// Description:	Adjust the gamma for image 
		//				this is specific for Error diffusion.
		// Return:		true if success
		//				false if failed
		private bool AdjGamma(double dVal)
		// ----------------------------------------------------------
		{
			if ( img.PixelFormat != PixelFormat.Format8bppIndexed )
				return false;

			// walk through image
			Bitmap bmp = (Bitmap)img;
			BitmapData imgData = bmp.LockBits(new Rectangle(0,0,bmp.Width, bmp.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
			unsafe
			{
				byte *ptr;
				for ( int y = 0; y < img.Height; y ++ )
				{
					ptr = (byte*)(imgData.Scan0)+(imgData.Stride*y);
					
					for ( int x = 0; x < img.Width; x ++ )
					{
						*ptr = (byte)(Math.Pow(*ptr,dVal)/Math.Pow(255.0, dVal)*255.0);
						ptr ++;
					}
				}
			}bmp.UnlockBits(imgData);
			return true;
		}

		// ----------------------------------------------------------
		// Description:	Adjust the gamma for image 
		//				this is specific for Error diffusion.
		// Return:		true if success
		//				false if failed
		private bool AdjBright(int iVal)
			// ----------------------------------------------------------
		{
			if ( img.PixelFormat != PixelFormat.Format8bppIndexed )
				return false;

			// walk through image
			Bitmap bmp = (Bitmap)img;
			BitmapData imgData = bmp.LockBits(new Rectangle(0,0,bmp.Width, bmp.Height),
				ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
			unsafe
			{
				byte *ptr;
				for ( int y = 0; y < img.Height; y ++ )
				{
					ptr = (byte*)(imgData.Scan0)+(imgData.Stride*y);
					
					for ( int x = 0; x < img.Width; x ++ )
					{
						*ptr = Requant(*ptr+iVal);
						ptr ++;
					}
				}
			}bmp.UnlockBits(imgData);
			return true;
		}

		// ----------------------------------------------------------
		// Description:	FM dithering algorithm
		// Return:		true if succeed
		//				false if failed
		public bool Stochastic()
			// ----------------------------------------------------------
		{
			if ((img == null)||(img.PixelFormat != PixelFormat.Format8bppIndexed))
				return false;

			DateTime now = DateTime.Now;
			Random rdm = new Random((int)now.ToFileTime());

			long x, y;
			try
			{
				// Palette is presumed to be gray
				// Create a 1st derivative image as a guide for dither priority.
				Bitmap bmpDrv = GetDerivative();
	
				// Get all the bitmaps ready
				Bitmap bmpSrc = (Bitmap)img;
				Bitmap bmpDes = new Bitmap (img.Width, img.Height, PixelFormat.Format1bppIndexed);
				Color drvClr, sClr;
				
				/* need to set palette */
				ColorPalette pal = bmpDes.Palette; 
				pal.Entries[0] = Color.FromArgb(0, 0, 0);
				pal.Entries[1] = Color.FromArgb(255, 255, 255);
				bmpDes.Palette = pal; // The crucial statement 

				// walk through image
				BitmapData desData = bmpDes.LockBits(new Rectangle(0,0,bmpDes.Width, bmpDes.Height),
					ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);

				BitmapData srcData = bmpSrc.LockBits(new Rectangle(0,0,bmpSrc.Width, bmpSrc.Height),
					ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

				unsafe
				{
					// walk through image
					byte bMask;
					long desPos, srcPos;
					// dither edges first
					for ( y = 0; y < img.Height-1; y ++ )
					{
						// image pointers
						desPos = ((y%2)>0)? 0 : (img.Width-1)/8;			// byte position 
						byte *srcPtr;																	// points to source
						byte *desPtr = (byte*)(((y%2)>0)? (byte*)(desData.Scan0)+(desData.Stride*y)	
							:(byte*)(desData.Scan0)+(desData.Stride*y)+desPos);				// points to destination
						
						bMask = (byte)(((y%2)>0)? 0x40 : 0x80 >> (img.Width-2)%8 );

						for ( x = 1; x < img.Width-1; x ++ )
						{
							// pointer for binary destination image
							srcPos = ((y%2)>0)? x : img.Width-1-x;
							drvClr = bmpDrv.GetPixel((int)srcPos,(int)y);								// look for an edge
							
							srcPtr = (byte*)(srcData.Scan0)+(srcData.Stride*y)+srcPos;
							double dErr = (255 - (double)*srcPtr)/4.0;											// calculate error

							if ( drvClr.G > 5 )		
							{
								// add a white pixel
								*desPtr = (byte)(*desPtr | bMask);
								dErr *= -1;
								*srcPtr = 0;

								// distribute error to next row
								srcPtr = (byte*)(srcData.Scan0)+(srcData.Stride*(y+1))+(srcPos-1);
								*srcPtr = Requant((int)*srcPtr + (int)dErr);
								
								srcPtr ++;
								*srcPtr = Requant((int)*srcPtr + (int)dErr);

								srcPtr ++;
								*srcPtr = Requant((int)*srcPtr + (int)dErr);

								srcPtr = (byte*)(srcData.Scan0)+(srcData.Stride*y)+srcPos;

								// distribute error to left/right
								if ((y%2)>0)																// propagate error to the left
								{
									srcPtr ++;
									*srcPtr = Requant((int)*srcPtr + (int)dErr);
								}
								else
								{
									srcPtr --;
									*srcPtr = Requant((int)*srcPtr + (int)dErr);
								}
							}

							if ((y%2)>0)																// propagate error to the left
							{
								desPtr = ((srcPos%8) == 0 )? ++desPtr : desPtr;
								bMask = (byte)(( bMask == 0x01 )? 0x80 : (bMask >> 1));
							}
							else
							{
								desPtr = ((srcPos%8) == 0 )? --desPtr : desPtr;
								bMask = (byte)(( bMask == 0x80 )? 0x01 : (bMask << 1));
							}
						}
						bmpSrc.UnlockBits(srcData);
						
						// pattern screening
						desPtr = (byte*)(desData.Scan0);
						int X, Y = 0;
						int ipos = 0;
						for ( y = 0; y < img.Height; y ++)
						{
							X = 0;
							for ( x = 0; x < img.Width; x ++ )
							{
								// screen the image
								sClr = bmpSrc.GetPixel((int)x,(int)y);
								if ( sClr.G > iScrn[Y*iScrnWid+X] )
								{
									bMask =(byte)( 0x80 >> ipos );
									*desPtr = (byte)(*desPtr | bMask);
								}
								if ( ipos == 7 )
								{
									desPtr ++;
									ipos = 0;
								}
								else
									ipos ++;

								if ( X < iScrnWid-1 )
									X++;
								else
								{
									SetScreen((int)DScreen.Random);
									X = 0;
								}
							}
							if ( Y < iScrnWid-1 )
								Y ++;
							else
								Y = 0;

							desPtr = (byte*)(desData.Scan0)+(desData.Stride*y);
						}
					}
					bmpDes.UnlockBits(desData);
					
				}
				// swap image
				img.Dispose();
				bmpDrv.Dispose();
				img = (Image)bmpDes;
				return true;
			}
			catch (Exception e)
			{
				log.Write("ErrDiff " + e.ToString());
			}
			return false;
		}

		// ----------------------------------------------------------
		// Description: Perform error diffusion
		// Return:      true if success
		//              false if failed
		public bool ErrDiff()
		// ----------------------------------------------------------
		{
			if ((img == null)||(img.PixelFormat != PixelFormat.Format8bppIndexed))
				return false;

			DateTime now = DateTime.Now;
			Random rdm = new Random((int)now.ToFileTime());

			long x, y;
			try
			{
				// Palette is presumed to be gray
				// Create a 1st derivative image as a guide for dither priority.
				Bitmap bmpDrv = GetDerivative();
	
				// Get all the bitmaps ready
				Bitmap bmpSrc = (Bitmap)img;
				Bitmap bmpDes = new Bitmap (img.Width, img.Height, PixelFormat.Format1bppIndexed);
				Color drvClr;
				
				/* need to set palette */
				ColorPalette pal = bmpDes.Palette; 
				pal.Entries[0] = Color.FromArgb(0, 0, 0);
				pal.Entries[1] = Color.FromArgb(255, 255, 255);
				bmpDes.Palette = pal; // The crucial statement 

				// walk through image
				BitmapData desData = bmpDes.LockBits(new Rectangle(0,0,bmpDes.Width, bmpDes.Height),
					ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);

				BitmapData srcData = bmpSrc.LockBits(new Rectangle(0,0,bmpSrc.Width, bmpSrc.Height),
					ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

				unsafe
				{
					// walk through image
					byte bMask;
					long desPos, srcPos;
					// dither edges first
					for ( y = 0; y < img.Height-1; y ++ )
					{
						// image pointers
						desPos = ((y%2)>0)? 0 : (img.Width-1)/8;			// byte position 
						byte *srcPtr;																	// points to source
						byte *desPtr = (byte*)(((y%2)>0)? (byte*)(desData.Scan0)+(desData.Stride*y)	
										:(byte*)(desData.Scan0)+(desData.Stride*y)+desPos);				// points to destination
						
						bMask = (byte)(((y%2)>0)? 0x40 : 0x80 >> (img.Width-2)%8 );

						for ( x = 1; x < img.Width-1; x ++ )
						{
							// pointer for binary destination image
							srcPos = ((y%2)>0)? x : img.Width-1-x;
							drvClr = bmpDrv.GetPixel((int)srcPos,(int)y);								// look for an edge
							
							srcPtr = (byte*)(srcData.Scan0)+(srcData.Stride*y)+srcPos;
							double dErr = (255 - (double)*srcPtr)/4.0;											// calculate error

							if (( drvClr.G > 10 )&&( *srcPtr > 127 ))									// past threshold 		
							{
								// add a white pixel
								*desPtr = (byte)(*desPtr | bMask);
								dErr *= -1;
								*srcPtr = 0;

								// distribute error to next row
								srcPtr = (byte*)(srcData.Scan0)+(srcData.Stride*(y+1))+(srcPos-1);
								*srcPtr = Requant((int)*srcPtr + (int)dErr);
								
								srcPtr ++;
								*srcPtr = Requant((int)*srcPtr + (int)dErr);

								srcPtr ++;
								*srcPtr = Requant((int)*srcPtr + (int)dErr);

								srcPtr = (byte*)(srcData.Scan0)+(srcData.Stride*y)+srcPos;

								// distribute error to left/right
								if ((y%2)>0)																// propagate error to the left
								{
									srcPtr ++;
									*srcPtr = Requant((int)*srcPtr + (int)dErr);
								}
								else
								{
									srcPtr --;
									*srcPtr = Requant((int)*srcPtr + (int)dErr);
								}
							}

							if ((y%2)>0)																// propagate error to the left
							{
								desPtr = ((srcPos%8) == 0 )? ++desPtr : desPtr;
								bMask = (byte)(( bMask == 0x01 )? 0x80 : (bMask >> 1));
							}
							else
							{
								desPtr = ((srcPos%8) == 0 )? --desPtr : desPtr;
								bMask = (byte)(( bMask == 0x80 )? 0x01 : (bMask << 1));
							}
						}
					}
					for ( y = 0; y < img.Height-1; y ++ )
					{
						// image pointers
						desPos = ((y%2)>0)? 0 : (img.Width-1)/8;										// byte position 
						byte *srcPtr;																	// points to source
						byte *desPtr = (byte*)(((y%2)>0)? (byte*)(desData.Scan0)+(desData.Stride*y)	
							:(byte*)(desData.Scan0)+(desData.Stride*y)+desPos);							// points to destination
						
						bMask = (byte)(((y%2)>0)? 0x40 : 0x80 >> (img.Width-2)%8 );

						for ( x = 1; x < img.Width-1; x ++ )
						{
							// pointer for binary destination image
							srcPos = ((y%2)>0)? x : img.Width-1-x;
							drvClr = bmpDrv.GetPixel((int)srcPos,(int)y);								// look for an edge
							
							srcPtr = (byte*)(srcData.Scan0)+(srcData.Stride*y)+srcPos;
							double dPrcnt = (double)rdm.Next(70)/100.0;
							double dErr = (255.0 - (double)*srcPtr);									// calculate error

							if (( drvClr.G <= 10 )&&( *srcPtr > 127 ))									// past threshold 		
							{
								// add a white pixel
								*desPtr = (byte)(*desPtr | bMask);
								dErr *= -1;
							}

							// distribute error to next row
							double dErrR = dErr * dPrcnt;
							srcPtr = (byte*)(srcData.Scan0)+(srcData.Stride*(y+1))+(srcPos-1);
							*srcPtr = Requant((int)*srcPtr + (int)dErrR);
								
							dErrR = (dErr - dErrR) / 3.0;
							srcPtr ++;
							*srcPtr = Requant((int)*srcPtr + (int)dErrR);

							dErr = dErrR;
							dErrR = dErrR*2*(double)rdm.Next(70)/100.0;
							srcPtr ++;
							*srcPtr = Requant((int)*srcPtr + (int)dErrR);

							dErrR = dErr - dErrR;
							srcPtr = (byte*)(srcData.Scan0)+(srcData.Stride*y)+srcPos;

							// distribute error to left/right
							if ((y%2)>0)																// propagate error to the left
							{
								srcPtr ++;
								*srcPtr = Requant((int)*srcPtr + (int)dErrR);

								desPtr = ((srcPos%8) == 0 )? ++desPtr : desPtr;
								bMask = (byte)(( bMask == 0x01 )? 0x80 : (bMask >> 1));
							}
							else
							{
								srcPtr --;
								*srcPtr = Requant((int)*srcPtr + (int)dErrR);

								desPtr = ((srcPos%8) == 0 )? --desPtr : desPtr;
								bMask = (byte)(( bMask == 0x80 )? 0x01 : (bMask << 1));
							}
						}
					}
					bmpDes.UnlockBits(desData);
					bmpSrc.UnlockBits(srcData);
				}
				// swap image
				img.Dispose();
				bmpDrv.Dispose();
				img = (Image)bmpDes;
				return true;
			}
			catch (Exception e)
			{
				log.Write("ErrDiff " + e.ToString());
			}
			return false;
		}	
	
		// --------------------------------------------------------------------
		// Description:	Absolute
		// Return:		positive integer
		private int Absol(int value)
		// --------------------------------------------------------------------
		{
			if (value < 0 )
				return (value*-1);
			return value;
		}

		// --------------------------------------------------------------------
		// Description:	Requantize the value
		private byte Requant(int value)
		// --------------------------------------------------------------------
		{
			if (value < 0 )
				return 0;

			else if ( value > 255)
				return 255;

			return (byte)value;
		}

		// --------------------------------------------------------------------
		// Description:	generate a 1st derivative image from 8 bpp gray
		private Bitmap GetDerivative()
		// --------------------------------------------------------------------
		{
			// Palette is presumed to be gray already
			if ( img.PixelFormat != PixelFormat.Format8bppIndexed )
				return null;
			
			Color clr1, clr2, clr3, clr4;
            int iSum;
			try
			{
				Bitmap bmpDes = new Bitmap(img.Width, img.Height, PixelFormat.Format8bppIndexed);
				Bitmap bmpSrc = (Bitmap)img;

				/* need to set palette */
				ColorPalette pal = bmpDes.Palette; 
				for ( int i = 0; i < 256; i ++ )
					pal.Entries[i] = Color.FromArgb(i, i, i);
				
				bmpDes.Palette = pal; // The crucial statement 

				// walk through image
				BitmapData desData = bmpDes.LockBits(new Rectangle(0,0,bmpDes.Width, bmpDes.Height),
					ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

				unsafe
				{
					byte *ptr = (byte*)(desData.Scan0);
					int x, y;
					for ( y = 0; y < bmpSrc.Height-1; y ++)
					{
						for ( x = 0; x < bmpSrc.Width-1; x ++ )
						{
							clr1 = bmpSrc.GetPixel(x, y); 
							clr2 = bmpSrc.GetPixel(x+1, y);
							clr3 = bmpSrc.GetPixel(x+1, y+1);
							clr4 = bmpSrc.GetPixel(x, y+1);

							iSum = Absol(((int)clr1.G*3) - (int)clr2.G - (int)clr3.G - (int)clr4.G);
							*ptr = (byte)iSum;
							ptr ++;
						}
						ptr = (byte*)(desData.Scan0)+(desData.Stride*y);
					}
					y = bmpSrc.Height-1;
				}
				bmpDes.UnlockBits(desData);
				return bmpDes;
			}
			catch (Exception e )
			{
                log.Write("GetDerivative " + e.ToString());
			}
			return null;
		}
    }
}
