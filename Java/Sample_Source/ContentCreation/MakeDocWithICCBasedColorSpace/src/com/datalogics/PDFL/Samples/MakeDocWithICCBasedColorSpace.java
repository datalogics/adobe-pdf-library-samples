package com.datalogics.pdfl.samples.ContentCreation.MakeDocWithICCBasedColorSpace;

import com.datalogics.PDFL.*;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.util.Arrays;
import java.util.EnumSet;

/*
 * This sample demonstrates creating a file containing an ICC-based color space.
 *
 * For more detail see the description of the ColorSpace sample programs on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/getting-pdf-documents-using-color-spaces
 *
 * Copyright (c) 2008-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class MakeDocWithICCBasedColorSpace {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) throws FileNotFoundException {
        Library lib = new Library();
        try {
            
            String sInput = Library.getResourceDirectory() + "Sample_Input/sRGB_IEC61966-2-1_noBPC.icc";
            String sOutput = "ICCBased-out.pdf";
            
            if ( args.length > 0 )
                sInput = args[0];
            
            if ( args.length > 1 )
                sOutput = args[1];
            
            System.out.println("MakeDocWithICCBasedColorSpace: Using input " + sInput +  " will create document " + sOutput );
            
            Document doc = new Document();
            Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, new Rect(0, 0, 5 * 72, 4 * 72));
            Content content = page.getContent();
            Font font = new Font("CourierStd", EnumSet.of(FontCreateFlags.EMBEDDED, FontCreateFlags.SUBSET));

            FileInputStream stream = new FileInputStream(sInput);
            PDFStream pdfStream = new PDFStream(stream, doc, null, null);
            ColorSpace cs = new ICCBasedColorSpace(pdfStream, 3);
            GraphicState gs = new GraphicState();
            gs.setFillColor(new Color(cs, Arrays.asList(1.0, 0.0, 0.0)));
            gs.setWidth(1.0);

            Matrix textMatrix = new Matrix(24, 0, 0, 24, // Set font width and height to 24 point size
                    1 * 72, 2 * 72);   // x, y coordinate on page, 1" x 2"

            TextRun textRun = new TextRun("Hello World!", font, gs, new TextState(), textMatrix);
            Text text = new Text();
            text.addRun(textRun);
            content.addElement(text);
            page.updateContent();

            doc.embedFonts();
            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
        } finally {
            lib.delete();
        }

    }
}
