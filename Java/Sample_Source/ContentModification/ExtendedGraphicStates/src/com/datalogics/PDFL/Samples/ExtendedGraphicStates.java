package com.datalogics.pdfl.samples.ContentModification.ExtendedGraphicStates;

/*
 * 
 * The Graphics State is an internal data structure in a PDF file that holds the parameters that describe graphics
 * within that file. These parameters define how individual graphics are presented on the page. Adobe Systems introduced
 * the Extended Graphic State to expand the original Graphics State data structure, providing space to define and store
 * more data objects within a PDF.
 * 
 * This sample program shows how to use the Extended Graphic State object to add graphics parameters to an image.
 * 
 * For more detail see the description of the ExtendedGraphicState sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/adding-text-and-graphic-elements#extendedgraphicstates
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
import com.datalogics.PDFL.BlendMode;
import com.datalogics.PDFL.Color;
import com.datalogics.PDFL.Content;
import com.datalogics.PDFL.Document;
import com.datalogics.PDFL.ExtendedGraphicState;
import com.datalogics.PDFL.Font;
import com.datalogics.PDFL.FontCreateFlags;
import com.datalogics.PDFL.GraphicState;
import com.datalogics.PDFL.Image;
import com.datalogics.PDFL.Library;
import com.datalogics.PDFL.Matrix;
import com.datalogics.PDFL.Page;
import com.datalogics.PDFL.Rect;
import com.datalogics.PDFL.SaveFlags;
import com.datalogics.PDFL.Text;
import com.datalogics.PDFL.TextRun;
import com.datalogics.PDFL.TextState;
import java.util.EnumSet;

public class ExtendedGraphicStates {

    static void blendPage(Document doc, Image foregroundImage, Image backgroundImage) {

        // This section demonstrates all the Blend Modes one can achieve
        // by setting the BlendMode property to each of the 16 enumerations
        // on a foreground image over a background image, and
        // plopping all these images on a single page.
        double height = 792;
        double width = 612;

        Rect pageRect = new Rect(0, 0, width, height);
        Page docpage = doc.createPage(doc.getNumPages() - 1, pageRect);
        Text t = new Text();
        GraphicState gsText = new GraphicState();
        gsText.setFillColor(new Color(0, 0, 1.0));
        TextState ts = new TextState();
        Font f = null;
        TextRun tr = null;
        Image individualForegroundImage = null;
        Image individualBackgroundImage = null;
        Matrix m = null;
        GraphicState gs = null;
        ExtendedGraphicState xgs = null;
        Content pgContent = null;

        try {
            pgContent = docpage.getContent();
            f = new Font("CourierStd", EnumSet.of(FontCreateFlags.EMBEDDED, FontCreateFlags.SUBSET));

            for (int i = 0; i < 16; i++) {
                try {
                    individualForegroundImage = foregroundImage.clone();
                    individualBackgroundImage = backgroundImage.clone();

                    gs = individualForegroundImage.getGraphicState();
                    individualForegroundImage.scale(0.125, 0.125);
                    individualForegroundImage.translate(800, 200 + height * (7 - i));
                    individualBackgroundImage.scale(0.125, 0.125);
                    individualBackgroundImage.translate(800, 200 + height * (7 - i));

                    // Halfway through, create 2nd column by shifting over and
                    // up
                    if (i > 7) {
                        individualForegroundImage.translate(2400, height * 8);
                        individualBackgroundImage.translate(2400, height * 8);
                    }

                    m = new Matrix();
                    if (i > 7) {
                        m = m.translate(480, 750 - ((i - 8) * 100)); // second
                                                                     // column
                    } else {
                        m = m.translate(180, 750 - (i * 100)); // first column
                    }
                    m = m.scale(12.0, 12.0);

                    xgs = new ExtendedGraphicState();
                    if (i == 0) {
                        xgs.setBlendMode(BlendMode.NORMAL);
                        tr = new TextRun("Normal", f, gsText, ts, m);
                    } else if (i == 1) {
                        xgs.setBlendMode(BlendMode.MULTIPLY);
                        tr = new TextRun("Multiply", f, gsText, ts, m);
                    } else if (i == 2) {
                        xgs.setBlendMode(BlendMode.SCREEN);
                        tr = new TextRun("Screen", f, gsText, ts, m);
                    } else if (i == 3) {
                        xgs.setBlendMode(BlendMode.OVERLAY);
                        tr = new TextRun("Overlay", f, gsText, ts, m);
                    } else if (i == 4) {
                        xgs.setBlendMode(BlendMode.DARKEN);
                        tr = new TextRun("Darken", f, gsText, ts, m);
                    } else if (i == 5) {
                        xgs.setBlendMode(BlendMode.LIGHTEN);
                        tr = new TextRun("Lighten", f, gsText, ts, m);
                    } else if (i == 6) {
                        xgs.setBlendMode(BlendMode.COLOR_DODGE);
                        tr = new TextRun("Color Dodge", f, gsText, ts, m);
                    } else if (i == 7) {
                        xgs.setBlendMode(BlendMode.COLOR_BURN);
                        tr = new TextRun("Color Burn", f, gsText, ts, m);
                    } else if (i == 8) {
                        xgs.setBlendMode(BlendMode.HARD_LIGHT);
                        tr = new TextRun("Hard Light", f, gsText, ts, m);
                    } else if (i == 9) {
                        xgs.setBlendMode(BlendMode.SOFT_LIGHT);
                        tr = new TextRun("SoftLight", f, gsText, ts, m);
                    } else if (i == 10) {
                        xgs.setBlendMode(BlendMode.DIFFERENCE);
                        tr = new TextRun("Difference", f, gsText, ts, m);
                    } else if (i == 11) {
                        xgs.setBlendMode(BlendMode.EXCLUSION);
                        tr = new TextRun("Exclusion", f, gsText, ts, m);
                    } else if (i == 12) {
                        xgs.setBlendMode(BlendMode.HUE);
                        tr = new TextRun("Hue", f, gsText, ts, m);
                    } else if (i == 13) {
                        xgs.setBlendMode(BlendMode.SATURATION);
                        tr = new TextRun("Saturation", f, gsText, ts, m);
                    } else if (i == 14) {
                        xgs.setBlendMode(BlendMode.COLOR);
                        tr = new TextRun("Color", f, gsText, ts, m);
                    } else if (i == 15) {
                        xgs.setBlendMode(BlendMode.LUMINOSITY);
                        tr = new TextRun("Luminosity", f, gsText, ts, m);
                    }
                    t.addRun(tr);

                    pgContent.addElement(individualBackgroundImage);
                    System.out.println("Added background image " + (i + 1) + " to the content.");

                    System.out.println("Set blend mode in extended graphic state.");
                    gs.setExtendedGraphicState(xgs);
                    individualForegroundImage.setGraphicState(gs);

                    pgContent.addElement(individualForegroundImage);
                    System.out.println("Added foreground image " + (i + 1) + " to the content.");

                    pgContent.addElement(t);

                } finally {
                    tr.delete();
                    individualBackgroundImage.delete();
                    individualForegroundImage.delete();
                    gs.delete();
                    xgs.delete();
                    m.delete();
                }
            }

            docpage.updateContent();
            System.out.println("Updated the content on page " + doc.getNumPages() + ".");
        } finally {
            t.delete();
            f.delete();
            gsText.delete();
            ts.delete();
            pgContent.delete();
            docpage.delete();
        }
    }

    /**
     * @param args
     */
    public static void main(String[] args) throws Throwable {
        System.out.println("ExtendedGraphicStates sample:");

        Library lib = new Library();
        Document doc = null;
        Image ducky = null;
        Image rainbow = null;

        try {
            String sInput1 = Library.getResourceDirectory() + "Sample_Input/ducky_alpha.tif";
            String sInput2 = Library.getResourceDirectory() + "Sample_Input/rainbow.tif";
            String sOutput = "ExtendedGraphicStates-out.pdf";
            System.out.println("Initialized the library.");

            doc = new Document();

            if (args.length > 0)
                sInput1 = args[0];
            if (args.length > 1)
                sInput2 = args[1];
            if (args.length > 2)
                sOutput = args[2];
            System.out.println("Input images: " + sInput1 + " and " + sInput2 + 
                  "; will write to " + sOutput);
            ducky = new Image(sInput1, doc);
            rainbow = new Image(sInput2, doc);

            // Ducky on top of rainbow
            blendPage(doc, ducky, rainbow);

            // Rainbow on top of ducky
            blendPage(doc, rainbow, ducky);

            doc.embedFonts();
            doc.save(EnumSet.of(SaveFlags.FULL), sOutput);
            System.out.println("Saved the document " + sOutput);

            // Kill the doc object
            System.out.println("Killed document object.");
        } finally {
            ducky.delete();
            rainbow.delete();
            doc.delete();
            lib.delete();
        }
    }
}
