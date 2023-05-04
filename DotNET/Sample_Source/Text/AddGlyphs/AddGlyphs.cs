using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 *
 * Use this program to create a new PDF file and add glyphs to the page,
 * managing them by individual Glyph ID codes.
 * 
 * For more detail see the description of the AddGlyphs sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/adding-text-and-graphic-elements#addglyphs
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
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


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sOutput = "AddGlyphs-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Output file: " + sOutput);

                Document doc = new Document();

                Rect pageRect = new Rect(0, 0, 612, 792);
                Page docpage = doc.CreatePage(Document.BeforeFirstPage, pageRect);
                Console.WriteLine("Created page.");

                Font font;
                try
                {
                    font = new Font("Times-Roman");
                }
                catch (ApplicationException ex)
                {
                    if (ex.Message.Equals("The specified font could not be found.") &&
                        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                            .OSPlatform.Linux) &&
                        !System.IO.Directory.Exists("/usr/share/fonts/msttcore/"))
                    {
                        Console.WriteLine("Please install Microsoft Core Fonts on Linux first.");
                        return;
                    }

                    throw;
                }

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
