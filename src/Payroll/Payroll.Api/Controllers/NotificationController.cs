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
    [Route("[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly PayrollService payrollService;
        private readonly EmployeeService employeeService;
        private readonly UserResolverService userResolverService;
        private readonly ScheduleService scheduleService;
        private readonly NotificationService notificationService;

        public NotificationController(PayrollService payrollService, EmployeeService employeeService, UserResolverService userResolverService, ScheduleService scheduleService, NotificationService notificationService)
        {
            this.payrollService = payrollService;
            this.employeeService = employeeService;
            this.userResolverService = userResolverService;
            this.scheduleService = scheduleService;
            this.notificationService = notificationService;
        }

        /// <summary>
        /// Get all notification types
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("types")]
        public async Task<IActionResult> Get()
        {
            var user = await notificationService.GetNotificationTypes();
            if (user == null) return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// Get notification details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await notificationService.GetNotificationAsync(id);
            if (user == null) return NotFound();

            return Ok(user);
        }
    }
}
