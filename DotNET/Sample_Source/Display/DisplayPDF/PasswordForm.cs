using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DisplayPDF
{
    public partial class PasswordForm : Form
    {
        DisplayPDFForm motherform;

        public PasswordForm( DisplayPDFForm topform)
        {
            InitializeComponent();
            motherform = topform;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            motherform.documentpassword = passwordtext.Text;
        }
    }
}
