package com.datalogics.pdfl.samples.ContentCreation.MakeDocWithSeparationColorSpace;

import com.datalogics.PDFL.*;
import java.util.Arrays;
import java.util.EnumSet;
import java.util.List;

/*
* This sample demonstrates creating a PDF document that uses a Separation color space.
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
public class MakeDocWithSeparationColorSpace {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        Library lib = new Library();
        try {
            String sOutput = "SeparationColorSpace-out.pdf";
            if ( args.length > 0 )
                sOutput = args[0];
            System.out.println("MakeDocWithSeparationColorSpace: will write " + sOutput ); 
            Document doc = new Document();
            Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, new Rect(0, 0, 5 * 72, 4 * 72));
            Content content = page.getContent();
            Font font = new Font("CourierStd", EnumSet.of(FontCreateFlags.EMBEDDED, FontCreateFlags.SUBSET));

            ColorSpace alternate = ColorSpace.DEVICE_RGB;

            List<Double> domain = Arrays.asList(0.0, 1.0);
            int nOutputs = 3;
            List<Double> range = Arrays.asList(0.0, 1.0, 0.0, 1.0, 0.0, 1.0);
            List<Double> C0 = Arrays.asList(0.0, 0.0, 0.0);
            List<Double> C1 = Arrays.asList(1.0, 0.0, 0.0);

            Function tintTransform = new ExponentialFunction(domain, nOutputs, C0, C1, 1.0);
            tintTransform.setRange(range);

            ColorSpace cs = new com.datalogics.PDFL.SeparationColorSpace("DLColor", alternate, tintTransform);
            GraphicState gs = new GraphicState();
            gs.setFillColor(new Color(cs, Arrays.asList(1.0)));
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
