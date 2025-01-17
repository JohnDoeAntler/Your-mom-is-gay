﻿using CherryProject.Attribute;
using CherryProject.Extension;
using CherryProject.Model.Enum;
using CherryProject.Service;
using CherryProject.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CherryProject.Panel
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class PanelPage : Page
	{
		private IndexGridViewItem panel { get; set; }
		private readonly ObservableCollection<IndexGridViewItem> items;

		public PanelPage()
		{
			this.InitializeComponent();
			this.items = IndexPage.GetIndexGridViewItems(SignInManager.CurrentUser.Role);

			Window.Current.SetTitleBar(titleBar);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			if (e.Parameter is IndexGridViewItem panel)
			{
				this.panel = panel;

				foreach (var t in panel.Views.OrderBy(x => x.Name))
				{
					if (t.GetCustomAttribute<HiddenAttribute>() == null)
					{
						NavigationViewControl.MenuItems.Add(
							new NavigationViewItem()
							{
								Content = t.ClassNameToString(),
								Tag = t.Name,
								Icon = new SymbolIcon(panel.Icon),
							}
						);
					}
				}

				NavigationViewControl.PaneTitle = panel.Title;

				contentFrame.Navigate(panel.Views.FirstOrDefault(x => x.GetCustomAttribute<HiddenAttribute>() == null), null, new DrillInNavigationTransitionInfo());

				header.Text = panel.Views.FirstOrDefault(x => x.GetCustomAttribute<HiddenAttribute>() == null).ClassNameToString();
			}
			else if (e.Parameter is Tuple<IndexGridViewItem, Guid> tuple)
			{
				this.panel = tuple.Item1;

				foreach (var t in tuple.Item1.Views.OrderBy(x => x.Name))
				{
					if (t.GetCustomAttribute<HiddenAttribute>() == null)
					{
						NavigationViewControl.MenuItems.Add(
							new NavigationViewItem()
							{
								Content = t.ClassNameToString(),
								Tag = t.Name,
								Icon = new SymbolIcon(tuple.Item1.Icon),
							}
						);
					}
				}

				NavigationViewControl.PaneTitle = tuple.Item1.Title;

				contentFrame.Navigate(tuple.Item1.Views.FirstOrDefault(), tuple.Item2, new DrillInNavigationTransitionInfo());

				header.Text = tuple.Item1.Views.FirstOrDefault(x => x.GetCustomAttribute<HiddenAttribute>() == null).ClassNameToString();
			}
		}

		private void OnBackRequested(object sender, RoutedEventArgs e)
		{
			Frame.Navigate(typeof(IndexPage), null, new DrillInNavigationTransitionInfo());
		}

		private void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
		{
			if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
			{
				if (string.IsNullOrEmpty(sender.Text))
				{
					sender.ItemsSource = null;
				}
				else
				{
					var list = new ObservableCollection<NavigationViewItemBase>(
						PermissionManager
							.GetPermission(SignInManager.CurrentUser.Role, false)
							.Where(x => x.ClassNameToString().Contains(sender.Text, StringComparison.OrdinalIgnoreCase))
							.Take(5)
							.Select(x => new NavigationViewItem()
							{
								Content = x.ClassNameToString(),
								Icon = new SymbolIcon(
										items
										.FirstOrDefault(y => y.Views.Any(z => z.Name == x.Name))
										.Icon
								)
							}
						)
					);

					if (list.Count != 0)
					{
						list.Add(new NavigationViewItemSeparator());
						list.Add(new NavigationViewItem()
						{
							Content = "Show all results"
						});
					}
					else
					{
						list.Add(new NavigationViewItem()
						{
							Content = $"No results for {sender.Text}",
							IsEnabled = false
						});
					}

					sender.ItemsSource = list;
				}
			}
		}

		private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (args.ChosenSuggestion != null)
			{
				// User selected an item from the suggestion list, take an action on it here.

				var selected = args.ChosenSuggestion as string;

				foreach (var item in items)
				{
					foreach (var view in item.Views)
					{
						if (selected == view.ClassNameToString())
						{
							item.Views = item.Views.OrderBy(x => x != view);
							
							Frame.Navigate(typeof(PanelPage), item, new DrillInNavigationTransitionInfo());
						}
					}
				}
			}
			else
			{
				// Use args.QueryText to determine what to do.

				var result = PermissionManager
						.GetPermission(SignInManager.CurrentUser.Role, false)
						.FirstOrDefault(x => x.ClassNameToString().Contains(sender.Text, StringComparison.OrdinalIgnoreCase));

				if (result != null)
				{
					var item = items.FirstOrDefault(x => x.Views.Any(y => y == result));
					item.Views = item.Views.OrderBy(x => x != result);

					Frame.Navigate(typeof(PanelPage), item, new DrillInNavigationTransitionInfo());
				}
			}
		}

		private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{

			if (args.IsSettingsInvoked)
			{
				Frame.Navigate(typeof(SettingPage), null, new DrillInNavigationTransitionInfo());
			}
			else
			{
				var item = args.InvokedItem as string;

				if (item as string == "Main Page")
				{
					Frame.Navigate(typeof(IndexPage), null, new DrillInNavigationTransitionInfo());
				}
				else
				{
					foreach (var view in panel.Views)
					{
						if (view.ClassNameToString() == item)
						{
							contentFrame.Navigate(view, null, new DrillInNavigationTransitionInfo());

							header.Text = view.ClassNameToString();
						}
					}
				}
			}
		}
	}
}
