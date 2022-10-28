package com.datalogics.pdfl.samples.Images.ImageEmbedICCProfile;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.PDFStream;

/*
 * 
 * This sample program demonstrates how to embed an ICC color profile in a graphics file.
 * The program sets up how the output will be rendered and generates a TIF image file or
 * series of TIF files as output.
 * 
 * For more detail see the description of the ImageEmbedICCProfile sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/exporting-images-from-pdf-files/#imageembediccprofile
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class ImageEmbedICCProfile {

	/**
	 * @param args
	 * @throws IOException
	 */
	public static void main(String[] args) throws Exception {
		System.out.println("Image Embed ICC Profile sample:");

		Library lib = new Library();

		try {
			String filename = Library.getResourceDirectory() + "Sample_Input/ducky.pdf";
			String profilename = Library.getResourceDirectory() + "Sample_Input/Probev1_ICCv2.icc";

			if (args.length > 0) {
				filename = args[0];
                        }

			Document doc = new Document(filename);

			if (args.length > 1) {
				profilename = args[1];
                        }

                        System.out.println("Input file " + filename + " Profile: " + profilename);
			ExportDocImage expdoc = new ExportDocImage();

			FileInputStream profileFileStream = new FileInputStream(profilename);
			PDFStream profilePDFStream = new PDFStream(profileFileStream, doc,
					null, null);

			expdoc.export_doc_images(doc, profilePDFStream);
		} finally {
			lib.delete();
		}

	}

}
