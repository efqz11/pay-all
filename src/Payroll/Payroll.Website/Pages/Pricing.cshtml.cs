using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Payroll.Website.Pages
{
    public class PricingModel : PageModel
    {
        private readonly ILogger<PricingModel> _logger;

        public PricingModel(ILogger<PricingModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
