using Datalogics.PDFL;
using System;

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

namespace AddHeaderFooter
{
    class AddHeaderFooter
    {
        static void Main(string[] args)
        {
            double fontSize = 12.0;
            double topBottomMargin = 0.5;
            double pageWidth = 8.5 * 72;
            double pageHeight = 11 * 72;
            String headerText = "Title of Document";
            String footerText = "Page 1";

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sOutput = "../AddHeaderFooter-out.pdf";

                Console.WriteLine("Output file: " + sOutput);

                using (Document doc = new Document())
                {
                    Rect pageRect = new Rect(0, 0, pageWidth, pageHeight);

                    // Create the new page
                    using (Page newPage = doc.CreatePage(Document.BeforeFirstPage, pageRect))
                    {
                        Font font = new Font("Times-Roman", FontCreateFlags.Subset);

                        double textHeight  = (font.Ascent + Math.Abs(font.Descent))/1000;

                        GraphicState graphicState = new GraphicState();
                        TextState textState = new TextState();

                        // Calculate the positioning of the Header and Footer
                        Matrix headerMatrix = new Matrix(fontSize, 0, 0, fontSize, (pageWidth - font.MeasureTextWidth(headerText, fontSize)) / 2, pageHeight - topBottomMargin * 72.0 + textHeight);

                        Matrix footerMatrix = new Matrix(fontSize, 0, 0, fontSize, (pageWidth - font.MeasureTextWidth(footerText, fontSize)) /2, topBottomMargin * 72.0 - textHeight);

                        TextRun headerTextRun = new TextRun(headerText, font, graphicState, textState, headerMatrix);

                        TextRun footerTextRun = new TextRun(footerText, font, graphicState, textState, footerMatrix);

                        // Add the Text Run elements to the Text
                        Text text = new Text();
                        text.AddRun(headerTextRun);
                        text.AddRun(footerTextRun);

                        // Add the Text to the Page Content
                        newPage.Content.AddElement(text);
                        newPage.UpdateContent();

                        doc.Save(SaveFlags.Full, sOutput);
                    }
                }
            }
        }
    }
}
