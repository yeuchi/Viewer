using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace Viewer
{
	/// <summary>
	/// Summary description for FocusStackDlg.
	/// </summary>
	public class FocusStackDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.ListView lstChoices;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ListView lstSelect;
		private System.Windows.Forms.Button btnStack;
		private System.Windows.Forms.Button btnOK;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FocusStackDlg(ArrayList m_list)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnStack = new System.Windows.Forms.Button();
			this.lstChoices = new System.Windows.Forms.ListView();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lstSelect = new System.Windows.Forms.ListView();
			this.btnOK = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(240, 224);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 0;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnStack
			// 
			this.btnStack.Location = new System.Drawing.Point(240, 160);
			this.btnStack.Name = "btnStack";
			this.btnStack.TabIndex = 1;
			this.btnStack.Text = "Stack";
			this.btnStack.Click += new System.EventHandler(this.btnStack_Click);
			// 
			// lstChoices
			// 
			this.lstChoices.Location = new System.Drawing.Point(8, 16);
			this.lstChoices.Name = "lstChoices";
			this.lstChoices.Size = new System.Drawing.Size(200, 96);
			this.lstChoices.TabIndex = 2;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.lstChoices);
			this.groupBox1.Location = new System.Drawing.Point(8, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(216, 120);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Choices";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.lstSelect);
			this.groupBox2.Location = new System.Drawing.Point(8, 152);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(216, 120);
			this.groupBox2.TabIndex = 5;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Selection";
			// 
			// lstSelect
			// 
			this.lstSelect.Location = new System.Drawing.Point(8, 16);
			this.lstSelect.Name = "lstSelect";
			this.lstSelect.Size = new System.Drawing.Size(200, 96);
			this.lstSelect.TabIndex = 2;
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(240, 192);
			this.btnOK.Name = "btnOK";
			this.btnOK.TabIndex = 6;
			this.btnOK.Text = "button1";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// FocusStackDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(336, 286);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnStack);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.groupBox2);
			this.Name = "FocusStackDlg";
			this.Text = "FocusStackDlg";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void btnStack_Click(object sender, System.EventArgs e)
		{
		
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
		
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
		
		}

		public void Init()
		{
		}
	}
}
