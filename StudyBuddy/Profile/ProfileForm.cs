﻿using StudyBuddy.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudyBuddy
{
    public partial class ProfileForm : Form
    {
        LocalUser localUser;
        User user;
        public ProfileForm(LocalUser localUser, User user)
        {
            this.localUser = localUser;
            this.user = user;
            InitializeComponent();
            this.Text = "Profilis";
        }


        private void Profile_Load(object sender, EventArgs e)
        {
            lastName.Text = user.LastName;
            karmaLabel.Text = karmaProgressBar.Value + "/" + karmaProgressBar.Maximum;
            username.Text = user.Username;
            firstName.Text = user.FirstName;
            profilePicture.ImageLocation = user.ProfilePictureLocation;
            
            if (user.IsLecturer) status.Text = "Dėstytojas";
            else status.Text = "Studentas";
            if(user.Username == localUser.Username)
            {
                writeMessageButton.Hide();
                leaveReviewButton.Hide();
            }
            else
            {
                button1.Hide();
            }
        }

        private void Profile_FormClosed(object sender, FormClosedEventArgs e)
        {
            //mainForm.checkProfileButton.Enabled = true;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            EditProfile editProfile = new EditProfile(localUser);
            editProfile.Show();
            this.Hide();

            editProfile.FormClosed += (a, b) =>
            {
                this.Show();
                this.Profile_Load(a, b);
            };
        }

        private void writeMessageButton_Click(object sender, EventArgs e)
        {
            new MessageForm(localUser, user.Username).Show();
        }

        private void leaveReviewButton_Click(object sender, EventArgs e)
        {
            new WriteUserReviewForm(localUser, user).Show();
        }

        private void ReadReviewsButton_Click(object sender, EventArgs e)
        {

            if (user.username == localUser.username)
            {
                ViewUserReviewsForm viewUserReviews = new ViewUserReviewsForm(localUser, localUser.username);
                viewUserReviews.Show();
            }
            else
            {
                ViewUserReviewsForm viewUserReviews = new ViewUserReviewsForm(localUser, user.username);
                viewUserReviews.Show();
            }
            


        }
    }
}
