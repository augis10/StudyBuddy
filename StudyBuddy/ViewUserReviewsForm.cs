﻿using StudyBuddyShared.Entity;
using StudyBuddyShared.Network;
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
    public partial class ViewUserReviewsForm : Form
    {
        List<UserReview> userReviews;
        LocalUser localUser;
        User user;
        Dictionary<string, User> users;
        private ListViewColumnSorter listViewColumnSorter;
        public ViewUserReviewsForm(LocalUser localUser)
        {
            InitializeComponent();
            this.localUser = localUser;
            progressBarKarma.Visible = false;
            labelKarmaProgress.Visible = false;
            this.AcceptButton = buttonSortReviews;
            listViewColumnSorter = new ListViewColumnSorter();
            this.listViewUserReviews.ListViewItemSorter = listViewColumnSorter;
            ResizeColumnWidth();
            this.Text = "Atsiliepimai";

        }

        public ViewUserReviewsForm(LocalUser localUser, String username) : this(localUser)
        {
            richTextBoxUsername.Text = username;
            //buttonSortReviews.PerformClick();
            buttonSortReviews_Click(buttonSortReviews, EventArgs.Empty);
        }

        private void buttonSortReviews_Click(object sender, EventArgs e)
        {
            listViewUserReviews.Items.Clear();
            new UserGetter(localUser,
                (status, user) =>
                {
                    this.Invoke((MethodInvoker)delegate //Grįžtama į main Thread !! SVARBU
                    {
                        if (status == UserGetter.GetStatus.Success) //Pavyko
                        {
                            this.user = user;

                            if (localUser.KarmaPoints < 0)
                                progressBarKarma.Value = 0;
                            else
                                progressBarKarma.Value = localUser.KarmaPoints;

                            labelKarmaProgress.Text = user.Username + " progresas " + localUser.KarmaPoints
                                + "/" + progressBarKarma.Maximum;
                            progressBarKarma.Visible = true;
                            labelKarmaProgress.Visible = true;
                            LoadUserReviews();
                        }
                        else //Ne
                        {
                            progressBarKarma.Visible = false;
                            labelKarmaProgress.Visible = false;
                            MessageBox.Show(
                            "Vartotojas " + richTextBoxUsername.Text + " nerastas",
                            "Įvyko klaida",
                            MessageBoxButtons.OK,  
                           MessageBoxIcon.Error);
                        }
                    });
                }
                ).get(richTextBoxUsername.Text);
        }

        private void richTextBoxUsername_TextChanged(object sender, EventArgs e)
        {
                buttonSortReviews.Enabled = richTextBoxUsername.Text.Length > 0;
        }

        private void LoadUserReviews()
        {
            new UserReviewGetter(localUser,
                (status, userReviews, users) =>
                {
                    this.Invoke((MethodInvoker)delegate //Grįžtama į main Thread !! SVARBU
                    {
                        if (status == UserReviewGetter.GetStatus.Success) // Pavyko
                        {
                            this.userReviews = userReviews;
                            this.users = users;
                            Console.WriteLine("success");
                            bool karmaAdd = false;
                            foreach (UserReview userReview in userReviews)
                            {
                                switch(userReview.Karma)
                                {
                                    case 0:
                                        karmaAdd = false;
                                        break;
                                    case 1:
                                        karmaAdd = true;
                                        break;
                                }
                                listViewUserReviews.Items.Add(
                                    new ListViewItem(
                                        new[] { 
                                            userReview.Username,
                                            userReview.Message,
                                            ((karmaAdd)? "+" : "-"),
                                            userReview.PostDate.ToString()}));
                            }
                            ResizeColumnWidth();
                        }
                        else // Ne
                        {
                            Console.WriteLine("error");
                            MessageBox.Show(
                            "Nepavyko užkrauti",
                            "Įvyko klaida",
                            MessageBoxButtons.OK,
                           MessageBoxIcon.Error);
                        }
                    });
                }
                ).get(true, user.Username);

            if (userReviews == null || users == null) // Dar nėra informacijos
            {
                return;
            }
        }
        private void ResizeColumnWidth()
        {
            foreach (ColumnHeader column in listViewUserReviews.Columns)
            {
                column.Width = -2;
            }
        }

        private void listViewUserReviews_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //Nustatyti ar paclickintas stulpelis jau yra rikiuojamas
            if (e.Column == listViewColumnSorter.SortColumn)
            {
                //Pakeisti dabartinę stulpelio rikiavimo tvarką
                if (listViewColumnSorter.Order == SortOrder.Ascending)
                {
                    listViewColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    listViewColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                //Nustatyti stulpelio, kuris bus rikiuojamas numerį. Pagal nutylėjimą rikiuojama didėjančia tvarka
                listViewColumnSorter.SortColumn = e.Column;
                listViewColumnSorter.Order = SortOrder.Ascending;
            }

            //Atlikti rikiavimą su naujais rikiavimo nustatymais
            this.listViewUserReviews.Sort();
        }

        private void listViewUserReviews_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            e.Cancel = true;
            e.NewWidth = listViewUserReviews.Columns[e.ColumnIndex].Width;
        }
    }
}
