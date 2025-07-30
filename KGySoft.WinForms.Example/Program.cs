#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Program.cs
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

#region Used Namespaces

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.Drawing;
using KGySoft.Reflection;
using KGySoft.WinForms.Components;
using KGySoft.WinForms.Example.Forms;
using KGySoft.WinForms.Forms;

#endregion

#region Used Aliases

using TaskDialog = KGySoft.WinForms.Components.TaskDialog;
using TaskDialogButton = KGySoft.WinForms.Components.TaskDialogButton;
using TaskDialogRadioButton = KGySoft.WinForms.Components.TaskDialogRadioButton;

#endregion

#endregion

namespace KGySoft.WinForms.Example
{
    static class Program
    {
        #region Properties

        // Some global settings for testing purposes
        internal static bool AutoScaleFont { get; set; }
        internal static AutoScaleMode AutoScaleMode => AutoScaleMode.Font;
        internal static FormStartPosition StartPosition => FormStartPosition.CenterParent; 

        #endregion

        #region Methods

        #region Private Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            using (TaskDialog dlg = new TaskDialog())
            {
                dlg.Options = TaskDialogOptions.AllowCancel | TaskDialogOptions.UseCommandLinks;
                dlg.Buttons.Add(new TaskDialogButton("btnControlsTest", "Controls Test"));
                dlg.Buttons.Add(new TaskDialogButton("btnTaskDialogTest", "TaskDialog Test"));
                dlg.Buttons.Add(new TaskDialogButton("btnMiscTests", "Misc Tests"));

                dlg.Buttons["btnControlsTest"].Click += (sender, _) => ShowControlsTestMenu(((TaskDialogButton)sender!).Parent!);
                dlg.Buttons["btnTaskDialogTest"].Click += (sender, _) => ShowTaskDialogTestMenu(((TaskDialogButton)sender!).Parent!);
                dlg.Buttons["btnMiscTests"].Click += (sender, _) => ShowMiscTestsMenu(((TaskDialogButton)sender!).Parent!);

                //var btn = new TaskDialogButton("Test");
                //btn.Click += (sender, _) =>
                //{
                //    using var frm = new Form1();
                //    frm.ShowDialog(((TaskDialogButton)sender!).Parent!);
                //};
                //dlg.Buttons.Add(btn);

                dlg.Show();
            }
        }

        private static void ShowControlsTestMenu(TaskDialog parent)
        {
            using TaskDialog td = new TaskDialog();
            td.Options = TaskDialogOptions.UseCommandLinks;
            td.StandardButtons = TaskDialogStandardButtonFlags.Cancel;
            td.MainInstruction = "Select a control to test";
            td.CheckBoxText = "Set BaseForm.AutoScaleFont to true";
            td.CheckBoxChecked = AutoScaleFont;
            td.CheckBoxCheckedChanged += (sender, _) => AutoScaleFont = ((TaskDialog)sender!).CheckBoxChecked;
            td.FooterText = "• BaseForm.AutoScaleFont makes a difference with per-monitor DPI awareness on Windows 8.1 or later when targeting older .NET versions.\r\n"
                + "• Uncheck it to observe the difference between standard and KGy SOFT controls while changing the DPI\r\n"
                + "• Check it to auto scale the fonts on the whole form, fixing some broken system behavior.";
            td.Buttons.Add(new TaskDialogButton("AdvancedButton"));
            td.Buttons.Add(new TaskDialogButton("ImageViewer"));
            td.Buttons.Add(new TaskDialogButton("AdvancedCheckBox"));
            td.Buttons.Add(new TaskDialogButton("AdvancedRadioButton"));
            td.Buttons.Add(new TaskDialogButton("AdvancedLabel"));
            td.Buttons.Add(new TaskDialogButton("AdvancedProgressBar"));
            td.Buttons.Add(new TaskDialogButton("CommandLinkButton"));
            td.Buttons.Add(new TaskDialogButton("AdvancedTextBox"));
            td.Buttons.Add(new TaskDialogButton("AdvancedComboBox"));
            td.Buttons.Add(new TaskDialogButton("AdvancedDateTimePicker"));

            foreach (TaskDialogButton button in td.Buttons)
            {
                string? name = button.Name;
                if (String.IsNullOrEmpty(name))
                    name = button.Text;

                button.Click += (_, _) =>
                {
                    using ControlsTestBaseForm frm = (ControlsTestBaseForm)Reflector.CreateInstance(Reflector.ResolveType($"{typeof(Program).Namespace}.Forms.frm{name}")!);
                    frm.ShowDialog();
                };
            }

            td.Show(parent);
        }

        private static void ShowTaskDialogTestMenu(TaskDialog parent)
        {
            using (TaskDialog dlg = new TaskDialog())
            {
                dlg.Caption = "Dialogs demo";
                dlg.MainInstruction = "Pick a task";
                dlg.StandardButtons = TaskDialogStandardButtonFlags.Close;
                dlg.CheckBoxText = "Force compatibility mode";
                dlg.Options = TaskDialogOptions.UseCommandLinks | TaskDialogOptions.AllowCancel;

                TaskDialogButton btn = new TaskDialogButton("Buttons Test");
                btn.Click += btnCustomButtons_Click;
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("Icons Test");
                btn.Click += btnIconTest_Click;
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("Text Elements Test");
                btn.Click += btnTextElements_Click;
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("Timer Test");
                btn.Click += btnTimerTest_Click;
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("Progress Bar Test"); // with enabled/disabled
                btn.Click += btnProgressBar_Click;
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("btnOptionsModal", "Options Test (Modal)");
                btn.Click += btnOptionsTest_Click;
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("btnOptionsModeless", "Options Test (Modeless)");
                btn.Click += btnOptionsTest_Click;
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("Help Test");
                btn.Click += btnHelpTest_Click;
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("Button Icons Test") { Description = "No native support, runs always in compatibility mode" };
                btn.Click += btnButtonIconsTest_Click;
                dlg.Buttons.Add(btn);

                dlg.Show(parent);
            }
        }

        private static void ShowMiscTestsMenu(TaskDialog parent)
        {
            using TaskDialog td = new TaskDialog();
            td.Options = TaskDialogOptions.UseCommandLinks;
            td.StandardButtons = TaskDialogStandardButtonFlags.Cancel;

            td.Buttons.Add(new TaskDialogButton("LocalizationExample", "Dynamic localization example"));
            td.Buttons.Add(new TaskDialogButton("MdiDemo", "MDI Form Example"));
            td.Buttons.Add(new TaskDialogButton("FadingDemo", "Custom fading control demo"));

            foreach (TaskDialogButton button in td.Buttons)
            {
                string? name = button.Name;
                if (String.IsNullOrEmpty(name))
                    name = button.Text;

                button.Click += (_, _) =>
                {
                    using Form frm = (Form)Reflector.CreateInstance(Reflector.ResolveType($"{typeof(Program).Namespace}.Forms.frm{name}")!);
                    frm.ShowDialog();
                };
            }

            var btnDialogsTest = new TaskDialogButton("Dialogs Test");
            btnDialogsTest.Click += btnDialogsTest_Click;
            td.Buttons.Add(btnDialogsTest);

            var btnMisc = new TaskDialogButton("Misc Tests");
            btnMisc.Click += (_, _) =>
            {
                using var frm = new MiscTest();
                frm.ShowDialog();
            };
            td.Buttons.Add(btnMisc);

            td.Show(parent);

        }

        #endregion

        #region Event handlers

        private static void btnOptionsTest_Click(object? sender, HandledEventArgs e)
        {
            var senderButton = (TaskDialogButton)sender!;
            using (TaskDialog dlg = new TaskDialog())
            {
                dlg.ForceCompatibilityMode = senderButton.Parent!.CheckBoxChecked;
                dlg.DetailsText = "This is the DetailsText";
                dlg.Message = "This is a <a href=\"https://kgysoft.net\">sample link</a>";
                dlg.Options = TaskDialogOptions.HyperlinksEnabled | TaskDialogOptions.UseCommandLinksNoIcon | TaskDialogOptions.DetailsExpanded | TaskDialogOptions.AllowCancel | TaskDialogOptions.AllowMinimize | TaskDialogOptions.ForceShowInTaskbar;
                dlg.FooterIcon = TaskDialogStandardIcons.Warning;
                dlg.FooterText = "• UseCommandLinks has higher priority than and UseCommandLinksNoIcon" + Environment.NewLine
                    + "• In native mode RightToLeftLayout cannot be undone" + Environment.NewLine
                    + "• AllowMinimize works only if the dialog was opened without an owner (modeless)" + Environment.NewLine
                    + "• AllowMinimize implicitly enables cancellation, as if AllowCancel was also set" + Environment.NewLine
                    + "• In native mode enabling AllowMinimize after showing the dialog will show a non-functional button" + Environment.NewLine
                    + "• In native mode toggling ForceShowInTaskbar works only if it was initially enabled";
                dlg.StandardButtons = TaskDialogStandardButtonFlags.Close;

                dlg.Buttons.Add(new TaskDialogButton("Toggle HyperlinksEnabled") { Description = "On", Tag = TaskDialogOptions.HyperlinksEnabled });
                dlg.Buttons.Add(new TaskDialogButton("Toggle AllowCancel") { Description = "On", Tag = TaskDialogOptions.AllowCancel });
                dlg.Buttons.Add(new TaskDialogButton("Toggle UseCommandLinks") { Description = "Off", Tag = TaskDialogOptions.UseCommandLinks });
                dlg.Buttons.Add(new TaskDialogButton("Toggle UseCommandLinksNoIcon") { Description = "On", Tag = TaskDialogOptions.UseCommandLinksNoIcon });
                dlg.Buttons.Add(new TaskDialogButton("Toggle ExpandFooterArea") { Description = "Off", Tag = TaskDialogOptions.ExpandFooterArea });
                dlg.Buttons.Add(new TaskDialogButton("Toggle DetailsExpanded") { Description = "On", Tag = TaskDialogOptions.DetailsExpanded });
                //dlg.Buttons.Add(new TaskDialogButton("Toggle PositionRelativeToWindow") { Description = "Off", Tag = TaskDialogOptions.PositionRelativeToWindow });
                dlg.Buttons.Add(new TaskDialogButton("Toggle RightToLeftLayout") { Description = "Off", Tag = TaskDialogOptions.RightToLeftLayout });
                dlg.Buttons.Add(new TaskDialogButton("Toggle AllowMinimize") { Description = "On", Tag = TaskDialogOptions.AllowMinimize });
                dlg.Buttons.Add(new TaskDialogButton("Toggle ForceShowSysMenu") { Description = "Off", Tag = TaskDialogOptions.ForceShowSysMenu });
                dlg.Buttons.Add(new TaskDialogButton("Toggle ForceShowInTaskbar") { Description = "On", Tag = TaskDialogOptions.ForceShowInTaskbar });
                dlg.Width = 300;

                foreach (TaskDialogButton button in dlg.Buttons)
                {
                    button.Click += (btn, _) =>
                    {
                        TaskDialogButton b = (TaskDialogButton)btn!;
                        TaskDialogOptions option = (TaskDialogOptions)b.Tag!;

                        if ((b.Parent!.Options & option) == TaskDialogOptions.None)
                        {
                            b.Parent.Options |= option;
                            b.Description = "On";
                        }
                        else
                        {
                            b.Parent.Options &= ~option;
                            b.Description = "Off";
                        }
                    };
                }

                dlg.Show(senderButton.Name == "btnOptionsModal" ? senderButton.Parent : null);
            }
        }

        private static void btnProgressBar_Click(object? sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender!).Parent!;
            char[] states = new[] { '|', '/', '-', '\\' };

            using (TaskDialog dlg = new TaskDialog())
            {
                dlg.ForceCompatibilityMode = senderDialog.CheckBoxChecked;
                dlg.Caption = "Progress Bar test";
                dlg.Options = TaskDialogOptions.AllowCancel | TaskDialogOptions.UseCommandLinks;
                dlg.StandardButtons = TaskDialogStandardButtonFlags.Close;
                dlg.MainInstruction = "Please Wait...";

                dlg.RadioButtons.Add(new TaskDialogRadioButton("rbNone", "No progress bar"));
                dlg.RadioButtons.Add(new TaskDialogRadioButton("rbMarquee", "Marquee progress bar") { Checked = true });
                dlg.RadioButtons.Add(new TaskDialogRadioButton("rbRegular", "Regular progress bar"));

                dlg.ProgressBarMaximum = 50;
                dlg.ProgressBarStyle = TaskDialogProgressBarStyle.Marquee;

                dlg.Buttons.Add(new TaskDialogButton("btnPause", "Pause"));
                dlg.Buttons.Add(new TaskDialogButton("btnRestart", "Restart") { Enabled = false });

                dlg.RadioButtons["rbNone"].Selected += (rb, _) =>
                {
                    TaskDialog td = ((TaskDialogRadioButton)rb!).Parent!;
                    td.ProgressBarStyle = TaskDialogProgressBarStyle.None;
                    td.Buttons["btnPause"].Enabled = false;
                    td.Buttons["btnRestart"].Enabled = false;
                };

                dlg.RadioButtons["rbMarquee"].Selected += (rb, _) =>
                {
                    TaskDialog td = ((TaskDialogRadioButton)rb!).Parent!;
                    td.ProgressBarStyle = TaskDialogProgressBarStyle.Marquee;
                    td.Buttons["btnPause"].Enabled = true;
                    td.Buttons["btnRestart"].Enabled = false;
                    td.MainInstruction = "Please Wait...";
                };

                dlg.RadioButtons["rbRegular"].Selected += (rb, _) =>
                {
                    TaskDialog td = ((TaskDialogRadioButton)rb!).Parent!;
                    td.ProgressBarStyle = TaskDialogProgressBarStyle.Regular;
                    td.Buttons["btnPause"].Enabled = true;
                    td.Buttons["btnRestart"].Enabled = td.ProgressBarValue == td.ProgressBarMaximum;
                };

                int state = 0;
                dlg.Tick += (d, _) =>
                {
                    TaskDialog td = (TaskDialog)d!;
                    if (td.ProgressBarStyle == TaskDialogProgressBarStyle.Regular && td.ProgressBarState == ProgressBarState.Normal)
                    {
                        if (td.ProgressBarValue < td.ProgressBarMaximum)
                            td.ProgressBarValue++;
                        else
                            td.Buttons["btnRestart"].Enabled = true;
                    }

                    switch (td.ProgressBarStyle)
                    {
                        case TaskDialogProgressBarStyle.None:
                            td.MainInstruction = "Please Wait... " + states[state];
                            state = (state + 1) % states.Length;
                            break;
                        case TaskDialogProgressBarStyle.Regular:
                            td.MainInstruction = "Progress: " + (td.ProgressBarValue == 0 ? "0 %" : $"{(float)td.ProgressBarValue / td.ProgressBarMaximum:P0}");
                            break;
                    }
                };

                dlg.Buttons["btnPause"].Click += (btn, _) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn!;
                    if (button.Parent!.ProgressBarState == ProgressBarState.Normal)
                    {
                        button.Parent.ProgressBarState = ProgressBarState.Paused;
                        button.Text = "Continue";
                    }
                    else
                    {
                        button.Parent.ProgressBarState = ProgressBarState.Normal;
                        button.Text = "Pause";
                    }
                };

                dlg.Buttons["btnRestart"].Click += (btn, _) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn!;
                    button.Parent!.ProgressBarValue = 0;
                    button.Enabled = false;
                };

                dlg.Show(senderDialog);
            }
        }

        private static void btnTimerTest_Click(object? sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender!).Parent!;

            using (TaskDialog dlg = new TaskDialog())
            {
                dlg.ForceCompatibilityMode = senderDialog.CheckBoxChecked;
                dlg.Caption = "Timer demo";
                dlg.MainInstruction = "Elapsed: 0 seconds";
                dlg.StandardButtons = TaskDialogStandardButtonFlags.Close;

                bool resetRequested = false;
                dlg.Buttons.Add(new TaskDialogButton("Reset Timer"));
                dlg.Buttons[0].Click += (_, _) => { resetRequested = true; };

                dlg.Tick += (td, args) =>
                {
                    ((TaskDialog)td!).MainInstruction = $"Elapsed: {args.Elapsed / 1000} seconds";
                    args.Reset = resetRequested;
                    resetRequested = false;
                };

                dlg.Show(senderDialog);
            }
        }

        private static void btnTextElements_Click(object? sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender!).Parent!;

            const string caption = "This is the Caption. When not set, displays application name.";
            const string mainInstruction = "This is the MainInstruction. When not set, Message is displayed here.";
            const string message = "This is the Message. When not set, it takes no place.\nCan contain <a href=\"https://kgysoft.net\">links</a>.";
            const string detailsText = "This is the DetailsText. When set, an expando button appears. It can take place either under Message or under FooterText, depending on Options.\n"
                    + "Use the radio buttons to change its place.\nCan contain <a href=\"https://kgysoft.net\">links</a>.";
            const string footerText = "This is FooterText. When set, this footer area appears. It can have a different icon from the main icon.\nCan contain <a href=\"https://kgysoft.net\">links</a>.";
            const string checkBoxText = "This is CheckBoxText. When set, this check box appears.";
            const string showDetailsText = "This is ShowDetailsText. When not set, shows HideDetailsText or a default text.";
            const string hideDetailsText = "This is HideDetailsText. When not set, shows ShowDetailsText or a default text.";

            using (TaskDialog dlg = new TaskDialog())
            {
                dlg.ForceCompatibilityMode = senderDialog.CheckBoxChecked;
                dlg.Icon = TaskDialogStandardIcons.Information;
                dlg.CustomFooterIcon = Icons.Application;
                dlg.Width = 300;

                dlg.Caption = caption;
                dlg.MainInstruction = mainInstruction;
                dlg.Message = message;
                dlg.DetailsText = detailsText;
                dlg.FooterText = footerText;
                dlg.CheckBoxText = checkBoxText;
                dlg.ShowDetailsText = showDetailsText;
                dlg.HideDetailsText = hideDetailsText;

                dlg.Options = TaskDialogOptions.HyperlinksEnabled | TaskDialogOptions.UseCommandLinksNoIcon | TaskDialogOptions.AllowCancel | TaskDialogOptions.DetailsExpanded;

                dlg.StandardButtons = TaskDialogStandardButtonFlags.Close;
                dlg.Buttons.Add(new TaskDialogButton("btnCaption", "Toggle Caption") { Description = "On" });
                dlg.Buttons.Add(new TaskDialogButton("btnMainInstruction", "Toggle MainInstruction") { Description = "On" });
                dlg.Buttons.Add(new TaskDialogButton("btnMessage", "Toggle Message") { Description = "On" });
                dlg.Buttons.Add(new TaskDialogButton("btnDetailsText", "Toggle DetailsText") { Description = "On" });
                dlg.Buttons.Add(new TaskDialogButton("btnFooterText", "Toggle FooterText") { Description = "On" });
                dlg.Buttons.Add(new TaskDialogButton("btnCheckBoxText", "Toggle CheckBoxText") { Description = "On" });
                dlg.Buttons.Add(new TaskDialogButton("btnShowDetailsText", "Toggle ShowDetailsText") { Description = "On" });
                dlg.Buttons.Add(new TaskDialogButton("btnHideDetailsText", "Toggle HideDetailsText") { Description = "On" });
                dlg.RadioButtons.Add(new TaskDialogRadioButton("rbMessage", "Expand message area") { Checked = true });
                dlg.RadioButtons.Add(new TaskDialogRadioButton("rbFooter", "Expand footer area"));

                dlg.Buttons["btnCaption"].Click += (btn, _) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn!;
                    button.Parent!.Caption = String.IsNullOrEmpty(button.Parent.Caption) ? caption : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.Caption) ? "Off" : "On";
                };

                dlg.Buttons["btnMainInstruction"].Click += (btn, _) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn!;
                    button.Parent!.MainInstruction = String.IsNullOrEmpty(button.Parent.MainInstruction) ? mainInstruction : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.MainInstruction) ? "Off" : "On";
                };

                dlg.Buttons["btnMessage"].Click += (btn, _) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn!;
                    button.Parent!.Message = String.IsNullOrEmpty(button.Parent.Message) ? message : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.Message) ? "Off" : "On";
                };

                dlg.Buttons["btnDetailsText"].Click += (btn, _) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn!;
                    button.Parent!.DetailsText = String.IsNullOrEmpty(button.Parent.DetailsText) ? detailsText : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.DetailsText) ? "Off" : "On";
                };

                dlg.Buttons["btnFooterText"].Click += (btn, _) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn!;
                    button.Parent!.FooterText = String.IsNullOrEmpty(button.Parent.FooterText) ? footerText : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.FooterText) ? "Off" : "On";
                };

                dlg.Buttons["btnCheckBoxText"].Click += (btn, _) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn!;
                    button.Parent!.CheckBoxText = String.IsNullOrEmpty(button.Parent.CheckBoxText) ? checkBoxText : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.CheckBoxText) ? "Off" : "On";
                };

                dlg.Buttons["btnShowDetailsText"].Click += (btn, _) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn!;
                    button.Parent!.ShowDetailsText = String.IsNullOrEmpty(button.Parent.ShowDetailsText) ? showDetailsText : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.ShowDetailsText) ? "Off" : "On";
                };

                dlg.Buttons["btnHideDetailsText"].Click += (btn, _) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn!;
                    button.Parent!.HideDetailsText = String.IsNullOrEmpty(button.Parent.HideDetailsText) ? hideDetailsText : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.HideDetailsText) ? "Off" : "On";
                };

                dlg.RadioButtons["rbMessage"].Selected += (rb, _) => ((TaskDialogRadioButton)rb!).Parent!.Options &= ~TaskDialogOptions.ExpandFooterArea;
                dlg.RadioButtons["rbFooter"].Selected += (rb, _) => ((TaskDialogRadioButton)rb!).Parent!.Options |= TaskDialogOptions.ExpandFooterArea;

                dlg.Show(senderDialog);
            }
        }

        private static void btnCustomButtons_Click(object? sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender!).Parent!;

            using TaskDialog dlg = new TaskDialog();
            dlg.ForceCompatibilityMode = senderDialog.CheckBoxChecked;
            dlg.Options = dlg.ForceCompatibilityMode ? TaskDialogOptions.TranslateStandardButtons : TaskDialogOptions.None;
            dlg.Caption = "Buttons test";
            dlg.Message = "OK and Cancel buttons are standard ones, while Custom button is a custom one." + Environment.NewLine
                + "You can define radio buttons, too.";

            dlg.StandardButtons = TaskDialogStandardButtonFlags.OK | TaskDialogStandardButtonFlags.Cancel;

            dlg.Buttons.Add(new TaskDialogButton("btnCustom", "Custom") { Description = "I am a custom button" });
            dlg.Buttons["btnCustom"].Click += (btn, args) =>
            {
                TaskDialog owner = ((TaskDialogButton)btn!).Parent!;
                using TaskDialog dlgQuestion = new TaskDialog();
                dlgQuestion.ForceCompatibilityMode = owner.ForceCompatibilityMode;
                dlgQuestion.Caption = "Confirmation";
                dlgQuestion.Icon = TaskDialogStandardIcons.Question;
                dlgQuestion.Message = "Do you want to close the Buttons test dialog?";
                dlgQuestion.StandardButtons = TaskDialogStandardButtonFlags.Yes | TaskDialogStandardButtonFlags.No;
                args.Handled = dlgQuestion.Show(owner) == TaskDialogResult.No;
            };

            dlg.RadioButtons.Add(new TaskDialogRadioButton("rbStandard", "Standard Button") { Checked = true });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("rbCommandLink", "Command Link Button with Glyph"));
            dlg.RadioButtons.Add(new TaskDialogRadioButton("rbCommandNoLink", "Command Link Button without Glyph"));
            dlg.RadioButtons["rbStandard"].Selected += (rb, _) => ((TaskDialogRadioButton)rb!).Parent!.Options = TaskDialogOptions.None;
            dlg.RadioButtons["rbCommandLink"].Selected += (rb, _) => ((TaskDialogRadioButton)rb!).Parent!.Options = TaskDialogOptions.UseCommandLinks;
            dlg.RadioButtons["rbCommandNoLink"].Selected += (rb, _) => ((TaskDialogRadioButton)rb!).Parent!.Options = TaskDialogOptions.UseCommandLinksNoIcon;

            dlg.CheckBoxText = "Has Elevated Icon";
            dlg.CheckBoxCheckedChanged += (tdSender, _) =>
            {
                TaskDialog td = (TaskDialog)tdSender!;
                td.Buttons["btnCustom"].IsElevated = td.CheckBoxChecked;
            };

            dlg.Show(senderDialog);
        }

        private static void btnIconTest_Click(object? sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender!).Parent!;

            using TaskDialog dlg = new TaskDialog();
            dlg.ForceCompatibilityMode = senderDialog.CheckBoxChecked;
            dlg.Options = TaskDialogOptions.AllowCancel | TaskDialogOptions.ForceShowSysMenu;
            dlg.Caption = "Icons test";
            dlg.MainInstruction = "Select an icon";
            dlg.FooterText = "Footer can have an icon, too.";
            dlg.RadioButtons.Add(new TaskDialogRadioButton("None") { Checked = true, Tag = TaskDialogStandardIcons.None });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("Information") { Tag = TaskDialogStandardIcons.Information });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("Warning") { Tag = TaskDialogStandardIcons.Warning });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("Question") { Tag = TaskDialogStandardIcons.Question });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("Error") { Tag = TaskDialogStandardIcons.Error });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("rbCustom", "Custom Icon"));
            dlg.RadioButtons.Add(new TaskDialogRadioButton("Security Shield") { Tag = TaskDialogStandardIcons.SecurityShield });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("Security Shield Gray") { Tag = TaskDialogStandardIcons.SecurityShieldGray });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("Security Shield Blue") { Tag = TaskDialogStandardIcons.SecurityShieldBlue });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("Security Success") { Tag = TaskDialogStandardIcons.SecuritySuccess });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("Security Warning") { Tag = TaskDialogStandardIcons.SecurityWarning });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("Security Error") { Tag = TaskDialogStandardIcons.SecurityError });
            dlg.RadioButtons.Add(new TaskDialogRadioButton("Security Question") { Tag = TaskDialogStandardIcons.SecurityQuestion });

            foreach (TaskDialogRadioButton radioButton in dlg.RadioButtons)
            {
                radioButton.Selected += (rbSender, _) =>
                {
                    TaskDialogRadioButton rb = (TaskDialogRadioButton)rbSender!;

                    if (rb.Name == "rbCustom")
                        rb.Parent!.CustomIcon = rb.Parent.CustomFooterIcon = Icons.Application;
                    else
                        rb.Parent!.Icon = rb.Parent.FooterIcon = (TaskDialogStandardIcons)rb.Tag!;
                };
            }

            dlg.Show(senderDialog);
        }

        private static void btnButtonIconsTest_Click(object? sender, HandledEventArgs e)
        {
            using TaskDialog dlg = new TaskDialog
            {
                Caption = "Custom Icons Test",
                DetailsText = "You can use multi-resolution icons on TaskDialog buttons." + Environment.NewLine + Environment.NewLine
                    + "On 100% DPI the icon size is always resized to 16x16 for standard buttons." + Environment.NewLine + Environment.NewLine
                    + "When buttons are displayed as command links, the preferred icon size is 32x32 on 100% DPI. If the icon has only larger images than twice of the preferred icon size, "
                    + "then the icon is resized; otherwise, the closest unscaled image is applied. Please note that this differs from the elevated icons behavior, which uses 16x16 icons on 100% DPI.",
                Options = TaskDialogOptions.AllowCancel | TaskDialogOptions.DetailsExpanded,
                Buttons =
                {
                    new TaskDialogButton("No icon") { Description = "As a command link, displays the default arrow glyph." },
                    new TaskDialogButton("btnElevated", "Elevated mode")
                    {
                        Description = "As an elevated button or command link, has always a 16x16 icon on 100% DPI, gradually increasing size on higher DPIs. "
                            + "This behavior is compatible with the native task dialogs." + Environment.NewLine
                            + "When elevated mode is disabled (use the check box below), a custom icon is displayed, which has a different sizing behavior as a command link (see Multi-resolution icon)",
                        IsElevated = true,
                    },
                    new TaskDialogButton("Multi-resolution icon")
                    {
                        Description = "As a button, the icon size is 16x16 on 100% DPI, increasing gradually on higher DPIs." + Environment.NewLine
                            + "As a command link, it renders the native icon image nearest to 32x32 on 100% DPI, gradually increasing the preferred size on higher DPIs",
                        CustomIcon = Icons.SystemApplication
                    },
                    new TaskDialogButton("Fix 16x16 icon")
                    {
                        Description = "As a button, the native 16x16 icon is displayed on 100% DPI, which is resized (gets blurry) on higher DPIs." + Environment.NewLine
                            + "As a command link, always the native 16x16 icon is displayed.",
                        CustomIcon = Icons.SystemInformation.Resize(new Size(16, 16))
                    },
                    new TaskDialogButton("Fix 256x256 icon")
                    {
                        Description = @"As a button, icon image is shrunk to 16x16 on 100% DPI." + Environment.NewLine
                            + "As a command link, rendered as a 64x64 icon on 100% DPI. When using 400% DPI or higher the unscaled 256x256 icon is displayed.",
                        CustomIcon = Icons.SystemWarning.Resize(new Size(256, 256))
                    },
                },
                RadioButtons =
                {
                    new TaskDialogRadioButton("rbButtons", "Show as buttons")
                    {
                        Checked = true,
                        Description = "When this option is selected, all icons have the same size, which is 16x16 on 100% DPI."
                    },
                    new TaskDialogRadioButton("rbCommandLinks", "Show as command links")
                    {
                        Description = "When this option is selected, icons preserve their native size." + Environment.NewLine
                            + "For multi-resolution icons, the preferred custom icon size is 32x32 on 100% DPI." + Environment.NewLine
                            + "Elevated buttons still use the 16x16 icon size on 100% DPI to maintain compatibility with the native task dialogs."
                    },
                },
                CheckBoxText = "Native elevated mode",
                CheckBoxChecked = true
            };

            try
            {
                dlg.Buttons["btnElevated"].CustomIcon = Icons.FromFile("imageres", 1028);
            }
            catch (Exception ex) when (ex is PlatformNotSupportedException or Win32Exception)
            {
                dlg.Buttons["btnElevated"].CustomIcon = Icons.Shield;
            }

            dlg.RadioButtons["rbButtons"].Selected += (rbSender, _) =>
            {
                TaskDialogRadioButton rb = (TaskDialogRadioButton)rbSender!;
                rb.Parent!.Options &= ~TaskDialogOptions.UseCommandLinks;
            };

            dlg.RadioButtons["rbCommandLinks"].Selected += (rbSender, _) =>
            {
                TaskDialogRadioButton rb = (TaskDialogRadioButton)rbSender!;
                rb.Parent!.Options |= TaskDialogOptions.UseCommandLinks;
            };

            dlg.CheckBoxCheckedChanged += (tdSender, _) =>
            {
                TaskDialog td = (TaskDialog)tdSender!;
                td.Buttons["btnElevated"].IsElevated = td.CheckBoxChecked;
            };

            dlg.Show(((TaskDialogButton)sender!).Parent);
        }

        private static void btnHelpTest_Click(object? sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender!).Parent!;

            using TaskDialog dlg = new TaskDialog
            {
                ForceCompatibilityMode = senderDialog.CheckBoxChecked,
                Caption = "Help Test",
                MainInstruction = "Press F1 for help",
                Message = "When the HelpRequested event is subscribed, you can press F1 on the task dialog to invoke the event." + Environment.NewLine + Environment.NewLine
                    + "In compatibility mode you can click also the ? button on the task dialog header if AllowCancel is enabled in options (and AllowMinimize is not enabled).",
                Options = TaskDialogOptions.AllowCancel // to show the ? button in compatibility mode
            };

            dlg.HelpRequested += (tdSender, _) =>
            {
                using TaskDialog helpDlg = new TaskDialog
                {
                    ForceCompatibilityMode = senderDialog.CheckBoxChecked,
                    Caption = "Help",
                    MainInstruction = "This is the help dialog.",
                    Message = "Help was requested",
                    Icon = TaskDialogStandardIcons.Question,
                    StandardButtons = TaskDialogStandardButtonFlags.Close,
                    Options = TaskDialogOptions.AllowCancel
                };
                helpDlg.Show((TaskDialog)tdSender!);
            };

            dlg.Show(senderDialog);
        }

        private static void btnDialogsTest_Click(object? sender, HandledEventArgs e)
        {
            const string sampleMessage = "Sample message";
            Dialogs.AutoRightToLeftLayout = true; // effective only if the current thread's UI culture is an RTL language (you can change it by the localization example)

            TaskDialog senderDialog = ((TaskDialogButton)sender!).Parent!;
            using TaskDialog dlg = new TaskDialog
            {
                Caption = "Dialogs Class Test",
                MainInstruction = "Select a control type by the radio buttons and check out the possible options",
                Message = "• You can use the static Dialogs.UseTaskDialogs (or the obsolete UseAdvancedDialogs) property to display alternative dialog types\r\n"
                    + "• Both the MessageBox and TaskDialog options support Windows system sounds, default button selection and copying the content by Ctrl+C. MessageBox does not support per-monitor DPI awareness correctly though.\r\n"
                    + "• If the static AutoRightToLeftLayout property is true, then the MessageBox and TaskDialog options use right-to-left layout if the current thread's UI culture is an RTL language\r\n"
                    + "• The AdvancedMessageDialog supports none of above, though the message can be selected and copied manually",
                Options = TaskDialogOptions.UseCommandLinks | TaskDialogOptions.AllowCancel,
                StandardButtons = TaskDialogStandardButtonFlags.Close,
                RadioButtons =
                {
                    new TaskDialogRadioButton("MessageBox") { Checked = true },
                    new TaskDialogRadioButton("TaskDialog"),
                    new TaskDialogRadioButton("AdvancedMessageDialog (obsolete)")
                },
                Buttons =
                {
                    new TaskDialogButton("btnInformation", "Information Dialog"),
                    new TaskDialogButton("btnWarning", "Warning Dialog"),
                    new TaskDialogButton("btnError", "Error Dialog"),
                    new TaskDialogButton("btnConfirmation", "Confirmation Dialog"),
                    new TaskDialogButton("btnCancellableConfirmation", "Cancellable Confirmation Dialog")
                }
            };

#pragma warning disable CS0618 // Type or member is obsolete (Dialogs.UseAdvancedDialogs)
            dlg.RadioButtons[0].Selected += (_, _) => Dialogs.UseTaskDialogs = Dialogs.UseAdvancedDialogs = false;
            dlg.RadioButtons[1].Selected += (_, _) => Dialogs.UseAdvancedDialogs = !(Dialogs.UseTaskDialogs = true);
            dlg.RadioButtons[2].Selected += (_, _) => Dialogs.UseTaskDialogs = !(Dialogs.UseAdvancedDialogs = true);
#pragma warning restore CS0618 // Type or member is obsolete

            dlg.Buttons["btnInformation"].Click += (_, _) => Dialogs.InfoMessage(sampleMessage);
            dlg.Buttons["btnWarning"].Click += (_, _) => Dialogs.WarningMessage(sampleMessage);
            dlg.Buttons["btnError"].Click += (_, _) => Dialogs.ErrorMessage(sampleMessage);
            dlg.Buttons["btnConfirmation"].Click += (_, _) => Dialogs.InfoMessage(Dialogs.ConfirmMessage(sampleMessage) ? "You clicked Yes" : "You clicked No");
            dlg.Buttons["btnCancellableConfirmation"].Click += (_, _) => Dialogs.InfoMessage(Dialogs.CancellableConfirmMessage(sampleMessage, MessageBoxDefaultButton.Button3) switch
            {
                true => "You selected Yes",
                false => "You selected No",
                null => "You selected Cancel or closed the dialog"
            });

            dlg.Show(senderDialog);
        }

        #endregion

        #endregion
    }
}
