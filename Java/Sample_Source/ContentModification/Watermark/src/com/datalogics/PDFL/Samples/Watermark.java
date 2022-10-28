package com.datalogics.pdfl.samples.ContentModification.Watermark;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.EnumSet;

import com.datalogics.PDFL.*;

/*
 * The Watermark sample program shows how to create a watermark and copy it to a new PDF file.
 * You could use this code to create a message to apply to PDF files you select, like
 * "Confidential" or "Draft Copy." Or you might want to place a copyright statement over
 * a set of photographs shown in a PDF file so that they cannot be easily duplicated without
 * the permission of the owner.
 * 
 * For more detail see the description of the Watermark sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/adding-text-and-graphic-elements#watermark
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class Watermark {

	/**
	 * @param args
	 * @throws IOException
	 */
	public static void main(String[] args) throws IOException {
		Library lib = new Library();

		try {
			String sInput = Library.getResourceDirectory() + "Sample_Input/sample.pdf";
                        String sWatermark = Library.getResourceDirectory() + "Sample_Input/ducky.pdf";
                        String sOutput = "Watermark-out.pdf";
			if (args.length > 0) 
                            sInput = args[0];
                        if (args.length > 1)
                            sWatermark = args[1];
                        if (args.length > 2)
                            sOutput = args[2];

                        System.out.println ("Adding watermark from " + sWatermark + " to " + sInput
                            + " and saving to " + sOutput);
			Document doc = new Document(sInput);
            System.setProperty("java.awt.headless", "true");

			Document watermarkDoc = new Document(sWatermark);

			WatermarkParams watermarkParams = new WatermarkParams();
			watermarkParams.setOpacity(0.8f);
			watermarkParams.setRotation(45.3f);
			watermarkParams.setScale(0.5f);
			watermarkParams.getTargetRange().setPageSpec(PageSpec.EVEN_PAGES_ONLY);

			doc.watermark(watermarkDoc.getPage(0), watermarkParams);

			watermarkParams.getTargetRange().setPageSpec(PageSpec.ODD_PAGES_ONLY);

			WatermarkTextParams watermarkTextParams = new WatermarkTextParams();
			watermarkTextParams.setText("Multiline\nWatermark");
			
			Font f = new Font("Courier", EnumSet.of(FontCreateFlags.EMBEDDED, FontCreateFlags.SUBSET));
			watermarkTextParams.setFont(f);
			watermarkTextParams.setTextAlign(HorizontalAlignment.CENTER);
			
			Color c = new Color(109.0f/255.0f, 15.0f/255.0f, 161.0f/255.0f);
			watermarkTextParams.setColor(c);
			
			doc.watermark(watermarkTextParams, watermarkParams);

            doc.embedFonts();
			doc.save(EnumSet.of(SaveFlags.FULL, SaveFlags.LINEARIZED), sOutput);
		} finally {
			lib.delete();
		}

	}

}
