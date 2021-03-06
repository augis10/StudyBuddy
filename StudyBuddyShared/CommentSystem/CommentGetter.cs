﻿using StudyBuddyShared.Entity;
using StudyBuddyShared.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudyBuddyShared.CommentSystem
{
    public class CommentGetter : ICommentGetter
    {
        public GetCommentDelegate Result { get; set; }

        public string PrivateKey { get; set; }

        private Network.CommentsGetter getter;

        public CommentGetter(LocalUser user) : this(user.PrivateKey)
        {

        }

        public CommentGetter(string privateKey)
        {
            this.PrivateKey = privateKey;
            getter = new Network.CommentsGetter(PrivateKey, (status, comments, users) =>
            {
                Result?.Invoke(EnumConverter.Convert<CommentGetStatus>(status), comments, users);
            });
        }

        public void Get(int helpRequestID, bool getUsers = true)
        {
            getter.get(helpRequestID, getUsers);
        }
    }
}
