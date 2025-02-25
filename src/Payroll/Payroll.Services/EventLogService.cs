using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Payroll.Model.Logs;
using Payroll.Models;
using Payroll.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Services
{
    public class EventLogService
    {
        private readonly Database.LogDbContext context;
        private readonly UserResolverService userResolverService;
        private readonly UserManager<AppUser> userManager;

        public EventLogService(Payroll.Database.LogDbContext context, UserResolverService userResolverService,
            UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userResolverService = userResolverService;
            this.userManager = userManager;
        }
        


        public async Task<bool> LogEventAsync(int result, int dataType, int logType, string key, string summary, string[] actions = null)
        {
            EventLog eventLogObj = new EventLog();

            eventLogObj.EventResultId = result;
            eventLogObj.EventDataTypeId = dataType;
            eventLogObj.EventLogTypeId = logType;
            eventLogObj.DataItemKey = key;
            eventLogObj.Summary = summary;
            eventLogObj.TimeStamp = DateTime.UtcNow;

            if (actions != null)
                eventLogObj.ActionDetails = actions;

            context.EventLogs.Add(eventLogObj);

            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public async Task<List<EventLog>> GetEventsAsync(int logType, string key, DateTime? start, DateTime? end, int limit, int page)
        {
            return await context.EventLogs.Where(a => a.DataItemKey == key && a.EventLogTypeId == logType && ((start == null || end == null) || (a.TimeStamp >= start.Value && a.TimeStamp <= end.Value)))
             .Skip((page - 1) * limit)
             .Take(limit)
                .Include(a => a.EventLogType)
                .Include(a => a.EventDataType)
                .Include(a => a.EventResult)
                .OrderByDescending(a => a.TimeStamp)
             .ToListAsync();
        }


        public async Task<EventLog> GetEventAsync(int logType, int id)
        {
            return await context.EventLogs.Where(a => a.EventLogTypeId == logType && a.Id == id)
                .Include(a=> a.EventLogType)
                .Include(a=> a.EventDataType)
                .Include(a=> a.EventResult)
                .OrderByDescending(a=> a.TimeStamp)
             .FirstOrDefaultAsync();
        }

        public async Task<EventLog> GetEventAsync(int id)
        {
            return await context.EventLogs.Where(a => a.Id == id)
                .Include(a => a.EventLogType)
                .Include(a => a.EventDataType)
                .Include(a => a.EventResult)
             .FirstOrDefaultAsync();
        }
    }
}
