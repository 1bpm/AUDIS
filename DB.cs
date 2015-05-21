using System;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace AUDIS {
	

	/// <summary>
	/// db access class
	/// </summary>
	public static class DB {
        public static SqlConnection connection;
        
        /// <summary>
        /// execute a query and return a dataview
        /// </summary>
        /// <param name="query">the query text</param>
        /// <returns>dataview of the result</returns>
        public static DataView Query(string query) {
        	SqlDataAdapter adapter = new SqlDataAdapter();
        	DataSet results = new DataSet();
        	adapter.SelectCommand=new SqlCommand(query,DB.connection);
        	adapter.Fill(results);
        	return new DataView(results.Tables[0]);
        }
        
        public static object ScalarQuery(string query) {
        	SqlCommand cmd = new SqlCommand(query,connection);
        	return cmd.ExecuteScalar();
        }
        
        public static void NonQuery(string query) {
        	SqlCommand sc = new SqlCommand(query,connection);
        	sc.ExecuteNonQuery();
        }
        

        /// <summary>
        /// insert to table using key/value pairs
        /// </summary>
        /// <param name="table">the table name</param>
        /// <param name="toInsert">dictionary of columnName = value</param>
        public static void Insert(string table, Dictionary<string,string> toInsert) {
        	string cols="(";
        	string data="(";
        	int cnt=1;
        	string query="INSERT INTO "+table;
        	foreach (string key in toInsert.Keys) {
        		string value=toInsert[key];
        		cols+=key;
        		data+="'"+value.Replace("'","''")+"'";
        		if (cnt<toInsert.Count) {
        			cols+=",";
        			data+=",";
        			cnt++;
        		}
        	}
        	cols+=")";
        	data+=")";
        	query+=cols + " VALUES " + data;
        	SqlCommand cm = new SqlCommand(query,connection);
        	try {
        		cm.ExecuteNonQuery();
        	} catch (Exception ex) {
        		//Log.Warn("could not insert to table "+ex.ToString());
        	}
        	
        }
        
        /// <summary>
        /// close database connections
        /// </summary>
        public static void Disconnect() {
        	DB.connection.Close();
        }
        
        /// <summary>
        /// create two connections
        /// </summary>
        /// <param name="profile">DBProfile object with connection parameters</param>
        public static void Connect() {
        	Instance.form.AddLine("connecting to db");
        	string connString="Data Source=" + Settings.dbHost + ";" +
                   "Initial Catalog=" + Settings.dbName + ";" +
                   "User="+Settings.dbUser+";Password="+Settings.dbPass+";";
        		DB.connection = new SqlConnection(connString);
                DB.connection.Open();
               // DB.readerConnection = new SqlConnection(connString);
               // DB.readerConnection.Open();

        }
        
        
        public static int BulkInsert (ScheduleInformation schedule,string inputFile) {
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
        		//for (int i=0;i<parts.Length;i++) {
        		//	if (parts[i].Trim().Length==0) {
        		//		parts[i]=null;
        		//	}
        		//}
        		if (parts==null) break;
        		//Program.Log(parts.Length + " fields"+parts[0]);
        		if (currentRow>=extract.skipRows) {
        			insertData.Rows.Add(parts);
        		}
        		currentRow++;
        	}
        	
        	int stagingCount=(int)ScalarQuery("SELECT COUNT(*) FROM "+extract.stagingTable);
        	int preBaseCount=(int)ScalarQuery("SELECT COUNT(*) FROM "+extract.baseTable);
        	
        	int insertedNumber=insertData.Rows.Count;
        	
        	SqlBulkCopy bulkInsert=new SqlBulkCopy(connection);
   
        	bulkInsert.DestinationTableName=extract.stagingTable;
        	bulkInsert.WriteToServer(insertData);
        	bulkInsert.Close();
        	
        	try {
        		
        		NonQuery("EXECUTE " + extract.loadingSP);
        	} catch {
        		Program.Log("worked, but loading procedure failed");
        		throw new Exception("error with loading procedure");
        	}
        	int postBaseCount=(int)ScalarQuery("SELECT COUNT(*) FROM "+extract.baseTable);
        	
        	try {
	        	NonQuery("EXEC [0AJ].DS_UnifyDQByScheduleID "+schedule.id);
        	} catch {
        		Program.Log("worked and imported, but DQ check did not work");
        	}
        	
        
        	
        	return insertedNumber;
       }

	}
}
