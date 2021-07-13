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

using AppKit;
using System;
using System.Collections.Generic;
using CoreGraphics;
using MonoDevelop.Core;
using ObjCRuntime;
using Foundation;

namespace MonoDevelop.FeatureSwitch
{
    interface IFeatureSwitchData
    {
        List<FeatureSwitch> Data { get; }
    }

    public class NSLabel : NSTextField
    {
        public NSLabel(string value = null)
        {
            Editable = false;
            Bordered = false;
            Bezeled = false;
            DrawsBackground = false;
            Selectable = false;
            TranslatesAutoresizingMaskIntoConstraints = false;

            if (!string.IsNullOrEmpty(value))
            {
                StringValue = value;
            }
        }
    }

    class FeatureSwitchTableView : NSTableView
    {
        public override bool IsFlipped => true;

        public FeatureSwitchTableView(IFeatureSwitchData source)
        {
            HeaderView = null;
            
            BackgroundColor = NSColor.Clear;

            var column = new NSTableColumn(PackageSourcesDelegate.nameColumn) { Title = GettextCatalog.GetString("Feature") };
            column.MinWidth = 300;
            AddColumn(column);

            AddColumn(new NSTableColumn(PackageSourcesDelegate.valueColumn) { Title = GettextCatalog.GetString("Enabled") });

            DataSource = new PackageSourcesDataSource(source);
            Delegate = new PackageSourcesDelegate();
        }

        FeatureSwitch DataForRow(int row)
        {
            return ((PackageSourcesDataSource)DataSource).DataForRow(row);
        }

        class DataCheckBox : NSButton
        {
            FeatureSwitch model;

            public DataCheckBox ()
            {
                SetButtonType(AppKit.NSButtonType.Switch);
                TranslatesAutoresizingMaskIntoConstraints = false;
                Title = string.Empty;
                Activated += DataCheckBox_Activated;
            }

            private void DataCheckBox_Activated(object sender, EventArgs e)
            {
                if (this.model != null)
                {
                    this.model.Enabled = State == NSCellStateValue.On;
                }
            }

            public void SetModel(FeatureSwitch model)
            {
                this.model = model;
                this.State = model.Enabled ? NSCellStateValue.On : NSCellStateValue.Off;
            }
        }

        class PackageSourcesDelegate : NSTableViewDelegate
        {
            internal const string nameColumn = "myCellIdentifier";
            internal const string valueColumn = "myValueIdentifier";

            public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
            {
                var table = (FeatureSwitchTableView)tableView;
                if (tableColumn.Identifier == nameColumn)
                {
                    var model = table.DataForRow((int)row);

                    var view = tableView.MakeView(tableColumn.Identifier, this) as NSTextField;
                    if (view == null)
                    {
                        view = new NSLabel();
                       
                        if (model != null)
                        {
                            view.StringValue = model.Name ?? string.Empty;
                        }
                    }
                    return view;
                }
                if (tableColumn.Identifier == valueColumn)
                {
                    var model = table.DataForRow((int)row);

                    var view = tableView.MakeView(tableColumn.Identifier, this) as DataCheckBox;
                    if (view == null)
                    {
                        view = new DataCheckBox();
                        view.SetButtonType(AppKit.NSButtonType.Switch);
                        view.Activated += (s,e) =>
                        {
                            table.OnItemChecked(model, view.State == NSCellStateValue.On);
                        };
                    }
                  
                    if (model != null)
                    {
                        view.SetModel(model);
                    }

                    return view;
                }

                throw new NotImplementedException(tableColumn.Identifier);
            }

            public override nfloat GetRowHeight(NSTableView tableView, nint row)
            {
                return 21;
            }
        }

        public event EventHandler<(FeatureSwitch, bool)> ItemChecked;

        private void OnItemChecked(FeatureSwitch model, bool value)
            => ItemChecked?.Invoke(this, (model, value));

        class PackageSourcesDataSource : NSTableViewDataSource
        {
            internal FeatureSwitch DataForRow(int row)
            {
                if (row < 0 && row > source.Data.Count - 1)
                    return null;

                return source.Data[row];
            }

            IFeatureSwitchData source;
            public PackageSourcesDataSource(IFeatureSwitchData source)
            {
                this.source = source;
            }

            public override nint GetRowCount(NSTableView tableView)
            {
                return source.Data.Count;
            }
        }
    }

    class FeatureSwitchOptionsWidget : NSStackView, IFeatureSwitchData
    {
        public FeatureSwitchOptionsWidget()
        {
            TranslatesAutoresizingMaskIntoConstraints = false;
            Orientation = NSUserInterfaceLayoutOrientation.Vertical;
            Alignment = NSLayoutAttribute.Leading;
            Spacing = 10;
            Distribution = NSStackViewDistribution.Fill;

            var scrollview = new AppKit.NSScrollView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                DrawsBackground = false,
                BackgroundColor = NSColor.Clear
            };
            AddArrangedSubview(scrollview);

            scrollview.LeadingAnchor.ConstraintEqualToAnchor(this.LeadingAnchor).Active = true;
            scrollview.TrailingAnchor.ConstraintEqualToAnchor(this.TrailingAnchor).Active = true;

            scrollview.HeightAnchor.ConstraintEqualToConstant(304).Active = true;

            tableView = new FeatureSwitchTableView(this);
            tableView.ItemChecked += (s,e) => changed = true;

            scrollview.DocumentView = tableView;

            foreach (FeatureSwitch feature in FeatureSwitchConfigurations.GetFeatures())
            {
                Data.Add(feature);
            }

            tableView.ReloadData();

            var someLabel = new NSLabel(GettextCatalog.GetString("Some features may require a restart of {0}", BrandingService.ApplicationName));
            AddArrangedSubview(someLabel);

            someLabel.LeadingAnchor.ConstraintEqualToAnchor(this.LeadingAnchor).Active = true;
            someLabel.TrailingAnchor.ConstraintEqualToAnchor(this.TrailingAnchor).Active = true;

            var restartButton = new NSButton() { Title = GettextCatalog.GetString("Restart {0}", BrandingService.ApplicationName) };
            restartButton.BezelStyle = NSBezelStyle.Rounded;
            restartButton.Action = new Selector(RestartSelectorName);
            restartButton.Target = this;

            AddArrangedSubview(restartButton);
            restartButton.WidthAnchor.ConstraintEqualToConstant (200).Active = true;
        }

        const string RestartSelectorName = "onRestartClicked:";

        [Export(RestartSelectorName)]
        private void Restart_Activated(NSObject target)
        {
            //reset ide api was hidden?
        }

        bool changed;

        FeatureSwitchTableView tableView;

        public override void SetFrameSize(CGSize newSize)
        {
            base.SetFrameSize(newSize);
            tableView.SetFrameSize(new CGSize(newSize.Width, tableView.Frame.Height));
        }

        internal void ApplyChanges()
        {
            if (!changed)
                return;

            FeatureSwitchConfigurations.OnFeaturesChanged();
        }

        public List<FeatureSwitch> Data { get; } = new List<FeatureSwitch>();
    }
}
