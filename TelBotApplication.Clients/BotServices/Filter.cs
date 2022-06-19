using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelBotApplication.DAL;
using TelBotApplication.DAL.Interfaces;
using TelBotApplication.Domain.Models;

namespace TelBotApplication.Clients.BotServices
{
    public class Filter : IFilter
    {
        private readonly ILogger<Filter> _logger;
        private readonly IServiceProvider _factory;
        private HashSet<TextFilter> _filters = default;
        private const int MinWordLength = 3;
        private const double ThresholdSentence = 0.35d;
        private const double ThresholdWord = 0.40d;
        private const int SubtokenLength = 2;
        public Filter(IServiceProvider factory, ILogger<Filter> logger)
        {
            _factory = factory;
            _logger = logger;
        }
        public async Task UpdateFilters()
        {
            using IServiceScope scope = _factory.CreateScope();
            while (true)
            {

                var context =
                    scope.ServiceProvider
                        .GetRequiredService<IUnitOfWork>();

                _filters = (await context.TextFilterService.GetAllAsync()).ToHashSet();


                await Task.Delay(20000);
            }
        }

        public string FindAnswerForAlertFrase(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            var normalText = NormalizeSentence(text);
            var arr = GetTokens(normalText);


            foreach (var filter in _filters)
            {
                var filterText = filter.Text;
                var filterFilter = filter.Filter;
                switch (filterFilter)
                {
                    case Domain.Enums.TypeOfFilter.Strong:
                        foreach (var word in arr)
                        {
                            if (String.Compare(word, filterText, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase) == 0)
                            {
                                return filter.Comment;
                            }
                        }
                        break;
                    case Domain.Enums.TypeOfFilter.Easy:
                        var result = CalculateFuzzyEqualValue(filterText, text);
                        if (result > ThresholdSentence)
                        {
                            return filter.Comment;
                        }
                        break;
                }
            }
            return string.Empty;
        }

        private string NormalizeSentence(string sentence)
        {
            var resultContainer = new StringBuilder(100);
            var lowerSentece = sentence.ToLower();
            foreach (var c in lowerSentece)
            {
                if (IsNormalChar(c))
                {
                    resultContainer.Append(c);
                }
            }

            return resultContainer.ToString();
        }

        /// <summary>
        /// Разбивает предложение на слова. 
        /// </summary>
        /// <param name="sentence">Предложение.</param>
        /// <returns>Набор слов.</returns>
        private string[] GetTokens(string sentence)
        {
            var tokens = new List<string>();
            char[] delimiterChars = { ' ', ',', '.', ':', ')', '(', '\t' };
            var words = sentence.Split(delimiterChars);
            foreach (var word in words)
            {
                if (word.Length >= MinWordLength)
                {
                    tokens.Add(word);
                }
            }

            return tokens.ToArray();
        }
        /// <summary>
        /// Возвращает признак подходящего символа.
        /// </summary>
        /// <param name="c">Символ.</param>
        /// <returns>True - если символ буква или цифра или пробел, False - иначе.</returns>
        private bool IsNormalChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == ' ';
        }


        private bool IsTokensFuzzyEqual(string firstToken, string secondToken)
        {
            var equalSubtokensCount = 0;
            var usedTokens = new bool[secondToken.Length - SubtokenLength + 1];
            for (var i = 0; i < firstToken.Length - SubtokenLength + 1; ++i)
            {
                var subtokenFirst = firstToken.Substring(i, SubtokenLength);
                for (var j = 0; j < secondToken.Length - SubtokenLength + 1; ++j)
                {
                    if (!usedTokens[j])
                    {
                        var subtokenSecond = secondToken.Substring(j, SubtokenLength);
                        if (subtokenFirst.Equals(subtokenSecond))
                        {
                            equalSubtokensCount++;
                            usedTokens[j] = true;
                            break;
                        }
                    }
                }
            }
            var subtokenFirstCount = firstToken.Length - SubtokenLength + 1;
            var subtokenSecondCount = secondToken.Length - SubtokenLength + 1;

            var tanimoto = (1.0 * equalSubtokensCount) / (subtokenFirstCount + subtokenSecondCount - equalSubtokensCount);

            return ThresholdWord <= tanimoto;
        }


        /// <summary>
        /// Вычисляет значение нечеткого сравнения предложений.
        /// </summary>
        /// <param name="first">Первое предложение.</param>
        /// <param name="second">Второе предложение.</param>
        /// <returns>Результат нечеткого сравнения предложений.</returns>
        public double CalculateFuzzyEqualValue(string first, string second)
        {
            if (string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(second))
            {
                return 1.0;
            }

            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second))
            {
                return 0.0;
            }

            var normalizedFirst = NormalizeSentence(first);
            var normalizedSecond = NormalizeSentence(second);

            var tokensFirst = GetTokens(normalizedFirst);
            var tokensSecond = GetTokens(normalizedSecond);

            var fuzzyEqualsTokens = GetFuzzyEqualsTokens(tokensFirst, tokensSecond);

            var equalsCount = fuzzyEqualsTokens.Length;
            var firstCount = tokensFirst.Length;
            var secondCount = tokensSecond.Length;

            var resultValue = (1.0 * equalsCount) / (firstCount + secondCount - equalsCount);

            return resultValue;
        }

        /// <summary>
        /// Возвращает эквивалентные слова из двух наборов.
        /// </summary>
        /// <param name="tokensFirst">Слова из первого предложения.</param>
        /// <param name="tokensSecond">Слова из второго набора предложений.</param>
        /// <returns>Набор эквивалентных слов.</returns>
        private string[] GetFuzzyEqualsTokens(string[] tokensFirst, string[] tokensSecond)
        {
            var equalsTokens = new List<string>();
            var usedToken = new bool[tokensSecond.Length];
            for (var i = 0; i < tokensFirst.Length; ++i)
            {
                for (var j = 0; j < tokensSecond.Length; ++j)
                {
                    if (!usedToken[j])
                    {
                        if (IsTokensFuzzyEqual(tokensFirst[i], tokensSecond[j]))
                        {
                            equalsTokens.Add(tokensFirst[i]);
                            usedToken[j] = true;
                            break;
                        }
                    }
                }
            }

            return equalsTokens.ToArray();
        }
    }
}
