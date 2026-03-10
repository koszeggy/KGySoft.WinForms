#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomPropertySetter.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Reflection;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.WinForms.Example.Controls
{
    public partial class CustomPropertySetter : BaseUserControl
    {
        #region Events

        public event EventHandler<PropertyChangedEventArgs> SelectedObjectsPropertyChanged
        {
            add => Events.AddHandler(nameof(SelectedObjectsPropertyChanged), value);
            remove => Events.RemoveHandler(nameof(SelectedObjectsPropertyChanged), value);
        }

        #endregion

        #region Properties

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public object[] SelectedObjects
        {
            get => field ?? Reflector.EmptyArray<object>();
            set
            {
                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                field = value ?? Reflector.EmptyArray<object>();
                ResetProperties();
            }
        }

        #endregion

        #region Constructors

        public CustomPropertySetter()
        {
            InitializeComponent();
            DynamicStringLocalization = DynamicStringLocalization.Custom;
            InitBindings();
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            #region Local Methods

            [DllImport("user32.dll")]
            static extern short GetKeyState(int nVirtKey);

            // Gets whether Enter is really pressed. Needed because selecting a suggested item by mouse raises the ProcessCmdKey with Enter
            static bool IsEnterDown()
            {
                if (!OSHelper.IsWindows)
                    return true;
                return GetKeyState((int)Keys.Enter) < 0;
            }

            #endregion

            if (keyData == Keys.Enter && IsEnterDown())
            {
                SetProperty();
                return true;
            }

            if (keyData == Keys.Escape)
            {
                ClearProperty();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Private Methods

        private void InitBindings()
        {
            CommandBindings.Add(ResetSelectedProperty)
                .AddSource(cmbProperty, nameof(cmbProperty.TextChanged));
            CommandBindings.Add(SetProperty)
                .AddSource(btnSet, nameof(btnSet.Click));
            CommandBindings.Add(ClearProperty)
                .AddSource(btnClear, nameof(btnClear.Click));

            // Note: On a BaseForm, we could just override ApplyStringResources, but this is how we can set the tool tips without an accessible ToolTip instance.
            CommandBindings.Add<LocalizationRequestedEventArgs>(Localize)
                .AddSource(typeof(LocalizationHelper), nameof(LocalizationHelper.LocalizationRequested));

            #region Local Methods

            static void Localize(ICommandSource<LocalizationRequestedEventArgs> src)
            {
                src.EventArgs.Value = src.EventArgs.Key switch
                {
                    $"{nameof(btnSet)}.ToolTipText" => "Set Value (Enter)",
                    $"{nameof(btnClear)}.ToolTipText" => "Clear Value (Esc)",
                    _ => src.EventArgs.Value
                };
            }

            #endregion
        }

        private void ResetProperties()
        {
            cmbProperty.Items.Clear();

            // Unlike on PropertyGrid, using the union of the properties, rather than the intersection of them.
            var propertyNames = new HashSet<string>();
            foreach (object selectedObject in SelectedObjects)
            {
                IEnumerable<string> properties = TypeDescriptor.GetProperties(selectedObject)
                        .Cast<PropertyDescriptor>()
                        .Where(p => p.IsBrowsable)
                        .Select(p => p.Name);
                propertyNames.AddRange(properties);
            }

            // ReSharper disable once CoVariantArrayConversion
            cmbProperty.Items.AddRange(propertyNames.ToArray());
            ResetSelectedProperty();
        }

        private void ResetSelectedProperty()
        {
            string? propertyValueText = null;

            string propertyName = cmbProperty.Text;
            foreach (object selectedObject in SelectedObjects)
            {
                // ignoring instance if it has no such a property
                PropertyDescriptor? propertyDescriptor = TypeDescriptor.GetProperties(selectedObject).Find(propertyName, false);
                if (propertyDescriptor == null)
                    continue;

                object? currentValue = propertyDescriptor.GetValue(selectedObject);
                string currentValueText = propertyDescriptor.Converter is TypeConverter converter && converter.CanConvertTo(typeof(string))
                        ? converter.ConvertToInvariantString(currentValue) ?? String.Empty
                        : currentValue.Convert<string?>() ?? String.Empty;

                // First instance: getting the value, whatever it is
                if (propertyValueText == null)
                {
                    propertyValueText = currentValueText;
                    continue;
                }

                // Non-first instance: checking if the value is the same
                if (propertyValueText == currentValueText)
                    continue;

                // Different values: clearing the displayed value
                propertyValueText = String.Empty;
                break;
            }

            txtValue.Text = propertyValueText;
            pnlButtons.Enabled = propertyValueText != null;
        }

        private void SetProperty()
        {
            string propertyName = cmbProperty.Text;
            string propertyValue = txtValue.Text;
            bool changed = false;

            foreach (object selectedObject in SelectedObjects)
            {
                // ignoring instance if it has no such a property
                PropertyDescriptor? propertyDescriptor = TypeDescriptor.GetProperties(selectedObject).Find(propertyName, false);
                if (propertyDescriptor == null)
                    continue;

                try
                {
                    object? value;
                    if (propertyDescriptor.PropertyType.In(typeof(Image), typeof(Bitmap), typeof(Metafile)))
                        value = Image.FromFile(propertyValue);
                    else if (propertyDescriptor.PropertyType == typeof(Icon))
                        value = new Icon(propertyValue);
                    else if (propertyDescriptor.PropertyType == typeof(bool))
                        value = propertyValue.Convert<bool>(); // to allow 0 or 1
                    else
                    {
                        value = propertyDescriptor.Converter is TypeConverter converter && converter.CanConvertFrom(typeof(string))
                            ? converter.ConvertFromInvariantString(propertyValue)
                            : propertyValue.Convert(propertyDescriptor.PropertyType);
                    }

                    propertyDescriptor.SetValue(selectedObject, value);
                    changed = true;
                }
                catch (Exception e)
                {
                    Dialogs.ErrorMessage($"Failed to set the property: {e.Message}");
                    return;
                }
            }

            if (changed)
                PropertyChanged(propertyName);
            ResetSelectedProperty();
        }

        private void ClearProperty()
        {
            string propertyName = cmbProperty.Text;
            bool changed = false;

            foreach (object selectedObject in SelectedObjects)
            {
                // ignoring instance if it has no such a property
                PropertyDescriptor? propertyDescriptor = TypeDescriptor.GetProperties(selectedObject).Find(propertyName, false);
                if (propertyDescriptor == null)
                    continue;

                try
                {
                    if (propertyDescriptor.CanResetValue(selectedObject))
                    {
                        propertyDescriptor.ResetValue(selectedObject);
                        continue;
                    }

                    // If the property is not resettable (e.g. Image), we set it to its default value
                    object? defaultValue = propertyDescriptor.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault() is DefaultValueAttribute d ? d.Value
                            : propertyDescriptor.PropertyType.IsValueType ? Activator.CreateInstance(propertyDescriptor.PropertyType)
                            : null;

                    propertyDescriptor.SetValue(selectedObject, defaultValue);
                    changed = true;
                }
                catch (Exception e)
                {
                    Dialogs.ErrorMessage($"Failed to reset the property: {e.Message}");
                    return;
                }
            }

            if (changed)
                PropertyChanged(propertyName);
            ResetSelectedProperty();
        }

        private void PropertyChanged(string propertyName)
            => Events.GetHandler<EventHandler<PropertyChangedEventArgs>>(nameof(SelectedObjectsPropertyChanged))?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #endregion
    }
}
