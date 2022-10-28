package com.datalogics.pdfl.samples.ContentModification.ChangeLinkColors;

import java.util.*;
import java.lang.Math;

import com.datalogics.PDFL.*;

/*
* This sample demonstrates how to change the color of hyperlink text (usually blue).
 * 
 * The program works by identifying text in a PDF file that is associated with hyperlinks.
 * Each link appears as a rectangle layer in the PDF file; ChangeLinkColors identifies these
 * rectangles, and then finds the text that lines up within these rectangles and changes the
 * color of each character that is a part of the hyperlink.  
 * 
 * For more detail see the description of the ChangeLinkColors sample program on our Developer’s site,
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/working-with-annotations#changelinkcolors
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
public class ChangeLinkColors {

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Throwable {
        System.out.println("ChangeLinkColors Sample:");

        Library lib = new Library();

        try {
            String sInput = Library.getResourceDirectory() + "Sample_Input/sample_links.pdf";
            String sOutput = "ChangeLinkColors-out.pdf";
            if (args.length > 0)
                sInput = args[0];
            if (args.length > 1)
                sOutput = args[1];
            System.out.println("Input file: " + sInput + ", writing to " + sOutput);
            Document doc = new Document(sInput);

            System.out.println("Opened a document.");

            Page page = doc.getPage(0);
                
            ArrayList<LinkAnnotation> linkAnnots = new ArrayList<LinkAnnotation>();
            
            // First, make a list of all the link annotations on the page.
            for(int i=0; i < page.getNumAnnotations(); i++)
            {
                Annotation annot = page.getAnnotation(i);
                if(annot instanceof LinkAnnotation)
                {
                    linkAnnots.add((LinkAnnotation)annot);
                }
            }
                
            Content content = page.getContent();

            // Iterate over the page's content and process the Text objects.
            findAndProcessText(content, linkAnnots);

            page.updateContent();
            doc.save(EnumSet.of(SaveFlags.FULL), "ChangeLinkColors-out.pdf");

            doc.close();
        }
        finally {
            lib.delete();
        }
    }

    static void findAndProcessText(Content content, ArrayList<LinkAnnotation> linkAnnots) {
        // This function recursively searches the given content for Text objects.
        for (int i = 0; i < content.getNumElements(); i++)
        {
            Element element = content.getElement(i);
            if (element instanceof Container)
            {
                Content nestedContent = ((Container)element).getContent();
                findAndProcessText(nestedContent, linkAnnots);
            }
            else if (element instanceof Form)
            {
                Content nestedContent = ((Form)element).getContent();
                findAndProcessText(nestedContent, linkAnnots);
            }
            else if (element instanceof Group)
            {
                Content nestedContent = ((Group)element).getContent();
                findAndProcessText(nestedContent, linkAnnots);
            }
            else if (element instanceof Text)
            {
                System.out.println("Found a Text object.");
                checkCharactersInText((Text)element, linkAnnots);
            } 
        }
    }

    static void checkCharactersInText(Text txt, ArrayList<LinkAnnotation> linkAnnots) {
        // This function checks to see if any characters in this Text object
        // fall within the bounds of the LinkAnnotations on this page.
        for(int i=0; i < linkAnnots.size(); i++)
        {               
            int charIndex;

            // Find the index of the first character in this Text object that intersects
            // with the LinkAnnotation rectangle, if there is one.
            for(charIndex = 0; charIndex < txt.getNumberOfCharacters(); charIndex++)
            {                   
                if(txt.rectIntersectsCharacter(linkAnnots.get(i).getRect(), charIndex))
                    break;
            }
            
            if(charIndex >= txt.getNumberOfCharacters())
            {
                // The LinkAnnotation rect falls outside of this Text object.
                // Try the next rect on the list.
                continue;
            }

            // We may get some false positives using only Text.RectIntersectsCharacter().
            // This function checks to see whether any part of the rectangle lies on the
            // character's bounding box.  It's common for a character's bounding box
            // to be taller than the height of the line; in these cases, the bounding
            // box may intersect a LinkAnnotation rectangle on the line above.
            //
            // We can weed out these false positives by checking the lower left corner
            // of the character's text matrix.  If the lower left corner falls within
            // the rectangle of the LinkAnnotation, then this character really
            // is contained within the LinkAnnotation rectangle.  Taking the floor and 
            // ceiling as shown below accounts for precision errors in the rect and
            // matrix values.
            //
            // This heuristic works well for many cases.  If you find that you're still
            // getting false positives or false negatives, you may need to tweak the
            // heuristic.
            Matrix charMatrix = txt.getTextMatrixForCharacter(charIndex);
            Boolean hWithinLinkBox = (Math.floor(linkAnnots.get(i).getRect().getLLx()) < Math.ceil(charMatrix.getH())) && (Math.floor(charMatrix.getH()) < Math.ceil(linkAnnots.get(i).getRect().getURx()));
            Boolean vWithinLinkBox = (Math.floor(linkAnnots.get(i).getRect().getLLy()) < Math.ceil(charMatrix.getV())) && (Math.floor(charMatrix.getV()) < Math.ceil(linkAnnots.get(i).getRect().getURy()));

            if (hWithinLinkBox && vWithinLinkBox)
            {
                System.out.println("Found a character that falls within the bounds of LinkAnnotation " + i);
                
                int startRunIndex;
                
                if(charIndex == 0)
                {
                    // This is the first character, no splitting needed.
                    startRunIndex = 0;
                }
                else
                {
                    txt.splitTextRunAtCharacter(charIndex);
                    startRunIndex = txt.findTextRunIndexForCharacter(charIndex);
                }
                
                // Now search for the last character that intersects with the
                // LinkAnnotation rectangle.
                while (charIndex < txt.getNumberOfCharacters())
                {
                    if (!(txt.rectIntersectsCharacter(linkAnnots.get(i).getRect(), charIndex)))
                        break;
                    charIndex++;
                }

                int endRunIndex;

                if (charIndex >= txt.getNumberOfCharacters())
                {
                    // The LinkAnnotation rect goes past the bounding box of
                    // this Text object, so the endRunIndex is the last TextRun 
                    // in this Text.
                    endRunIndex = txt.getNumberOfRuns() - 1;
                }
                else
                {
                    txt.splitTextRunAtCharacter(charIndex);
                    endRunIndex = txt.findTextRunIndexForCharacter(charIndex);
                }

                // Change the GraphicState on all the TextRuns from
                // startRunIndex to endRunIndex.
                for (int runIndex = startRunIndex; runIndex <= endRunIndex; runIndex++)
                {
                    TextRun txtRun = txt.getRun(runIndex);
                    GraphicState gs = txtRun.getGraphicState();
                    gs.setFillColor(new Color(0.0, 0.0, 1.0));
                    txtRun.setGraphicState(gs);
                }
            }
        }
    }
}
