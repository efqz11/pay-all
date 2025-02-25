using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{
    public class SearchResultVm
    {
        public string ResultType { get; set; }
        public List<SearchResultItemVm> Result { get; set; }
        public string SearchTerm { get;  set; }
    }

    public class SearchResultItemVm
    {
        public string Item1 { get; set; }
        public string Item2 { get; set; }
        public string Link { get; set; }
        public string Icon { get; set; }
        public string Item3 { get;  set; }
    }
}
