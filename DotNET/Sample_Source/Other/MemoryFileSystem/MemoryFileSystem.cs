using System;
using Datalogics.PDFL;

/*
 * 
 * This sample demonstrates how to initialize and use RAM memory instead of the local
 * hard disk to save temporary files, in order to save processing time.
 * 
 * The program sets the DefaultTempStore property of the Adobe PDF Library object to
 * TempStoreType.Memory. The program can also set a maximum amount of RAM to use by
 * applying a value to the DefaultTempStoreMemLimit property.
 *
 * Copyright (c) 2007-2020, Datalogics, Inc. All rights reserved.
 *
 * For complete copyright information, refer to:
 * http://dev.datalogics.com/adobe-pdf-library/license-for-downloaded-pdf-samples/
 *
 */
namespace MemoryFileSystem
{
    class MemoryFileSystem
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MemoryFileSystem Sample:");


            using (Library lib = new Library())
            {
                Console.WriteLine("Initialized the library.");

                String sOutput = "TempFileSystem.pdf";

                if (args.Length > 0)
                    sOutput = args[0];

                Console.WriteLine("Writing to output " + sOutput);

                Rect bounds = new Rect(0, 0, 612, 792);

                // Set in-memory file system as temporary storage
                lib.DefaultTempStore = TempStoreType.Memory;

                // Set memory limit up to 100 kB. When occupied memory
                // exceeds the limit value, disk temporary storage will be used.
                lib.DefaultTempStoreMemLimit = 100;
                using (Document doc = new Document())
                {
                    doc.CreatePage(Document.BeforeFirstPage, bounds);
                    doc.Save(SaveFlags.Full, sOutput);
                }
            }
        }
    }
}
