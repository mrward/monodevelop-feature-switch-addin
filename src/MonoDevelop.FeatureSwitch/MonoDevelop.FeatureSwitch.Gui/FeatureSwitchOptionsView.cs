//
// FeatureSwitchOptionsView.cs
//
// Author:
//       jmedrano <josmed@microsoft.com>
//
// Copyright (c) 2021 Microsoft Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using MonoDevelop.Core;

namespace MonoDevelop.FeatureSwitch.Gui
{
	class FeatureSwitchOptionsView : NSStackView, IFeatureSwitchData
	{
		readonly FeatureSwitchTableView tableView;
		bool changed;

		public List<FeatureSwitch> Data { get; } = new List<FeatureSwitch> ();

		public FeatureSwitchOptionsView ()
		{
			TranslatesAutoresizingMaskIntoConstraints = false;
			Orientation = NSUserInterfaceLayoutOrientation.Vertical;
			Alignment = NSLayoutAttribute.Leading;
			Spacing = 10;
			Distribution = NSStackViewDistribution.Fill;

			var scrollview = new NSScrollView () {
				TranslatesAutoresizingMaskIntoConstraints = false,
				DrawsBackground = false,
				BackgroundColor = NSColor.Clear,
				AutohidesScrollers = true,
				HasVerticalScroller = true,
				VerticalScroller = new NoBackgroundScroller ()
			};
			AddArrangedSubview (scrollview);

			scrollview.LeadingAnchor.ConstraintEqualToAnchor (LeadingAnchor).Active = true;
			scrollview.TrailingAnchor.ConstraintEqualToAnchor (TrailingAnchor).Active = true;

			scrollview.HeightAnchor.ConstraintEqualToConstant (304).Active = true;

			tableView = new FeatureSwitchTableView (this);
			tableView.ItemChecked += FeatureSwitchTableViewItemChecked;

			scrollview.DocumentView = tableView;

			var separatorRestart = new NSBox ();
			separatorRestart.BoxType = NSBoxType.NSBoxSeparator;
			AddArrangedSubview (separatorRestart);

			separatorRestart.LeadingAnchor.ConstraintEqualToAnchor (LeadingAnchor).Active = true;
			separatorRestart.TrailingAnchor.ConstraintEqualToAnchor (TrailingAnchor).Active = true;

			var restartLabel = new NSLabel ();
			restartLabel.StringValue = GettextCatalog.GetString ("Some features may require a restart of {0}", BrandingService.ApplicationName);
			AddArrangedSubview (restartLabel);

			restartLabel.LeadingAnchor.ConstraintEqualToAnchor (LeadingAnchor).Active = true;
			restartLabel.TrailingAnchor.ConstraintEqualToAnchor (TrailingAnchor).Active = true;

			if (IdeRestarter.CanRestart ())
			{
				var restartButton = new NSButton ();
				restartButton.Title = GettextCatalog.GetString ("Restart {0}", BrandingService.ApplicationName);
				restartButton.BezelStyle = NSBezelStyle.Rounded;
				restartButton.Action = new Selector (RestartSelectorName);
				restartButton.Target = this;

				AddArrangedSubview (restartButton);
				restartButton.WidthAnchor.ConstraintEqualToConstant (200).Active = true;
			}
		}

		public void AddFeatures (IEnumerable<FeatureSwitch> features)
		{
			Data.AddRange (features);
			tableView.ReloadData();
		}

		void FeatureSwitchTableViewItemChecked (object sender, (FeatureSwitch, bool) e)
		{
			changed = true;
		}

		const string RestartSelectorName = "onRestartClicked:";

		[Export (RestartSelectorName)]
		void RestartActivated (NSObject target)
		{
			IdeRestarter.RestartAsync (true).Ignore ();
		}

		public override void SetFrameSize (CGSize newSize)
		{
			base.SetFrameSize (newSize);
			tableView.SetFrameSize (new CGSize (newSize.Width, tableView.Frame.Height));
		}

		internal void ApplyChanges ()
		{
			if (!changed)
				return;

			FeatureSwitchConfigurations.OnFeaturesChanged ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				tableView.ItemChecked -= FeatureSwitchTableViewItemChecked;
			}
			base.Dispose (disposing);
		}
	}
}
