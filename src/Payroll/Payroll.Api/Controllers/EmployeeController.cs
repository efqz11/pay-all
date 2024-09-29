using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payroll.Models;
using Payroll.Services;

namespace Payroll.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService employeeService;
        private readonly ScheduleService scheduleService;
        private readonly RequestService requestService;
        private readonly NotificationService notificationService;
        private readonly UserResolverService userResolverService;
        private readonly CompanyService companyService;

        public EmployeeController(EmployeeService employeeService, ScheduleService scheduleService, RequestService requestService, NotificationService notificationService, UserResolverService userResolverService, CompanyService companyService)
        {
            this.employeeService = employeeService;
            this.scheduleService = scheduleService;
            this.requestService = requestService;
            this.notificationService = notificationService;
            this.userResolverService = userResolverService;
            this.companyService = companyService;
        }

        /// <summary>
        /// secure route that accepts HTTP GET requests and returns the details of the user with the specified id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await employeeService.GetCurrentEmployee();
            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Get Dashboard information (due tasks, whos out, upcoming birthdays & clock records)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/dashboard")]
        public async Task<IActionResult> GetDashboard(int id)
        {
            var pendingTasks = await employeeService.GetPendingTasks(id);
            var whosOut = await companyService.GetWhosOut(DateTime.Today);
            var upComingBDays = await companyService.GetUpcomingBirthdays(DateTime.Today);
            var getBiometricRecords = await employeeService.GeTodaysBiometricRecords(id);
            var data = new {
                pendingTasks,
                whosOut,
                UpcomingBirthDays = upComingBDays,
                BiometricRecords = getBiometricRecords,
            };

            return Ok(data);
        }

        /// <summary>
        /// Get This week time sheet (days, and hour mins worked)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/timesheet/today")]
        public async Task<IActionResult> GetTodaysTimeSheet(int id)
        {
            var user = await scheduleService.GetThisWeekTimeSheet(id);
            if (user == null) return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// Get Announcements related to employment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/announcements")]
        public async Task<IActionResult> GetAnnouncement(int id)
        {
            var user = await employeeService.GetAnnouncements(id);
            if (user == null) return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// Get Compensation related to employment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/compensations")]
        public async Task<IActionResult> GetCompensation(int id)
        {
            var user = await employeeService.GetCompensations(id);
            if (user == null) return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// Get job history related to employee
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/job-history")]
        public async Task<IActionResult> GetEmployeeJobHistory(int id)
        {
            var user = await employeeService.GetEmployeeJobHistory(id);
            if (user == null) return NotFound();

            return Ok(user);
        }


        ///// <summary>
        ///// Get employment history
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[HttpGet("{id}/employment-history")]
        //public async Task<IActionResult> GetEmploymentHistory(int id)
        //{
        //    var user = await employeeService.GetEmploymentHistory(id);
        //    if (user == null) return NotFound();

        //    return Ok(user);
        //}

        /// <summary>
        /// Get employees reporting directly or working under
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/direct-reports")]
        public async Task<IActionResult> GetDirectReports(int id)
        {
            var user = await employeeService.GetEmployeeDirectReports(id);
            if (user == null) return NotFound();

            return Ok(user);
        }



        /// <summary>
        /// Get Time-off balance for employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet("{id}/time-off/balance")]
        public async Task<IActionResult> GetTimeoffBalance(int id, int? year = null)
        {
            var user = await employeeService.GetDayOffSummary(id, year);
            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Get Time-off usage for employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("{id}/time-off/usage")]
        public async Task<IActionResult> GetTimeoffUsage(int id, int? year = null, int page = 1, int limit = 10)
        {
            var user = await employeeService.GetDayOffUsages(id, year, page, limit);
            if (user == null) return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// Get Schedules for employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet("{id}/schedules")]
        public async Task<IActionResult> GetSchedule(int id, DateTime start, DateTime end, int page = 1, int limit = 10)
        {
            var user = await scheduleService.GetAttendancesAsync(
                empIds :new[] { id }, 
                start: start, 
                end: end,
                page: 1,
                limit: limit);
            if (user == null) return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// Get Tasks for employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet("{id}/tasks")]
        public async Task<IActionResult> GetTasks(int id, DateTime start, DateTime end, int page = 1, int limit = 10)
        {
            var user = await scheduleService.GetTasksAsync(
                empIds: new[] { id },
                start: start,
                end: end,
                page: 1,
                limit: limit);
            if (user == null) return NotFound();

            return Ok(user);
        }



        /// <summary>
        /// Get Requests for employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet("{id}/requests")]
        public async Task<IActionResult> GetRequests(int id, DateTime start, DateTime end, RequestType? type = null, WorkItemStatus? status = null, int page = 1, int limit = 10)
        {
            var user = await requestService.GetRequests(id, start, end, type, status, page, limit);
            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Get Notifications for employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications(DateTime? start = null, DateTime? end = null, int type = 0, bool showSeen = false, int page = 1, int limit = 10)
        {
            var user = await notificationService.GetUserNotifications(
                    userResolverService.GetUserId(),
                    start,
                    end,
                    showSeen,
                    type,
                    page,
                    limit);
            if (user == null) return NotFound();

            return Ok(user);
        }

    }
}
