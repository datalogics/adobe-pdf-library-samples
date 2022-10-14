package com.datalogics.pdfl.samples.ContentCreation.AddHeaderFooter;

/*
 *
 * This sample demonstrates creating a new PDF document with a Header and Footer.
 * 
 * Copyright (c) 2022, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
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
import java.util.EnumSet;

public class AddHeaderFooter {

    public static void main(String[] args) throws Throwable {
        double fontSize = 12.0;
        double topBottomMargin = 0.5;
        double pageWidth = 612;
        double pageHeight = 792;
        String headerText = "Title of Document";
        String footerText = "Page 1";

        System.out.println("AddHeaderFooter sample:");
        String sOutput = "AddHeaderFooter-out.pdf";
        Library lib = new Library();

        try {
            System.out.println("Output file: " + sOutput);
            System.out.println("Initialized the library.");

            Document doc = new Document();
            Rect pageRect = new Rect(0, 0, pageWidth, pageHeight);

            // Create the new page
            Page newPage = doc.createPage(Document.BEFORE_FIRST_PAGE, pageRect);

            GraphicState graphicState = new GraphicState();
            TextState textState = new TextState();

            Font font = new Font("Times-Roman", EnumSet.of(FontCreateFlags.SUBSET));
            double textHeight = (font.getAscent() + Math.abs(font.getDescent()))/1000;

            // Calculate the positioning of the Header and Footer
            Matrix headerMatrix = new Matrix(fontSize, 0, 0, fontSize, (pageWidth - font.measureTextWidth(headerText, fontSize)) / 2, pageHeight - topBottomMargin * 72.0 + textHeight);

            Matrix footerMatrix = new Matrix(fontSize, 0, 0, fontSize, (pageWidth - font.measureTextWidth(footerText, fontSize)) / 2, topBottomMargin * 72.0 - textHeight);

            TextRun headerTextRun = new TextRun(headerText, font, graphicState, textState, headerMatrix);

            TextRun footerTextRun = new TextRun(footerText, font, graphicState, textState, footerMatrix);

            // Add the Text Run elements to the Text
            Text text = new Text();
            text.addRun(headerTextRun);
            text.addRun(footerTextRun);

            // Add the Text to the Page Content
            newPage.getContent().addElement(text);
            newPage.updateContent();

            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);

            newPage.delete();
            doc.delete();
        } finally {
            lib.delete();
        }
    }
}
