using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Payroll.Models
{
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }                    /*Log id*/
        public DateTime AuditDateTimeUtc { get; set; }  /*Log time*/

        public AuditType AuditType { get; set; }
        public AuditAction AuditAction { get; set; }    /*Create, Update or Delete*/

        public string AuditUser { get; set; }           /*Log User*/
        public string AuditUserId { get; set; }
        public string AuditUserRoles { get; set; }

        public string ContextName { get; set; }
        public string FullContextName { get; set; }
        public string ModelName { get; set; }
        public string FullModelName { get; set; }
        public string TableName { get; set; }           /*Table where rows been 
                                                          created/updated/deleted*/
        public string KeyId { get; set; }           /*Table Pk value*/
        public string KeyValues { get; set; }           /*Table Pk and it's values*/
        public string OldValues { get; set; }           /*Changed column name and old value*/
        public string NewValues { get; set; }           /*Changed column name 
                                                          and current value*/

        public string ChangedColumns { get; set; }      /*Changed column names*/


        public string Status { get; set; }
        public string Message { get; set; }

        public int CompanyId { get; set; }
        public int JobId { get; set; }
        public int EmployeeId { get; set; }
    }

}
