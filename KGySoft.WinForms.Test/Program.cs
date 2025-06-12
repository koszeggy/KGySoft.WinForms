using System;
using System.ComponentModel;
using System.Windows.Forms;
using KGySoft.Drawing;
using KGySoft.Reflection;
using KGySoft.WinForms.Components;
using KGySoft.WinForms.Test.Forms;

using TaskDialog = KGySoft.WinForms.Components.TaskDialog;
using TaskDialogButton = KGySoft.WinForms.Components.TaskDialogButton;
using TaskDialogRadioButton = KGySoft.WinForms.Components.TaskDialogRadioButton;

namespace KGySoft.WinForms.Test
{
    static class Program
    {
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
                //dlg.Buttons.Add(new TaskDialogButton("btnMisc", "Misc Tests"));

                dlg.Buttons["btnControlsTest"].Click += (sender, args) => ShowControlsTestMenu(((TaskDialogButton)sender).Parent);
                dlg.Buttons["btnTaskDialogTest"].Click += (sender, args) => ShowTaskDialogTestMenu(((TaskDialogButton)sender).Parent);
                //dlg.Buttons["btnMisc"].Click += (sender, args) =>
                //{
                //    using var frm = new MiscTest();
                //    frm.ShowDialog();
                //};
                dlg.Show();
            }
        }

        private static void ShowControlsTestMenu(TaskDialog parent)
        {
            using (TaskDialog td = new TaskDialog())
            {
                td.Options = TaskDialogOptions.UseCommandLinks;
                td.StandardButtons = TaskDialogStandardButtonFlags.Cancel;
                td.MainInstruction = "Select a control to test";
                td.Buttons.Add(new TaskDialogButton("AdvancedButton"));
                td.Buttons.Add(new TaskDialogButton("AdvancedCheckBox"));
                td.Buttons.Add(new TaskDialogButton("AdvancedLabel"));
                td.Buttons.Add(new TaskDialogButton("AdvancedProgressBar"));
                td.Buttons.Add(new TaskDialogButton("AdvancedRadioButton"));
                td.Buttons.Add(new TaskDialogButton("CommandLinkButton"));
                td.Buttons.Add(new TaskDialogButton("AdvancedTextBox"));
                td.Buttons.Add(new TaskDialogButton("AdvancedComboBox"));
                td.Buttons.Add(new TaskDialogButton("AdvancedDateTimePicker"));
                td.Buttons.Add(new TaskDialogButton("FadingDemo", "Custom fading control demo"));

                foreach (TaskDialogButton button in td.Buttons)
                {
                    string name = button.Name;
                    if (String.IsNullOrEmpty(name))
                        name = button.Text;

                    button.Click += (sender, args) =>
                    {
                        using (ControlsTestBaseForm frm = (ControlsTestBaseForm)Reflector.CreateInstance(Reflector.ResolveType($"{typeof(Program).Namespace}.Forms.frm{name}")))
                        {
                            frm.ShowDialog();
                        }                        
                    };
                }

                td.Show(parent);
            }
        }

        private static void ShowTaskDialogTestMenu(TaskDialog parent)
        {
            using (TaskDialog dlg = new TaskDialog())
            {
                dlg.Caption = "Dialogs demo";
                dlg.MainInstruction = "Pick a task";
                dlg.StandardButtons = TaskDialogStandardButtonFlags.Close;
                dlg.CheckBoxText = "Force emulated dialog";
                dlg.Options = TaskDialogOptions.UseCommandLinks | TaskDialogOptions.AllowCancel;

                TaskDialogButton btn = new TaskDialogButton("Buttons Test");
                btn.Click += new EventHandler<HandledEventArgs>(btnCustomButtons_Click);
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("Icons Test");
                btn.Click += new EventHandler<HandledEventArgs>(btnIconTest_Click);
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("Text Elements Test");
                btn.Click += new EventHandler<HandledEventArgs>(btnTextElements_Click);
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("Timer Test");
                btn.Click += new EventHandler<HandledEventArgs>(btnTimerTest_Click);
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("Progress Bar Test"); // with enabled/disabled
                btn.Click += new EventHandler<HandledEventArgs>(btnProgressBar_Click);
                dlg.Buttons.Add(btn);

                btn = new TaskDialogButton("Options Test");
                btn.Click += new EventHandler<HandledEventArgs>(btnOptionsTest_Click);
                dlg.Buttons.Add(btn);

                dlg.Show(parent);
            }
        }

        private static void btnOptionsTest_Click(object sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender).Parent;

            using (TaskDialog dlg = new TaskDialog())
            {
                dlg.ForceCompatibilityMode = senderDialog.CheckBoxChecked;
                dlg.DetailsText = "This is the DetailsText";
                dlg.Message = "This is a <a href=\"https://kgysoft.net\">sample link</a>";
                dlg.Options = TaskDialogOptions.HyperlinksEnabled | TaskDialogOptions.UseCommandLinks | TaskDialogOptions.DetailsExpanded;
                dlg.FooterIcon = TaskDialogStandardIcons.Warning;
                dlg.FooterText = "Warning: In native mode RightToLeftLayout cannot be undone";
                dlg.StandardButtons = TaskDialogStandardButtonFlags.Close;

                dlg.Buttons.Add(new TaskDialogButton("Toggle HyperlinksEnabled") { Description = "On", Tag = TaskDialogOptions.HyperlinksEnabled });
                dlg.Buttons.Add(new TaskDialogButton("Toggle AllowCancel") { Description = "Off", Tag = TaskDialogOptions.AllowCancel });
                dlg.Buttons.Add(new TaskDialogButton("Toggle UseCommandLinks") { Description = "Off", Tag = TaskDialogOptions.UseCommandLinks });
                dlg.Buttons.Add(new TaskDialogButton("Toggle UseCommandLinksNoIcon") { Description = "On", Tag = TaskDialogOptions.UseCommandLinksNoIcon });
                dlg.Buttons.Add(new TaskDialogButton("Toggle ExpandFooterArea") { Description = "Off", Tag = TaskDialogOptions.ExpandFooterArea });
                dlg.Buttons.Add(new TaskDialogButton("Toggle DetailsExpanded") { Description = "On", Tag = TaskDialogOptions.DetailsExpanded });
                //dlg.Buttons.Add(new TaskDialogButton("Toggle PositionRelativeToWindow") { Description = "Off", Tag = TaskDialogOptions.PositionRelativeToWindow });
                dlg.Buttons.Add(new TaskDialogButton("Toggle RightToLeftLayout") { Description = "Off", Tag = TaskDialogOptions.RightToLeftLayout });
                dlg.Buttons.Add(new TaskDialogButton("Toggle AllowMinimize") { Description = "Off", Tag = TaskDialogOptions.AllowMinimize });
                dlg.Width = 300;

                foreach (TaskDialogButton button in dlg.Buttons)
                {
                    button.Click += (btn, args) =>
                    {
                        TaskDialogButton b = (TaskDialogButton)btn;
                        TaskDialogOptions option = (TaskDialogOptions)b.Tag;

                        if ((b.Parent.Options & option) == TaskDialogOptions.None)
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

                dlg.Show();
            }
        }

        private static void btnProgressBar_Click(object sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender).Parent;
            char[] states = new[] { '|', '/', '-', '\\'  };

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

                dlg.RadioButtons["rbNone"].Selected += (rb, args) =>
                {
                    TaskDialog td = ((TaskDialogRadioButton)rb).Parent;
                    td.ProgressBarStyle = TaskDialogProgressBarStyle.None;
                    td.Buttons["btnPause"].Enabled = false;
                    td.Buttons["btnRestart"].Enabled = false;
                };

                dlg.RadioButtons["rbMarquee"].Selected += (rb, args) =>
                {
                    TaskDialog td = ((TaskDialogRadioButton)rb).Parent;
                    td.ProgressBarStyle = TaskDialogProgressBarStyle.Marquee;
                    td.Buttons["btnPause"].Enabled = true;
                    td.Buttons["btnRestart"].Enabled = false;
                    td.MainInstruction = "Please Wait...";
                };

                dlg.RadioButtons["rbRegular"].Selected += (rb, args) =>
                {
                    TaskDialog td = ((TaskDialogRadioButton)rb).Parent;
                    td.ProgressBarStyle = TaskDialogProgressBarStyle.Regular;
                    td.Buttons["btnPause"].Enabled = true;
                    td.Buttons["btnRestart"].Enabled = td.ProgressBarValue == td.ProgressBarMaximum;
                };

                int state = 0;
                dlg.Tick += (d, args) =>
                {
                    TaskDialog td = (TaskDialog)d;
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

                dlg.Buttons["btnPause"].Click += (btn, args) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn;
                    if (button.Parent.ProgressBarState == ProgressBarState.Normal)
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

                dlg.Buttons["btnRestart"].Click += (btn, args) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn;
                    button.Parent.ProgressBarValue = 0;
                    button.Enabled = false;
                };

                dlg.Show();
            }
        }

        private static void btnTimerTest_Click(object sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender).Parent;

            using (TaskDialog dlg = new TaskDialog())
            {
                dlg.ForceCompatibilityMode = senderDialog.CheckBoxChecked;
                dlg.Caption = "Timer demo";
                dlg.MainInstruction = "Elapsed: 0 seconds";
                dlg.StandardButtons = TaskDialogStandardButtonFlags.Close;

                bool resetRequested = false;
                dlg.Buttons.Add(new TaskDialogButton("Reset Timer"));
                dlg.Buttons[0].Click += (btn, args) => { resetRequested = true; };

                dlg.Tick += (td, args) =>
                {
                    ((TaskDialog)td).MainInstruction = $"Elapsed: {args.Elapsed / 1000} seconds";
                    args.Reset = resetRequested;
                    resetRequested = false;
                };

                dlg.Show();
            }
        }

        private static void btnTextElements_Click(object sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender).Parent;

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
                dlg.CustomFooterIcon =  Icons.Application;
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

                dlg.Buttons["btnCaption"].Click += (btn, args) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn;
                    button.Parent.Caption = String.IsNullOrEmpty(button.Parent.Caption) ? caption : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.Caption) ? "Off" : "On";
                };

                dlg.Buttons["btnMainInstruction"].Click += (btn, args) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn;
                    button.Parent.MainInstruction = String.IsNullOrEmpty(button.Parent.MainInstruction) ? mainInstruction : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.MainInstruction) ? "Off" : "On";
                };

                dlg.Buttons["btnMessage"].Click += (btn, args) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn;
                    button.Parent.Message = String.IsNullOrEmpty(button.Parent.Message) ? message : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.Message) ? "Off" : "On";
                };

                dlg.Buttons["btnDetailsText"].Click += (btn, args) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn;
                    button.Parent.DetailsText = String.IsNullOrEmpty(button.Parent.DetailsText) ? detailsText : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.DetailsText) ? "Off" : "On";
                };

                dlg.Buttons["btnFooterText"].Click += (btn, args) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn;
                    button.Parent.FooterText = String.IsNullOrEmpty(button.Parent.FooterText) ? footerText : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.FooterText) ? "Off" : "On";
                };

                dlg.Buttons["btnCheckBoxText"].Click += (btn, args) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn;
                    button.Parent.CheckBoxText = String.IsNullOrEmpty(button.Parent.CheckBoxText) ? checkBoxText : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.CheckBoxText) ? "Off" : "On";
                };

                dlg.Buttons["btnShowDetailsText"].Click += (btn, args) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn;
                    button.Parent.ShowDetailsText = String.IsNullOrEmpty(button.Parent.ShowDetailsText) ? showDetailsText : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.ShowDetailsText) ? "Off" : "On";
                };

                dlg.Buttons["btnHideDetailsText"].Click += (btn, args) =>
                {
                    TaskDialogButton button = (TaskDialogButton)btn;
                    button.Parent.HideDetailsText = String.IsNullOrEmpty(button.Parent.HideDetailsText) ? hideDetailsText : null;
                    button.Description = String.IsNullOrEmpty(button.Parent.HideDetailsText) ? "Off" : "On";
                };

                dlg.RadioButtons["rbMessage"].Selected += (rb, args) =>
                {
                    ((TaskDialogRadioButton)rb).Parent.Options &= ~TaskDialogOptions.ExpandFooterArea;
                };

                dlg.RadioButtons["rbFooter"].Selected += (rb, args) =>
                {
                    ((TaskDialogRadioButton)rb).Parent.Options |= TaskDialogOptions.ExpandFooterArea;
                };

                dlg.Show();
            }
        }

        private static void btnCustomButtons_Click(object sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender).Parent;

            using (TaskDialog dlg = new TaskDialog())
            {
                dlg.ForceCompatibilityMode = senderDialog.CheckBoxChecked;
                dlg.Options = dlg.ForceCompatibilityMode ? TaskDialogOptions.TranslateStandardButtons : TaskDialogOptions.None;
                dlg.Caption = "Buttons test";
                dlg.Message = "OK and Cancel buttons are standard ones, while Custom button is a custom one." + Environment.NewLine
                    + "You can define radio buttons, too.";

                dlg.StandardButtons = TaskDialogStandardButtonFlags.OK | TaskDialogStandardButtonFlags.Cancel;

                dlg.Buttons.Add(new TaskDialogButton("btnCustom", "Custom") { Description = "I am a custom button" });
                dlg.Buttons["btnCustom"].Click += (btn, args) =>
                {
                    TaskDialog owner = ((TaskDialogButton)btn).Parent;
                    using (TaskDialog dlgQuestion = new TaskDialog())
                    {
                        dlgQuestion.ForceCompatibilityMode = owner.ForceCompatibilityMode;
                        dlgQuestion.Caption = "Confirmation";
                        dlgQuestion.Icon = TaskDialogStandardIcons.Question;
                        dlgQuestion.Message = "Do you want to close the Buttons test dialog?";
                        dlgQuestion.StandardButtons = TaskDialogStandardButtonFlags.Yes | TaskDialogStandardButtonFlags.No;
                        args.Handled = dlgQuestion.Show(owner) == TaskDialogResult.No;
                    }
                };

                dlg.RadioButtons.Add(new TaskDialogRadioButton("rbStandard", "Standard Button") { Checked = true });
                dlg.RadioButtons.Add(new TaskDialogRadioButton("rbCommandLink", "Command Link Button with Glyph"));
                dlg.RadioButtons.Add(new TaskDialogRadioButton("rbCommandNoLink", "Command Link Button without Glyph"));
                dlg.RadioButtons["rbStandard"].Selected += (rb, args) =>
                {
                    ((TaskDialogRadioButton)rb).Parent.Options = TaskDialogOptions.None;
                };

                dlg.RadioButtons["rbCommandLink"].Selected += (rb, args) =>
                {
                    ((TaskDialogRadioButton)rb).Parent.Options = TaskDialogOptions.UseCommandLinks;
                };

                dlg.RadioButtons["rbCommandNoLink"].Selected += (rb, args) =>
                {
                    ((TaskDialogRadioButton)rb).Parent.Options = TaskDialogOptions.UseCommandLinksNoIcon;
                };

                dlg.CheckBoxText = "Has Elevated Icon";
                dlg.CheckBoxCheckedChanged += (tdSender, args) =>
                {
                    TaskDialog td = (TaskDialog)tdSender;
                    td.Buttons["btnCustom"].IsElevated = td.CheckBoxChecked;
                };

                dlg.Show();
            }
        }

        private static void btnIconTest_Click(object sender, HandledEventArgs e)
        {
            TaskDialog senderDialog = ((TaskDialogButton)sender).Parent;

            using (TaskDialog dlg = new TaskDialog())
            {
                dlg.ForceCompatibilityMode = senderDialog.CheckBoxChecked;
                dlg.CustomIcon = dlg.CustomFooterIcon = Icons.Application;
                dlg.Options = TaskDialogOptions.AllowCancel;
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
                    radioButton.Selected += (rbSender, args) =>
                    {
                        TaskDialogRadioButton rb = (TaskDialogRadioButton)rbSender;

                        if (rb.Name == "rbCustom")
                            rb.Parent.CustomIcon = rb.Parent.CustomFooterIcon = Icons.Application;
                        else
                            rb.Parent.Icon = rb.Parent.FooterIcon = (TaskDialogStandardIcons)rb.Tag;
                    };
                }

                dlg.Show();
            }
        }
    }
}
