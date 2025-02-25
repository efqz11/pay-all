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
    public class ResourceController : ControllerBase
    {
        private readonly PayrollService payrollService;
        private readonly EmployeeService employeeService;
        private readonly UserResolverService userResolverService;

        public ResourceController(PayrollService payrollService, EmployeeService employeeService, UserResolverService userResolverService)
        {
            this.payrollService = payrollService;
            this.employeeService = employeeService;
            this.userResolverService = userResolverService;
        }

        /// <summary>
        /// Get pay slips for the year
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/payslips/{year}")]
        public async Task<IActionResult> GetPaySlips(int id, int year)
        {
            var user = await payrollService.GetPayStubsByYear(id, year);
            if (user == null) return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// Get resources for the year
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/shared")]
        public async Task<IActionResult> GetSharedFiles(int id)
        {
            var user = await employeeService.GetCompanyFileShares(id);
            if (user == null) return NotFound();

            return Ok(user);
        }
        
    }
}
