#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucCustomSelector.cs
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.CoreLibraries;
using KGySoft.Libraries.Language;
using KGySoft.Reflection;
using KGySoft.WinForms.Forms;

#endregion

#region Suppressions

#if !NETCOREAPP3_0_OR_GREATER
#pragma warning disable CS8602 // Dereference of a possibly null reference. - analyzer false alarm for .NET Framework
#endif

#if NETCOREAPP3_0 || NETCOREAPP3_1
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type - inconsistent nullability annotations on different platforms
#pragma warning disable CS8604 // Possible null reference argument - inconsistent nullability annotations on different platforms
#endif

#endregion

namespace KGySoft.WinForms.Controls
{

    /// <summary>
    /// A selector control that can be used for general purposes.
    /// </summary>
    [DefaultBindingProperty("Value")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucCustomSelector: ucCaptionedBase
    {
        #region Fields

        private SelectorButtons buttons = SelectorButtons.Browse;
        private Button btnClearSelection = null!;
        private Button btnSelectAll = null!;
        private Button btnSelectNone = null!;
        private Button btnBrowse = null!;
        private Button btnEditor = null!;
        private Button btnNew = null!;
        private FlatStyle buttonStyle = FlatStyle.Standard;
        private object? value = ControlExtensions.NotSelectedValue;
        private SelectorStates state = SelectorStates.NotSelected;
        private bool readOnly;
        private bool textEditable = true;
        private bool autoFind = true;
        private bool autoImage;
        private bool deleteContent;
        private RelevantControlValues relevantControlValue = RelevantControlValues.Value;
        private bool checkChangedOnLeave;

        private string toolTipClearSelection = String.Empty;
        private string toolTipSelectAll = String.Empty;
        private string toolTipSelectNone = String.Empty;
        private string toolTipBrowse = String.Empty;
        private string toolTipEditor = String.Empty;
        private string toolTipNew = String.Empty;
        private string textNotSelected = ControlExtensions.NotSelectedText;
        private string textAllSelected = ControlExtensions.AllSelectedText;
        private string textNoneSelected = ControlExtensions.NoneSelectedText;
        private object valueNotSelected = ControlExtensions.NotSelectedValue;
        private object valueAllSelected = ControlExtensions.AllSelectedValue;
        private object valueNoneSelected = ControlExtensions.NoneSelectedValue;

        #endregion

        #region Properties

        #region Mandatory overridden properties

        /// <summary>
        /// Gets the wrapped <see cref="AdvancedComboBox"/> control.
        /// </summary>
        protected override Control MainControl => cmbCombo;

        /// <summary>
        /// Gets or sets the relevant property of the control (<see cref="Value"/> or <see cref="Text"/>,
        /// depends on <see cref="RelevantControlValue"/>.
        /// </summary>
        public override object? ControlValue
        {
            get
            {
                switch (relevantControlValue)
                {
                    case RelevantControlValues.Value:
                        return Value;
                    case RelevantControlValues.Text:
                        return Text;
                    case RelevantControlValues.State:
                        return State;
                    default:
                        return null;
                }
            }
            set => SetControlValue(value);
        }

        /// <summary>
        /// Sets the <see cref="SelectorStates.NotSelected"/> state.
        /// </summary>
        public override void Clear()
        {
            Value = valueNotSelected;
            base.Clear();
        }

        #endregion

        #region New properties

        /// <summary>
        /// Gets or sets the relevant control value for supporting <see cref="ucBase"/> features of
        /// saving/restoring value and marking the control as modified.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the relevant control value for supporting ucBase features of saving/restoring value and marking the control as modified.")]
        [DefaultValue(typeof(RelevantControlValues), "Value")]
        public RelevantControlValues RelevantControlValue
        {
            get => relevantControlValue;
            set => relevantControlValue = value;
        }

        /// <summary>
        /// Gets or sets the associated value of the selector control.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object? Value
        {
            get => value;
            set => SetValue(value);
        }

        /// <summary>
        /// Gets or sets the value of the 'NotSelected' <see cref="State"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object ValueNotSelected
        {
            get => valueNotSelected;
            set
            {
                if (!Equals(value, valueNotSelected) && !Equals(value, valueAllSelected) && !Equals(value, valueNoneSelected))
                {
                    valueNotSelected = value;
                    if (state == SelectorStates.NotSelected)
                        SetValue(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the 'All' <see cref="State"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object ValueAllSelected
        {
            get => valueAllSelected;
            set
            {
                if (!Equals(value, valueNotSelected) && !Equals(value, valueAllSelected) && !Equals(value, valueNoneSelected))
                {
                    valueAllSelected = value;
                    if (state == SelectorStates.All)
                        SetValue(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the 'None' <see cref="State"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object ValueNoneSelected
        {
            get => valueNoneSelected;
            set
            {
                if (!Equals(value, valueNotSelected) && !Equals(value, valueAllSelected) && !Equals(value, valueNoneSelected))
                {
                    valueNoneSelected = value;
                    if (state == SelectorStates.None)
                        SetValue(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value specifying the style of the combo box.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets a value specifying the style of the combo box.")]
        [DefaultValue(typeof(ComboBoxStyle), "Simple")]
        public ComboBoxStyle DropDownStyle
        {
            get => cmbCombo.DropDownStyle;
            set => cmbCombo.DropDownStyle = value;
        }

        /// <summary>
        /// Gets or sets whether the enabled combo box should be drawn by the system in <see cref="ComboBoxStyle.DropDownList"/> mode.
        /// If this property is <see langword="false"/>, then drop-down list appearance will be the same as in case of <see cref="ComboBoxStyle.DropDown"/> mode
        /// even with Windows Vista/Windows 7 themes.
        /// </summary>
        [Category("AdvancedComboBox")]
        [Description("Gets or sets whether the enabled combo box should be drawn by the system in DropDownList mode. " +
            "If this property is false, then drop-down list appearance will be the same as in case of DropDown mode " +
            "even with Windows Vista/Windows 7 themes.")]
        [DefaultValue(true)]
        public bool SystemDrawDropDownListMode
        {
            get => cmbCombo.SystemDrawDropDownListMode;
            set => cmbCombo.SystemDrawDropDownListMode = value;
        }

        /// <summary>
        /// Gets or sets the state of the selector control.
        /// If <see cref="Value"/> has meaning in the used scenario, then set the <see cref="Value"/> property instead.
        /// Setting <see cref="SelectorStates.ValueSet"/> sets <see cref="ControlExtensions.UndefinedValue"/> to the <see cref="Value"/> property.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the state of the selector control.")]
        [DefaultValue(typeof(SelectorStates), "NotSelected")]
        [RefreshProperties(RefreshProperties.All)]
        public SelectorStates State
        {
            get => state;
            set => SetState(value);
        }

        /// <summary>
        /// Visible buttons.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Visible buttons.")]
        [DefaultValue(typeof(SelectorButtons), "Browse")]
        [TypeConverter(typeof(FlagsEnumConverter))]
        public SelectorButtons Buttons
        {
            get => buttons;
            set
            {
                buttons = value;
                RefreshActionPanel();
            }
        }

        /// <summary>
        /// Style of the buttons.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Style of the buttons.")]
        [DefaultValue(typeof(FlatStyle), "Standard")]
        public FlatStyle ButtonStyle
        {
            get => buttonStyle;
            set
            {
                buttonStyle = value;
                RefreshActionPanel();
            }
        }

        /// <summary>
        /// The image of the selector control.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("The image of the selector control.")]
        [DefaultValue(null)]
        public Image? Image
        {
            get => autoImage ? null : pbImage.Image;
            set
            {
                pbImage.Image = value;
                pbImage.Visible = value != null;
                autoImage = false;
            }
        }

        /// <summary>
        /// Size mode of the image of the selector.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Size mode of the image of the selector.")]
        [DefaultValue(typeof(PictureBoxSizeMode), "CenterImage")]
        public PictureBoxSizeMode ImageSizeMode
        {
            get => pbImage.SizeMode;
            set
            {
                if (value != PictureBoxSizeMode.AutoSize)
                    pbImage.SizeMode = value;
            }
        }

        /// <summary>
        /// Border style of the image of the selector.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Border style of the image of the selector.")]
        [DefaultValue(typeof(BorderStyle), "None")]
        public BorderStyle ImageBorderStyle
        {
            get => pbImage.BorderStyle;
            set => pbImage.BorderStyle = value;
        }

        /// <summary>
        /// Gets or sets whether the image of the selector control should
        /// display automatically an image depending on the state of the control.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets whether the image of the selector control should display automatically an image depending on the state of the control.")]
        [DefaultValue(false)]
        public bool AutoImage
        {
            get => autoImage;
            set
            {
                autoImage = value;
                if (!value)
                    Image = null;
                else
                    RefreshImage();
            }
        }

        /// <summary>
        /// Gets or sets whether the text field can be edited manually.
        /// If the selector is not ReadOnly but this property is false,
        /// then the control value can be edited only via the selector buttons.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets whether the text field can be edited manually. If the selector is not ReadOnly but this property is false, then the control value can be edited only via the selector buttons.")]
        [DefaultValue(true)]
        public virtual bool TextEditable
        {
            get => textEditable;
            set
            {
                textEditable = value;
                cmbCombo.ReadOnly = readOnly || !value;
                ResetColor();
            }
        }

        /// <summary>
        /// Gets or sets whether <see cref="Value"/> should be auto-calculated based on the typed text.
        /// If the value of this property is <see langword="false"/>, then <see cref="Text"/> will be simply assigned to <see cref="Value"/>
        /// on leaving the control. If the property is <see langword="true"/>, then <see cref="Value"/> is calculated by
        /// the <see cref="AutoFind"/> event or by the <see cref="DefaultAutoFind">DefaultAutoFind</see> method if the <see cref="AutoFind"/> event is not subscribed.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets whether Value should be auto-calculated based on the typed text. " +
            "If the value of this property is false, then Text will be simply assigned to Value " +
            "on leaving the control. If the property is true, then Value is calculated by " +
            "the AutoFind event or by DefaultAutoFind method if the AutoFind event is not subscribed.")]
        [DefaultValue(true)]
        public virtual bool AutoFindText
        {
            get => autoFind;
            set => autoFind = value;
        }

        /// <summary>
        /// Gets or sets the text of the inner <see cref="ComboBox"/>. Can be set only in <see cref="SelectorStates.ValueSet"/>&#160;<see cref="State"/>. 
        /// For data binding using <see cref="Value"/> property is preferable.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting Text does not change <see cref="Value"/> (though at runtime if <see cref="AutoFindText"/> is true, then 
        /// <see cref="AutoFind"/> may set Value and Text).
        /// </para>
        /// <para>
        /// If <see cref="Value"/> has meaning in a scenario (<see cref="RelevantControlValue"/> is Value), then <see cref="Text"/> should never be set
        /// directly from code. Text of a value can be returned by overriding the <see cref="GetTextByValue">GetTextByValue</see> method.
        /// </para>
        /// <para>
        /// If you need to set both <see cref="Value"/> and <see cref="Text"/> without raising events
        /// use the <see cref="Assign"/> property.
        /// </para>
        /// </remarks>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the text of the inner ComboBox. Can be set only in ValueSet state. For data binding using Value property is preferable. "
            + "Setting Text property does not change Value, therefore if RelevantControlValue is Value, then primarily Value property should be set.")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
        [RefreshProperties(RefreshProperties.All)]
        [AllowNull]
        public override string Text
        {
            get => cmbCombo.Text;
            set
            {
                // this is for prevent Text to be set "ucCustomSelector1" or similar
                // in design time.
                if ((DesignMode && value == ToString().Split(' ')[0])
                    || state != SelectorStates.ValueSet)
                {
                    return;
                }

                SetText(value);
            }
        }

        /// <summary>
        /// Gets or sets the read-only state for the control. This disables most of the
        /// buttons, too. To disable manual text editing only, use the <see cref="TextEditable"/> property.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the read-only state for the control. This disables most of the buttons, too. To disable manual text editing only, use the TextEditable property.")]
        public override bool ReadOnly
        {
            get => readOnly;
            set
            {
                readOnly = value;
                cmbCombo.ReadOnly = !textEditable || value;
                RefreshActionPanel();
                ResetColor();
            }
        }

        /// <summary>
        /// Gets or sets the tooltip text of the Clear Selection button.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the tooltip text of the Clear Selection button.")]
        [Browsable(true)]
        [DefaultValue("")]
        public string ToolTipClearSelection
        {
            get => toolTipClearSelection;
            set
            {
                toolTipClearSelection = value;
                SetToolTip(btnClearSelection, Language.Translate(value));
            }
        }

        /// <summary>
        /// Gets or sets the tooltip text of the Select All button.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the tooltip text of the Select All button.")]
        [Browsable(true)]
        [DefaultValue("")]
        public string ToolTipSelectAll
        {
            get => toolTipSelectAll;
            set
            {
                toolTipSelectAll = value;
                SetToolTip(btnSelectAll, Language.Translate(value));
            }
        }

        /// <summary>
        /// Gets or sets the tooltip text of the Select None button.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the tooltip text of the Select None button.")]
        [Browsable(true)]
        [DefaultValue("")]
        public string ToolTipSelectNone
        {
            get => toolTipSelectNone;
            set
            {
                toolTipSelectNone = value;
                SetToolTip(btnSelectNone, Language.Translate(value));
            }
        }

        /// <summary>
        /// Gets or sets the tooltip text of the Browse button.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the tooltip text of the Browse button.")]
        [Browsable(true)]
        [DefaultValue("")]
        public string ToolTipBrowse
        {
            get => toolTipBrowse;
            set
            {
                toolTipBrowse = value;
                SetToolTip(btnBrowse, Language.Translate(value));
            }
        }

        /// <summary>
        /// Gets or sets the tooltip text of the Editor button.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the tooltip text of the Editor button.")]
        [Browsable(true)]
        [DefaultValue("")]
        public string ToolTipEditor
        {
            get => toolTipEditor;
            set
            {
                toolTipEditor = value;
                SetToolTip(btnEditor, Language.Translate(value));
            }
        }

        /// <summary>
        /// Gets or sets the tooltip text of the New button.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the tooltip text of the New button.")]
        [Browsable(true)]
        [DefaultValue("")]
        public string ToolTipNew
        {
            get => toolTipNew;
            set
            {
                toolTipNew = value;
                SetToolTip(btnNew, Language.Translate(value));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Text"/> of the 'NotSelected' <see cref="State"/>.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the Text of the 'NotSelected' State.")]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [DefaultValue(ControlExtensions.NotSelectedText)]
        public string TextNotSelected
        {
            get => textNotSelected;
            set
            {
                textNotSelected = value;
                if (state == SelectorStates.NotSelected)
                    SetText(Language.Translate(value));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Text"/> of the 'All' <see cref="State"/>.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the Text of 'All' State.")]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [DefaultValue(ControlExtensions.AllSelectedText)]
        public string TextAllSelected
        {
            get => textAllSelected;
            set
            {
                textAllSelected = value;
                if (state == SelectorStates.All)
                    SetText(Language.Translate(value));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Text"/> of the 'None' <see cref="State"/>.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the Text of the 'None' State.")]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [DefaultValue(ControlExtensions.NoneSelectedText)]
        public string TextNoneSelected
        {
            get => textNoneSelected;
            set
            {
                textNoneSelected = value;
                if (state == SelectorStates.None)
                    SetText(Language.Translate(value));
            }
        }

        /// <summary>
        /// Gets the inner combo box.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("The inner AdvancedComboBox")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] // Do not change this! If something is needed, make a new property instead.
        [Browsable(false)]
        public AdvancedComboBox ComboBox => cmbCombo;

        /// <summary>
        /// Gets an object representing the collection of the items contained in the inner <see cref="AdvancedComboBox"/>.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets an object representing the collection of the items contained in the inner AdvancedComboBox.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [MergableProperty(false)]
        public ComboBox.ObjectCollection Items => cmbCombo.Items;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when <see cref="Value"/> has changed.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs when Value has changed.")]
        public event EventHandler? ValueChanged;

        /// <summary>
        /// Occurs when <see cref="State"/> has changed.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs when State has changed.")]
        public event EventHandler? StateChanged;

        /// <summary>
        /// Occurs when the 'Clear' button is clicked.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs when the 'Clear' button is clicked.")]
        public event EventHandler? ButtonClearClick;

        /// <summary>
        /// Occurs when the 'All' button is clicked.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs when the 'All' button is clicked.")]
        public event EventHandler? ButtonAllClick;

        /// <summary>
        /// Occurs when the 'None' button is clicked.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs when the 'None' button is clicked.")]
        public event EventHandler? ButtonNoneClick;

        /// <summary>
        /// Occurs when the 'Browse' button is clicked.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs when the 'Browse' button is clicked.")]
        public event EventHandler? ButtonBrowseClick;

        /// <summary>
        /// Occurs when the 'Edit' button is clicked.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs when the 'Edit' button is clicked.")]
        public event EventHandler? ButtonEditorClick;

        /// <summary>
        /// Occurs when the 'New' button is clicked.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs when the 'New' button is clicked.")]
        public event EventHandler? ButtonNewClick;

        /// <summary>
        /// Occurs on leaving the control after editing the text manually and when <see cref="AutoFindText"/> property is <see langword="true"/>.
        /// Can be used to calculate <see cref="Value"/> based on the typed text. If this event is not subscribed, then <see cref="DefaultAutoFind">DefaultAutoFind</see> will be called,
        /// which tries to find the element in <see cref="Items"/> or simply makes <see cref="Value"/> equal to <see cref="Text"/>.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs on leaving the control after editing the text manually and when AutoFindText property is true. " +
            "Can be used to calculate Value based on the typed text. If this event is not subscribed, then DefaultAutoFind will be called, " +
            "which tries to find the element in Items or simply makes Value equal to Text.")]
        public event EventHandler<AutoFindEventArgs>? AutoFind;

        /// <summary>
        /// Occurs when a displayed text of a value needs to be calculated. Does not occur when <see cref="Value"/> equals <see cref="ValueNotSelected"/>,
        /// <see cref="ValueAllSelected"/> or <see cref="ValueNoneSelected"/>.
        /// If not handled, then the calculated text will gain its value either by the defined <see cref="DataSource"/> or by the string representation of <see cref="Value"/>.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs when a displayed text of a value needs to be calculated. Does not occur when Value equals ValueNotSelected, " +
            "ValueAllSelected or ValueNoneSelected. If not handled, then the calculated text will gain its value either by the defined DataSource " +
            "or by the string representation of Value.")]
        public event EventHandler<CalculateTextEventArgs>? CalculateText;

        /// <summary>
        /// Occurs when the <see cref="Image"/> of the control is clicked.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs when the Image of the control is clicked.")]
        public event EventHandler? ImageClick;

        /// <summary>
        /// Occurs when the <see cref="Image"/> of the control is double-clicked.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs when the Image of the control is double-clicked.")]
        public event EventHandler? ImageDoubleClick;

        /// <summary>
        /// Occurs when the text of the inner textbox changes.
        /// </summary>
        [Browsable(true)]
        [Category("ucCustomSelector")]
        [Description("Occurs when the text of the inner textbox changes.")]
        public new event EventHandler? TextChanged
        {
            add => cmbCombo.TextChanged += value;
            remove => cmbCombo.TextChanged -= value;
        }

        /// <summary>
        /// Occurs on leaving the control when <see cref="Text"/> is different from the value when the control received focus.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Occurs on leaving the control when Text is different from the value when the control received focus.")]
        public event EventHandler? TextChangedOnLeave
        {
            add => cmbCombo.TextChangedOnLeave += value;
            remove => cmbCombo.TextChangedOnLeave -= value;
        }

        #endregion

        #region Constructor, Dispose

        /// <summary>
        /// Initializes a new instance of the <see cref="ucCustomSelector"/> class.
        /// </summary>
        public ucCustomSelector()
        {
            InitializeComponent();
            cmbCombo.DropDownStyle = ComboBoxStyle.Simple;

            cmbCombo.TextChangedOnLeave += cmbCombo_TextChangedOnLeave;
            cmbCombo.Enter += cmbCombo_Enter;
            cmbCombo.KeyPress += cmbCombo_KeyPress;
            cmbCombo.TextChanged += cmbCombo_TextChanged;
            pbImage.Click += pbImage_Click;
            pbImage.DoubleClick += pbImage_DoubleClick;
            cmbCombo.SelectedValueChanged += cmbCombo_SelectedValueChanged;
            cmbCombo.SelectedIndexChanged += cmbCombo_SelectedIndexChanged;

            CreateActionPanel();
            RefreshActionPanel();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            pbImage.Click -= pbImage_Click;
            pbImage.DoubleClick -= pbImage_DoubleClick;

            base.Dispose(disposing);
        }

        #endregion

        #region Methods

        #region Public methods

        #region Simple public methods

        /// <summary>
        /// Sets the value-text pair without raising events.
        /// </summary>
        /// <param name="value">The value to set in <see cref="Value"/>.</param>
        /// <param name="text">The text to set in <see cref="Text"/>.</param>
        /// <remarks>
        /// This can be useful for initializing.
        /// </remarks>
        [SuppressMessage("ReSharper", "ParameterHidesMember", Justification = "Public method, changing the name would be breaking")]
        public void Assign(object? value, string text)
        {
            if (value == null || value.In(valueNotSelected, valueAllSelected, valueNoneSelected))
            {
                Value = value;
                return;
            }

            state = SelectorStates.ValueSet;
            RefreshImage();
            this.value = value;
            Text = text;
        }

        #endregion

        #region New overridable public methods

        /// <summary>
        /// The default behaviour when clicking the 'Clear' button.
        /// Can be overridden and can be called from a handled <see cref="ButtonClearClick"/>.
        /// </summary>
        public virtual void DefaultClearClick()
        {
            Value = valueNotSelected;
        }

        /// <summary>
        /// The default behaviour when clicking the 'All' button.
        /// Can be overridden and can be called from a handled <see cref="ButtonAllClick"/>.
        /// </summary>
        public virtual void DefaultAllClick()
        {
            Value = valueAllSelected;
        }

        /// <summary>
        /// The default behaviour when clicking the 'None' button.
        /// Can be overridden and can be called from a handled <see cref="ButtonNoneClick"/>.
        /// </summary>
        public virtual void DefaultNoneClick()
        {
            Value = valueNoneSelected;
        }

        /// <summary>
        /// The default behaviour when clicking the 'Browse' button.
        /// Can be overridden and can be called from a handled <see cref="ButtonBrowseClick"/>.
        /// </summary>
        public virtual void DefaultBrowseClick()
        {
            string text = cmbCombo.Text;
            if (Dialogs.InputDialog(ref text) && text != cmbCombo.Text)
            {
                Value = text;
            }
        }

        /// <summary>
        /// The default behaviour when clicking the 'Edit' button.
        /// Can be overridden and can be called from a handled <see cref="ButtonEditorClick"/>.
        /// </summary>
        public virtual void DefaultEditorClick()
        {
            Dialogs.InfoMessage("This is an abstract default method for {0} button. " +
                "You should implement Button{0}Click event or inherit class {1} and override Default{0}Click method.",
                "Editor",
                GetType().Name);
        }

        /// <summary>
        /// The default behaviour when clicking the 'New' button.
        /// Can be overridden and can be called from a handled <see cref="ButtonNewClick"/>.
        /// </summary>
        public virtual void DefaultNewClick()
        {
            Dialogs.InfoMessage("This is an abstract default method for {0} button. " +
                "You should implement Button{0}Click event or inherit class {1} and override Default{0}Click method.",
                "New",
                GetType().Name);
        }

        #endregion

        #endregion

        #region Protected methods

        #region Simple protected methods

        /// <summary>
        /// Creates an action button.
        /// </summary>
        /// <param name="bmp">The image of the button.</param>
        /// <param name="toolTip">The tooltip of the button</param>
        /// <returns>The created button.</returns>
        protected Button CreateActionButton(Bitmap? bmp, string toolTip)
        {
            Button result = new Button();
            result.Size = new Size(23, 22);
            result.Margin = new Padding(0, 0, 0, 0);
            result.Padding = new Padding(0, 0, 0, 0);
            result.Image = bmp;
            SetToolTip(result, toolTip);
            result.TabStop = false;
            result.FlatStyle = buttonStyle;
            result.Visible = false;
            result.Click += SelectorButtonClick;
            pnlActionPanel.Controls.Add(result);
            return result;
        }

        /// <summary>
        /// If <see cref="AutoImage"/> is <see langword="true"/>, refreshes the <see cref="Image"/> property based on the current <see cref="State"/>.
        /// </summary>
        protected void RefreshImage()
        {
            if (!autoImage)
                return;
            pbImage.SizeMode = PictureBoxSizeMode.CenterImage;
            pbImage.Visible = true;
            switch (state)
            {
                case SelectorStates.NotSelected:
                    pbImage.Image = Properties.Resources.Clear;
                    break;
                case SelectorStates.All:
                    pbImage.Image = Properties.Resources.All;
                    break;
                case SelectorStates.None:
                    pbImage.Image = Properties.Resources.None;
                    break;
                case SelectorStates.ValueSet:
                    pbImage.Image = Properties.Resources.Edit;
                    break;
            }
        }

        #endregion

        #region Overridden methods

        /// <inheritdoc />
        protected override void ResetColor()
        {
            // BackColor when control is Enabled and not ReadOnly and TextEditable is true
            if (Enabled && MainControl.Enabled && !ReadOnly && TextEditable)
            {
                if (!IsModified && MainControl.BackColor != ColorEnabled)
                    MainControl.BackColor = ColorEnabled;
                else if (IsModified && MainControl.BackColor != ColorModified)
                    MainControl.BackColor = ColorModified;
            }
            // BackColor when control is not Enabled or is ReadOnly or TextEditable is false
            else if (!Enabled || !MainControl.Enabled || ReadOnly || !TextEditable)
            {
                if (cmbCombo.DisabledBackColor != ColorDisabled)
                    cmbCombo.DisabledBackColor = ColorDisabled;
            }

            // TextColor in Enabled state (also ReadOnly and not TextEditable)
            if (Enabled && MainControl.Enabled && MainControl.Enabled)
            {
                if (MainControl.ForeColor != ColorControlTextEnabled)
                    MainControl.ForeColor = ColorControlTextEnabled;
            }
            // TextColor in disabled state (ReadOnly state is indifferent)
            else
            {
                if (cmbCombo.DisabledForeColor != ColorControlTextDisabled)
                    cmbCombo.DisabledForeColor = ColorControlTextDisabled;
            }

        }

        #endregion

        #region New overridable protected methods

        /// <summary>
        /// Refreshes the controls on the action panel.
        /// </summary>
        protected virtual void RefreshActionPanel()
        {
            btnClearSelection.Visible = (buttons & SelectorButtons.ClearSelection) != 0;
            btnClearSelection.Enabled = !readOnly;
            btnSelectAll.Visible = (buttons & SelectorButtons.SelectAll) != 0;
            btnSelectAll.Enabled = !readOnly;
            btnSelectNone.Visible = (buttons & SelectorButtons.SelectNone) != 0;
            btnSelectNone.Enabled = !readOnly;
            btnBrowse.Visible = (buttons & SelectorButtons.Browse) != 0;
            btnBrowse.Enabled = !readOnly;
            btnEditor.Visible = (buttons & SelectorButtons.Editor) != 0;
            btnNew.Visible = (buttons & SelectorButtons.New) != 0;
            foreach (Control c in pnlActionPanel.Controls)
            {
                if (c is Button button)
                    button.FlatStyle = ButtonStyle;
            }
        }

        /// <summary>
        /// Creates the controls on the action panel.
        /// </summary>
        protected virtual void CreateActionPanel()
        {
            pnlActionPanel.Controls.Clear();
            btnClearSelection = CreateActionButton(Properties.Resources.Clear, toolTipClearSelection);
            btnSelectAll = CreateActionButton(Properties.Resources.All, toolTipSelectAll);
            btnSelectNone = CreateActionButton(Properties.Resources.None, toolTipSelectNone);
            btnBrowse = CreateActionButton(Properties.Resources.Browse, toolTipBrowse);
            btnEditor = CreateActionButton(Properties.Resources.Edit, toolTipEditor);
            btnNew = CreateActionButton(Properties.Resources.New, toolTipNew);
        }

        /// <summary>
        /// The default AutoFind behaviour if <see cref="AutoFind"/> event is not handled.
        /// Can be overridden to set <see cref="Value"/> and <see cref="Text"/> based on
        /// the written text or text chunk and can be triggered also from a handled <see cref="AutoFind"/> event by setting the <see cref="AutoFindEventArgs.DefaultAutoFind"/> property.
        /// The default implementation sets <paramref name="text"/> to <see cref="Value"/>
        /// or tries to find the given text in <see cref="Items"/>.
        /// </summary>
        /// <param name="text">Text or text chunk for that can be used for search.</param>
        protected virtual void DefaultAutoFind(string text)
        {
            if (Items.Count > 0)
            {
                bool toResolve = DataSource != null && !String.IsNullOrEmpty(ValueMember) && !String.IsNullOrEmpty(DisplayMember);
                foreach (object item in Items)
                {
                    if (cmbCombo.GetItemText(item) == text)
                    {
                        Value = !toResolve ? item : GetItemValue(item);
                        return;
                    }
                }
            }

            // fallback: assigning text to Value
            Value = text;
        }

        /// <summary>
        /// Returns the text based on <see cref="Value"/>.
        /// In <see cref="ucCustomSelector"/> it returns only texts for special values
        /// or when <see cref="DataSource"/> is not <see langword="null"/> and <paramref name="value"/> can be found in <see cref="ValueMember"/> of data source.
        /// Otherwise, returns with the ToString of <see cref="Value"/>.
        /// Override this method to calculate texts for other values.
        /// </summary>
        /// <param name="value">The value to obtain the text for.</param>
        [SuppressMessage("ReSharper", "ParameterHidesMember", Justification = "Renaming would be a breaking change")]
        protected virtual string GetTextByValue(object? value)
        {
            if (Equals(value, valueNotSelected) || (DataSource != null && value.In(null, DBNull.Value)))
                return Language.Translate(textNotSelected);
            else if (Equals(value, valueAllSelected))
                return Language.Translate(textAllSelected);
            else if (Equals(value, valueNoneSelected))
                return Language.Translate(textNoneSelected);
            else
            {
                string text = value?.ToString() ?? String.Empty;
                if ((DataSource is IList || DataSource is IListSource) && !String.IsNullOrEmpty(ValueMember) && !String.IsNullOrEmpty(DisplayMember))
                {
                    IEnumerable<object> ds = (DataSource is IListSource listSource ? listSource.GetList() : (IList)DataSource).Cast<object>();
                    object? item = ds.FirstOrDefault(i => Equals(value, GetItemValue(i)));
                    if (item != null)
                        text = cmbCombo.GetItemText(item) ?? String.Empty;
                }

                var e = new CalculateTextEventArgs(value, text);
                if (CalculateText != null)
                {
                    OnCalculateText(e);
                    return e.Text;
                }
                else if (Equals(value, ControlExtensions.UndefinedValue))
                    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract - false alarm, can happen
                    return cmbCombo.Text ?? String.Empty;
                else
                    return e.Text;
            }
        }

        /// <summary>
        /// Sets <see cref="Value"/>, and adjusts <see cref="Text"/> and <see cref="State"/>.
        /// </summary>
        /// <param name="value">The value to set.</param>
        [SuppressMessage("ReSharper", "ParameterHidesMember", Justification = "Intended, sets the value; renaming would be a breaking change")]
        protected virtual void SetValue(object? value)
        {
            if (!Equals(this.value, value))
            {
                this.value = value;
                SelectorStates newState;
                if (Equals(value, valueNotSelected))
                    newState = SelectorStates.NotSelected;
                else if (Equals(value, valueAllSelected))
                    newState = SelectorStates.All;
                else if (Equals(value, valueNoneSelected))
                    newState = SelectorStates.None;
                else
                    newState = SelectorStates.ValueSet;

                if (state != newState)
                {
                    state = newState;
                    OnStateChanged(EventArgs.Empty);
                }
                RefreshImage();
                SetTextByValue();
                ResetColor();
                OnValueChanged(EventArgs.Empty);
            }
            else
                SetTextByValue();
        }


        /// <summary>
        /// Invokes the <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnValueChanged(EventArgs e) => ValueChanged?.Invoke(this, e);

        /// <summary>
        /// Invokes the <see cref="StateChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnStateChanged(EventArgs e) => StateChanged?.Invoke(this, e);

        /// <summary>
        /// Invokes the <see cref="ButtonClearClick"/> event, or when it is not handled
        /// calls the <see cref="DefaultClearClick">DefaultClearClick</see> method.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnButtonClearClick(EventArgs e)
        {
            if (ButtonClearClick is EventHandler handler)
                handler.Invoke(this, e);
            else
                DefaultClearClick();
        }

        /// <summary>
        /// Invokes the <see cref="ButtonAllClick"/> event, or when it is not handled
        /// calls the <see cref="DefaultAllClick">DefaultAllClick</see> method.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnButtonAllClick(EventArgs e)
        {
            if (ButtonAllClick is EventHandler handler)
                handler.Invoke(this, e);
            else
                DefaultAllClick();
        }

        /// <summary>
        /// Invokes the <see cref="ButtonNoneClick"/> event, or when it is not handled
        /// calls the <see cref="DefaultNoneClick">DefaultNoneClick</see> method.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnButtonNoneClick(EventArgs e)
        {
            if (ButtonNoneClick is EventHandler handler)
                handler.Invoke(this, e);
            else
                DefaultNoneClick();
        }

        /// <summary>
        /// Invokes the <see cref="ButtonBrowseClick"/> event, or when it is not handled
        /// calls the <see cref="DefaultBrowseClick">DefaultBrowseClick</see> method.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnButtonBrowseClick(EventArgs e)
        {
            if (ButtonBrowseClick is EventHandler handler)
                handler.Invoke(this, e);
            else
                DefaultBrowseClick();
        }

        /// <summary>
        /// Invokes the <see cref="ButtonEditorClick"/> event, or when it is not handled
        /// calls the <see cref="DefaultEditorClick">DefaultEditorClick</see> method.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnButtonEditorClick(EventArgs e)
        {
            if (ButtonEditorClick is EventHandler handler)
                handler.Invoke(this, e);
            else
                DefaultEditorClick();
        }

        /// <summary>
        /// Invokes the <see cref="ButtonNewClick"/> event, or when it is not handled
        /// calls the <see cref="DefaultNewClick">DefaultNewClick</see> method.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnButtonNewClick(EventArgs e)
        {
            if (ButtonNewClick is EventHandler handler)
                handler.Invoke(this, e);
            else
                DefaultNewClick();
        }

        /// <summary>
        /// Invokes the <see cref="AutoFind"/> event, or when it is not handled
        /// calls the <see cref="DefaultAutoFind">DefaultAutoFind</see> method.
        /// </summary>
        /// <param name="e">An <see cref="AutoFindEventArgs"/> that contains the event data.</param>
        protected virtual void OnAutoFind(AutoFindEventArgs e)
        {
            if (AutoFind is EventHandler<AutoFindEventArgs> handler)
            {
                handler.Invoke(this, e);
                if (!e.DefaultAutoFind)
                {
                    Value = e.Value;
                    return;
                }
            }

            DefaultAutoFind(e.SearchPattern);
        }

        /// <summary>
        /// Invokes the <see cref="CalculateText"/> event.
        /// </summary>
        /// <param name="e">A <see cref="CalculateTextEventArgs"/> that contains the event data.</param>
        protected virtual void OnCalculateText(CalculateTextEventArgs e) => CalculateText?.Invoke(this, e);

        /// <summary>
        /// Invokes the <see cref="ImageClick"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnImageClick(EventArgs e) => ImageClick?.Invoke(this, e);

        /// <summary>
        /// Invokes the <see cref="ImageDoubleClick"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnImageDoubleClick(EventArgs e) => ImageDoubleClick?.Invoke(this, e);

        #endregion

        #endregion

        #region Private methods

        #region Private implementation

        private void SetText(string? newValue)
        {
            if (String.IsNullOrEmpty(newValue))
                cmbCombo.Clear();
            else
                cmbCombo.Text = newValue;
        }

        /// <summary>
        /// Sets the text based on Value.
        /// </summary>
        private void SetTextByValue()
        {
            string text = GetTextByValue(value);

            // assigning even if equal to set SelectedIndex and SelectedValue if text is among elements (might needed after AutoFind)
            SetText(text);

            // clearing text in dropdown list mode if could not be set
            if (cmbCombo.DropDownStyle == ComboBoxStyle.DropDownList && cmbCombo.SelectedIndex >= 0 && cmbCombo.Text != text)
                cmbCombo.SelectedIndex = -1;
        }

        private object? GetItemValue(object item)
        {
            Debug.Assert(DataSource != null && !String.IsNullOrEmpty(ValueMember) && !String.IsNullOrEmpty(DisplayMember));

            return Reflector.GetProperty(item, ValueMember);
        }

        /// <summary>
        /// Sets <see cref="State"/> adjusting <see cref="Value"/> and <see cref="Text"/>.
        /// </summary>
        /// <param name="newState">State to set.</param>
        private void SetState(SelectorStates newState)
        {
            if (newState != state)
            {
                switch (newState)
                {
                    case SelectorStates.NotSelected:
                        SetValue(valueNotSelected);
                        break;
                    case SelectorStates.All:
                        SetValue(valueAllSelected);
                        break;
                    case SelectorStates.None:
                        SetValue(valueNoneSelected);
                        break;
                    case SelectorStates.ValueSet:
                    default:
                        SetValue(ControlExtensions.UndefinedValue);
                        break;
                }
            }
        }

        private void SetControlValue(object? newValue)
        {
            switch (relevantControlValue)
            {
                case RelevantControlValues.Value:
                    Value = newValue;
                    break;
                case RelevantControlValues.Text:
                    if (String.IsNullOrEmpty(newValue as string) || newValue.Equals(Language.Translate(textNotSelected)))
                        Value = valueNotSelected;
                    else if (Equals(newValue, Language.Translate(textAllSelected)))
                        Value = valueAllSelected;
                    else if (Equals(newValue, Language.Translate(textNoneSelected)))
                        Value = valueNoneSelected;
                    else
                    {
                        Value = newValue;
                    }
                    break;
                case RelevantControlValues.State:
                    if (newValue is RelevantControlValues)
                        SetState((SelectorStates)newValue);
                    else
                        Value = valueNotSelected;
                    break;
            }
        }

        #endregion

        #region Handled events

        private void SelectorButtonClick(object? sender, EventArgs e)
        {
            if (sender == btnClearSelection)
                OnButtonClearClick(e);
            else if (sender == btnSelectAll)
                OnButtonAllClick(e);
            else if (sender == btnSelectNone)
                OnButtonNoneClick(e);
            else if (sender == btnBrowse)
                OnButtonBrowseClick(e);
            else if (sender == btnEditor)
                OnButtonEditorClick(e);
            else if (sender == btnNew)
                OnButtonNewClick(e);
        }

        private void pbImage_Click(object? sender, EventArgs e) => OnImageClick(e);
        private void pbImage_DoubleClick(object? sender, EventArgs e) => OnImageDoubleClick(e);

        private void cmbCombo_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (readOnly || !textEditable || DropDownStyle == ComboBoxStyle.DropDownList)
                return;
            if (state != SelectorStates.ValueSet && deleteContent)
            {
                cmbCombo.Clear();
                deleteContent = false;
            }
        }

        private void cmbCombo_Enter(object? sender, EventArgs e)
        {
            checkChangedOnLeave = false;
            deleteContent = state != SelectorStates.ValueSet;
        }

        private void cmbCombo_TextChangedOnLeave(object? sender, EventArgs e)
        {
            ResetColor();
            if (!checkChangedOnLeave)
                return;
            if (autoFind)
            {
                OnAutoFind(new AutoFindEventArgs(cmbCombo.Text, valueNotSelected));
            }
            else
            {
                Value = cmbCombo.Text;
            }
        }

        void cmbCombo_SelectedValueChanged(object? sender, EventArgs e)
        {
            // if DropDownList, cmbCombo.SelectedValue will be null if Value is not among DataSource elements
            if (DataSource != null && !(cmbCombo.DropDownStyle == ComboBoxStyle.DropDownList && cmbCombo.SelectedValue == null) && !Equals(value, cmbCombo.SelectedValue))
            {
                Value = cmbCombo.SelectedValue;
                checkChangedOnLeave = false;
            }
        }

        void cmbCombo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (DataSource == null && SelectedIndex != -1 && !Equals(value, cmbCombo.SelectedItem))
            {
                Value = cmbCombo.SelectedItem;
                checkChangedOnLeave = false;
            }
        }

        private void ucCustomSelector_Load(object? sender, EventArgs e)
        {
            SetTextByValue();
        }

        void cmbCombo_TextChanged(object? sender, EventArgs e)
        {
            if (cmbCombo.Focused)
                checkChangedOnLeave = true;
            else
                ResetColor();
        }

        #endregion

        #region Designer-related methods

        /// <summary>
        /// This method indicates to designers whether the property
        /// value is different from the ambient value, in which case
        /// the designer should persist the value.
        /// </summary>
        private bool ShouldSerializeText()
        {
            return state == SelectorStates.ValueSet;
        }

        #endregion

        #endregion

        #endregion

        #region IListControl Members

        /// <summary>
        /// Gets or sets the value of the member property specified by the <see cref="ValueMember"/> property.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Bindable(BindableSupport.Default)]
        [DisallowNull]
        public object? SelectedValue
        {
            get => cmbCombo.SelectedValue;
            set => cmbCombo.SelectedValue = value;
        }

        /// <summary>
        /// Gets whether there is no selected item in the combo box (<see cref="SelectedValue"/> is <see langword="null"/>, <see cref="DBNull"/> or equals to <see cref="ControlExtensions.NotSelectedValue"/>)
        /// </summary>
        [Browsable(false)]
        public bool IsEmpty => cmbCombo.IsEmpty();

        /// <summary>
        /// Occurs when the <see cref="SelectedIndex"/> property has changed.
        /// </summary>
        [Category("ucCustomSelector")]
        public event EventHandler SelectedIndexChanged
        {
            add => cmbCombo.SelectedIndexChanged += value;
            remove => cmbCombo.SelectedIndexChanged -= value;
        }

        /// <summary>
        /// Occurs when the <see cref="SelectedValue"/> property changes.
        /// </summary>
        [Category("ucCustomSelector")]
        public event EventHandler SelectedValueChanged
        {
            add => cmbCombo.SelectedValueChanged += value;
            remove => cmbCombo.SelectedValueChanged -= value;
        }

        /// <summary>
        /// Gets or sets the currently selected item in the combo box.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Bindable(BindableSupport.Yes)]
        public object? SelectedItem
        {
            get => cmbCombo.SelectedItem;
            set => cmbCombo.SelectedItem = value;
        }

        /// <summary>
        /// Gets or sets the text that is selected in the editable portion of a combo box.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string SelectedText
        {
            get => cmbCombo.SelectedText;
            set => cmbCombo.SelectedText = value;
        }

        /// <summary>
        /// Gets or sets the index specifying the currently selected item.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int SelectedIndex
        {
            get => cmbCombo.SelectedIndex;
            set => cmbCombo.SelectedIndex = value;
        }

        /// <summary>
        /// Gets or sets the data source for the inner <see cref="AdvancedComboBox"/>.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the data source for the inner AdvancedComboBox.")]
        [DefaultValue(null)]
        [RefreshProperties(RefreshProperties.Repaint)]
        [AttributeProvider(typeof(IListSource))]
        public object? DataSource
        {
            get => cmbCombo.DataSource;
            set => cmbCombo.DataSource = value;
        }

        /// <summary>
        /// Gets or sets the property to display for the inner <see cref="AdvancedComboBox"/>.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the property to display for the inner AdvancedComboBox.")]
        [DefaultValue("")]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string DisplayMember
        {
            get => cmbCombo.DisplayMember;
            set => cmbCombo.DisplayMember = value;
        }

        /// <summary>
        /// Gets or sets the property to use as the actual value for the items in the inner <see cref="AdvancedComboBox"/>.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets the property to use as the actual value for the items in the inner AdvancedComboBox.")]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ValueMember
        {
            get => cmbCombo.ValueMember;
            set => cmbCombo.ValueMember = value;
        }

        /// <summary>
        /// Gets or sets an option that controls how automatic completion works for the inner text box.
        /// </summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets an option that controls how automatic completion works for the inner text box.")]
        [DefaultValue(AutoCompleteMode.None)]
        public AutoCompleteMode AutoCompleteMode
        {
            get => cmbCombo.AutoCompleteMode;
            set => cmbCombo.AutoCompleteMode = value;
        }

        ///<summary>
        /// Gets or sets a value specifying the source of complete strings used for automatic completion.
        ///</summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets a value specifying the source of complete strings used for automatic completion.")]
        [DefaultValue(AutoCompleteSource.None)]
        public AutoCompleteSource AutoCompleteSource
        {
            get => cmbCombo.AutoCompleteSource;
            set => cmbCombo.AutoCompleteSource = value;
        }

        ///<summary>
        /// Gets or sets a custom <see cref="AutoCompleteStringCollection"/> if the <see cref="AutoCompleteSource"/> property is <see cref="AutoCompleteSource.CustomSource"/>.
        ///</summary>
        [Category("ucCustomSelector")]
        [Description("Gets or sets a custom AutoCompleteStringCollection if the AutoCompleteSource property is CustomSource.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get => cmbCombo.AutoCompleteCustomSource;
            set => cmbCombo.AutoCompleteCustomSource = value;
        }

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, the displayed column must contain string values.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, dataTable, valueMember, displayMember, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the value column must have a data type that is convertible to signed integer type.</param>
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, dataTable, valueMember, displayMember, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <see cref="DataTable"/>. Items will not be sorted or translated.
        /// </summary>
        /// <param name="dataTable">The data source table.</param>
        /// <param name="displayMember">Column name to display in the combo box.</param>
        /// <param name="valueMember">Column name to use as the actual value for the items in the combo box.</param>
        public void LoadFrom(DataTable dataTable, string valueMember, string displayMember)
        {
            ListControlExtensions.LoadFrom(cmbCombo, dataTable, valueMember, displayMember);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will be used as value member.</param>
        /// <param name="translateNames">Indicates whether the displayed enum field names should be translated.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        public void LoadFrom(Type enumType, Type valueMemberType, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, enumType, valueMemberType, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will be used as value member.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If <see cref="SelectionPlusItems.ItemAll"/> or <see cref="SelectionPlusItems.ItemNone"/> is requested,
        /// then the <paramref name="valueMemberType"/> must be a signed integer type or an enum with signed underlying type.</param>
        public void LoadFrom(Type enumType, Type valueMemberType, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, enumType, valueMemberType, plusItems);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted or translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        /// <param name="valueMemberType">Type of the actual value for the items in the combo box. If <see langword="null"/>, then original enum value will be used as value member.</param>
        public void LoadFrom(Type enumType, Type valueMemberType)
        {
            ListControlExtensions.LoadFrom(cmbCombo, enumType, valueMemberType);
        }

        /// <summary>
        /// Binds the combo box to the values of an <see cref="Enum"/>. Items will not be sorted or translated.
        /// </summary>
        /// <param name="enumType">An <see cref="Enum"/> type with the fields to bind.</param>
        public void LoadFrom(Type enumType)
        {
            ListControlExtensions.LoadFrom(cmbCombo, enumType);
        }

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the <paramref name="collection"/>.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        /// <param name="translateNames">Indicates whether the displayed values should be translated. If so, <paramref name="displayMember"/> must be writable and should refer to a <see cref="string"/> property.</param>
        /// <param name="distinctionPostfix">Distinction postfix for translated items. Can be <see langword="null"/> to omit distinction.</param>
        /// <param name="sortByDisplayedValues">If <see langword="true"/>, then items will be sorted by displayed values. Requested <paramref name="plusItems"/> will always be the first items.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember, bool translateNames, string distinctionPostfix, bool sortByDisplayedValues, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, collection, valueMember, displayMember, translateNames, distinctionPostfix, sortByDisplayedValues, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>. Items will not be sorted and only the <paramref name="plusItems"/> will be translated.
        /// </summary>
        /// <typeparam name="T">The element type of the <paramref name="collection"/>.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        /// <param name="plusItems">Requested additional items (Not selected/All/None). If plus itmes are requested, then <paramref name="valueMember"/> must refer to a property,
        /// which is convertible to signed integer type.</param>
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember, SelectionPlusItems plusItems)
        {
            ListControlExtensions.LoadFrom(cmbCombo, collection, valueMember, displayMember, plusItems);
        }

        /// <summary>
        /// Binds the combo box to a <paramref name="collection"/>. Items will not be sorted or translated.
        /// </summary>
        /// <typeparam name="T">The element type of the <paramref name="collection"/>.</typeparam>
        /// <param name="collection">The source collection.</param>
        /// <param name="displayMember">Property name to display in the combo box.</param>
        /// <param name="valueMember">Property name to use as the actual value for the items in the combo box.</param>
        public void LoadFrom<T>(IEnumerable<T> collection, string valueMember, string displayMember)
        {
            ListControlExtensions.LoadFrom(cmbCombo, collection, valueMember, displayMember);
        }

        #endregion
    }
}
