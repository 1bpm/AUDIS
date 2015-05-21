using System;
using System.IO;
using System.Windows.Forms;

namespace AUDIS
{

	
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program {

		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			MainForm mf=new MainForm();
			Application.Run(mf);
		}
		
		/// <summary>
		/// log to the form
		/// </summary>
		/// <param name="data">string to append to form text window</param>
		public static void Log(string data) {
			LogWriter(data);
			if (Instance.form!=null) Instance.form.AddLine(data);
		}
		
		private static void LogWriter(string line) {
			using (StreamWriter writer = File.AppendText(Settings.logFile)) {
				line=DateTime.Now.ToString()+" : "+line;
				writer.WriteLine(line);
			}
			
		}
		
	}
}
