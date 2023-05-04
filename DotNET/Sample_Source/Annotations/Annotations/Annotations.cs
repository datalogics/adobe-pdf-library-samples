using System;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates how to find and describe annotations in an existing PDF document.
 * 
 * For more detail see the description of the Annotations sample program on our Developer's site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/working-with-annotations#annotations
 *
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/.
 *
 */
namespace Annotations
{
    class Annotations
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Annotations Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample_annotations.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                Console.WriteLine("Input file: " + sInput);

                Document doc = new Document(sInput);

                Page pg = doc.GetPage(0);
                Annotation ann = pg.GetAnnotation(0);

                Console.WriteLine(ann.Title);
                Console.WriteLine(ann.GetType().Name);
            }
        }
    }
}
