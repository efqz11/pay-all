using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payroll.Database;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Payroll
{
    public class SearchService
    {
        private readonly PayrollDbContext context;

        public SearchService(PayrollDbContext context) 
        {
            this.context = context;
        }

        public SearchResultVm GetSearhResult (string query, IUrlHelper urlHelper)
        {
            query = query.ToLower();


            if (query.StartsWith("employee") || query.StartsWith("staff"))
            {
                var empData = new SearchResultVm
                {
                    ResultType = "employee",
                    Result = context.Employees
                    .Include(x=> x.Department).Select(x => new SearchResultItemVm
                    {
                        Item1 = x.FirstName,
                        Item2 = x.Department.Name + " department",
                        Link = UrlHelperExtensions.Action(urlHelper, "Index", "Employee", new { id = x.Id }),
                        Icon = "ion-person-stalker"
                    }).ToList(),
                    SearchTerm = query
                };


                return empData;
            }

            if (query.StartsWith("payadjustment") || query.StartsWith("adjustment") || query.StartsWith("adjus"))
            {
                var empData = new SearchResultVm
                {
                    ResultType = "adjustment",
                    Result = context.PayAdjustments.Select(x => new SearchResultItemVm
                    {
                        Item1 = x.Name,
                        Item2 = x.VariationType.ToString(),
                        Link = urlHelper.Action("Index", "PayAdjustment", new { id = x.Id }),
                        Icon = "ion-android-options"
                    }).ToList(),
                    SearchTerm = query
                };


                return empData;
            }

            if (query.StartsWith("payrol"))
            {
                var empData = new SearchResultVm
                {
                    ResultType = "payrol",
                    Result = context.PayrollPeriods
                    .Include(x => x.PayrollPeriodEmployees).Select(x => new SearchResultItemVm
                    {
                        Item1 = x.Name,
                        Item2 = x.PayrollPeriodStrings,
                        Link = UrlHelperExtensions.Action(urlHelper, "Detail", "Payroll", new { id = x.Id }),
                        Item3 = x.PayrollPeriodEmployees.Count() + " Employees",
                        Icon = "ion-clipboard"
                    }).ToList(),
                    SearchTerm = query
                };


                return empData;
            }

            var XempData = new SearchResultVm
            {
                ResultType = "payrol",
                Result = context.Employees
                    .Where(x=> x.FirstName.ToLower().Contains(query) || x.EmpID.ToString() == query)
                    .Include(x => x.PayrollPeriodEmployees).Select(x => new SearchResultItemVm
                    {
                        Item1 = x.EmpID + " - "+ x.FirstName,
                        Item2 = x.Department.Name,
                        Link = UrlHelperExtensions.Action(urlHelper, "Detail", "Employee", new { id = x.Id }),
                        Item3 = x.PayrollPeriodEmployees.Count() + " Payrols",
                        Icon = "ion-user"
                    }).ToList(),
                SearchTerm = query
            };


            return XempData;
        }
    }
}
