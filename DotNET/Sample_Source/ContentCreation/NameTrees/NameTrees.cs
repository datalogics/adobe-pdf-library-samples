using System;
using Datalogics.PDFL;


/*
 * 
 * This sample demonstrates working with the NameTree class. A name tree is a type
 * of dictionary that is often used as a data structure in PDF files. Unlike a standard
 * dictionary, instead of using a code value as a key to identify an object, a name tree
 * uses names as keys to map to data objects. 
 *
 * For more detail see the description of the NameTrees sample program on our Developer’s site, 
 * http://dev.datalogics.com/adobe-pdf-library/sample-program-descriptions/net-core-sample-programs/working-with-large-amounts-of-data-in-a-pdf
 * 
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */

namespace NameTrees
{
    class NameTrees
    {
        static void Main()
        {
            Console.WriteLine("NameTree Sample:");


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
                NameTree nametree = new NameTree(doc);

                // Use put() to put a key-value pair in it
                PDFString key = new PDFString("Bailout", doc, false, false);
                PDFString value = new PDFString("Smorgasbord", doc, false, false);
                nametree.Put(key, value);
                Console.WriteLine("\nCreated NameTree and added first key-value pair.");

                // Put another key-value pair in it
                nametree.Put(new PDFString("Brandish", doc, false, false),
                    new PDFString("Copasetic", doc, false, false));

                // Retrieve second entry
                PDFString keysearchstring = new PDFString("Brandish", doc, false, false);
                PDFObject lookup = nametree.Get(keysearchstring);
                Console.WriteLine("\nRetrieving two entries:");
                Console.WriteLine(lookup);

                // Retrieve first entry
                lookup = nametree.Get(new PDFString("Bailout", doc, false, false));
                Console.WriteLine(lookup);

                // Use remove() method to remove first entry
                nametree.Remove(new PDFString("Bailout", doc, false, false));

                // Get both entries, and demonstrate that first is now gone
                Console.WriteLine("\nAfter removing entry 1, we now have:");
                lookup = nametree.Get(new PDFString("Bailout", doc, false, false));
                Console.WriteLine(lookup);
                lookup = nametree.Get(new PDFString("Brandish", doc, false, false));
                Console.WriteLine(lookup);

                // Create two new NameTrees and set each to our original numbertree
                NameTree abc = nametree;
                NameTree xyz = nametree;

                // Show they are equal
                if (abc.Equals(xyz))
                    Console.WriteLine("\nThe two NameTrees abc and xyz are the same");
                else
                    Console.WriteLine("\nThe two NameTrees abc and xyz are not the same");

                // Create two new NameTrees
                abc = new NameTree(doc);
                xyz = new NameTree(doc);

                // Show they are not equal
                if (abc.Equals(xyz))
                    Console.WriteLine("\nThe two NameTrees abc and xyz are the same");
                else
                    Console.WriteLine("\nThe two NameTrees abc and xyz are not the same");

                // Get the PDFDict from the NameTree
                PDFDict dict = nametree.PDFDict;
                Console.WriteLine("\nThe PDFDict from the NameTree:");
                Console.WriteLine(dict);

                // Kill the NameTree object
                nametree.Dispose();
                Console.WriteLine("\nDisposed the NameTree object.");

                //////////////////////////////////////////////////
                // Now use Document methods to operate on NameTree
                //////////////////////////////////////////////////

                // Create a NameTree in the document using createNameTree() method
                NameTree docCreatedNameTree = doc.CreateNameTree("MyNameTree");
                docCreatedNameTree.Put(new PDFString("Argyle", doc, false, false),
                    new PDFString("Seamstress", doc, false, false));
                Console.WriteLine("Created a NameTree object in the document.");

                // Look for the NameTree in the document by using the getNameTree() method
                Console.WriteLine(
                    "\nTwo searches for NameTree using getNameTree() method; first fails, second succeeeds:");
                docCreatedNameTree = doc.GetNameTree("Garbage");
                Console.WriteLine(docCreatedNameTree);
                docCreatedNameTree = doc.GetNameTree("MyNameTree");
                Console.WriteLine(docCreatedNameTree);

                // Remove the NameTree from the document by using the remove NameTree() method
                Console.WriteLine("\nRemove the NameTree from the document.");
                doc.RemoveNameTree("Garbage");
                docCreatedNameTree = doc.GetNameTree("MyNameTree");
                Console.WriteLine(docCreatedNameTree);
                doc.RemoveNameTree("MyNameTree");
                docCreatedNameTree = doc.GetNameTree("MyNameTree");
                Console.WriteLine(docCreatedNameTree);

                // Dispose the doc object
                doc.Dispose();
            }
        }
    }
}
