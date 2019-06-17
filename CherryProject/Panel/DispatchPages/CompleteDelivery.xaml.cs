﻿using CherryProject.Dialog;
using CherryProject.Extension;
using CherryProject.Model;
using CherryProject.Model.Enum;
using Microsoft.EntityFrameworkCore;
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

namespace CherryProject.Panel.DispatchPages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class CompleteDelivery : Page
	{
		private Dic dic;

		public CompleteDelivery()
		{
			this.InitializeComponent();
		}

		private async void SelectDic_Click(object sender, RoutedEventArgs e)
		{
			DicDialog dialog = new DicDialog();

			ContentDialogResult button;

			bool isCompleted = false;

			using (var context = new Context())
			{
				do
				{
					// if dialog displays more than 1 times
					if (dialog.Dic != null)
					{
						// if the order is not endorsed
						if (!isCompleted)
						{
							ContentDialog error = new ContentDialog
							{
								Title = "Alert",
								Content = "The DIC has not been completed, please wait till storemen completely assemble it.",
								CloseButtonText = "OK",
								Width = 400
							};

							await error.EnqueueAndShowIfAsync();
						}
					}

					button = await dialog.EnqueueAndShowIfAsync();

				} while (button == ContentDialogResult.Primary && !(isCompleted = context.DidSpare.Count(x => dialog.Dic.Did.Any(y => y.Id == x.DidId)) == dialog.Dic.Did.Sum(x => x.Quantity)));
			}

			if (button == ContentDialogResult.Primary)
			{
				DicGUID.Text = (dic = dialog.Dic).Id.ToString();
				SelectedDic.Text = $"Selected DIC: {dic.Id}";
			}
		}

		private async void Submit_Click(object sender, RoutedEventArgs e)
		{
			ContentDialog dialog = new ContentDialog
			{
				Title = "Confirmation",
				Content = "Are you ensure to complete the delivery?",
				PrimaryButtonText = "Complete Delivery",
				CloseButtonText = "Cancel"
			};

			if (await dialog.EnqueueAndShowIfAsync() == ContentDialogResult.Primary)
			{
				if (dic == null)
				{
					ContentDialog error = new ContentDialog
					{
						Title = "Error",
						Content = "The information you typed has mistakes, please ensure the input data validation is correct.",
						CloseButtonText = "OK",
						Width = 400
					};

					await error.EnqueueAndShowIfAsync();
				}
				else
				{
					try
					{
						using (var context = new Context())
						{
							var dic = await context.Dic.FirstOrDefaultAsync(x => x.Id == this.dic.Id);
							dic = await dic.ModifyAsync(x => x.Status = DicStatusEnum.Dispatched.ToString());

							await context.SaveChangesAsync();
						}

						ContentDialog message = new ContentDialog
						{
							Title = "Success",
							Content = "Successfully completed an delivery.",
							CloseButtonText = "OK",
							Width = 400
						};

						await message.EnqueueAndShowIfAsync();
					}
					catch (Exception err)
					{
						ContentDialog error = new ContentDialog
						{
							Title = "Error",
							Content = $"The information you typed might duplicated, please try again later.\n{err.ToString()}",
							CloseButtonText = "OK",
							Width = 400
						};

						await error.EnqueueAndShowIfAsync();
					}
				}
			}
		}
	}
}