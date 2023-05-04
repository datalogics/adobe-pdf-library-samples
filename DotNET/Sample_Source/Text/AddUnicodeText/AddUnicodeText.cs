using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * 
 * This sample program adds six lines of Unicode text to a PDF file, in six different languages.
 *
 * For more detail see the description of the AddUnicodeText sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/adding-text-and-graphic-elements#addunicodetext
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 * 
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace AddUnicodeText
{
    class AddUnicodeText
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AddUnicodeText Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sOutput = "AddUnicodeText-out.pdf";

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

                strings.Add("Chinese (Mandarin) - \u4e16\u754c\u4eba\u6743\u5ba3\u8a00");
                strings.Add("Japanese - \u300e\u4e16\u754c\u4eba\u6a29\u5ba3\u8a00\u300f");
                strings.Add("French - \u0044\u00e9\u0063\u006c\u0061\u0072\u0061\u0074\u0069\u006f\u006e" +
                            "\u0020\u0075\u006e\u0069\u0076\u0065\u0072\u0073\u0065\u006c\u006c\u0065\u0020\u0064" +
                            "\u0065\u0073\u0020\u0064\u0072\u006f\u0069\u0074\u0073\u0020\u0064\u0065\u0020\u006c" +
                            "\u2019\u0068\u006f\u006d\u006d\u0065");
                strings.Add("Korean - \uc138\u0020\uacc4\u0020\uc778\u0020\uad8c\u0020\uc120\u0020\uc5b8");
                strings.Add("English - \u0055\u006e\u0069\u0076\u0065\u0072\u0073\u0061\u006c\u0020\u0044" +
                            "\u0065\u0063\u006c\u0061\u0072\u0061\u0074\u0069\u006f\u006e\u0020\u006f\u0066\u0020" +
                            "\u0048\u0075\u006d\u0061\u006e\u0020\u0052\u0069\u0067\u0068\u0074\u0073");
                strings.Add("Greek - \u039f\u0399\u039a\u039f\u03a5\u039c\u0395\u039d\u0399\u039a\u0397\u0020" +
                            "\u0394\u0399\u0391\u039a\u0397\u03a1\u03a5\u039e\u0397\u0020\u0393\u0399\u0391\u0020" +
                            "\u03a4\u0391\u0020\u0391\u039d\u0398\u03a1\u03a9\u03a0\u0399\u039d\u0391\u0020\u0394" +
                            "\u0399\u039a\u0391\u0399\u03a9\u039c\u0391\u03a4\u0391");
                strings.Add("Russian - \u0412\u0441\u0435\u043e\u0431\u0449\u0430\u044f\u0020\u0434\u0435" +
                            "\u043a\u043b\u0430\u0440\u0430\u0446\u0438\u044f\u0020\u043f\u0440\u0430\u0432\u0020" +
                            "\u0447\u0435\u043b\u043e\u0432\u0435\u043a\u0430");

                List<Font> fonts = new List<Font>();
                try
                {
                    fonts.Add(new Font("Arial"));
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

                fonts.Add(new Font("KozGoPr6N-Medium"));
                fonts.Add(new Font("AdobeMyungjoStd-Medium"));

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
                        TextRun tr = new TextRun(font.Name + " - " + str, font, gs, ts, m);
                        unicodeText.AddRun(tr);
                    }

                    // Start the next string lower down the page.
                    y -= 18;
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
