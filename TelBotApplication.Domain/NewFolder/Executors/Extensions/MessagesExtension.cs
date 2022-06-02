using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelBotApplication.Domain.NewFolder.Executors.Extensions
{
    public static class MessagesExtension
    {

        public static async Task SendTextMessageWhithDelayAsync(this ITelegramBotClient botClient,bool isEnabled, Message message,
            ChatId chatId,
            string text,
            TimeSpan delay,
            ParseMode? parseMode = default,
            IEnumerable<MessageEntity>? entities = default,
            bool? disableWebPagePreview = default,
            bool? disableNotification = default,
            int? replyToMessageId = default,
            bool? allowSendingWithoutReply = default,
            IReplyMarkup? replyMarkup = default,
            CancellationToken cancellationToken = default)
        {
            if (!isEnabled)
            {
                return;
            }
            _ = await Task.Factory.StartNew(async () =>
            {
                var result = await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: text, parseMode: parseMode, entities: entities, disableWebPagePreview: disableWebPagePreview,
               disableNotification: disableNotification, replyToMessageId: replyToMessageId, allowSendingWithoutReply: allowSendingWithoutReply, replyMarkup: replyMarkup, cancellationToken: cancellationToken);

                await Task.Delay(delay, cancellationToken);
                await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
            }, cancellationToken).ConfigureAwait(false);
        }
        public static async Task SendAnimationWhithDelayAsync(
            this ITelegramBotClient botClient,bool isEnabled, Message message,
            ChatId chatId,
            InputOnlineFile animation,
            TimeSpan delay,
            int? duration = default,
            int? width = default,
            int? height = default,
            InputMedia? thumb = default,
            string? caption = default,
            ParseMode? parseMode = default,
            IEnumerable<MessageEntity>? captionEntities = default,
            bool? disableNotification = default,
            int? replyToMessageId = default,
            bool? allowSendingWithoutReply = default,
            IReplyMarkup? replyMarkup = default,
            CancellationToken cancellationToken = default)
        {
            if (!isEnabled)
            {
                return;
            }
            _ = await Task.Factory.StartNew(async () =>
            {
                var result = await botClient.SendAnimationAsync(chatId: message.Chat.Id,
                    animation: animation, duration: duration, width: width, height: height,
                    thumb: thumb, caption: caption, parseMode: parseMode, captionEntities: captionEntities, disableNotification: disableNotification,
                    replyToMessageId: replyToMessageId, allowSendingWithoutReply: allowSendingWithoutReply, replyMarkup: replyMarkup, cancellationToken: cancellationToken);

                await Task.Delay(delay, cancellationToken);
                await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static async Task SendPhotoWhithDelayAsync(
            this ITelegramBotClient botClient,
             TimeSpan delay,
            ChatId chatId,
            InputOnlineFile photo,
            bool isEnabled,
            string? caption = default,
            ParseMode? parseMode = default,
            IEnumerable<MessageEntity>? captionEntities = default,
            bool? disableNotification = default,
            int? replyToMessageId = default,
            bool? allowSendingWithoutReply = default,
            IReplyMarkup? replyMarkup = default,
            CancellationToken cancellationToken = default)
        {
            if (!isEnabled)
            {
                return ;
            }
            _ = await Task.Factory.StartNew(async () =>
            {
                var result = await botClient.SendPhotoAsync(chatId: chatId,
                   photo:photo, caption: caption, parseMode: parseMode, captionEntities: captionEntities, disableNotification: disableNotification,
                    replyToMessageId: replyToMessageId, allowSendingWithoutReply: allowSendingWithoutReply, replyMarkup: replyMarkup, cancellationToken: cancellationToken);

                await Task.Delay(delay, cancellationToken);
                await botClient.DeleteMessageAsync(result.Chat.Id, result.MessageId, cancellationToken);
            }, cancellationToken).ConfigureAwait(false);
        }
    }

}
#region enum
//switch (Text)
      //      {
      //          case MessageType.Unknown:
      //              break;
      //          case MessageType.Text:
      //              break;
      //          case MessageType.Photo:
      //              break;
      //          case MessageType.Audio:
      //              break;
      //          case MessageType.Video:
      //              break;
      //          case MessageType.Voice:
      //              break;
      //          case MessageType.Document:
      //              break;
      //          case MessageType.Sticker:
      //              break;
      //          case MessageType.Location:
      //              break;
      //          case MessageType.Contact:
      //              break;
      //          case MessageType.Venue:
      //              break;
      //          case MessageType.Game:
      //              break;
      //          case MessageType.VideoNote:
      //              break;
      //          case MessageType.Invoice:
      //              break;
      //          case MessageType.SuccessfulPayment:
      //              break;
      //          case MessageType.WebsiteConnected:
      //              break;
      //          case MessageType.ChatMembersAdded:
      //              break;
      //          case MessageType.ChatMemberLeft:
      //              break;
      //          case MessageType.ChatTitleChanged:
      //              break;
      //          case MessageType.ChatPhotoChanged:
      //              break;
      //          case MessageType.MessagePinned:
      //              break;
      //          case MessageType.ChatPhotoDeleted:
      //              break;
      //          case MessageType.GroupCreated:
      //              break;
      //          case MessageType.SupergroupCreated:
      //              break;
      //          case MessageType.ChannelCreated:
      //              break;
      //          case MessageType.MigratedToSupergroup:
      //              break;
      //          case MessageType.MigratedFromGroup:
      //              break;
      //          case MessageType.Poll:
      //              break;
      //          case MessageType.Dice:
      //              break;
      //          case MessageType.MessageAutoDeleteTimerChanged:
      //              break;
      //          case MessageType.ProximityAlertTriggered:
      //              break;
      //          case MessageType.VoiceChatScheduled:
      //              break;
      //          case MessageType.VoiceChatStarted:
      //              break;
      //          case MessageType.VoiceChatEnded:
      //              break;
      //          case MessageType.VoiceChatParticipantsInvited:
      //              break;
      //          default:
      //              break;
      //      }
#endregion

