using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

/*
 *
 * Use this program to create a new PDF file and add glyphs to the page,
 * managing them by individual Glyph ID codes.
 * 
 * For more detail see the description of the AddGlyphs sample program see
 * https://github.com/datalogics/adobe-pdf-library-samples/tree/master/DotNETFramework/Sample_Source/Text#addglyphs
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace AddGlyphs
{
    class AddGlyphs
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AddGlyphs Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sOutput = "../AddGlyphs-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Output file: " + sOutput);

                Document doc = new Document();

                Rect pageRect = new Rect(0, 0, 612, 792);
                Page docpage = doc.CreatePage(Document.BeforeFirstPage, pageRect);
                Console.WriteLine("Created page.");

                Font font = new Font("Times-Roman");

                List<Char> glyphIDs = new List<Char>();
                glyphIDs.Add('\u002b');
                glyphIDs.Add('\u0028');
                glyphIDs.Add('\u002f');
                glyphIDs.Add('\u002f');
                glyphIDs.Add('\u0032');

                List<Char> unicode = new List<Char>();
                unicode.Add('H');
                unicode.Add('E');
                unicode.Add('L');
                unicode.Add('L');
                unicode.Add('O');

                TextState state = new TextState();
                state.FontSize = 50;

                Matrix m = new Matrix();
                m = m.Translate(docpage.CropBox.Bottom, docpage.CropBox.Right);

                Text text = new Text();
                text.AddGlyphs(glyphIDs, unicode, font, new GraphicState(), state, m, TextFlags.TextRun);

                docpage.Content.AddElement(text);
                docpage.UpdateContent();

                doc.EmbedFonts(EmbedFlags.None);

                doc.Save(SaveFlags.Full, sOutput);

            }
        }
    }
}
