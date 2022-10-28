/*
 * 
 * Demonstrates working with the Calibrated Gray Space (CaLGray), based on the CIE color space.
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

package com.datalogics.pdfl.samples.ContentCreation.MakeDocWithCalGrayColorSpace;

import com.datalogics.PDFL.CalGrayColorSpace;
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
import java.util.ArrayList;
import java.util.Arrays;
import java.util.EnumSet;

public class MakeDocWithCalGrayColorSpace {

    public static void main(String[] args) {
        Library lib = new Library();
        try {
            String sOutput = "MakeDocWithCalGrayColorSpace-out.pdf";
            if (args.length > 0)
                sOutput = args[0];
            System.out.println("MakeDocWithCalGrayColorSpace: will write to " + sOutput);
            
            Document doc = new Document();
            Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, new Rect(0,
                    0, 5 * 72, 4 * 72));
            Content content = page.getContent();
            Font font = new Font("CourierStd", EnumSet.of(FontCreateFlags.EMBEDDED, FontCreateFlags.SUBSET));

            // a space consisting of the Y dimension of the CIE 1931 XYZ
            // space with the CCIR XA/11-recommended D65 white point and
            // opto-electronic transfer function.

            ArrayList<Double> whitePoint = new ArrayList<Double>(Arrays.asList(0.9505, 1.0000, 1.0890));
            ArrayList<Double> blackPoint = new ArrayList<Double>(Arrays.asList(0.0, 0.0, 0.0));
            double gamma = 2.222;

            ColorSpace cs = new CalGrayColorSpace(whitePoint, blackPoint, gamma);
            GraphicState gs = new GraphicState();
            gs.setFillColor(new Color(cs, new ArrayList<Double>(Arrays.asList(0.5))));
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
