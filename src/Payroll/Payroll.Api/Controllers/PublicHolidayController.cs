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
    [Route("public-holiday")]
    [ApiController]
    public class PublicHolidayController : ControllerBase
    {
        private readonly CompanyService companyService;
        private readonly EmployeeService employeeService;
        private readonly UserResolverService userResolverService;

        public PublicHolidayController(CompanyService companyService, EmployeeService employeeService, UserResolverService userResolverService)
        {
            this.companyService = companyService;
            this.employeeService = employeeService;
            this.userResolverService = userResolverService;
        }


        /// <summary>
        /// Get upcoming public holidays
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpComingPublicHolidays(int limit = 5)
        {
            var user = await companyService.GetUpComingPublicHolidays(limit);
            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Get public holiday for the year
        /// </summary>
        /// <returns></returns>
        [HttpGet("{year}")]
        public async Task<IActionResult> GetPublicHolidayForYear(int year)
        {
            var user = await companyService.GetUpComingPublicHolidaysForYear(year);
            if (user == null) return NotFound();

            return Ok(user);
        }

    }
}
