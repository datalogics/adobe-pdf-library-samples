package com.datalogics.pdfl.samples.Printing.PrintPDF;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.List;

import com.datalogics.PDFL.*;

/*
 *
 * Run this program to automatically send a PDF file to a printer. The program prompts for
 * the PDF file name, and then it sends the PDF file to the default printer assigned to the
 * computer in use. This program is useful in that it identifies the API you need to use to
 * print PDF files. It also generates a Postscript (PS) output file. 
 *
 * For more detail see the description of this sample on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/printing-pdf-files-and-generating-postscript-ps-files-from-pdf/
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class PrintPDF {
    public static void main (String[] args) throws Throwable
    {
        System.out.println("PrintPDF sample:");

        Library lib = new Library();

        String filename = Library.getResourceDirectory() + "Sample_Input/sample.pdf";
        if (args.length > 0) 
            filename = args[0];

        Document doc = new Document(filename);

		// Platform print to a file...
		//
		// Printed output from the following method is composed by the selected
		// printer's driver; along with assistance from DLE / APDFL. The actual
		// output format will vary (e.g., PCL, PostScript, XPS, etc.). PostScript
		// files produced via a PostScript driver and this method are NOT suitable
		// for Distillation, Normalization, etc. All output from the method below
		// will be optimized specifically for the target printer and is only suitable
		// for direct consumption by that printer (or a print spooler that will
		// ultimately transfer it to the target printer).
		// Get some parameters 
		PrintUserParams userParams = new PrintUserParams();
		// These are the "other" print parameters that hang off the user parameters.
		PrintParams printParams = userParams.getPrintParams();
		
		userParams.setNCopies(1);
		printParams.setShrinkToFit(true);
		printParams.setExpandToFit(true);

		// Printing with specified page ranges
		// Uncomment next code to allow DLE printing with specified page ranges

		// List<PageRange> pageRanges = new ArrayList<PageRange>();

		// pageRanges.add(new PageRange(0, 1, PageSpec.ALL_PAGES));
		// pageRanges.add(new PageRange(0, 1, PageSpec.EVEN_PAGES_ONLY));
		// pageRanges.add(new PageRange(0, 1, PageSpec.ODD_PAGES_ONLY));

		// printParams.setPageRanges(pageRanges);

		// Print printer driver output to a file
		// Why .prn? Because we're printing via the printer driver (here)
		// and cannot tell what type of output the driver will produce.
		// PostScript drivers will produce PostScript output. PCL drivers
		// will produce PCL or PCL_XL. Similarly, XPS drivers may produce
		// XPS and PDF drivers may produce PDF (directly).
		//
		// PostScript produced via the printToFile method is NOT equivalent
		// to PostScript produced via the exportAsPostScript method.
		doc.printToFile(userParams, "PrintPDF_out.prn");


		// Export as (DLE/PDFL composed) PostScript...
		//
		// PostScript files produced via this *export* method are suitable
		// for Distillation, Normalization, etc. If a PostScript Printer
		// Description (PPD) is registered (not shown here) the output will
		// be device specific. Otherwise, it will be device independent.
		// Consult the PostScript Language Document Structuring Conventions
		// for more information about the conformance / structure of the
		// exported PostScript.
		//
		// https://partners.adobe.com/public/developer/en/ps/5001.DSC_Spec.pdf
		//
		// NOTE: userParams are only valid for ONE print job...
		// Get fresh parameters 
		userParams = new PrintUserParams();
		// These are the "other" print parameters that hang off the user parameters.
		printParams = userParams.getPrintParams();
		
		userParams.setNCopies(1);
		
		doc.exportAsPostScript(userParams, "PrintPDF_out.ps");


		// Now let's, print directly to a printer (without ui)...
		//
		// Printed output from the following method is composed by the selected
		// printer's driver; along with assistance from DLE / APDFL. The actual
		// output format will vary (e.g., PCL, PostScript, XPS, etc.). PostScript
		// files produced via a PostScript driver and this method are NOT suitable
		// for Distillation, Normalization, etc. All output from the method below
		// will be optimized specifically for the target printer and is only suitable
		// for direct consumption by that printer (or a print spooler that will
		// ultimately transfer it to the target printer).
		// Get fresh parameters 
		userParams = new PrintUserParams();
		// These are the "other" print parameters that hang off the user parameters.
		printParams = userParams.getPrintParams();
		
		userParams.setNCopies(1);
		printParams.setShrinkToFit(true);
		printParams.setExpandToFit(true);

		// Use the default printer
		userParams.useDefaultPrinter(doc);
		//Print the document and report progress on-screen.
		doc.print(userParams, new PGDPrintCancelProc(false), new PGDPrintProgressProc());


		// Print to a printer
		// For a list of the current print drivers available under WinNT, look at:
		// HKEY_CURRENT_USER\Software\Microsoft\WindowsNT\CurrentVersion\Devices
		// but some special virtual printers modify their ports, so pose a print dialog
		// SetPageSize property and ShrinkToFit are incompatible. So we need to set SetPageSize to False
		// to print to a printer in a proper way.
		//userParams.getPrintParams().setSetPageSize(false);
		//userParams.setShrinkToFit(true);

		//if (userParams.posePrintDialog(doc))

		// #if WIN32
		// This sets the file name that the printer driver knows about
		// userParams.inFileName = args[0];
		// #endif
		//{
			// Added override of dialog box paper height and width
			// because UseMediaBox seems to yield a printed product that more closely
			// resembles what comes out of Acrobat.  This eliminates a number of
			// print problems, including PCL blank page problems.
			//userParams.setPaperHeight(PrintUserParams.USE_MEDIA_BOX);
			//userParams.setPaperWidth(PrintUserParams.USE_MEDIA_BOX);
		    //doc.print(userParams);
		//}
		
		// this properly terminates the library, and is required
		lib.delete();
    }
};
