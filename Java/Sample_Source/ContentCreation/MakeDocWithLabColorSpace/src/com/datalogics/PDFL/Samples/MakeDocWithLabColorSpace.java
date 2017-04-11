package com.datalogics.PDFL.Samples;

import com.datalogics.PDFL.Color;
import com.datalogics.PDFL.ColorSpace;
import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Font;
import com.datalogics.PDFL.FontCreateFlags;
import com.datalogics.PDFL.GraphicState;
import com.datalogics.PDFL.LabColorSpace;
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

/*
 * Demonstrates working with the Lab color space. The Lab color space is based on the
 * CIE XYZ color space, but it includes a dimension L, for lightness,
 * along with a and b coordinates, to define the color.
 *
 * For more detail see the description of the ColorSpace sample programs on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-sample-programs/getting-pdf-documents-using-color-spaces
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class MakeDocWithLabColorSpace {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        Library lib = new Library();
        try {
            String sOutput = "LabColorSpace-out.pdf";
            if ( args.length > 0 )
                sOutput = args[0];
            System.out.println("MakeDocWithLabColorSpace: will write " + sOutput ); 
            Document doc = new Document();
            Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, new Rect(0,
                    0, 5 * 72, 4 * 72));
            Content content = page.getContent();
            Font font = new Font("CourierStd", EnumSet.of(FontCreateFlags.EMBEDDED, FontCreateFlags.SUBSET));

            // CIE 1976 L*a*b* space with the CCIR XA/11-recommended D65
            // white point. The a* and b* components, although
            // theoretically unbounded, are defined to lie in the useful
            // range -128 to +127

            List<Double> whitePoint = Arrays.asList(0.9505, 1.0000, 1.0890);
            List<Double> blackPoint = Arrays.asList(0.0, 0.0, 0.0);
            List<Double> range = Arrays.asList(-128.0, 127.0, -128.0, 127.0);
            ColorSpace cs = new LabColorSpace(whitePoint, blackPoint, range);

            GraphicState gs = new GraphicState();
            gs.setFillColor(new Color(cs, Arrays.asList(55.0, -54.0, 55.0)));
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
