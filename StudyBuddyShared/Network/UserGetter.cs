﻿using Newtonsoft.Json.Linq;
using StudyBuddyShared.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StudyBuddyShared.Network
{
    public class UserGetter
    {
        public enum GetStatus
        {
            InvalidPrivateKey,
            Success,
            JsonReadException,
            FailedToConnect,
            UnknownError,
            FailedToFindUser,
            UsernameEmpty,
            FieldsMissing
        }

        public delegate void GetUserResultDelegate(GetStatus status, User user);
        public GetUserResultDelegate GetUserResult { get; set; }
        public string PrivateKey { get; set; }
        private Thread getUserThread;

        public UserGetter() : this("") { }
        public UserGetter(LocalUser user) : this(user.PrivateKey) { }
        public UserGetter(string privateKey) {
            PrivateKey = privateKey;
        }

        public UserGetter(GetUserResultDelegate getUserResult) : this("", getUserResult) { }
        public UserGetter(LocalUser user, GetUserResultDelegate getUserResult) : this(user.PrivateKey, getUserResult) { }
        public UserGetter(string privateKey, GetUserResultDelegate getUserResult) : this(privateKey)
        {
            GetUserResult = getUserResult;
        }

        public void get(string username)
        {
            if (getUserThread != null && getUserThread.IsAlive) // Jau vyksta užklausa
            {
                return;
            }
            if (String.IsNullOrEmpty(username) || String.IsNullOrWhiteSpace(username))
            {
                GetUserResult(GetStatus.UsernameEmpty, null);
                return;
            }
            getUserThread = new Thread(() => getLogic(username)); // There's probably a better way
            getUserThread.Start();
        }

        private void getLogic(string username)
        {
            JObject obj = new APICaller("getUser.php").AddParam("username", username).AddParam("privateKey", PrivateKey).Call();
            if (obj["status"].ToString() == "success")
            {
                User user = new User
                {
                    Username = obj["user"]["username"].ToString(),
                    FirstName = obj["user"]["firstName"].ToString(),
                    LastName = obj["user"]["lastName"].ToString(),
                    KarmaPoints = obj["user"]["karmaPoints"].ToObject<int>(),
                    IsLecturer = Convert.ToBoolean(obj["user"]["lecturer"].ToObject<int>()),
                    ProfilePictureLocation = obj["user"]["profilePicture"].ToString()
                };
                GetUserResult(GetStatus.Success, user);
            }
            else
            {
                GetStatus status = GetStatus.UnknownError;
                if (!Enum.TryParse<GetStatus>(obj["message"].ToString(), out status))
                {
                    status = GetStatus.UnknownError;
                }
                GetUserResult(status, null);
            }
        }
    }
}
