/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * LayersManager :
 * 
 * This class is used to manage what layers are currently turned on in the
 * document. Layers can be turned on/off in two different views (one for 
 * displaying to the user and one for controlling which layers are printed).
 * These are controlled INDIVIDUALY!
 * 
 * This class also uses the LayerItems class that defines what controls
 * (checkbox, label, etc.) are displayed for each layer that is found in
 * the document.
 */

using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    [ToolboxItem(false)]
    public class LayersManager : UserControl
    {
        #region Members
        DotNETController dleController;

        public LayerItems masterLayer;    // master layer to turn on/off all layers
        public List<LayerItems> layersInDocument;   // list of layers that are found in the document
        public IList<OptionalContentGroup> docLayers;
        public OptionalContentContext docLayerContext;
        public IList<Boolean> ocgStates;
        #endregion

        #region Constructor
        public LayersManager(DotNETController parentControl)
        {
            dleController = parentControl;

            this.Dock = DockStyle.Fill;
        }
        #endregion

        #region Methods

        public void CreateLayerItems(Document doc)
        {
            DestroyLayers();
            if (doc == null)
            {
                layersInDocument = new List<LayerItems>();
                return;
            }
            int controlHeight = 0;
            docLayerContext = new OptionalContentContext(doc);
            docLayers = doc.OptionalContentGroups;
            ocgStates = docLayerContext.GetOCGStates(docLayers);

            if (docLayers.Count > 1)
            {
                // create the master layer control, turns on/off all layers
                masterLayer = new LayerItems(this.Width);
                masterLayer.layerName.Text = "All Layers";
                masterLayer.Location = new System.Drawing.Point(0, controlHeight);
                masterLayer.displayLayer.Name = "Master";
                masterLayer.printLayer.Name = "Master";

                // add the events for when the checkboxes are clicked
                masterLayer.displayLayer.Click += new EventHandler(dleController.displayLayer_Click);
                masterLayer.printLayer.Click += new EventHandler(dleController.printLayer_Click);

                // increase the height (y coordinate) so we do not overlap controls
                controlHeight += 65;

                // add the controls we just created to the layer manager
                this.Controls.Add(masterLayer);
            }

            layersInDocument = new List<LayerItems>(docLayers.Count);

            // create the layer items for all of the other layers that exist
            for (int i = 0; i < docLayers.Count; i++)
            {
                layersInDocument.Insert(i, new LayerItems(this.Width));
                layersInDocument[i].layerName.Text = doc.OptionalContentGroups[i].Name;
                layersInDocument[i].Location = new System.Drawing.Point(0, controlHeight);

                // add the events for when the checkboxes are clicked
                layersInDocument[i].displayLayer.Click += new EventHandler(dleController.displayLayer_Click);
                layersInDocument[i].printLayer.Click += new EventHandler(dleController.printLayer_Click);

                controlHeight += 65;
            }

            foreach (LayerItems l in layersInDocument) this.Controls.Add(l);

            this.Invalidate();
        }
        public void ResizeLayerControls()
        {
            if (masterLayer != null)
                masterLayer.Width = this.Width;

            if (layersInDocument != null)
            {
                for (int i = 0; i < layersInDocument.Count; i++)
                {
                    layersInDocument[i].Width = this.Width;
                }
            }
        }

        /**
         * destroyLayers - 
         * 
         * Remove the controls for each layer and clear the list of
         * layers. Used when we open a new document.
         */
        public void DestroyLayers()
        {
            docLayers = null;
            docLayerContext = null;
            ocgStates = null;

            if (layersInDocument != null)
            {
                // remove the control for each layer
                foreach (LayerItems l in layersInDocument)
                    this.Controls.Remove(l);

                layersInDocument.Clear();
                layersInDocument = null;

                // remove the master control also
                if (masterLayer != null) this.Controls.Remove(masterLayer);
            }
        }
        #endregion
    }
}
