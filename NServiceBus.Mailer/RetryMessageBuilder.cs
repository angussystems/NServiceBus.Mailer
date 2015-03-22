﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace NServiceBus.Mailer
{
    static class RetryMessageBuilder
    {
        public static IEnumerable<MailMessage> GetMessagesToRetry(MailMessage originalMessage, DateTime originalTimeSent, SmtpFailedRecipientsException exception)
        {
            var newBody = GetForwardBody(originalMessage, originalTimeSent);
            return exception.InnerExceptions
                .Select(x => x.FailedRecipient)
                .Select(failedRecipient => ReSend(failedRecipient, newBody, originalMessage));
        }

        static MailMessage ReSend(string failedRecipient, string newBody, MailMessage originalMessage)
        {
            return new MailMessage
                   {
                       To = new List<string> { failedRecipient },
                       From = originalMessage.From,
                       Body = newBody,
                       BodyEncoding = originalMessage.BodyEncoding,
                       DeliveryNotificationOptions = originalMessage.DeliveryNotificationOptions,
                       Headers = originalMessage.Headers,
                       HeadersEncoding = originalMessage.HeadersEncoding,
                       IsBodyHtml = originalMessage.IsBodyHtml,
                       Priority = originalMessage.Priority,
                       ReplyTo = originalMessage.ReplyTo,
                       Sender = originalMessage.Sender,
                       Subject = originalMessage.Subject,
                       SubjectEncoding = originalMessage.SubjectEncoding,
                       AttachmentContext = originalMessage.AttachmentContext,
                       AlternateViews = originalMessage.AlternateViews,
                   };
        }

        static string GetForwardBody(MailMessage original, DateTime timesent)
        {
            if (original.IsBodyHtml)
            {
                return GetHtmlPrefix(original, timesent);
            }
            return GetTextPrefix(original, timesent);
        }

        static string GetTextPrefix(MailMessage original, DateTime timesent)
        {
            return string.Format(
                @"
This message was forwarded due to the original email failing to send
-----Original Message-----
To: {0}
CC: {1}
Sent: {2}

{3}
",
                string.Join(",", original.To),
                string.Join(",", original.Cc),
                timesent.ToString("R"),
                original.Body);
        }

        static string GetHtmlPrefix(MailMessage original, DateTime timesent)
        {
            return string.Format(
                @"
This message was forwarded due to the original email failing to send<br/>
-----Original Message-----<br/>
To: {0}<br/>
CC: {1}<br/>
Sent: {2}<br/><br/>
{3}
",
                string.Join(",", original.To),
                string.Join(",", original.Cc),
                timesent.ToString("R"),
                original.Body);
        }
    }
}