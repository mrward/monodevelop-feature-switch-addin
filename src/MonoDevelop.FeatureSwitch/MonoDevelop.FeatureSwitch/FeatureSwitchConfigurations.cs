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

		const string MD_FEATURES_ENABLED = nameof (MD_FEATURES_ENABLED);
		const string MD_FEATURES_DISABLED = nameof (MD_FEATURES_DISABLED);

		static string originalFeaturesEnabledEnvironmentValue;
		static string originalFeaturesDisabledEnvironmentValue;
		static string currentFeaturesEnabledEnvironmentValue;

		static FeatureSwitchConfigurations ()
		{
			originalFeaturesEnabledEnvironmentValue = Environment.GetEnvironmentVariable (MD_FEATURES_ENABLED);
			currentFeaturesEnabledEnvironmentValue = originalFeaturesDisabledEnvironmentValue;

			originalFeaturesDisabledEnvironmentValue = Environment.GetEnvironmentVariable (MD_FEATURES_DISABLED);

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
				features [name] = new FeatureSwitch (name, enabled, enabled);
			}
		}

		public static void AddSavedFeature (string name, bool? enabled)
		{
			lock (features) {
				bool? originallyEnabled = FeatureSwitchController.IsFeatureEnabledIgnoringConfiguration (name);
				var feature = new FeatureSwitch (name, enabled, originallyEnabled);
				features [name] = feature;

				if (feature.NeedsForceEnable) {
					ForceEnableFeatures ();
				}
			}
		}

		static void ForceEnableFeatures ()
		{
			StringBuilder builder = null;
			foreach (FeatureSwitch feature in features.Values) {
				if (feature.NeedsForceEnable) {
					if (builder == null) {
						builder = StringBuilderCache.Allocate (originalFeaturesEnabledEnvironmentValue);
					}
					if (builder.Length > 0) {
						builder.Append (';');
					}
					builder.Append (feature.Name);
				}
			}

			if (builder != null) {
				currentFeaturesEnabledEnvironmentValue = StringBuilderCache.ReturnAndFree (builder);
				Environment.SetEnvironmentVariable (MD_FEATURES_ENABLED, currentFeaturesEnabledEnvironmentValue);
			} else if (currentFeaturesEnabledEnvironmentValue != originalFeaturesDisabledEnvironmentValue) {
				currentFeaturesEnabledEnvironmentValue = originalFeaturesDisabledEnvironmentValue;
				Environment.SetEnvironmentVariable (MD_FEATURES_ENABLED, currentFeaturesEnabledEnvironmentValue);
			}
		}

		internal static void OnFeaturesChanged ()
		{
			lock (features) {
				ForceEnableFeatures ();
			}
		}
	}
}
