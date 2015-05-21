using System;
using System.IO;
using System.Data;

namespace AUDIS {
	/// <summary>
	/// information about a given unify extract definition
	/// </summary>
	public class ExtractInformation {
		public int id;
		public int maxTimeout;
		public int runByHour;
		public string archivalConvention;
		public string localIdentifier;
		
		public string stagingTable;
		public string loadingSP;
		public string baseTable;
		public int skipRows;
		
		public string bookName;					// the book name within Unify
		public string extractName;				// the extract name within Unify
		
		public string extractAdditionalHt;		// additional IDs for fields to be set in conditional cases
		public string extractPeriodHtID;		// html ID for period string
		public string extractYearHtID;			// html ID for year (STRING!)
		public string[] setPopups;
		
		
		
		/// <summary>
		/// set the extract information
		/// </summary>
		/// <param name="theID">the db id for extract row</param>
		public ExtractInformation(int theID) {
			DataView result=DB.Query("SELECT * FROM "+Settings.tableExtract+" WHERE ID="+theID.ToString());	
			DataTable dT=result.Table;
			DataRow dr=dT.Rows[0];
			
			id=theID;
			string[] delim = new string[1];
			delim[0]=";";
				
			maxTimeout=(int) dr["maxTimeout"];
			runByHour=(int) dr["runByHour"];
			archivalConvention=dr["archivalConvention"].ToString();
			localIdentifier=dr["localIdentifier"].ToString();
			bookName=dr["bookName"].ToString();
			extractName=dr["extractName"].ToString();
			extractYearHtID=dr["extractYearHtID"].ToString();
			extractPeriodHtID=dr["extractPeriodHtID"].ToString();
			if (Convert.IsDBNull(dr["extractAdditionalHt"])) {
				extractAdditionalHt=null;
			} else {
				extractAdditionalHt=dr["extractAdditionalHt"].ToString();//.Split(delim,StringSplitOptions.None);
			}
			stagingTable=dr["stagingTable"].ToString();
			loadingSP=dr["loadingSP"].ToString();
			baseTable=dr["masterFinalTable"].ToString();
			skipRows=(int)dr["skipRows"];
			
			if (dr["setHtIDToAll"]!=DBNull.Value) {
				setPopups=dr["setHtIDtoAll"].ToString().Split(delim,StringSplitOptions.None);
			}
			
		}
		
		/// <summary>
		/// check if the extract is ready to download based on the current time and the runByHour field
		/// </summary>
		/// <returns></returns>
		public bool IsReady() {
			if (runByHour<=(Convert.ToInt32(DateTime.Now.ToString("HH"))*100)) {
				return true;
			} else {
				return false;
			}
		}
		

		
		
	}

	
}
