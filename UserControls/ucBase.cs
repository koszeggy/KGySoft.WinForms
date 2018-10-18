using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using KGySoft.ComponentModel;
using KGySoft.Libraries.Language;

namespace KGySoft.Controls
{
    // Leszármaztatott controlban a következőket kell a működéséhez beállítani:
    // - MainControl csak olvasható property felülírása
    // - ReadOnly és belső control Enabled változásnál ResetColor hívás
    // - Belső control Value változásnál (mind szerkesztés közben mind külső módosításnál) ResetColor hívás, ha AutoSaveValue van

    /// <summary>
    /// Base class of custom user controls.
    /// </summary>
    [ToolboxItem(false)]
    public partial class ucBase: UserControl, ICustomTranslated, IReadOnlyCapable
    {
        #region Fields

        private string toolTipText = String.Empty;
        private object savedValue = null;
        private Color colorEnabled = SystemColors.Window;
        private Color colorDisabled = SystemColors.Control;
        private Color colorModified = Color.Gold;
        private Color colorControlTextEnabled = SystemColors.ControlText;
        private Color colorControlTextDisabled = SystemColors.ControlDarkDark;
        private bool autoSaveValue = true;
        private bool translationEnabled = true;
        private TypeDescriptionProvider localizableProvider;
        private readonly CommandBindingsCollection commandBindings = new WinformsCollandBindingsCollection();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the command bindings of this form.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CommandBindingsCollection CommandBindings => commandBindings;

        /// <summary>
        /// Gets or sets the ReadOnly state of the inner content.
        /// <remarks>Must override! Setter must call base setter.</remarks>
        /// </summary>
        [
            Category("ucBase"),
            Description("Gets or sets the ReadOnly state of the inner content."),
            DefaultValue(true)
        ]
        public virtual bool ReadOnly
        {
            get { return true; }
            set { ResetColor(); }
        }

        /// <summary>
        /// Tooltip text of the control.
        /// </summary>
        [
            Category("ucBase"),
            Description("Tooltip text of the control."),
            DefaultValue(""),
            Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))
        ]
        public string ToolTip
        {
            get
            {
                return toolTipText;
            }
            set
            {
                toolTipText = value;
                SetToolTip(MainControl, toolTipText);
            }
        }

        /// <summary>
        /// Gets or sets the associated value of the control.
        /// This can be a text or number or anything else in derived controls.
        /// <remarks>Must override!</remarks>
        /// </summary>
        [
            Category("ucBase"),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual object ControlValue
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// Returns an earlier saved value of the control.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public object SavedValue
        {
            get { return savedValue; }
        }

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
        /// <remarks>Must override!</remarks>
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected virtual Control MainControl
        {
            get { return null; }
        }

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
            get { return autoSaveValue; }
            set { autoSaveValue = value; }
        }

        /// <summary>
        /// BackColor of the inner <see cref="MainControl"/> when control is Enabled and not ReadOnly.
        /// </summary>
        [Category("ucBase")]
        [Description("BackColor of the inner main control when control is Enabled and not ReadOnly.")]
        [DefaultValue(typeof(Color), "Window")]
        public virtual Color ColorEnabled
        {
            get { return colorEnabled; }
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
            get { return colorDisabled; }
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
            get { return colorModified; }
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
            get { return colorControlTextEnabled; }
            set
            {
                colorControlTextEnabled = value;
                ResetColor();
            }
        }

        /// <summary>
        /// ForeColor of the inner <see cref="MainControl"/> when control is not Enabled.
        /// <remarks>
        /// This is not supported in WindowsForms by default: The ForeColor of a disabled control is
        /// always gray. The controls that support coloring in disabled state must implement the
        /// <see cref="IDisabledColorCapable"/> interface and then setting this property will have effect.
        /// </remarks>
        /// </summary>
        [Category("ucBase")]
        [Description("Fore color of the inner main control when control is not Enabled. May not work with every controls.")]
        [DefaultValue(typeof(Color), "ControlDarkDark")]
        public virtual Color ColorControlTextDisabled
        {
            get { return colorControlTextDisabled; }
            set
            {
                colorControlTextDisabled = value;
                ResetColor();
            }
        }

        /// <summary>
        /// Gets or sets whetther translation of this control is enabled or not.
        /// </summary>
        [Category("ucBase")]
        [Description("Gets or sets whether translation of this control is enabled or not.")]
        [DefaultValue(true)]
        public bool TranslationEnabled
        {
            get { return translationEnabled; }
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
            this.EnabledChanged += new System.EventHandler(this.ucBase_EnabledChanged);
            this.Load += new System.EventHandler(this.ucBase_Load);
            this.BindingContextChanged += new EventHandler(ucBase_BindingContextChanged);
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
        /// Clearing the content of <see cref="MainControl"/>.
        /// <remarks>Must override! When override, call base.Clear()!</remarks>
        /// </summary>
        public virtual void Clear()
        {
            ResetColor();
        }

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
        /// <remarks>Do not use this feature if the control is binded to a business object that can do this, too!</remarks>
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

        public override string ToString()
        {
            return String.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }

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
                if (!IsModified && MainControl.BackColor != colorEnabled)
                    MainControl.BackColor = colorEnabled;
                else if (IsModified && MainControl.BackColor != colorModified)
                    MainControl.BackColor = colorModified;
            }
            // BackColor when control is not Enabled or is ReadOnly
            else if ((!Enabled || !MainControl.Enabled || ReadOnly))
            {
                IDisabledColorCapable dcc = MainControl as IDisabledColorCapable;
                if (dcc != null)
                {
                    if (dcc.DisabledBackColor != colorDisabled)
                        dcc.DisabledBackColor = colorDisabled;
                }
                else if (MainControl.BackColor != colorDisabled)
                    MainControl.BackColor = colorDisabled;
            }

            // TextColor in Enabled state (also ReadOnly)
            if (Enabled && MainControl.Enabled && MainControl.ForeColor != colorControlTextEnabled)
                MainControl.ForeColor = colorControlTextEnabled;
            // TextColor in disabled state (ReadOnly state is indifferent)
            else if (!Enabled || !MainControl.Enabled)
            {
                IDisabledColorCapable dcc = MainControl as IDisabledColorCapable;
                if (dcc != null)
                {
                    if (dcc.DisabledForeColor != colorControlTextDisabled)
                        dcc.DisabledForeColor = colorControlTextDisabled;
                }
                else if (MainControl.ForeColor != colorControlTextDisabled)
                    MainControl.ForeColor = colorControlTextDisabled;
            }
        }

        /// <summary>
        /// Sets the ToolTip text for an inner control.
        /// </summary>
        protected void SetToolTip(Control control, string toolTipText)
        {
            if (DesignMode || MainControl == null)
                return;
            baseToolTip.SetToolTip(control, toolTipText);
        }

        /// <summary>
        /// When overridden, may handle special translation of the control.
        /// <remarks>Will not be called when <see cref="TranslationEnabled"/> is false.</remarks>
        /// </summary>
        /// <param name="translationFinished">If returns true in overridden methods, no further translation will be performed on child elements.</param>
        /// </returns>
        protected virtual void TranslateContent(ref bool translationFinished)
        {
        }

        #endregion

        #region Handled events

        private void ucBase_EnabledChanged(object sender, EventArgs e)
        {
            ResetColor();
        }

        private void ucBase_Load(object sender, EventArgs e)
        {
            ResetColor();
        }

        void ucBase_BindingContextChanged(object sender, EventArgs e)
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
