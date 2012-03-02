﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultipleClipboards.Presentation.Tabs
{
	/// <summary>
	/// Interaction logic for AboutTab.xaml
	/// </summary>
	public partial class AboutTab : UserControl
	{
		public AboutTab()
		{
			InitializeComponent();
			this.VersionTextBlock.Text = string.Format("Multiple Clipboards v{0}", Assembly.GetEntryAssembly().GetName().Version);
		}
	}
}
