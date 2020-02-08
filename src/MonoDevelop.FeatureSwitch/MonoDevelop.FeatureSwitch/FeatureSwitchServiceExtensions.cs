//
// FeatureSwitchServiceExtensions.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2020 Microsoft Corporation
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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Core.FeatureConfiguration;
using MonoDevelop.Core;

namespace MonoDevelop.FeatureSwitch
{
	static class FeatureSwitchServiceExtensions
	{
		/// <summary>
		/// Gets registered features. The FeatureSwitchService.DescribeFeatures method is internal
		/// so we use reflection to get this information.
		/// </summary>
		public static IEnumerable<FeatureSwitch> DescribeFeatures ()
		{
			try {
				MethodInfo method = typeof (FeatureSwitchService)
					.GetMethod (nameof (DescribeFeatures), BindingFlags.NonPublic | BindingFlags.Static);

				if (method == null) {
					LoggingService.LogError ("Could not find FeatureSwitchService.DescribeFeatures method using reflection");
					return Enumerable.Empty<FeatureSwitch> ();
				}

				var results = method.Invoke (null, new object[0]) as IEnumerable;

				if (results == null) {
					LoggingService.LogError ("FeatureSwitchService.DescribeFeatures did not return an IEnumerable");
					return Enumerable.Empty<FeatureSwitch> ();
				}

				var features = new List<FeatureSwitch> ();

				foreach (object featureSwitchObject in results) {
					FeatureSwitch feature = ConvertToFeatureSwitch (featureSwitchObject);
					if (feature != null) {
						features.Add (feature);
					}
				}

				return features;

			} catch (Exception ex) {
				LoggingService.LogError ("Failed to get features from FeatureSwitchService.DescribeFeatures", ex);
			}

			return Enumerable.Empty<FeatureSwitch> ();
		}

		static FeatureSwitch ConvertToFeatureSwitch (object featureSwitchObject)
		{
			Type type = featureSwitchObject.GetType ();
			PropertyInfo nameProperty = type.GetProperty ("Name");
			PropertyInfo currentValueProperty = type.GetProperty ("CurrentValue");
			PropertyInfo defaultValueProperty = type.GetProperty ("DefaultValue");

			if (nameProperty == null || currentValueProperty == null || defaultValueProperty == null) {
				LoggingService.LogInfo ("Unable to get feature switch info using reflection. Object: {0}", featureSwitchObject);
				return null;
			}

			string name = (string)nameProperty.GetValue (featureSwitchObject);
			bool? currentValue = (bool?)currentValueProperty.GetValue (featureSwitchObject);
			bool defaultValue = (bool)defaultValueProperty.GetValue (featureSwitchObject);

			bool enabled = currentValue ?? defaultValue;
			return new FeatureSwitch (name, enabled);
		}
	}
}
