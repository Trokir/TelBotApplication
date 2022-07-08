namespace TelBotApplication.Clients.Hubs
{
    public static class Strings
    {
        public static string HubUrl => @"https://localhost:5001/hubs/member";

        public static class Events
        {
            public static string MessageSent => nameof(INewMember.SendLog);
            public static string MessageEdited => nameof(INewMember.EditLog);
            public static string PhotoSent => nameof(INewMember.SendPhotoLog);
            public static string DocumentSent => nameof(INewMember.SendDocumentlLog);
            public static string StickerSent => nameof(INewMember.SendStickerLog);
            public static string VideoSent => nameof(INewMember.SendVideoLog);
        }
    }
}
