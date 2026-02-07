#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ContentPanel.cs
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

#endregion

namespace KGySoft.WinForms.Controls
{
	/// <summary>
	/// A panel that has no serializable properties and cannot be altered externally.
	/// </summary>
	[ToolboxItem(false)]
    [Designer(typeof(ScrollableControlDesigner))]
	[Obsolete("Used by the obsoleted ucCaptionedContainer")]
    [SuppressMessage("ReSharper", "ValueParameterNotUsed", Justification = "ContentPanel does not allow resetting most of its properties")]
    public sealed class ContentPanel: Panel
	{
		#region Overridden events

		/// <inheritdoc cref="Panel.AutoSizeChanged"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public new event EventHandler AutoSizeChanged
		{
			add => base.AutoSizeChanged += value;
            remove => base.AutoSizeChanged -= value;
        }

        /// <inheritdoc cref="Control.DockChanged"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler DockChanged
		{
			add => base.DockChanged += value;
            remove => base.DockChanged -= value;
        }

        /// <inheritdoc cref="Control.LocationChanged"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public new event EventHandler LocationChanged
		{
			add => base.LocationChanged += value;
            remove => base.LocationChanged -= value;
        }

        /// <inheritdoc cref="Control.TabIndexChanged"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler TabIndexChanged
		{
			add => base.TabIndexChanged += value;
            remove => base.TabIndexChanged -= value;
        }

        /// <inheritdoc cref="Control.TabStopChanged"/>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler TabStopChanged
		{
			add => base.TabStopChanged += value;
            remove => base.TabStopChanged -= value;
        }

        /// <inheritdoc cref="Control.VisibleChanged"/>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler VisibleChanged
		{
			add => base.VisibleChanged += value;
            remove => base.VisibleChanged -= value;
        }

		#endregion

		#region Properties

        /// <inheritdoc cref="Control.Anchor"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new AnchorStyles Anchor
		{
			get => base.Anchor;
            set { }
		}

		/// <summary>
		/// Always returns <see langword="true"/>.
		/// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AutoSize
		{
			get => true;
            set { }
		}

        /// <inheritdoc cref="Panel.AutoSizeMode"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Localizable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override AutoSizeMode AutoSizeMode
		{
			get => base.AutoSizeMode;
            set { }
		}

        /// <inheritdoc cref="Panel.BorderStyle"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new BorderStyle BorderStyle
		{
			get => base.BorderStyle;
            set { }
		}

        /// <inheritdoc/>
		protected override Padding DefaultMargin => new(0, 0, 0, 0);

        /// <inheritdoc cref="Control.Dock"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new DockStyle Dock
		{
			get => base.Dock;
            set { }
		}

        /// <inheritdoc cref="ScrollableControl.DockPadding"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new DockPaddingEdges DockPadding => base.DockPadding;

        /// <inheritdoc cref="Control.Location"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public new Point Location
		{
			get => base.Location;
            set { }
		}

        /// <inheritdoc cref="Control.MaximumSize"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new Size MaximumSize
		{
			get => base.MaximumSize;
            set { }
		}

        /// <inheritdoc cref="Control.MinimumSize"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new Size MinimumSize
		{
			get => base.MinimumSize;
            set { }
		}

        /// <inheritdoc cref="Control.Name"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new string Name
		{
			get => base.Name;
            set { }
		}

        /// <inheritdoc cref="Control.Parent"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new Control? Parent
		{
			get => base.Parent;
            set { }
		}

        /// <inheritdoc cref="Control.Size"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new Size Size
		{
			get => base.Size;
            set { }
		}

        /// <inheritdoc cref="Control.TabIndex"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new int TabIndex
		{
			get => base.TabIndex;
            set => base.TabIndex = value;
        }

        /// <inheritdoc cref="Control.TabStop"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new bool TabStop
		{
			get => base.TabStop;
            set => base.TabStop = value;
        }

        /// <inheritdoc cref="Control.Visible"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool Visible
		{
			get => base.Visible;
            set => base.Visible = value;
        }
		#endregion

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="ContentPanel"/> instance that will replace the original
		/// <see cref="Panel"/> given in <paramref name="panelToClone"/> parameter. An original
		/// panel is required because that can be placed in designer while content panel does
		/// not save its properties.
		/// </summary>
		/// <param name="panelToClone"></param>
		public ContentPanel(Panel panelToClone)
		{
			base.Dock = panelToClone.Dock;
			base.Name = panelToClone.Name;
			base.TabIndex = 0;
			base.Parent = panelToClone.Parent;
			panelToClone.Parent = null;
		}

		#endregion
	}
}
