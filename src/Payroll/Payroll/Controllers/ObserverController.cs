using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Payroll.Database;
using Payroll.Models;
using Payroll.Services;
using Payroll.ViewModels;
using Serilog.Events;

namespace Payroll.Controllers
{
    public class ObserverController : BaseController
    {
        private readonly LogDbContext context;

        public ObserverController(LogDbContext context)
        {
            this.context = context;
        }

        

        public async Task<IActionResult> Index(DateTime? start = null, DateTime? end = null, string env = "", int limit = 10, int page = 1)
        {
            var task = await context.ApplicationLogs
                .Where(x => (!start.HasValue && !end.HasValue || start >= x.TimeStamp && end <= x.TimeStamp) && (string.IsNullOrWhiteSpace(env) || x.Environment == env))
             .OrderByDescending(a => a.TimeStamp)
             .Skip((page - 1) * limit)
             .Take(limit)
             .ToListAsync();

            if (task == null)
                return BadRequest();

            return View(task);
        }

        public async Task<IActionResult> ViewSummary(int id)
        {
            var task = await context.ApplicationLogs.FindAsync(id);
            if (task == null)
                return ThrowJsonError();

            try
            {
                Properties data = null;
                var xml = new XmlSerializer(typeof(Properties));
                using (StringReader sr = new StringReader(task.Properties))
                {
                    data = (Properties)xml.Deserialize(sr);
                }

                ViewBag.Props = data;
            }
            catch (Exception)
            {
            }
            
            return PartialView("_ViewSummary", task);
        }

    }


    [XmlRoot(ElementName = "property")]
    public class Property
    {
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "properties")]
    public class Properties
    {
        [XmlElement(ElementName = "property")]
        public List<Property> Property { get; set; }
    }
}