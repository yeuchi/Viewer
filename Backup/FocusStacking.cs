using System;
using System.Drawing;
using System.Drawing.Imaging;					// bitmap data
using System.Data;
using System.Collections;

namespace Viewer
{
	public class FocusStacking
	{
		ArrayList aryImgList = null;
		Image dstImg = null;

		/// /////////////////////////////////////////////////////////////////////////////
		/// Constructor
		
		// --------------------------------------------------------------------
		// Description: Constructor
		public FocusStacking(ArrayList imgList)
		// --------------------------------------------------------------------
		{
		}

		/// /////////////////////////////////////////////////////////////////////////////
		/// Empty / IsEmpty
		/// 
		// --------------------------------------------------------------------
		// Description:
		public void Empty()
		// --------------------------------------------------------------------
		{
		}

		// --------------------------------------------------------------------
		// Description:
		public bool IsEmpty()
		// --------------------------------------------------------------------
		{
			return false;
		}

		/// /////////////////////////////////////////////////////////////////////////////
		/// Properties
		/// 
		// --------------------------------------------------------------------
		// Description: Get / Set image list property
		//
		public ArrayList imgList
		// --------------------------------------------------------------------
		{
			get{return aryImgList;}
			set{aryImgList = value;}
		}

		// --------------------------------------------------------------------
		// Description: Get / set destination property
		//
		public Image img
		// --------------------------------------------------------------------
		{
			get{return this.dstImg;}
			set{this.dstImg = value;}
		}

		/// /////////////////////////////////////////////////////////////////////////////
		/// Methods
		/// 
		// --------------------------------------------------------------------
		// Description:	Compare sharpness
		// Return:		success: the nth image that is in focus
		//				failed:	 -1
		public int EdgeCompare( int x,	// [in] x co-ordinate
								int y )	// [in] y co-ordinate
		// --------------------------------------------------------------------
		{
			try
			{
			}
			catch(Exception e)
			{
			}
			return -1;
		}

		// --------------------------------------------------------------------
		// Description:	Laplacian trasnform at specified
		// Return:		magnitude
		public int Laplacian (  int x,			// [in] x co-ordinate
								int y,			// [in] y co-ordinate
								int iDiameter )	// [in] diameter of filter
		// --------------------------------------------------------------------
		{
			try
			{
			}
			catch(Exception e)
			{
			}
			return -1;
		}
	}
}
