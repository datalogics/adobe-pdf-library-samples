using Datalogics.PDFL;

/*
 * A sample which demonstrates the use of the DLE API to print a
 * PDF file to a windows printer output device.
 * 
 * Copyright 2015, Datalogics, Inc. All rights reserved.
 *
 * The information and code in this sample is for the exclusive use of Datalogics
 * customers and evaluation users only.  Datalogics permits you to use, modify and
 * distribute this file in accordance with the terms of your license agreement.
 * Sample code is for demonstrative purposes only and is not intended for production use.
 *
 */

namespace PrintPDFGUI
{
    public class SamplePrintCancel : PrintCancelProc
    {
        /// <summary>
        /// The method implements a callback (it MUST be named "Call", return a bool and take no arguments)
        /// It will be driven by DLE and if the method returns true printing will attempt to cancel.
        /// </summary>
        /// <returns>A boolean that tells DLE to continue (false) or cancel (true)</returns>
        public override bool Call()
        {   // The cancel callback that will be driven (from the other side) by DLE
            return this.Cancel;
        }

        private bool mCancel = false;
        public bool Cancel
        {   // Keep track of whether to cancel or not
            set
            {
                this.mCancel = value;
            }
            get
            {
                return this.mCancel;
            }
        }
    }
}
