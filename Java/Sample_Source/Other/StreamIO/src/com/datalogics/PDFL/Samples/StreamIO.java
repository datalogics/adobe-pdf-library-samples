package com.datalogics.pdfl.samples.Other.StreamIO;

import java.util.EnumSet;
import java.io.*;
import javax.imageio.stream.*;

import com.datalogics.PDFL.*;

/*
 *
 * Run this program to extract content from a PDF file from a stream. The system will prompt you to enter
 * the name of a PDF file to use for input. The program generates two output PDF files.
 *
 * A stream is a string of bytes of any length, embedded in a PDF document with a dictionary
 * that is used to interpret the values in the stream. 
 * 
 * This program is similar to ImagefromStream, but in this example the PDF file streams hold text.
 *
 * For more detail see the description of the StreamIO sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/exporting-images-from-pdf-files/#streamio
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class StreamIO {

	/**
	 * @param args
	 */

	public static void main(String[] args) throws IOException {
        System.out.println("StreamIO sample:");
        
        System.setProperty("java.awt.headless", "true");

    	Library lib = new Library();

		try {
            String filename = Library.getResourceDirectory() + "Sample_Input/sample.pdf";
            String sOutput1 = "StreamIO-out1.pdf";
            String sOutput2 = "StreamIO-out2.pdf";
            if (args.length > 0)
                filename = args[0];
            if (args.length > 1)
                sOutput1 = args[1];
            if (args.length > 2)
                sOutput2 = args[2];
            
            System.out.println("Reading " + filename + " and writing " +
                sOutput1 + " and " + sOutput2 );

            StreamIOSample.readFromStream(filename,sOutput1);
            StreamIOSample.writeToStream(filename,sOutput2);

		}
		finally {
			lib.delete();
		}
	}

    public static class StreamIOSample {
        
        /**
        * Demontrates reading from stream.
        * @param filename - name of the input file from which stream is created.
        */
        static void readFromStream(String filename, String output) throws java.io.IOException {
            //First we create InputStream from the file.
            InputStream in = new FileInputStream(filename);
            //Second we create ImageInputStream that is required by DLE interface. 
            //We use MemoryCacheImageInputStream, this means that buffer will be cached in memory.
            //Another option is to use FileCacheImageInputStream with temporary file cache.
            ImageInputStream iin = new javax.imageio.stream.MemoryCacheImageInputStream(in);

            // A document is then opened, using the ImageInputStream as its data source.
	        Document doc = new Document(iin);

			// Add a watermark to have some visible change to the PDF
            WatermarkParams watermarkParams = new WatermarkParams();
            watermarkParams.getTargetRange().setPageSpec(PageSpec.ALL_PAGES);
            WatermarkTextParams watermarkTextParams = new WatermarkTextParams();
            watermarkTextParams.setText("this pdf was opened\nfrom a stream");
            doc.watermark(watermarkTextParams, watermarkParams);
         
            //save document
            doc.save(EnumSet.of(SaveFlags.FULL, SaveFlags.LINEARIZED), output);
            doc.delete();
        }

        /*
        * Demonstrate writing a PDF Document to the OutputStream.
        */
        static void writeToStream(String filename, String output) throws java.io.IOException{
	        Document doc = new Document(filename);

		    // Add some content to the Document so there will be something to see.
            doc.setCreator("DLE StreamIO Sample");	        
            Page p = doc.createPage(Document.BEFORE_FIRST_PAGE, new Rect(0, 0, 612, 792));
            addContentToPage(p);
            
            //First create OutputStream
            OutputStream out = new FileOutputStream(output);
            //Second create ImageOutputStream that is required by DLE interface. 
            //MemoryCacheImageOutputStream or FileCacheImageOutputStream can be used.
            ImageOutputStream ios = new javax.imageio.stream.MemoryCacheImageOutputStream(out); 
            //Save the document
            System.out.println("Saving ...");
            doc.save(EnumSet.of(SaveFlags.OPTIMIZE_MARKED_JBIG_2DICTIONARIES), ios);
            //Flush ImageOutputStream to uderlaying OutputStream
            //ios.flush();
            ios.close();
            //out.close();
            doc.delete();
        }

        private static void addContentToPage(Page p)
        {
            Path rect = new Path();
            rect.addRect(new Point(100, 100), 100, 100);
            GraphicState gs = new GraphicState();
            gs.setStrokeColor(new Color(0, 1.0, 0));
            gs.setWidth(3.0);
            rect.setGraphicState(gs);
            rect.setPaintOp(EnumSet.of(PathPaintOpFlags.STROKE));
            p.getContent().addElement(rect);
            p.updateContent();
        }

    }
}
