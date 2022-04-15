using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public void DeleteMessages(Message[] messages)
        {
            _context.Messages.RemoveRange(messages);
        }

        public Task<List<Message>> GetAllMessagesFromUser(string username)
        {
            return _context.Messages.Where(msg => msg.SenderUsername.ToUpper() == username.ToUpper()).ToListAsync();
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups.Include(c => c.Connections)
                .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .Include(u => u.Sender)
                .Include(u => u.Recipient)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(x => x.Connections).FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username && !u.RecipientDeleted),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && !u.SenderDeleted),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username && u.DateRead == null && !u.RecipientDeleted)
            };

            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messages
                .Where(
                    m =>    (m.Recipient.UserName.ToLower() == currentUsername.ToLower() && !m.RecipientDeleted
                            && m.Sender.UserName.ToLower() == recipientUsername.ToLower()) ||
                            (m.Recipient.UserName.ToLower() == recipientUsername.ToLower() && !m.SenderDeleted
                            && m.Sender.UserName.ToLower() == currentUsername.ToLower()))
                .OrderBy(x => x.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername.ToLower() == currentUsername.ToLower()).ToList();
            if (unreadMessages.Any())
                unreadMessages.ForEach(m => m.DateRead = DateTime.UtcNow);

            return messages;

        }

        public Task<int> GetUnreadMessageCountForUser(string username)
        {
            return _context.Messages.CountAsync(x => x.RecipientUsername.ToUpper() == username.ToUpper() && x.DateRead == null);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}