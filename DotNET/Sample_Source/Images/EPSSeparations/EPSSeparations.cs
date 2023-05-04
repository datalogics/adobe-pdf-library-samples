using System;
using System.Collections.Generic;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates working with color separations with Encapsulated PostScript (EPS) graphics
 * from a PDF file.
 *
 * For more detail see the description of the EPSSeparations sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/manipulating-graphics-and-separating-colors-for-images#epsseparations
 * 
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace EPSSeparations
{
    class EPSSeparations
    {
        static void Main(string[] args)
        {
            Console.WriteLine("EPS Separations Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput1 = Library.ResourceDirectory + "Sample_Input/spotcolors1.pdf";
                String sInput2 = Library.ResourceDirectory + "Sample_Input/spotcolors.pdf";

                if (args.Length > 0)
                    sInput1 = args[0];

                if (args.Length > 1)
                    sInput2 = args[1];

                Console.WriteLine("Will perform simple separation on " + sInput1 + " and complex separation on " +
                                  sInput2);

                SimpleSeparationsExample(sInput1);
                ComplexSeparationsExample(sInput2);
            }
        }

        static void SimpleSeparationsExample(String input)
        {
            Document doc = new Document(input);

            Console.WriteLine("Opened " + input);

            // Loop through all pages in the document and make a set of EPS separations
            // for each page, using the default parameters.
            for (int pgNum = 0; pgNum < doc.NumPages; pgNum++)
            {
                Page pg = doc.GetPage(pgNum);

                // First, list the inks on the page and create a SeparationPlate for each ink.
                IList<Ink> pageinks = pg.ListInks();
                List<SeparationPlate> plates = new List<SeparationPlate>();

                foreach (Ink i in pageinks)
                {
                    Console.WriteLine("Found color " + i.ColorantName + " on page " + (pgNum + 1) + ".");
                    SeparationPlate newplate = new SeparationPlate(i,
                        new System.IO.FileStream("Simple-Pg" + (pgNum + 1) + "-" + i.ColorantName + ".eps",
                            System.IO.FileMode.Create));
                    plates.Add(newplate);
                }

                // Set up the parameters for making separations using the plates that were just created.    
                SeparationParams parms = new SeparationParams(plates);

                Console.WriteLine("Making separations for page " + (pgNum + 1) + "...");
                pg.MakeSeparations(parms);

                // Close the output stream for each EPS file.    
                foreach (SeparationPlate p in plates)
                {
                    p.EPSOutput.Close();
                }
            }
        }

        static void ComplexSeparationsExample(String input)
        {
            // This document has 4 pages, each with a combination of CMYK and spot colors.
            Document doc = new Document(input);

            Console.WriteLine("Opened " + input);

            Page pg = doc.GetPage(0);

            // As before, first list the inks on each page.
            IList<Ink> pageinks = pg.ListInks();
            IList<SeparationPlate> plates = new List<SeparationPlate>();

            // Create a Dictionary to hold plate objects that we've created.  On later pages,
            // a lookup can be performed for each page ink to see if a plate has already
            // been created for that ink.
            Dictionary<String, SeparationPlate> colors = new Dictionary<string, SeparationPlate>();

            foreach (Ink i in pageinks)
            {
                Console.WriteLine("Found color " + i.ColorantName + " on page 1.");

                // Change the ink density for the two spot colors that appear on page 1:
                // PANTONE 7442 C and 7467 C.
                //
                // When the plates are created for these inks and placed in the Dictionary,
                // the new density values will be preserved.  When lookups for these inks
                // are performed on the Dictionary for subsequent pages, these plates with
                // the new density values will be used.
                if (i.ColorantName == "PANTONE 7442 C")
                {
                    i.Density = 0.75;
                }
                else if (i.ColorantName == "PANTONE 7467 C")
                {
                    i.Density = 0.50;
                }

                SeparationPlate newplate = new SeparationPlate(i,
                    new System.IO.FileStream("Complex-Pg1-" + i.ColorantName + ".eps", System.IO.FileMode.Create));
                plates.Add(newplate);
                colors.Add(i.ColorantName, newplate);
            }

            // As before, set up the parameters for separations using the newly-created plates.
            SeparationParams parms = new SeparationParams(plates);

            Console.WriteLine("Making separations for page 1...");
            pg.MakeSeparations(parms);

            // Close the output streams for the EPS files.
            foreach (SeparationPlate p in plates)
            {
                p.EPSOutput.Close();
            }

            // Loop over the rest of the pages and reuse plates created for inks on previous pages.
            for (int pgNum = 1; pgNum < doc.NumPages; pgNum++)
            {
                pg = doc.GetPage(pgNum);
                pageinks = pg.ListInks();
                plates = new List<SeparationPlate>();
                SeparationPlate newplate;

                foreach (Ink i in pageinks)
                {
                    if (colors.ContainsKey(i.ColorantName))
                    {
                        newplate = colors[i.ColorantName];
                        newplate.EPSOutput = new System.IO.FileStream(
                            "Complex-Pg" + (pgNum + 1) + "-" + i.ColorantName + ".eps", System.IO.FileMode.Create);
                    }
                    else
                    {
                        newplate = new SeparationPlate(i,
                            new System.IO.FileStream("Complex-Pg" + (pgNum + 1) + "-" + i.ColorantName + ".eps",
                                System.IO.FileMode.Create));
                        colors.Add(i.ColorantName, newplate);
                    }

                    plates.Add(newplate);
                }

                parms.Plates = plates;

                Console.WriteLine("Making separations for page " + (pgNum + 1) + "...");
                pg.MakeSeparations(parms);

                foreach (SeparationPlate p in plates)
                {
                    p.EPSOutput.Close();
                }
            }
        }
    }
}
