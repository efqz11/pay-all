using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Services
{
    public class SignalServer : Hub
    {

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }


        public async Task Send(string userId, string message)
        {
            var _message = message ?? $"Send message to you with user id {userId}";
            await Clients.Client(userId).SendAsync("ReceiveMessage", _message);
        }
    }
}
