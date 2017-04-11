/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*  
 *  frmPassword.cs - 
 * 
 *  This dialog is displayed if the PDF is password protected.
 * 
 *  If the user enters the correct password to unlock all permissions
 *  then the user will be able to read, edit, and save the document.
 * 
 *  If the user enters the correct password to only open the document
 *  then they will only be allowed to read the document.
 * 
 *  If the user opts not to enter a password or enters an incorrect 
 *  password then the file is not opened and they are notified by 
 *  a message box.
 * 
 *  Note: Not all files have a password for all permissions and a 
 *  password for read only, but when we open the file try to open
 *  the document with both. (See DotNETController.cs - OpenPasswordProtectedDocument)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DotNETViewerComponent
{
    public partial class frmPassword : Form
    {
        #region Memebers
        public string password;
        #endregion

        #region Constructor
        public frmPassword()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        private void frmPassword_Load(object sender, EventArgs e)
        {
            //set focus to the textbox where password is entered
            txtPassword.Focus();
        }
        #endregion

        #region Events
        private void btnOK_Click(object sender, EventArgs e)
        {
            password = txtPassword.Text;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            password = "";
            this.Close();
        }
        #endregion
    }
}