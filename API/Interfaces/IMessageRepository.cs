using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection> GetConnection(string connectionId);
        Task<Group> GetMessageGroup(string groupName);
        Task<Group> GetGroupForConnection(string connectionId);
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        void DeleteMessages(Message[] messages);
        Task<Message> GetMessage(int id);
        Task<List<Message>> GetAllMessagesFromUser(string username);
        Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
        Task<int> GetUnreadMessageCountForUser(string username);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername);
        Task<bool> SaveAllAsync();
    }
}