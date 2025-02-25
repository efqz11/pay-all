using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    [Authorize]
    //[A1AuthorizePermission]
    public class BaseController : Controller
    {

        [TempData]
        public int WorkingEmployeeId { get; set; }
        public int WorkingEmployeeObject { get; set; }


        public void UpdateWorkingEmployeeId(int? empId, string text)
        {
            if (empId.HasValue)
            {
                if(empId == 0)
                {
                    this.HttpContext.Session.Remove(nameof(WorkingEmployeeId));
                    this.HttpContext.Session.Remove(nameof(WorkingEmployeeObject));
                }
                else
                {
                    this.HttpContext.Session.SetInt32(nameof(WorkingEmployeeId), empId.Value);

                    if (!string.IsNullOrWhiteSpace(text))
                        this.HttpContext.Session.SetString(nameof(WorkingEmployeeObject),
                            JsonConvert.SerializeObject(new { id = empId, text }));
                }
            }

            //TempData.Keep(nameof(WorkingEmployeeId));
        }


        public async Task<IActionResult> SelectEmployees(string act = "", string cnt = "", string onsuccess = "", string update = "", int? routeDataId = 0)
        {
            Database.PayrollDbContext context = GetPayrollDbContext();
            Services.UserResolverService userResolverService = GetUserResolverService();
            var q = context.Employees
                .Where(x => x.Department.CompanyId == userResolverService.GetCompanyId());

            var deptGrp = await q.GroupBy(a => new { a.DepartmentId, a.Department.Name })
                .Select(a => Tuple.Create(a.Key.DepartmentId, a.Key.Name, a.Count())).ToListAsync();
            var locGrp = await q.Where(a => a.LocationId.HasValue)
                .GroupBy(a => new { a.LocationId, a.Location.Name })
                .Select(a => Tuple.Create(a.Key.LocationId, a.Key.Name, a.Count())).ToListAsync();
            var empJobType = await q // .Where(a => a.JobType.HasValue)
                .GroupBy(a => new { id = (int?)a.JobType, a.JobType } )
                .Select(a => Tuple.Create(a.Key.id, a.Key.JobType, a.Count())).ToListAsync();


            var model = GetEmployeeSelectorModal();
            var employeeSelectorVm = new EmployeeSelectorVm
            {
                ByLocation = locGrp,
                ByDept = deptGrp,
                ByJobType = empJobType,
                Action = act ?? "NewAnnouncement",
                Controller = cnt  ?? "NewsUpdate",
                Update = update ?? ".modal__container",
                RouteDataId = routeDataId,
                OnSuccess = onsuccess,

                ChosenEmployeeDataString = model?.ChosenEmployeeDataString ?? "[]",
                GroupByCategory = model?.GroupByCategory ?? GroupByCategory.ChooseEmployees,
                GroupByCategoryValue = model?.GroupByCategoryValue ?? "",
                TotalMatchedEmployees = model?.TotalMatchedEmployees ?? 0,
                TotalFoundEmployees = model?.TotalFoundEmployees ?? 0,
                EmployeeIds = model?.EmployeeIds ?? null,
                Summary = model?.Summary ?? "",
                IsEditView = model != null
            };

            return PartialView("_EmployeeSelector", employeeSelectorVm);
        }

        private Services.UserResolverService GetUserResolverService()
        {
            return (Payroll.Services.UserResolverService)
                            HttpContext.RequestServices.GetService
                            (typeof(Payroll.Services.UserResolverService));
        }

        private Database.PayrollDbContext GetPayrollDbContext()
        {
            return (Payroll.Database.PayrollDbContext)
                            HttpContext.RequestServices.GetService
                            (typeof(Payroll.Database.PayrollDbContext));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectEmployees(EmployeeSelectorVm model)
        {
            if (!ModelState.IsValid) return ThrowJsonError(ModelState);

            if (model.GroupByCategory != GroupByCategory.ChooseEmployees && string.IsNullOrWhiteSpace(model.GroupByCategoryValue))
                return ThrowJsonError("Please choose atleast one option");


            Database.PayrollDbContext context = GetPayrollDbContext();
            Services.UserResolverService userResolverService = GetUserResolverService();

            int[] empIds = null;
            string[] cats = null;
            string summary = null;
            var totalMatched = 0;
            var q = context.Employees
                .Where(x => x.CompanyId == userResolverService.GetCompanyId());
            var selectedIds = model.GroupByCategoryValueArray;
            switch (model.GroupByCategory)
            {
                case GroupByCategory.Location:
                    totalMatched = await q.CountAsync(a => a.LocationId.HasValue && selectedIds.Contains(a.LocationId.Value));
                    cats = await context.Locations.Where(a => a.CompanyId == userResolverService.GetCompanyId() && selectedIds.Contains(a.Id)).Select(a => a.Name).ToArrayAsync();

                    empIds = await q.Where(x => x.LocationId.HasValue && selectedIds.Contains(x.LocationId.Value))
                                .Select(x => x.Id).ToArrayAsync();
                    summary = cats.Count() == 1 ? "Location: " + cats.First() : "Multiple Locations";
                    break;
                case GroupByCategory.Department:
                    totalMatched = await q.CountAsync(a => a.DepartmentId.HasValue && selectedIds.Contains(a.DepartmentId.Value));
                    cats = await context.Departments.Where(a => a.CompanyId == userResolverService.GetCompanyId() && selectedIds.Contains(a.Id)).Select(a => a.Name).ToArrayAsync();

                    empIds = await q.Where(x => x.DepartmentId.HasValue && selectedIds.Contains(x.DepartmentId.Value))
                                .Select(x => x.Id).ToArrayAsync();
                    summary = cats.Count() == 1 ? "Department: " + cats.First() : "Multiple Departments";
                    break;
                case GroupByCategory.JobType:
                    totalMatched = await q.CountAsync(a => selectedIds.Contains((int)a.JobType));
                    cats = selectedIds.Select(x => ((JobType?)x).GetDisplayName()).ToArray();

                    empIds = await q.Where(x => selectedIds.Contains((int)x.JobType))
                                .Select(x => x.Id).ToArrayAsync();
                    summary = cats.Count() == 1 ? "Job Type: " + cats.First() : "Multiple JobTypes";
                    break;
                case GroupByCategory.ChooseEmployees:
                    totalMatched = await q.CountAsync(a => model.EmployeeIds.Contains(a.Id));
                    cats = default(string[]);
                    empIds = model.EmployeeIds;
                    var empData = await q.Where(a => empIds.Contains(a.Id)).Select(a => new { a.Id, text = a.GetSystemName(User) }).ToArrayAsync();
                    summary = totalMatched == 1 ? "Single Employee:" + empData.First().text : "Multiple Employees chosen";
                    model.ChosenEmployeeDataString = JsonConvert.SerializeObject(empData);
                    break;
                default:
                    break;
            }

            if (totalMatched <= 0) return ThrowJsonError("There aren't any matching employees!");

            model.EmployeeIds = empIds;
            model.Summary = summary;
            model.TotalMatchedEmployees = totalMatched;
            model.TotalFoundEmployees = await q.CountAsync();
            SaveEmployeeSelectorModal(model);

            if (model.RouteDataId != null)
                return RedirectToAction(model.Action, model.Controller, new { id = model.RouteDataId });

            return RedirectToAction(model.Action, model.Controller, new { model });
        }


        public int GetWorkingEmployeeId()
        {
            var _empId = this.HttpContext.Session.GetInt32(nameof(WorkingEmployeeId));
            TempData[nameof(WorkingEmployeeId)] = _empId ?? -1;
            TempData.Keep(nameof(WorkingEmployeeId));
            return _empId ?? 0;
        }


        public void SaveInSession(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
                this.HttpContext.Session.SetString(SessionVariables.UserSaveInSessionData, data);
        }

        public T GetDataSavedInSesssion<T>() where T:class
        {
            var data = this.HttpContext.Session.GetString(SessionVariables.UserSaveInSessionData);
            try
            {
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SaveEmployeeSelectorModal(EmployeeSelectorVm model)
        {
            if (model != null)
            {
                this.HttpContext.Session.SetString(SessionVariables.EmployeeSelector, JsonConvert.SerializeObject(model));
                var summary = $"{model?.Summary ?? "None"} · <i class='fad fa-users'></i> {model?.TotalMatchedEmployees ?? 0}";

                this.HttpContext.Session.SetString(SessionVariables.EmployeeSelectorSummary, summary);
            }

            //TempData.Keep(nameof(WorkingEmployeeId));
        }

        public EmployeeSelectorVm GetEmployeeSelectorModal()
        {
            var data = this.HttpContext.Session.GetString(SessionVariables.EmployeeSelector);
            try
            {
                return JsonConvert.DeserializeObject<EmployeeSelectorVm>(data);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public void SetTempDataMessage(string message, MsgAlertType alertType = MsgAlertType.info)
        {
            TempData["alert.type"] = alertType.ToString();
            TempData["alert.message"] = message;
            TempData["alert"] = true;
        }

        /// <summary>
        /// Throws beautifully formatted JSON Error which can be handled by JS
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public JsonResult ThrowJsonError(Exception e)
        {
            //Logger.Error(e.Message, e);
            Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            return Json(new { message = e.Message });
        }


        public JsonResult ThrowJsonError(ModelStateDictionary modelState)
        {
            ((ILogger)this.HttpContext.RequestServices.GetService(typeof(ILogger<BaseController>))).LogError("Model State Validation Error", JsonConvert.SerializeObject(modelState));

            var query = from state in modelState.Values
                        from error in state.Errors
                        select error.ErrorMessage;

            var errorList = query.ToList();

            Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            return Json(new { Message = errorList.FirstOrDefault() });
        }

        public JsonResult ThrowJsonError(ModelStateDictionary modelState, string[] keys)
        {
            var errorList = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            

            Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            return Json(new { Message = errorList.FirstOrDefault(x=> keys.Contains(x.Key)).Value.FirstOrDefault() });
        }

        public bool HasErrorsInModelState(ModelStateDictionary modelState, string[] keys)
        {
            var errorList = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return errorList.Any(a => keys.Contains(a.Key));
        }


        /// <summary>
        /// Throw json error with message
        /// 'We're sorry to inform you that an error has occured. please try again in sometime'
        /// </summary>
        /// <returns></returns>
        public JsonResult ThrowJsonError(string msg = "We're sorry to inform you that an error has occured. please try again in sometime")
        {
            return
                ThrowJsonError( 
                    new Exception(msg));
        }

        public JsonResult ThrowJsonSuccess(string msg = "Operation completed successfully")
        {
            Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
            return Json(new { Message = msg });
        }
        public JsonResult ThrowJsonSuccess(object msg)
        {
            Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
            return Json(msg);
        }
        //- See more at: http://www.leniel.net/2013/12/getting-aspnet-mvc-action-exception-message-on-ajax-fail-error-callback.html#sthash.5Jqjx2bA.dpuf

    }

    public class NotificationResponseType
    {
    }
}