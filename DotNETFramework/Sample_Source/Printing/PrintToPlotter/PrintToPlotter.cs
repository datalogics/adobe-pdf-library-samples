using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Datalogics.PDFL;
using Datalogics.Printing;

/*
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace PrintToPlotter
{
    class PrintToPlotter
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("PrintToPlotter Sample:");

            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/ducky.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                Console.WriteLine("Input file: " + sInput);

                Document doc = new Document(sInput);

                // Ask the user how to print
                PrinterSettings settings = new PrinterSettings();

                PrintDialog dialog = new PrintDialog();
                dialog.PrinterSettings = settings;

                dialog.ShowDialog();

#if !MONO                // make a printer out of it
                Printer printer = new Printer(settings.PrinterName);

                if (HPGLPlotter.Supports(printer))
                {
                    Page page = doc.GetPage(0);

                    Rect mb = page.MediaBox;
                    uint width = (uint)(mb.Right - mb.Left);
                    uint height = (uint)(mb.Top - mb.Bottom);

                    // Obtain the resolution via a Graphics created from the settings
                    // for that purpose
                    Graphics g = settings.CreateMeasurementGraphics();
                    uint resX = (uint)g.DpiX;
                    uint resY = (uint)g.DpiY;

                    uint scaled_width = width * resX / 72;
                    uint scaled_height = height * resY / 72;

                    if (settings.PrintToFile)
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.DefaultExt = ".hpg";
                        sfd.FileName = "plot.hpg";
                        if (sfd.ShowDialog() != DialogResult.OK)
                            throw new ApplicationException("User cancelled filename query");
                        settings.PrintFileName = sfd.FileName;
                    }
                        
                    using (HPGLPlotter plotter = new HPGLPlotter(printer, settings.PrintFileName, scaled_width, scaled_height, resX))
                    {
                        // Make a bitmap
                        using (Bitmap bitmap = new Bitmap((int)scaled_width, (int)scaled_height))
                        {
                            // Must invert the page to get from PDF with origin at lower left,
                            // to a bitmap with the origin at upper right.
                            Matrix matrix = new Matrix().Scale(resX / 72, -(int)resY / 72).Translate(0, -height);

                            // The bitmap starts out black, so clear it to white
                            Graphics graphics = Graphics.FromImage(bitmap);
                            graphics.Clear(System.Drawing.Color.White);

                            // Draw directly to the bitmap
                            page.DrawContents(bitmap,
                                matrix, // matrix
                                new Rect(0, 0, width, height));  // updateRect

                            plotter.WriteBitmap(bitmap);
                        }
                    }

                }
#endif

            }
                
        }
    }
}
