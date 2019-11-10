﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using StudyBuddy.Entity;
using StudyBuddyApp.Services;
using StudyBuddyShared.ConversationSystems;
using Xamarin.Forms;
using Message = StudyBuddy.Entity.Message;
using StudyBuddyShared.Network;
using StudyBuddyApp.Droid.Utility;
using StudyBuddyApp.Utility;

namespace StudyBuddyApp.Droid.Services
{
    [Service]
    class MessagingService : Service
    {
        IMessageGetter messageGetter = null;
        IMessageSender messageSender = null;

        Dictionary<string, User> users = new Dictionary<string, User>();
        Dictionary<int, List<Message>> messages = new Dictionary<int, List<Message>>();
        List<Conversation> conversations = new List<Conversation>();

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if(messageGetter != null)
            {

                return StartCommandResult.Sticky;
            }
            if(LocalUserManager.LocalUser == null)
            {
                if (Xamarin.Forms.Application.Current.Properties.ContainsKey("PrivateKey"))
                {
                    LocalUserManager.LocalUser = new LocalUser { PrivateKey = (string)Xamarin.Forms.Application.Current.Properties["PrivateKey"] };
                }
                else
                {
                    StopSelf();
                }
            }
            messageGetter = ConversationSystemManager.NewMessageGetter();
            messageSender = ConversationSystemManager.NewMessageSender();
            messageGetter.GetMessageResult += (status, conversations, messages, users) => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    //DependencyService.Get<IToast>().ShortToast(status.ToString());
                });
                if (status == AllMessageGetter.MessageStatus.Success && conversations.Count != 0)
                {
                    this.conversations = conversations
                        .Union(this.conversations, new EntityComparer())
                        .OrderByDescending(conv => conv.LastActivity)
                        .ToList();
                    foreach (KeyValuePair<int, List<Message>> entry in messages)
                    {
                        if (this.messages.ContainsKey(entry.Key))
                        {
                            this.messages[entry.Key].AddRange(entry.Value);
                        }
                        else
                        {
                            this.messages[entry.Key] = entry.Value;
                        }
                    }
                    this.users = users
                        .Concat(this.users.Where(pair => !users.ContainsKey(pair.Key)))
                        .ToDictionary(x => x.Key, x => x.Value);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(new MessagingTask(), MessagingTask.Messages, new Tuple<Dictionary<int, List<Message>>, Dictionary<string, User>>(messages, this.users));

                    });
                }
                else if (status == AllMessageGetter.MessageStatus.InvalidPrivateKey)
                {
                    if (LocalUserManager.LocalUser != null)
                    {
                        messageGetter.PrivateKey = LocalUserManager.LocalUser.PrivateKey;
                    }
                    else
                    {
                        messageGetter.StopGetting();
                        messageSender.StopSending();
                        StopSelf();
                    }
                } 
            };
            MessagingCenter.Subscribe<MessagingTask>(this, MessagingTask.GetMessages, (task) =>
            {
                MessagingCenter.Send(new MessagingTask(), MessagingTask.LocalMessages, new Tuple<Dictionary<int, List<Message>>, Dictionary<string, User>>(messages, users));
            });

            MessagingCenter.Subscribe<MessagingTask>(this, MessagingTask.GetConversations, (task) =>
            {
                MessagingCenter.Send(new MessagingTask(), MessagingTask.LocalConversations, new Tuple<List<Conversation>, Dictionary<string, User>>(conversations, users));
            });

            messageSender.PostMessageResult += (status, message) =>
            {
                if (status == StudyBuddy.Network.MessagePoster.MessageStatus.InvalidPrivateKey)
                {
                    if (LocalUserManager.LocalUser != null)
                    {
                        messageGetter.PrivateKey = LocalUserManager.LocalUser.PrivateKey;
                    }
                    else
                    {
                        messageGetter.StopGetting();
                        messageSender.StopSending();
                        StopSelf();
                    }
                }
            };


            MessagingCenter.Subscribe<MessagingTask, Message>(this, MessagingTask.AddMessageToQueue, (task, message) =>
            {
                messageSender.AddMessageToQueue(message);
            });


            MessagingCenter.Subscribe<MessagingTask>(this, MessagingTask.Stop, (task) =>
            {
                messageGetter.StopGetting();
                messageSender.StopSending();
                StopSelf();
            });

            messageGetter.GetUsers = true;
            messageGetter.StartGetting();

            messageSender.StartSending();
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send(new MessagingTask(), MessagingTask.Started);
            });
            Console.WriteLine("SERVICE STARTED");
            return StartCommandResult.Sticky;
        }
    }

    public class EntityComparer : IEqualityComparer<Conversation>
    {
        public bool Equals(Conversation x, Conversation y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Conversation obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}