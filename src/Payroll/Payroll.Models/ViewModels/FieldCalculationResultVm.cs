using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{

    public class FieldCalculationResultVm
    {
        public FieldCalculationResultVm()
        {

        }
        public FieldCalculationResultVm(string fieldName, string type)
        {
            //ListType = type;
            FieldType = type;
            FieldName = fieldName;
        }

        //public ListType ListType { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
    }
}
