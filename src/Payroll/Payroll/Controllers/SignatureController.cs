using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExcelDataReader;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;
using RadPdf.Web.UI;

namespace Payroll.Controllers
{
    [Authorize(Roles = Roles.PayAll.admin)]
    public class SignatureController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly AccountDbContext accDbcontext;
        private readonly UserResolverService userResolverService;
        private readonly EmployeeService employeeService;
        private readonly ILogger<CompanyController> logger;

        public SignatureController(PayrollDbContext context, ILogger<CompanyController> logger, AccountDbContext accountDbContext, UserResolverService userResolverService, EmployeeService employeeService)
        {
            this.context = context;
            this.logger = logger;
            this.accDbcontext = accountDbContext;
            this.userResolverService = userResolverService;
            this.employeeService = employeeService;
        }
        
        public async Task<IActionResult> Configure(int id)
        {
            var file = await context.CompanyFiles.FirstOrDefaultAsync(a => a.Id == id && a.IsSignatureAvailable);
            if (file == null)
                return NotFound();

            ViewBag.EmployeeFields = typeof(EmployeeSummaryVm).GetProperties()
                .Where(a=> a.CustomAttributes.Any(x=> x.AttributeType == typeof(DisplayAttribute)))
                .ToDictionary(a => ((DisplayAttribute)a.GetCustomAttributes(typeof(DisplayAttribute), false).First()).Name, x => x.Name);
            var stats = await context.CompanyFileShares.Where(a => a.CompanyFileId == id)
                .GroupBy(a => a.IsSigned)
                .ToDictionaryAsync(a => a.Key, a => a.Count());

            var data = new Dictionary<string, int>();
            data.Add("Signed" , stats.Where(a=> a.Key== true).Sum(a=> a.Value));
            data.Add("Waiting" , stats.Where(a=> a.Key== false).Sum(a=> a.Value));
            ViewBag.Stats = data;
            return View(file);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(int id, [FromBody]List<FillableConfiguration> pagedConfig)
        {
            var file = await context.CompanyFiles.FirstOrDefaultAsync(a => a.Id == id && a.IsSignatureAvailable);
            if (file == null)
                return ThrowJsonError("File was not found!");
            if (pagedConfig == null)
                return ThrowJsonError("Configure values are empty");

            if (pagedConfig.Count <= 0)
                return ThrowJsonError("Kindly post some data for configuration");
            if (pagedConfig.Count(a => a.required) <= 0 && pagedConfig.Count(a => a.elementType == "signature") <= 0)
                return ThrowJsonError("There must be atleast one required field or signatue to proceed");

            file.FillableConfiguration = pagedConfig.Where(a => !a.deleted).ToList();
            file.IsSignatureSetupCompleted = true;
            await context.SaveChangesAsync();
            return Ok();
        }


        public async Task<IActionResult> SendRequest(int id)
        {
            var file = await context.CompanyFiles
                .Include(a=> a.CompanyFileShares)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsSignatureAvailable && a.IsSignatureSetupCompleted);
            if (file == null)
                return ThrowJsonError("File was not found or signature setup is incomplete!");
            var empSelector = GetEmployeeSelectorModal();
            if(!empSelector.IsValid())
                return ThrowJsonError("Employee was not selected!");

            var msg = $"We will be sending signature requests to {empSelector.Summary} - {empSelector.EmployeeIds.Count()} employees.";
            if (file.CompanyFileShares.Count() > 0)
                msg+= $"<br>(already sent to {file.CompanyFileShares.Count()} employee(s) and they will be excluded)";

            SetTempDataMessage(msg, MsgAlertType.warning);
            return PartialView("_SendRequest", file);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequestConfirm(int id)
        {
            var file = await context.CompanyFiles
                .Include(a => a.CompanyFileShares)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsSignatureAvailable && a.IsSignatureSetupCompleted);
            if (file == null)
                return ThrowJsonError("File was not found or signature setup is incomplete!");

            var empSelector = GetEmployeeSelectorModal();
            if (!empSelector.IsValid())
                return ThrowJsonError("Employee was not selected!");

            var sentEmps = file.CompanyFileShares.Select(a => a.EmployeeId).ToArray();
            var notSentEmployees = empSelector.EmployeeIds.Except(sentEmps);

            if(notSentEmployees.Count () > 0)
            {
                var shares = notSentEmployees.Select(a => new CompanyFileShare
                {
                    EmployeeId = a,
                    CompanyFileId = id,
                });

                context.CompanyFileShares.AddRange(shares);
                await context.SaveChangesAsync();
            }

            return ThrowJsonSuccess();
        }


        public async Task<IActionResult> Sign(int id)
        {
            var file = await context.CompanyFileShares
                .Include(a => a.CompanyFile)
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id && a.EmployeeId == userResolverService.GetEmployeeId() && a.CompanyFile.IsSignatureSetupCompleted);
            if (file == null)
                return NotFound();

            if (file.EmployeeId != userResolverService.GetEmployeeId())
                return NotFound("Unauthorized to access this file");

            var emp = await employeeService.GetCurrentEmployeeSummaryAsync(file.EmployeeId);

            var empFields = emp.GetType().GetProperties()
                .ToDictionary(a => "{" + a.Name + "}", a => a.GetValue(emp, null)?.ToString() ?? "");

            var fields = file.CompanyFile.FillableConfiguration;
            foreach (var item in fields)
            {
                if (item.elementType == "placeholder")
                    item.value = empFields.ContainsKey(item.placeholder) ? empFields[item.placeholder] : "";
                else if (item.elementType == "inputField")
                    item.value = file.FileConfigValues.ContainsKey(item.name) ? file.FileConfigValues[item.name].ToString() : "";
                else if (item.elementType == "signature")
                    item.value = file.FileConfigValues.ContainsKey(item.name) ? file.FileConfigValues[item.name].ToString() : "";
            }

            // replace with updated placeholders
            file.CompanyFile.FillableConfiguration = fields;

            return View(file);
        }


        [HttpPost]
        public async Task<IActionResult> SaveValues(int id, [FromBody]List<FillableConfiguration> values)
        {
            var file = await context.CompanyFileShares
                .Include(a => a.CompanyFile)
                .FirstOrDefaultAsync(a => a.Id == id && a.EmployeeId == userResolverService.GetEmployeeId() && a.CompanyFile.IsSignatureSetupCompleted);
            if (file == null)
                return NotFound();

            if (values == null)
                return ThrowJsonError("Configure values are empty");

            var fillInElementTypes = new[] { "placeholder", "inputField", "signature" };

            if (file.CompanyFile.FillableConfiguration.Count(a => fillInElementTypes.Contains(a.elementType))
                !=
                values.Count(v => fillInElementTypes.Contains(v.elementType) && !string.IsNullOrWhiteSpace(v.value)))
                return ThrowJsonError("Kindly fill in all the elemnts");

            var dict = new Dictionary<string, object>();
            foreach (var item in file.CompanyFile.FillableConfiguration)
            {
                if(fillInElementTypes.Contains(item.elementType))
                    dict.Add(item.name, values.FirstOrDefault(x=> x.name == item.name)?.value ?? "");
                else
                    dict.Add(item.name, item.value);
            }

            file.FileConfigValues = dict;
            file.IsSigned = true;
            file.SignedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return Ok();
        }

        public async Task<IActionResult> GetTestFile()
        {
            var myfile = System.IO.File.ReadAllBytes("wwwroot/test.pdf");
            return new FileContentResult(myfile, "application/pdf");

        }
    }


}

