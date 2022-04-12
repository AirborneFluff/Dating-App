using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SingalR
{
    [Authorize]
    public class PressenceHub : Hub
    {
        private readonly PressenceTracker _tracker;
        private readonly IMessageRepository _messageRepository;
        public PressenceHub(PressenceTracker tracker, IMessageRepository messageRepository)
        {
            this._messageRepository = messageRepository;
            this._tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var isOnline = await _tracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
            if (isOnline) await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());

            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);

            var unreadMessageCount = await _messageRepository.GetUnreadMessageCountForUser(Context.User.GetUsername());

            await Clients.Caller.SendAsync("UnreadMessageCount", unreadMessageCount);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _tracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
            if (isOffline) await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());

            await base.OnDisconnectedAsync(exception);
        }
    }
}