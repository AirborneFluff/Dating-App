using Microsoft.AspNetCore.SignalR;

namespace API.Extensions
{
    public static class HubClientProxyExtensions
    {
        public static string TestExtension (this IClientProxy proxy)
        {
            return "new string()";
        }
    }
}