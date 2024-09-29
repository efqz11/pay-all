using Payroll.Controllers;
using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{

    public class DepartmentReportVm
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int EmployeeCount { get; set; }
        public int ManagerEmployeesCount { get; set; }
        public DepartmentHead[] Managers { get; set; }
    }
}
