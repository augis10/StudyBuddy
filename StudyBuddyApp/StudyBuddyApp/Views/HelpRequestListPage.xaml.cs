﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using StudyBuddy.Entity;
using StudyBuddy.Network;
using StudyBuddyApp.Models;
using StudyBuddyApp.ViewModels;
using StudyBuddyShared.Utility.Extensions;
using System.Collections.Generic;

namespace StudyBuddyApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HelpRequestListPage : ContentPage
    {
        public ObservableCollection<HelpRequestModel> Items { get; set; }
        public ObservableCollection<HelpRequestModel> FilteredItems { get; set; }
        public HelpRequestListPage()
        {
            InitializeComponent();

            Items = new ObservableCollection<HelpRequestModel>
            {};
            FilteredItems = new ObservableCollection<HelpRequestModel>
            { };

            HelpRequestListGetter();
            Filter(null, null, false);

            HelpRequestList.ItemsSource = FilteredItems;
            PickCategory.ItemsSource = Items;

            MessagingCenter.Subscribe<HelpRequestAddPage>(this, "AddPageClosed", (addPage) =>
            {
                AddButton.IsEnabled = true;
            });
        }

        private void HelpRequestList_Refreshing(object sender, EventArgs e)
        {
            //AddButton.IsEnabled = true;
            HelpRequestListGetter();
            Filter(null, null, false);
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private void HelpRequestListGetter()
        {
            new HelpRequestGetter(LocalUserManager.LocalUser, (status, requests, users) =>
            {
                if (status == HelpRequestGetter.GetStatus.Success)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Items.Clear();
                        requests.ForEach(request =>
                        {
                            Items.Add(new HelpRequestModel
                            {
                                Title = request.Title,
                                Description = request.Description,
                                Name = users[request.CreatorUsername].FirstName + " " + users[request.CreatorUsername].LastName,
                                Category = request.Category,
                                Date = request.Timestamp.ToFullDate(),
                                HelpRequest = request
                            });
                        });
                        //FilteredItems = Items;
                        HelpRequestList.IsRefreshing = false;
                        //HelpRequestList.ItemsSource = null;
                        //HelpRequestList.ItemsSource = Items;
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        HelpRequestList.IsRefreshing = false;
                    });
                }

            }).get(true);
        }


        void Filter(string search = null, string category = null, bool own = false)
        {
            if (Items == null || LocalUserManager.LocalUser == null) // Dar nėra informacijos
            {
                return;
            }
            List<HelpRequestModel> filtered = Items.ToList();
            FilteredItems.Clear();
            filtered.ForEach((helpRequest) =>
            {
                if ((String.IsNullOrEmpty(category) || category == helpRequest.Category) &&
                    (String.IsNullOrEmpty(search) || helpRequest.Title.ToLower().Contains(search) || helpRequest.Description.ToLower().Contains(search)) &&
                    (!own || helpRequest.Name == LocalUserManager.LocalUser.Username))
                {
                    FilteredItems.Add(helpRequest);
                }
            });

        }

        async private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            AddButton.IsEnabled = false;
            await Navigation.PushModalAsync(new NavigationPage(new HelpRequestAddPage()));
        }

        private void RequestSearchBar_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PickCategory.SelectedIndex >= 0)
            {
                Filter(RequestSearchBar.Text, Items[PickCategory.SelectedIndex].Title, false);
            }
            else
            {
                Filter(RequestSearchBar.Text, null, false);
            }

        }

        private void PickCategory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PickCategory.SelectedIndex >= 0)
            {
                Filter(RequestSearchBar.Text, Items[PickCategory.SelectedIndex].Category, false);
            }
            else
            {
                Filter(RequestSearchBar.Text, null, false);
            }

        }
    }
}
