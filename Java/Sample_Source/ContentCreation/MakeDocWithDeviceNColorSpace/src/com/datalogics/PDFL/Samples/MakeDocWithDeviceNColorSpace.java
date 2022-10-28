package com.datalogics.pdfl.samples.ContentCreation.MakeDocWithDeviceNColorSpace;

import com.datalogics.PDFL.*;
import java.util.Arrays;
import java.util.EnumSet;
import java.util.List;

/*
 * This sample demonstrates creating a file containing a DeviceN color space.
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
public class MakeDocWithDeviceNColorSpace {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        Library lib = new Library();
        try {
            String sOutput = "DeviceN-out.pdf";
            if (args.length > 0 )
                sOutput = args[0];
            System.out.println("MakeDocWithDeviceNColorSpace: will create document " + sOutput ); 
            Document doc = new Document();
            Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, new Rect(0, 0, 5 * 72, 4 * 72));
            Content content = page.getContent();
            Font font = new Font("CourierStd", EnumSet.of(FontCreateFlags.EMBEDDED, FontCreateFlags.SUBSET));

            ColorSpace alternate = ColorSpace.DEVICE_RGB;

            List<Double> domain = Arrays.asList(0.0, 1.0, 0.0, 1.0);
            List<Double> range = Arrays.asList(0.0, 1.0, 0.0, 1.0, 0.0, 1.0);
            String code = "{0 exch}"; // r b -> r 0 b

            Function tintTransform = new PostScriptCalculatorFunction(domain, range, code);

            ColorSpace cs = new com.datalogics.PDFL.DeviceNColorSpace(
                    Arrays.asList("DLRed", "DLBlue"),
                    alternate, tintTransform);
            GraphicState gs = new GraphicState();
            gs.setFillColor(new Color(cs, Arrays.asList(0.75, 0.75)));
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
