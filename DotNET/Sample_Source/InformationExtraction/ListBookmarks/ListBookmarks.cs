using System;
using Datalogics.PDFL;

/*
 * 
 * This sample finds and describes the bookmarks included in a PDF document.
 * 
 * For more detail see the description of the List sample programs, and ListBookmarks, on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/listing-information-about-values-and-objects-in-pdf-files
 * 
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace ListBookmarks
{
    class ListBookmarks
    {
        static void EnumerateBookmarks(Bookmark b)
        {
            if (b != null)
            {
                Console.Write(b);
                Console.Write(": ");
                Console.Write(b.Title);
                ViewDestination v = b.ViewDestination;
                if (v != null)
                {
                    Console.Write(", page ");
                    Console.Write(v.PageNumber);
                    Console.Write(", fit ");
                    Console.Write(v.FitType);
                    Console.Write(", dest rect ");
                    Console.Write(v.DestRect);
                    Console.Write(", zoom ");
                    Console.Write(v.Zoom);
                }

                Console.WriteLine();
                EnumerateBookmarks(b.FirstChild);
                EnumerateBookmarks(b.Next);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("ListBookmarks Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sInput = Library.ResourceDirectory + "Sample_Input/sample.pdf";

                if (args.Length > 0)
                    sInput = args[0];

                Console.WriteLine("Input file: " + sInput);

                Document doc = new Document(sInput);

                Bookmark rootBookmark = doc.BookmarkRoot;
                EnumerateBookmarks(rootBookmark);
            }
        }
    }
}
