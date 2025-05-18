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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Test.Forms
{
    internal partial class ControlsTestBaseForm : Form
    {
        #region Constants

        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int WM_LBUTTONDOWN = 0x201;

        #endregion

        #region Properties

        [DefaultValue("Click the items to see their properties")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string InstructionsText
        {
            get => lblInstuction.Text;
            set => lblInstuction.Text = value;
        }

        #endregion

        #region Constructors

        public ControlsTestBaseForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override void WndProc(ref Message m)
        {
            static Control FindControl(Control parent, Point cursorPosition)
            {
                if (!parent.HasChildren)
                    return parent;

                Control? child = parent.GetChildAtPoint(parent.PointToClient(cursorPosition), GetChildAtPointSkip.Invisible);
                if (child == null)
                    return parent;

                return FindControl(child, cursorPosition);
            }

            switch (m.Msg)
            {
                case WM_LBUTTONDOWN: // when clicking over the disabled pnlTestArea (so the form gets the mouse down event)
                case WM_MOUSEACTIVATE when (m.LParam.ToInt32() >> 16) == WM_LBUTTONDOWN: // when clicking over a child control, even disabled ones
                    Control child = FindControl(this, Cursor.Position);
                    if (child != null && child != grdProperties && !grdProperties.Contains(child) && grdProperties.SelectedObject != child)
                        grdProperties.SelectedObject = child;
                    break;
            }

            base.WndProc(ref m);
        }

        #endregion

        #region Event handlers

        private void miResetValue_Click(object sender, EventArgs e)
        {
            object selectedObject = grdProperties.SelectedObject;
            if (selectedObject == null)
                return;

            PropertyDescriptor descriptor = grdProperties.SelectedGridItem?.PropertyDescriptor;
            if (descriptor == null)
                return;

            if (descriptor.CanResetValue(selectedObject))
            {
                descriptor.ResetValue(selectedObject);
                return;
            }

            if (descriptor.IsReadOnly)
                return;

            // If the property is not resettable (e.g. Image), we set it to its default value
            object defaultValue = descriptor.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault() is DefaultValueAttribute d ? d.Value
                : descriptor.PropertyType.IsValueType ? Activator.CreateInstance(descriptor.PropertyType)
                : null;

            descriptor.SetValue(selectedObject, defaultValue);
            grdProperties.Refresh();
        }

        #endregion

        #endregion
    }
}
