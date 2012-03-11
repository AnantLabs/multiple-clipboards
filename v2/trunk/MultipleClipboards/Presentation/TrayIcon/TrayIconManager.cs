using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Forms;
using MultipleClipboards.Entities;
using MultipleClipboards.Presentation.Icons;
using wyDay.Controls;

namespace MultipleClipboards.Presentation.TrayIcon
{
	public sealed class TrayIconManager : IDisposable
	{
		private readonly IDictionary<ulong, MenuItem> menuItemsByClipboardId;
		private readonly NotifyIcon notifyIcon;
		private readonly VistaMenu menuHelper;
		private ContextMenu contextMenu;

		public TrayIconManager(NotifyIcon notifyIcon)
		{
			this.menuHelper = new VistaMenu();
			this.menuItemsByClipboardId = new Dictionary<ulong, MenuItem>();
			this.notifyIcon = notifyIcon;
			this.notifyIcon.MouseDoubleClick += NotifyIconMouseDoubleClick;
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
	}
}
