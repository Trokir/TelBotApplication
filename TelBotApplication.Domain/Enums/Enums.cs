namespace TelBotApplication.Domain.Enums
{
    public enum TypeOfreactions
    {
        Text = 1,
        Animation = 2,
        Venue = 3,
        Photo = 4
    }
    public enum TypeOfMessage
    {
        Spam = 1,
        Ham = 2
    }
    public enum TypeOfFilter
    {
        Strong = 1,
        Easy = 2
    }
    public enum TypeOfMessageLog
    {
        Added,
        Edited,
        Document,
        Video,
        Sticker,
        Photo
    }
    public enum AnchorCallBack
    {
        /// <summary>
        /// Ссылка
        /// </summary>
        Link = 0,
        /// <summary>
        /// Реакция
        /// </summary>
        Reaction = 1,
        /// <summary>
        /// Поделиться
        /// </summary>
        Share = 2,
        /// <summary>
        /// Вызвать триггер
        /// </summary>
        CallTrigger = 3,
        /// <summary>
        /// None
        /// </summary>
        None = 4
    }
    public enum AnchorAction
    {
        DropPrevious,
        ChangeCurrent,
        Add,
        None
    }

}
