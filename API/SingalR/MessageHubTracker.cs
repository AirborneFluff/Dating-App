using Microsoft.AspNetCore.SignalR;

namespace API.SingalR
{
    public class MessageHubTracker
    {        
        private static readonly Dictionary<string, List<string>> UserGroups =
            new Dictionary<string, List<string>>();

        public Task UserConnected(string groupName, string username)
        {
            lock(UserGroups)
            {
                if (UserGroups.ContainsKey(groupName))
                {
                    UserGroups[groupName].Add(username);
                } else
                {
                    UserGroups.Add(groupName, new List<string>{username});
                }
            }

            return Task.CompletedTask;
        }

        public Task UserDisconnected(string groupName, string username)
        {
            lock(UserGroups)
            {
                if (!UserGroups.ContainsKey(groupName)) return Task.CompletedTask;

                UserGroups[groupName].Remove(username);
                if (UserGroups[groupName].Count == 0)
                    UserGroups.Remove(groupName);
            }

            return Task.CompletedTask;
        }

        public Task<string[]> GetOnlineUsernames()
        {
            string[] onlineUsers;
            lock(UserGroups) {
                onlineUsers = UserGroups.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
            }

            return Task.FromResult(onlineUsers);
        }
        
    }
}