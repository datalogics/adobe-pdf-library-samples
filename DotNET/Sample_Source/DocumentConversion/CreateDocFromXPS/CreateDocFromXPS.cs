using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 * This sample demonstrates converting an XPS file into a PDF document.
 * 
 * XML Paper Specification (XPS) is a standard document format that Microsoft created in 2006
 * as an alternative to the PDF format.  
 *
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace CreateDocFromXPS
{
    class CreateDocFromXPS
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CreateDocFromXPS sample:");


            using(Library lib = new Library())
            {

                String sInput = Library.ResourceDirectory + "Sample_Input/brownfox.xps";
                String sOutput = "CreateDocFromXPS-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput + ", writing to " + sOutput);

                // First, create an XPSConvertParams to specify conversion parameters 
                // for creating the document.
                XPSConvertParams xpsparams = new XPSConvertParams();
                
                // PDFL requires a .joboptions file to specify settings for XPS conversion.
                // A default .joboptions file is provided in the Resources directory of 
                // the PDFL distribution.  This file is used by default, but a custom file
                // can be used instead by setting the pathToSettingsFile property.
                Console.WriteLine("Using settings file located at: " + xpsparams.PathToSettingsFile);
                
                // Create the document.
                Console.WriteLine("Creating a document from an XPS file...");
	            Document doc = new Document(sInput, xpsparams);
    	        
                // Save the document.
                Console.WriteLine("Saving the document...");
                doc.Save(SaveFlags.Full, sOutput);
    		}
        }
    }
}
