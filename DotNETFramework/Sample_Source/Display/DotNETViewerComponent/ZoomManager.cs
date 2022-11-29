/*
Copyright (C) 2008-2009, Datalogics, Inc. All rights reserved.
 
The information and code in this sample is for the exclusive use
of Datalogics customers and evaluation users only.  Datalogics 
permits you to use, modify and distribute this file in accordance 
with the terms of your license agreement. Sample code is for 
demonstrative purposes only and is not intended for production use.
*/

/*
 * ZoomManager : 
 * 
 * ZoomManager is here for exactly what it sounds like, to handle
 * the zoom portion of the view.
 */

using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using Datalogics.PDFL;

namespace DotNETViewerComponent
{
    public class ZoomManager
    {
        #region Members
        public bool customZoom = false;

        public String[] zoomArray;
        public int selectedZoomIndex;
        public double currentZoomFactor;
        public FitModes fit;

        public event ZoomLevelChanged onZoomLevelChange;
        public delegate void ZoomLevelChanged(int selectedZoomIndex);

        public event CustomZoomLevelSelected onCustomZoomLevel;
        public delegate void CustomZoomLevelSelected(int customZoomLevel);
        #endregion

        #region Constructors
        public ZoomManager()
        {
            initialize();
        }
        #endregion

        #region Methods
        /**
         * initialize -
         * 
         * setup the default values for the zoom manager and create
         * the zoomArray.
         */
        private void initialize()
        {
            selectedZoomIndex = 3; // 100% by default

            currentZoomFactor = 1.0;
            fit = FitModes.FitNone;

            // this is the list of zoom levels that should be displayed to the user
            zoomArray = new String[13];
            zoomArray[0] = "25%";
            zoomArray[1] = "50%";
            zoomArray[2] = "75%";
            zoomArray[3] = "100%";
            zoomArray[4] = "125%";
            zoomArray[5] = "150%";
            zoomArray[6] = "200%";
            zoomArray[7] = "300%";
            zoomArray[8] = "400%";
            zoomArray[9] = "600%";
            zoomArray[10] = "800%";
            zoomArray[11] = "Fit to Width";
            zoomArray[12] = "Fit to Page";
        }

        private bool LessThanVal(string value)
        {
            double dValue;
            return (Double.TryParse(value.Remove(value.Length - 1), out dValue)) ? currentZoomFactor < (dValue / 100.0) : false;
        }

        private bool LessEqualThanVal(string value)
        {
            double dValue;
            return (Double.TryParse(value.Remove(value.Length - 1), out dValue)) ? currentZoomFactor <= (dValue / 100.0) : false;
        }

        /**
         * increaseZoom - 
         * 
         * increase the zoom percent by 1 step on the list.
         */
        public void increaseZoom()
        {
            if (customZoom)
            {
                int newZoomIndex = Array.FindIndex(zoomArray, LessThanVal);
                if (newZoomIndex == -1)
                    return;

                selectedZoomIndex = newZoomIndex - 1;
                customZoom = false;
            }

            if ((selectedZoomIndex + 1) < zoomArray.Length - 2)
            {
                selectedZoomIndex += 1;
                onZoomLevelChanged(selectedZoomIndex);
            }
        }

        /**
         * decreaseZoom - 
         * 
         * decrease the zoom percent by 1 step on the list.
         */
        public void decreaseZoom()
        {
            if (customZoom)
            {
                int newZoomIndex = Array.FindIndex(zoomArray, LessEqualThanVal);
                if (newZoomIndex == -1)
                    return;

                selectedZoomIndex = newZoomIndex;
                customZoom = false;
            }

            if ((selectedZoomIndex - 1) >= 0 && selectedZoomIndex < zoomArray.Length - 2)
            {
                selectedZoomIndex -= 1;
                onZoomLevelChanged(selectedZoomIndex);
            }
        }

        /**
         * setCustomZoom(double) -
         * 
         * Set the zoom level to a custom level.
         * Returns true if the pageSizeAndLocationArray needs to be
         * recreated.
         */
        public void setCustomZoom(double zoomFactor)
        {
            // store the current zoom factor and then
            // set the new zoom factor
            currentZoomFactor = zoomFactor;
            customZoom = true;
        }

        /**
         * zoomLevelChange(int) -
         * 
         * Changes the current zoom level to that which is selected from
         * our list of possible zoom levels. If fit to page or fit to width
         * are selected it calculates the custom zoom level.
         */
        public void zoomLevelChanged(int selectedZoomIndex)
        {
            if (customZoom && selectedZoomIndex == -1) return; // OK, we just set these values in custom zoom

            this.selectedZoomIndex = selectedZoomIndex;
            string zoomLevel = zoomArray[selectedZoomIndex].ToString();

            if (zoomLevel.Contains("Fit to"))
            {
                fit = zoomLevel.Contains("Page") ? FitModes.FitPage : FitModes.FitWidth;
            }
            else
            {
                // parse the String version of the zoom level to get the double version
                zoomLevel = zoomLevel.Replace("%", "");
                currentZoomFactor = (double.Parse(zoomLevel) / 100);
                fit = FitModes.FitNone;
            }
            customZoom = false;
        }

        /**
         * onZoomLevelChanged - 
         * 
         * fire the onZoomLevelChange event to pass the newly 
         * selected zoom index for the ZoomArray to the 
         * user interface
         */
        public void onZoomLevelChanged(int selectedZoomIndex)
        {
            if (onZoomLevelChange != null)
            {
                onZoomLevelChange(selectedZoomIndex);
            }
        }

        /**
         * onCustomZoomLevel - 
         * 
         * first the onCustomZoomLevel event to pass back the
         * custom zoom level to the user interface
         */
        public void onCustomZoomLevelSelected()
        {
            if (onCustomZoomLevel != null)
            {
                selectedZoomIndex = -1;
                onZoomLevelChanged(selectedZoomIndex);
                onCustomZoomLevel((int)(currentZoomFactor * 100.0));
            }
        }
        #endregion
    }
}