using System.Threading.Tasks;

namespace TelBotApplication.Clients.Hubs
{
    public interface INewMember
    {
        Task SendLog(string message);
        Task EditLog(string message);
        Task SendPhotoLog(string message);
        Task SendDocumentlLog(string message);
        Task SendStickerLog(string message);
        Task SendVideoLog(string message);
    }

}
