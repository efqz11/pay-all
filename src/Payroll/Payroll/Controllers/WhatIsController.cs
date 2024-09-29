using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;

namespace Payroll.Controllers
{
    public class WhatIsController : BaseController
    {
        private readonly PayrollDbContext context;
        private readonly AccountDbContext accountDbContext;
        private readonly CompanyService companyService;
        private readonly UserResolverService userResolverService;
        private readonly EmployeeService employeeService;
        private readonly ScheduleService scheduleService;
        private readonly FileUploadService fileUploadService;
        private readonly AccessGrantService accessGrantService;
        private readonly UserManager<AppUser> userManager;

        public WhatIsController(PayrollDbContext context, AccountDbContext accountDbContext, CompanyService companyService, UserResolverService userResolverService, EmployeeService employeeService, ScheduleService scheduleService, FileUploadService fileUploadService, AccessGrantService accessGrantService, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.accountDbContext = accountDbContext;
            this.companyService = companyService;
            this.userResolverService = userResolverService;
            this.employeeService = employeeService;
            this.scheduleService = scheduleService;
            this.fileUploadService = fileUploadService;
            this.accessGrantService = accessGrantService;
            this.userManager = userManager;
        }

        public async Task<IActionResult> PayAdjustment()
        {
            ViewBag.Header = "What is Pay Adjustment?";
            ViewBag.Summary = "A pay adjustment is a change in an employee's pay rate. You can change an employee's hourly wage or salary. Typically, compensation adjustment is an increase in the pay rate, such as when an employee earns a raise.";
            return PartialView("_PayAdjustment");
        }

    }

}
