
using System;
using System.IO;
using TelBotApplication.Domain.Interfaces;

namespace TelBotApplication.Domain.ML
{
    public class SpamConfiguration : ISpamConfiguration
    {
        public string SpamTextMessagePath { get => Path.Combine(Environment.CurrentDirectory, "SPAM_text_message.csv"); }
        public string UrlSpamClassification { get => Path.Combine(Environment.CurrentDirectory, "url_spam_classification.csv"); }

    }
}
