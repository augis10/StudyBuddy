﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using StudyBuddyApp.ViewModels;
using StudyBuddyShared.SystemManager;
using StudyBuddyApp.Utility;
using StudyBuddyShared.HelpRequestSystem;
using System.Collections.ObjectModel;
using StudyBuddyApp.Models;
using StudyBuddyShared.CommentSystem;
using StudyBuddyShared.Utility.Extensions;
using StudyBuddyShared.Entity;

namespace StudyBuddyApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelpRequestCommentsPage : ContentPage
    {
        public ObservableCollection<object> Items { get; set; }
        readonly HelpRequestCommentsModel viewModel;
        private long sendEditorLastFocused = 0;
        Dictionary<string, User> users;
        public HelpRequestCommentsPage(HelpRequestCommentsModel helpRequestCommentsModel)
        {
            InitializeComponent();
            Items = new ObservableCollection<object> { };
            viewModel = helpRequestCommentsModel;
            GetComments(viewModel.HelpRequestModel.HelpRequest.Id);
            CommentList.ItemsSource = Items;
            Items.Add(viewModel);
            BindingContext = viewModel;
            if (viewModel.Creator.Username == LocalUserManager.LocalUser.Username)
            {
                deleteButton.IsEnabled = true;
            }
            else
            {
                deleteButton.IsEnabled = false;
            }
        }
        public async void OnImageButtonClicked(object sender, EventArgs e)
        {
            await ProfileOpener.OpenProfile(viewModel.Creator);
        }

        private async void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            if (await DisplayActionSheet("Ar tikrai norite ištrinti?", "Ne", "Taip") == "Taip")
            {
                var remover = HelpRequestSystemManager.NewHelpRequestRemover();
                remover.Result += (status, helpRequest) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        if (status == HelpRequestManageStatus.Success)
                        {
                            DependencyService.Get<IToast>().LongToast("Ištrinta sėkmingai");
                            await Navigation.PopAsync();
                        }
                        else
                        {
                            DependencyService.Get<IToast>().LongToast("Ištrinti nepavyko");
                        }
                    });
                };
                remover.Remove(viewModel.HelpRequestModel.HelpRequest);
            }
            return;
        }

        void GetComments(int helpRequestID)
        {
            var commentGetter = CommentSystemManager.NewCommentGetter();
            commentGetter.Result += (status, comments, users) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (status == CommentGetStatus.Success)
                    {
                        this.users = users;
                        Items.Clear();
                        Items.Add(viewModel);
                        comments.ForEach(comment =>
                        {
                            Items.Add(new CommentModel
                            {
                                Name = users[comment.Username].FirstName + " " + users[comment.Username].LastName,
                                Username = comment.Username,
                                Message = comment.Message,
                                CreatedDate = comment.CreatedDate.ToFullDate(false),
                                Comment = comment,
                                ImageLocation = users[comment.Username].ProfilePictureLocation
                            });
                        });

                    }
                    else
                    {
                        await DisplayAlert("Klaida", "Nepavyko įkelti komentarų", "OK");
                    }
                    CommentList.IsRefreshing = false;
                });
            };
            commentGetter.Get(helpRequestID);
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            GetComments(viewModel.HelpRequestModel.HelpRequest.Id);
        }

        private async void CommentList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;
            if(e.Item is HelpRequestCommentsModel)
            {
                ((ListView)sender).SelectedItem = null;
                return;
            }
            var selectedItem = ((ListView)sender).SelectedItem as CommentModel;

            if(selectedItem.Username.Equals(LocalUserManager.LocalUser.Username))
            {
                if (await DisplayActionSheet("Ar norite ištrinti komentarą?", "Taip", "Ne") == "Taip")
                RemoveComment(selectedItem.Comment.CommentID);
            }
            else
            {
                ((ListView)sender).IsEnabled = false;
                await ProfileOpener.OpenProfile(users[selectedItem.Username]);
                ((ListView)sender).IsEnabled = true;

            }
            ((ListView)sender).SelectedItem = null;
        }

        private void SendEditor_Unfocused(object sender, FocusEventArgs e)
        {
            sendEditorLastFocused = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        private void FakeEntry_Focused(object sender, FocusEventArgs e)
        {
            //Send message and refocus if needed
            PostButton_Clicked(null, null);
            if (sendEditorLastFocused + 500 >= DateTimeOffset.Now.ToUnixTimeMilliseconds())
            {
                SendEditor.Focus();
            }
            else
            {
                FakeEntry.Unfocus();
            }
        }

        private void PostButton_Clicked(object sender, EventArgs e)
        {
            if (SendEditor.Text == null)
                return;
            else if (SendEditor.Text.Length == 0)
                return;
                
            var commentManager = CommentSystemManager.NewCommentPoster();
            PostButton.IsEnabled = false;
            commentManager.Result += (status, comment) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (status == CommentManagerStatus.Success)
                    {
                        DependencyService.Get<IToast>().LongToast("Komentaras sėkmingai išsiųstas");
                        Items.Add(new CommentModel
                        {
                            Name = LocalUserManager.LocalUser.FirstName + " " + LocalUserManager.LocalUser.LastName,
                            Username = comment.Username,
                            Message = comment.Message,
                            CreatedDate = DateTime.Now.ToFullDate(),
                            Comment = comment,
                            ImageLocation = LocalUserManager.LocalUser.ProfilePictureLocation
                        });
                        CommentList.ScrollTo(Items.LastOrDefault(), ScrollToPosition.End, false);
                    }
                    else
                    {
                        DependencyService.Get<IToast>().LongToast("Komentaras nebuvo išsiųstas");
                    }
                    PostButton.IsEnabled = true;
                });
            };

            commentManager.PostComment(new Comment
            {
                Message = SendEditor.Text,
                HelpRequestID = viewModel.HelpRequestModel.HelpRequest.Id,
                Username = LocalUserManager.LocalUser.Username
            });
            SendEditor.Text = "";
        }

        private void RemoveComment(int commentID)
        {
            var commentManager = CommentSystemManager.NewCommentRemover();
            commentManager.Result += (status, comment) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (status == CommentManagerStatus.Success)
                    {
                        DependencyService.Get<IToast>().LongToast("Komentaras sėkmingai ištrintas");
                    }
                    else
                    {
                        DependencyService.Get<IToast>().LongToast("Komentaras nebuvo ištrintas");
                    }
                });
            };

            commentManager.RemoveComment(new Comment { 
                CommentID = commentID});
      
            GetComments(viewModel.HelpRequestModel.HelpRequest.Id);
        }

        private void HelpRequestCommentsPage_Refreshing(object sender, EventArgs e)
        {
            GetComments(viewModel.HelpRequestModel.HelpRequest.Id);
            CommentList.IsRefreshing = false;
        }

        private void SendEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            //PostButton.IsEnabled = SendEditor.Text.Length > 0;
        }
    }
}