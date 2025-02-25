using Payroll.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace Payroll.ViewModels
{
    public class MapUserToEmployeeVm
    {
        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public string UserId { get; set; }
        public bool HasMapping { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public AppUser AppUser { get; set; }
        public bool CreateNewUser { get; set; }
        public int ContractId { get; set; }
        public bool IsAlreadyMapped { get; internal set; }
        public bool ChangePasswordOnLogin { get;  set; }
        public bool SendOtpAndLoginFirst { get;  set; }

        public MapUserToEmployeeVm()
        {
        }
    }
}
