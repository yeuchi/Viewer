// ============================================================================
// Module:		BinaryTree
//
// Description:	sort an array of items and keep count - histogram
//
// Purpose:		more efficient method of creating color histogram R,G,B
//				specific for Volume histogram...
//
// Input:		A raster image
// Output:		RGB histogram
//
// Author:		Chi Toung Yeung			cty
//
// History:
// 23Jan07		transfer code from C++										cty
// 24Jan07		ready for test and use										cty	
// 01Feb07		adjust Print method for C#									cty
// 11Feb07		Fixed Print()												cty
// ============================================================================

using System;

namespace Viewer
{
	/// <summary>
	/// Summary description for BinaryTree.
	/// </summary>
	public class BinaryTree
	{

		BinaryTree m_pLeft, m_pRight;
		ulong m_lCount = 0;
		ulong m_lItem = 0;

		///////////////////////////////////////////////////////////////////////////////////////////
		// Construct / Destruct

		// ====================================================================
		//	Description:	Constructor
		//	Return:			void
		public BinaryTree()
		// ====================================================================
		{
			m_pLeft = null;
			m_pRight = null;

			Empty();
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Empty / IsEmpty

		// ====================================================================
		//	Description:	
		//	Return:	
		public bool IsEmpty()
		// ====================================================================
		{
			if ( m_lItem != 0 )
				return false;

			return true;
		}

		// ====================================================================
		//	Description:	
		//	Return:	
		public void Empty()
		// ====================================================================
		{
			if ( m_pLeft != null )
			{
				m_pLeft = null;
			}

			if ( m_pRight != null )
			{
				m_pRight = null;
			}

			m_lItem = 0;
			m_lCount = 0;
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Insert / Remove / Print

		// ====================================================================
		//	Description:	
		//	Return:	
		public bool Insert ( ulong item )
		// ====================================================================
		{
			if ( m_lCount == 0 )
			{
				m_lItem = item;
				m_lCount ++;
				return true;
			}
			else if ( item == m_lItem )
			{
				m_lCount ++;
				return true;
			}
			else if ( item < m_lItem )
			{
				if ( m_pLeft == null )
				{
					m_pLeft = new BinaryTree();

					if ( m_pLeft == null )
						return false;
				}
				return ( m_pLeft.Insert ( item ) );
			}
			else if ( m_lItem < item )
			{
				if ( m_pRight == null )
				{
					m_pRight = new BinaryTree ();

					if ( m_pRight == null )
						return false;
				}
				return ( m_pRight.Insert ( item ) );
			}
			return false;
		}

		// ====================================================================
		//	Description:	
		//	Return:
		public bool Remove ( ulong item )
		// ====================================================================
		{
			if ( m_lItem == 0 )
				return false;
		
			else if ( item < m_lItem )
				return ( m_pLeft.Remove ( item ) );

			else if ( m_lItem < item )
				return ( m_pRight.Remove ( item ) );

			else if ( item == m_lItem )
			{
				if ( ( m_pLeft == null ) && ( m_pRight == null ) )
				{
					m_lItem = 0;
					m_lCount = 0;
				}
				else if ( m_pLeft != null )
				{
					m_lItem = m_pLeft.m_lItem;
					m_lCount = 1;
					Remove ( m_pLeft.m_lItem );
				}
				else if ( m_pRight != null )
				{
					m_lItem = m_pRight.m_lItem;
					m_lCount = 1;
					Remove ( m_pRight.m_lItem );
				}
			}
			return true;
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Count / Print - String and long

		// ====================================================================
		//	Description:	Count number of entries
		//	Return:			number of entries (long)
		public ulong NumEntries ()
		// ====================================================================
		{
			ulong lCount = 0;

			if ( m_pLeft != null )
				lCount = m_pLeft.NumEntries();

			if ( m_pRight != null )
				lCount += m_pRight.NumEntries();

			return ( ++lCount );
		}

		// ====================================================================
		//	Description:	
		//	Return:
		public ulong Print ( int []vol, ulong lCnt )
		// ====================================================================
		{
			ulong first, second, third, fourth;
			ulong lMask;
			//String tmp;

			if ( m_pLeft != null )
				lCnt = m_pLeft.Print (vol, lCnt);

			// black channel info
			lMask = 0xFF;
			fourth = m_lItem & lMask;
	
			// magenta or blue channel info
			lMask <<= 8;
			third = m_lItem & lMask;

			// yellow or green channel info
			lMask <<= 8;
			second = m_lItem & lMask;

			// cyan or red channel info
			lMask <<= 8;
			first = m_lItem & lMask;

			vol[lCnt*4] = (int)(first >> 24);
			vol[lCnt*4+1] = (int)(second >> 16);
			vol[lCnt*4+2] = (int)(third >> 8);
			vol[lCnt*4+3] =(int) m_lCount;

			++lCnt;

			if ( m_pRight != null )
				lCnt = m_pRight.Print (vol, lCnt);

			return lCnt;
		}

		///////////////////////////////////////////////////////////////////////////////////////////
		// Find

		// ====================================================================
		//	Description:	
		//	Return:
		public ulong FindMin ()
		// ====================================================================
		{
			if ( m_lItem == 0 )
				return 0;

			else if ( m_pLeft == null )
				return m_lItem;

			else
				return ( m_pLeft.FindMin() );
		}

		// ====================================================================
		//	Description:	
		//	Return:
		public ulong FindMax ()
		// ====================================================================
		{
			if ( m_lItem == 0 )
				return 0;

			else if ( m_pRight == null )
				return m_lItem;

			else
				return ( m_pRight.FindMax() );
		}

		// ====================================================================
		//	Description:	
		//	Return:
		public ulong FindFreq ( ulong item )
		// ====================================================================
		{
			if ( m_lItem == 0 )
				return 0;

			else if ( item == m_lItem )
				return m_lCount;

			else if ( item < m_lItem )
			{
				if ( m_pLeft == null )
					return 0;

				return ( m_pLeft.FindFreq ( item ) );
			}
			else if ( m_lItem < item )
			{
				if ( m_pRight == null )
					return 0;

				return ( m_pRight.FindFreq ( item ) );
			}
			return 0;
		}
	}
}
