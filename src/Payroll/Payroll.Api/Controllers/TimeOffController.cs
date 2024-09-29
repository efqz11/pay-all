using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payroll.Services;

namespace Payroll.Api.Controllers
{
    [Authorize]
    [Route("time-off")]
    [ApiController]
    public class TimeOffController : ControllerBase
    {
        private readonly PayrollService payrollService;
        private readonly UserResolverService userResolverService;
        private readonly EmployeeService employeeService;

        public TimeOffController(PayrollService payrollService, UserResolverService userResolverService, EmployeeService employeeService)
        {
            this.payrollService = payrollService;
            this.userResolverService = userResolverService;
            this.employeeService = employeeService;
        }

        /// <summary>
        /// Get Time off balance, Usage and Leave requests
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/chart")]
        public async Task<IActionResult> GetChart(int id)
        {
            var _empId = id;
            var data = new
            {
                DayOffBalances = await employeeService.GetCurrentEmployeeDayOffBalances(_empId),
                DayOffUsages = await employeeService.GetCurrentEmployeeDayOffUsage(_empId),
                LeaveRequestByStatus = await employeeService.GetLeaveRequestByStatus(_empId),
            };
            return Ok(data);
        }


        /// <summary>
        /// Get Time off balance, Usage and Leave requests for year
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/chart/{year}")]
        public async Task<IActionResult> GetChartByYear(int id, int year)
        {
            var _empId = id;
            var _year = year;
            var data = new
            {
                DayOffBalances = await employeeService.GetCurrentEmployeeDayOffBalances(_empId, year),
                DayOffUsages = await employeeService.GetCurrentEmployeeDayOffUsage(_empId, year),
                LeaveRequestByStatus = await employeeService.GetLeaveRequestByStatus(_empId, year),
            };
            return Ok(data);
        }

        ///// <summary>
        ///// Get Time off usage summary
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[HttpGet("{id}/usage")]
        //public async Task<IActionResult> GetRemaining(int id)
        //{
        //    var data = await employeeService.GetDayOffSummary(id);
        //    if (data == null) return NotFound();
        //    return Ok(data);
        //}

        /// <summary>
        /// Get Time off usage summary for the year
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/usage/{year}")]
        public async Task<IActionResult> GetUsageSummary(int id, int year)
        {
            var data = await employeeService.GetDayOffSummary(id, year);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpGet("{id}/usage/summary")]
        public async Task<IActionResult> GetDayOffSummary(int id, int year)
        {
            var data = await employeeService.GetDayOffSummary(id, DateTime.Now.Year);
            if (data == null) return NotFound();
            return Ok(data);
        }



    }
}
