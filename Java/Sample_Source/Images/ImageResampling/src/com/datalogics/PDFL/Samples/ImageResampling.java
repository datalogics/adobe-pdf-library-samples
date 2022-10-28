package com.datalogics.pdfl.samples.Images.ImageResampling;

/*
 * 
 * This sample demonstrates how to find and resample images within a PDF document. The images are
 * then saved to a new PDF output document with a new resolution.
 * 
 * Resampling involves resizing an image or images within a PDF document. Commonly this process is 
 * used to reduce the resolution of an image or series of images, to make them smaller. As a result
 * the process makes the PDF document smaller, too.
 *
 * For more detail see the description of the ImageResampling sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/entering-or-generating-graphics-from-pdf-files
 * 
 *
 * Copyright (c) 2008-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.EnumSet;

import com.datalogics.PDFL.*;

public class ImageResampling
{
	static int numreplaced;
    public static void ResampleImages(Content content ) throws Exception
    {
        int i = 0;
        while (i < content.getNumElements())
        {
            Element e = content.getElement(i);
            System.out.println(i + " / "+content.getNumElements()+" = " + e.toString());
            if (e instanceof Image)
            {
                Image img = (Image)e;
                try
                {
                    Image newimg = img.changeResolution(200);
                    System.out.println("Replacing an image...");
                    content.addElement((Element)newimg, i);
                    content.removeElement(i);
                    System.out.println("Replaced.");
                    numreplaced++;
                }
                catch (Exception ex)
                {
                	System.out.println(ex.getMessage());
                }
            }
            else if (e instanceof Container)
            {
            	System.out.println("Recursing through a Container");
                ResampleImages(((Container)e).getContent());
            }
            else if (e instanceof Group)
            {
            	System.out.println("Recursing through a Group");
                ResampleImages(((Group)e).getContent());
            }
            else if (e instanceof Form)
            {
            	System.out.println("Recursing through a Form");
                Content formcontent = (((Form)e).getContent());
                ResampleImages(formcontent);
                ((Form)e).setContent(formcontent);
            }
            i++;
        }
    }

    public static void main(String [] args) throws Exception
    {
        System.out.println("ImageResampling sample:");
    	
        Library lib = new Library();

        try
        {
                String sInput = Library.getResourceDirectory() + "Sample_Input/ducky.pdf";
                String sOutput = "ImageResampling-out.pdf";
	        if (args.length > 0) {
	            sInput = args[0];
                }
                if (args.length > 1 ) {
                    sOutput = args[1];
                }
                System.out.println("Input file: " + sInput + ", will write to " + sOutput);
	        
	       	Document doc = new Document(sInput);
	       	System.out.println("Opened a document.");
	       	try
	       	{
	       		int	pgno;
	       		for ( pgno = 0; pgno < doc.getNumPages(); pgno++)
	       		{
	       			numreplaced = 0;
	       			Page pg = doc.getPage(pgno);
	       			Content content = pg.getContent();
	       			ResampleImages(content);
	       			if ( numreplaced != 0)
	       			{
	       				pg.updateContent();       				
	       			}
	       		}
	       		doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
	       	}
	       	catch (Exception ex)
	       	{
	       		System.out.println(ex.getMessage());
	       	}
       	}
        finally
        {
        	lib.delete();
        }
    }
}
