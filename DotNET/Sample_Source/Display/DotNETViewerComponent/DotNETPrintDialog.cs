/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/// <summary>
/// This is a custom version of the standard PrintDialog class that allows customization.
/// </summary>
/// <remarks>
/// Since PrintDialog is sealed, this class uses direct calls to the x86 api to show the 
/// common print dialog, and allows the hook procedure to run, allowing additional controls
/// to be added to the dialog in the hook callback.
/// 
/// As much as possible, this class tries to mirror the interface of the standard PrintDialog
/// class.  This may not be possible in all cases, and deviations from the standard control
/// are noted.
/// 
/// The shrinkToFit option is a custom option that was added to the standard PrintDialog.
/// 
/// Based on code from:
///		http://www.abstraction.net/ViewArticle.aspx?articleID=89
/// </remarks>

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace DotNETViewerComponent
{
    class DotNETPrintDialog : System.Windows.Forms.CommonDialog
    {
        #region Imports from the x86 API

		private delegate IntPtr PrintHookProc(IntPtr hdlg, int msg, IntPtr wparam, IntPtr lparam);

		private const int PD_ALLPAGES = 0x00000000;
		private const int PD_NOSELECTION = 0x00000004;
		private const int PD_NOPAGENUMS = 0x00000008;
		private const int PD_ENABLEPRINTHOOK = 0x00001000;
		private const int PD_RETURNDC = 0x00000100;
        private const int PD_PRINTTOFILE = 0x00000020;
        private const int PD_COLLATE = 0x00000010;

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
		private struct PRINTDLG
		{
			public Int32 lStructSize;
			public IntPtr hwndOwner;
			public IntPtr hDevMode;
			public IntPtr hDevNames;
			public IntPtr hDC;
			public Int32 Flags;
			public UInt16 nFromPage;
			public UInt16 nToPage;
			public UInt16 nMinPage;
			public UInt16 nMaxPage;
			public UInt16 nCopies;
			public IntPtr hInstance;
			public Int32 lCustData;
			public PrintHookProc lpfnPrintHook;
			public IntPtr lpfnSetupHook;
			public string lpPrintTemplateName;
			public string lpSetupTemplateName;
			public IntPtr hPrintTemplate;
			public IntPtr hSetupTemplate;
		}

        [DllImport("comdlg32.dll", CharSet = CharSet.Auto)]
		private static extern bool PrintDlg(ref PRINTDLG pdlg);

		[DllImport("comdlg32.dll", CharSet = CharSet.Auto)]
		private static extern Int32 CommDlgExtendedError();

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SetParent(IntPtr hwnd, IntPtr parent);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr GetDlgItem(IntPtr hwnd, int item_id);

		[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
        private struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;

			public int Width { get { return (int)(right - left); } }
			public int Height { get { return (int)(bottom - top); } }
		}

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
		private struct POINT
		{
			public int x;
			public int y;
		}

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool GetWindowRect(IntPtr hwnd, ref RECT rect);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool ScreenToClient(IntPtr hwnd, ref POINT point);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool SetWindowPos(IntPtr hwnd, IntPtr insert_after, int x, int y, int width, int height, Int32 flags);

		private const int WM_INITDIALOG = 0x0110;

		private const int SWP_NOSIZE = 0x0001;
		private const int SWP_NOMOVE = 0x0002;
		private const int SWP_NOZORDER = 0x0004;

		private const int IDOK = 1;
		private const int IDCANCEL = 2;

		#endregion

        // members

        private bool fShrinkToFit;
        private bool allowShrinkToFit = false;

        private PRINTDLG pdlg;
        private PrintDocument pDoc = null;
        private PrinterSettings pSettings = null;
        private CheckBox shrinkToFitCheckBox = new CheckBox();

        // properties

        public bool AllowSomePages
        {
            get { return (pdlg.Flags & PD_NOPAGENUMS) != PD_NOPAGENUMS; }
            set
            {
                if(value)
                    pdlg.Flags = (pdlg.Flags | PD_NOPAGENUMS) ^ PD_NOPAGENUMS;
                else
                    pdlg.Flags |= PD_NOPAGENUMS;
            }
        }

        public bool AllowSelection
        {
            get { return (pdlg.Flags & PD_NOSELECTION) != PD_NOSELECTION; }
            set
            {
                if(value)
                    pdlg.Flags = (pdlg.Flags | PD_NOSELECTION) ^ PD_NOSELECTION;
                else
                    pdlg.Flags |= PD_NOSELECTION;
            }
        }

        public bool AllowShrinkToFit
        {
            get { return allowShrinkToFit; }
            set { allowShrinkToFit = value; }
        }

        public bool ShrinkToFit
        {
            get { return fShrinkToFit; }
            set { fShrinkToFit = value; }
        }

        public PrintDocument Document
        {
            get { return pDoc; }
            set { pDoc = value; }
        }

        public PrinterSettings PrinterSettings
        {
            get { return pDoc == null ? pSettings : pDoc.PrinterSettings; }
            set
            {
                pDoc = null;
                pSettings = value == null ? new PrinterSettings() : value;
            }
        }

        // constructor

        public DotNETPrintDialog()
        {
            Reset();
        }

        // methods

        public override void  Reset()
        {
 	        pdlg = new PRINTDLG();
            pdlg.lStructSize = Marshal.SizeOf(pdlg);
            pDoc = null;
            pSettings = new PrinterSettings();
            AllowShrinkToFit = false;
        }

        protected override bool  RunDialog(IntPtr hwndOwner)
        {
 	        pdlg.hwndOwner = hwndOwner;

            pdlg.Flags |= PD_ALLPAGES | PD_RETURNDC;
            pdlg.hDevMode = PrinterSettings.GetHdevmode();
            pdlg.hDevNames = PrinterSettings.GetHdevnames();
			pdlg.nMinPage = (ushort)PrinterSettings.MinimumPage;
			pdlg.nMaxPage = (ushort)PrinterSettings.MaximumPage;
            pdlg.nFromPage = (ushort)PrinterSettings.FromPage;
            pdlg.nToPage = (ushort)PrinterSettings.ToPage;

            // allow our custom hook to be added
            pdlg.Flags |= PD_ENABLEPRINTHOOK;

            // set the hook to out delegate 
            pdlg.lpfnPrintHook = new PrintHookProc(this.HookProc);

            bool f = false;
            try
            {
                f = PrintDlg(ref pdlg);
            }
            catch(Exception ex)
            {
                //any exception in the direct x86 call is most likely caused by a faulty
				//printer driver (we had one issue where the user provided a stack trace 
				//pointing to this call that was resolved by uninstalling printer and installing
				//with a new printer driver)
				throw new ApplicationException("An error occurred while showing the Print Dialog.", ex);
            }

            if(!f)
            {
                // closed with Cancel, or an error showing the dialog
                long err = CommDlgExtendedError();

                if(err != 0)
                {
                    throw new ApplicationException("An error occurred while showing the Print Dialog.");
                }
            }

            PrinterSettings.SetHdevmode(pdlg.hDevMode);
            PrinterSettings.SetHdevnames(pdlg.hDevNames);

            // NumCopies Hack: 
            // Sometimes the number of copies the user asked for is reported in pdlg.nCopies and must be 
            // explicitly moved into PrinterSettings.Copies; but sometimes the number is already stored in 
            // PrinterSettings and pdlg.nCopies == 1, even after the user set it to something else.  The
            // actual behavior depends on which printer the user selected and cannot be predicted at
            // compile time.  See http://www.tech-archive.net/Archive/DotNet/microsoft.public.dotnet.general/2004-03/0517.html
            // for more about this Microsoft bug.  Note that the Collate flag is also sensitive to this bug.
            //
            // To summarize: we cannot blindly assign pdlg.nCopies to PrinterSettings.Copies because sometimes 
            // we'll overwrite the user's desired number of copies with 1.  The fix is to only copy pdlg.nCopies
            // if it's LARGER than PrinterSettings.Copies, which implies that the user entered a value > 1 in
            // "Copies" in the dialog and it wasn't automatically picked up.  We also copy the Collate flag.
            if (pdlg.nCopies > PrinterSettings.Copies)
            {
                PrinterSettings.Copies = (short)pdlg.nCopies;
                PrinterSettings.Collate = ((pdlg.Flags & PD_COLLATE) == PD_COLLATE);
            }
			
            PrinterSettings.FromPage = pdlg.nFromPage;
			PrinterSettings.ToPage = pdlg.nToPage;
            PrinterSettings.PrintToFile = ((pdlg.Flags & PD_PRINTTOFILE) != 0);

			return f;
        }

        protected override IntPtr HookProc(IntPtr hdlg, int msg, IntPtr wparam, IntPtr lparam)
        {
            if(msg == WM_INITDIALOG && AllowShrinkToFit)
            {
                // create checkbox and add to dialog
                SetParent(shrinkToFitCheckBox.Handle, hdlg);

                // get dialog size
                RECT rect = new RECT();
                GetWindowRect(hdlg, ref rect);

                // determine position of the checkbox using the OK button as reference
                RECT button_rect = new RECT();
                IntPtr h_button = GetDlgItem(hdlg, IDOK);
                GetWindowRect(h_button, ref button_rect);

                POINT pt;
                pt.x = button_rect.left;
                pt.y = button_rect.top;
                ScreenToClient(hdlg, ref pt);

                shrinkToFitCheckBox.Location = new Point(12, pt.y);
                shrinkToFitCheckBox.Size = new Size(rect.Width - 12, shrinkToFitCheckBox.Size.Height);
                shrinkToFitCheckBox.Visible = true;
				shrinkToFitCheckBox.Text = "Shrink output to fit printable area";
				shrinkToFitCheckBox.Checked = ShrinkToFit;
				shrinkToFitCheckBox.CheckedChanged += new EventHandler(m_cb_CheckedChanged);

                // resize the print dialog so our checkbox fits
                SetWindowPos(hdlg, new IntPtr(0), 0, 0, rect.Width, rect.Height + shrinkToFitCheckBox.Bounds.Height, SWP_NOZORDER | SWP_NOMOVE);

                //move the OK button down
				h_button = GetDlgItem(hdlg, IDOK);
				GetWindowRect(h_button, ref rect);
				pt.x = rect.left;
				pt.y = rect.top;
				ScreenToClient(hdlg, ref pt);

				SetWindowPos(h_button, new IntPtr(0), pt.x, pt.y + shrinkToFitCheckBox.Bounds.Height, 0, 0, SWP_NOZORDER | SWP_NOSIZE);

                //move the Cancel button down
				h_button = GetDlgItem(hdlg, IDCANCEL);
				GetWindowRect(h_button, ref rect);
				pt.x = rect.left;
				pt.y = rect.top;
				ScreenToClient(hdlg, ref pt);

				SetWindowPos(h_button, new IntPtr(0), pt.x, pt.y + shrinkToFitCheckBox.Bounds.Height, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
            }

            return base.HookProc(hdlg, msg, wparam, lparam);
        }

        // events

        /*
         * m_cb_CheckedChanged - 
         * Occurs when the ShrinkToFit Checkbox is clicked
         */
		private void m_cb_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox cb = sender as CheckBox;
			ShrinkToFit = cb.Checked;
		}
    }
}
