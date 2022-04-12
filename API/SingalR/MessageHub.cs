using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SingalR
{
    public class MessageHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly IHubContext<PressenceHub> _pressenceHub;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PressenceTracker _pressenceTacker;
        public MessageHub(IUnitOfWork unitOfWork,
            IMapper mapper,
            IHubContext<PressenceHub> pressenceHub,
            PressenceTracker pressenceTacker)
        {
            this._mapper = mapper;
            this._unitOfWork = unitOfWork;
            this._pressenceHub = pressenceHub;
            this._pressenceTacker = pressenceTacker;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User.GetUsername();
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messagesForConnector = await _unitOfWork.MessageRepository.GetMessageThread(Context.User.GetUsername(), otherUser);
            var messagesForConnected = await _unitOfWork.MessageRepository.GetMessageThread(otherUser, Context.User.GetUsername());

            if (_unitOfWork.HasChanges()) await _unitOfWork.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread", messagesForConnector);
            await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync("ReceiveMessageThread", messagesForConnected);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();
            if (username.ToLower() == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot message yourself");

            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            if (recipient == null) throw new HubException("Not found user");

            if (string.IsNullOrWhiteSpace(createMessageDto.Content)) throw new HubException("No message content sent");

            var newMessage = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                newMessage.DateRead = DateTime.UtcNow;
            } else {
                var connections = await _pressenceTacker.GetConnectionsForUser(recipient.UserName);
                if (connections != null) {
                    await _pressenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                    new {username = sender.UserName, knownAs = sender.KnownAs});
                }
            }
            _unitOfWork.MessageRepository.AddMessage(newMessage);
            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(newMessage));
            }
        }

        private async Task<Group> AddToGroup(string groupName) 
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _unitOfWork.Complete()) return group;

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _unitOfWork.MessageRepository.RemoveConnection(connection);

            if (await _unitOfWork.Complete()) return group;

            throw new HubException("Failed to remove from group");
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}