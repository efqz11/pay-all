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
    public class PaymentController : ControllerBase
    {
        private readonly PayrollService payrollService;
        private readonly UserResolverService userResolverService;

        public PaymentController(PayrollService payrollService, UserResolverService userResolverService)
        {
            this.payrollService = payrollService;
            this.userResolverService = userResolverService;
        }

        /// <summary>
        /// Get pay checks for the year
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/pay-stubs/{year}")]
        public async Task<IActionResult> GetPayStubsByYear(int id, int year)
        {
            var user = await payrollService.GetPayStubsByYear(id, year);
            if (user == null) return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// Get pay checks for the year
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/pay-stubs/start/{start}/end/{end}")]
        public async Task<IActionResult> GetPayStubsByYear(int id, DateTime start, DateTime end)
        {
            var user = await payrollService.GetPayStubsByDuration(id, start, end);
            if (user == null) return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// Get last paycheck for employee
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/pay-stubs/latest")]
        public async Task<IActionResult> GetLatestPaystub(int id)
        {
            var user = await payrollService.GetLastPayStub(id);
            if (user == null) return NotFound();

            return Ok(user);
        }


        /// <summary>
        /// Get pay componenents and values for given pay check
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/pay-stubs/{payid}/variants")]
        public async Task<IActionResult> GetPaystubVariants(int id, int payid)
        {
            var user = await payrollService.GetPayStubVariants(id, payid);
            if (user == null) return NotFound();

            return Ok(user);
        }

    }
}
