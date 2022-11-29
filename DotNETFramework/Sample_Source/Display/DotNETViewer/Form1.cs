using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DotNETViewerComponent;
//using Datalogics.PDFL;

namespace DotNETViewer
{
    public partial class Form1 : Form
    {
        // DotNETViewer component object
        DotNETViewerComponent.DotNETViewer myViewer;

        public Form1()
        {
            // create a new instance of the DotNETViewer component
            myViewer = new DotNETViewerComponent.DotNETViewer();

            // for this example the DotNETViewer is placed at (0, 0) in the form
            // and set to fill the form. These values can be changed.
            myViewer.Location = new Point(0, 0);
            myViewer.Dock = DockStyle.Fill;

            // add the new DotNETViewer component to the form
            this.Controls.Add(myViewer);

            InitializeComponent();
            if (DotNETViewer.commandLineArgs != null &&
                DotNETViewer.commandLineArgs.GetLength(0) > 0 &&
                DotNETViewer.commandLineArgs[0] == "--test")
            {
                myViewer.RunTests();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!myViewer.Controller.docView.DocChangeRequest())
            {
                e.Cancel = true;
            }
            else
            {
                myViewer.Controller.docView.CloseDocument();
            }
        }
    }
}