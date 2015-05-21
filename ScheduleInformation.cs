using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Globalization;


namespace AUDIS {
	/// <summary>
	/// information pertaining to a scheduled extract
	/// </summary>
	public class ScheduleInformation {
		
			public int id;
			public DateTime duedate; //datetime
			public string period;
			public int version;
			public int completed=0;
			public int extractID;
			public ExtractInformation extract;
			private PageMechaniser automation;
		
			private int monthNum;
			public string strYear;
			public string strMonthTxt;
			public string strMonthNum;
			public string strQuarterTxt;
			public string strDayNum;
			public string singleDayDate;
		
		public ScheduleInformation(int theID) {
			DataView result=DB.Query("SELECT * FROM "+Settings.tableSchedule+" WHERE ID="+theID.ToString());
			DataTable tbl=result.Table;
			DataRow dr=tbl.Rows[0];
			id=theID;
			duedate=(DateTime)dr["duedate"];
			
			
			period=dr["period"].ToString();
			version=(int)dr["version"];
			extractID=(int)dr["extractID"];
			extract=new ExtractInformation(extractID);
			SetRelativePeriods();
		}
		
		public void Run() {
			automation=new PageMechaniser(this);
		}
		
		public void SetLog(string status,int rowsLoaded) {
				SetLog(status,rowsLoaded,"OK");
		}
			
		public void SetLog(string status, int rowsLoaded,string notes) {
			DB.Insert(Settings.tableLog,new Dictionary<string,string>(){
				        {"created",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
			          	{"scheduleID",id.ToString()},
			          	{"status",status},
			          	{"rowsLoaded",rowsLoaded.ToString()},
			          	{"extractID",extractID.ToString()},
			          	{"notes",notes}
			          });
		}
		
		public void DespatchEmail() {
				DB.NonQuery("EXEC "+ Settings.emailerProcedure +" "+id.ToString()+", "+Settings.testing.ToString());//"+Settings.testing);
		}
			
			
		public void ImportData(string theData) {
			string savePath=Path.Combine(Settings.fileSavehaven,GetArchivalString());
				// current date, user etc.
			System.IO.File.WriteAllText(savePath,theData);
			
			int rowsLoaded=0;
			if (Settings.testing=="1") {
				rowsLoaded=999;
			} else {
				rowsLoaded=DB.BulkInsert(this,savePath);
			}
			
			if (rowsLoaded>0) {
        		DB.NonQuery("UPDATE "+Settings.tableSchedule+" SET completed=1 WHERE id="+
        		         this.id);
        	}
			SetLog("OK (200)",rowsLoaded);
			Program.Log("data imported");
			DespatchEmail();
		}
		
		private string GetArchivalString() {
			string importInformation="["+extract.localIdentifier+"]"+
				"["+DateTime.Now.ToString("dd-MM-yyyy HHmm") +"]"+
				"["+Environment.UserName+"]";
			string template=extract.archivalConvention;
			template = template
				.Replace("[YYYY]",duedate.Year.ToString())
				.Replace("[MM]",strMonthNum)
				.Replace("[MMM]",strMonthTxt)
				.Replace("[DD]",strDayNum)
				.Replace("[NUM]",version.ToString())
				.Replace("[QNUM]",strQuarterTxt)
				.Replace(".csv",importInformation)
				+ ".csv";
			return template;
			
		}
			
		private void SetSingleDay() {
			
		}
		
		private void SetRelativePeriods() {
			monthNum=0;
			DateTime extractDate=duedate;
			//if (period.StartsWith("W/E")) {
		//		extractDate=period.Replace("W/E ","");
		//	}
			string thisyear=extractDate.Year.ToString();
			strDayNum=extractDate.Day.ToString();
			switch (period.ToUpper().Trim()) {
				case "JAN": monthNum=1; break;
				case "FEB": monthNum=2;	break;
				case "MAR":	monthNum=3;	break;
				case "APR":	monthNum=4;	break;
				case "MAY":	monthNum=5;	break;
				case "JUN":	monthNum=6; break;
				case "JUL":	monthNum=7;	break;
				case "AUG":	monthNum=8; break;
				case "SEP":	monthNum=9; break;
				case "OCT":	monthNum=10; break;
				case "NOV":	monthNum=11; break;
				case "DEC":	monthNum=12; break;
				case "QTR1":
					strQuarterTxt="1";
					singleDayDate="01-APR-"+extractDate.Year.ToString();
					monthNum=6;
					break;
				case "QTR2":
					strQuarterTxt="2";
					monthNum=9;
					singleDayDate="01-JUL-"+extractDate.Year.ToString();
					break;
				case "QTR3":
					strQuarterTxt="3";
					monthNum=12;
					int tYear=extractDate.Year-1;
					singleDayDate="01-OCT-"+tYear.ToString();
					break;
				case "QTR4":
					strQuarterTxt="4";
					monthNum=3;
					singleDayDate="01-JAN-"+extractDate.Year.ToString();
					break;
				case "WEEKLY":
					// 
					break;
				case "DAILY":
					// duedate
					break;
				default:
					break;
					
			}
			
			
			
			
			if ((monthNum<4) || (Settings.testing=="1") ) {
				Program.Log("year set as last-this");
				int prevYear=extractDate.Year-1;
				strYear=prevYear.ToString()+"-"+extractDate.Year.ToString().Substring(2,2);
			} else {
				Program.Log("year set as this-next");
				int nextYear=extractDate.Year+1;
				strYear=extractDate.Year.ToString()+"-"+nextYear.ToString().Substring(2,2);
			}
			
			Program.Log("set monthNum as "+monthNum.ToString());
			
			// set the name of the month in upper case from the month number determined above. STUPID!
			if (monthNum>0) {
				strMonthTxt=CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNum).ToUpper().ToString();
				strMonthNum=monthNum.ToString("D2");
			}
			Program.Log("out of it");
		}
		}
	}

