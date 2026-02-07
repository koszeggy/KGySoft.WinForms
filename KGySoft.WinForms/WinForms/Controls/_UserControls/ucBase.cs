#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucBase.cs
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
using System.Drawing.Design;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.Libraries.Language;

#endregion


namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Base class of custom user controls.
    /// </summary>
    /// <remarks>
    /// <para>When deriving from this control, the following steps must be done:
    /// <list>
    /// <item>Override <see cref="MainControl"/>, <see cref="ControlValue"/> and <see cref="ReadOnly"/> properties.</item>
    /// <item>Override <see cref="Clear"/>.</item>
    /// <item>When the <see cref="Control.Enabled"/> or <c>ReadOnly</c> properties of the inner control changes, call <see cref="ResetColor"/>.</item>
    /// <item>When <see cref="AutoSaveValue"/> is <see langword="true"/>, call <see cref="ResetColor"/> when the effective value of <see cref="MainControl"/> changes.</item>
    /// </list></para>
    /// </remarks>
    [ToolboxItem(false)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Compatibility, legacy code")]
    [Obsolete("This class and its descendants are not recommended to use anymore. They may not be maintained or can be even removed in the future.")]
    public partial class ucBase: UserControl, ICustomTranslated, IReadOnlyCapable
    {
        #region Fields

        private string toolTipText = String.Empty;
        private object? savedValue;
        private Color colorEnabled = SystemColors.Window;
        private Color colorDisabled = SystemColors.Control;
        private Color colorModified = Color.Gold;
        private Color colorControlTextEnabled = SystemColors.ControlText;
        private Color colorControlTextDisabled = SystemColors.ControlDarkDark;
        private bool autoSaveValue = true;
        private bool translationEnabled = true;
        private readonly CommandBindingsCollection commandBindings = new WinFormsCommandBindingsCollection();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the command bindings of this control. The <see cref="O:KGySoft.ComponentModel.CommandBindingsCollection.Add">Add</see> methods also add
        /// the <see cref="PropertyCommandStateUpdater"/> to the created bindings.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CommandBindingsCollection CommandBindings => commandBindings;

        /// <summary>
        /// Gets or sets the read-only state of the inner content.
        /// Should be overridden in a derived class. The base implementation always returns <see langword="true"/>.
        /// </summary>
        [Category("ucBase")]
        [Description("Gets or sets the ReadOnly state of the inner content.")]
        [DefaultValue(true)]
        [SuppressMessage("ReSharper", "ValueParameterNotUsed", Justification = "Intended, base implementation")]
        public virtual bool ReadOnly
        {
            get => true;
            set => ResetColor();
        }

        /// <summary>
        /// Gets or sets the tooltip text of the control.
        /// </summary>
        [Category("ucBase")]
        [Description("Tooltip text of the control.")]
        [DefaultValue("")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ToolTip
        {
            get => toolTipText;
            set
            {
                toolTipText = value;
                SetToolTip(MainControl, toolTipText);
            }
        }

        /// <summary>
        /// Gets or sets the associated value of the control.
        /// This can be a text or number or anything else in derived controls. The base implementation returns <see langword="null"/>.
        /// Should be overridden in a derived class.
        /// </summary>
        [Category("ucBase")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual object? ControlValue
        {
            get => null;
            set { }
        }

        /// <summary>
        /// Returns an earlier saved value of the control.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public object? SavedValue => savedValue;

        /// <summary>
        /// Tells whether the content of the control is modified since the last <see cref="SaveValue"/> call.
        /// <remarks>Do not use this property for checking the state of business objects!</remarks>
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsModified
        {
            get
            {
                if (savedValue == null)
                    return false;

                return !Equals(savedValue, ControlValue);
            }
        }

        /// <summary>
        /// Returns the main inner control of the user control.
        /// Should be overridden in a derived class.
        /// The base implementation returns <see langword="null"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected virtual Control? MainControl => null;

        /// <summary>
        /// Gets or sets whether the control should automatically call <see cref="SaveValue"/> to
        /// display its modified state in <see cref="ColorModified"/> color when data binding context has
        /// been changed. This automatism works only when control is used with data binding and bound
        /// property affects <see cref="ControlValue"/>.
        /// </summary>
        [Category("ucBase")]
        [Description("Gets or sets whether the control should automatically call SaveValue to " +
            "display its modified state in ColorModified color when data binding context has " +
            "been changed. This automatism works only when control is used with data binding and bound " +
            "property affects ControlValue.")]
        [DefaultValue(true)]
        public bool AutoSaveValue
        {
            get => autoSaveValue;
            set => autoSaveValue = value;
        }

        /// <summary>
        /// BackColor of the inner <see cref="MainControl"/> when control is Enabled and not ReadOnly.
        /// </summary>
        [Category("ucBase")]
        [Description("BackColor of the inner main control when control is Enabled and not ReadOnly.")]
        [DefaultValue(typeof(Color), "Window")]
        public virtual Color ColorEnabled
        {
            get => colorEnabled;
            set
            {
                colorEnabled = value;
                ResetColor();
            }
        }

        /// <summary>
        /// BackColor of the inner <see cref="MainControl"/> when control is not Enabled or is ReadOnly.
        /// </summary>
        [Category("ucBase")]
        [Description("BackColor of the inner main control when control is not Enabled or is ReadOnly.")]
        [DefaultValue(typeof(Color), "Control")]
        public virtual Color ColorDisabled
        {
            get => colorDisabled;
            set
            {
                colorDisabled = value;
                ResetColor();
            }
        }

        /// <summary>
        /// BackColor of the modified inner <see cref="MainControl"/> when control is Enabled and not ReadOnly.
        /// <remarks>To use this feature call <see cref="SaveValue"/> after setting an initial value in the control.</remarks>
        /// </summary>
        [Category("ucBase")]
        [Description("BackColor of the modified inner main control when control is Enabled and not ReadOnly.")]
        [DefaultValue(typeof(Color), "Gold")]
        public virtual Color ColorModified
        {
            get => colorModified;
            set
            {
                colorModified = value;
                ResetColor();
            }
        }

        /// <summary>
        /// ForeColor of the inner <see cref="MainControl"/> when control is Enabled.
        /// </summary>
        /// <remarks>In descendants, if the <see cref="MainControl"/> is an input control
        /// consider modifying the default value to WindowText.</remarks>
        [Category("ucBase")]
        [Description("Fore color of the inner main control when control is Enabled.")]
        [DefaultValue(typeof(Color), "ControlText")]
        public virtual Color ColorControlTextEnabled
        {
            get => colorControlTextEnabled;
            set
            {
                colorControlTextEnabled = value;
                ResetColor();
            }
        }

        /// <summary>
        /// ForeColor of the inner <see cref="MainControl"/> when control is not Enabled.
        /// </summary>
        /// <remarks>
        /// This is not supported in WindowsForms by default: The ForeColor of a disabled control is
        /// always gray. The controls that support coloring in disabled state must implement the
        /// <see cref="ISupportsDisabledColor"/> interface and then setting this property will have effect.
        /// </remarks>
        [Category("ucBase")]
        [Description("Fore color of the inner main control when control is not Enabled. May not work with every controls.")]
        [DefaultValue(typeof(Color), "ControlDarkDark")]
        public virtual Color ColorControlTextDisabled
        {
            get => colorControlTextDisabled;
            set
            {
                colorControlTextDisabled = value;
                ResetColor();
            }
        }

        /// <summary>
        /// Gets or sets whether translation of this control is enabled or not.
        /// </summary>
        [Category("ucBase")]
        [Description("Gets or sets whether translation of this control is enabled or not.")]
        [DefaultValue(true)]
        public bool TranslationEnabled
        {
            get => translationEnabled;
            set
            {
                if (value == translationEnabled)
                    return;

                Language.MarkLocalizable(value, this);

                translationEnabled = value;
            }
        }

        #endregion

        #region Constructor, Dispose

        /// <summary>
        /// <see cref="ucBase"/> constructor. Public only for designer,
        /// not intended to instantiate an underived ucBase instance.
        /// </summary>
        public ucBase()
        {
            InitializeComponent();
            EnabledChanged += ucBase_EnabledChanged;
            Load += ucBase_Load;
            BindingContextChanged += ucBase_BindingContextChanged;
            Language.MarkLocalizable(true, this);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            EnabledChanged -= ucBase_EnabledChanged;
            Load -= ucBase_Load;
            BindingContextChanged -= ucBase_BindingContextChanged;
            if (disposing && (components != null))
            {
                components.Dispose();
                commandBindings.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Clears the content of <see cref="MainControl"/>.
        /// The base implementation only calls <see cref="ResetColor">ResetColor</see>.
        /// </summary>
        public virtual void Clear() => ResetColor();

        /// <summary>
        /// Stores the value of the control. This makes possible to sign when the value has been modified.
        /// See <see cref="IsModified"/> and <see cref="ColorModified"/> properties.
        /// </summary>
        public void SaveValue()
        {
            savedValue = ControlValue;
            ResetColor();
        }

        /// <summary>
        /// Restores the earlier saved value.
        /// <remarks>Do not use this feature if the control is bound to a business object that can do this, too!</remarks>
        /// </summary>
        public void RestoreSavedValue()
        {
            if (savedValue != null)
                ControlValue = savedValue;
            ResetColor();
        }

        /// <summary>
        /// Clears the saved value, so the control will not change its color when modified (like before calling <see cref="SaveValue"/>).
        /// </summary>
        public void ClearSavedValue()
        {
            savedValue = null;
            ResetColor();
        }

        /// <inheritdoc />
        public override string ToString() => String.IsNullOrEmpty(Name) ? base.ToString() : Name;

        #endregion

        #region Protected methods

        /// <summary>
        /// Resets the color of the control.
        /// <remarks>Call this method in overridden ReadOnly change, Enable change of <see cref="MainControl"/>
        /// and when the content of <see cref="MainControl"/> has been changed.
        /// </remarks>
        /// </summary>
        protected virtual void ResetColor()
        {
            if (MainControl == null)
                return;

            // BackColor when control is Enabled and not ReadOnly
            if (Enabled && MainControl.Enabled && !ReadOnly)
            {
                if (IsModified && MainControl.BackColor != colorModified)
                    MainControl.BackColor = colorModified;
                else if (MainControl is ISupportsDisabledColor sdc && sdc.EnabledBackColor != colorEnabled)
                    sdc.EnabledBackColor = colorEnabled;
                else if (MainControl.BackColor != colorEnabled)
                    MainControl.BackColor = colorEnabled;
            }
            // BackColor when control is not Enabled or is ReadOnly
            else if ((!Enabled || !MainControl.Enabled || ReadOnly))
            {
                if (MainControl is ISupportsDisabledColor sdc && sdc.DisabledBackColor != colorDisabled)
                    sdc.DisabledBackColor = colorDisabled;
                else if (MainControl.BackColor != colorDisabled)
                    MainControl.BackColor = colorDisabled;
            }

            // TextColor in Enabled state (also ReadOnly)
            if (Enabled && MainControl.Enabled)
            {
                if (MainControl is ISupportsDisabledColor sdc && sdc.EnabledForeColor != colorControlTextEnabled)
                    sdc.EnabledForeColor = colorControlTextEnabled;
                else if (MainControl.ForeColor != colorControlTextEnabled)
                    MainControl.ForeColor = colorControlTextEnabled;
            }
            // TextColor in disabled state (ReadOnly state is indifferent)
            else if (!Enabled || !MainControl.Enabled)
            {
                if (MainControl is ISupportsDisabledColor sdc && sdc.DisabledForeColor != colorControlTextDisabled)
                    sdc.DisabledForeColor = colorControlTextDisabled;
                else if (MainControl.ForeColor != colorControlTextDisabled)
                    MainControl.ForeColor = colorControlTextDisabled;
            }
        }

        /// <summary>
        /// Sets the ToolTip text for an inner control.
        /// </summary>
        [SuppressMessage("ReSharper", "ParameterHidesMember", Justification = "Renaming it would be a breaking change")]
        protected void SetToolTip(Control? control, string? toolTipText)
        {
            if (DesignMode || control == null)
                return;
            baseToolTip.SetToolTip(control, toolTipText);
        }

        /// <summary>
        /// When overridden, may handle special translation of the control.
        /// <remarks>Will not be called when <see cref="TranslationEnabled"/> is false.</remarks>
        /// </summary>
        /// <param name="translationFinished">If returns true in overridden methods, no further translation will be performed on child elements.</param>
        protected virtual void TranslateContent(ref bool translationFinished)
        {
        }

        #endregion

        #region Handled events

        private void ucBase_EnabledChanged(object? sender, EventArgs e)
        {
            ResetColor();
        }

        private void ucBase_Load(object? sender, EventArgs e)
        {
            ResetColor();
        }

        void ucBase_BindingContextChanged(object? sender, EventArgs e)
        {
            if (autoSaveValue && !DesignMode && DataBindings.Count > 0)
                SaveValue();
        }

        #endregion

        #region ICustomTranslated Members

        bool ICustomTranslated.TranslateControl(out bool translationFinished)
        {
            if (!translationEnabled)
            {
                translationFinished = true;
                return false;
            }
            ToolTip = Language.Translate(toolTipText);
            translationFinished = false;
            TranslateContent(ref translationFinished);
            return true;
        }

        #endregion
    }
}
