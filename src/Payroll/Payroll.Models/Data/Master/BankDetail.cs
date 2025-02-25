using Payroll.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Models
{
    public class BankDetail : Audit
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int? FileDataId { get; set; }
        public FileData FileData { get; set; }

        [SelectableField]
        [Display(Name = "Name of the Bank")]
        public string BankName { get; set; }
        [SelectableField]
        [Display(Name = "Name of Account holder")]
        public string BankAccountName { get; set; }
        [Display(Name = "Bank Account No.")]
        [SelectableField]
        public string BankAccountNumber { get; set; }

        [SelectableField]
        public string BankCurrency { get; set; }

        [SelectableField]
        [Display(Name = "Bank Address / Branch")]
        public string BankAddress { get; set; }

        [Display(Name = "Swift Code")]
        [SelectableField]
        public string BankSwiftCode { get; set; }

        [Display(Name = "IBAN")]
        [SelectableField]
        public string BankIBAN { get; set; }
        public RecordStatus RecordStatus { get; set; }

    }
}
