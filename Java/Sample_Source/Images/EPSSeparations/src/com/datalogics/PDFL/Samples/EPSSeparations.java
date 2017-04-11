package com.datalogics.PDFL.Samples;

import java.util.*;

import com.datalogics.PDFL.*;

/*
 * 
 * This sample demonstrates working with color separations with Encapsulated PostScript (EPS) graphics
 * from a PDF file.
 *
 * For more detail see the description of the EPSSeparations sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/java-sample-programs/manipulating-graphics-and-separating-colors-for-images#epsseparations
 * 
 * Copyright (c) 2007-2017, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

public class EPSSeparations {

    /**
     * @param args
     */
    public static void main(String[] args) throws Throwable {
        System.out.println("EPS Separations sample:");

        Library lib = new Library();
        String sInput1 = "../../Resources/Sample_Input/spotcolors1.pdf";
        String sInput2 = "../../Resources/Sample_Input/spotcolors.pdf";
        if (args.length > 0)
            sInput1 = args[0];
        if (args.length > 1)
            sInput2 = args[1];
        System.out.println("Will perform simple separation on " + sInput1 +
            " and complex separation on " + sInput2);

        try {
                simpleSeparationsExample(sInput1);
                complexSeparationsExample(sInput2);
        }
        finally {
            lib.delete();
        }
    }
    
    public static void simpleSeparationsExample(String f) throws Throwable {
        // This document has 7 colored squares on a page: CMYK and 3 spot colors.
        Document doc = new Document(f);
        
        // Loop through all pages in the document and make a set of EPS separations
        // for each page, using the default parameters.
        for(int pgNum=0; pgNum < doc.getNumPages(); pgNum++)
        {
            Page pg = doc.getPage(pgNum);

            // First, list the inks on the page and create a SeparationPlate for each ink.
            List<Ink> pageinks = pg.listInks();
            ArrayList<SeparationPlate> plates = new ArrayList<SeparationPlate>();

            for(Ink i : pageinks)
            {
                System.out.println("Found color " + i.getColorantName() + " on page " + (pgNum+1) + ".");
                SeparationPlate newplate = new SeparationPlate(i, new java.io.FileOutputStream("Simple-Pg" + (pgNum+1) + "-" + i.getColorantName() + ".eps"));
                plates.add(newplate);
            }

            // Set up the parameters for making separations using the plates that were just created.
            SeparationParams parms = new SeparationParams(plates);

            System.out.println("Making separations for page " + (pgNum+1) + "...");
            pg.makeSeparations(parms);

            // Close the output stream for each EPS file.
            for(SeparationPlate p : plates)
            {
                p.getEPSOutput().close();
            }
        }
    }
    
    public static void complexSeparationsExample(String f) throws Throwable {
        // This document has 4 pages, each with a combination of CMYK and spot colors.
        Document doc = new Document(f);

        Page pg = doc.getPage(0);

        // As before, first list the inks on each page.
        List<Ink> pageinks = pg.listInks();
        ArrayList<SeparationPlate> plates = new ArrayList<SeparationPlate>();

        // Create a HashMap to hold plate objects that we've created.  On later pages,
        // a lookup can be performed for each page ink to see if a plate has already
        // been created for that ink.
        Map<String, SeparationPlate> colors = new HashMap<String, SeparationPlate>();

        for(Ink i : pageinks)
        {
            System.out.println("Found color " + i.getColorantName() + " on page 1.");

            // Change the ink density for the two spot colors that appear on page 1:
            // PANTONE 7442 C and 7467 C.
            //
            // When the plates are created for these inks and placed in the HashMap,
            // the new density values will be preserved.  When lookups for these inks
            // are performed on the HashMap for subsequent pages, these plates with
            // the new density values will be used.
            if(i.getColorantName() == "PANTONE 7442 C")
            {
                i.setDensity(0.75);
            }
            else if(i.getColorantName() == "PANTONE 7467 C")
            {
                i.setDensity(0.50);
            }
            SeparationPlate newplate = new SeparationPlate(i, new java.io.FileOutputStream("Complex-Pg1-" + i.getColorantName() + ".eps"));
            plates.add(newplate);
            colors.put(i.getColorantName(), newplate);
        }

        // As before, set up the parameters for separations using the newly-created plates.
        SeparationParams parms = new SeparationParams(plates);

        System.out.println("Making separations for page 1...");
        pg.makeSeparations(parms);

        // Close the output streams for the EPS files.
        for(SeparationPlate p : plates)
        {
            p.getEPSOutput().close();
        }

        // Loop over the rest of the pages and reuse plates created for inks on previous pages.
        for (int pgNum = 1; pgNum < doc.getNumPages(); pgNum++)
        {
            pg = doc.getPage(pgNum);
            pageinks = pg.listInks();
            plates = new ArrayList<SeparationPlate>();
            SeparationPlate newplate;

            for(Ink i : pageinks)
            {
                System.out.println("Found color " + i.getColorantName() + " on page " + (pgNum+1) + ".");
                if (colors.containsKey(i.getColorantName()))
                {
                    newplate = colors.get(i.getColorantName());
                    newplate.setEPSOutput(new java.io.FileOutputStream("Complex-Pg" + (pgNum+1) + "-" + i.getColorantName() + ".eps"));
                }
                else
                {
                    newplate = new SeparationPlate(i, new java.io.FileOutputStream("Complex-Pg" + (pgNum+1) + "-" + i.getColorantName() + ".eps"));
                    colors.put(i.getColorantName(), newplate);
                }
                plates.add(newplate);
            }

            parms.setPlates(plates);

            System.out.println("Making separations for page " + (pgNum+1) + "...");
            pg.makeSeparations(parms);

            for(SeparationPlate p : plates)
            {
                p.getEPSOutput().close();
            }
        }
    }
}
