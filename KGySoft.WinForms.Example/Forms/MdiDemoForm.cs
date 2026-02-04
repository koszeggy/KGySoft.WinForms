#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MdiDemoForm.cs
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using KGySoft.ComponentModel;
using KGySoft.Drawing;
using KGySoft.WinForms.Forms;

#endregion

namespace KGySoft.WinForms.Example.Forms
{
    internal partial class MdiDemoForm : BaseForm
    {
        #region Constructors

        public MdiDemoForm()
        {
            InitializeComponent();
            InitCommandBindings();
            if (!IsDesignMode && SystemFonts.MessageBoxFont is Font font)
                Font = font;
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override void OnOwnedMdiChildClosed(OwnedMdiChildClosedEventArgs e)
        {
            base.OnOwnedMdiChildClosed(e);
            WriteLog($"{Text}: Owned child {e.MdiChild.Text} has been closed");
        }

        #endregion

        #region Private Methods

        private void InitCommandBindings()
        {
            // Could've used event subscriptions as well, but this way we don't need to bother with unsubscribing.
            // See more at https://github.com/koszeggy/KGySoft.CoreLibraries#command-binding
            CommandBindings.Add(OnAddRootChildCommand).AddSource(miAddRootChild, nameof(miAddRootChild.Click));
            CommandBindings.Add<PaintEventArgs>(OnPaintMdiClientCommand).AddSource(MdiClient!, nameof(MdiClient.Paint));
            CommandBindings.Add(OnCloseAllCommand).AddSource(miCloseAll, nameof(miCloseAll.Click));
            CommandBindings.Add(OnMinimizeAllCommand).AddSource(miMinimizeAll, nameof(miMinimizeAll.Click));

            CommandBindings.Add(InvalidateMdiClientArea).AddSource(MdiClient!, nameof(MdiClient.ClientSizeChanged));
            CommandBindings.Add(() => LayoutMdi(MdiLayout.Cascade)).AddSource(miCascade, nameof(miCascade.Click));
            CommandBindings.Add(() => LayoutMdi(MdiLayout.TileHorizontal)).AddSource(miTileHorizontally, nameof(miTileHorizontally.Click));
            CommandBindings.Add(() => LayoutMdi(MdiLayout.TileVertical)).AddSource(miTileVertically, nameof(miTileVertically.Click));
        }

        private void WriteLog(string text) => txtLog.AppendText(text + Environment.NewLine);

        #endregion

        #region Command Handlers

        private void OnAddRootChildCommand()
        {
            var child = new MdiChildForm(WriteLog) { Text = @"Root Child " + (MdiChildren.Length + 1) };
            ShowMdiChild(child);
            WriteLog($"{child.Text} is opened by {Text}");
        }

        private void OnCloseAllCommand()
        {
            foreach (Form mdiChild in MdiChildren)
                mdiChild.Close();
        }

        private void OnMinimizeAllCommand()
        {
            foreach (Form mdiChild in MdiChildren)
                mdiChild.WindowState = FormWindowState.Minimized;
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void OnPaintMdiClientCommand(ICommandSource<PaintEventArgs> src)
        {
            string text = "You can add MDI child windows to a BaseForm by the ShowMdiChild method. "
                    + "ShowMdiChild can be called on another child window as well, and can be used to suspend the "
                    + "caller form, somewhat similarly to ShowDialog. As ShowMdiChild is not a blocking call even if "
                    + "the caller form is suspended, you can use the OnMdiChildClosed method to get notified when the child is closed";

            using Icon icon = Icons.SystemInformation;
            Rectangle rect = ((MdiClient)src.Source).ClientRectangle;
            rect.Inflate(-5, -5);
            Size iconSize = new Size(16, 16).Scale(DeviceScale);
            using Bitmap iconImage = icon.ExtractNearestBitmap(iconSize, PixelFormat.Format32bppArgb); // just because Graphics.DrawIcon has much worse quality than DrawImage
            iconSize = iconImage.Size;
            src.EventArgs.Graphics.DrawImage(iconImage, new Rectangle(rect.X, rect.Y, iconSize.Width, iconSize.Height));
            rect.X += iconSize.Width;
            rect.Width -= iconSize.Width;

            using var font = new Font(Font, FontStyle.Bold);
            rect.Offset(1, 1);
            TextRenderer.DrawText(src.EventArgs.Graphics, text, font, rect, Color.Black, TextFormatFlags.WordBreak | TextFormatFlags.Top | TextFormatFlags.Left);
            rect.Offset(-1, -1);
            TextRenderer.DrawText(src.EventArgs.Graphics, text, font, rect, Color.White, TextFormatFlags.WordBreak | TextFormatFlags.Top | TextFormatFlags.Left);
        }

        #endregion

        #endregion
    }
}
