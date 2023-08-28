using BusinessLayer.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DAL
{
    public class DAL_CONCRETE_SPECIMEN
    {
        public string INSERT_CONCRETE_SPECIMEN(string Emp_Code, Concrete_Specimen _CONCRETE, out ResultMessage oblMsg)
        {

            string errorMsg = "";
            oblMsg = new ResultMessage();
            using (var connection = new SqlConnection(sqlConnection.GetConnectionString_SGX()))
            {

                connection.Open();
                SqlCommand command;
                SqlTransaction transactionScope = null;
                transactionScope = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    int IS_FILE_UPLOAD = 0;
                    string DMS_Path = ConfigurationManager.AppSettings["DMSPATH"].ToString();
                    string filePath = "REPORT\\CONCRETE SPECIMEN\\";
                    string directoryPath = DMS_Path + filePath;
                    string ext = "";
                    if (_CONCRETE.UploadFile != null)
                    {
                        IS_FILE_UPLOAD = 1;
                        ext = new System.IO.FileInfo(_CONCRETE.UploadFile.FileName).Extension;
                    }

                    SqlParameter[] param =
                    { 
                      new SqlParameter("@ERRORSTR", SqlDbType.VarChar, 200)
                     ,new SqlParameter("@Test_ID", SqlDbType.Decimal) 
                     ,new SqlParameter("@Test_No", SqlDbType.VarChar, 20)  
                     ,new SqlParameter("@Dry_Bulb", _CONCRETE.DryBulb)  
                     ,new SqlParameter("@Wet_Bulb", _CONCRETE.WetBulb )    
                     ,new SqlParameter("@Date_Test", _CONCRETE.TestDateTime )    
                     ,new SqlParameter("@Added_By", Emp_Code) 
                     ,new SqlParameter("@Tested_By", _CONCRETE.TESTED_BY)
                     ,new SqlParameter("@Qc_Incharge",_CONCRETE.QC_INCHARGE)
                     ,new SqlParameter("@IS_FILE_UPLOAD", IS_FILE_UPLOAD)  
                     ,new SqlParameter("@FILE_PATH", string.IsNullOrEmpty(ext)?(object)DBNull.Value:ext) 

                    };
                    param[0].Direction = ParameterDirection.Output;
                    param[1].Direction = ParameterDirection.Output;
                    param[2].Direction = ParameterDirection.Output;
                    new DataAccess().InsertWithTransaction("[AGG].[USP_INSERT_CONCRETE_SPECIMENS_HDR]", CommandType.StoredProcedure, out command, connection, transactionScope, param);
                    decimal Test_ID = (decimal)command.Parameters["@Test_ID"].Value;
                    string Test_No = (string)command.Parameters["@Test_No"].Value;
                    string error_1 = (string)command.Parameters["@ERRORSTR"].Value;

                    if (Test_ID == -1) { errorMsg = error_1; }

                    if (Test_ID > 0)
                    {

                        if (_CONCRETE.Concrete_Specimen_Dtl_List != null)
                        {
                            foreach (Concrete_Specimen_Dtl item in _CONCRETE.Concrete_Specimen_Dtl_List)
                            {
                                SqlParameter[] param2 =
                                {
                                new SqlParameter("@ERRORSTR", SqlDbType.VarChar, 200)
                               ,new SqlParameter("@Test_Dtl_ID", SqlDbType.Decimal)  
                               //,new SqlParameter("@DESK_FILE_PATH", SqlDbType.VarChar, 50)
                               ,new SqlParameter("@Test_ID" ,Test_ID)  
                               ,new SqlParameter("@Client" , item.Client)
                               ,new SqlParameter("@Id_Mark", (item.Id_Mark == null)? (object)DBNull.Value : item.Id_Mark) 
                               ,new SqlParameter("@Dimension_L", (item.Dimension_L == null)? (object)DBNull.Value : item.Dimension_L)
                               ,new SqlParameter("@Dimension_B", (item.Dimension_B == null)? (object)DBNull.Value : item.Dimension_B)
                               ,new SqlParameter("@Dimension_H", (item.Dimension_H == null)? (object)DBNull.Value : item.Dimension_H) 
                               ,new SqlParameter("@Cast_Date",  (item.Date_Of_Casting == null)? (object)DBNull.Value : item.Date_Of_Casting) 
                               ,new SqlParameter("@Test_Age",  (item.Age_Test == null) ? (object)DBNull.Value : item.Age_Test) 
                               ,new SqlParameter("@Concrete_grade", item.Grade_Id) 
                               ,new SqlParameter("@Weight", (item.Weight == null)? (object)DBNull.Value : item.Weight) 
                               ,new SqlParameter("@Crushing_Load", (item.Crushing_Load == null)? (object)DBNull.Value : item.Crushing_Load) 
                               ,new SqlParameter("@Compressive_strength", (item.Compressive_Strength == null)? (object)DBNull.Value : item.Compressive_Strength) 
                               ,new SqlParameter("@Avg_Compressive", (item.AVG_Compressive_Strength == null)? (object)DBNull.Value : item.AVG_Compressive_Strength)  
                               ,new SqlParameter("@Remarks", item.Remarks)   
    
                                    };
                                param2[0].Direction = ParameterDirection.Output;
                                param2[1].Direction = ParameterDirection.Output;

                                new DataAccess().InsertWithTransaction("[AGG].[USP_INSERT_CONCRETE_SPECIMENS_Dtl]", CommandType.StoredProcedure, out command, connection, transactionScope, param2);
                                decimal Test_Dtl_ID = (decimal)command.Parameters["@Test_Dtl_ID"].Value;
                                string error_2 = (string)command.Parameters["@ERRORSTR"].Value;
                                //string deskFilePath = (string)command.Parameters["@DESK_FILE_PATH"].Value;
                                if (Test_Dtl_ID == -1) { errorMsg = error_2; }


                            }
                        }
                    }
                    if (errorMsg == "")
                    {

                        if (_CONCRETE.UploadFile != null)
                        {
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            if (_CONCRETE.UploadFile != null)
                            {
                                string fileName = Test_No.Replace("/", "_") + ext; ;
                               
                                _CONCRETE.UploadFile.SaveAs(directoryPath + fileName);

                            }
                        }
                            
                                oblMsg.ID = Test_ID;
                                oblMsg.CODE = Test_No;
                                oblMsg.MsgType = "Success";
                                transactionScope.Commit();
                            }
                            else
                            {
                                oblMsg.Msg = errorMsg;
                                oblMsg.MsgType = "Error";
                                transactionScope.Rollback();
                            }
                        }


                catch (Exception ex)
                {
                    try
                    {
                        transactionScope.Rollback();
                    }
                    catch (Exception ex1)
                    {
                        errorMsg = "Error: Exception occured. " + ex1.StackTrace.ToString();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return errorMsg;
        }

        public List<CONCRETE_SPECIFICATION_DATALIST> Select_Concrete_Specimen_List(Concrete_List CONCRETE_Datalist)
        {
            SqlParameter[] param = {    new SqlParameter("@FROM_DT", CONCRETE_Datalist.From_DT),
                                       new SqlParameter("@TO_DT", CONCRETE_Datalist.To_DT) , 
                                   };

            DataSet ds = new DataAccess(sqlConnection.GetConnectionString_SGX()).GetDataSet("[AGG].[USP_SELECT_CONCRETE_SPECIMENS]", CommandType.StoredProcedure, param);

            List<CONCRETE_SPECIFICATION_DATALIST> _list = new List<CONCRETE_SPECIFICATION_DATALIST>();
            DataTable dt = ds.Tables[0];
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow Rows in ds.Tables[0].Rows)
                {
                    _list.Add(new CONCRETE_SPECIFICATION_DATALIST
                    {
                        Test_ID = Convert.ToInt32(Rows["Test_ID"] == DBNull.Value ? 0 : Rows["Test_ID"]),
                        Test_No = Convert.ToString(Rows["Test_No"] == DBNull.Value ? "" : Rows["Test_No"]),
                        Date_Test = Convert.ToString(Rows["Date_Test"]),
                        Dry_Bulb = Convert.ToString(Rows["Dry_Bulb"]),
                        Wet_Bulb = Convert.ToString(Rows["Wet_Bulb"]),
                        SAMPLE_COUNT = Convert.ToDecimal(Rows["SAMPLE_COUNT"] == DBNull.Value ? 0 : Rows["SAMPLE_COUNT"]),

                    });
                }
            }

            return _list;
        }
        public Concrete_Specimen Edit_Concrete_Specimens(decimal Test_ID)
        {
            SqlParameter[] param = { new SqlParameter("@Test_ID", Test_ID) };
            DataSet ds = new DataAccess(sqlConnection.GetConnectionString_SGX()).GetDataSet("[AGG].[USP_INSERT_CONCRETE_SPECIMENS_EDIT]", CommandType.StoredProcedure, param);
            Concrete_Specimen _objSPGR = new Concrete_Specimen();
            List<Concrete_Specimen_Dtl> _list = new List<Concrete_Specimen_Dtl>();
            Concrete_Specimen_Dtl dtl = null;
            DataTable dt = ds.Tables[0];
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                //dtl = new Concrete_Specimen_Dtl();
                _objSPGR.Test_ID = Convert.ToInt32(dt.Rows[0]["Test_ID"] == DBNull.Value ? 0 : dt.Rows[0]["Test_ID"]);
                _objSPGR.Test_No = Convert.ToString(dt.Rows[0]["Test_No"]);
                _objSPGR.DryBulb = Convert.ToString(dt.Rows[0]["Dry_Bulb"]);
                _objSPGR.WetBulb = Convert.ToString(dt.Rows[0]["Wet_Bulb"]);
                _objSPGR.TestDateTime = Convert.ToDateTime(dt.Rows[0]["Date_Test"]);
                _objSPGR.TESTED_BY = Convert.ToString(dt.Rows[0]["Tested_By"]);
                _objSPGR.Added_By = Convert.ToString(dt.Rows[0]["Added_By"]);
                _objSPGR.QC_INCHARGE = Convert.ToString(dt.Rows[0]["Qc_Incharge"]);
                _objSPGR.FILE_PATH = Convert.ToString(dt.Rows[0]["FILE_PATH"]);
                _objSPGR.IS_FILE_UPLOAD = Convert.ToInt32(dt.Rows[0]["IS_FILE_UPLOAD"] == DBNull.Value ? 0 : dt.Rows[0]["IS_FILE_UPLOAD"]);
            }
            //DataTable dt = ds.Tables[1];
            foreach (DataRow row in dt.Rows)
            {
                dtl = new Concrete_Specimen_Dtl();
                dtl.Test_ID = Convert.ToInt32(row["Test_ID"] == DBNull.Value ? "" : row["Test_ID"]);
                dtl.Client = Convert.ToString(row["Client"] == DBNull.Value ? "" : row["Client"]);
                dtl.Id_Mark = Convert.ToDecimal(row["Id_Mark"] == DBNull.Value ? 0 : row["Id_Mark"]);
                dtl.Dimension_L = Convert.ToDecimal(row["Dimension_L"] == DBNull.Value ? 0 : row["Dimension_L"]);
                dtl.Dimension_B = Convert.ToDecimal(row["Dimension_B"] == DBNull.Value ? 0 : row["Dimension_B"]);
                dtl.Dimension_H = Convert.ToDecimal(row["Dimension_H"] == DBNull.Value ? 0 : row["Dimension_H"]);
                dtl.Date_Of_Casting = Convert.ToDateTime(row["Cast_Date"] == DBNull.Value ? 0 : row["Cast_Date"]);
                dtl.Age_Test = Convert.ToInt32(row["Test_Age"] == DBNull.Value ? 0 : row["Test_Age"]);
                dtl.Grade_Id = Convert.ToDecimal(row["Concrete_Grade"] == DBNull.Value ? "" : row["Concrete_Grade"]);
                dtl.Weight = Convert.ToDecimal(row["Weight"] == DBNull.Value ? 0 : row["Weight"]);
                dtl.Crushing_Load = Convert.ToDecimal(row["Crushing_Load"] == DBNull.Value ? 0 : row["Crushing_Load"]);
                dtl.Compressive_Strength = Convert.ToDecimal(row["Compressive_strength"] == DBNull.Value ? 0 : row["Compressive_strength"]);
                dtl.AVG_Compressive_Strength = Convert.ToDecimal(row["Avg_Compressive"] == DBNull.Value ? 0 : row["Avg_Compressive"]);
                dtl.Remarks = Convert.ToString(row["Remarks"] == DBNull.Value ? "" : row["Remarks"]);
                _list.Add(dtl);
            }
            _objSPGR.Concrete_Specimen_Dtl_List = _list;
            return _objSPGR;
        }

        public string UPDATE_CONCRETE_SPECIMEN(string Emp_Code, Concrete_Specimen _CONCRETE, out ResultMessage oblMsg)
        {
            string errorMsg = "";
            oblMsg = new ResultMessage();
            using (var connection = new SqlConnection(sqlConnection.GetConnectionString_SGX()))
            {
                connection.Open();
                SqlCommand command;
                SqlTransaction transactionScope = null;
                transactionScope = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    int IS_FILE_UPLOAD = 1;

                    string DMS_Path = ConfigurationManager.AppSettings["DMSPATH"].ToString();
                    string filePath = "REPORT\\CONCRETE SPECIMEN\\";
                    string directoryPath = DMS_Path + filePath;
                    string ext = "";
                    if (_CONCRETE.UploadFile != null)
                    {
                        IS_FILE_UPLOAD = 1;
                        ext = new System.IO.FileInfo(_CONCRETE.UploadFile.FileName).Extension;
                    }


                    SqlParameter[] param =
                    { 
                      new SqlParameter("@ERRORSTR", SqlDbType.VarChar, 200)
                     ,new SqlParameter("@Test_ID", _CONCRETE.Test_ID) 
                     ,new SqlParameter("@Test_No", _CONCRETE.Test_No)  
                     ,new SqlParameter("@Dry_Bulb", _CONCRETE.DryBulb)  
                     ,new SqlParameter("@Wet_Bulb", _CONCRETE.WetBulb )    
                     ,new SqlParameter("@Date_Test", _CONCRETE.TestDateTime )    
                     ,new SqlParameter("@Added_By", Emp_Code) 
                     ,new SqlParameter("@Tested_By", _CONCRETE.TESTED_BY)
                     ,new SqlParameter("@Qc_Incharge",_CONCRETE.QC_INCHARGE)
                     ,new SqlParameter("@IS_FILE_UPLOAD", IS_FILE_UPLOAD)  
                     ,new SqlParameter("@FILE_PATH", string.IsNullOrEmpty(ext)?(object)DBNull.Value:ext) 

                    };
                    param[0].Direction = ParameterDirection.Output;

                    new DataAccess().InsertWithTransaction("[AGG].[USP_UPDATE_CONCRETE_SPECIMENS_HDR]", CommandType.StoredProcedure, out command, connection, transactionScope, param);

                    string error_1 = (string)command.Parameters["@ERRORSTR"].Value;

                    if (error_1 != "") { errorMsg = error_1; }

                    if (errorMsg == "")
                    {

                        if (_CONCRETE.Concrete_Specimen_Dtl_List != null)
                        {
                            foreach (Concrete_Specimen_Dtl item in _CONCRETE.Concrete_Specimen_Dtl_List)
                            {

                                SqlParameter[] param2 =
                                {
                                new SqlParameter("@ERRORSTR", SqlDbType.VarChar, 200)
                               ,new SqlParameter("@Test_Dtl_ID", (item.Test_Dtl_ID == null)? (object)DBNull.Value : item.Test_Dtl_ID)  
                             
                               ,new SqlParameter("@Test_ID" , (item.Test_ID == null)? (object)DBNull.Value : item.Test_ID)  
                               ,new SqlParameter("@Client" , item.Client)
                               ,new SqlParameter("@Id_Mark", (item.Id_Mark == null)? (object)DBNull.Value : item.Id_Mark) 
                               ,new SqlParameter("@Dimension_L", (item.Dimension_L == null)? (object)DBNull.Value : item.Dimension_L)
                               ,new SqlParameter("@Dimension_B", (item.Dimension_B == null)? (object)DBNull.Value : item.Dimension_B)
                               ,new SqlParameter("@Dimension_H", (item.Dimension_H == null)? (object)DBNull.Value : item.Dimension_H) 
                               ,new SqlParameter("@Cast_Date",  (item.Date_Of_Casting == null)? (object)DBNull.Value : item.Date_Of_Casting) 
                               ,new SqlParameter("@Test_Age",  (item.Age_Test == null) ? (object)DBNull.Value : item.Age_Test) 
                               ,new SqlParameter("@Concrete_grade", ( item.Grade_Id  == null) ? (object)DBNull.Value :  item.Grade_Id ) 
                               ,new SqlParameter("@Weight", (item.Weight == null)? (object)DBNull.Value : item.Weight) 
                               ,new SqlParameter("@Crushing_Load", (item.Crushing_Load == null)? (object)DBNull.Value : item.Crushing_Load) 
                               ,new SqlParameter("@Compressive_strength", (item.Compressive_Strength == null)? (object)DBNull.Value : item.Compressive_Strength) 
                               ,new SqlParameter("@Avg_Compressive", (item.AVG_Compressive_Strength == null)? (object)DBNull.Value : item.AVG_Compressive_Strength)  
                               ,new SqlParameter("@Remarks", item.Remarks)   
                                };
                                param2[0].Direction = ParameterDirection.Output;
                                new DataAccess().InsertWithTransaction("[AGG].[USP_INSERT_CONCRETE_SPECIMENS_DTL]", CommandType.StoredProcedure, out command, connection, transactionScope, param2);

                                string error_2 = (string)command.Parameters["@ERRORSTR"].Value;
                                if (error_2 != "") { errorMsg = error_2; break; }


                            }
                        }
                    }


                    if (errorMsg == "")
                    {
                        // Below code is used for attached slip file
                        if (_CONCRETE.UploadFile != null)
                        {
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            if (_CONCRETE.UploadFile != null)
                            {
                                string fileName = _CONCRETE.Test_No.Replace("/", "_") + ext;
                                _CONCRETE.UploadFile.SaveAs(directoryPath + fileName);
                            }
                        }

                        oblMsg.ID = Convert.ToDecimal (_CONCRETE.Test_ID);
                        oblMsg.CODE = _CONCRETE.Test_No;
                        oblMsg.MsgType = "Success";
                        transactionScope.Commit();
                    }
                    else
                    {
                        oblMsg.Msg = errorMsg;
                        oblMsg.MsgType = "Error";
                        transactionScope.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        transactionScope.Rollback();
                    }
                    catch (Exception ex1)
                    {
                        errorMsg = "Error: Exception occured. " + ex1.StackTrace.ToString();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return errorMsg;
        }

        public Concrete_Specimen View_Concrete_Specimens(decimal Test_ID)
        {
            SqlParameter[] param = { new SqlParameter("@Test_ID", Test_ID) };
            DataSet ds = new DataAccess(sqlConnection.GetConnectionString_SGX()).GetDataSet("[AGG].[USP_CONCRETE_SPECIMENS_VIEW]", CommandType.StoredProcedure, param);
            Concrete_Specimen _objSPGR = new Concrete_Specimen();
            List<Concrete_Specimen_Dtl> _list = new List<Concrete_Specimen_Dtl>();
            Concrete_Specimen_Dtl dtl = null;
            DataTable dt = ds.Tables[0];
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                //dtl = new Concrete_Specimen_Dtl();
                //_objSPGR.Test_ID = Convert.ToInt32(dt.Rows[0]["Test_Dtl_ID"] == DBNull.Value ? 0 : dt.Rows[0]["Test_Dtl_ID"]);
                _objSPGR.Test_No = Convert.ToString(dt.Rows[0]["Test_No"]);
                _objSPGR.DryBulb = Convert.ToString(dt.Rows[0]["Dry_Bulb"]);
                _objSPGR.WetBulb = Convert.ToString(dt.Rows[0]["Wet_Bulb"]);
                _objSPGR.TestDateTime = Convert.ToDateTime(dt.Rows[0]["Date_Test"]);
                _objSPGR.TESTED_BY = Convert.ToString(dt.Rows[0]["Tested_By"]);
                _objSPGR.Added_By = Convert.ToString(dt.Rows[0]["Added_By"]);
                _objSPGR.QC_INCHARGE = Convert.ToString(dt.Rows[0]["Qc_Incharge"]);
                _objSPGR.FILE_PATH = Convert.ToString(dt.Rows[0]["FILE_PATH"]);
                _objSPGR.IS_FILE_UPLOAD = Convert.ToInt32(dt.Rows[0]["IS_FILE_UPLOAD"] == DBNull.Value ? 0 : dt.Rows[0]["IS_FILE_UPLOAD"]);
            }
            //DataTable dt = ds.Tables[1];
            foreach (DataRow row in dt.Rows)
            {
                dtl = new Concrete_Specimen_Dtl();
                dtl.Test_ID = Convert.ToInt32(row["Test_Dtl_ID"] == DBNull.Value ? 0 : row["Test_Dtl_ID"]);
                dtl.Client = Convert.ToString(row["Client"] == DBNull.Value ? "" : row["Client"]);
                dtl.Id_Mark = Convert.ToDecimal(row["Id_Mark"] == DBNull.Value ? 0 : row["Id_Mark"]);
                dtl.Dimension_L = Convert.ToDecimal(row["Dimension_L"] == DBNull.Value ? 0 : row["Dimension_L"]);
                dtl.Dimension_B = Convert.ToDecimal(row["Dimension_B"] == DBNull.Value ? 0 : row["Dimension_B"]);
                dtl.Dimension_H = Convert.ToDecimal(row["Dimension_H"] == DBNull.Value ? 0 : row["Dimension_H"]);
                dtl.Date_Of_Casting = Convert.ToDateTime(row["Cast_Date"] == DBNull.Value ? 0 : row["Cast_Date"]);
                dtl.Age_Test = Convert.ToInt32(row["Test_Age"] == DBNull.Value ? 0 : row["Test_Age"]);
                dtl.Grade_Id = Convert.ToDecimal(row["Concrete_Grade"] == DBNull.Value ? "" : row["Concrete_Grade"]);
                dtl.Weight = Convert.ToDecimal(row["Weight"] == DBNull.Value ? 0 : row["Weight"]);
                dtl.Crushing_Load = Convert.ToDecimal(row["Crushing_Load"] == DBNull.Value ? 0 : row["Crushing_Load"]);
                dtl.Compressive_Strength = Convert.ToDecimal(row["Compressive_strength"] == DBNull.Value ? 0 : row["Compressive_strength"]);
                dtl.AVG_Compressive_Strength = Convert.ToDecimal(row["Avg_Compressive"] == DBNull.Value ? 0 : row["Avg_Compressive"]);
                dtl.Remarks = Convert.ToString(row["Remarks"] == DBNull.Value ? "" : row["Remarks"]);
                _list.Add(dtl);
            }
            _objSPGR.Concrete_Specimen_Dtl_List = _list;
            return _objSPGR;
        }
    }
}
