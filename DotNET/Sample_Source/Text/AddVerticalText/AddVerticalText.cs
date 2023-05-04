using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * 
 * This program describes how to render text from top to bottom on a page.  The program provides
 * a WritingMode “Vertical” with a string of Unicode characters to present sample text. 
 * 
 * The sample offers several rows of Unicode characters. The sample PDF file thus presents multiple columns
 * of vertical text.  The characters appear in English as well as Mandarin, Japanese, and Korean.
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace AddVerticalText
{
    class AddVerticalText
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AddVerticalText Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sOutput = "AddVerticalText-out.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Output file: " + sOutput);

                Document doc = new Document();
                Rect pageRect = new Rect(0, 0, 612, 792);
                Page docpage = doc.CreatePage(Document.BeforeFirstPage, pageRect);

                Text unicodeText = new Text();
                GraphicState gs = new GraphicState();
                TextState ts = new TextState();

                List<String> strings = new List<String>();

                strings.Add("\u0055\u006e\u0069\u0076\u0065\u0072\u0073\u0061\u006c\u0020\u0044\u0065\u0063" +
                            "\u006c\u0061\u0072\u0061\u0074\u0069\u006f\u006e\u0020\u006f\u0066\u0020\u0048\u0075" +
                            "\u006d\u0061\u006e\u0020\u0052\u0069\u0067\u0068\u0074\u0073");
                strings.Add("\u4e16\u754c\u4eba\u6743\u5ba3\u8a00");
                strings.Add("\u300e\u4e16\u754c\u4eba\u6a29\u5ba3\u8a00\u300f");
                strings.Add("\uc138\u0020\uacc4\u0020\uc778\u0020\uad8c\u0020\uc120\u0020\uc5b8");

                // Create the fonts with vertical WritingMode.  We don't need any special
                // FontCreateFlags, so we just pass zero
                List<Font> fonts = new List<Font>();
                fonts.Add(new Font("KozGoPr6N-Medium", 0, WritingMode.Vertical));
                fonts.Add(new Font("AdobeMyungjoStd-Medium", 0, WritingMode.Vertical));

                // These will be used to place the strings into position on the page.
                int x = 1 * 72;
                int y = 10 * 72;

                foreach (String str in strings)
                {
                    // Find a font that can represent all characters in the string, if there is one.
                    Font font = GetRepresentableFont(fonts, str);
                    if (font == null)
                    {
                        Console.WriteLine(
                            "Couldn't find a font that can represent all characters in the string: " + str);
                    }
                    else
                    {
                        // From this point, the string is handled the same way as non-Unicode text.
                        Matrix m = new Matrix(14, 0, 0, 14, x, y);
                        TextRun tr = new TextRun(str, font, gs, ts, m);
                        unicodeText.AddRun(tr);
                    }

                    // Start the next string moving across the page to the right
                    x += 30;
                }

                docpage.Content.AddElement(unicodeText);
                docpage.UpdateContent();


                // Save the document.
                Console.WriteLine("Embedding fonts.");
                doc.EmbedFonts(EmbedFlags.None);
                doc.Save(SaveFlags.Full, sOutput);
            }
        }

        static Font GetRepresentableFont(List<Font> fonts, String str)
        {
            foreach (Font font in fonts)
            {
                if (font.IsTextRepresentable(str))
                    return font;
            }

            return null;
        }
    }
}
