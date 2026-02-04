#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucCaptionedContainerDesigner.cs
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
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

#endregion

namespace KGySoft.WinForms.Controls.Design
{
    /// <summary>
    /// Designer of <see cref="ucCaptionedContainer"/> that makes possible to use <see cref="ucCaptionedContainer"/>
    /// as a container control in design time.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Compatibility, legacy code")]
    [Obsolete("Needed for the obsoleted ucCaptionedContainer")]
    internal sealed class ucCaptionedContainerDesigner : ParentControlDesigner
    {
        #region Fields

        private IDesignerHost? designerHost;

        #endregion

        #region Properties

        public override System.Collections.ICollection AssociatedComponents
        {
            get
            {
                List<Control> list = new List<Control>();
                foreach (Control control in ((ucCaptionedContainer)Control).PanelContent.Controls)
                {
                    list.Add(control);
                }
                return list;

            }
        }

        #endregion

        #region Methods

        #region Public Methods

        public override void Initialize(IComponent component)
        {
            if (component is not ucCaptionedContainer container)
                throw new InvalidOperationException("The ucCaptionedContainerDesigner can be used only for user controls that are derived from ucCaptionedBase class.");
            base.Initialize(container);
            AutoResizeHandles = true;
            EnableDesignMode(container.PanelContent, "ContentPanel");
            designerHost = (IDesignerHost?)component.Site?.GetService(typeof(IDesignerHost));
        }

        public override bool CanParent(Control control) => false;

        public override int NumberOfInternalControlDesigners() => 1;

        public override ControlDesigner? InternalControlDesigner(int internalControlIndex)
        {
            Control panel = ((ucCaptionedContainer)Control).PanelContent;
            switch (internalControlIndex)
            {
                case 0:
                    return designerHost?.GetDesigner(panel) as ControlDesigner;
                default:
                    return null;
            }
        }

        #endregion

        #region Protected Methods

        protected override Control GetParentForComponent(IComponent component)
        {
            return ((ucCaptionedContainer)Control).PanelContent;
        }

        protected override IComponent[]? CreateToolCore(ToolboxItem tool, int x, int y, int width, int height, bool hasLocation, bool hasSize)
        {
            if (designerHost?.GetDesigner(((ucCaptionedContainer)Control).PanelContent) is ParentControlDesigner panelDesigner)
                InvokeCreateTool(panelDesigner, tool);
            return null;
        }

        #endregion

        #endregion
    }
}
