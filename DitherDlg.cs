using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Viewer
{
	/// <summary>
	/// Summary description for DitherDlg.
	/// </summary>
	public class DitherDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button_FM;
		private System.Windows.Forms.Button button_AM;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		ImageView m_Orig;
		ImageView m_prv;
		private System.Windows.Forms.Button button_Stochastic;
		Log log;

		public DitherDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			log = new Log("log.txt");
			
		}

		public bool OnInitForm(ImageView fm)
		{
			try
			{
				m_Orig = fm;
				this.Text = fm.Text;

				/* Show a preview image */
				if (m_prv != null)
					m_prv.Dispose();

				m_prv = new ImageView();
				m_prv.MdiParent = this.ParentForm;
				m_prv.OnInitForm("Preview", (Bitmap)fm.m_Img);
				m_prv.OnFormSize(m_Orig.Width - m_Orig.XPADD, m_Orig.Height - m_Orig.YPADD);
				m_prv.Show();
				return true;
			}
			catch (Exception e)
			{
				log.Write(e.ToString());
			}
			return false;
		}

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
			this.button_FM = new System.Windows.Forms.Button();
			this.button_AM = new System.Windows.Forms.Button();
			this.button_OK = new System.Windows.Forms.Button();
			this.button_Cancel = new System.Windows.Forms.Button();
			this.button_Stochastic = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// button_FM
			// 
			this.button_FM.Location = new System.Drawing.Point(16, 24);
			this.button_FM.Name = "button_FM";
			this.button_FM.Size = new System.Drawing.Size(136, 23);
			this.button_FM.TabIndex = 0;
			this.button_FM.Text = "Error Diffusion";
			this.button_FM.Click += new System.EventHandler(this.button_FM_Click);
			// 
			// button_AM
			// 
			this.button_AM.Location = new System.Drawing.Point(16, 56);
			this.button_AM.Name = "button_AM";
			this.button_AM.Size = new System.Drawing.Size(136, 23);
			this.button_AM.TabIndex = 1;
			this.button_AM.Text = "Halftone (square)";
			this.button_AM.Click += new System.EventHandler(this.button_AM_Click);
			// 
			// button_OK
			// 
			this.button_OK.Location = new System.Drawing.Point(176, 128);
			this.button_OK.Name = "button_OK";
			this.button_OK.TabIndex = 2;
			this.button_OK.Text = "OK";
			this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
			// 
			// button_Cancel
			// 
			this.button_Cancel.Location = new System.Drawing.Point(176, 160);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.TabIndex = 3;
			this.button_Cancel.Text = "Cancel";
			this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
			// 
			// button_Stochastic
			// 
			this.button_Stochastic.Location = new System.Drawing.Point(16, 88);
			this.button_Stochastic.Name = "button_Stochastic";
			this.button_Stochastic.Size = new System.Drawing.Size(136, 23);
			this.button_Stochastic.TabIndex = 4;
			this.button_Stochastic.Text = "Stochastic";
			this.button_Stochastic.Click += new System.EventHandler(this.button_Stochastic_Click);
			// 
			// DitherDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 206);
			this.Controls.Add(this.button_Stochastic);
			this.Controls.Add(this.button_Cancel);
			this.Controls.Add(this.button_OK);
			this.Controls.Add(this.button_AM);
			this.Controls.Add(this.button_FM);
			this.Name = "DitherDlg";
			this.Text = "DitherDlg";
			this.ResumeLayout(false);

		}
		#endregion

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			m_prv.Dispose();
			this.Dispose();
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			m_Orig.OnInitForm(m_Orig.Text, (Bitmap)m_prv.m_Img);
			button_Cancel_Click(sender, e);
		}

		private void button_FM_Click(object sender, System.EventArgs e)
		{
			Dither dt = new Dither("log.txt");
			dt.SetImage(m_prv.m_Img);
			dt.ToGray();
			dt.ErrDiff();
			m_prv.m_Img = dt.GetImage();
			m_prv.Repaint();
		}

		private void button_AM_Click(object sender, System.EventArgs e)
		{
			Dither dt = new Dither("log.txt");
			dt.SetImage(m_prv.m_Img);
			dt.ToGray();
			dt.SetScreen((int)DScreen.Square);
			dt.Halftone();
			m_prv.m_Img = dt.GetImage();
			m_prv.Repaint();
		}

		private void button_Stochastic_Click(object sender, System.EventArgs e)
		{
			Dither dt = new Dither("log.txt");
			dt.SetImage(m_prv.m_Img);
			dt.ToGray();
			dt.SetScreen((int)DScreen.Random);
			dt.Stochastic();
			m_prv.m_Img = dt.GetImage();
			m_prv.Repaint();
		}
	}
}
