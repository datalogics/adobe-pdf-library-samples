using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * This sample demonstrates how to change the color of hyperlink text (usually blue).
 * 
 * The program works by identifying text in a PDF file that is associated with hyperlinks.
 * Each link appears as a rectangle layer in the PDF file; ChangeLinkColors identifies these
 * rectangles, and then finds the text that lines up within these rectangles and changes the
 * color of each character that is a part of the hyperlink.  
 * 
 * For more detail see the description of the ChangeLinkColors sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/working-with-annotations#changelinkcolors
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace ChangeLinkColors
{
    class ChangeLinkColors
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ChangeLinkColors Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample_links.pdf";
                String sOutput = "ChangeLinkColors-out.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                if (args.Length > 1)
                    sOutput = args[1];

                Console.WriteLine("Input file: " + sInput + ", writing to " + sOutput);

                Document doc = new Document(sInput);

                Console.WriteLine("Opened a document.");

                Page page = doc.GetPage(0);

                List<LinkAnnotation> linkAnnots = new List<LinkAnnotation>();

                // First, make a list of all the link annotations on the page.
                for (int i = 0; i < page.NumAnnotations; i++)
                {
                    Annotation annot = page.GetAnnotation(i);
                    if (annot is LinkAnnotation)
                    {
                        linkAnnots.Add(annot as LinkAnnotation);
                    }
                }

                Content content = page.Content;

                // Iterate over the page's content and process the Text objects.
                FindAndProcessText(content, linkAnnots);

                page.UpdateContent();
                doc.Save(SaveFlags.Full, sOutput);

                doc.Close();
            }
        }

        static void FindAndProcessText(Content content, List<LinkAnnotation> linkAnnots)
        {
            // This function recursively searches the given content for Text objects.
            for (int i = 0; i < content.NumElements; i++)
            {
                Element element = content.GetElement(i);
                if (element is Container)
                {
                    Content nestedContent = ((Container) element).Content;
                    FindAndProcessText(nestedContent, linkAnnots);
                }
                else if (element is Form)
                {
                    Content nestedContent = ((Form) element).Content;
                    FindAndProcessText(nestedContent, linkAnnots);
                }
                else if (element is Group)
                {
                    Content nestedContent = ((Group) element).Content;
                    FindAndProcessText(nestedContent, linkAnnots);
                }
                else if (element is Text)
                {
                    Console.WriteLine("Found a Text object.");
                    CheckCharactersInText(element as Text, linkAnnots);
                }
            }
        }

        static void CheckCharactersInText(Text txt, List<LinkAnnotation> linkAnnots)
        {
            // This function checks to see if any characters in this Text object
            // fall within the bounds of the LinkAnnotations on this page.
            for (int i = 0; i < linkAnnots.Count; i++)
            {
                int charIndex;

                // Find the index of the first character in this Text object that intersects
                // with the LinkAnnotation rectangle, if there is one.
                for (charIndex = 0; charIndex < txt.NumberOfCharacters; charIndex++)
                {
                    if (txt.RectIntersectsCharacter(linkAnnots[i].Rect, charIndex))
                        break;
                }

                if (charIndex >= txt.NumberOfCharacters)
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
                Matrix charMatrix = txt.GetTextMatrixForCharacter(charIndex);
                bool hWithinLinkBox = (Math.Floor(linkAnnots[i].Rect.LLx) < Math.Ceiling(charMatrix.H)) &&
                                      (Math.Floor(charMatrix.H) < Math.Ceiling(linkAnnots[i].Rect.URx));
                bool vWithinLinkBox = (Math.Floor(linkAnnots[i].Rect.LLy) < Math.Ceiling(charMatrix.V)) &&
                                      (Math.Floor(charMatrix.V) < Math.Ceiling(linkAnnots[i].Rect.URy));

                if (hWithinLinkBox && vWithinLinkBox)
                {
                    Console.WriteLine("Found a character that falls within the bounds of LinkAnnotation " + i);

                    int startRunIndex;

                    if (charIndex == 0)
                    {
                        // This is the first character, no splitting needed.
                        startRunIndex = 0;
                    }
                    else
                    {
                        txt.SplitTextRunAtCharacter(charIndex);
                        startRunIndex = txt.FindTextRunIndexForCharacter(charIndex);
                    }

                    // Now search for the last character that intersects with the
                    // LinkAnnotation rectangle.
                    while (charIndex < txt.NumberOfCharacters)
                    {
                        if (!(txt.RectIntersectsCharacter(linkAnnots[i].Rect, charIndex)))
                            break;
                        charIndex++;
                    }

                    int endRunIndex;

                    if (charIndex >= txt.NumberOfCharacters)
                    {
                        // The LinkAnnotation rect goes past the bounding box of
                        // this Text object, so the endRunIndex is the last TextRun 
                        // in this Text.
                        endRunIndex = txt.NumberOfRuns - 1;
                    }
                    else
                    {
                        txt.SplitTextRunAtCharacter(charIndex);
                        endRunIndex = txt.FindTextRunIndexForCharacter(charIndex);
                    }

                    // Change the GraphicState on all the TextRuns from
                    // startRunIndex to endRunIndex.
                    for (int runIndex = startRunIndex; runIndex <= endRunIndex; runIndex++)
                    {
                        TextRun txtRun = txt.GetRun(runIndex);
                        GraphicState gs = txtRun.GraphicState;
                        gs.FillColor = new Color(0.0, 0.0, 1.0);
                        txtRun.GraphicState = gs;
                    }
                }
            }
        }
    }
}
