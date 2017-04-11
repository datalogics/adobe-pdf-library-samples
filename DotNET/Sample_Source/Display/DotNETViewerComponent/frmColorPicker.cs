/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * frmColorPicker.cs
 * 
 * The ColorPicker is a form that can be displayed to change the colors
 * that are used to draw annotations/create free text annotations. It contains
 * two pictureboxes, when the user clicks on either of them it displays a 
 * color dialog so the user can select their color. These pictureboxes overlap 
 * each other, the one that is further left is the fill color, making the one
 * further right the stroke color.
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
    public partial class frmColorPicker : Form
    {
        #region Members
        public ColorPairWithTransparencyFlags colorPair;
        public ColorPairWithTransparencyFlags savedColorPair;
        private ColorPairWithTransparencyFlags newColorPair;
        #endregion

        #region Constructor
        public frmColorPicker()
        {
            InitializeComponent();
            colorPair = new ColorPairWithTransparencyFlags();
            savedColorPair = new ColorPairWithTransparencyFlags();
            newColorPair = new ColorPairWithTransparencyFlags();
        }
        #endregion

        #region Events
        /** 
         * frmColorPicker_Load - 
         * 
         * when the form loads we set the color of the pictureboxes
         * in the form to the color that we have saved, this displays
         * the last colors used in the document or the colors that are
         * used in the currently selected annotation.
         */
        private void frmColorPicker_Load(object sender, EventArgs e)
        {
            if (savedColorPair.fillColorTransparent)
            {
                fillColorPictureBox.Image = global::DotNETViewerComponent.Properties.Resources.transparencySymbol100x100;
            }
            else
            {
                fillColorPictureBox.BackColor = savedColorPair.fillColor;
                fillColorPictureBox.Image = null;
            }

            if (savedColorPair.strokeColorTransparent)
            {
                strokeColorPictureBox.Image = global::DotNETViewerComponent.Properties.Resources.transparencySymbol100x100;
            }
            else
            {
                strokeColorPictureBox.BackColor = savedColorPair.strokeColor;
                strokeColorPictureBox.Image = null;
            }

            newColorPair = savedColorPair;
        }

        /**
         * OKBtn_Click -
         * 
         * Accepts the new colors that the user has selected
         */
        private void OKBtn_Click(object sender, EventArgs e)
        {
            colorPair = newColorPair;
            this.Close();
        }

        /**
         * cancelBtn_Click -
         * 
         * Ignores the new colors that the user has selected and
         * continues to use the previous colors.
         */
        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /**
         * swapBtn_Click - 
         * 
         * Swaps the fill and the stroke colors. The stroke color
         * is not allowed to be transparent.
         */
        private void swapBtn_Click(object sender, EventArgs e)
        {
            Color tempColor = newColorPair.fillColor;

            newColorPair.fillColor = newColorPair.strokeColor;
            newColorPair.fillColorTransparent = false;

            newColorPair.strokeColor = tempColor;

            if (newColorPair.fillColorTransparent)
            {
                fillColorPictureBox.Image = global::DotNETViewerComponent.Properties.Resources.transparencySymbol100x100;
            }
            else
            {
                fillColorPictureBox.BackColor = newColorPair.fillColor;
                fillColorPictureBox.Image = null;
            }

            strokeColorPictureBox.BackColor = newColorPair.strokeColor;
            strokeColorPictureBox.Image = null;
        }

        /**
         * fillTransparentBtn_Click - 
         * 
         * Set the fillColor to be transparent.
         */
        private void fillTransparentBtn_Click(object sender, EventArgs e)
        {
            newColorPair.fillColorTransparent = true;
            fillColorPictureBox.Image = global::DotNETViewerComponent.Properties.Resources.transparencySymbol100x100;
        }

        /**
         * fillColorPictureBox_Click -
         * 
         * Display a ColorDialog for the user to select
         * their color, change the color of the 
         * fillColorPictureBox to that color, and turn off
         * the transparent flag.
         */
        private void fillColorPictureBox_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = fillColorPictureBox.BackColor;
            
            colorDialog.ShowDialog();
            newColorPair.fillColor = colorDialog.Color;
            newColorPair.fillColorTransparent = false;
            fillColorPictureBox.BackColor = newColorPair.fillColor;
            fillColorPictureBox.Image = null;
        }

        /**
         * 
         * strokeColorPictureBox_Click -
         * Display a ColorDialog for the user to select
         * their color, change the color of the 
         * strokeColorPictureBox to that color, and turn off
         * the transparent flag.
         */
        private void strokeColorPictureBox_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = strokeColorPictureBox.BackColor;

            colorDialog.ShowDialog();
            newColorPair.strokeColor = colorDialog.Color;
            newColorPair.strokeColorTransparent = false;
            strokeColorPictureBox.BackColor = newColorPair.strokeColor;
            strokeColorPictureBox.Image = null;
        }
        #endregion
    }
}