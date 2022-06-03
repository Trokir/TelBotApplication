using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TelBotApplication.DAL.Interfaces;

namespace TelBotApplication.DAL.Services
{
    public class TextFilter : ITextFilter
    {
        private readonly HashSet<string> IfIntrestedWas = new HashSet<string>
        {
            "что-нибудь",
            "сообщений",
            "новое?",
            "интересное",
            "что-то",
            "что",
            "было",
            "что-нибудь",
            "сообщений",
            "новое",
            "да",
            "да.",
            "прочти",
            "сообщения",
            "расскажите",
            "кратко",
            "пж",
            "плиз"

        };
        public bool IsAlertFrase(string text)
        {

            string[] arr = text.Split(new[] { ' ', ',', '.', ',', '.', ' ', '?' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arr.Length; i++)
            {
            arr[i] = Regex.Replace(arr[i], @"\s+", " ",RegexOptions.Multiline).Trim();
            }
            if (arr.Any())
            {
                HashSet<string> arrr = arr.Intersect(IfIntrestedWas).ToHashSet();
                if (arrr.Any() && arrr.Count > 3)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
