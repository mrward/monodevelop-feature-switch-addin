//
// FeatureSwitchEnvironmentVariables.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoDevelop.Core;

namespace MonoDevelop.FeatureSwitch
{
	static class FeatureSwitchEnvironmentVariables
	{
		const string MD_FEATURES_ENABLED = nameof (MD_FEATURES_ENABLED);
		const string MD_FEATURES_DISABLED = nameof (MD_FEATURES_DISABLED);

		public static IEnumerable<FeatureSwitch> GetFeatures ()
		{
			foreach (string enabledFeature in GetEnvironmentVariableValues (MD_FEATURES_ENABLED)) {
				yield return new FeatureSwitch (enabledFeature, enabled: true);
			}

			// Return disabled last since these override the enabled features in
			// the FeatureSwitchService.
			foreach (string disabledFeature in GetEnvironmentVariableValues (MD_FEATURES_DISABLED)) {
				yield return new FeatureSwitch (disabledFeature, enabled: false);
			}
		}

		static IEnumerable<string> GetEnvironmentVariableValues (string variableName)
		{
			string value = Environment.GetEnvironmentVariable (variableName);
			if (value != null) {
				foreach (string feature in value.Split (';')) {
					if (!string.IsNullOrEmpty (feature)) {
						yield return feature;
					}
				}
			}
		}

		internal static void Update (IEnumerable<FeatureSwitch> features)
		{
			UpdateEnvironmentVariable (MD_FEATURES_ENABLED, features.Where (feature => feature.Enabled));
			UpdateEnvironmentVariable (MD_FEATURES_DISABLED, features.Where (feature => !feature.Enabled));
		}

		internal static void UpdateEnvironmentVariable (
			string environmentVariableName,
			IEnumerable<FeatureSwitch> features)
		{
			var builder = StringBuilderCache.Allocate ();
			foreach (FeatureSwitch feature in features) {
				if (builder.Length > 0) {
					builder.Append (';');
				}
				builder.Append (feature.Name);
			}

			string value = StringBuilderCache.ReturnAndFree (builder);
			Environment.SetEnvironmentVariable (environmentVariableName, value);
		}
	}
}
