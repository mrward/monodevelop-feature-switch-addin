//
// FeatureSwitchOptionsPanel.UI.cs
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
using MonoDevelop.Components;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using Xwt;

namespace MonoDevelop.FeatureSwitch
{
	partial class FeatureSwitchOptionsPanel
	{
		const int UnspecifiedIndex = 0;
		const int EnabledIndex = 1;
		const int DisabledIndex = 2;

		Button restartButton;
		Gtk.Widget widget;
		ListView featuresListView;
		ListStore featuresListStore;
		DataField<string> featureNameDataField = new DataField<string> ();
		DataField<int> featureEnabledIndexDataField = new DataField<int> ();
		DataField<FeatureSwitch> featureDataField = new DataField<FeatureSwitch> ();
		bool changed;

		public override Control CreatePanelWidget ()
		{
			var vbox = new VBox ();
			vbox.Spacing = 6;

			featuresListStore = new ListStore (featureNameDataField, featureEnabledIndexDataField, featureDataField);
			featuresListView = new ListView ();
			featuresListView.DataSource = featuresListStore;

			var cellView = new TextCellView ();
			cellView.TextField = featureNameDataField;
			var column = new ListViewColumn ("Feature", cellView);
			featuresListView.Columns.Add (column);

			var featuresComboBoxDataSource = new List<string> ();
			var comboBoxCellView = new ComboBoxCellView ();
			comboBoxCellView.SelectedIndexField = featureEnabledIndexDataField;
			comboBoxCellView.Editable = true;
			comboBoxCellView.Items.Add (GettextCatalog.GetString ("Unspecified"));
			comboBoxCellView.Items.Add (GettextCatalog.GetString ("Enabled"));
			comboBoxCellView.Items.Add (GettextCatalog.GetString ("Disabled"));
			comboBoxCellView.SelectionChanged += FeatureEnabledComboBoxSelectionChanged;
			column = new ListViewColumn ("Enabled", comboBoxCellView);
			featuresListView.Columns.Add (column);

			vbox.PackStart (featuresListView, true, true);

			var restartLabel = new Label ();
			restartLabel.Text = GettextCatalog.GetString ("Some features may require a restart of {0}", BrandingService.ApplicationName);
			restartLabel.TextAlignment = Alignment.Start;
			vbox.PackStart (restartLabel);

			var restartButtonHBox = new HBox ();
			vbox.PackStart (restartButtonHBox, false, false);

			restartButton = new Button ();
			restartButton.Label = GettextCatalog.GetString ("Restart {0}", BrandingService.ApplicationName);
			restartButtonHBox.PackStart (restartButton, false, false);

			restartButton.Clicked += RestartButtonClicked;

			AddFeatures ();

			widget = vbox.ToGtkWidget ();
			return widget;
		}

		void FeatureEnabledComboBoxSelectionChanged (object sender, WidgetEventArgs e)
		{
			changed = true;
		}

		void AddFeatures ()
		{
			foreach (FeatureSwitch feature in FeatureSwitchConfigurations.GetFeatures ()) {
				int row = featuresListStore.AddRow ();

				featuresListStore.SetValues (
					row,
					featureNameDataField,
					feature.Name,
					featureEnabledIndexDataField,
					GetFeatureEnabledIndex (feature),
					featureDataField,
					feature);
			}
		}

		static int GetFeatureEnabledIndex (FeatureSwitch feature)
		{
			if (feature.Enabled is null) {
				return UnspecifiedIndex;
			}

			if (feature.Enabled.Value) {
				return EnabledIndex;
			}

			return DisabledIndex;
		}

		void RestartButtonClicked (object sender, EventArgs e)
		{
			IdeApp.Restart (true).Ignore ();
			(widget?.Toplevel as Gtk.Dialog)?.Respond (Gtk.ResponseType.Ok);
		}
	}
}
