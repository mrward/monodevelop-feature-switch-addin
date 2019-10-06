﻿//
// FeatureSwitch.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2019 Microsoft Corporation
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

namespace MonoDevelop.FeatureSwitch
{
	class FeatureSwitch : IComparable<FeatureSwitch>
	{
		public FeatureSwitch (string name, bool? enabled, bool? originallyEnabled)
		{
			Name = name ?? string.Empty;
			Enabled = enabled;
			OriginallyEnabled = originallyEnabled;
		}

		public string Name { get; }
		public bool? Enabled { get; set; }
		public bool? OriginallyEnabled { get; }

		/// <summary>
		/// If the feature was disabled originally then we need to force enable it
		/// since it will be disabled if any IFeatureSwitchController disables it.
		/// </summary>
		public bool NeedsForceEnable =>
			Enabled == true &&
			OriginallyEnabled == false;

		public int CompareTo (FeatureSwitch other)
		{
			if (other is null) {
				return -1;
			}
			return string.Compare (Name, other.Name, StringComparison.Ordinal);
		}

		public override string ToString ()
		{
			return string.Format ("Feature={0} Enabled={1}", Name, Enabled);
		}
	}
}
