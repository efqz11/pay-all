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
    public class ScheduleController : ControllerBase
    {
        private readonly PayrollService payrollService;
        private readonly EmployeeService employeeService;
        private readonly UserResolverService userResolverService;
        private readonly ScheduleService scheduleService;

        public ScheduleController(PayrollService payrollService, EmployeeService employeeService, UserResolverService userResolverService, ScheduleService scheduleService)
        {
            this.payrollService = payrollService;
            this.employeeService = employeeService;
            this.userResolverService = userResolverService;
            this.scheduleService = scheduleService;
        }

        /// <summary>
        /// Get attendance details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await scheduleService.GetAttendance(id);
            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Post new biometric record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}/biometric")]
        public async Task<IActionResult> NewBiometricRecord(int id, [FromBody]BiometricRecord model)
        {
            if (!ModelState.IsValid) return NotFound(ModelState);
            await scheduleService.AddNewBiometricRecord(id, model);
            return Ok();
        }
    }
    
}
