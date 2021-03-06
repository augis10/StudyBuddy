﻿using System;
using System.Collections.Generic;
using System.Text;

using StudyBuddyShared.Entity;
using static StudyBuddyShared.Network.MessagePoster; // Fix this maybe

namespace StudyBuddyShared.ConversationSystem
{
    public enum MessageSendStatus
    {
        InvalidPrivateKey,
        Success,
        JsonReadException,
        FailedToConnect,
        UnknownError,
        FieldsMissing,
        ConversationNotFound,
        MessageTooLong,
        MessageEmpty
    }

    public delegate void MessagePostDelegate(MessageSendStatus status, Message message);

    public interface IMessageSender : IPrivateKey
    {
        Queue<Message> Queue { get; set; } // Message queue

        bool RetryOnFail { get; set; }  // Should try to resend message if failed
        
        int RetryInterval { get; set; } // How long should it wait before retrying Miliseconds

        bool Sending { get; } // Is active

        bool StartSending(); // Start sending

        bool StopSending();  // Stop sending

        MessagePostDelegate Result { get; set; } // Result delegate

        void AddMessageToQueue(Message message); // Add to queue

    }
}
