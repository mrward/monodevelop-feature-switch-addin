//
// FeatureSwitchConfigurationProperty.cs
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

using System.Xml;
using MonoDevelop.Core;

namespace MonoDevelop.FeatureSwitch
{
	class FeatureSwitchConfigurationProperty : ICustomXmlSerializer
	{
		public ICustomXmlSerializer ReadFrom (XmlReader reader)
		{
			while (reader.Read ()) {
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "Feature") {
					string name = reader.GetAttribute ("name");
					bool? enabled = GetFeatureEnabled (name, reader.GetAttribute ("enabled"));
					FeatureSwitchConfigurations.AddFeature (name, enabled);
				}
			}
			return null;
		}

		static bool? GetFeatureEnabled (string name, string text)
		{
			if (string.IsNullOrEmpty (text)) {
				return null;
			}

			if (bool.TryParse (text, out bool result)) {
				return result;
			}

			LoggingService.LogError ("Unable to read feature enabled configuration: name={0}, enabled={1}", name, text);
			return null;
		}

		public void WriteTo (XmlWriter writer)
		{
			foreach (FeatureSwitch feature in FeatureSwitchConfigurations.GetFeatures ()) {
				writer.WriteStartElement ("Feature");
				writer.WriteAttributeString ("name", feature.Name);
				writer.WriteAttributeString ("enabled", GetFeatureEnabledText (feature));
			}
		}

		static string GetFeatureEnabledText (FeatureSwitch feature)
		{
			if (feature.Enabled == null) {
				return string.Empty;
			}

			return feature.Enabled.Value.ToString ();
		}
	}
}
