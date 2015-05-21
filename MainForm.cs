using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AUDIS
{
	

	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form {
		private Scheduled theJob;
		public MainForm() {
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			Instance.form=this;
			
		}
		
		public void AddLine(string toAdd){
			log.AppendText(toAdd+"\r\n");
		}
		
		public void RunAUDIS() {
			Settings.ReadIn();
			AddLine("Begin");
			try {
				DB.Connect();
				theJob=new Scheduled();
			} catch (Exception e) {
				AddLine("Core application error: "+e.ToString());
			}		
			Application.Exit();
		}
		
		void RichTextBox1TextChanged(object sender, EventArgs e) {
			
		}
		
		void LabelClick(object sender, EventArgs e) {
			
		}
		
		void MainFormLoad(object sender, EventArgs e) {
			this.Show();
			RunAUDIS();
		}
	}
}
