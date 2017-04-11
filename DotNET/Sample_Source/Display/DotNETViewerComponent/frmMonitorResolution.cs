/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*  
 *  frmMonitorResolution.cs - 
 * 
 *  This dialog is displayed when the user selects 'Monitor Resolution' from the Options menu.
 * 
 *  The smaller the DPI specified, the smaller the image will appear within the program.
 * 
 *  Valid DPI values
 *      - between 36 and 288
 */

using System;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotNETViewerComponent
{
    public partial class frmMonitorResolution : Form
    {
        #region Members
        public int monitorResolution;
        #endregion

        #region Constructor
        public frmMonitorResolution()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        private void frmMonitorResolution_Load(object sender, EventArgs e)
        {
            txtMonitorResolution.Focus();
            txtMonitorResolution.Text = monitorResolution.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtMonitorResolution.Text != "")
                monitorResolution = int.Parse(txtMonitorResolution.Text);

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
       
        /**
         * txtMonitorResolution_Validating - 
         * 
         * validates the new DPI entered by the user
         * 
         * valid value for DPI
         *  - between 36 and 288
         */
        private void txtMonitorResolution_Validating(object sender, CancelEventArgs e)
        {
            txtMonitorResolution.Text = txtMonitorResolution.Text.Trim();

            // Valid only if, whole line is #'s 0-9
            if (Regex.IsMatch(txtMonitorResolution.Text, "^[0-9]+$"))
            {
                //Valid only if, greater than or equal to 36 and less than or eqaul to 288
                if (int.Parse(txtMonitorResolution.Text) >= 36 && int.Parse(txtMonitorResolution.Text) <= 288)
                {
                    lblValidationMsg.Visible = false;
                    e.Cancel = false;
                }
                else
                {
                    lblValidationMsg.Visible = true;
                    e.Cancel = true;
                }
            }
            else
            {
                lblValidationMsg.Visible = true;
                e.Cancel = true;
            }
        }
        #endregion
    }
}