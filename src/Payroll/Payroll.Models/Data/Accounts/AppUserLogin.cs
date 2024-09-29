using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class AppUserLogin : Audit
    {
        public int Id { get; set; }

        [ForeignKey("AppUser")]
        public string UserId { get; set; }
        public virtual AppUser AppUser { get; set; }

        public DateTime LoggedDate { get; set; }

        public DateTime LoggedOutDate { get; set; }

        public string UserAgent { get; set; }

        public string UserPlatform { get; set; }
        public string DeviceIcon { get; set; }
        public string PlatformIcon { get; set; }

        public string IPAdress { get; set; }


        public bool IsDeviceReviwed { get; set; }
    }
}
