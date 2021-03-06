﻿using StudyBuddyApp.Models;
using StudyBuddyShared.SystemManager;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StudyBuddyApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        List<HomeMenuItem> menuItems;
        public MenuPage()
        {
            InitializeComponent();

            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.Profile, Title="Profilis" },
                new HomeMenuItem {Id = MenuItemType.HelpRequestList, Title="Pagalbos prašymai" },
                new HomeMenuItem {Id = MenuItemType.ConversationListPage, Title="Pokalbiai" },
                new HomeMenuItem {Id = MenuItemType.RankingsListPage, Title="Reitingai"},
                new HomeMenuItem {Id = MenuItemType.SettingsPage, Title="Nustatymai"},
                new HomeMenuItem {Id = MenuItemType.LogOut, Title="Atsijungti" }
                
            };

            //Jei dėstytojas, tai pridedame jam reikalingą kategorijų sąrašą
            if (LocalUserManager.LocalUser.IsLecturer)
                menuItems.Insert(1, new HomeMenuItem { Id = MenuItemType.CategoryList, Title = "Kategorijos" });

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.SelectedItem = menuItems[0];
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                var id = (int)((HomeMenuItem)e.SelectedItem).Id;
                await RootPage.NavigateFromMenu(id);
            };
        }
    }
}