using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{

    /// <summary>
    /// Actions can be basically anything, that communicates with employer with a contract (comapny) with employee
    /// </summary>
    public class Reminder : Audit
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeRoleId { get; set; }
        public EmployeeRole EmployeeRole { get; set; }

        [NotMapped]
        public String _Role { get; set; }

        public RemindAbout About { get; set; }
        public int When { get; set; }
        public RemindIn In { get; set; }
        public EmployeeSelectorReminder Of { get; set; }
        public RemindBeforeAfter BeforeOrAfter { get; set; }

        public RemindAction RemindAction { get; set; }

        //public PayFrequency RemindAction { get; set; }

        
        public string Note { get; set; }



        public Reminder()
        {
        }
    }
}
