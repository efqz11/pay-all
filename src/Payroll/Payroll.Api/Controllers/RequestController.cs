using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payroll.Models;
using Payroll.Models.ViewModels;
using Payroll.Services;

namespace Payroll.Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly RequestService requestService;
        private readonly EmployeeService employeeService;
        private readonly UserResolverService userResolverService;
        private readonly ScheduleService scheduleService;

        public RequestController(RequestService requestService, EmployeeService employeeService, UserResolverService userResolverService, ScheduleService scheduleService)
        {
            this.requestService = requestService;
            this.employeeService = employeeService;
            this.userResolverService = userResolverService;
            this.scheduleService = scheduleService;
        }

        /// <summary>
        /// Get Submitted Requests for the company (year)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet("pending")]
        public async Task<IActionResult> GetRequests(DateTime? start = null, DateTime? end = null, RequestType? type = null, int page = 1, int limit = 10)
        {
            var user = await requestService.GetRequestsCompany(start ?? DateTime.Now.AddMonths(-12), end ?? DateTime.Now.AddMonths(12), type, WorkItemStatus.Submitted, page, limit);
            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Get All Requests for the company
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetRequests(DateTime start, DateTime end, RequestType? type = null, WorkItemStatus? status = null, int page = 1, int limit = 10)
        {
            var user = await requestService.GetRequestsCompany(start, end, type, status, page, limit);
            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Get Request detail by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequest(int id)
        {
            var user = await requestService.GetRequestById(id);
            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Approve or Reject submitted Request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("take-action")]
        public async Task<IActionResult> TakeRequestAction(RequestActionVm model)
        {
            try
            {
                await requestService.ProcessRequestAction(model);
                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        /// <summary>
        /// Create or update request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ProcessRequest(ProcessRequestVm model)
        {
            Request request = null;
            try
            {
                request = await requestService.ProcessRequest(model);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }

            return Ok(request);
        }


        
    }
}
