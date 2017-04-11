package com.datalogics.PDFL.Samples;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.EnumSet;

import com.datalogics.PDFL.Color;
import com.datalogics.PDFL.ColorSpace;
import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.Font;
import com.datalogics.PDFL.FontCreateFlags;
import com.datalogics.PDFL.GraphicState;
import com.datalogics.PDFL.IndexedColorSpace;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.Text;
import com.datalogics.PDFL.TextRun;
import com.datalogics.PDFL.TextState;

/*
 * This sample demonstrates working with the Indexed Color Space. Indexed Color Spaces are used
 * to reduce the amount of system memory used for processing images when a limited
 * number of colors are needed.
 *
 * For more detail see the description of the ColorSpace sample programs on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/getting-pdf-documents-using-color-spaces
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class MakeDocWithIndexedColorSpace {

	/**
	 * @param args
	 */
	public static void main(String[] args) {
		Library lib = new Library();
		try {
                        String sOutput = "IndexedColorSpace-out.pdf";
                        if ( args.length > 0 )
                            sOutput = args[0];
                        System.out.println("MakeDocWithIndexedColorSpace: will write " + sOutput ); 
			Document doc = new Document();
			Page page = doc.createPage(Document.BEFORE_FIRST_PAGE, new Rect(0,
					0, 5 * 72, 4 * 72));
			Content content = page.getContent();
            Font font = new Font("CourierStd", EnumSet.of(FontCreateFlags.EMBEDDED, FontCreateFlags.SUBSET));

			ColorSpace base = ColorSpace.DEVICE_RGB;

			ArrayList<Integer> lookup = new ArrayList<Integer>();

			for (Integer r : Arrays.asList(0, 255)) {
				for (Integer g : Arrays.asList(0, 255)) {
					for (Integer b : Arrays.asList(0, 255)) {
						lookup.add(r);
						lookup.add(g);
						lookup.add(b);
					}
				}
			}

			IndexedColorSpace cs = new IndexedColorSpace(base, 7, lookup);

			GraphicState gs = new GraphicState();
			gs.setFillColor(new Color(cs, Arrays.asList(4.0)));
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
