/*
 * Idegnyugatató form hosszú várakozásokhoz
 * 
 * Támogatja a hívó felfüggesztését, a progress bart és az animálást (vagy bármit, amit a hívó akar)
 * a lefagyottság érzés elkerülésére (nem aszinkron módon).
 * 
 * Használata:
 * Létrehozás után Execute() hívás, utána try-finally block, ahol a finally-ban Close() hívás van
 * 
 * Kívülr?l is elérhet?:
 * - Image (a max 50x50-es ikon)
 * - Caption (felirat)
 * - Progress (folyamatjelz?)
 * - Timer (id?zít?)
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KGySoft.Drawing;

namespace KGySoft.Controls
{

    public sealed partial class WaitForm : BaseForm
    {
        #region Static konstantsok

        public static bool DisplayAsTopMost = true;

        #endregion

        #region objektumváltozók

        Action<WaitForm, WaitActionArgs> OnAction = null; // a változást frissít? eljárás

        #endregion

        #region property-k

        /// <summary>
        /// Gets the <see cref="PictureBox"/> object that contains the image.
        /// </summary>
        public PictureBox Image
        {
            get { return pbImage; }
        }

        /// <summary>
        /// Gets the <see cref="Label"/> object that contains the caption.
        /// </summary>
        public Label Caption
        {
            get { return lblCaption; }
        }

        /// <summary>
        /// Gets the <see cref="System.Windows.Forms.ProgressBar"/> object that is associated with the progress bar.
        /// </summary>
        public ProgressBar ProgressBar
        {
            get { return pbProgress; }
        }

        /// <summary>
        /// Gets the <see cref="System.Windows.Forms.Timer"/> object that is associated with the timer.
        /// </summary>
        public Timer Timer
        {
            get { return tTimer; }
        }

        #endregion

        #region Konstruktor, publikus metódusok

        ///<summary>
        /// Creates a new instance of <see cref="WaitForm"/> class.
        ///</summary>
        public WaitForm()
        {
            InitializeComponent();
            this.pbImage.Image = Images.HourGlass;
            this.TopMost = DisplayAsTopMost;
        }

        /// <summary>
        /// Executes the wait form. Can be called from any thread.
        /// </summary>
        /// <param name="caller">Caller form. Can be null if not an MDI child or not a <see cref="BaseForm"/>.</param>
        /// <param name="suspend">If true, suspends caller MDI child specified as <paramref name="caller"/>.</param>
        /// <param name="caption">Caption. When null, caption is not changed, which is "Please wait..." by default.</param>
        /// <param name="showProgressBar">Specifies whether the progress bar should be visible. Default style is marquee but can be changed via <see cref="ProgressBar"/> property.</param>
        /// <param name="doAction">An optionally defined callback action that will be called during the wait process.</param>
        /// <param name="interval">Interval in millisecundum for calling <paramref name="doAction"/>.</param>
        public void Execute(IWin32Window owner, bool suspend, string caption, bool showProgressBar, Action<WaitForm, WaitActionArgs> doAction, int interval)
        {
            Action doExecute = delegate
            {
                if (caption != null)
                    lblCaption.Text = caption;

                pbProgress.Visible = showProgressBar;
                OnAction = doAction;

                Timer.Interval = interval;
                Timer.Enabled = OnAction != null;

                //if (MainMdiParent != null)
                //{
                //    ShowMdiChild(caller, suspend);
                //}
                //else
                //{
                    Show(owner); // itt a hívót nem tiltjuk le, esetleg meg lehet csinálni, csak a form bezárásakor ne felejtsük el újra engedélyezni
                //}
                Refresh();
            };

            if (InvokeRequired)
                Invoke(doExecute);
            else
                doExecute.Invoke();
        }

        /// <summary>
        /// Executes the wait form.
        /// </summary>
        /// <param name="caller">Caller form. Can be null if not an MDI child or not a <see cref="BaseForm"/>.</param>
        /// <param name="suspend">If true, suspends caller MDI child specified as <paramref name="caller"/>.</param>
        /// <param name="caption">Caption. When null, caption is not changed, which is "Please wait..." by default.</param>
        /// <param name="showProgressBar">Specifies whether the progress bar should be visible. Default style is marquee but can be changed via <see cref="ProgressBar"/> property.</param>
        public void Execute(IWin32Window owner, bool suspend, string caption, bool showProgressBar)
        {
            Execute(owner, suspend, caption, showProgressBar, null, 1000);
        }

        /// <summary>
        /// Executes the wait form.
        /// </summary>
        /// <param name="caption">Caption. When null, caption is not changed, which is "Please wait..." by default.</param>
        /// <param name="showProgressBar">Specifies whether the progress bar should be visible. Default style is marquee but can be changed via <see cref="ProgressBar"/> property.</param>
        public void Execute(string caption, bool showProgressBar)
        {
            Execute(null, false, caption, showProgressBar, null, 1000);
        }

        /// <summary>
        /// Executes the wait form.
        /// </summary>
        /// <param name="caption">Caption. When null, caption is not changed, which is "Please wait..." by default.</param>
        public void Execute(string caption)
        {
            Execute(null, false, caption, false, null, 1000);
        }

        /// <summary>
        /// Executes the wait form without showing the progress bar displaying the already set message, or "Please wait..." by default.
        /// </summary>
        public void Execute()
        {
            Execute(null, false, null, false, null, 1000);
        }

        #endregion

        #region események

        private void tTimer_Tick(object sender, EventArgs e)
        {
            if (OnAction != null)
                OnAction(this, new WaitActionArgs(this));
            Refresh();
        }

        #endregion
    }

    public class WaitActionArgs
    {
        private WaitForm frm;

        public WaitActionArgs(WaitForm frm)
        {
            this.frm = frm;
        }

        /// <summary>
        /// Gets or sets the displayed icon image.
        /// This property can be accessed from any thread.
        /// </summary>
        public Image Image
        {
            get
            {
                Func<Image> get = () => frm.Image.Image;

                return frm.InvokeRequired ? (Image)frm.Invoke(get) : get.Invoke();
            }
            set
            {
                Action set = () => frm.Image.Image = value;

                if (frm.InvokeRequired)
                    frm.Invoke(set);
                else
                    set.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets the caption.
        /// This property can be accessed from any thread.
        /// </summary>
        public string Caption
        {
            get
            {
                Func<string> get = () => frm.Caption.Text;

                return frm.InvokeRequired ? (string)frm.Invoke(get) : get.Invoke();
            }
            set
            {
                Action set = () => frm.Caption.Text = value;

                if (frm.InvokeRequired)
                    frm.Invoke(set);
                else
                    set.Invoke();                
            }
        }

        /// <summary>
        /// gets or sets the pogress position.
        /// This property can be accessed from any thread.
        /// </summary>
        public int ProgressPosition
        {
            get
            {
                Func<int> get = () => frm.ProgressBar.Value;

                return frm.InvokeRequired ? (int)frm.Invoke(get) : get.Invoke();
            }
            set
            {
                Action set = () => frm.ProgressBar.Value = value;

                if (frm.InvokeRequired)
                    frm.Invoke(set);
                else
                    set.Invoke();                
            }
        }       
    }
}

