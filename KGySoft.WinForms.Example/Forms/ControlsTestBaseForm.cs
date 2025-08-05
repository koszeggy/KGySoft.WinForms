#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ControlsTestBaseForm.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
using System.Linq;
using System.Windows.Forms;

using KGySoft.Reflection;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.WinForms.Example.Forms
{
    internal partial class ControlsTestBaseForm : BaseForm
    {
        #region Constants

        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int WM_LBUTTONDOWN = 0x201;

        #endregion

        #region Constructors

        public ControlsTestBaseForm()
        {
            InitializeComponent();
            AutoScaleFont = Program.AutoScaleFont;
            AutoScaleMode = Program.AutoScaleMode;
            StartPosition = Program.StartPosition;
            if (!IsDesignMode && SystemFonts.MessageBoxFont is Font font)
                Font = font;
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override void WndProc(ref Message m)
        {
            #region Local Methods

            static Control? FindControl(Control parent, Point cursorPosition)
            {
                if (!parent.HasChildren)
                    return parent;

                Control? child = parent.GetChildAtPoint(parent.PointToClient(cursorPosition), GetChildAtPointSkip.Invisible);
                if (child == null)
                    return parent;

                return FindControl(child, cursorPosition);
            }

            #endregion

            if (DesignMode)
            {
                base.WndProc(ref m);
                return;
            }

            switch (m.Msg)
            {
                case WM_LBUTTONDOWN: // when clicking over the disabled pnlTestArea (so the form gets the mouse down event)
                case WM_MOUSEACTIVATE when (m.LParam.ToInt32() >> 16) == WM_LBUTTONDOWN: // when clicking over a child control, even disabled ones
                    Control? child = FindControl(this, Cursor.Position);
                    if (child == null || child == grdProperties || grdProperties.Contains(child))
                        break;

                    if (child.Parent is CheckGroupBox)
                        child = child.Parent; // CheckBox or the content panel of CheckGroupBox

                    // selecting a single object
                    if ((ModifierKeys & Keys.Shift) == 0)
                    {
                        if (grdProperties.SelectedObject != child)
                            grdProperties.SelectedObject = child;
                        break;
                    }

                    // adding child to selected objects
                    var selectedObjects = grdProperties.SelectedObjects.ToList();
                    if (!selectedObjects.Contains(child))
                    {
                        selectedObjects.Add(child);
                        grdProperties.SelectedObjects = selectedObjects.ToArray();
                    }

                    break;
            }

            base.WndProc(ref m);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            lblInstruction.SendToBack();
            Text += @$" AutoScaleFont: {AutoScaleFont}";
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (!Enabled)
                Enabled = true; // prevents the form from being disabled along with the property grid and the close button
        }

        #endregion

        #region Event handlers

        private void miResetValue_Click(object sender, EventArgs e)
        {
            #region Local Methods

            // Gets the actual selected object(s)
            static object? GetSelectedObject(PropertyGrid grid)
            {
                object[] selectedObjects = grid.SelectedObjects;

                // when multiple objects are selected, the descriptor is a MergePropertyDescriptor that expects an array of objects
                var result = selectedObjects.Length switch { 0 => null, 1 => selectedObjects[0], _ => selectedObjects };
                
                GridItem? selectedItem = grid.SelectedGridItem;
                if (result == null || selectedItem?.Parent?.GridItemType != GridItemType.Property)
                    return result;

                // Non-root property: the actual instance is a child property value of the root object.
                // Unfortunately, the GridItem does not expose a public API to get the actual object, but if we are lucky, we can just return the internal Instance property.
                if (Reflector.TryGetProperty(selectedItem, "Instance", out object? value))
                    return value;

                // Fallback solution by using public API if possible (may fail when multiple objects are selected, see below)
                var parents = new Stack<GridItem>();
                for (GridItem parent = selectedItem.Parent; parent.GridItemType == GridItemType.Property; parent = parent.Parent)
                    parents.Push(parent);

                foreach (GridItem gridItem in parents)
                {
                    PropertyDescriptor? descriptor = gridItem.PropertyDescriptor;
                    if (descriptor == null)
                        return null;

                    // ISSUE: MergePropertyDescriptor.GetValue works differently than SetValue and ResetValue: instead of returning an array of objects,
                    // it returns a single object if the result is the same for all selected objects; otherwise, it returns null.
                    // So trying to call its internal GetValues method. If it's not available, trying to fall back to the default GetValue method, maybe it works as expected on other platforms.
                    if (selectedObjects.Length > 1)
                    {
                        if (Reflector.TryInvokeMethod(descriptor, "GetValues", out object? getObjectsResult, result) && getObjectsResult is Array)
                        {
                            result = getObjectsResult;
                            continue;
                        }
                    }

                    result = gridItem.PropertyDescriptor?.GetValue(result);
                    if (result == null)
                        return null;
                }

                return result;
            }

            #endregion

            PropertyDescriptor? descriptor = grdProperties.SelectedGridItem?.PropertyDescriptor;
            if (descriptor?.IsReadOnly != false)
                return;

            object? selectedObject = GetSelectedObject(grdProperties);
            if (selectedObject == null)
                return;

            if (descriptor.CanResetValue(selectedObject))
            {
                descriptor.ResetValue(selectedObject);
                return;
            }

            // If the property is not resettable (e.g. Image), we set it to its default value
            object? defaultValue = descriptor.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault() is DefaultValueAttribute d ? d.Value
                : descriptor.PropertyType.IsValueType ? Activator.CreateInstance(descriptor.PropertyType)
                : null;

            descriptor.SetValue(selectedObject, defaultValue);
            grdProperties.Refresh();
        }

        private void grdProperties_SelectedObjectsChanged(object sender, EventArgs e)
        {
            var selectedObjects = grdProperties.SelectedObjects;
            lblSelection.Text = @$"{(selectedObjects.Length == 1 ? (selectedObjects[0] as Control)?.Name ?? grdProperties.SelectedObject : $"{selectedObjects.Length} controls selected")}";
        }

        #endregion

        #endregion
    }
}
