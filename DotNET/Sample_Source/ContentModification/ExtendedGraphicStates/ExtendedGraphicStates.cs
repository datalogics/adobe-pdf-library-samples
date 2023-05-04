using System;
using Datalogics.PDFL;

/*
 * 
 * The Graphics State is an internal data structure in a PDF file that holds the parameters that describe graphics
 * within that file. These parameters define how individual graphics are presented on the page. Adobe Systems introduced
 * the Extended Graphic State to expand the original Graphics State data structure, providing space to define and store
 * more data objects within a PDF.
 * 
 * This sample program shows how to use the Extended Graphic State object to add graphics parameters to an image.
 * 
 * For more detail see the description of the ExtendedGraphicState sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/adding-text-and-graphic-elements#extendedgraphicstates
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ExtendedGraphicStates
{
    class ExtendedGraphicStates
    {
        static void blendPage(Document doc, Image foregroundImage, Image backgroundImage)
        {
            double height = 792;
            double width = 612;
            Rect pageRect = new Rect(0, 0, width, height);
            Page docpage = doc.CreatePage(doc.NumPages - 1, pageRect);

            // This section demonstrates all the Blend Modes one can achieve
            // by setting the BlendMode property to each of the 16 enumerations
            // on a foreground "ducky" over a background rainbow pattern, and 
            // plopping all these images on a single page.

            Text t = new Text();
            Font f;
            try
            {
                f = new Font("Arial", FontCreateFlags.Embedded | FontCreateFlags.Subset);
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

            GraphicState gsText = new GraphicState();
            gsText.FillColor = new Color(0, 0, 1.0);
            TextState ts = new TextState();

            for (int i = 0; i < 16; i++)
            {
                Image individualForegroundImage = foregroundImage.Clone();
                Image individualBackgroundImage = backgroundImage.Clone();

                GraphicState gs = individualForegroundImage.GraphicState;
                individualForegroundImage.Scale(0.125, 0.125);
                individualForegroundImage.Translate(800, 200 + height * (7 - i));
                individualBackgroundImage.Scale(0.125, 0.125);
                individualBackgroundImage.Translate(800, 200 + height * (7 - i));

                // Halfway through, create 2nd column by shifting over and up
                if (i > 7)
                {
                    individualForegroundImage.Translate(2400, height * 8);
                    individualBackgroundImage.Translate(2400, height * 8);
                }

                docpage.Content.AddElement(individualBackgroundImage);
                Console.WriteLine("Added background image " + (i + 1) + " to the content.");
                docpage.Content.AddElement(individualForegroundImage);
                Console.WriteLine("Added foreground image " + (i + 1) + " to the content.");

                Matrix m = new Matrix();
                if (i > 7)
                    m = m.Translate(480, 750 - ((i - 8) * 100)); // second column
                else
                    m = m.Translate(180, 750 - (i * 100)); // first column
                m = m.Scale(12.0, 12.0);

                ExtendedGraphicState xgs = new ExtendedGraphicState();
                TextRun tr = null;
                if (i == 0)
                {
                    xgs.BlendMode = BlendMode.Normal;
                    tr = new TextRun("Normal", f, gsText, ts, m);
                }
                else if (i == 1)
                {
                    xgs.BlendMode = BlendMode.Multiply;
                    tr = new TextRun("Multiply", f, gsText, ts, m);
                }
                else if (i == 2)
                {
                    xgs.BlendMode = BlendMode.Screen;
                    tr = new TextRun("Screen", f, gsText, ts, m);
                }
                else if (i == 3)
                {
                    xgs.BlendMode = BlendMode.Overlay;
                    tr = new TextRun("Overlay", f, gsText, ts, m);
                }
                else if (i == 4)
                {
                    xgs.BlendMode = BlendMode.Darken;
                    tr = new TextRun("Darken", f, gsText, ts, m);
                }
                else if (i == 5)
                {
                    xgs.BlendMode = BlendMode.Lighten;
                    tr = new TextRun("Lighten", f, gsText, ts, m);
                }
                else if (i == 6)
                {
                    xgs.BlendMode = BlendMode.ColorDodge;
                    tr = new TextRun("Color Dodge", f, gsText, ts, m);
                }
                else if (i == 7)
                {
                    xgs.BlendMode = BlendMode.ColorBurn;
                    tr = new TextRun("Color Burn", f, gsText, ts, m);
                }
                else if (i == 8)
                {
                    xgs.BlendMode = BlendMode.HardLight;
                    tr = new TextRun("Hard Light", f, gsText, ts, m);
                }
                else if (i == 9)
                {
                    xgs.BlendMode = BlendMode.SoftLight;
                    tr = new TextRun("SoftLight", f, gsText, ts, m);
                }
                else if (i == 10)
                {
                    xgs.BlendMode = BlendMode.Difference;
                    tr = new TextRun("Difference", f, gsText, ts, m);
                }
                else if (i == 11)
                {
                    xgs.BlendMode = BlendMode.Exclusion;
                    tr = new TextRun("Exclusion", f, gsText, ts, m);
                }
                else if (i == 12)
                {
                    xgs.BlendMode = BlendMode.Hue;
                    tr = new TextRun("Hue", f, gsText, ts, m);
                }
                else if (i == 13)
                {
                    xgs.BlendMode = BlendMode.Saturation;
                    tr = new TextRun("Saturation", f, gsText, ts, m);
                }
                else if (i == 14)
                {
                    xgs.BlendMode = BlendMode.Color;
                    tr = new TextRun("Color", f, gsText, ts, m);
                }
                else if (i == 15)
                {
                    xgs.BlendMode = BlendMode.Luminosity;
                    tr = new TextRun("Luminosity", f, gsText, ts, m);
                }

                t.AddRun(tr);
                docpage.Content.AddElement(t);
                docpage.UpdateContent();
                Console.WriteLine("Updated the content on page 1.");

                gs.ExtendedGraphicState = xgs;
                individualForegroundImage.GraphicState = gs;
                Console.WriteLine("Set blend mode in extended graphic state.");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("ExtendedGraphicStates Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput1 = Library.ResourceDirectory + "Sample_Input/ducky_alpha.tif";
                String sInput2 = Library.ResourceDirectory + "Sample_Input/rainbow.tif";
                String sOutput = "ExtendedGraphicStates-out.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];

                if (args.Length > 1)
                    sInput2 = args[1];

                if (args.Length > 2)
                    sOutput = args[2];

                Console.WriteLine("Input files: " + sInput1 + " and " + sInput2 + ". Saving to output file: " +
                                  sOutput);

                Document doc = new Document();

                Image ImageOne = new Image(sInput1, doc);
                Image imageTwo = new Image(sInput2, doc);

                blendPage(doc, ImageOne, imageTwo);

                blendPage(doc, imageTwo, ImageOne);

                doc.EmbedFonts();
                doc.Save(SaveFlags.Full, sOutput);
            }
        }
    }
}
