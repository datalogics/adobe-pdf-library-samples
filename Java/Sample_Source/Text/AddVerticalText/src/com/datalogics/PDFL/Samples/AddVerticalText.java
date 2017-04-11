package com.datalogics.PDFL.Samples;

/*
 *
 * This program describes how to render text from top to bottom on a page. The program includes an
 * enumerator called WritingMode that is set to "Vertical." It also provides several rows of Unicode
 * characters to serve as sample text. 
 *
 * The PDF output file presents multiple columns of vertical text. The characters appear in English as well as
 * Mandarin, Japanese, and Korean.
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.EmbedFlags;
import com.datalogics.PDFL.Font;
import com.datalogics.PDFL.FontCreateFlags;
import com.datalogics.PDFL.WritingMode;
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
import java.util.List;
import java.util.EnumSet;

public class AddVerticalText {

    /**
     * @param args
     */
    public static void main(String[] args) throws Throwable {
        System.out.println("AddVerticalText sample:");
        String sOutput = "AddVerticalText-out.pdf";
        Library lib = new Library();

        try {
            if (args.length != 0)
                sOutput = args[0];
            System.out.println("Output file: " + sOutput);
            System.out.println("Initialized the library.");
            Document doc = new Document();
            Rect pageRect = new Rect(0, 0, 612, 792);
            Page docpage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);

            Text unicodeText = new Text();
            GraphicState gs = new GraphicState();
            TextState ts = new TextState();

            List<String> strings = new ArrayList<String>();

            strings.add("\u0055\u006e\u0069\u0076\u0065\u0072\u0073\u0061\u006c\u0020\u0044\u0065\u0063\u006c\u0061\u0072\u0061\u0074\u0069\u006f\u006e\u0020\u006f\u0066\u0020\u0048\u0075\u006d\u0061\u006e\u0020\u0052\u0069\u0067\u0068\u0074\u0073");
            strings.add("\u4e16\u754c\u4eba\u6743\u5ba3\u8a00");
            strings.add("\u300e\u4e16\u754c\u4eba\u6a29\u5ba3\u8a00\u300f");
            strings.add("\uc138\u0020\uacc4\u0020\uc778\u0020\uad8c\u0020\uc120\u0020\uc5b8");

            // Create the fonts with vertical WritingMode.  We don't need any special
            // FontCreateFlags, so we just pass zero
            List<Font> fonts = new ArrayList<Font>();
            fonts.add(new Font("KozGoPr6N-Medium", EnumSet.noneOf(FontCreateFlags.class), WritingMode.VERTICAL));
            fonts.add(new Font("AdobeMyungjoStd-Medium", EnumSet.noneOf(FontCreateFlags.class), WritingMode.VERTICAL));

            // These will be used to place the strings into position on the page.
            int x = 1 * 72;
            int y = 10 * 72;

            for (String str : strings) {
                // Find a font that can represent all characters in the string, if there is one.
                Font font = GetRepresentableFont(fonts, str);
                if (font == null) {
                    System.out.println("Couldn't find a font that can represent all characters in the string: " + str);
                } else {
                    // From this point, the string is handled the same way as non-Unicode text.
                    Matrix m = new Matrix(14, 0, 0, 14, x, y);
                    TextRun tr = new TextRun(str, font, gs, ts, m);
                    unicodeText.addRun(tr);
                }

                // Start the next string moving across the page to the right
                x += 30;
            }
            docpage.getContent().addElement(unicodeText);
            docpage.updateContent();

            // Save the document.
            System.out.println("Embedding fonts.");
            doc.embedFonts(EnumSet.of(EmbedFlags.NONE));
            System.out.println("Saving to " + sOutput);
            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);

        } finally {
            lib.delete();
        }
    }

    public static Font GetRepresentableFont(List<Font> fonts, String str) {
        for (Font font : fonts) {
            if (font.isTextRepresentable(str)) {
                return font;
            }
        }
        return null;
    }
}
