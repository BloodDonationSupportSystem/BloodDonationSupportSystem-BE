using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Shared.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task JoinStaffGroup()
        {
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Admin" || userRole == "Staff")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Staff");
                await Groups.AddToGroupAsync(Context.ConnectionId, "Emergency");
            }
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public async Task LeaveStaffGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Staff");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Emergency");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public override async Task OnConnectedAsync()
        {
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Auto-join groups based on role
            if (userRole == "Admin" || userRole == "Staff")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Staff");
                await Groups.AddToGroupAsync(Context.ConnectionId, "Emergency");
            }

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Groups are automatically cleaned up when user disconnects
            await base.OnDisconnectedAsync(exception);
        }
    }
}