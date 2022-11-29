using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DotNETViewerComponent
{
    public partial class frmLinkAnnotationProperties : Form
    {
        #region Constructors
        public frmLinkAnnotationProperties()
        {
            InitializeComponent();
            viewRect.Checked = true;
            zoomCheckBox.Enabled = false;
            positionCheckBox.Enabled = false;
        }
        #endregion
        #region Methods
        private void customParams_CheckedChanged(object sender, EventArgs e)
        {
            if (customParams.Checked)
            {
                zoomCheckBox.Enabled = true;
                positionCheckBox.Enabled = true;
                zoomCheckBox.Checked = true;
                positionCheckBox.Checked = true;
            }
            else
            {
                zoomCheckBox.Enabled = false;
                positionCheckBox.Enabled = false;
            }
        }
        private void SetLink_Click(object sender, EventArgs e)
        {
            isCustom =  customParams.Checked;
            isZoom = zoomCheckBox.Checked;
            isPosition = positionCheckBox.Checked;
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            isCustom = false;
            isZoom = false;
            isPosition = false;
            this.Close();
        }
        #endregion
        #region Properties
        public bool IsCustom { get { return isCustom; } }
        public bool IsZoom { get { return isZoom; } }
        public bool IsPosition { get { return isPosition; } }
        #endregion
        #region Members
        private bool isCustom;
        private bool isZoom;
        private bool isPosition;
        #endregion

        
    }
}