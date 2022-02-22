//
// FeatureSwitchTableViewDelegate.cs
//
// Author:
//       jmedrano <josmed@microsoft.com>
//
// Copyright (c) 2021
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
using AppKit;
using ObjCRuntime;

namespace MonoDevelop.FeatureSwitch.Gui
{
	class FeatureSwitchTableViewDelegate : NSTableViewDelegate
	{
		internal const string FeatureCheckBoxColumnIdentifier = nameof (FeatureCheckBoxColumnIdentifier);

		public override NSView GetViewForItem (NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			var table = (FeatureSwitchTableView)tableView;

			if (tableColumn.Identifier == FeatureCheckBoxColumnIdentifier) {
				FeatureSwitch model = table.DataForRow ((int)row);

				var view = tableView.MakeView (tableColumn.Identifier, this) as FeatureSwitchCheckBox;
				if (view == null) {
					view = new FeatureSwitchCheckBox ();
					view.SetButtonType (NSButtonType.Switch);
					view.Activated += (s, e) => {
						table.OnItemChecked (model, view.State == NSCellStateValue.On);
					};
				}

				view.SetModel (model);

				return view;
			}

			throw new NotImplementedException (tableColumn.Identifier);
		}

		public override nfloat GetRowHeight (NSTableView tableView, nint row)
		{
			return 21;
		}
	}
}
