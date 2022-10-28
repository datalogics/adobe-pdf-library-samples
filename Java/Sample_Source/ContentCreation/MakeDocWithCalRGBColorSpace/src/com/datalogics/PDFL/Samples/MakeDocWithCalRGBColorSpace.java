/*
 * 
 * Demonstrates working with the Calibrated RGB Color Space, based on the CIE color space.
 * A, B, and C represent red, blue, and green color values.
 *
 * For more detail see the description of the ColorSpace sample programs on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/getting-pdf-documents-using-color-spaces
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
package com.datalogics.pdfl.samples.ContentCreation.MakeDocWithCalRGBColorSpace;

import com.datalogics.PDFL.CalRGBColorSpace;
import com.datalogics.PDFL.Color;
import com.datalogics.PDFL.ColorSpace;
import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Font;
import com.datalogics.PDFL.FontCreateFlags;
import com.datalogics.PDFL.GraphicState;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.Text;
import com.datalogics.PDFL.TextRun;
import com.datalogics.PDFL.TextState;
import java.util.Arrays;
import java.util.EnumSet;
import java.util.List;

/**
 *
 */
public class MakeDocWithCalRGBColorSpace {

    public static void main(String[] args) {

        Library lib = new Library();
        try {
            String sOutput = "MakeDocWithCalRGBColorSpace-out.pdf"; 
            if (args.length > 0)
                sOutput = args[0];
            System.out.println("MakeDocWithCalRGBColorSpace:  will write to " + sOutput);
            Document doc = new Document();
            Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, new Rect(0,
                    0, 5 * 72, 4 * 72));
            Content content = page.getContent();
            Font font = new Font("CourierStd", EnumSet.of(FontCreateFlags.EMBEDDED, FontCreateFlags.SUBSET));

            // a CalRGB color space for the CCIR XA/11-recommended D65
            // white point with 1.8 gammas and Sony Trinitron phosphor
            // chromaticities

            List<Double> whitePoint = Arrays.asList(0.9505, 1.0000, 1.0890);
            List<Double> blackPoint = Arrays.asList(0.0, 0.0, 0.0);
            List<Double> gamma = Arrays.asList(1.8, 1.8, 1.8);
            List<Double> matrix = Arrays.asList(0.4497, 0.2446, 0.0252, 0.3163, 0.6720, 0.1412, 0.1845, 0.0833, 0.9227);

            ColorSpace cs = new CalRGBColorSpace(whitePoint, blackPoint, gamma, matrix);
            GraphicState gs = new GraphicState();
            gs.setFillColor(new Color(cs, Arrays.asList(0.3, 0.7, 0.3)));
            gs.setWidth(1.0);

            Matrix textMatrix = new Matrix(24, 0, 0, 24, // Set font width and
                    // height to 24
                    // point size
                    1 * 72, 2 * 72); // x, y coordinate on page, 1" x 2"

            TextRun textRun = new TextRun("Hello World!", font, gs,
                    new TextState(), textMatrix);
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
