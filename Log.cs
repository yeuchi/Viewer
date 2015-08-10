// ============================================================================
//	Company:		Center for Geographic Information Science (CGIS)
//					All rights reserved.	Copyright 2007
//
//  Module:         Log.cs
//  
//  Description:    read/write log file
//
//  Input:          a temporary log text
//  Output:         Append to a log text file specified
//
//  Author:         Chi Toung (CT) Yeung
//
//  History:
//  06Apr07         1st implementation                                      cty
//  19May07         added write with levels                                 cty
//  19May07         Added, default methods: empty, Isempty, get setpaths    cty
// ============================================================================
using System;
//using System.Collections.Generic;
using System.Text;
using System.IO;                    // Stream

namespace Viewer
{
    class Log
    {
        string m_str;
        string m_sReadFile;
        string m_sWriteFile;
        int m_iFilter;

        ////////////////////////////////////////////////////////////////////////////
        // Constructor

        // ----------------------------------------------------------
        //  Description:    Construct
        public Log(string sWriteFile)
        // ----------------------------------------------------------
        {
            m_sWriteFile = sWriteFile;            
        }

        // ----------------------------------------------------------
        //  Description:    Construct
        public Log(string sReadFile,
                    string sWriteFile)
        // ----------------------------------------------------------
        {
            m_sReadFile = sReadFile;
            m_sWriteFile = sWriteFile;
        }

        ////////////////////////////////////////////////////////////////////////////
        // Empty / IsEmpty

        // ----------------------------------------------------------
        //  Description: Is this object empty ?
        //  Return:         true if is empty
        //                  false if not
        public bool IsEmpty()
        // ----------------------------------------------------------
        {
            if (m_str.Length > 0)
                return false;
            return true;
        }

        // ----------------------------------------------------------
        //  Description: 
        public void Empty()
        // ----------------------------------------------------------
        {
            m_str = "";
        }

        ////////////////////////////////////////////////////////////////////////////
        // Read Write

        // ----------------------------------------------------------
        // Description: Read temp log file
        // Return:      true if success
        //              false if failed
        public bool Read()
        // ----------------------------------------------------------
        {
            try
            {
                m_str = "";
                FileInfo fi = new FileInfo(m_sReadFile);
                FileStream fs = fi.OpenRead();


                int b = fs.ReadByte();
                if ((b == -1) || (b == 0))
                {
                    m_str = "Read: Failed read byte Log file \n";
                    return false;
                }

                while (b != -1)
                {
                    m_str += (char)b;
                    b = fs.ReadByte();
                }
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                m_str = "Read(): Failed";
                m_str += e.ToString();
            }
            return false;
        }

        // ----------------------------------------------------------
        // Description: Writes the given string if above filter level
        // Return:      true if written
        //              false if not
        public bool Write(string str, int ilevel)
        // ----------------------------------------------------------
        {
            if (ilevel >= m_iFilter)
            {
                m_str = str;
                return Write();
            }
            return false;
        }

        // ----------------------------------------------------------
        // Description: Writes the given string
        // Return:      true if success
        //              false if failed
        public bool Write(string str)
        // ----------------------------------------------------------
        {
            m_str = str;
            return Write();
        }

        // ----------------------------------------------------------
        // Description: Write to a log file
        // Return:      true if success
        //              false if failed
        public bool Write()
        // ----------------------------------------------------------
        {
            try
            {
                FileStream fsOut = new FileStream(m_sWriteFile, FileMode.OpenOrCreate, FileAccess.Write);
                MemoryStream ms = new MemoryStream();

                DateTime now = DateTime.Now;
                string str = "The Date/Time now is:" + now.ToShortDateString() + " " + now.ToShortTimeString() + "\r\n" + m_str + "\r\n";
                
                char[] f = str.ToCharArray();
                byte[] b = new byte[str.Length];

                for (int i = 0; i < str.Length; i++)
                    b[i] = (byte)f[i];

                fsOut.Seek(0, SeekOrigin.End);
                fsOut.Write(b, 0, str.Length);
                fsOut.Close();
                return true;
            }
            catch (Exception e)
            {
                m_str = "Failed Error log\n";
                m_str += e.ToString();
                return false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        // Other methods

        // ----------------------------------------------------------
        // Description: Set the output path
        public void SetWritePath(string sPath)
        // ----------------------------------------------------------
        {
            m_sWriteFile = sPath;
        }

        // ----------------------------------------------------------
        // Description: Set input path
        // Return:      True if file found
        //              false if file not exist
        public bool SetReadPath(string sPath)
        // ----------------------------------------------------------
        {
            if (File.Exists(sPath))
            {
                m_sReadFile = sPath;
                return true;
            }
            return false;
        }

        // ----------------------------------------------------------
        // Description: Set the filter level
        public void SetFilter(int i)
        // ----------------------------------------------------------
        {
            m_iFilter = i;
        }

        // ----------------------------------------------------------
        // Description: get the filter level
        public int GetFilter()
        // ----------------------------------------------------------
        {
            return m_iFilter;
        }

        // ----------------------------------------------------------
        // Description: Return message
        public string GetError()
        // ----------------------------------------------------------
        {
            return m_str;
        }
    }
}
