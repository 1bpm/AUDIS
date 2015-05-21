/*
 * Created by SharpDevelop.
 * User: rknight
 * Date: 04/08/2015
 * Time: 16:25
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Data.SqlClient;
using System.Data;
using Microsoft.VisualBasic.FileIO;

namespace AUDIS
{
	/// <summary>
	/// Description of Loader.
	/// </summary>
	public class Loader {
		private int skipRows;
		private string stagingTable;
		private string masterTable;
		private string loadingProcedure;
		
		public Loader(ScheduleInformation schedule) {
			ExtractInformation extract=schedule.extract;
			Initialise(extract);
			
		}
		
		public Loader(ExtractInformation extract) {
			Initialise(extract);
		}
		
		private void Initialise(ExtractInformation extract) {
			skipRows=extract.skipRows;
			stagingTable=extract.stagingTable;
			masterTable=extract.baseTable;
			loadingProcedure=extract.loadingSP;
		}
		
		
		public static void BulkInsert (ScheduleInformation schedule,string inputFile) {
        	ExtractInformation extract=schedule.extract;
        	SqlCommand truncater=new SqlCommand("TRUNCATE TABLE "+extract.stagingTable,connection);
        	truncater.ExecuteNonQuery();      	                                   
        	
        	DataTable insertData=new DataTable();
        	SqlCommand emptyTableCommand=new SqlCommand("SELECT * FROM "+
        	                                            extract.stagingTable+" WHERE 1=0",connection);
        	SqlDataAdapter adapter=new SqlDataAdapter(emptyTableCommand);
        	adapter.Fill(insertData);
        	
        	TextFieldParser tfp=new TextFieldParser(inputFile);
        	tfp.TextFieldType=FieldType.Delimited;
        	tfp.Delimiters=new string[]{","};
        	int currentRow=0;
        	while (true) {
        		string[] parts=tfp.ReadFields();
        		if (parts==null) break;
        		//Program.Log(parts.Length + " fields"+parts[0]);
        		if (currentRow>=extract.skipRows) {
        			insertData.Rows.Add(parts);
        		}
        		currentRow++;
        	}
        	
        	int stagingCount=(int)ScalarQuery("SELECT COUNT(*) FROM "+extract.stagingTable);
        	int preBaseCount=(int)ScalarQuery("SELECT COUNT(*) FROM "+extract.baseTable);
        	
        	SqlBulkCopy bulkInsert=new SqlBulkCopy(connection);
        	bulkInsert.DestinationTableName=extract.stagingTable;
        	bulkInsert.WriteToServer(insertData);
        	bulkInsert.Close();
        	
        	NonQuery("EXECUTE " + extract.loadingSP);
        	int postBaseCount=(int)ScalarQuery("SELECT COUNT(*) FROM "+extract.baseTable);
        	
        	NonQuery("EXEC [0AJ].DS_UnifyDQByScheduleID "+schedule.id);
       }

		
		
		
		
		
			
			
		public void BulkInsert(string stagingTable) {
        	int skipRows=2;
        	DataTable insertData=new DataTable();
        	SqlCommand emptyTableCommand=new SqlCommand("SELECT * FROM "+
        	                                            stagingTable+" WHERE 1=0",DB.connection);
        	SqlDataAdapter adapter=new SqlDataAdapter(emptyTableCommand);
        	adapter.Fill(insertData);
        	
        	TextFieldParser tfp=new TextFieldParser("dasOutput.csv");
        	tfp.TextFieldType=FieldType.Delimited;
        	tfp.Delimiters=new string[]{","};
        	int currentRow=0;
        	while (true) {
        		string[] parts=tfp.ReadFields();
        		if (parts==null) break;
        		//Program.Log(parts.Length + " fields"+parts[0]);
        		if (currentRow>skipRows) {
        			insertData.Rows.Add(parts);
        		}
        		currentRow++;
        	}
        	SqlBulkCopy bulkInsert=new SqlBulkCopy(DB.connection);
        	bulkInsert.DestinationTableName=stagingTable;
        	bulkInsert.WriteToServer(insertData);
        	bulkInsert.Close();
        	
        }
        
			
			
			
			
		
	}
}
