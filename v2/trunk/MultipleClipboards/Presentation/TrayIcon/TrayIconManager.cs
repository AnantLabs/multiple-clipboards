using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MultipleClipboards.Entities;
using MultipleClipboards.Messaging;
using MultipleClipboards.Presentation.Icons;
using wyDay.Controls;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using Timer = System.Timers.Timer;

namespace MultipleClipboards.Presentation.TrayIcon
{
	public sealed class TrayIconManager : IDisposable
	{
		private readonly IDictionary<ulong, MenuItem> menuItemsByClipboardId;
		private readonly NotifyIcon notifyIcon;
		private readonly VistaMenu menuHelper;
		private readonly Timer trayPopupTimer;
		private ContextMenu contextMenu;
		private Popup trayPopup;
		private Border trayPopupBorder;
		private Image trayPopupIcon;
		private TextBlock trayPopupTextBlock;

		public TrayIconManager(NotifyIcon notifyIcon)
		{
			this.menuHelper = new VistaMenu();
			this.menuItemsByClipboardId = new Dictionary<ulong, MenuItem>();
			this.notifyIcon = notifyIcon;
			this.notifyIcon.MouseDoubleClick += NotifyIconMouseDoubleClick;
			this.trayPopupTimer = new Timer(5000);
			this.trayPopupTimer.Elapsed += (sender, args) => Application.Current.Dispatcher.Invoke(new Action(OnTrayPopupTimerStop));
			this.CacheTrayPopupElements();
			this.trayPopup.CustomPopupPlacementCallback = GetPopupPlacement;
			MessageBus.Instance.Subscribe<TrayNotification>(this.NotificationRecieved);
		}

		public void OnClipboardManagerInitialized()
		{
			AppController.ClipboardManager.ClipboardHistory.CollectionChanged += ClipboardHistoryCollectionChanged;
			this.InitializeContextMenu();
		}

		public void ShowTrayIcon()
		{
			this.notifyIcon.Visible = true;
		}

		public void HideTrayIcon()
		{
			this.notifyIcon.Visible = false;
		}

		public void Dispose()
		{
			this.notifyIcon.Dispose();
		}

		private static CustomPopupPlacement[] GetPopupPlacement(Size popupSize, Size targetSize, Point offset)
		{
			var point = SystemParameters.WorkArea.BottomRight;
			point.Y = point.Y - popupSize.Height;
			return new[] { new CustomPopupPlacement(point, PopupPrimaryAxis.Horizontal) };
		}

		private void CacheTrayPopupElements()
		{
			// HACK: This is kind of ugly, and there may be a better way to do this, but for such a simple control I do not care.
			//		 I need to find the child elements in the popup that have dynamic content, but VisualTreeHelper is of no use
			//		 because the popup has not been shown yet.  But, this is a popup that I have defined in my own resource dictionary,
			//		 so I will just find the controls by manually traversing the tree;
			//		 There are a lot of potential null refference exceptions in here, but that's OK since this would fail later on anyway without them.

			this.trayPopup = (Popup)Application.Current.FindResource("TrayPopup");
			this.trayPopupBorder = (Border)trayPopup.Child;
			var grid = (Grid)this.trayPopupBorder.Child;
			this.trayPopupIcon = (Image)grid.Children[0];
			this.trayPopupTextBlock = (TextBlock)grid.Children[1];
		}

		private void ClipboardHistoryCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// First, remove all items from the menu that have been cleared from the history queue.
			if (e.OldItems != null)
			{
				foreach (var clipboardData in e.OldItems.Cast<ClipboardData>())
				{
					var menuItem = this.menuItemsByClipboardId[clipboardData.Id];
					this.contextMenu.MenuItems.Remove(menuItem);
					menuItem.Dispose();
					this.menuItemsByClipboardId.Remove(clipboardData.Id);
				}
			}

			// Now add the new ones.
			if (e.NewItems != null)
			{
				this.AddClipboardHistoryItemsToConextMenu(e.NewItems.Cast<ClipboardData>());
			}
		}

		private static void NotifyIconMouseDoubleClick(object sender, MouseEventArgs e)
		{
			AppController.ShowMainWindow();
		}

		private void InitializeContextMenu()
		{
			var exitMenuItem = new MenuItem("Exit", (sender, args) => AppController.Shutdown());
			var mainWindowMenuItem = new MenuItem("Multiple Clipboards...", (sender, args) => AppController.ShowMainWindow())
			{
				DefaultItem = true
			};
			var seperator = new MenuItem("-");
			this.contextMenu = new ContextMenu(new[] { seperator, mainWindowMenuItem, exitMenuItem });
			this.AddClipboardHistoryItemsToConextMenu(AppController.ClipboardManager.ClipboardHistory);

			this.menuHelper.SetImage(exitMenuItem, IconFactory.GetTrayContextMenuBitmap(IconType.Exit));
			this.menuHelper.SetImage(mainWindowMenuItem, IconFactory.GetTrayContextMenuBitmap(IconType.Clipboard));
			this.menuHelper.EndInit();

			this.notifyIcon.ContextMenu = this.contextMenu;
		}

		private void AddClipboardHistoryItemsToConextMenu(IEnumerable<ClipboardData> clipboardDataItems)
		{
			foreach (var clipboardData in clipboardDataItems)
			{
				var menuItem = BuildClipboardHistoryMenuItem(clipboardData);
				this.contextMenu.MenuItems.Add(0, menuItem);
				this.menuHelper.SetImage(menuItem, IconFactory.GetTrayContextMenuBitmap(clipboardData.IconType));
			}

			this.menuHelper.EndInit();
		}

		private static MenuItem BuildClipboardHistoryMenuItem(ClipboardData clipboardData)
		{
			var menuItem = new MenuItem(string.Format("{0}\t{1}", clipboardData.DataPreview, clipboardData.TimeStamp.ToString("T")));
			menuItem.Click += (sender, args) => AppController.ClipboardManager.PlaceHistoricalEntryOnClipboard(ClipboardDefinition.SystemClipboardId, clipboardData.Id);
			return menuItem;
		}

		private void NotificationRecieved(TrayNotification mainWindowNotification)
		{
			if (this.trayPopupBorder == null || this.trayPopupIcon == null || this.trayPopupTextBlock == null)
			{
				throw new NullReferenceException("Unable to show notification popup because the required UI elements could not be found.");
			}

			if (mainWindowNotification.BorderBrush == null)
			{
				switch (mainWindowNotification.IconType)
				{
					case IconType.Error:
						this.trayPopupBorder.BorderBrush = Brushes.Red;
						break;

					case IconType.Warning:
						this.trayPopupBorder.BorderBrush = Brushes.Yellow;
						break;

					case IconType.Success:
						this.trayPopupBorder.BorderBrush = Brushes.Green;
						break;

					default:
						this.trayPopupBorder.BorderBrush = Brushes.Black;
						break;
				}
			}
			else
			{
				this.trayPopupBorder.BorderBrush = mainWindowNotification.BorderBrush;
			}

			var bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(IconFactory.GetIconPath32(mainWindowNotification.IconType), UriKind.Relative);
			bitmap.DecodePixelWidth = 32;
			bitmap.EndInit();

			this.trayPopupIcon.Source = bitmap;
			this.trayPopupTextBlock.Text = mainWindowNotification.MessageBody;
			this.trayPopup.IsOpen = true;
			this.trayPopupTimer.Start();
		}

		private void OnTrayPopupTimerStop()
		{
			this.trayPopupTimer.Stop();
			this.trayPopup.IsOpen = false;
		}
	}
}
