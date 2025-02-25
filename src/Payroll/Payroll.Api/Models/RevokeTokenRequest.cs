using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Payroll.Api.Models
{
    public class RevokeTokenRequest
    {
        [DataMember]
        public string Token { get; set; }
    }

}
