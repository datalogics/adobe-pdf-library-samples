using System;
using Datalogics.PDFL;


/*
 * 
 * This sample demonstrates working with the NumberTree class. A number tree is a type of dictionary
 * that is often used as a data structure in PDF files. It is similar to a name tree, except that the
 * keys used in a number tree are integers, rather than character strings, and these keys are sorted
 * in ascending numerical order. 
 *
 * For more detail see the description of the NumberTrees sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/working-with-large-amounts-of-data-in-a-pdf
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace NumberTrees
{
    class NumberTrees
    {
        static void Main()
        {
            Console.WriteLine("NumberTree Sample:");


            // ReSharper disable once UnusedVariable
            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                // Create a new document and blank first page
                Document doc = new Document();
                Rect rect = new Rect(0, 0, 612, 792);
                doc.CreatePage(Document.BeforeFirstPage, rect);
                Console.WriteLine("Created new document and first page.");

                // Create a NumberTree and put a key-value pair in it
                NumberTree numbertree = new NumberTree(doc);

                // Use put() to put a key-value pair in it
                int key = 1;
                PDFString value = new PDFString("Smorgasbord", doc, false, false);
                numbertree.Put(key, value);
                Console.WriteLine("\nCreated NumberTree and added first key-value pair.");

                // Put another key-value pair in it
                numbertree.Put(2, new PDFString("Copasetic", doc, false, false));

                // Retrieve second entry
                int keysearchinteger = 2;
                PDFObject lookup = numbertree.Get(keysearchinteger);
                Console.WriteLine("\nRetrieving two entries:");
                Console.WriteLine(lookup);

                // Retrieve first entry
                lookup = numbertree.Get(1);
                Console.WriteLine(lookup);

                // Use remove() method to remove first entry
                numbertree.Remove(1);

                // Get 1 and 2, and demonstrate that 1 is now gone
                Console.WriteLine("\nAfter removing entry 1, we now have:");
                lookup = numbertree.Get(1);
                Console.WriteLine(lookup);
                lookup = numbertree.Get(2);
                Console.WriteLine(lookup);

                // Create two new NumberTrees and set each to our original numbertree
                NumberTree abc = numbertree;
                NumberTree xyz = numbertree;

                // Show they are equal
                if (abc.Equals(xyz))
                    Console.WriteLine("\nThe two NumberTrees abc and xyz are the same");
                else
                    Console.WriteLine("\nThe two NumberTrees abc and xyz are not the same");

                // Create two new NumberTrees
                abc = new NumberTree(doc);
                xyz = new NumberTree(doc);

                // Show they are not equal
                if (abc.Equals(xyz))
                    Console.WriteLine("\nThe two NumberTrees abc and xyz are the same");
                else
                    Console.WriteLine("\nThe two NumberTrees abc and xyz are not the same");

                // Get the PDFDict from the NumberTree
                PDFDict dict = numbertree.PDFDict;
                Console.WriteLine("\nThe PDFDict from the NumberTree:");
                Console.WriteLine(dict);

                // Dispose the NumberTree object
                numbertree.Dispose();
                Console.WriteLine("\nDisposed the NumberTree object.");

                // Dispose the doc object
                doc.Dispose();
                Console.WriteLine("Disposed document object.");
            }
        }
    }
}
