//
// FeatureSwitchConfigurations.cs
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
	static class FeatureSwitchConfigurations
	{
		static readonly FeatureSwitchConfigurationProperty featureSwitchConfiguration;
		static readonly ConfigurationProperty<FeatureSwitchConfigurationProperty> configurations;
		static readonly Dictionary<string, FeatureSwitch> features =
			new Dictionary<string, FeatureSwitch> (StringComparer.OrdinalIgnoreCase);

		static FeatureSwitchConfigurations ()
		{
			ReadFeatureEnvironmentVariables ();

			configurations = ConfigurationProperty.Create (
				"MonoDevelop.FeatureSwitchAddin.Configuration",
				new FeatureSwitchConfigurationProperty ()
			);
			featureSwitchConfiguration = configurations.Value;
		}

		public static IEnumerable<FeatureSwitch> GetFeatures ()
		{
			lock (features) {
				var list = features.Values.ToList ();
				list.Sort ();
				return list;
			}
		}

		public static FeatureSwitch GetFeature (string name)
		{
			lock (features) {
				if (features.TryGetValue (name, out FeatureSwitch feature)) {
					return feature;
				}
				return null;
			}
		}

		public static void AddFeature (string name, bool? enabled)
		{
			lock (features) {
				features [name] = new FeatureSwitch (name, enabled.GetValueOrDefault ());
			}

			OnFeaturesChanged ();
		}

		public static void AddSavedFeature (string name, bool enabled)
		{
			lock (features) {
				var feature = new FeatureSwitch (name, enabled);
				features [name] = feature;
			}
		}

		static void ReadFeatureEnvironmentVariables ()
		{
			try {
				foreach (FeatureSwitch feature in FeatureSwitchEnvironmentVariables.GetFeatures ()) {
					features [feature.Name] = feature;
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Unable to read environment variables", ex);
			}
		}

		internal static void OnFeaturesChanged ()
		{
			try {
				lock (features) {
					FeatureSwitchEnvironmentVariables.Update (features.Values);
				}
			} catch (Exception ex) {
				LoggingService.LogError ("Unable to update environment variables", ex);
			}
		}
	}
}
