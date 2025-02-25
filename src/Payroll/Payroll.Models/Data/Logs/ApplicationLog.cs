using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Payroll.Model.Logs
{
    public class ApplicationLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Message { get; set; }

        public string MessageTemplate { get; set; }

        [MaxLength(128)]
        public string Level { get; set; }

        [Required]
        public DateTime TimeStamp { get; set; }

        public string Exception { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Environment { get; set; }


        public string Properties { get; set; }
    }
}
