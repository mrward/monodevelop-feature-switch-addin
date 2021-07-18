//
// FeatureSwitchDataSource.cs
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

using AppKit;
using System;

namespace MonoDevelop.FeatureSwitch.Gui
{
	class FeatureSwitchDataSource : NSTableViewDataSource
	{
		IFeatureSwitchData source;

		internal FeatureSwitch DataForRow (int row)
		{
			if (row < 0 && row > source.Data.Count - 1) {
				return null;
			}
			return source.Data[row];
		}

		public FeatureSwitchDataSource (IFeatureSwitchData source)
		{
			this.source = source;
		}

		public override nint GetRowCount (NSTableView tableView)
		{
			return source.Data.Count;
		}
	}
}
