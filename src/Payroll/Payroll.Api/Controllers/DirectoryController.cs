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
    public class DirectoryController : ControllerBase
    {
        private readonly EmployeeService employeeService;

        public DirectoryController(EmployeeService employeeService)
        {
            this.employeeService = employeeService;
        }
        // GET api/values
        [HttpGet]
        public async Task<ActionResult> All(int groupBy = 1, string query = "")
        {
            var data = await employeeService.GetDirectoryAll(groupBy, query);
            return Ok(data.ToDictionary(a=> a.Key, a=> a.ToList()));
        }

        //// GET api/values/5
        //[HttpGet("{id}")]
        //public ActionResult<string> Circle(int id)
        //{
        //    return "value";
        //}

    }
}
