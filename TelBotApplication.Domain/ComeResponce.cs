using Newtonsoft.Json;


namespace TelBotApplication.Domain
{

    public class ComingResponce
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }
        [JsonProperty("result")]
        public Result Result { get; set; }
    }
    [JsonObject(Title = "Result")]
    public class Result
    {
        [JsonProperty("message_id")]
        public int MessageId { get; set; }
        [JsonProperty("from")]
        public From From { get; set; }
        [JsonProperty("chat")]
        public Chat Chat { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("reply_markup")]
        public ReplyMarkup ReplyMarkup { get; set; }
    }
    [JsonObject(Title = "From")]
    public class From
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("is_bot")]
        public bool IsBot { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }
    }
    [JsonObject(Title = "Chat")]
    public class Chat
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    [JsonObject(Title = "Reply_Markup")]
    public class ReplyMarkup
    {
        [JsonProperty("inline_keyboard")]
        public InlineKeyboard[][] InlineKeyboard { get; set; }
    }
    [JsonObject(Title = "Inline_Keyboard")]
    public class InlineKeyboard
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("callback_data")]
        public string CallbackData { get; set; }
    }

}
