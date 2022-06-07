using NServiceBus;


namespace TelBotApplication.DAL.Messages
{
    public class LogMessage : ICommand
    {

        public string ChatMessage { get; set; }

    }
}
