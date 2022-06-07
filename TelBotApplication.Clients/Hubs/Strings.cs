using TelBotApplication.Clients.Hubs;

namespace TelBotApplication.Clients
{
    public static class Strings
    {
        public static string HubUrl => @"https://localhost:5001/hubs/member";

        public static class Events
        {
            public static string MessageSent => nameof(INewMember.SendLog);

            public static string GetCallBackFromNewMemeber => nameof(INewMember.SendLog);
            
        }
    }
}
