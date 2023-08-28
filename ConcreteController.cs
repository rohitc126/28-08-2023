using BusinessLayer;
using BusinessLayer.DAL;
using BusinessLayer.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eSGBIZ.Controllers
{
    public class ConcreteController : BaseController
    {
        //
        // GET: /ConcreteSpecimens/

        public ActionResult ConcreteSpecimensEntry()
        {
            ViewBag.Header = "Concrete Specimens";
            Concrete_Specimen _Concrete = new Concrete_Specimen();

            List<GradeMaster> gradeList = new DAL_Common().GetGradeList();
            _Concrete.Grade_List = new SelectList(gradeList, "Grade_Id", "Grade_Name");

            var empExceptionList = new List<string> { "CAL0229", "CAL0230" };
            List<EMPLOYEE_DETAILS> _empList = new DAL_Common().GetEmployee_List("", "", "", "", "", emp.Employee_Code, "").Where(x => x.activeFlag == "Y" && !empExceptionList.Contains(x.Employee_Code)).ToList<EMPLOYEE_DETAILS>();
            _Concrete.EMPLOYEE_LIST = new SelectList(_empList.OrderBy(x => x.EmployeeName), "Employee_Code", "EmployeeName");
            return View(_Concrete);
        }
        [HttpPost]
        [SubmitButtonSelector(Name = "Save")]
        [ActionName("ConcreteSpecimensEntry")]
        public ActionResult INSERT_CONCRETE_SPECIMEN_SAVE(Concrete_Specimen _CONSPECI)
        {
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();

            if (ModelState.IsValid)
            {
                try
                {
                    ResultMessage objMst;
                    string result = new DAL_CONCRETE_SPECIMEN().INSERT_CONCRETE_SPECIMEN(emp.Employee_Code, _CONSPECI, out objMst);

                    if (result == "")
                    {
                        Success(string.Format("<b>Concrete Specimen inserted successfully. Test NO : </b> <b style='color:red'> " + objMst.CODE + "</b>"), true);
                        return RedirectToAction("ConcreteSpecimensList", "Concrete");
                    }
                    else
                    {
                        Danger(string.Format("<b>Error:</b>" + result), true);
                    }
                }
                catch (Exception ex)
                {
                    Danger(string.Format("<b>Error:</b>" + ex.Message), true);
                }
            }
            else
            {
                Danger(string.Format("<b>Error:102 :</b>" + string.Join("; ", ModelState.Values.SelectMany(z => z.Errors).Select(z => z.ErrorMessage))), true);
            }


            List<GradeMaster> gradeList = new DAL_Common().GetGradeList();
            _CONSPECI.Grade_List = new SelectList(gradeList, "Grade_Id", "Grade_Name");

            var empExceptionList = new List<string> { "CAL0229", "CAL0230" };
            List<EMPLOYEE_DETAILS> _empList = new DAL_Common().GetEmployee_List("", "", "", "", "", emp.Employee_Code, "").Where(x => x.activeFlag == "Y" && !empExceptionList.Contains(x.Employee_Code)).ToList<EMPLOYEE_DETAILS>();
            _CONSPECI.EMPLOYEE_LIST = new SelectList(_empList.OrderBy(x => x.EmployeeName), "Employee_Code", "EmployeeName");

            return View(_CONSPECI);
        }

        [Authorize]
        public ActionResult ConcreteSpecimensList()
        {
            ViewBag.Header = "Concrete Specimen List";
            Concrete_List _CONSPECI = new Concrete_List();

            return View(_CONSPECI);

        }
        public ActionResult _ConcreteSpecimensList()
        {
            return PartialView("_ConcreteSpecimensList");
        }
        [HttpPost]
        public ActionResult _ConcreteSpecimens_Data_List(DateTime fDate, DateTime tDate)
        {
            // Server Side Processing
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            string searchValue = Request["search[value]"];
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][name]"];
            string sortDirection = Request["order[0][dir]"];
            int totalRow = 0;

            Concrete_List _SPCON = new Concrete_List();
            List<CONCRETE_SPECIFICATION_DATALIST> CONSPData = new List<CONCRETE_SPECIFICATION_DATALIST>();
            try
            {
                _SPCON.From_DT = fDate;
                _SPCON.To_DT = tDate;


                CONSPData = new DAL_CONCRETE_SPECIMEN().Select_Concrete_Specimen_List(_SPCON);

                totalRow = CONSPData.Count();

            }
            catch (Exception ex)
            {
                Danger(string.Format("<b>Exception occured.</b>"), true);
            }

            if (!string.IsNullOrEmpty(searchValue)) // Filter Operation
            {
                CONSPData = CONSPData.
                    Where(x => x.Test_No.ToLower().Contains(searchValue.ToLower())
                    || x.Date_Test.ToLower().Contains(searchValue.ToLower()) ||

                        x.Wet_Bulb.ToLower().Contains(searchValue.ToLower()) ||
                        x.Dry_Bulb.ToLower().Contains(searchValue.ToLower())
                         ).ToList<CONCRETE_SPECIFICATION_DATALIST>();

            }
            int totalRowFilter = CONSPData.Count();

            if (length == -1)
            {
                length = totalRow;
            }
            CONSPData = CONSPData.Skip(start).Take(length).ToList<CONCRETE_SPECIFICATION_DATALIST>();

            var jsonResult = Json(new { data = CONSPData, draw = Request["draw"], recordsTotal = totalRow, recordsFiltered = totalRowFilter }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public ActionResult ConcreteSpecimenEdit(decimal Test_ID)
        {
            ViewBag.Header = "Concrete Specimen Updation";

            Concrete_Specimen _objSPGRA = new Concrete_Specimen();
            _objSPGRA = new DAL_CONCRETE_SPECIMEN().Edit_Concrete_Specimens(Test_ID);

            var empExceptionList = new List<string> { "CAL0229", "CAL0230" };
            List<EMPLOYEE_DETAILS> _empList = new DAL_Common().GetEmployee_List("", "", "", "", "", emp.Employee_Code, "").Where(x => x.activeFlag == "Y" && !empExceptionList.Contains(x.Employee_Code)).ToList<EMPLOYEE_DETAILS>();
            _objSPGRA.EMPLOYEE_LIST = new SelectList(_empList.OrderBy(x => x.EmployeeName), "Employee_Code", "EmployeeName");

            return View(_objSPGRA);
        }


        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[4096];
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    return;
                }
                output.Write(buffer, 0, read);
            }
        }

        public string ConvertViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext vContext = new ViewContext(this.ControllerContext, vResult.View, ViewData, new TempDataDictionary(), writer);
                vResult.View.Render(vContext, writer);
                return writer.ToString();
            }
        }

        public FileResult ShowDocument(string FilePath)
        {
            string DMS_Path = ConfigurationManager.AppSettings["DMSPATH"].ToString();
            string directoryPath = DMS_Path + "REPORT\\CONCRETE SPECIMEN\\" + FilePath;
            return File(directoryPath, GetMimeType(FilePath));
        }

        private string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }



        public FileResult ShowDocument1(string FilePath)
        {
            string DMS_Path = ConfigurationManager.AppSettings["DMSPATH"].ToString();
            string directoryPath = DMS_Path + "REPORT\\CONCRETE SPECIMEN1\\" + FilePath;
            return File(directoryPath, GetMimeType1(FilePath));
        }

        private string GetMimeType1(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        //[HttpPost]
        //[SubmitButtonSelector(Name = "Save")]
        //[ActionName("ConcreteSpecimenEdit")]
        //public ActionResult Update_CONCRETE_SPECIMEN_SAVE(Concrete_Specimen _CONSPECI)
        //{
        //    var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            ResultMessage objMst;
        //            string result = new DAL_CONCRETE_SPECIMEN().INSERT_CONCRETE_SPECIMEN_TWO(emp.Employee_Code, _CONSPECI, out objMst);

        //            if (result == "")
        //            {
        //                Success(string.Format("<b>Concrete Specimen Updated successfully. Test NO : </b> <b style='color:red'> " + objMst.CODE + "</b>"), true);
        //                return RedirectToAction("ConcreteSpecimensList", "Concrete");
        //            }
        //            else
        //            {
        //                Danger(string.Format("<b>Error:</b>" + result), true);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Danger(string.Format("<b>Error:</b>" + ex.Message), true);
        //        }
        //    }
        //    else
        //    {
        //        Danger(string.Format("<b>Error:102 :</b>" + string.Join("; ", ModelState.Values.SelectMany(z => z.Errors).Select(z => z.ErrorMessage))), true);
        //    }
        //    var empExceptionList = new List<string> { "CAL0229", "CAL0230" };
        //    List<EMPLOYEE_DETAILS> _empList = new DAL_Common().GetEmployee_List("", "", "", "", "", emp.Employee_Code, "").Where(x => x.activeFlag == "Y" && !empExceptionList.Contains(x.Employee_Code)).ToList<EMPLOYEE_DETAILS>();
        //    _CONSPECI.EMPLOYEE_LIST = new SelectList(_empList.OrderBy(x => x.EmployeeName), "Employee_Code", "EmployeeName");


        //    return View(_CONSPECI);
        //}

        public ActionResult ConcreteSpecimenView(decimal Test_ID)
        {
            Concrete_Specimen _objCONCSP = new Concrete_Specimen();
            _objCONCSP = new DAL_CONCRETE_SPECIMEN().View_Concrete_Specimens(Test_ID);
            _objCONCSP.FILE_PATH = _objCONCSP.FILE_PATH;
            _objCONCSP.IS_FILE_UPLOAD = _objCONCSP.IS_FILE_UPLOAD;
            return PartialView("ConcreteSpecimenView", _objCONCSP);
        }
        [HttpPost]
        [SubmitButtonSelector(Name = "Save")]
        [ActionName("ConcreteSpecimenEdit")]
        public ActionResult ICONCRETE_SPECIMEN_UPDATE(Concrete_Specimen _CONSPECI)
        {
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => new { x.Key, x.Value.Errors }).ToArray();

            if (ModelState.IsValid)
            {
                try
                {
                    ResultMessage objMst;
                    string result = new DAL_CONCRETE_SPECIMEN().UPDATE_CONCRETE_SPECIMEN(emp.Employee_Code, _CONSPECI, out objMst);

                    if (result == "")
                    {
                        Success(string.Format("<b>Concrete Specimen inserted successfully. Test NO : </b> <b style='color:red'> " + objMst.CODE + "</b>"), true);
                        return RedirectToAction("ConcreteSpecimenEdit", "Concrete");
                    }
                    else
                    {
                        Danger(string.Format("<b>Error:</b>" + result), true);
                    }
                }
                catch (Exception ex)
                {
                    Danger(string.Format("<b>Error:</b>" + ex.Message), true);
                }
            }
            else
            {
                Danger(string.Format("<b>Error:102 :</b>" + string.Join("; ", ModelState.Values.SelectMany(z => z.Errors).Select(z => z.ErrorMessage))), true);
            }
            var empExceptionList = new List<string> { "CAL0229", "CAL0230" };
            List<EMPLOYEE_DETAILS> _empList = new DAL_Common().GetEmployee_List("", "", "", "", "", emp.Employee_Code, "").Where(x => x.activeFlag == "Y" && !empExceptionList.Contains(x.Employee_Code)).ToList<EMPLOYEE_DETAILS>();
            _CONSPECI.EMPLOYEE_LIST = new SelectList(_empList.OrderBy(x => x.EmployeeName), "Employee_Code", "EmployeeName");


            return View(_CONSPECI);
        }

    }
}
