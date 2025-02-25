using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Payroll.Model.Logs
{
    public class EventResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string TextColor { get; set; }

    }
}
