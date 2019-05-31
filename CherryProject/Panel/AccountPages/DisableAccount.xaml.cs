﻿using CherryProject.Extension;
using CherryProject.Model;
using CherryProject.Model.Enum;
using CherryProject.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CherryProject.Panel.AccountPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DisableAccount : Page
    {
		private User user;

        public DisableAccount()
        {
            this.InitializeComponent();

			Enum.GetValues(typeof(GeneralStatusEnum)).Cast<GeneralStatusEnum>().ToList().ForEach(x =>
			{
				Status.Items.Add(x.ToString());
			});
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			if (e.Parameter is User user)
			{
				Status.SelectedItem = (this.user = user).Status;

				Status.SelectionChanged += Status_SelectionChanged;
			}
			else
			{
				ContentDialog dialog = new ContentDialog
				{
					Title = "Alert",
					Content = "You have to choose an account before disabling it.",
					CloseButtonText = "OK"
				};

				ContentDialogResult result = await dialog.EnqueueAndShowIfAsync();

				this.Frame.Navigate(typeof(SearchAccounts), this.GetType(), new DrillInNavigationTransitionInfo());
			}
		}

		private async void Status_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ContentDialog dialog = new ContentDialog
			{
				Title = "Confirmation",
				Content = "Are you ensure to disable/enable current account?",
				PrimaryButtonText = "Disable Account",
				CloseButtonText = "Cancel"
			};

			ContentDialogResult result = await dialog.EnqueueAndShowIfAsync();

			if (result == ContentDialogResult.Primary)
			{
				try
				{
					user = await user.ModifyAsync(x => x.Status = Status.SelectedItem as string);

					ContentDialog message = new ContentDialog
					{
						Title = "Success",
						Content = "Successfully disabled user.",
						CloseButtonText = "OK",
						Width = 400
					};

					await message.EnqueueAndShowIfAsync();

					this.Frame.Navigate(typeof(ViewAccount), user, new DrillInNavigationTransitionInfo());
				}
				catch (Exception)
				{
					ContentDialog error = new ContentDialog
					{
						Title = "Error",
						Content = "The information you typed might duplicated, please re-type the username and make sure the email has not been registered before.",
						CloseButtonText = "OK",
						Width = 400
					};

					await error.EnqueueAndShowIfAsync();
				}
			}
		}
	}
}