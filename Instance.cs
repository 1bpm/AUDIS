using System;
using System.Data;

namespace AUDIS
{
	/// <summary>
	/// static container for the form, that's all at the moment...
	/// </summary>
	public static class Instance {
		public static MainForm form=null;
		
	}
	
	
	
	
	/// <summary>
	/// get the jobs scheduled to be downloaded today.
	/// </summary>
	public class Scheduled {
		
		public Scheduled() {
			Program.Log("Determining schedule");
			string todayQuery="SELECT "+Settings.tableSchedule+".id FROM "+Settings.tableSchedule+" WHERE "
                + "(SELECT automate FROM "+ Settings.tableExtract +" WHERE "+ Settings.tableExtract+".id="+ Settings.tableSchedule+".extractID)=1 "
                + "AND (completed IS null OR completed!=1)  AND duedate<=DATEADD(dd, DATEDIFF(dd, 0, GETDATE()),0)"
                + " ORDER BY duedate ASC";
			// AND runByHour<=(DATEPART(hour,getdate())*100))=1
			//  AND UPPER(TYPE) LIKE '%UNIFY%'
			DataView toRun=DB.Query(todayQuery);
			Program.Log("there are "+toRun.Count.ToString()+" downloads scheduled for today");
			if (toRun.Count<1) {
				Program.Log("Nothing scheduled");
			} else {
				Program.Log("iterating the scheduled");
				foreach (DataRow dR in toRun.Table.Rows) {
					Program.Log("getting schedule row");
					ScheduleInformation schedule=new ScheduleInformation((int)dR["id"]);
					
					Program.Log("schedule ID "+dR["id"].ToString());
					if (schedule.extract.IsReady()) {
						Program.Log("Beginning "+schedule.extract.bookName + " for "+ schedule.duedate.ToString());
						try {
							schedule.Run();
						} catch (Exception e) {
							schedule.SetLog("500 (Error)",0,e.ToString());
							schedule.DespatchEmail();
						}
					} else {
						Program.Log(schedule.extract.bookName+" scheduled but not ready according to RunByHour ("+schedule.extract.runByHour.ToString()+")");
					}
				}
				//DB.NonQuery("EXEC [0AJ].DS_UnifyDQByDate");
			}
		}
	}
	
	
	
	
	
}
