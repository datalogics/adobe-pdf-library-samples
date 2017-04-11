/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * frmAnnotationProperties.cs
 * 
 * A form that can be used to allow a user to edit the Title/Contents
 * of an Annotation.
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
    public partial class frmAnnotationProperties : Form
    {
        #region Constructors
        public frmAnnotationProperties()
        {
            InitializeComponent();
        }

        public frmAnnotationProperties(String title, String contents)
        {
            InitializeComponent();
            annotationTitleTextBox.Text = title;
            annotationContentsTextBox.Text = contents;
        }
        #endregion

        #region Properties
        public String AnnotationTitle
        {
            get { return annotationTitleTextBox.Text.ToString(); }
        }

        public String AnnotationContents
        {
            get { return annotationContentsTextBox.Text.ToString(); }
        }
        #endregion
    }
}