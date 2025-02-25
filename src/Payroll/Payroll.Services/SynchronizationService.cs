using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payroll.Database;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Services
{
    public class SynchronizationService
    {
        private readonly AccountDbContext context;
        private readonly PayrollDbContext payrolDbContext;
        private readonly UserResolverService userResolverService;
        private readonly ILogger<SynchronizationService> logger;
        private readonly IBackgroundJobClient backgroundJobClient;

        public SynchronizationService(AccountDbContext context, PayrollDbContext payrolDbContext, UserResolverService userResolverService,
          ILogger<SynchronizationService> logger, IBackgroundJobClient backgroundJobClient)
        {
            this.context = context;
            this.payrolDbContext = payrolDbContext;
            this.userResolverService = userResolverService;
            this.logger = logger;
            this.backgroundJobClient = backgroundJobClient;
        }
        
        /// <summary>
        /// Creates or Updates Company on Payrol Database
        /// Chooses company, set connection strins and run create or update command
        /// </summary>
        /// <param name="companyId"></param>
        public async Task<IdentityResult> SyncCompanyOnMasterDatabase(int companyId, string companyName = "", Address locAddress = null)
        {
            logger.LogWarning("Sychronizing Company Accounts with Master Paroll Company");
            var cmpAccount = await context.CompanyAccounts.FindAsync(companyId);
            if (cmpAccount == null) throw new NullReferenceException("Company was not found");

            if(cmpAccount.Status != CompanyStatus.Approved)
            {
                logger.LogWarning("Company account was not 'Approved'. Consider approving first and then run sync");
            }

            // checking Payrol database >> Have any
            bool exist = await payrolDbContext.Companies.AnyAsync(x => x.Id == companyId);
            if (exist)
            {
                var rec = await payrolDbContext.Companies
                    .Include(x=> x.Locations)
                    .ThenInclude(x => x.Addresses)
                    .FirstOrDefaultAsync(x=> x.Id == companyId);
                rec.Name = cmpAccount.Name;
                //rec.Address = cmpAccount.Address;
                rec.LogoUrl = cmpAccount.LogoUrl;
                rec.Website = cmpAccount.Website;
                rec.Hotline = cmpAccount.Hotline;
                rec.TaxCode = cmpAccount.TaxCode;
                rec.ManagingDirector = cmpAccount.ManagingDirector;

                logger.LogWarning("Already exists, Updating all fields", rec);
                if (locAddress != null && !rec.Locations.Any(x=> x.Name.Equals(locAddress.GetAddressString())))
                    rec.Locations.Add(new Location
                    {
                        Name = locAddress.GetAddressString(),
                        Addresses = new List<Address> { new Address { Street1 = locAddress.Street1, Street2 = locAddress.Street2, AddressType = AddressType.Company, City = locAddress.City, State = locAddress.State, ZipCode = locAddress.ZipCode, } }
                    });

                payrolDbContext.Companies.Update(rec);
            }
            else
            {
                var rec = new Company { Id = companyId };
                rec.Name = cmpAccount.Name;
                //rec.Address = cmpAccount.Address = "Address";
                rec.LogoUrl = cmpAccount.LogoUrl;
                rec.Website = cmpAccount.Website;
                rec.Hotline = cmpAccount.Hotline;
                rec.TaxCode = cmpAccount.TaxCode;
                rec.ManagingDirector = cmpAccount.ManagingDirector;

                if (locAddress != null)
                    rec.Locations.Add(new Location {
                        Name = locAddress.GetAddressString(),
                        Addresses = new List<Address> { new Address { Street1 = locAddress.Street1, Street2 = locAddress.Street2, AddressType = AddressType.Company, City = locAddress.City, State = locAddress.State, ZipCode = locAddress.ZipCode, } } 
                    });

                payrolDbContext.Companies.Add(rec);

                logger.LogWarning("None found!, Creating new company master paruoll", rec);
            }

            int result = await payrolDbContext.SaveChangesAsync();
            logger.LogWarning(result + "record(s) updated to database");


            //backgroundJobClient.Enqueue(() => SyncSchedulesAttendanceOnCompanyWorkTimeUpdate(companyId, "Updating Attendance time and color"));

            return result > 0 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError { Description = "Failied on Save Changeds" });
        }


        /// <summary>
        /// Creates or Updates Company on Payrol Database
        /// Chooses company, set connection strins and run create or update command
        /// </summary>
        /// <param name="companyId"></param>
        public async Task<IdentityResult> SyncSchedulesAttendanceOnCompanyWorkTimeUpdate(int companyId, string message)
        {
            logger.LogWarning("Sychronizing Company Accounts with Master Paroll Company");
            var cmpWorkTimes = await payrolDbContext.CompanyWorkTimes.Where(x => x.CompanyId == companyId && x.IsActive).ToListAsync();
            if (cmpWorkTimes == null) throw new NullReferenceException("Company work time(s) was not found");
            var cmpWorkTimeIds = cmpWorkTimes.Select(a => a.Id).ToArray();

            var schedules = await payrolDbContext.Schedules.Where(x => x.CompanyId == companyId).ToListAsync();
            schedules.ForEach(t =>
            {
                if (t.DaysData != null && t.DaysData.Count > 0)
                {
                    var _data = t.DaysData.ToList();
                    foreach (var item in _data)
                    {
                        item.ShiftName = cmpWorkTimes.FirstOrDefault(x => x.Id == item.ShiftId)?.ShiftName;
                        item.Color = cmpWorkTimes.FirstOrDefault(x => x.Id == item.ShiftId)?.ColorCombination;
                        item.WorkStart = cmpWorkTimes.First(x => x.Id == item.ShiftId)?.StartTime;
                        item.WorkEnd = cmpWorkTimes.First(x => x.Id == item.ShiftId)?.EndTime;
                    }

                    t.DaysData = _data;
                }
            });


            var attendaces = await payrolDbContext.Attendances.Where(x => x.CompanyId == companyId && cmpWorkTimeIds.Contains(x.ShiftId)).ToListAsync();
            attendaces.ForEach(a =>
            {
                var _m = cmpWorkTimes.FirstOrDefault(x => x.Id == a.ShiftId);
                var _endDate = _m.EndTime.Hours < _m.StartTime.Hours ? a.WorkStartTime.AddDays(1) : a.WorkStartTime;

                a.ShiftColor = _m.ColorCombination;
                a.ShiftName = _m.ShiftName;
                a.WorkStartTime = new DateTime(a.WorkStartTime.Year, a.WorkStartTime.Month, a.WorkStartTime.Day, _m.StartTime.Hours, _m.StartTime.Minutes, _m.StartTime.Seconds);

                a.WorkEndTime = new DateTime(_endDate.Year, _endDate.Month, _endDate.Day, _m.EndTime.Hours, _m.EndTime.Minutes, _m.EndTime.Seconds);
            });

            //var workItems = await payrolDbContext.WorkItems.Where(x => x.Work.CompanyId == companyId).ToListAsync();
            //workItems.ForEach(t =>
            //{
            //    t.WorkColor = schedules.FirstOrDefault(x => x.Id == t.ScheduleId)?.Color;
            //    t.WorkStartTime = t.WorkStartTime.Add(schedules.First(x => x.Id == t.ScheduleId).WorkStart.Value);
            //    t.WorkStartTime = t.WorkStartTime.Add(schedules.First(x => x.Id == t.ScheduleId).WorkEnd.Value);
            //});


            int result = await payrolDbContext.SaveChangesAsync();

            logger.LogWarning(result + "record(s) updated to database");
            return result > 0 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError { Description = "No Records updated to database" });
        }
    }
}
