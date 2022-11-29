using System;
using System.Windows.Forms;

/*
 * Copyright (c) 2007-2013, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.  Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 *
 * Demonstrates a GUI-based browser for PDF objects.  This dialog prompts for passwords.
 */

namespace PDF_Object_Explorer
{
    public partial class frmPassword : Form
    {
        public string password;

        
        public frmPassword()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void frmPassword_Load(object sender, EventArgs e)
        {
            txtPassword.Focus();
        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
          
        }


        private void btnOK_Click(object sender, EventArgs e)
        {
            password = txtPassword.Text;
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            password = "";
            this.Close();
        }
    }
}
