using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class SetupOverviewPageVm
    {
        public string Title { get; set; }
        public string Icon{ get; set; }
        /// <summary>
        /// Question and answers (qn, ans)
        /// </summary>
        /// <value></value>
        public IDictionary<string, string> QuestionsAnswers { get; set; }

        public string MainActionUrl { get; set; }


        public IList<string> ItemsToHaveReady { get; set; }

        /// <summary>
        /// Link to user manual (title, link)
        /// </summary>
        /// <value></value>
        public IDictionary<string, string> UserManualLinks { get; set; }
        public string BtnName { get; }
        public int Step { get; }
        public string btnLink { get; }
        public CompanyAccount Company { get;  set; }

        public SetupOverviewPageVm(int step, (string icon, string title, string baseUrl) p)
        {
            var icon = ";";
            var qn = new Dictionary<string, string>();
            var items = new List<string>();
            var manual = new Dictionary<string, string>();
            //var btnName = p.title;

            this.btnLink = p.baseUrl;
            this.Title = p.title;
            this.Icon = p.icon;
            this.BtnName = p.title;
            this.Step = step;
            switch (step)
            {
                case 1: 
                qn.Add("Why do we need to ask for this?", "We need to know where your employees are located in order to calculate and file your taxes accurately. Please enter every address where you have employees physically working in the United States, including remote employees and employees who work from home");
                items.AddRange(new [] { "Physical address", "Filing address" ,"Mailing address" , "Company phone number" });
                manual.Add("Get Started: Add Company Addresses", "#"+ step);
                break;

                case 2:
                    qn.Add("Why do we need to ask for this?", "We need to know when to pay your employees. Some states have laws around when you must pay your employees. Please choose pay schedules that are legal for your employees.");
                    items.AddRange(new[] { "Pay period", "Pay dates", "Desired date of first payroll with Payall" });
                    manual.Add("Get Started: Add Company Addresses", "#" + step);
                    break;

                case 3:
                    qn.Add("Why do we need to ask for this?", "We need to know when to pay your employees. Some states have laws around when you must pay your employees. Please choose pay schedules that are legal for your employees.");
                    items.AddRange(new[] { "Official Company Logo", "Soft copy in png/jpg 50 x 50 (pixels)" });
                    manual.Add("Get Started: Add Company Addresses", "#" + step);
                    break;

                case 4:
                    qn.Add("Why do we need to ask for this?", "We need to know when to pay your employees. Some states have laws around when you must pay your employees. Please choose pay schedules that are legal for your employees.");
                    items.AddRange(new[] { "Pay period", "Pay dates", "Desired date of first payroll with Payall" });
                    manual.Add("Get Started: Add Work times", "#" + step);
                    break;
                case 5:
                    qn.Add("Why do we need to ask for this?", "We need to know your specific set of formats for different fields used in the data.");
                    items.AddRange(new[] { "Date & time formats", "Name format" });
                    manual.Add("Get Started: Custom formats", "#" + step);
                    break;
                //case 5:
                //    qn.Add("Why do we need to ask for this?", "We need to know your different locations where employees work and what partment or teams they belong to. This categorization helps Payall to organize employees data and give you essential information about your employees.");
                //    items.AddRange(new[] { "List of locations", "List of departments", "List of teams", "List of divisions" });
                //    manual.Add("Get Started: Add Organization", "#" + step);
                //    break;
                case 6:
                    qn.Add("Why do we need to ask for this?", "We need to know your what information is being accumulated in the payroll for the employees.");
                    items.AddRange(new[] { "Constant additions", "Variable deductions" });
                    manual.Add("Get Started: Setup Pay components", "#" + step);
                    break;

                case 7: 
                qn.Add("Why do we need to ask for this?", "We need some basic info before we ask your employees to self-onboard. You can enter some or all of your employees on this step before proceeding to the next step");
                items.AddRange(new [] { "Employee full name and email address", "Start date" ,"Salary info", "Work location" });
                manual.Add("Get Started: Add Employees", "#"+ step);
                    break;

                case 8:
                    qn.Add("Why do we need to ask for this?", "We need to know your different locations where employees work and what partment or teams they belong to. This categorization helps Payall to organize employees data and give you essential information about your employees.");
                    items.AddRange(new[] { "Leave or PTO policies", "Paid holidays", "Accrurals" });
                    manual.Add("Get Started: Setup leaves", "#" + step);
                    break;
                case 10:
                    qn.Add("Why do we need to ask for this?", "Payall makes roles simle by assigning users to predefned set of roles which you can choose to enable for the company.");
                    //items.AddRange(new[] { "Leave or PTO policies", "Paid holidays", "Accrurals" });
                    manual.Add("Get Started: Access rights", "#" + step);
                    break;
                case 9:
                    qn.Add("How to pick the perfect plan?", "Gusto makes it easy to find the right plan for your small business.You pay monthly and there are never any hidden fees.You can upgrade, downgrade or cancel at any time.We do the math for you and even file your payroll taxes at no extra cost.");
                    manual.Add("Questions about our plans?", "#");
                    manual.Add("Check out a list of each plan’s features", "#");
                    manual.Add("Learn how to change your plan", "#");
                    break;
            }


            //this.Icon = icon; 
            this.QuestionsAnswers = qn;
            this.ItemsToHaveReady = items;
            this.UserManualLinks = manual;
            //this.BtnName = btnName;
            //this.Title = btnName;
        }

    }
}
