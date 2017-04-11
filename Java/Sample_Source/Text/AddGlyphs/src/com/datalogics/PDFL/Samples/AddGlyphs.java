package com.datalogics.PDFL.Samples;


import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;

import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.EmbedFlags;
import com.datalogics.PDFL.Font;
import com.datalogics.PDFL.FontCreateFlags;
import com.datalogics.PDFL.GraphicState;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.Text;
import com.datalogics.PDFL.TextFlags;
import com.datalogics.PDFL.TextState;

/*
 *
 * Use this program to create a new PDF file and add glyphs to the page,
 * managing them by individual Glyph ID codes.
 * 
 * For more detail see the description of the AddGlyphs sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/adding-text-and-graphic-elements#addglyphs
 * 
 *
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class AddGlyphs {

	/**
	 * @param args
	 */
        public static void main(String[] args) throws Throwable {
        System.out.println("AddGlyphs sample:");

        Library lib = new Library();
        String fontName = "Arial";
        String sOutput = "AddGlyphs-out.pdf";
        
        try {

            if (args.length != 0)
                sOutput = args[0];
            System.out.println("Output file: " + sOutput);
            Document doc = new Document();
            Rect pageRect = new Rect(0, 0, 612, 792);
            Page docpage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);
            System.out.println("Created page.");

            Font font = new Font(fontName);

            List<Character> glyphIDs = new ArrayList<Character>();
            glyphIDs.add(new Character('\u002b'));
            glyphIDs.add(new Character('\u0028'));
            glyphIDs.add(new Character('\u002f'));
            glyphIDs.add(new Character('\u002f'));
            glyphIDs.add(new Character('\u0032'));

            List<Character> unicode = new ArrayList<Character>();
            unicode.add(new Character('H'));
            unicode.add(new Character('E'));
            unicode.add(new Character('L'));
            unicode.add(new Character('L'));
            unicode.add(new Character('O'));

            TextState state = new TextState();
            state.setFontSize(50);

            Matrix m = new Matrix();
            m = m.translate(docpage.getCropBox().getBottom(), docpage.getCropBox().getRight());

            Text text = new Text();
            text.addGlyphs(glyphIDs, unicode, font, new GraphicState(), state, m, EnumSet.of(TextFlags.TEXT_RUN));

            docpage.getContent().addElement(text);
            docpage.updateContent();

            doc.embedFonts(EnumSet.of(EmbedFlags.NONE));

            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
        }
        catch (Exception e) {
            if(e.getMessage().equals("The specified font could not be found."))
                System.out.println(fontName + " font is not installed in your system, please select a different font.");
            else
                System.out.println(e.toString());
        }
        finally {
            lib.delete();
        }
    }
}
