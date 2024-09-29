using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class Goal : Audit
    {
        public int Id { get; set; }


        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        public int? LocationId { get; set; }
        public Location Location { get; set; }

        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }


        public GoalType GoalType { get; set; }

        public int GoalOwnerId { get; set; }
        public Employee GoalOwner { get; set; }



        public MeasureUsing MeasureUsing { get; set; }
        public int MeasureUsingValue { get; set; }



        public int? AlignedGoalId { get; set; }
        public virtual Goal AlignedGoal { get; set; }

        public virtual List<Goal> AlignedGoals { get; set; }

        public Goal()
        {
            AlignedGoals = new List<Goal>();
        }

    }
}
