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
using StudyBuddyShared.Entity;
using StudyBuddyApp.Services;
using StudyBuddyShared.ConversationSystem;
using Xamarin.Forms;
using Message = StudyBuddyShared.Entity.Message;
using StudyBuddyShared.Network;
using StudyBuddyApp.Droid.Utility;
using StudyBuddyApp.Utility;
using StudyBuddyShared.SystemManager;
using StudyBuddyApp.EntityFramework;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using StudyBuddyShared.UserSystem;
using Microsoft.Data.Sqlite;

namespace StudyBuddyApp.Droid.Services
{
    [Service(Enabled = true)]
    public class MessagingService : Service
    {
        public const string SERVICE_KILLED_MESSAGE = "MessagingServiceKilled";
        IMessageGetter messageGetter = null;
        IMessageSender messageSender = null;
        long notificationTimestamp = 0;
        Dictionary<string, User> users = new Dictionary<string, User>();
        Dictionary<int, List<Message>> messages = new Dictionary<int, List<Message>>();
        List<Conversation> conversations = new List<Conversation>();
        DatabaseContext db;
        bool stoppedSelf = false;

        public void Stop()
        {
            stoppedSelf = true;
            StopSelf();
        }

        public void LoadData() // Loading data from the database
        {
            db = new DatabaseContext();
            db.Database.EnsureCreated();
            try
            {
                db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                users = db.Users.ToDictionary((user) => user.Username, (user) => user);
                conversations = db.Conversations.ToList();
                messages = db.Conversations.ToDictionary(conv => conv.Id, conv => db.Messages.Where(msg => msg.Conversation == conv.Id).ToList());
            }
            catch(Exception e)
            {
                users = new Dictionary<string, User>();
                messages = new Dictionary<int, List<Message>>();
                conversations = new List<Conversation>();
            }
            db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToast>().ShortToast("starting service");
            });
            if (messageGetter != null)
            {

                return StartCommandResult.Sticky;
            }
            if (App.Current.Properties.ContainsKey("notificationTimestamp"))
            {
                notificationTimestamp = (long)App.Current.Properties["notificationTimestamp"];
            }
            if (LocalUserManager.LocalUser == null)
            {
                if (Xamarin.Forms.Application.Current.Properties.ContainsKey("PrivateKey"))
                {
                    LocalUserManager.LocalUser = new LocalUser { PrivateKey = (string)Xamarin.Forms.Application.Current.Properties["PrivateKey"] };
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DependencyService.Get<IToast>().ShortToast("no private key");
                    });
                    Stop();
                }
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToast>().ShortToast("started service");
            });
            messageGetter = ConversationSystemManager.NewMessageGetter();
            messageSender = ConversationSystemManager.NewMessageSender();
            //Message getter result 
            messageGetter.Result += (status, conversations, messages, users) => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    //DependencyService.Get<IToast>().ShortToast(status.ToString());
                });
                //Got messages
                if (status == MessageGetAllStatus.Success && conversations.Count != 0)
                {
                    //joining conversations
                    this.conversations = conversations
                        .Union(this.conversations, new EntityComparer())
                        .OrderByDescending(conv => conv.LastActivity)
                        .ToList();
                    //Joining messages
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
                    //Joining users
                    this.users = users
                        .Concat(this.users.Where(pair => !users.ContainsKey(pair.Key)))
                        .ToDictionary(x => x.Key, x => x.Value);
                    //Sending messages back to ui
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send(new MessagingTask(messagesDict: messages, users: this.users), MessagingTask.NewMessages);
                        DependencyService.Get<IToast>().ShortToast("New message");
                    });

                    //Saving values
                    try
                    {
                        foreach (var entry in users)
                        {
                            if (db.Users.Any(user => user.Username == entry.Value.Username))
                            {
                                db.Users.Update(entry.Value);
                            }
                            else
                            {
                                db.Users.Add(entry.Value);
                            }
                        }
                        foreach (var entry in conversations)
                        {
                            if (db.Conversations.Any(conv => conv.Id == entry.Id))
                            {
                                db.Conversations.Update(entry);
                            }
                            else
                            {
                                db.Conversations.Add(entry);
                            }
                        }
                        foreach (var entry in messages)
                        {
                            foreach (var message in entry.Value)
                            {
                                if (db.Messages.Any(_message => _message.Id == message.Id))
                                {
                                    db.Messages.Update(message);
                                }
                                else
                                {
                                    db.Messages.Add(message);
                                }
                            }
                        }

                        db.SaveChanges();
                        //Untracking values
                        foreach (var entry in users)
                        {
                            db.Entry(entry.Value).State = EntityState.Detached;
                        }
                        foreach (var entry in conversations)
                        {
                            db.Entry(entry).State = EntityState.Detached;
                        }
                        foreach (var entry in messages)
                        {
                            foreach (var message in entry.Value)
                            {
                                db.Entry(message).State = EntityState.Detached;
                            }
                        }
                    }
                    catch (SqliteException e)
                    {
                        Console.WriteLine("Table not found, the user might have logged off mid writing");
                    }
                    //Creating notifications
                    long temp = notificationTimestamp;
                    conversations.ForEach((conversation) =>
                    {
                        Message message = messages[conversation.Id].FindLast(m => users.ContainsKey(m.Username) && m.Timestamp > notificationTimestamp);
                        if(message != null)
                        {
                            if (notificationTimestamp != -1)
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    DependencyService.Get<INotification>().DisplayMessageNotification(conversation, message, users);
                                });
                            }
                            temp = Math.Max(message.Timestamp, temp);
                        }
                    });
                    //New notification timestamp limit
                    if(temp != notificationTimestamp)
                    {
                        notificationTimestamp = temp;
                        App.Current.Properties["notificationTimestamp"] = notificationTimestamp;
                        App.Current.SavePropertiesAsync();
                    }
                }
                //Failed to get stopping service
                else if (status == MessageGetAllStatus.InvalidPrivateKey)
                {
                    if (LocalUserManager.LocalUser != null)
                    {
                        messageGetter.PrivateKey = LocalUserManager.LocalUser.PrivateKey;
                    }
                    else
                    {
                        messageGetter.Result = null;
                        messageGetter.StopGetting();
                        messageSender.Result = null;
                        messageSender.StopSending();
                        Stop();
                    }
                } 
            };
            //Get all messages request
            MessagingCenter.Subscribe<MessagingTask>(this, MessagingTask.GetMessages, (task) =>
            {
                MessagingCenter.Send(new MessagingTask(messagesDict: messages, users: users), MessagingTask.LocalMessages);
            });
            //Get specfic amoount of messages
            MessagingCenter.Subscribe<MessagingTask>(this, MessagingTask.GetMessagesLimited, (task) =>
            {
                List<Message> messagesList = null;
                if (messages.ContainsKey(task.Conversation.Id))
                {
                    int toRemove = (task.Message == null ? 0 : messages[task.Conversation.Id].Count - messages[task.Conversation.Id].FindIndex((m) => task.Message.Id == m.Id));
                    messagesList = messages[task.Conversation.Id].AsEnumerable().Reverse().Skip(toRemove).Take(task.GetCount).Reverse().ToList();
                }
                else
                {
                    messagesList = new List<Message>();
                }
                MessagingCenter.Send(new MessagingTask(messages: messagesList, users: users, conversation: task.Conversation), MessagingTask.LocalMessagesLimited);
            });
            // Get all conversations
            MessagingCenter.Subscribe<MessagingTask>(this, MessagingTask.GetConversations, (task) =>
            {
                MessagingCenter.Send(new MessagingTask(conversations: conversations, users: users), MessagingTask.LocalConversations);
            });
            //Sender result
            messageSender.Result += (status, message) =>
            {
                //If failed stop
                if (status == MessageSendStatus.InvalidPrivateKey)
                {
                    if (LocalUserManager.LocalUser != null)
                    {
                        messageGetter.PrivateKey = LocalUserManager.LocalUser.PrivateKey;
                    }
                    else
                    {

                        messageGetter.Result = null;
                        messageGetter.StopGetting();
                        messageSender.Result = null;
                        messageSender.StopSending();
                    }
                }
            };

            //Enqueue message to sender
            MessagingCenter.Subscribe<MessagingTask>(this, MessagingTask.AddMessageToQueue, (task) =>
            {
                messageSender.AddMessageToQueue(task.Message);
            });

            //Stop the service
            MessagingCenter.Subscribe<MessagingTask>(this, MessagingTask.Stop, (task) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    messageGetter.Result = null;
                    messageGetter.StopGetting();
                    messageSender.Result = null;
                    messageSender.StopSending();
                    Stop();
                });
            });
            new Thread(() =>
            {
                var temp = users.ToDictionary(entry => entry.Key,
                                                entry => entry.Value);
                LoadData();
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(new MessagingTask(messagesDict: messages, users: users), MessagingTask.NewMessages);
                });
                foreach(var entry in temp)
                {
                    var getter = UserSystemManager.UserGetter();
                    if (getter != null)
                    {
                        getter.Result += (status, user) =>
                        {
                            if (status == UserGetStatus.Success)
                            {
                                users[user.Username] = user;
                            }
                        };
                        getter.Get(entry.Value.Username);
                    }
                }

                conversations.ForEach(conv =>
                {
                    messageGetter.TimeStamp = Math.Max(conv.LastActivity, messageGetter.TimeStamp);
                });
                messageGetter.GetUsers = true;
                if (!stoppedSelf)
                {
                    messageGetter.StartGetting();
                    messageSender.StartSending();
                }
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send(new MessagingTask(), MessagingTask.Started);
                });
                Console.WriteLine("SERVICE STARTED");
            }).Start();
;           return StartCommandResult.Sticky;
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            //base.OnTaskRemoved(rootIntent);
            Console.WriteLine("OnTaskRemoved");
            DependencyService.Get<IToast>().ShortToast("OnTaskRemoved");
            Intent intent = new Intent(SERVICE_KILLED_MESSAGE);

            SendBroadcast(intent);
        }
        public override void OnDestroy()
        {
            //base.OnDestroy();
            Console.WriteLine("OnTaskRemoved");
            DependencyService.Get<IToast>().ShortToast("OnDestroy");
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