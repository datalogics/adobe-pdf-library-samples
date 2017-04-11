
package com.datalogics.PDFL.Samples;

/*
 * 
 * This sample demonstrates working with masking in PDF documents. Masking an image allows you to remove or 
 * change a feature, while a soft mask allows you to place an image on a page and define the level of 
 * transparency for that image.
 *
 * For more detail see the description of the ImageSoftMask sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/entering-or-generating-graphics-from-pdf-files
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
import com.datalogics.PDFL.*;
import java.util.EnumSet;
public class ImageSoftMask
{

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable {
        System.out.println("Image Soft Mask sample:");

    	Library lib = new Library();

		try
		{
                        String sInput = "../../Resources/Sample_Input/ducky.jpg";
                        String sMask = "../../Resources/Sample_Input/Mask.tif";
                        String sOutput = "ImageSoftMask-out.pdf";
                        
                        if (args.length > 0)
                            sInput = args[0];
                        if (args.length > 1)
                            sMask = args[1];
                        if (args.length > 2)
                            sOutput = args[2];
                        System.out.println("Input file: " + sInput + ", mask: " + sMask + "; will write to " + sOutput);

			System.out.println("Initialized the library.");
			Document doc = new Document();
			Rect pageRect = new Rect(0, 0, 612, 792);
			Page docpage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);

			Image duckyImage;
			duckyImage = new Image(sInput);
			System.out.println("Created the image to mask.");
			
			Image maskImage;
			maskImage = new Image(sMask);
			System.out.println("Created the image to use as mask.");

			duckyImage.setSoftMask(maskImage);
			System.out.println("Issued the setSoftMask method.");

			docpage.getContent().addElement(duckyImage);
			docpage.updateContent();
			System.out.println("Got the content, added the element and updated the content.");

            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
			System.out.println("Saved the document " + sOutput);
		}
		finally {
			lib.delete();
		}
	}

}

