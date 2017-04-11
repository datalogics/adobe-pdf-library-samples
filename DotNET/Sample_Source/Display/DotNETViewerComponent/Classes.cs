/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * Classes.cs
 * 
 * contains various classes that are not large enough to warrant their
 * own files and are used in DotNETViewerComponent
 */
using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace DotNETViewerComponent
{
    #region LayerItems Class
    /*
     * This class is used as a template control for the Layer Manager
     * so that we can quickly create controls for each layer in a document.
     */
    [ToolboxItem(false)]
    public class LayerItems : Control
    {
        #region Members
        public CheckBox displayLayer;
        public CheckBox printLayer;
        public Label layerName;
        #endregion

        #region Constructor
        public LayerItems(int width)
        {
            displayLayer = new CheckBox();
            printLayer = new CheckBox();
            layerName = new Label();

            this.Width = width;
            this.Height = 65;

            this.displayLayer.Visible = true;
            this.displayLayer.Image = global::DotNETViewerComponent.Properties.Resources.EyeCheckbox1;
            this.displayLayer.Size = new Size(60, 45);
            this.displayLayer.Location = new System.Drawing.Point(10, 10);
            this.displayLayer.Checked = true;

            this.printLayer.Visible = true;
            this.printLayer.Image = global::DotNETViewerComponent.Properties.Resources.PrintMenu1;
            this.printLayer.Size = new Size(60, 45);
            this.printLayer.Location = new System.Drawing.Point(80, 10);
            this.printLayer.Checked = true;

            this.layerName.Visible = true;
            this.layerName.Size = new Size(60, 45);
            this.layerName.Location = new System.Drawing.Point(150, 25);

            this.Controls.Add(displayLayer);
            this.Controls.Add(printLayer);
            this.Controls.Add(layerName);
        }
        #endregion
    };
    #endregion

    #region ColorPairWithTransparencyFlags
    /*
     * contains information regarding the current drawing colors
     * and whether or not a color is actually supposed to be transparent
     * 
     * TODO : does this need to be a class or can it be a struct?
     */
    public class ColorPairWithTransparencyFlags
    {
        // Color/transparency flags
        public System.Drawing.Color strokeColor;
        public System.Drawing.Color fillColor;
        public bool strokeColorTransparent = false;
        public bool fillColorTransparent = false;

        public ColorPairWithTransparencyFlags Clone()
        {
            return this.MemberwiseClone() as ColorPairWithTransparencyFlags;
        }
    };
    #endregion

    #region CancelPrinting Exception Class
    public class CancelPrinting : ApplicationException
    {
        // just an empty exception used during printing
    };
    #endregion
}
