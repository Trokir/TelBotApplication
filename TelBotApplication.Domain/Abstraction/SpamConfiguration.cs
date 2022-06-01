
using System;
using System.IO;

namespace TelBotApplication.Domain.Abstraction
{
    public class SpamConfiguration : ISpamConfiguration
    {
        public string SpamTextMessagePath { get => Path.Combine(Environment.CurrentDirectory, "SPAM_text_message.csv"); }
        public string UrlSpamClassification { get => Path.Combine(Environment.CurrentDirectory, "url_spam_classification.csv"); }

    }
}
